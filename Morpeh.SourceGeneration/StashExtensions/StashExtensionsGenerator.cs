using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.StashExtensions;

[Generator]
public sealed class StashExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());
        
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is StructDeclarationSyntax;
    }
    
    private static Optional<ComponentToGenerate> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var structDeclarationSyntax = (StructDeclarationSyntax) ctx.Node;

        var structDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax, token);
        if (structDeclarationSymbol is not ITypeSymbol structDeclarationTypeSymbol)
        {
            return OptionalExt.None<ComponentToGenerate>();
        }

        if (!structDeclarationTypeSymbol.HaveInterface("Scellecs.Morpeh.IComponent"))
        {
            return OptionalExt.None<ComponentToGenerate>(); 
        }
        
        var fields = new List<FieldToGenerate>();
        foreach (var memberDeclaration in structDeclarationSyntax.Members)
        {
            if (memberDeclaration is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }
            
            var typeSymbol = ctx.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
            if (typeSymbol is null)
            {
                continue;
            }
            
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                fields.Add(new FieldToGenerate(typeSymbol, variable.Identifier.ToString()));
            }
        }
        
        return new ComponentToGenerate(structDeclarationTypeSymbol, fields.ToArray());
    }

    private static void GenerateCode(SourceProductionContext context, ComponentToGenerate componentToGenerate)
    {
        var code = GenerateCode(componentToGenerate);
        context.AddSource($"{componentToGenerate.TypeSymbol.ToDisplayString()}_ext.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(ComponentToGenerate componentToGenerate)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent("using Scellecs.Morpeh;");
        builder.AppendLine();

        using (new CodeBuilder.NamespaceBlock(builder, componentToGenerate.TypeSymbol))
        {
            builder.AppendIdent().Append("public static partial class ")
                .Append(componentToGenerate.TypeSymbol.Name).Append("Ext").AppendLine();

            using (new CodeBuilder.BracketsBlock(builder))
            {
                if (componentToGenerate.Fields.Length == 0)
                {
                    AddSetOrRemove();
                }
                else
                {
                    AddSet();
                }
            }
        }
        
        return builder.ToString();

        void AddSet()
        {
            builder.AppendIdent().Append("public static void Set(this Stash<")
                .Append(componentToGenerate.TypeSymbol.ToDisplayString())
                .Append("> stash, Entity entity");

            foreach (var fieldToGenerate in componentToGenerate.Fields)
            {
                builder.Append(", in ").Append(fieldToGenerate.Type.ToDisplayString()).Append(" ")
                    .Append(fieldToGenerate.Name.FirstCharToLower());
            }

            builder.Append(")").AppendLine();

            using (new CodeBuilder.BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("ref var component = ref stash.Get(entity, out var exist);");
                    
                builder.AppendLineWithIdent("if (exist)");
                using (new CodeBuilder.BracketsBlock(builder))
                {
                    foreach (var fieldToGenerate in componentToGenerate.Fields)
                    {
                        builder.AppendIdent().Append("component.").Append(fieldToGenerate.Name).Append(" = ")
                            .Append(fieldToGenerate.Name.FirstCharToLower()).Append(";").AppendLine();
                    }
                }
                    
                builder.AppendLineWithIdent("else");
                using (new CodeBuilder.BracketsBlock(builder))
                {
                    builder.AppendIdent().Append("stash.Set(entity, new ")
                        .Append(componentToGenerate.TypeSymbol.ToDisplayString())
                        .Append(" { ");
                        
                    builder.AppendArray(
                        componentToGenerate.Fields,
                        (f, cb) => cb.Append(f.Name).Append(" = ").Append(f.Name.FirstCharToLower()),
                        cb => cb.Append(", ")
                    );
                        
                    builder.Append(" });").AppendLine();
                }
            }
        }
        
        void AddSetOrRemove()
        {
            builder.AppendIdent().Append("public static void SetOrRemove(this Stash<")
                .Append(componentToGenerate.TypeSymbol.ToDisplayString())
                .Append("> stash, Entity entity, bool setOrRemove)").AppendLine();

            using (new CodeBuilder.BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("if (setOrRemove)");
                using (new CodeBuilder.BracketsBlock(builder))
                {
                    builder.AppendLineWithIdent("stash.Set(entity);");
                }
                builder.AppendLineWithIdent("else");
                using (new CodeBuilder.BracketsBlock(builder))
                {
                    builder.AppendLineWithIdent("stash.Remove(entity);");
                }
            }
        }
    }
}
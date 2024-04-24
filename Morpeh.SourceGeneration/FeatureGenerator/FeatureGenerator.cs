using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Morpeh.SourceGeneration.Common;
using Morpeh.SourceGeneration.SystemGenerator;

namespace Morpeh.SourceGeneration.FeatureGenerator;

[Generator]
public sealed class FeatureGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());

        context.RegisterPostInitializationOutput(i => i.AddSource("FeatureInterface.g", FeatureInterface.InterfaceText));
        
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }
    
    private static Optional<FeatureToGenerate> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;

        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classDeclarationTypeSymbol)
        {
            return OptionalExt.None<FeatureToGenerate>();
        }
        
        if (!classDeclarationSyntax.HaveInterface(FeatureInterface.FeatureInterfaceName))
        {
            return OptionalExt.None<FeatureToGenerate>();
        }
        
        var systems = new List<SystemToGenerate>();
        foreach (var memberDeclaration in classDeclarationSyntax.Members)
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
            
            var systemType = SystemTypeExt.ResolveSystemType(typeSymbol);
            if (!systemType.HasValue)
            {
                continue;
            }
            
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                systems.Add(new SystemToGenerate(typeSymbol, systemType.Value, variable.Identifier.ToString()));
            }
        }
        
        return new FeatureToGenerate(classDeclarationTypeSymbol, systems.ToArray());
    }

    private static void GenerateCode(SourceProductionContext context, FeatureToGenerate featureToGenerate)
    {
        var code = GenerateCode(featureToGenerate);
        context.AddSource($"{featureToGenerate.TypeSymbol.Name}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(FeatureToGenerate featureToGenerate)
    {
        var builder = new CodeBuilder();

        using (new CodeBuilder.NamespaceBlock(builder, featureToGenerate.TypeSymbol))
        {
            builder.AppendIdent().Append("public partial class ").Append(featureToGenerate.TypeSymbol.Name).AppendLine();
            
            using (new CodeBuilder.BracketsBlock(builder))
            {
                AppendConstructor();
                builder.AppendLine();
                AppendInject();
                builder.AppendLine();
                AppendStartAsync();
                builder.AppendLine();
                AppendUniTask();
                builder.AppendLine();
                AppendTick();
                builder.AppendLine();
                AppendDispose();
            }
        }
        
        return builder.ToString();
        
        void AppendConstructor()
        {
            builder.AppendIdent().Append("public ").Append(featureToGenerate.TypeSymbol.Name).Append("()").AppendLine();
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(" = new ")
                        .Append(systemToGenerate.Type.ToDisplayString()).Append("();").AppendLine();
                }
            }
        }
        
        void AppendInject()
        {
            builder.AppendLineWithIdent("public void Inject(VContainer.IObjectResolver objectResolver)");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    builder.AppendIdent().Append("objectResolver.Inject(").Append(systemToGenerate.Name)
                        .Append(");").AppendLine();
                }
            }
        }
        
        void AppendStartAsync()
        {
            builder.AppendLineWithIdent("public void Initialize(Scellecs.Morpeh.World world)");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(".Initialize(world);").AppendLine();
                }
            }
        }
        
        void AppendUniTask()
        {
            builder.AppendLineWithIdent("public async Cysharp.Threading.Tasks.UniTask StartAsync(System.Threading.CancellationToken cancellation)");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    if (systemToGenerate.SystemType.HasFlag(SystemType.AsyncInitialize))
                    {
                        builder.AppendIdent().Append("await ").Append(systemToGenerate.Name)
                            .Append(".StartAsync(cancellation);").AppendLine();
                    }
                    else
                    {
                        builder.AppendIdent().Append(systemToGenerate.Name).Append(".Start();").AppendLine();
                    }
                }
            }
        }
        
        void AppendTick()
        {
            builder.AppendLineWithIdent("public void Tick()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    if (systemToGenerate.SystemType.HasFlag(SystemType.Update))
                    {
                        builder.AppendIdent().Append(systemToGenerate.Name).Append(".Tick();").AppendLine();
                    }
                }
            }
        }
        
        void AppendDispose()
        {
            builder.AppendLineWithIdent("public void Dispose()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(".Dispose();").AppendLine();
                }
            }
        }
    }
}
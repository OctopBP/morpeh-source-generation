using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator;

[Generator]
public sealed class SystemGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());

        context.RegisterPostInitializationOutput(i => i.AddSource("Interfaces.g", SystemInterfaces.InterfacesText));
        
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }
    
    private static Optional<SystemToGenerate> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;

        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classDeclarationTypeSymbol)
        {
            return OptionalExt.None<SystemToGenerate>();
        }
        
        var systemType = SystemTypeExt.ResolveSystemType(classDeclarationTypeSymbol);
        if (systemType == SystemType.None)
        {
            return OptionalExt.None<SystemToGenerate>();
        }

        var (stashes, filters) = StashAndFiltersToGenerates(ctx, classDeclarationSyntax);
        return new SystemToGenerate(classDeclarationTypeSymbol, systemType, stashes.ToArray(), filters.ToArray());
    }

    private static (List<StashToGenerate> stashes, List<FilterToGenerate> filters) StashAndFiltersToGenerates
        (GeneratorSyntaxContext ctx, TypeDeclarationSyntax classDeclarationSyntax)
    {
        var stashes = new List<StashToGenerate>();
        var filters = new List<FilterToGenerate>();
        foreach (var memberDeclaration in classDeclarationSyntax.Members)
        {
            if (memberDeclaration is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }

            var fieldType = fieldDeclaration.Declaration.Type;
            if (fieldType is GenericNameSyntax genericNameSyntax)
            {
                switch (genericNameSyntax.GetNameText())
                {
                    case "Stash": AddStashes(ctx, genericNameSyntax, fieldDeclaration, stashes); break;
                    case "Filter": AddFilter(ctx, genericNameSyntax, fieldDeclaration, filters); break;
                }
            }
        }

        return (stashes, filters);
    }

    private static void AddFilter(GeneratorSyntaxContext ctx, GenericNameSyntax genericNameSyntax,
        FieldDeclarationSyntax fieldDeclaration, List<FilterToGenerate> filters)
    {
        var arguments = genericNameSyntax.TypeArgumentList.Arguments;
        if (arguments.Count is < 1 or > 2)
        {
            return;
        }
                    
        var withTypes = ExtractTypesFromTuple(ctx, arguments[0]).ToArray();
        var withoutTypes = arguments.Count == 2
            ? ExtractTypesFromTuple(ctx, arguments[1]).ToArray()
            : [];
                    
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            filters.Add(
                new FilterToGenerate(variable.Identifier.ToString(), withTypes, withoutTypes));
        }
    }

    private static void AddStashes(GeneratorSyntaxContext ctx, GenericNameSyntax genericNameSyntax,
        FieldDeclarationSyntax fieldDeclaration, List<StashToGenerate> stashes)
    {
        var arguments = genericNameSyntax.TypeArgumentList.Arguments;
        if (arguments.Count != 1)
        {
            return;
        }
                    
        var firstArgument = arguments[0];
        var argumentSymbol = ctx.SemanticModel.GetSymbolInfo(firstArgument).Symbol;
        if (argumentSymbol is null)
        {
            return;
        }
                    
        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            stashes.Add(new StashToGenerate(argumentSymbol, variable.Identifier.ToString()));
        }
    }

    private static IEnumerable<ISymbol> ExtractTypesFromTuple(GeneratorSyntaxContext ctx, SyntaxNode syntax)
    {
        var list = new List<ISymbol>();
        if (syntax is TupleTypeSyntax tupleTypeSyntax)
        {
            foreach (var tupleElementSyntax in tupleTypeSyntax.Elements)
            {
                var elementSymbol = ctx.SemanticModel.GetSymbolInfo(tupleElementSyntax.Type).Symbol;
                if (elementSymbol is not null)
                {
                    list.Add(elementSymbol);
                }
            }
        }
        else
        {
            var elementSymbol = ctx.SemanticModel.GetSymbolInfo(syntax).Symbol;
            if (elementSymbol is not null)
            {
                list.Add(elementSymbol);
            }
        }

        return list;
    }

    private static IEnumerable<ISymbol> ExtractTypeofType(GeneratorSyntaxContext ctx, AttributeSyntax filterAttribute)
    {
        if (filterAttribute.ArgumentList is null)
        {
            return [];
        }

        var arguments = filterAttribute.ArgumentList.Arguments;
        if (arguments.Count == 0)
        {
            return [];
        }

        var list = new List<ISymbol>();
        foreach (var attributeArgumentSyntax in arguments)
        {
            if (attributeArgumentSyntax.Expression is not TypeOfExpressionSyntax typeOfExpressionSyntax)
            {
                continue;
            }
                            
            var typeSymbol = ctx.SemanticModel.GetSymbolInfo(typeOfExpressionSyntax.Type).Symbol;
            if (typeSymbol is null)
            {
                continue;
            }
            
            list.Add(typeSymbol);
        }
        
        return list;
    }

    private static void GenerateCode(SourceProductionContext context, SystemToGenerate systemToGenerate)
    {
        var code = GenerateCode(systemToGenerate);
        context.AddSource($"{systemToGenerate.TypeSymbol.ToDisplayString()}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(SystemToGenerate systemToGenerate)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent("using Scellecs.Morpeh;");
        builder.AppendLine();

        using (new CodeBuilder.NamespaceBlock(builder, systemToGenerate.TypeSymbol))
        {
            builder.AppendIdent().Append("public partial class ").Append(systemToGenerate.TypeSymbol.Name)
                .Append(" : System.IDisposable").AppendLine();

            using (new CodeBuilder.BracketsBlock(builder))
            {
                AppendWorld();
                builder.AppendLine();
                AppendInitialize();
                builder.AppendLine();
                
                if (systemToGenerate.SystemType.HasFlag(SystemType.Initialize))
                {
                    AppendStart();
                    builder.AppendLine();
                }
            
                if (systemToGenerate.SystemType.HasFlag(SystemType.Update))
                {
                    AppendUpdate();
                    builder.AppendLine();
                }
                
                AppendDispose();
            }
        }
        
        return builder.ToString();

        void AppendWorld()
        {
            builder.AppendLineWithIdent("private World _world;");
        }
        
        void AppendInitialize()
        {
            builder.AppendLineWithIdent("public void Initialize(World world)");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("_world = world;");
                builder.AppendLine();
                
                builder.AppendLineWithIdent("// Stashes");

                foreach (var stash in systemToGenerate.Stashes)
                {
                    builder.AppendIdent().Append(stash.Name).Append(" = _world.GetStash<")
                        .Append(stash.Type.ToDisplayString()).Append(">();").AppendLine();
                }

                builder.AppendLine();
                builder.AppendLineWithIdent("// Filters");

                foreach (var filter in systemToGenerate.Filters)
                {
                    builder.AppendIdent().Append(filter.Name).Append(" = new Filter<");
                    if (filter.With.Length > 1) builder.Append("(");
                    builder.AppendArray(filter.With, (symbol, cb) => cb.Append(symbol.ToDisplayString()), cb => cb.Append(", "));
                    if (filter.With.Length > 1) builder.Append(")");
                    
                    if (filter.Without.Length > 0) builder.Append(", ");
                    if (filter.Without.Length > 1) builder.Append("(");
                    builder.AppendArray(filter.Without, (symbol, cb) => cb.Append(symbol.ToDisplayString()), cb => cb.Append(", "));
                    if (filter.Without.Length > 1) builder.Append(")");
                    
                    builder.Append(">(");
                        
                    builder.Append("_world.Filter");
                    foreach (var with in filter.With)
                    {
                        builder.Append(".With<").Append(with.ToDisplayString()).Append(">()");
                    }

                    foreach (var without in filter.Without)
                    {
                        builder.Append(".Without<").Append(without.ToDisplayString()).Append(">()");
                    }
                    
                    builder.Append(".Build()");
                        
                    builder.Append(");").AppendLine();
                }
            }
        }

        void AppendStart()
        {
            builder.AppendLineWithIdent("public void CallStart()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("Start();");
                builder.AppendLineWithIdent("_world.Commit();");
            }
        }
        
        void AppendUpdate()
        {
            builder.AppendLineWithIdent("public void CallUpdate()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("Update();");
                builder.AppendLineWithIdent("_world.Commit();");
            }
        }
        
        void AppendDispose()
        {
            builder.AppendLineWithIdent("public void Dispose()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var filter in systemToGenerate.Filters)
                {
                    builder.AppendIdent().Append(filter.Name).Append(" = null;").AppendLine();
                }
            }
        }
    }
}
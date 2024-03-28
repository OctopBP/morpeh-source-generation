using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Morpeh.SourceGeneration.Common;
using Morpeh.SourceGeneration.SystemGenerator.Filter;

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
            .Select((array, _) => array.OfType<SystemToGenerate>().ToImmutableArray())
            .SelectMany(static (array, _) => array);

        context.RegisterPostInitializationOutput(i =>
        {
            i.AddSource($"{Filter.WithAttribute.AttributeFullName}.g", Filter.WithAttribute.AttributeText);
            i.AddSource($"{Filter.WithoutAttribute.AttributeFullName}.g", Filter.WithoutAttribute.AttributeText);
            i.AddSource("Interfaces.g", SystemInterfaces.InterfacesText);
        });
        
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }
    
    private static SystemToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;

        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classDeclarationTypeSymbol)
        {
            return null;
        }
        
        // TODO: maybe better to use classDeclarationTypeSymbol.AllInterfaces
        var systemType = ResolveSystemType(classDeclarationSyntax);

        if (systemType == SystemType.None)
        {
            return null;
        }

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
                if (genericNameSyntax.GetNameText() == "Stash")
                {
                    var arguments = genericNameSyntax.TypeArgumentList.Arguments;
                    if (arguments.Count != 1)
                    {
                        continue;
                    }
                    
                    var firstArgument = arguments[0];
                    var argumentSymbol = ctx.SemanticModel.GetSymbolInfo(firstArgument).Symbol;
                    if (argumentSymbol is null)
                    {
                        continue;
                    }
                    
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        stashes.Add(new StashToGenerate(argumentSymbol, variable.Identifier.ToString()));
                    }
                
                }
            }
            else if (fieldDeclaration.Declaration.Type.ToString() == "Filter")
            {
                var withTypes = new List<ISymbol>();
                var withoutTypes = new List<ISymbol>();
                foreach (var filterAttributes in fieldDeclaration.AttributeLists)
                {
                    foreach (var filterAttribute in filterAttributes.Attributes)
                    {
                        if (filterAttribute.Name.AttributeIsEqualByName(WithAttribute.AttributeName))
                        {
                            var symbols = ExtractTypeofType(ctx, filterAttribute);
                            withTypes.AddRange(symbols);
                        }
                        else if (filterAttribute.Name.AttributeIsEqualByName(WithoutAttribute.AttributeName))
                        {
                            var symbols = ExtractTypeofType(ctx, filterAttribute);
                            withoutTypes.AddRange(symbols);
                        }
                    }
                }

                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    filters.Add(new FilterToGenerate(
                        variable.Identifier.ToString(), withTypes.ToArray(), withoutTypes.ToArray()));
                }
            }
        }
        
        return new SystemToGenerate(classDeclarationTypeSymbol, systemType, stashes.ToArray(), filters.ToArray());
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

    private static SystemType ResolveSystemType(ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (classDeclarationSyntax.HaveInterface(SystemInterfaces.InitializeInterfaceName))
        {
            return SystemType.Initialize;
        }
        
        if (classDeclarationSyntax.HaveInterface(SystemInterfaces.UpdateInterfaceName))
        {
            return SystemType.Update;
        }

        return SystemType.None;
    }

    private static void GenerateCode(SourceProductionContext context, SystemToGenerate systemToGenerate)
    {
        var code = GenerateCode(systemToGenerate);
        context.AddSource($"{systemToGenerate.TypeSymbol.Name}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(SystemToGenerate systemToGenerate)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent("using Scellecs.Morpeh;");
        builder.AppendLine();

        using (new CodeBuilder.NamespaceBlock(builder, systemToGenerate.TypeSymbol))
        {
            builder.AppendIdent().Append("public partial class ").Append(systemToGenerate.TypeSymbol.Name);

            var interfaces = systemToGenerate.SystemType switch {
                SystemType.None => "",
                SystemType.Initialize => " : VContainer.Unity.IAsyncStartable",
                SystemType.Update => " : VContainer.Unity.IAsyncStartable, VContainer.Unity.ITickable",
                _ => throw new ArgumentOutOfRangeException(),
            };
            builder
                .Append(interfaces)
                .AppendLine();

            using (new CodeBuilder.BracketsBlock(builder))
            {
                AppendInitialize();
                builder.AppendLine();
                AppendDispose();
            }
        }
        
        return builder.ToString();

        void AppendInitialize()
        {
            builder.AppendLineWithIdent("public void Initialize()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
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
                    builder.AppendIdent().Append(filter.Name).Append(" = _world.Filter");
                    foreach (var with in filter.With)
                    {
                        builder.Append(".With<").Append(with.ToDisplayString()).Append(">()");
                    }

                    foreach (var without in filter.Without)
                    {
                        builder.Append(".Without<").Append(without.ToDisplayString()).Append(">()");
                    }

                    builder.Append(".Build();").AppendLine();
                }
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
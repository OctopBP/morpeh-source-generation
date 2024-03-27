using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        var systemType = ResolveSystemType(classDeclarationSyntax);

        if (systemType == SystemType.None)
        {
            return null;
        }

        // var enumNamespace = enumDeclarationTypeSymbol.GetNamespace();

        var stashes = new List<StashToGenerate>();
        var filters = new List<FilterToGenerate>();
        foreach (var memberTypeSymbol in classDeclarationTypeSymbol.GetTypeMembers())
        {
            if (memberTypeSymbol.IsGenericType)
            {
                stashes.Add(new StashToGenerate(memberTypeSymbol, "_name"));
            } else if (memberTypeSymbol.Name == "Filter")
            {
                filters.Add(new FilterToGenerate(memberTypeSymbol.Name, [], []));
            }
        }

        return new SystemToGenerate(classDeclarationTypeSymbol, systemType, stashes.ToArray(), filters.ToArray());
    }

    private static SystemType ResolveSystemType(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var systemType = SystemType.None;
        
        if (classDeclarationSyntax.HaveInterface(SystemInterfaces.InitializeInterfaceName))
        {
            systemType |= SystemType.Initialize;
        }
        
        if (classDeclarationSyntax.HaveInterface(SystemInterfaces.UpdateInterfaceName))
        {
            systemType |= SystemType.Update;
        }

        return systemType;
    }

    private static void GenerateCode(SourceProductionContext context, SystemToGenerate systemToGenerate)
    {
        var code = GenerateCode(systemToGenerate);
        context.AddSource($"{systemToGenerate.TypeSymbol.Name}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(SystemToGenerate systemToGenerate)
    {
        var builder = new CodeBuilder();

        builder.AppendIdent().Append("public partial class ").Append(systemToGenerate.TypeSymbol.Name);
        switch (systemToGenerate.SystemType)
        {
            case SystemType.None: break;
            case SystemType.Initialize: builder.Append(" : IAsyncStartable"); break;
            case SystemType.Update: builder.Append(" : IAsyncStartable, ITickable"); break;
            default: throw new ArgumentOutOfRangeException();
        }
        builder.AppendLine();

        builder.OpenBrackets();

        AppendInitialize();
        AppendDispose();
        
        builder.CloseBrackets();

        return builder.ToString();

        void AppendInitialize()
        {
            builder.AppendLineWithIdent("public void Initialize()");
            builder.OpenBrackets();
            builder.AppendLineWithIdent("// Stashes");
            
            foreach (var stash in systemToGenerate.Stashes)
            {
                builder.AppendIdent().Append(stash.Name).Append(" = _world.GetStash<")
                    .Append(stash.Type.ToDisplayString()).Append(">();").AppendLine();
            }
            
            builder.AppendLineWithIdent("// Filters");
            
            foreach (var filter in systemToGenerate.Filters)
            {
                builder.AppendIdent().Append(filter.Name).Append(" = _world.Filter");
                foreach (var with in filter.With)
                {
                    builder.Append(".With<").Append(with.Name).Append(">()");
                }
                foreach (var without in filter.Without)
                {
                    builder.Append(".Without<").Append(without.Name).Append(">()");
                }
                builder.Append(".Build();").AppendLine();
            }
            
            builder.CloseBrackets();
        }

        void AppendDispose()
        {
            builder.AppendLineWithIdent("public void Dispose()");
            builder.OpenBrackets();
            
            foreach (var filter in systemToGenerate.Filters)
            {
                builder.AppendIdent().Append(filter.Name).Append(" = null").AppendLine();
            }
            
            builder.CloseBrackets();
        }
    }
}
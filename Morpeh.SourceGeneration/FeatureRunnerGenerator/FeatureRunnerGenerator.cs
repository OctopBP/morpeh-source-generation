using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.FeatureRunnerGenerator;

[Generator]
public sealed class FeatureRunnerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());

        context.RegisterPostInitializationOutput(i => i.AddSource("FeatureInterface.g", FeatureRunnerInterface.InterfaceText));
        
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }
    
    private static Optional<FeatureRunnerToGenerate> GetSemanticTargetForGeneration(
        GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;

        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classDeclarationTypeSymbol)
        {
            return OptionalExt.None<FeatureRunnerToGenerate>();
        }
        
        if (!classDeclarationSyntax.HaveInterface(FeatureRunnerInterface.FeatureRunnerInterfaceName))
        {
            return OptionalExt.None<FeatureRunnerToGenerate>();
        }
        
        var systems = new List<FeaturesToGenerate>();
        foreach (var memberDeclaration in classDeclarationSyntax.Members)
        {
            if (memberDeclaration is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }
            
            var symbol = ctx.SemanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type).Symbol;
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                systems.Add(new FeaturesToGenerate(symbol, variable.Identifier.ToString()));
            }
        }
        
        return new FeatureRunnerToGenerate(classDeclarationTypeSymbol, systems.ToArray());
    }

    private static void GenerateCode(SourceProductionContext context, FeatureRunnerToGenerate featureRunnerToGenerate)
    {
        var code = GenerateCode(featureRunnerToGenerate);
        context.AddSource($"{featureRunnerToGenerate.TypeSymbol.Name}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(FeatureRunnerToGenerate featureRunnerToGenerate)
    {
        var builder = new CodeBuilder();

        using (new CodeBuilder.NamespaceBlock(builder, featureRunnerToGenerate.TypeSymbol))
        {
            builder.AppendIdent().Append("public partial class ").Append(featureRunnerToGenerate.TypeSymbol.Name)
                .AppendLine();
            
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
                AppendUpdate();
                builder.AppendLine();
                AppendDispose();
            }
        }
        
        return builder.ToString();
        
        void AppendConstructor()
        {
            builder.AppendIdent().Append("public ").Append(featureRunnerToGenerate.TypeSymbol.Name).Append("()")
                .AppendLine();
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
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
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(".Inject(objectResolver);").AppendLine();
                }
            }
        }
        
        void AppendStartAsync()
        {
            builder.AppendLineWithIdent("public void Initialize(Scellecs.Morpeh.World world)");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
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
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append("await ").Append(systemToGenerate.Name)
                        .Append(".StartAsync(cancellation);").AppendLine();
                }
            }
        }
        
        void AppendUpdate()
        {
            builder.AppendLineWithIdent("public void Update()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(".Update();").AppendLine();
                }
            }
        }
        
        void AppendDispose()
        {
            builder.AppendLineWithIdent("public void Dispose()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name).Append(".Dispose();").AppendLine();
                }
            }
        }
    }
}
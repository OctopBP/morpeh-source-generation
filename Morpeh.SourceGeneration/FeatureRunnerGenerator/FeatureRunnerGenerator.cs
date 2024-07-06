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

        context.RegisterPostInitializationOutput(i =>
        {
            i.AddSource("FeatureInterface.g", FeatureRunnerInterface.InterfaceText);
            i.AddSource("WithWorldAttribute.g", WithWorldAttribute.AttributeText);
        });
        
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
        
        var withWorld = classDeclarationSyntax.HaveAttribute(WithWorldAttribute.AttributeName);
        
        return new FeatureRunnerToGenerate(classDeclarationTypeSymbol, systems.ToArray(), withWorld);
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
                .Append(" : System.IDisposable").AppendLine();
            
            using (new CodeBuilder.BracketsBlock(builder))
            {
                if (featureRunnerToGenerate.WithWorld)
                {
                    AppendWorld();
                    builder.AppendLine();
                }
                
                AppendConstructor();
                builder.AppendLine();
                AppendInject();
                builder.AppendLine();
                AppendInitialize();
                builder.AppendLine();
                AppendStart();
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
        
        void AppendWorld()
        {
            builder.AppendLineWithIdent("private readonly Scellecs.Morpeh.World _world = Scellecs.Morpeh.World.Create();");
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
        
        void AppendInitialize()
        {
            builder.AppendLineWithIdent(featureRunnerToGenerate.WithWorld
                ? "public void Initialize()"
                : "public void Initialize(Scellecs.Morpeh.World world)");
            
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name)
                        .Append(featureRunnerToGenerate.WithWorld ? ".Initialize(_world);" : ".Initialize(world);")
                        .AppendLine();
                }
            }
        }
        
        void AppendStart()
        {
            builder.AppendLineWithIdent("public void Start()");
            using (new CodeBuilder.BracketsBlock(builder))
            {
                foreach (var systemToGenerate in featureRunnerToGenerate.Systems)
                {
                    builder.AppendIdent().Append(systemToGenerate.Name)
                        .Append(".Start();").AppendLine();
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
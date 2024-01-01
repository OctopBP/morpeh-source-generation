using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MorpehAttributes.SystemList;

[Generator]
public class SystemListGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SystemListSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SystemListSyntaxReceiver receiver)
        {
            return;
        } 

        var classSource = "";
        
        var fields = BuildFields(receiver.SystemInfos);
        classSource += fields;
        
        context.AddSource($"test.g.cs", SourceText.From(classSource, Encoding.UTF8));
    }

    private static string BuildFields(SystemInfo[] systems)
    {
        var fields = systems
            .Select(system => system.Type.Name)
            .Select(name => $"private readonly {name} _{name};");
        
        return string.Join('\n', fields);
    }
    
    private static string ProcessFeature(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return "";
    }
    
    private static string AddUpdate(IFieldSymbol[] fields)
    {
        return "";
    }
}
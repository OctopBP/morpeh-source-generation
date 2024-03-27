using System;

namespace Morpeh.SourceGeneration.Common; 

public static class Utils
{
    public static string GeneratedCodeAttribute(this Type type)
    {
        var assemblyName = type.Assembly.GetName();
        return $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName.Name}"", ""{assemblyName.Version}"")]";
    }
}
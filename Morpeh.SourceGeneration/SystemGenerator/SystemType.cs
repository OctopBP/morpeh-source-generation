using System;
using Microsoft.CodeAnalysis;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator;

[Flags]
public enum SystemType
{
    None = 0,
    Initialize = 1,
    AsyncInitialize = 2,
    Update = 4,
}

public static class SystemTypeExt
{
    public static SystemType ResolveSystemType(ITypeSymbol typeSymbol)
    {
        var type = SystemType.None;
        if (typeSymbol.HaveInterface(SystemInterfaces.UpdateInterfaceName))
        {
            type |= SystemType.Update;
        }
     
        if (typeSymbol.HaveInterface(SystemInterfaces.InitializeInterfaceName))
        {
            type |= SystemType.Initialize;
        }
        
        if (typeSymbol.HaveInterface(SystemInterfaces.AsyncInitializeInterfaceName))
        {
            type |= SystemType.AsyncInitialize;
        }

        return type;
    }
}
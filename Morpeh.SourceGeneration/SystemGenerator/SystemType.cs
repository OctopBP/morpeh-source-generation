using System;
using Microsoft.CodeAnalysis;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator;

[Flags]
public enum SystemType
{
    Initialize = 0,
    AsyncInitialize = 1,
    Update = 2,
    UpdateWithAsyncInitialize = AsyncInitialize | Update,
}

public static class SystemTypeExt
{
    public static Optional<SystemType> ResolveSystemType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.HaveInterface(SystemInterfaces.UpdateInterfaceName))
        {
            return typeSymbol.HaveInterface(SystemInterfaces.AsyncInitializeInterfaceName)
                ? SystemType.UpdateWithAsyncInitialize
                : SystemType.Update;
        }
     
        if (typeSymbol.HaveInterface(SystemInterfaces.InitializeInterfaceName))
        {
            return SystemType.Initialize;
        }
        
        if (typeSymbol.HaveInterface(SystemInterfaces.AsyncInitializeInterfaceName))
        {
            return SystemType.AsyncInitialize;
        }

        return OptionalExt.None<SystemType>();
    }
}
using Microsoft.CodeAnalysis;
using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator;

public enum SystemType
{
    None,
    Initialize,
    Update,
}

public static class SystemTypeExt
{
    public static SystemType ResolveSystemType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.HaveInterface(SystemInterfaces.InitializeInterfaceName))
        {
            return SystemType.Initialize;
        }
        
        if (typeSymbol.HaveInterface(SystemInterfaces.UpdateInterfaceName))
        {
            return SystemType.Update;
        }

        return SystemType.None;
    }
}
using System.Runtime.CompilerServices;

namespace Morpeh.SourceGeneration.Test;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}
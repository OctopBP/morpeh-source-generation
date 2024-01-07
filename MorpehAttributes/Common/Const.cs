namespace MorpehAttributes.Common;

public static class Const
{
    public const string AutoGeneratedText =
"""
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
""";

    public const string UtilUsings =
"""
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
""";

    public const string UtilAttributes = "[MethodImpl(MethodImplOptions.AggressiveInlining)] [Il2CppSetOption(Option.NullChecks, false)] [Il2CppSetOption(Option.ArrayBoundsChecks, false)] [Il2CppSetOption(Option.DivideByZeroChecks, false)]";
}
﻿//HintName: WithAttribute.g.cs
/// <auto-generated />

[global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
internal sealed class WithAttribute : global::System.Attribute
{
    private System.Type[] _types;

    public WithAttribute(params System.Type[] types)
    {
        _types = types;
    }
}
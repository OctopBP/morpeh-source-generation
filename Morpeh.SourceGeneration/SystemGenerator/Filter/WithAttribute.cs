using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator.Filter;

public static class WithAttribute
{
    public const string AttributeName = "With";
    public static readonly string AttributeFullName = AttributeName.AddAttributePostfix();

    public static readonly string AttributeText =
        $$"""
        /// <auto-generated />

        [global::System.AttributeUsage(global::System.AttributeTargets.Enum, Inherited = true, AllowMultiple = true)]
        internal sealed class {{AttributeFullName}} : global::System.Attribute
        {
            private System.Type[] _types;
        
            public {{AttributeFullName}}(params System.Type[] types)
            {
                _types = types;
            }
        }
        """;
}
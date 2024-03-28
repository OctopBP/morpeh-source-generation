using Morpeh.SourceGeneration.Common;

namespace Morpeh.SourceGeneration.SystemGenerator.Filter;

public static class WithoutAttribute
{
    public const string AttributeName = "Without";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();

    public static readonly string AttributeText =
        $$"""
        /// <auto-generated />

        [global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
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
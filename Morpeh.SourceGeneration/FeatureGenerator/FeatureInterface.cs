namespace Morpeh.SourceGeneration.FeatureGenerator;

public static class FeatureInterface
{
    public const string FeatureInterfaceName = "IFeature";
    public const string InterfaceText = $$"""
       /// <auto-generated />

       public interface {{FeatureInterfaceName}} { }
       """;
}
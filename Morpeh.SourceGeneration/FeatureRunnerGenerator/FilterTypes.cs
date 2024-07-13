namespace Morpeh.SourceGeneration.FeatureRunnerGenerator;

public static class FilterTypes
{
    public const string FiltersText =
        """
        using Unity.IL2CPP.CompilerServices;
        using Scellecs.Morpeh;
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public class Filter<TWith>
        {
            private readonly Filter _filter;
            public Filter Get() => _filter;
            
            public Filter(Filter filter)
            {
                _filter = filter;
            }
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public class Filter<TWith, TWithout>
        {
            private readonly Filter _filter;
            public Filter Get() => _filter;
            
            public Filter(Filter filter)
            {
                _filter = filter;
            }
        }
        """;
}
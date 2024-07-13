//HintName: Features.Components.TestComponent3_ext.g.cs
using Scellecs.Morpeh;

namespace Features.Components
{
    public static partial class TestComponent3Ext
    {
        public static void Set(this Stash<Features.Components.TestComponent3> stash, Entity entity, in int value, in Other.TestClass testClass)
        {
            ref var component = ref stash.Get(entity, out var exist);
            if (exist)
            {
                component.Value = value;
                component.TestClass = testClass;
            }
            else
            {
                stash.Set(entity, new Features.Components.TestComponent3 { Value = value, TestClass = testClass });
            }
        }
    }
}

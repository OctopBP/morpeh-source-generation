//HintName: Features.Components.TestComponent2_ext.g.cs
using Scellecs.Morpeh;

namespace Features.Components
{
    public static partial class TestComponent2Ext
    {
        public static void Set(this Stash<Features.Components.TestComponent2> stash, Entity entity, in int value)
        {
            ref var component = ref stash.Get(entity, out var exist);
            if (exist)
            {
                component.Value = value;
            }
            else
            {
                stash.Set(entity, new Features.Components.TestComponent2 { Value = value });
            }
        }
    }
}

//HintName: Features.Components.TestComponent1_ext.g.cs
using Scellecs.Morpeh;

namespace Features.Components
{
    public static partial class TestComponent1Ext
    {
        public static void SetOrRemove(this Stash<Features.Components.TestComponent1> stash, Entity entity, bool setOrRemove)
        {
            if (setOrRemove)
            {
                stash.Set(entity);
            }
            else
            {
                stash.Remove(entity);
            }
        }
    }
}

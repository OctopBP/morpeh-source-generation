namespace MorpehAttributes.SystemList;

public enum UpdatePhase
{
    Update,
    Fixed,
    Late
}

public enum SystemType
{
    Default,
    Register
}

public class SystemInfo(
    Type type,
    UpdatePhase updatePhase = UpdatePhase.Update,
    SystemType systemType = SystemType.Default)
{
    public readonly Type Type = type;
    public readonly UpdatePhase UpdatePhase = updatePhase;
    public readonly SystemType SystemType = systemType;
}

[AttributeUsage(AttributeTargets.Class)]
public class SystemListAttribute : Attribute
{
    public readonly SystemInfo[] Systems;
    
    public SystemListAttribute(params SystemInfo[] systems)
    {
        Systems = systems;
    }
}

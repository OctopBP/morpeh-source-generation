using System;

namespace MorpehAttributes.Shared.SystemList;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SystemListAttribute : Attribute
{
    public readonly Type SystemType;
    public readonly UpdatePhase UpdatePhase;
    public readonly SystemRegisterType SystemRegisterType;

    public SystemListAttribute(Type systemType, UpdatePhase updatePhase, SystemRegisterType systemRegisterType)
    {
        SystemType = systemType;
        UpdatePhase = updatePhase;
        SystemRegisterType = systemRegisterType;
    }
}
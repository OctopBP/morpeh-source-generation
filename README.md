# How to use

## Feature Runner

```csharp
public partial class FeatureRunner : IFeatureRunner
{
    private readonly SomeFeature _someFeature;
}
```

## Features

```csharp
public partial class SomeFeature : IFeature
{
    private readonly SomeSystem1 _someSystem1;
    private readonly SomeSystem2 _someSystem2;
}
```

### Register attribute
`Register` attribute can be added to a system field in a feature. This system will be registered in the DI container and can be injected into other systems.

```csharp
public partial class SomeFeature : IFeature
{
    [Register] private readonly SomeServiceSystem _someServiceSystem;
}
```

## Systems

### Start System

```csharp
public partial class SomeSystem1 : IInitializeSystem
{
    public void Start()
    {
        // ...        
    }
}
```

### Update System

```csharp
public partial class SomeSystem2 : IUpdateSystem
{
    public void Update()
    {
        // ...        
    }
}
```

### Stashes

You dont need to initialize stashash it will be initialized and disposed automatically.

```csharp
public partial class SomeSystem : IStartSystem
{
    private Stash<InputMoveDirection> _inputStash;
    private Stash<PlayerPosition> _playerPosition;

    // ...
}
```

### Stash extensions
There are extensions for Stashes generated for each component.
For marker components without fields, there is the SetOrRemove(Entity entity, bool setOrRemove) extension.

```csharp
// Some examples
struct SomeComponent : IComponent {}

Stash<SomeComponent> _someStash;

var toggle = /*...*/;
_someStash.SetOrRemove(entity, toggle);
```

For components with fields, there is the new Set(Entity entity, /*...*/) extension. This allows you to assign values directly without creating a new structure.

```csharp
// Some examples
struct SomeComponent : IComponent
{
    public int Value;
}

Stash<SomeComponent> _someStash;

_someStash.Set(entity, value: 10);
```

### Filters

You can use generic filters. The first type or tuple defines with components, and an optional second type or tuple defines without components.

```csharp
public partial class SomeSystem : IStartSystem
{
    private Filter<Player> _anyPlayerFilter;
    private Filter<Player, Disabled> _activePlayerFilter;
    private Filter<(Player, Transform), Disabled> _playerWithTransformFilter;
    private Filter<(Player, Transform), (Dead, Disabled)> _alivePlayerFilter;

    // ...
}
```

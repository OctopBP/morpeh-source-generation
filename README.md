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

### Filters

For fillers you can use With and Without attributes. It also will be initialized and disposed automatically.

```csharp
public partial class SomeSystem : IStartSystem
{
    [With(typeof(InputMoveDirection))]
    private Filter _inputFilter;

    [With(typeof(Player), typeof(PlayerPosition))]
    [Without(typeof(Disabled))]
    private Filter _playerFilter;

    // ...
}
```

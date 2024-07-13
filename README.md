# Morpeh Source Generation

This is source generation library for [Morpeh Framework](https://github.com/scellecs/morpeh).
It can replace a lot of boilerplate code in your project.

## Table of Contents
* [How to install](#how-to-install)
* [Feature Runner](#feature-runner)
* [Features](#features)
* * [Register attribute](#register-attribute)
* [Systems](#systems)
* * [Initialize System](#initialize-system)
* * [Update System](#update-system)
* [Stashes](#stashes)
* * [Stash extensions](#stash-extensions)
* [Filters](#filters)

## How to install

1. Inport `Morpeh.SourceGeneration.dll` to your unity project
2. Disable all ckeckboxes in `Morpeh.SourceGeneration.dll` import settings panel. Press `Apply`
3. Add `RoslynAnalyzer` tag to `Morpeh.SourceGeneration.dll`

> If you using assembly definitions you have to plase `Morpeh.SourceGeneration.dll` into folder with `.asmdef` that have refference to `Morpeh`

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

### Initialize System

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

## Filters

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

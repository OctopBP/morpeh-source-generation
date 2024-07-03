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

# TinyContainer
A simple service locator implementation for Unity.

## Installation
You can install this package via the Unity Package Manager "Add package from git URL" option by using this URL:
`https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer` or add `"com.jnk.tinycontainer" : "https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer"` to `Packages/manifest.json`.

Use the `#*.*.*` postfix to use a specific release of the package, e.g. `https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer#1.0.0`.

## Usage
### Accessing a TinyContainer
`TinyContainer` instances are components which can be added to GameObjects and can be nested in the Unity hierarchy.
Use `TinyContainer.For(this)` in your MonoBehaviours to access the nearest `TinyContainer` upwards in the hierarchy.

There is a singleton-like instance called the `Root` which all requests will fall back to.
You can setup your `Root` manually by putting a GameObject with a `TinyContainer` and `TinyContainerRoot` component at the scene root. Otherwise it's created automatically when `TinyContainer.Root` is first accessed.

### Registering Instances
You have a few options when registering your types with a container.
```cs
private void Awake()
{
    TinyContainer.Root
        // Register a instance directly.
        .Register<ILocalizer>(new MockLocalizer())
        // Register a factory method which will be executed
        // when the type is first requested.
        .RegisterLazy<ISerializer>(_ => new JsonSerializer())
        // Register a factory method which will be executed
        // every time the type is requested.
        .RegisterPerRequest(_ => new MaterialPropertyBlock());
}
```
The explicit generic type argument is optional and can be used to register instances by interface or base class.

### Requesting Types
The requested type can be inferred from the `out` parameter. I recommended registering instances in `Awake()` and to resolve types in `Start()` to make sure everything is registered before it is being requested.
```cs
private ILocalizer _localizer;
private ISerializer _serializer;

private void Start()
{
    TinyContainer.For(this)
        .Get(out _localizer)
        .Get(out _serializer);
}
```

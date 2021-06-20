# TinyContainer
A simple service locator implementation for Unity.

## Disclaimer
This package is primarily for use in my personal projects and I almost certainly will be making breaking changes if I'm unhappy with the syntax / API.

## Installation
You can install this package via the Unity Package Manager "Add package from git URL" option by using this URL:
`https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer` or add `"com.jnk.tinycontainer" : "https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer"` to `Packages/manifest.json`.

Use the `#*.*.*` postfix to use a specific release of the package, e.g. `https://github.com/JanikHelbig/TinyContainer.git?path=Assets/TinyContainer#1.1.0`.

## Usage
### Accessing a TinyContainer
`TinyContainer` instances are components which can be added to GameObjects and can be nested in the Unity hierarchy.
Use `TinyContainer.For(this)` in your MonoBehaviours to access the nearest `TinyContainer` upwards in the hierarchy.

There is a singleton-like instance called the `Root` which all requests will fall back to.
You can setup your `Root` manually by putting a GameObject with a `TinyContainer` and `TinyContainerRoot` component at the scene root. Otherwise it's created automatically when `TinyContainer.Root` is first accessed.

Additionally you can create a `TinyContainer` which belongs to a specific scene. If you add a `TinyContainerScene` component to a container, your MonoBehaviours can access it by using `TinyContainer.ForSceneOf(this)`, provided they belong to the same scene. When trying to find the closest container or trying to resolve a type, the request will try to locate a scene container before falling back to the Root.

### Registering Instances
You have two options when registering your types with a container.
```cs
private void Awake()
{
    TinyContainer.Root
        // Register a instance directly.
        .Register<ILocalizer>(new MockLocalizer())
        // Register a factory method which will be executed
        // every time the type is requested.
        .RegisterPerRequest<ISerializer>(_ => new JsonSerializer());
}
```
The explicit generic type argument is optional and can be used to register instances by interface or base class.

### Resolving Types
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

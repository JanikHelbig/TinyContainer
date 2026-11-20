using System;
using System.Collections.Generic;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jnk.TinyContainer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TinyContainer/TinyContainer")]
    public class TinyContainer : MonoBehaviour
    {
        private static TinyContainer _global;
        private static readonly Dictionary<Scene, TinyContainer> _sceneContainers = new();
        private static readonly List<GameObject> _temporarySceneGameObjects = new();

        /// <summary>
        /// The global container instance.
        /// </summary>
        public static TinyContainer Global
        {
            get
            {
                if (_global != null)
                    return _global;

                #if UNITY_2023_1_OR_NEWER
                if (FindAnyObjectByType<TinyContainerGlobal>() is {} global)
                #else
                if (FindObjectOfType<TinyContainerGlobal>() is {} global)
                #endif
                {
                    global.BootstrapOnDemand();
                    return _global;
                }

                var container = new GameObject("TinyContainer [Global]", typeof(TinyContainer));
                container.AddComponent<TinyContainerGlobal>().BootstrapOnDemand();

                return _global;
            }
        }

        public static bool IsGlobalConfigured => _global != null;

        [SerializeField] private EventFunction enabledEventFunctions = EventFunction.FixedUpdate | EventFunction.Update | EventFunction.LateUpdate;
        [SerializeField] private bool disposeOnDestroy = true;

        private readonly Dictionary<Type, object> _instances = new();
        private readonly Dictionary<Type, Func<TinyContainer, object>> _factories = new();
        private readonly List<IFixedUpdateHandler> _fixedUpdateHandlers = new();
        private readonly List<IUpdateHandler> _updateHandlers = new();
        private readonly List<ILateUpdateHandler> _lateUpdateHandlers = new();

        public EventFunction EnabledEventFunctions
        {
            get => enabledEventFunctions;
            set => enabledEventFunctions = value;
        }

        public IEnumerable<object> RegisteredInstances => _instances.Values;

        private bool IsSceneContainer => _sceneContainers.TryGetValue(gameObject.scene, out TinyContainer container) && container == this;

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_global != null && _global == this)
            {
                Debug.LogWarning("This TinyContainer has already been configured as the Global instance.", this);
                return;
            }

            if (_global != null && _global != this)
            {
                Debug.LogError("Another TinyContainer has already been configured as the Global instance.", this);
                return;
            }

            _global = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(_global);
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (_sceneContainers.ContainsKey(scene))
            {
                Debug.LogError($"Another TinyContainer has already been configured for scene '{scene.name}'.", this);
                return;
            }

            if (transform.parent != null)
                Debug.LogWarning($"The scene container for scene '{scene.name}' is not on a root GameObject. Lookups from this scene that run before its Awake fall back to Global.", this);

            _sceneContainers[scene] = this;
        }

        /// <summary>
        /// Returns the <see cref="TinyContainer"/> configured for the scene of the MonoBehaviour. Falls back to the global instance.
        /// </summary>
        public static TinyContainer ForSceneOf(MonoBehaviour monoBehaviour)
        {
            Scene scene = monoBehaviour.gameObject.scene;

            if (_sceneContainers.TryGetValue(scene, out TinyContainer container) && container != monoBehaviour)
                return container;

            _temporarySceneGameObjects.Clear();
            scene.GetRootGameObjects(_temporarySceneGameObjects);

            foreach (GameObject go in _temporarySceneGameObjects)
            {
                if (go.TryGetComponent(out TinyContainerScene sceneContainer) == false)
                    continue;

                if (sceneContainer.Container == monoBehaviour)
                    continue;

                sceneContainer.BootstrapOnDemand();
                return sceneContainer.Container;
            }

            return Global;
        }

        /// <summary>
        /// Returns the closest <see cref="TinyContainer"/> upwards in the hierarchy. Falls back to the scene container, then to the global instance.
        /// </summary>
        public static TinyContainer For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.IsNull()?.GetComponentInParent<TinyContainer>().IsNull()
                   ?? ForSceneOf(monoBehaviour)
                   ?? Global;
        }

        /// <summary>
        /// Register the instance with the container.
        /// </summary>
        public TinyContainer Register<T>(T instance)
        {
            Type type = typeof(T);

            if (IsNullOrDestroyed(instance))
                throw new ArgumentNullException(nameof(instance), $"Cannot register a null or destroyed instance as '{type.Name}'.");

            if (IsNotRegistered(type))
                AddInstance(type, instance);

            return this;
        }

        /// <summary>
        /// Register the instance with the container.
        /// </summary>
        public TinyContainer Register(Type type, object instance)
        {
            if (IsNullOrDestroyed(instance))
                throw new ArgumentNullException(nameof(instance), $"Cannot register a null or destroyed instance as '{type.Name}'.");

            if (type.IsInstanceOfType(instance) == false)
                throw new ArgumentException("Type of instance does not match.", nameof(instance));

            if (IsNotRegistered(type))
                AddInstance(type, instance);

            return this;
        }

        /// <summary>
        /// Register a per-request factory method with the container.
        /// </summary>
        public TinyContainer RegisterPerRequest<T>(Func<TinyContainer, T> factoryMethod) where T : class
        {
            Type type = typeof(T);

            if (IsNotRegistered(type) == false)
                return this;

            if (typeof(IFixedUpdateHandler).IsAssignableFrom(type))
                Debug.LogWarning($"Type {type.Name} was registered per request. {nameof(IFixedUpdateHandler.FixedUpdate)} will not be called.", this);

            if (typeof(IUpdateHandler).IsAssignableFrom(type))
                Debug.LogWarning($"Type {type.Name} was registered per request. {nameof(IUpdateHandler.Update)} will not be called.", this);

            if (typeof(ILateUpdateHandler).IsAssignableFrom(type))
                Debug.LogWarning($"Type {type.Name} was registered per request. {nameof(ILateUpdateHandler.LateUpdate)} will not be called.", this);

            _factories[type] = factoryMethod;

            return this;
        }

        private bool IsNotRegistered(Type type)
        {
            if (_instances.ContainsKey(type) == false &&
                _factories.ContainsKey(type) == false)
                return true;

            Debug.LogError($"Type {type.FullName} has already been registered.", this);
            return false;
        }

        private void AddInstance(Type type, object instance)
        {
            _instances[type] = instance;

            AddUnique(_fixedUpdateHandlers, instance);
            AddUnique(_updateHandlers, instance);
            AddUnique(_lateUpdateHandlers, instance);
        }

        private static void AddUnique<T>(List<T> list, object instance) where T : class
        {
            if (instance is T item && list.Exists(x => ReferenceEquals(x, item)) == false)
                list.Add(item);
        }

        private static bool IsNullOrDestroyed(object instance)
        {
            return instance == null || instance is UnityEngine.Object unityObject && unityObject == null;
        }

        /// <summary>
        /// Returns the first instance for the type upwards in the hierarchy.
        /// </summary>
        public TinyContainer Get<T>(out T instance) where T : class
        {
            instance = Get<T>();
            return this;
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            T instance = null;

            if (TryGetInstance(type, ref instance))
            {
                if (IsNullOrDestroyed(instance))
                    throw new TypeNotFoundException($"The instance registered for type '{type.Name}' in container '{name}' (scene '{gameObject.scene.name}') has been destroyed.");

                return instance;
            }

            if (TryGetInstanceFromFactory(type, ref instance))
                return instance;

            if (TryGetNextContainerInHierarchy(out TinyContainer nextContainer))
                return nextContainer.Get<T>();

            throw new TypeNotFoundException($"Could not resolve type '{type.Name}'.");
        }

        /// <summary>
        /// Returns the first instance for the type upwards in the hierarchy.
        /// </summary>
        public bool TryGet<T>(out T instance) where T : class
        {
            Type type = typeof(T);
            instance = null;

            if (TryGetInstance(type, ref instance))
            {
                if (IsNullOrDestroyed(instance) == false)
                    return true;

                instance = null;
                return false;
            }

            if (TryGetInstanceFromFactory(type, ref instance))
                return true;

            return TryGetNextContainerInHierarchy(out TinyContainer nextContainer) && nextContainer.TryGet(out instance);
        }

        private bool TryGetInstance<T>(Type type, ref T instance) where T : class
        {
            if (false == _instances.TryGetValue(type, out object obj))
                return false;

            instance = (T) obj;

            return true;
        }

        private bool TryGetInstanceFromFactory<T>(Type type, ref T instance) where T : class
        {
            if (false == _factories.TryGetValue(type, out Func<TinyContainer, object> objFactory))
                return false;

            instance = (T) objFactory.Invoke(this);

            return true;
        }

        private bool TryGetNextContainerInHierarchy(out TinyContainer container)
        {
            if (this == _global)
            {
                container = null;
                return false;
            }

            container = transform.parent.IsNull()?.GetComponentInParent<TinyContainer>().IsNull()
                        ?? (IsSceneContainer ? Global : ForSceneOf(this));
            return true;
        }

        private void FixedUpdate()
        {
            if (!enabledEventFunctions.HasFlag(EventFunction.FixedUpdate))
                return;

            foreach (IFixedUpdateHandler handler in _fixedUpdateHandlers)
                handler.FixedUpdate();
        }

        private void Update()
        {
            if (!enabledEventFunctions.HasFlag(EventFunction.Update))
                return;

            foreach (IUpdateHandler handler in _updateHandlers)
                handler.Update();
        }

        private void LateUpdate()
        {
            if (!enabledEventFunctions.HasFlag(EventFunction.LateUpdate))
                return;

            foreach (ILateUpdateHandler handler in _lateUpdateHandlers)
                handler.LateUpdate();
        }

        private void OnDestroy()
        {
            UnregisterForSceneIfNecessary();
            HandleRegisteredIDisposables();

            _instances.Clear();
            _factories.Clear();

            if (this == _global)
                _global = null;
        }

        private void UnregisterForSceneIfNecessary()
        {
            if (_sceneContainers.ContainsValue(this) == false)
                return;

            bool removedSuccessfully = _sceneContainers.Remove(gameObject.scene);
            Debug.Assert(removedSuccessfully, "Error when removing TinyContainer from scene dictionary. You might have moved the container to a different scene. This is not supported.", this);
        }

        private void HandleRegisteredIDisposables()
        {
            if (disposeOnDestroy == false) return;

            var disposables = new List<IDisposable>();
            foreach (object instance in _instances.Values)
                AddUnique(disposables, instance);

            foreach (IDisposable disposable in disposables)
                disposable.Dispose();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticFields()
        {
            _global = null;
            _sceneContainers.Clear();
            _temporarySceneGameObjects.Clear();
        }

        #if UNITY_EDITOR

        [MenuItem("GameObject/TinyContainer/Add Global Container")]
        private static void AddGlobalContainer()
        {
            var go = new GameObject("TinyContainer [Global]", typeof(TinyContainerGlobal));
        }

        [MenuItem("GameObject/TinyContainer/Add Scene Container")]
        private static void AddSceneContainer()
        {
            var go = new GameObject("TinyContainer [Scene]", typeof(TinyContainerScene));
        }

        #endif
    }
}

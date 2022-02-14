using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Jnk.TinyContainer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TinyContainer/TinyContainer")]
    public class TinyContainer : MonoBehaviour
    {
        private static TinyContainer _global;
        private static Dictionary<Scene, TinyContainer> _sceneContainers;
        private static List<GameObject> _temporarySceneGameObjects;

        /// <summary>
        /// The global container instance.
        /// </summary>
        public static TinyContainer Global
        {
            get
            {
                if (_global != null)
                    return _global;

                if (FindObjectOfType<TinyContainerGlobal>() is {} global)
                {
                    global.BootstrapOnDemand();
                    return _global;
                }

                var container = new GameObject("TinyContainer [Global]", typeof(TinyContainer));
                container.AddComponent<TinyContainerGlobal>().BootstrapOnDemand();

                return _global;
            }
        }

        [SerializeField]
        private bool disposeOnDestroy = true;

        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<TinyContainer, object>> _factories = new Dictionary<Type, Func<TinyContainer, object>>();

        public IEnumerable<object> RegisteredInstances => _instances.Values;

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (_global != null && _global != this)
            {
                Debug.LogError("TinyContainer Global has already been configured.", this);
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
                Debug.LogError($"TinyContainer for the scene {scene.name} has already been configured.", this);
                return;
            }

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
            return monoBehaviour.GetComponentInParent<TinyContainer>().IsNull() ?? ForSceneOf(monoBehaviour) ?? Global;
        }

        /// <summary>
        /// Register the instance with the container.
        /// </summary>
        public TinyContainer Register<T>(T instance)
        {
            Type type = typeof(T);

            if (IsNotRegistered(type))
                _instances[type] = instance;

            return this;
        }

        /// <summary>
        /// Register the instance with the container.
        /// </summary>
        public TinyContainer Register(Type type, object instance)
        {
            if (type.IsInstanceOfType(instance) == false)
                throw new ArgumentException("Type of instance does not match.", nameof(instance));

            if (IsNotRegistered(type))
                _instances[type] = instance;

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

            if (type.IsAssignableFrom(typeof(IUpdateHandler)) == false)
                Debug.LogWarning($"Type {type.Name} was registered per request. {nameof(IUpdateHandler.Update)} will not be called.");

            if (type.IsAssignableFrom(typeof(IFixedUpdateHandler)) == false)
                Debug.LogWarning($"Type {type.Name} was registered per request. {nameof(IFixedUpdateHandler.FixedUpdate)} will not be called.");

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

        /// <summary>
        /// Returns the first instance for the type upwards in the hierarchy.
        /// </summary>
        public TinyContainer Get<T>(out T instance) where T : class
        {
            Type type = typeof(T);
            instance = null;

            if (TryGetInstance(type, ref instance))
                return this;

            if (TryGetInstanceFromFactory(type, ref instance))
                return this;

            if (TryGetNextContainerInHierarchy(out TinyContainer nextContainer))
            {
                nextContainer.Get(out instance);
                return this;
            }

            throw new TypeNotFoundException($"Could not resolve type '{typeof(T).Name}'.");
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            T instance = null;

            if (TryGetInstance(type, ref instance))
                return instance;

            if (TryGetInstanceFromFactory(type, ref instance))
                return instance;

            if (TryGetNextContainerInHierarchy(out TinyContainer nextContainer))
                return nextContainer.Get<T>();

            throw new TypeNotFoundException($"Could not resolve type '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Returns the first instance for the type upwards in the hierarchy.
        /// </summary>
        public bool TryGet<T>(out T instance) where T : class
        {
            Type type = typeof(T);
            instance = null;

            if (TryGetInstance(type, ref instance))
                return true;

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

            container = transform.parent.IsNull()?.GetComponentInParent<TinyContainer>().IsNull() ?? ForSceneOf(this);
            return true;
        }

        private void Update()
        {
            foreach (object instance in _instances.Values)
                if (instance is IUpdateHandler updateHandler)
                    updateHandler.Update();
        }

        private void FixedUpdate()
        {
            foreach (object instance in _instances.Values)
                if (instance is IFixedUpdateHandler fixedUpdateHandler)
                    fixedUpdateHandler.FixedUpdate();
        }

        private void OnDestroy()
        {
            UnregisterForSceneIfNecessary();
            HandleRegisteredIDisposables();
        }

        private void UnregisterForSceneIfNecessary()
        {
            if (_sceneContainers == null || _sceneContainers.ContainsValue(this) == false)
                return;

            bool removedSuccessfully = _sceneContainers.Remove(gameObject.scene);
            Debug.Assert(removedSuccessfully, "Error when removing TinyContainer from scene dictionary. You might have moved the container to a different scene. This is not supported.", this);
        }

        private void HandleRegisteredIDisposables()
        {
            if (disposeOnDestroy == false) return;

            foreach (IDisposable disposable in _instances.Values.OfType<IDisposable>())
                disposable.Dispose();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticFields()
        {
            _global = null;
            _sceneContainers = new Dictionary<Scene, TinyContainer>();
            _temporarySceneGameObjects = new List<GameObject>();
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

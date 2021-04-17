using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TinyContainer/TinyContainer")]
    public class TinyContainer : MonoBehaviour
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<TinyContainer, object>> _lazyInstances = new Dictionary<Type, Func<TinyContainer, object>>();
        private readonly Dictionary<Type, Func<TinyContainer, object>> _factories = new Dictionary<Type, Func<TinyContainer, object>>();

        private static TinyContainer _root;

        /// <summary>
        /// The root container instance.
        /// </summary>
        public static TinyContainer Root
        {
            get
            {
                if (_root != null)
                    return _root;

                if (FindObjectOfType<TinyContainerRoot>() is {} root)
                {
                    root.BootstrapOnDemand();
                    return _root;
                }

                var container = new GameObject("TinyContainer [Root]", typeof(TinyContainer));
                container.AddComponent<TinyContainerRoot>().BootstrapOnDemand();

                return _root;
            }
        }

        internal void ConfigureAsRoot()
        {
            if (_root != null && _root != this)
            {
                Debug.LogWarning("TinyContainer Root has already been configured.", this);
                return;
            }

            _root = this;
            DontDestroyOnLoad(_root);
        }

        /// <summary>
        /// Returns the closest <see cref="TinyContainer"/> upwards in the hierarchy. Falls back to the root container.
        /// </summary>
        public static TinyContainer For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponentInParent<TinyContainer>().IsNull() ?? Root;
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

            if (TryGetInstanceLazily(type, ref instance))
                return this;

            if (TryGetInstanceFromFactory(type, ref instance))
                return this;

            if (TryGetInstanceFromParentContainer(out instance))
                return this;

            Debug.LogError($"Could not find instance for parameter of type {type.FullName}.", this);
            return this;
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
        /// Register a factory method for instantiating the instance on demand.
        /// </summary>
        public TinyContainer RegisterLazy<T>(Func<TinyContainer, T> factoryMethod) where T : class
        {
            Type type = typeof(T);

            if (IsNotRegistered(type))
                _lazyInstances[type] = factoryMethod;

            return this;
        }

        /// <summary>
        /// Register a per-request factory method with the container.
        /// </summary>
        public TinyContainer RegisterPerRequest<T>(Func<TinyContainer, T> factoryMethod) where T : class
        {
            Type type = typeof(T);

            if (IsNotRegistered(type))
                _factories[type] = factoryMethod;

            return this;
        }

        private bool IsNotRegistered(Type type)
        {
            if (_instances.ContainsKey(type) || _lazyInstances.ContainsKey(type) || _factories.ContainsKey(type))
            {
                Debug.LogWarning($"Type {type.FullName} has already been registered. Skipping.", this);
                return false;
            }

            Debug.Log($"Type {type.FullName} registered.", this);
            return true;
        }

        private bool TryGetInstance<T>(Type type, ref T instance) where T : class
        {
            if (false == _instances.TryGetValue(type, out object obj))
                return false;

            instance = (T) obj;

            return true;
        }

        private bool TryGetInstanceLazily<T>(Type type, ref T instance) where T : class
        {
            if (false == _lazyInstances.TryGetValue(type, out Func<TinyContainer, object> objFactory))
                return false;

            instance = (T) objFactory.Invoke(this);

            _lazyInstances.Remove(type);
            _instances.Add(type, instance);

            return true;
        }

        private bool TryGetInstanceFromFactory<T>(Type type, ref T instance) where T : class
        {
            if (false == _factories.TryGetValue(type, out Func<TinyContainer, object> objFactory))
                return false;

            instance = (T) objFactory.Invoke(this);

            return true;
        }

        private bool TryGetInstanceFromParentContainer<T>(out T instance) where T : class
        {
            if (this == _root)
            {
                instance = null;
                return false;
            }

            TinyContainer parentContainer = transform.parent.IsNull()?.GetComponentInParent<TinyContainer>().IsNull() ?? Root;
            parentContainer.Get(out instance);

            return instance != null;
        }
    }
}

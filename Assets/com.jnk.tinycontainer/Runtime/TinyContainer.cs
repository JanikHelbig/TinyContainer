using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TinyContainer/TinyContainer")]
    public class TinyContainer : MonoBehaviour
    {
        private static TinyContainer _root;

        public static TinyContainer Root
        {
            get
            {
                if (_root != null)
                    return _root;

                if (FindObjectOfType<TinyContainerRoot>() is { } containerRoot)
                {
                    containerRoot.Initialize();
                }
                else
                {
                    Debug.LogError("Could not find Root container.");
                }

                return _root;
            }
        }

        private readonly Dictionary<Type, object> _instanceDictionary = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<TinyContainer, object>> _factoryDictionary = new Dictionary<Type, Func<TinyContainer, object>>();

        public void ConfigureAsRoot()
        {
            if (_root != null && _root != this)
            {
                Debug.LogWarning("TinyContainer Root has already been configured.", this);
                return;
            }

            _root = this;

            DontDestroyOnLoad(_root);
        }

        public static TinyContainer For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponentInParent<TinyContainer>().IsNull() ?? Root;
        }

        public TinyContainer Get<T>(out T instance) where T : class
        {
            Type type = typeof(T);
            instance = null;

            if (TryGetInstance(type, ref instance))
                return this;

            if (TryGetInstanceFromFactory(type, ref instance))
                return this;

            if (TryGetInstanceFromParentContainer(out instance))
                return this;

            Debug.LogError($"Could not find instance for parameter of type {type.FullName}.", this);
            return this;
        }

        public TinyContainer RegisterInstance<T>(T instance)
        {
            Type type = typeof(T);

            if (IsNotRegistered(type))
                _instanceDictionary[type] = instance;

            return this;
        }

        public TinyContainer RegisterInstance(Type type, object instance)
        {
            if (false == type.IsInstanceOfType(instance))
                throw new ArgumentException("Instance type does not match.", nameof(instance));

            if (IsNotRegistered(type))
                _instanceDictionary[type] = instance;

            return this;
        }

        public TinyContainer RegisterFactory<T>(Func<TinyContainer, T> factoryMethod) where T : class
        {
            Type type = typeof(T);

            if (IsNotRegistered(type))
                _factoryDictionary[type] = factoryMethod;

            return this;
        }

        private bool IsNotRegistered(Type type)
        {
            if (_instanceDictionary.ContainsKey(type) || _factoryDictionary.ContainsKey(type))
            {
                Debug.LogWarning($"Type {type.FullName} has already been registered. Skipping.", this);
                return false;
            }

            Debug.Log($"Type {type.FullName} registered.", this);
            return true;
        }

        private bool TryGetInstance<T>(Type type, ref T instance) where T : class
        {
            if (false == _instanceDictionary.TryGetValue(type, out object obj))
                return false;

            instance = (T) obj;
            return true;
        }

        private bool TryGetInstanceFromFactory<T>(Type type, ref T instance) where T : class
        {
            if (false == _factoryDictionary.TryGetValue(type, out Func<TinyContainer, object> objFactory))
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

            parentContainer.Get(out T instanceInParent);
            instance = instanceInParent;
            return instance != null;
        }
    }
}

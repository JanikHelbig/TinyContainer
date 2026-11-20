using System.Collections.Generic;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Object Installer")]
    public class TinyContainerObjectInstaller : MonoBehaviour
    {
        [SerializeField]
        private List<Object> objects;

        protected void Awake()
        {
            TinyContainer container = TinyContainer.For(this);

            for (var i = 0; i < objects.Count; i++)
            {
                Object obj = objects[i];

                if (obj == null)
                {
                    Debug.LogError($"Element {i} of the {nameof(TinyContainerObjectInstaller)} on '{name}' is missing or not assigned and was not registered.", this);
                    continue;
                }

                container.Register(obj.GetType(), obj);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Object Bootstrapper")]
    public class TinyContainerObjectInstaller : MonoBehaviour
    {
        [SerializeField]
        private List<Object> objects;

        protected void Awake()
        {
            TinyContainer container = TinyContainer.For(this);

            foreach (Object obj in objects)
                container.Register(obj.GetType(), obj);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Object Bootstrapper")]
    public class TinyContainerObjectBootstrapper : TinyContainerBootstrapperBase
    {
        [SerializeField]
        private List<Object> objects;
        
        protected override void Bootstrap()
        {
            foreach (Object obj in objects)
                Container.RegisterInstance(obj.GetType(), obj);
        }
    }
}
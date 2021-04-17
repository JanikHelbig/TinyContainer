using System;
using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Root")]
    public class TinyContainerRoot : TinyContainerBootstrapperBase
    {
        private bool _hasBeenBootstrapped;

        protected override void Bootstrap()
        {
            if (_hasBeenBootstrapped == false)
                Container.ConfigureAsRoot();
        }

        public void BootstrapOnDemand()
        {
            Bootstrap();
            _hasBeenBootstrapped = true;
        }
    }
}

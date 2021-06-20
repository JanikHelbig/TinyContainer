using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Scene")]
    public class TinyContainerScene : TinyContainerBootstrapperBase
    {
        private bool _hasBeenBootstrapped;

        protected override void Bootstrap()
        {
            if (_hasBeenBootstrapped == false)
                Container.ConfigureForScene();

            _hasBeenBootstrapped = true;
        }

        public void BootstrapOnDemand() => Bootstrap();
    }
}

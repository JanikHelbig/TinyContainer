using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Global")]
    public class TinyContainerGlobal : TinyContainerBootstrapperBase
    {
        [SerializeField]
        private bool dontDestroyOnLoad;

        private bool _hasBeenBootstrapped;

        protected override void Bootstrap()
        {
            if (_hasBeenBootstrapped == false)
                Container.ConfigureAsGlobal(dontDestroyOnLoad);

            _hasBeenBootstrapped = true;
        }

        public void BootstrapOnDemand() => Bootstrap();
    }
}

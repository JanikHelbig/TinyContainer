using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Global")]
    public class TinyContainerGlobal : TinyContainerBootstrapperBase
    {
        [SerializeField]
        private bool dontDestroyOnLoad;

        protected override void Bootstrap()
        {
            Container.ConfigureAsGlobal(dontDestroyOnLoad);
        }

        public void BootstrapOnDemand() => Bootstrap();
    }
}

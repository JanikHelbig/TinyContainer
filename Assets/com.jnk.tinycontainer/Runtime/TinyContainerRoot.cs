using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Root")]
    public class TinyContainerRoot : TinyContainerBootstrapperBase
    {
        protected override void Bootstrap() => Container.ConfigureAsRoot();
        public void Initialize() => Bootstrap();
    }
}

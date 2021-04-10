using UnityEngine;

namespace Jnk.TinyContainer
{
    [RequireComponent(typeof(TinyContainer))]
    public abstract class TinyContainerBootstrapperBase : MonoBehaviour
    {
        private TinyContainer _container;
        protected TinyContainer Container => _container.IsNull() ?? (_container = GetComponent<TinyContainer>());
        
        private void Awake() => Bootstrap();

        protected abstract void Bootstrap();
    }
}
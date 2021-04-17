using UnityEngine;
using Jnk.TinyContainer;

namespace Examples
{
    public class ExampleBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            TinyContainer.Root
                .RegisterLazy<ILocalization>(_ => new MockLocalization())
                .RegisterLazy<ISerializer>(_ => new JsonSerializer());
        }
    }
}

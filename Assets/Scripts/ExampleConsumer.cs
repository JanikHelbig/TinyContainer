using UnityEngine;
using Jnk.TinyContainer;

namespace Examples
{
    public class ExampleConsumer : MonoBehaviour
    {
        private ILocalization _localization;
        private ISerializer _serializer;

        private void Start()
        {
            TinyContainer.For(this)
                .Get(out _localization)
                .Get(out _serializer);
        }
    }
}

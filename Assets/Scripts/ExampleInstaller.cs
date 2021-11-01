using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Jnk.TinyContainer;

namespace Examples
{
    public class ExampleInstaller : MonoBehaviour
    {
        private void Awake()
        {
            TinyContainer.Global
                .Register(new LifeCyclePrinter())
                .Register<ILocalization>(new MockLocalization());

            TinyContainer.ForSceneOf(this)
                .Register<ISerializer>(new JsonSerializer());
        }

        private void Start()
        {
            bool found = TinyContainer.Global.TryGet(out ISerializer serializer);

            if (found == false)
                Debug.Log($"Could not find serializer.");

            foreach (var startHandler in TinyContainer.Global.RegisteredInstances.OfType<ICustomStartHandler>())
                startHandler.Start();
        }
    }
}

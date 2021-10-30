using UnityEngine;
using Jnk.TinyContainer;

namespace Examples
{
    public class LifeCyclePrinter : IUpdateHandler, IFixedUpdateHandler, ICustomStartHandler
    {
        public void Update()
        {
            Debug.Log("Update");
        }

        public void FixedUpdate()
        {
            Debug.Log("FixedUpdate");
        }

        public void Start()
        {
            Debug.Log("Start");
        }
    }
}
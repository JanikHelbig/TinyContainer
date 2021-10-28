using Jnk.TinyContainer;
using UnityEngine;

namespace Examples
{
    public class UpdatePrinter : IUpdateHandler, IFixedUpdateHandler
    {
        public void Update()
        {
            Debug.Log("Update");
        }

        public void FixedUpdate()
        {
            Debug.Log("FixedUpdate");
        }
    }
}
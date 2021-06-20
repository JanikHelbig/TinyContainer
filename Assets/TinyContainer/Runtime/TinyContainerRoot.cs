﻿using UnityEngine;

namespace Jnk.TinyContainer
{
    [AddComponentMenu("TinyContainer/TinyContainer Root")]
    public class TinyContainerRoot : TinyContainerBootstrapperBase
    {
        [SerializeField]
        private bool dontDestroyOnLoad;

        private bool _hasBeenBootstrapped;

        protected override void Bootstrap()
        {
            if (_hasBeenBootstrapped == false)
                Container.ConfigureAsRoot(dontDestroyOnLoad);

            _hasBeenBootstrapped = true;
        }

        public void BootstrapOnDemand() => Bootstrap();
    }
}
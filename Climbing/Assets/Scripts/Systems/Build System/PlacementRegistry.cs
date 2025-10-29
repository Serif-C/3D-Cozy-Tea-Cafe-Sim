using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TeaShop.Systems.Building
{
    public class PlacementRegistry : MonoBehaviour
    {
        public event Action<PlaceableInstance> Placed;
        public event Action<PlaceableInstance> Removed;

        private readonly List<PlaceableInstance> instances = new List<PlaceableInstance>();

        public void Register(PlaceableInstance inst)
        {
            if (inst == null) return;
            if (!instances.Contains(inst)) instances.Add(inst);
            Action<PlaceableInstance> h = Placed;
            if ( h != null) h(inst);
        }

        public void UnRegister(PlaceableInstance inst)
        {
            if (inst == null) return;
            if (instances.Remove(inst))
            {
                Action<PlaceableInstance> h = Removed;
                if (h != null) h(inst);
            }
        }

        public IReadOnlyList<PlaceableInstance> All() { return instances; }
    }
}

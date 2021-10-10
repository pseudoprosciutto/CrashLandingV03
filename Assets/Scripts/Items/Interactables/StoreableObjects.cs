using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    /// <summary>
    /// A type of holdable objects which also allows the object to be held in inventory
    /// if in inventory, than can be used with equipUse button.
    /// </summary>
    public class StoreableObjects : HoldableObjects
    {
public virtual void Store() { }
        public virtual void SwitchOut() { }
        public bool canBeStored { get; protected set; }

        private void Awake()
        {
            base.Awake();
            canBeStored=true;
        }
    }
}

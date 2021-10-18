using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    public class LaserPistol : HoldableObjects, IEquippedUseable
    {
        void FixedUpdate()
        {
            base.FixedUpdate();
            LookForEquipmentBeingUsed();
        }

        private void LookForEquipmentBeingUsed()
        {
            
        }

        public void UseAsEquipment()
        {
            Shoot();
        }

        #region Laser Pistol Methods
        void Shoot() { print("Pew Pew!"); }
        #endregion

    }
}

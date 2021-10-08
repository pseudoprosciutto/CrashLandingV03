/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    /// <summary>
	/// Handles Character Item relations when Item is in hands.
	/// </summary>
    public class InventorySystem : MonoBehaviour
    {
        CharacterEngine Engine;
        GameObject objectStored;
        GameObject tempObject;

        public bool changeObjectCoolingDown;  //is object cooling down?
        public float changeObjectCoolDownTime = 1.2f;
        void Awake()
        {
            changeObjectCoolingDown = false;
        }
    }
}

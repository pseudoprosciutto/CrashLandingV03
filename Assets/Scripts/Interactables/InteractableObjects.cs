/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    /// <summary>
    /// Base class for which objects which the character interacts with
    /// are abstracted from.
    ///
    /// state can be interacted with
    /// state is being interacted with
    /// </summary>
    public class InteractableObjects : MonoBehaviour //ScriptableObject
    {
    // an interactable doesnt always have to be interactable
    //  public bool isInteractableState {get; protected set;}
        public bool isInteractedWith { get; protected set; }
        public bool canBeStored { get; protected set; }

        //to be overwritten
        public virtual void Interact(CharacterEngine character)
        {
            //this script wont show unless using this class as a test.
            Debug.Log("Object recognizes - interact test");
            character.isInteracting_Test = true;
        }
    }
}
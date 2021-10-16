/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity;

namespace CL03
{
    /// <summary>
    /// Crates are puzzle pieces used to move jump platforms and used as a weight for pressure buttons
	/// needs to know if it is held, if it is on the ground   
    /// </summary>
    public class CrateObject : HoldableObjects
    {
        #region Properties
        //Object State

        
        //need collider for box collider size to bring to other
        protected BoxCollider2D boxCollider;
        #endregion

        #region Awake, Enable, Disable functions
        public override void Awake()
        {
//            inPushOrPullState = false;
            objectCollider = GetComponent<Collider2D>();
            isThrowable = true;
            boxCollider = GetComponent<BoxCollider2D>();
            rb = GetComponent<Rigidbody2D>();
            CanBeHeld_On();
            rb.mass = 6;
            objectMass = rb.mass;
            canBeStored = false;
        }
        private void OnEnable()
        {
            //isHeld = lastHoldState; //  --redundant?

            //default state is on
            CanBeHeld_On();
        }

        private void OnDisable()
        {
            //  lastHoldState = isHeld;
            CanBeHeld_Off();
        }
        //Update State
        public override void IsHeldPositionCheck()
        {
            //Being held
            if (isHeld)
            {
                
                if (HeldBy != null)
                {
                rb.bodyType = RigidbodyType2D.Static;
                rb.mass = 0;
                    //make static

                    //ensure it is kepts in position
                    //				ObjectBeingHeld.transform.position = activeHoldPosition.position; //for when I can say elsewhere what is activeHoldPosition

                    if (HeldBy_Inventory.isHoldingSomethingAbove) //held above so stay above
                        this.gameObject.transform.position = HeldBy_Inventory.holdPoint_Above.position;

                    else if (!HeldBy_Inventory.isHoldingSomethingAbove) //held front so stay front
                        this.gameObject.transform.position = HeldBy_Inventory.holdPoint_Front.position;
                    /** creates overflow error
                    //Cast the ray to check above the player's head
                    RaycastHit2D headCheck = Raycast(new Vector2(0f, boxCollider.size.y), Vector2.up, .2f);

                    //If that ray hits, the player's head is blocked
                    if (headCheck.collider.gameObject.CompareTag("Surface"))
                    {
                        HeldBy_Engine.isHeadBlocked = true;

                    }
    */
                }
                else
                {
                    rb.mass = objectMass;
                }
            }
        }
        #endregion

        #region Interactions
        /// <summary>
        /// CrateObject interact will hold and 
        /// Interact - function found on all InteractableObjectss
        /// </summary>
        /// <param name="character"></param>
        public override void Interact(CharacterEngine character)
        {
            //Throw crate:
            //if is held and is throwable and the character is holding this object
            if (isHeld && isThrowable && HeldBy.Equals(character.gameObject))
            {
                //change body type
                rb.bodyType = RigidbodyType2D.Dynamic;
                //add force to this object to throw it

                //change state so it can no longer be seen as held
                Throw();
            }
            //Pick up object:
            //if object is not held
            if (!isHeld)
            {
                rb.mass = 1;
                GetPickedUp(character);
            }
        }





        #endregion
        RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
        {
            //Call the overloaded Raycast() method using the ground layermask and return 
            //the results
            return Raycast(offset, rayDirection, length);
        }
    }
}

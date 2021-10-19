using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
{
    public class LaserPistol : HoldableObjects, IEquippedUseable
    {
        [Required]
        BoxCollider2D BC;

        bool shootingIsCoolingDown = false;
        float shootingCoolDownTime = .25f;

        float colliderSizeHeldX;
        float colliderSizeHeldY;
        float colliderSizeGroundX;
        float colliderSizeGroundY;
        Vector2 colliderSizeHeld;
        Vector2 colliderSizeGround;

        private void Start()
        {
            BC = GetComponent<BoxCollider2D>();
            colliderSizeGround = new Vector2(colliderSizeGroundX, colliderSizeGroundY);
            colliderSizeHeld = new Vector2(colliderSizeHeldX, colliderSizeHeldY);
            useableInInventory = true;
        }
        public override void FixedUpdate()
        {
            //placement of equipped item
            if (isInInventory)
            {
                this.gameObject.transform.position = new Vector3(HeldBy.transform.position.x, HeldBy.transform.position.y+1, HeldBy.transform.position.z);


                if (HeldBy_Engine.direction != objectDirection)
                {
                    FlipObjectDirection();
                }
            }
            base.FixedUpdate();
          //  LookForEquipmentBeingUsed();
        }


        void FlipObjectDirection()
        {
            //Turn the character by flipping the direction
            objectDirection *= -1;
            //Record the current scale
            Vector3 scale = transform.localScale;
            //Set the X scale to be the original times the direction
            scale.x = originalXScale * objectDirection;

            //Apply the new scale
            transform.localScale = scale;
        }


        public override void Interact(CharacterEngine character)
        {
            base.Interact(character);
        }

        private void LookForEquipmentBeingUsed()
        {
            if (isInHands) BC.size = colliderSizeHeld; else BC.size = colliderSizeGround;
        }

        #region Inventory Equipping States

        /// <summary>
        /// Change state to store object in inventory
        /// </summary>
        public override void StoreInInventory()
        {
            isInHands = false;
            isInInventory = true;
            
           // this.gameObject.SetActive(false);
            //this.enabled = false;
        }

        public override void TakeOutOfInventory()
        {
            this.gameObject.SetActive(true);
            //this.enabled = true;
            isInHands = true;
            isInInventory = false;
        }
        #endregion
        /// <summary>
        /// The Laser Pistol's use
        /// </summary>
        override public void UseAsEquipment()
        {
            //cant use equipment if we are cooling down, hanging, or holding something else.
            if (shootingIsCoolingDown || HeldBy_Engine.isHanging || HeldBy_Inventory.isHoldingSomething ) return;
            StartCoroutine(ShootCoolingDown());
            Shoot();
        }

        #region Laser Pistol Methods
        void Shoot()
        {
            print("Pew Pew!");
        }
        #endregion




        /// <summary>
        /// 
        /// Interact action for the holdable object when near it and not holding it.
        /// pick up object and hold in hands ->inventory system script.
        /// is held and in hands, but not in inventory.
        /// freeze rotation.
        /// change to kinematic to prevent physics changes.
        /// 
        /// </summary>
        /// <param name="character"> CharacterEngine who is interacting with object</param>
        public override void GetPickedUp(CharacterEngine character)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;

            print("laser gun picked up");
            isHeld = true;
            isInHands = true;
            isInInventory = false; //cant be in inventory if just picked up.
            rb.freezeRotation = true; //put rotation back to normal
            rb.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            HeldBy = character.gameObject;
            HeldBy_Engine = character;
            HeldBy_Inventory = HeldBy.GetComponent<InventorySystem>();

            HeldBy_Inventory.PickUpItem(this.gameObject);
            //print("object should be picked up here by " + character.ToString());
        }
        //Interact action for the object when putting down object

        IEnumerator ShootCoolingDown()
        {
            shootingIsCoolingDown = true;
            yield return new WaitForSeconds(shootingCoolDownTime);
            shootingIsCoolingDown = false;
            yield break;
        }
    }
}

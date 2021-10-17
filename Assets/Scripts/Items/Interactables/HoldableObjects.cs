/* Code by: Matthew Sheehan */

using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
namespace CL03
{
    /// <summary>
    /// Interactable Objects which can be held.
    /// 
    /// Holdable Objects properties:
    /// isHeld state, - Can be held by character;
    /// heldby know who is holding it if any character;
    /// cooldown useage;
    /// isThrowable;
    /// isOnGround;
    /// (maybe?) isPushable; (maybe will be under heavycrate specific?)
    /// PushorPull state; (maybe will be under heavycrate specific
    /// </summary>
    public class HoldableObjects : InteractableObjects
    {

        //held properties
        [SerializeField]
        protected bool canBeHeld;
       
        public bool canBeStored = false; //default false

        [SerializeField]
        [ReadOnly]
        protected bool isHeld;

        [SerializeField]
        protected GameObject HeldBy; //who is holding this object?

        protected CharacterEngine HeldBy_Engine; //engine of object being held
        protected InventorySystem HeldBy_Inventory; //inventory of object being held.

        [Space]
        protected bool isThrowable;

        //inventory propertes
        [SerializeField]
        [ReadOnly]
        public bool isInHands { get; protected set; } = false; //default false

        
        [SerializeField]
        [ReadOnly]
        protected bool isInInventory;

        [Space]
        protected Rigidbody2D rb;
        protected Collider2D objectCollider;

        //protect set because we dont want outside changing this.

        protected bool _freezeRotation = false; //when held object must be still.

        public float gravity = 1f;
        public float objectMass;

        public bool isStaticCoolingDown = false;
        public float staticCoolDownTime = .1f;

        protected bool isOnGround; //where is this object?

        public bool isColliding;
        //public bool isPushable;
        //public bool inPushOrPullState;
        bool delayBegan = false;



        /// <summary>
        /// awake - called when loaded
        /// </summary>
        public virtual void Awake()
        {
            //realized this worked before initializeng this here but just in case...
            rb = GetComponent<Rigidbody2D>();
            isThrowable = true;
            //made this collider find generic
            objectCollider = GetComponent<Collider2D>();
            objectMass = rb.mass;
            canBeHeld = true;
        }
        
        /// <summary>
        /// physics check
        /// </summary>
        public virtual void FixedUpdate()
        {
            PhysicsCheck();
            IsHeldPositionCheck();
            
        }

        /// <summary>
        /// Holdable items can be held above head or below.
        /// </summary>
        public virtual void IsHeldPositionCheck()
        {
            if (isHeld && HeldBy != null && isInHands)
            {
                
                if (HeldBy_Inventory.isHoldingSomethingAbove) //held above so stay above
                    this.gameObject.transform.position = HeldBy_Inventory.holdPoint_Above.position;

                else if (!HeldBy_Inventory.isHoldingSomethingAbove) //held front so stay front
                    this.gameObject.transform.position = HeldBy_Inventory.holdPoint_Front.position;

                else
                {
                    rb.mass = objectMass;
                }
            }
        }

        /// <summary>
        /// can be stored
        /// </summary>
        public void CanBeStored_TRUE()
        {
            canBeStored = true;
        }

        /// <summary>
        /// cant be stored
        /// </summary>
        public void CanBeStored_FALSE()
        {
            canBeStored = false;
        }

        // Start is called before the first frame update
        /// <summary>
        /// prevent ability to be held
        /// </summary>
        public void CanBeHeld_Off()
        {
            canBeHeld = false;
        }

        /// <summary>
        /// allow ability to be held
        /// </summary>
        public void CanBeHeld_On()
        {
            canBeHeld = true;
        }



        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="character"></param>
        public override void Interact(CharacterEngine character)
        {
            
                //if object is not held
                if (!isHeld && character.isSelected)
                {
                    GetPickedUp(character);
                    //               isOnGround = false;  //do i need to know if object is on ground?
                }
                else if (isHeld && (HeldBy_Engine == character) && isThrowable && character.isSelected)
                {
                    // print(character.ToString()+"interacting with holdable object- object side");

                    Throw(HeldBy_Engine);
                    Debug.Log("Holdable default action is to be thrown");

                }
            
        }


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
        public virtual void GetPickedUp(CharacterEngine character)
        {
            isHeld = true;
            isInHands = true;
            isInInventory= false; //cant be in inventory if just picked up.
            rb.freezeRotation = true; //put rotation back to normal
            rb.transform.rotation = new Quaternion(0f,0f,0f,0f);
            rb.bodyType = RigidbodyType2D.Kinematic;
            HeldBy = character.gameObject;
            HeldBy_Engine = character;
            HeldBy_Inventory = HeldBy.GetComponent<InventorySystem>();

            HeldBy_Inventory.PickUpItem(this.gameObject);
            print("object should be picked up here by "+character.ToString());   
        }
        //Interact action for the object when putting down object


        public virtual void GetPutDown()
        {

            //put down object infront of character for now regardless of where
            isHeld = false;
            isInHands = false;
            HeldBy = null;
            Transform temp = HeldBy_Inventory.holdPoint_Front.transform;
            rb.freezeRotation = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 12;
            rb.mass = 100;

            StartCoroutine(DropObject(temp));

            Debug.Log("Object was put down - object side");
        }

        IEnumerator LaunchObject() {
            yield break; }

        /// <summary>
        /// go through all state clearing, give gravity to just put down.
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        IEnumerator DropObject(Transform temp)
        {
            rb.transform.position = temp.position;
            HeldBy = null;
            HeldBy_Engine = null;
            isInteractedWith = false;
            isHeld = false;
            isInHands = false;
            canBeHeld = true;

            //stop horizontal move to drop down.
            yield return new WaitForSeconds(.02f);
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;

            rb.gravityScale = gravity;
            rb.isKinematic = true;
            yield return new WaitForSeconds(.1f);
            //remove all constrains and let object fall as it might
            rb.constraints = RigidbodyConstraints2D.None;


            rb.mass = objectMass;

            rb.freezeRotation = false;
            rb.isKinematic = false;
        }

        #region Inventory States

        /// <summary>
        /// Change state to store object in inventory
        /// </summary>
        public void StoreInInventory()
        {
            isInHands = false;
            isInInventory = true;
            this.gameObject.SetActive(false);
            //this.enabled = false;
        }

        public void TakeOutOfInventory()
        {
            this.gameObject.SetActive(true);
            //this.enabled = true;
            isInHands = true;
            isInInventory = false;
        }
        #endregion


         /// <summary>
         ///Default Interact action when crate is being held by selected character.
         /// 
         /// </summary>
         /// <param name="engine"></param>
        public void Throw(CharacterEngine engine)
        {
            HeldBy_Inventory.ReleaseItem(this.gameObject);
            rb.bodyType = RigidbodyType2D.Dynamic;
            isHeld = false;
            isInHands = false;
            HeldBy = null;
            rb.constraints = RigidbodyConstraints2D.None;
            //Transform temp = HeldBy_Inventory.holdPoint_Front.transform;
            rb.freezeRotation = false;

            rb.velocity=new Vector2(7f*engine.direction,2f);
            //StartCoroutine(DropObject(temp));

            Debug.Log("Throw Object.");
         //   GetPutDown();
        }

        #region OnCollision Event
        private void OnCollisionStay2D(Collision2D collision)
        {
            //if object isnt held
            if (!isHeld)
            {
                //if is not cooling down
                if (!isStaticCoolingDown)
                {

                    //if the collision it is staying on is surface
                    if (collision.gameObject.CompareTag("Surface"))
                    {
                        isOnGround = true;
                        //MakeXStillState();
                        //add delay before making still
                        if (!delayBegan)
                            StartCoroutine(DelayStillState());
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //no need to process this if the person holding object colldes with object
            if (collision.gameObject == HeldBy) return;
            if (isHeld) { //HeldBy_Engine.isObjectColliding = true;
                          }
            isColliding = true;
            
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if(collision.gameObject == HeldBy) return;

            if (collision.gameObject.CompareTag("Surface"))
            {
                isOnGround = false;
                delayBegan = false;
            }
            if (isHeld) {// HeldBy_Engine.isObjectColliding = false;
                         }
            isColliding = false;


        }
        #endregion
        #region Move State commands
        /// <summary>
        /// Delayed still state for after the object is on ground.
        /// </summary>
        /// <returns></returns>
        public IEnumerator DelayStillState()
        {
            delayBegan = true;
            yield return new WaitForSeconds(.05f);
            if (!isHeld)
            {
                MakeXStillState();
             //   Debug.Log("DelayedStillState");
            }
            delayBegan = false;
            yield break;
        }
        /// <summary>
        /// Make still x state
        /// </summary>
        public void MakeXStillState()
        {
            //Debug.Log("Still X");
            //no matter what, x wont go still
            if (isHeld) { return; }
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
            // if ()
        }
        /// <summary>
        /// Make move x state
        /// </summary>
        public void MakeXMoveState()
        {
            //Debug.Log("Move X");
            rb.constraints -= RigidbodyConstraints2D.FreezePositionX;
        }

        /// <summary>
        /// from fixed update usually - Physics Check to manage states
        /// </summary>
        public virtual void PhysicsCheck()
        {
           // if (isOnGround && !isHeld)
            if (!isHeld) return;            
            if (rb.constraints == RigidbodyConstraints2D.FreezePositionX) MakeXMoveState();
        }


        public void EnterStaticState() => rb.bodyType = RigidbodyType2D.Static;
            
        public void EnterDynamicState() => rb.bodyType = RigidbodyType2D.Dynamic;


        IEnumerator StaticCoolDown()
        {
            yield return new WaitForSeconds(staticCoolDownTime);
        }

        #endregion
    }
}

        ////Interact action for when push pressed not holding item and it is not too heavy. this may be used with a seperate button or in a seperate object.
        //public virtual void PushAndPull()
        //{
        //    if (!isPushable) { return; }

        //    if (isOnGround)
        //    {
        //        inPushOrPullState = true;
        //        MakeXMoveState();
        //    }
        //}

        //IEnumerator PushPullCoolDown()
        //{
        //    yield return new WaitForSeconds(.2f);
        //}
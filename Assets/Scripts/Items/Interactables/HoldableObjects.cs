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

        [SerializeField]
        [ReadOnly]
        protected bool isHeld;
        [SerializeField]
        protected GameObject HeldBy;
        protected CharacterEngine HeldBy_Engine;
        protected InventorySystem HeldBy_Inventory;
        protected Rigidbody2D rb;
        protected Collider2D objectCollider;

        protected bool canBeHeld;
        protected bool justGotLetGo;
        //held properties
        protected bool _freezeRotation = false;

        protected bool isOnGround;

        protected bool isThrowable;

        public float gravity = 1f;
        public float objectMass;
        public bool isStaticCoolingDown = false;
        public float staticCoolDownTime = .1f;

        public bool isColliding;
        public bool isPushable;
        public bool inPushOrPullState;
        bool delayBegan = false;

        /// <summary>
        /// awake - called when loaded
        /// </summary>
        public virtual void Awake()
        {
            //realized this worked before initializeng this here but just in case...
            rb = GetComponent<Rigidbody2D>();
            //made this collider find generic
            objectCollider = GetComponent<Collider2D>();
            objectMass = rb.mass;
        }
        public virtual void Update()
        {
            //Being held and object is found / not null

          
        }
        /// <summary>
        /// physics check
        /// </summary>
        public virtual void FixedUpdate()
        {
            PhysicsCheck();
            IsHeldPositionCheck();
            
        }

        public virtual void IsHeldPositionCheck()
        {
            if (isHeld && HeldBy != null)
            {
                rb.mass = 1;

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
                } */

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
        /// Main 
        /// </summary>
        /// <param name="character"></param>
        public override void Interact(CharacterEngine character)
        {
            print(character.ToString()+"interacting with holdable object- object side");

            //Pick up object:
            //if object is not held
            if (!isHeld)
            {
                GetPickedUp(character);
                isOnGround = false;
            }

            if (isHeld && HeldBy.Equals(character.gameObject))
            {
                Debug.Log("Interacting with holdable object while being held.");
            }
        }

        //Interact action for the holdable object when near it and not holding it.
        /// <summary>
        /// pick up object and hold in inventorysystem script. assign is held states and
        /// send message to held by inventory to pick up object
        /// </summary>
        /// <param name="character"></param>
        public virtual void GetPickedUp(CharacterEngine character)
        {
            isHeld = true;
            rb.freezeRotation = true;
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
            HeldBy = null;
            Transform temp = HeldBy_Inventory.holdPoint_Front.transform;
            rb.freezeRotation = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 12;
            rb.mass = 100;

            StartCoroutine(DropObject(temp));

            Debug.Log("Object was put down - object side");
        }


        IEnumerator DropObject(Transform temp)
        {
            HeldBy = null;
            HeldBy_Engine = null;
            isInteractedWith = false;
            isHeld = false;
            canBeHeld = true;

            //stop horizontal move to drop down.
            yield return new WaitForSeconds(.02f);
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;

            rb.gravityScale = gravity;
            yield return new WaitForSeconds(.1f);
            rb.transform.position = temp.position;
            rb.isKinematic = true;
            //take out any velocity by hard positioning (not sure effective)
            //remove all constrains and let object fall as it might
            rb.constraints = RigidbodyConstraints2D.None;

            yield return new WaitForSeconds(.2f);
            rb.mass = objectMass;

                 rb.freezeRotation = false;
            rb.isKinematic = false;
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
            if (isHeld) { HeldBy_Engine.isObjectColliding = true; }
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
            if (isHeld) { HeldBy_Engine.isObjectColliding = false; }
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
            yield return new WaitForSeconds(.1f);
            if (!isHeld)
            {
                MakeXStillState();
                Debug.Log("DelayedStillState");
            }
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
            if (!isHeld) return;

            if (rb.constraints == RigidbodyConstraints2D.FreezePositionX) MakeXMoveState();
        }

        IEnumerator StaticCoolDown()
        {
            yield return new WaitForSeconds(staticCoolDownTime);
        }

        //Interact action when crate is being held by selected character.
        // SHOULD this be at the character engine level instead?
        public void Throw()
        {
            //HeldBy = null;
            //HeldBy_Engine = null;

            //add force
            Debug.Log("Throw Object.");
            GetPutDown();
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
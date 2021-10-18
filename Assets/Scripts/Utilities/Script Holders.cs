using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
	public class ScriptHolders : MonoBehaviour
	{







        #region INTERACTSYSTEM.CS as of 10/16/21
        /** INTERACT SYSTEM
         * 
         * /* Code by: Matthew Sheehan *

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
	{
		/// <summary>
		/// This system handles character interaction with interactables.
		/// </summary>
		public class InteractSystem : MonoBehaviour
		{
			InputHandler input;                     //The current inputs for the player
			CharacterEngine engine;             //engine on character to grab and set states
			InventorySystem inventory;

			bool interactCoolingDown = false;

			//default these badboys to be true so we can see where we reaching
			public bool drawDebugRaycasts = true;

			protected LayerMask grabables;
			//Interactable classified layers
			protected LayerMask interactablesLayer;

			[GUIColor(0.8f, 0.3f, 0.3f, .2f)]
			[PreviewField]
			public GameObject WithInArmsReach; // { get; set{ if (!isHoldingSomething) return WithInReach(); } }

			public float interactCoolDownTime = 0.5f;      //prevent spamming interaction It takes time to lift objects or interact with something

			Collider2D objectCollider;              //recognized object
			HoldableObjects objectScript;           //recognized object's script

			public Vector2 objectColliderSize;
			public bool isInteracting_Test;    //test bool

			[Title("Interact Distances")]
			public float reachDistance = .9f;       //The reach distance for object grabs
			public float grabHeightLow = .5f;          //Height of grab checks
			public float grabHeightHigh = 1f;          //Height of grab checks


			void Awake()
			{
				//look on game object and pair components
				input = GetComponentInParent<InputHandler>();
				engine = GetComponent<CharacterEngine>();
				inventory = GetComponent<InventorySystem>();


				//layers that can be interacted with:

				//grabables (meaning can be held)
				grabables = engine.crateLayer;
				grabables |= engine.itemsLayer;

				//items the interact key works with
				interactablesLayer = grabables;
				interactablesLayer |= engine.staticInteractablesLayer; //interactables in environment

			}

			private void Start()
			{
			}

			private void FixedUpdate()
			{
				//if items check detects items, then look for keyboard interaction 
				if (ItemsCheck())
				{
					if (engine.isSelected && !interactCoolingDown && input.interactPressed)
					{
						InteractCheck();
					}
				}
			}

			#region Interacting Behaviors, Item control

			/// <summary>
			/// Check to see if there is an item with in characters reach,
			/// and decide what the character will do with that
			///
			/// </summary><returns>bool if there is an item in sight.</returns>
			private bool ItemsCheck()
			{
				// if no object held then then look around for other items in sight. We're all grabby grabby here.
				if (inventory.objectBeingHeld == null)
				{
					//ensure flag set/ we realize nothing is being held. should be unnecessary
					inventory.isHoldingSomething = false;

					//look to see if there is something close by to interact with (note: using Raycast 2 to show different debug colors (cyan, magenta))
					RaycastHit2D ObjectCheckLow = Raycast2(new Vector2(engine.footOffset * engine.direction, grabHeightLow), new Vector2(engine.direction, 0f), reachDistance, interactablesLayer);
					RaycastHit2D ObjectCheckHigh = Raycast2(new Vector2(engine.footOffset * engine.direction, grabHeightHigh), new Vector2(engine.direction, 0f), reachDistance, interactablesLayer);

					//if something close by is found then can (only be one object in front of character)
					if (ObjectCheckHigh) //up high (priority)
					{                   //lets cache it until it no longer is in our vicinity
						WithInArmsReach = ObjectCheckHigh.collider.gameObject;
						//run a check to see if player has any input to interact with nearby object.
						//	InteractCheck();
						return true;
					}
					else if (ObjectCheckLow) //or down low
					{
						//lets cache it until it no longer is in our vicinity
						WithInArmsReach = ObjectCheckLow.collider.gameObject;
						//run a check to see if player has any input to interact with nearby object.
						//	InteractCheck();
						return true;
					}
					else //nothing found
					{
						WithInArmsReach = null;
						//if (WithInArmsReach) { }

					}

				}
				//default false
				return false;
			}



			/// <summary>
			/// Interaction:
			/// We check to see how we are going to interact with object.
			/// </summary>
			void InteractCheck()
			{
				//Begin cooldown
				StartCoroutine(InteractCoolingDown());

				if (inventory.isHoldingSomething && inventory.objectBeingHeld != null)
				{
					StartCoroutine(InteractCoolingDown());
					print("interacting with whats in hands, command side.");

					//interact with object in hands
					inventory.objectBeingHeld.GetComponent<HoldableObjects>().Interact(engine);
				}
				//Free hands to interact with something not being held.
				else if (WithInArmsReach != null && !inventory.isHoldingSomething)
				{
					StartCoroutine(InteractCoolingDown());
					WithInArmsReach.GetComponent<InteractableObjects>().Interact(engine);
				}

				//Hands full interact with whats in hands.
				else { print("there is an interaction going on but nothing is in hands or seen to interact with"); }
			}

			/// <summary>
			/// Interact cool down coroutine
			/// </summary>
			/// <returns>jumpCollingDown = false</returns>
			private IEnumerator InteractCoolingDown()
			{
				interactCoolingDown = true;
				yield return new WaitForSeconds(interactCoolDownTime);
				interactCoolingDown = false;
				Debug.Log("interact Cooldown passed");
				yield return null;
			}
			#endregion

			#region Handling Objects
			//This is commanded from an action of the interactable object.
			/// <summary>
			/// action: Picks up and holds 
			/// </summary>
			/// <param name="ItemInFrontToPickUp">sent as "this.gameObject" - the item to be picked up by character</param>
			public void PickUpAndHoldItem(GameObject ItemInFrontToPickUp)
			{
				if (inventory.objectBeingHeld != null)
				{
					Debug.Log("Engine Error PickUpHoldItem: Item being held: " + inventory.objectBeingHeld.ToString());
					return; //should not be allowed to finish 
				}
				inventory.objectBeingHeld = ItemInFrontToPickUp;
				objectCollider = inventory.objectBeingHeld.GetComponent<Collider2D>();
				objectScript = inventory.objectBeingHeld.GetComponent<HoldableObjects>();
				inventory.isHoldingSomething = true;
				WithInArmsReach = null;

				//objectCollider.enabled = true;
				//objectCollider.size = ObjectBeingHeld.GetComponent<BoxCollider2D>().size;
				//objectCollider.transform.position = ObjectBeingHeld.GetComponent<BoxCollider2D>().transform.position;
				//			objectCollider.transform.SetParent(holdPoint_Front);
			}


			//LOOK AT: this might be extra code, 
			void BreakOverHead(HoldableObjects holdable)
			{
				print("Break over Head method   : holdable.GetPutDown();");
				holdable.GetPutDown();
			}

			#endregion

			/// <summary>
			/// Cyan. hit: Magenta; - alternate Color Raycast method - 
			/// Specified hits with layer mask
			/// Returns RaycastHit2D
			/// and Creates visual line in editor scene if drawDebugRaycasts
			/// </summary>
			/// <param name="offset">offset to char, Vector2 </param>
			/// <param name="rayDirection">direction of ray, Vector2 </param>
			/// <param name="length">length of line, float</param>
			/// <param name="mask">the layer raycast is looking to hit on, LayerMask</param>
			/// <returns>RaycastHit2D</returns>
			public RaycastHit2D Raycast2(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
			{
				//Record the player's position
				Vector2 pos = transform.position;

				//Send out the desired raycast and record the result
				RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

				//If we want to show debug raycasts in the scene...
				if (drawDebugRaycasts)
				{
					//...determine the color based on if the raycast hit...
					Color color = hit ? Color.magenta : Color.cyan;
					//...and draw the ray in the scene view
					Debug.DrawRay(pos + offset, rayDirection * length, color);
				}

				//Return the results of the raycast
				return hit;
			}
		}
	}

			///// <summary>
			///// Interact cool down coroutine
			///// </summary>
			///// <returns>jumpCollingDown = false</returns>
			//IEnumerator InteractCoolingDown()
			//{
			//	interactCoolingDown = true;
			//	yield return new WaitForSeconds(interactCoolDownTime);
			//	interactCoolingDown = false;
			//	Debug.Log("interact Cooldown passed");
			//	yield return null;
			//}
		 *
		 * 
         * 
         * 
         */
        #endregion

        #region INVENTORY SYSTEM.CS as of 10/16/21
        /** INVENTORYSYSTEM.CS
		 * 
		 * // * Code by: Matthew Sheehan *

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
	{
		/// <summary>
		/// Handles Character Item relations when Item is in hands.
		/// </summary>
		public class InventorySystem : MonoBehaviour
		{
			CharacterEngine engine;
			BoxCollider2D bodyCollider;
			InputHandler input;


			[GUIColor(0.3f, 0.3f, 0.8f, .2f)]
			[PreviewField]
			public GameObject objectBeingHeld = null; // { get; set { if(isHoldingSomething)return } }

			[Title("Inventory Item", "if null then nothing is stored for this character.", TitleAlignments.Centered)]
			[GUIColor(0.3f, 0.8f, 0.8f, .2f)]
			[PreviewField]
			public GameObject inventoryItem = null;

			[Required]
			[ChildGameObjectsOnly]
			public Transform holdPoint_Front;  //the front location for generic object being held (maybe will need to change pivot point of object depending on sizes)
			[Required]
			[ChildGameObjectsOnly]
			public Transform holdPoint_Above;  //the above location for generic object being held


			//maybe we should store inventory items in an array. 
			public HoldableObjects[] Possessions;
			int possessionsCursor;

			GameObject tempObject; //container to hold temporary "storeable" object
			GameObject tempObject2; //container to hold temporary "storeable" object
			HoldableObjects tempObject_Script; // container to access holdable object script
			HoldableObjects storeableInHand;

			#region Interactable Object Properties

			public float dropItemCoolDownTime = 1f;
			public bool isDroppingItemCoolDown;

			public bool isHoldingSomething;
			public bool isHoldingSomethingAbove;
			public float changeItemCoolDownTime = 1.2f;
			public bool changeObjectCoolingDown;  //is object cooling down?


			protected Transform activeHoldPosition;

			public float interactCoolDownTime = 0.5f;      //prevent spamming interaction It takes time to lift objects or interact with something

			public BoxCollider2D objectCollider;              //recognized object
			public HoldableObjects objectScript;           //recognized object's script

			public Vector2 objectColliderSize;
			public bool isInteracting_Test;    //test bool

			#endregion


			void Awake()
			{
				changeObjectCoolingDown = false;
				// character doesnt hold something on initialization
				isHoldingSomething = false;
				isHoldingSomethingAbove = false;
				isDroppingItemCoolDown = false;
				Possessions = new HoldableObjects[2];
				possessionsCursor = 0;
			}

			void Start()
			{
				input = GetComponentInParent<InputHandler>();
				engine = GetComponent<CharacterEngine>();
				bodyCollider = GetComponent<BoxCollider2D>();
			}

			/// <summary>
			/// Message from holdable once picked up
			/// </summary>
			/// <param name="Item"></param>
			public void PickUpItem(GameObject Item)
			{

				//Possessions[possesionsCursor] = Item;
				isHoldingSomething = true;

				objectBeingHeld = Item;
				objectScript = Item.GetComponent<HoldableObjects>();
				objectCollider = Item.GetComponent<BoxCollider2D>();

			}

			public void DropItem(GameObject Item)
			{//null item sent to drop item should be an bug
				if (Item == null) { print("null item drop sent to Inventory.DropItem(GameObject Item)"); return; }
				if (isDroppingItemCoolDown) return;
				//Possessions[possesionsCursor] = Null;
				StartCoroutine(DroppingItemCoolDown());
				isHoldingSomethingAbove = false;
				isHoldingSomething = false;
				objectBeingHeld = null;
				objectCollider = null;
				objectScript.GetPutDown();
				objectScript = null;
			}


			private void FixedUpdate()
			{
				//HandledObjectsCheck();
				//ROTATE OBJECTS IN POSSESSION
				//if selected and object held. press up or down the object being held changes position
				if (objectBeingHeld != null)
				{
					PositionOfItemInHands();
					//engine.ChangeCollider(objectCollider.size,false); }
				}
				HandledObjectsCheck();
				//should this be after held? instead of pressed
			}

			private void PositionOfItemInHands()
			{
				//change position of object in hand if commanded and send change message to engine
				if (input.vertical > .2f && !engine.isHeadBlocked) { isHoldingSomethingAbove = true; }
				//engine.ChangeCollider(objectCollider.size,true); }
				if (input.vertical < -.2f) { isHoldingSomethingAbove = false; }
			}

			void HandledObjectsCheck()
			{

				//DROP OBJECT
				if (input.dropObjectPressed)
				{
					if (objectBeingHeld)
					{
						DropItem(objectBeingHeld);
					}
				}
				//SWITCH TO INVENTORY
				if (input.changeObjectPressed && (inventoryItem != null || objectBeingHeld != null) && !changeObjectCoolingDown)
				{

					Debug.Log("Change object button pressed and change object cooling down is false. Inventory Swap()");

					InventorySwap();
				}
			}
			
			/// <summary>
			/// as long as an item is held a change item method will run when called.
			/// </summary>
			private void InventorySwap()
			{
				//if held is null but inventory has an item, or object held exists then a swap can happen. this should cover all cases
				if ((objectBeingHeld == null && inventoryItem != null) || objectScript.canBeStored)
				{
					SwapItems();
				}
				//if there is an object being held and an inventory item we switch them
				//if (objectBeingHeld.TryGetComponent<StoreableObjects>(out tempObject))
				//{
				//print("This tempObject try get Storeable");
				//}

				//There is no inventory item so we put whatever is in our hands there provided it can Be Stored.
				else { print("Didn't Swap"); }
			}

			void SwapItems()
			{
				if (inventoryItem != null) tempObject = inventoryItem;
				else tempObject = null;
				//tell objects to store state
				objectScript = objectBeingHeld.GetComponent<HoldableObjects>();
				if (objectScript != null) objectScript.StoreInInventory();
				//put inventory item in hands
				if (objectBeingHeld != null) inventoryItem = objectBeingHeld;
				else inventoryItem = null;
				possessionsCursor++;
				if (possessionsCursor == 2) possessionsCursor = 0;
				objectBeingHeld = tempObject;
				//get script if not its null
				if (objectBeingHeld != null)
					objectScript = objectBeingHeld.GetComponent<HoldableObjects>();
				//tell object to take out store state
				if (objectScript != null) objectScript.TakeOutOfInventory(); ;
				inventoryItem = tempObject;
				print("Swap Complete");

			}

			public IEnumerator ChangingItemCoolDown()
			{
				changeObjectCoolingDown = true;
				yield return new WaitForSeconds(changeItemCoolDownTime);
				changeObjectCoolingDown = false;
				Debug.Log("Can Change Items again");
				yield break;
			}



			/// <summary>
			/// Dropping Items cant be spammed
			/// </summary>
			/// <returns></returns>
			public IEnumerator DroppingItemCoolDown()
			{
				isDroppingItemCoolDown = true;
				yield return new WaitForSeconds(dropItemCoolDownTime);
				isDroppingItemCoolDown = false;
				Debug.Log("Can Drop Items again");
				yield break;
			}

		}
	}
		///// <summary>
		///// action: Drops Item that is active in hands
		///// </summary>
		//public void a_DropItem(GameObject _ObjectBeingHeld)
		//{
		//	//inventory.
		//	//Transform tempTrans = objectCollider.transform;
		//	//	objectCollider.transform.SetParent(null);
		//	//objectCollider.transform.position = tempTrans.position;
		//	objectScript = _ObjectBeingHeld.GetComponent<HoldableObjects>();
		//	print("objectScript.GetPutDown();");
		//	// objectScript.GetPutDown();
		//	StartCoroutine(DroppingItemCoolDown());
		//	Debug.Log("Object Dropped - Engine Side");
		//}
		 */
        #endregion

        #region HOLDABLEOBJECTS.CS as of 10/16/21
        /**
		 * /* Code by: Matthew Sheehan *

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

				//made this collider find generic
				objectCollider = GetComponent<Collider2D>();
				objectMass = rb.mass;


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
			/// Main 
			/// Pick up object if interact 
			/// </summary>
			/// <param name="character"></param>
			public override void Interact(CharacterEngine character)
			{
				// print(character.ToString()+"interacting with holdable object- object side");

				//if object is not held
				if (!isHeld)
				{
					GetPickedUp(character);
					//               isOnGround = false;  //do i need to know if object is on ground?
				}

				if (isHeld && HeldBy.Equals(character.gameObject))
				{
					Throw();
					Debug.Log("Interacting with holdable object while being held.");
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
				isInInventory = false; //cant be in inventory if just picked up.
				rb.freezeRotation = true; //
				rb.bodyType = RigidbodyType2D.Kinematic;
				HeldBy = character.gameObject;
				HeldBy_Engine = character;
				HeldBy_Inventory = HeldBy.GetComponent<InventorySystem>();

				HeldBy_Inventory.PickUpItem(this.gameObject);
				print("object should be picked up here by " + character.ToString());



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
				this.enabled = false;
			}



			public void TakeOutOfInventory()
			{
				this.gameObject.SetActive(true);
				this.enabled = true;
				isInHands = true;
				isInInventory = false;
			}
			#endregion


			//Interact action when crate is being held by selected character.
			// SHOULD this be at the character engine level instead?
			public void Throw()
			{
				Transform temp = HeldBy_Inventory.holdPoint_Front.transform;


				StartCoroutine(DropObject(temp));
				rb.AddForce(new Vector2(30, 1));

				Debug.Log("Throw Object.");
				GetPutDown();
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
				if (collision.gameObject == HeldBy) return;

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
				yield return new WaitForSeconds(.05f);
				if (!isHeld)
				{
					MakeXStillState();
					Debug.Log("DelayedStillState");
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
				if (!isHeld) return;

				if (rb.constraints == RigidbodyConstraints2D.FreezePositionX) MakeXMoveState();
			}

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
		 */
        #endregion

        #region CRATEOBJECT.CS as of 10/16/21
        /**
		/* Code by: Matthew Sheehan *

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
		* /
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
*/
        #endregion

        #region INTERACTABLEOBJECTS.cs as of 10/16/21
        /**
		/* Code by: Matthew Sheehan *

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
			public bool isInteractableState { get; protected set; }
			public bool isInteractedWith { get; protected set; }


			//to be overwritten
			public virtual void Interact(CharacterEngine character)
			{
				//this script wont show unless using this class as a test.
				Debug.Log("Object recognizes - interact test");
				//            character.isInteracting_Test = true;
			}
		}
	}
*/
        #endregion


    }
    #region Interfaces
	/// <summary>
    /// For Holdable Object which can be used in inventory.
    /// </summary>
    public interface IEquippedUseable
	{
		public void UseAsEquipment();
	}
    #endregion
}


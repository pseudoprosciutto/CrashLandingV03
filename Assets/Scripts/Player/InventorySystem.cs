/* Code by: Matthew Sheehan */

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
		public GameObject objectBeingHeld=null; // { get; set { if(isHoldingSomething)return } }

		[Title("Inventory Item", "if null then nothing is stored for this character.",TitleAlignments.Centered)]
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
			inventoryItem = null;
        }

		void Start()
        {
			input = GetComponentInParent<InputHandler>();
			engine = GetComponent<CharacterEngine>();
			bodyCollider = GetComponent<BoxCollider2D>();
		}


		/// <summary>
        /// F Update cycle:
        ///
        /// Object in hands?
        ///  -what position? 
        ///
        /// </summary>
		private void FixedUpdate()
        {
			if(!engine.isSelected)
			//ROTATE OBJECTS IN POSSESSION
			//if selected and object held. press up or down the object being held changes position
				return;
			if (objectBeingHeld != null)
			{
				PositionOfItemInHands();
					//engine.ChangeCollider(objectCollider.size,false); }
			}
			
			HandleObjectsInput();
		}

        #region Item Actions
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
		/// <summary>
        /// This is a general disengagement of item.
        /// </summary>
        /// <param name="Item"></param>
		public void ReleaseItem(GameObject Item)
        {
			if (Item == null) { print("null item drop sent to Inventory.DropItem(GameObject Item)"); return; }
			if (isDroppingItemCoolDown) return;
			StartCoroutine(DroppingItemCoolDown());
			isHoldingSomethingAbove = false;
			isHoldingSomething = false;
			objectBeingHeld = null;
			objectCollider = null;
			objectScript = null;
		}
        #endregion
        #region Handling
		/// <summary>
        /// Look for input to change position of item in hands.
        /// </summary>
        private void PositionOfItemInHands()
        {
			//change position of object in hand if commanded and send change message to engine
			if (input.vertical > .2f && !engine.isHeadBlocked && !engine.ObjectInFrontCornerCheck() && !(engine.hitOverHeadLeft || engine.hitOverHeadRight) && !engine.ObjectChangeHitFrontCheck()) { isHoldingSomethingAbove = true; }
			//engine.ChangeCollider(objectCollider.size,true); }
			if (input.vertical < -.2f && !engine.ObjectInFrontCornerCheck() && !(engine.hitOverHeadLeft || engine.hitOverHeadRight) && !engine.ObjectChangeHitFrontCheck()) { isHoldingSomethingAbove = false; }
		}

		/// <summary>
		/// check for input to drop or change object.
		/// </summary>
		void HandleObjectsInput()
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
				StartCoroutine(ChangingItemCoolDown());
				Debug.Log("Change object button pressed and change object cooling down is false. Inventory Swap()");
				InventorySwap();
			}

			//USE EQUIPPED ITEM
			//needs to add equipment use held also.
			if(input.equipmentUsePressed && inventoryItem != null)
            {
				inventoryItem.GetComponent<HoldableObjects>().UseAsEquipment();
            }
			//else if held.
		}
		#endregion
		#region Inventory Management
		/// <summary>
		/// as long as an item is held a change item method will run when called.
		/// </summary>
		private void InventorySwap()
		{
			//if an object exists in hand or in inventory a swap may happen
			if ((objectBeingHeld != null) || (inventoryItem != null))
			{
				tempObject = objectBeingHeld;//this may be null or not.
											 //we know we can process the inventory item out because it already passed muster to get in. 
											 // temp object to store away is not null checked to see if we can access its get storeable bool.
				if (tempObject != null)
				{
					//if (it cant be stored)
					if (objectBeingHeld.GetComponent<HoldableObjects>().canBeStored == false)
					{
						//we put down object clear holding states and tell the object its no longer held.
						DropItem(objectBeingHeld);
						//put temp object back to null
						tempObject = null;
					}
					else
					{
						//else	it can be stored, we deactivate its physical self.
						objectScript.StoreInInventory();
						//						objectBeingHeld.SetActive(false);
					}
					//either way the space will be pushed between variables, all that matters is
					//that it is physically activated and deactivated after the variables have the object
				}

				//inventory item into hands
				objectBeingHeld = inventoryItem;

				inventoryItem = tempObject; //doesnt matter if tempobject is null.

				//if there is an item in our hand
				if (objectBeingHeld != null)
				{
					objectScript = objectBeingHeld.GetComponent<HoldableObjects>();
					objectScript.TakeOutOfInventory();
					objectCollider = objectBeingHeld.GetComponent<BoxCollider2D>();
					//					objectBeingHeld.SetActive(true);
				}
				else //we ensure we dont recognize an item in our hands.
				{
					isHoldingSomethingAbove = false;
					isHoldingSomething = false;
					objectCollider = null;
					objectScript = null;
				}

				//if this new object being held is a real object and not null
				//make all the overwriting variable object scripts etc for this new item
				//we need physically activate, move and, assign it to the position of hand in front.
				//else if null
				//change all the variables to know what is in hand is null and doesnt exist,
				//and are able to pick up objects again because hands are free.

			}
			// else an object didnt exist in hand or inventory and yet got past checking clause
			else { print("Didn't Swap , this should not show."); }
		}
		/// <summary>
        /// regardless of what the inventory item is, it is no longer. now it is in the hands of
        /// </summary>
		void BringInventoryItemToHands()
		{
		}

		void BringItemInHandsToInventory(GameObject ItemInHands)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// NYI   Force Remove Item inventory
		/// </summary>
		void ForceRemoveItemInventory()
		{
			throw new NotImplementedException(); 
		}

		/// <summary>
        ///  NYI  Force Remove Item Hands
        /// </summary>
		void ForceRemoveItemHands()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// this is ugly and will change next.
		/// </summary>
		 void UseEquipment()
		{

		}
			
        

        #endregion
        #region CoolDown IEnumerators
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
        #endregion
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
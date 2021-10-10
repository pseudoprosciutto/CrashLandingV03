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
		HoldableObjects[] possessions;

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
			
			isHoldingSomething = true;
			objectBeingHeld = Item;			
			objectScript = Item.GetComponent<HoldableObjects>();
			objectCollider = Item.GetComponent<BoxCollider2D>();

		}

		public void DropItem(GameObject Item)
		{//null item sent to drop item should be an bug
			if (Item == null) { print("null item drop sent to Inventory.DropItem(GameObject Item)"); return; }
			if (isDroppingItemCoolDown) return;
			
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
			if (input.changeObjectPressed  && (inventoryItem != null || objectBeingHeld != null)  && !changeObjectCoolingDown)
				{
				
					Debug.Log("Change object button pressed and change object cooling down is false. Inventory Swap()");

					InventorySwap();
				}
			}
/*******/
		/// <summary>
        /// as long as an item is held a change item method will run when called.
        /// </summary>
        private void InventorySwap()
        {
			//if held is null but inventory has an item, or object held exists then a swap can happen. this should cover all cases
				if((objectBeingHeld == null && inventoryItem !=null) || objectScript.canBeStored)
				{
					SwapItems(objectBeingHeld);
				}
				//if there is an object being held and an inventory item we switch them
				//if (objectBeingHeld.TryGetComponent<StoreableObjects>(out tempObject))
				//{
					//print("This tempObject try get Storeable");
				//}

                        //There is no inventory item so we put whatever is in our hands there provided it can Be Stored.
            else{ print("Didn't Swap"); }
        }

        void SwapItems(GameObject storeableObject)
        {
			if (inventoryItem != null) tempObject = inventoryItem;
			else tempObject = null;
			//tell objects to store state
			if (objectScript != null) objectScript.StoreInInventory();
			//put inventory item in hands
			if (storeableObject != null) inventoryItem = storeableObject;
			else inventoryItem = null;

			objectBeingHeld = tempObject;
			//get script if not its null
			objectBeingHeld.TryGetComponent<HoldableObjects>(out objectScript);
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
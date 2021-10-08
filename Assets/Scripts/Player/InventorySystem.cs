/* Code by: Matthew Sheehan */

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
        GameObject objectStored; //container for object stored in inventory
        GameObject objectInHand; //container for object picked up and held in hand
        GameObject tempObject; //container to hold temporary object
		BoxCollider2D bodyCollider;
		InputHandler input;

		#region Interactable Object Properties
		public bool inSwitchItemProcess = false;
		public float switchItemCoolDownTime = 1.7f;


		public bool isHoldingSomething;
		public bool isHoldingSomethingAbove;

		//   [Title("Inventory Item", "if null then nothing is stored for this character.",TitleAlignments.Centered)]
		//  [BoxGroup()]
		[GUIColor(0.3f, 0.8f, 0.8f, .2f)]
		[PreviewField]
		public GameObject InventoryItem;

		[GUIColor(0.3f, 0.3f, 0.8f, .2f)]
		[PreviewField]
		public GameObject ObjectBeingHeld; // { get; set { if(isHoldingSomething)return } }
		[Required]
		[ChildGameObjectsOnly]
		public Transform holdPoint_Front;  //the front location for generic object being held (maybe will need to change pivot point of object depending on sizes)
		[Required]
		[ChildGameObjectsOnly]
		public Transform holdPoint_Above;  //the above location for generic object being held


		protected Transform activeHoldPosition;



		public float interactCoolDownTime = 0.5f;      //prevent spamming interaction It takes time to lift objects or interact with something

		Collider2D objectCollider;              //recognized object
		HoldableObjects objectScript;           //recognized object's script

		public Vector2 objectColliderSize;
		public bool isInteracting_Test;    //test bool
		public bool changeObjectCoolingDown;  //is object cooling down?
		public float changeObjectCoolDownTime = 1.2f;
        #endregion


        void Awake()
        {
            changeObjectCoolingDown = false;
			// character doesnt hold something on initialization
			isHoldingSomething = false;
			isHoldingSomethingAbove = false;
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
			print("inventory registers being picked up");
			objectInHand = Item;
			objectScript = Item.GetComponent<HoldableObjects>();
			objectCollider = Item.GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
			//HandledObjectsCheck();
			//ROTATE OBJECTS IN POSSESSION
			//if selected and object held. press up or down the object being held changes position
			if (ObjectBeingHeld != null)
			{
				//change position of object in hand if commanded
				if (input.vertical > .2f) { isHoldingSomethingAbove = true; }
				if (input.vertical < -.2f) { isHoldingSomethingAbove = false; }
			}

			//should this be after held? instead of pressed
			if (input.changeObjectPressed && !changeObjectCoolingDown)
			{
				Debug.Log("Change object button pressed and change object cooling down is false.");
			}
		}
    }
}

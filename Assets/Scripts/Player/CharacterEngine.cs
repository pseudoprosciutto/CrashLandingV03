/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// CrashLanding v03 
/// </summary>
namespace CL03
{
    /// <summary>
    /// Character script, Handles individual character's control,
    /// states, raycasts, actions and physics.
    /// </summary>
    public class CharacterEngine : MonoBehaviour
    {
        #region FIELDS
        #region Component References: Rigidbody, BoxCollider, Input Handler
        BoxCollider2D bodyCollider;             //The collider component
		Rigidbody2D rigidBody;                  //The rigidbody component
		InputHandler input;                     //The current inputs for the player
		#endregion

		#region Character State bools
		[BoxGroup("Character State")] public bool isSelected;
		[Space]
		[BoxGroup("Character State")] public bool isOnGround;                 //Is the player on the ground?
		[BoxGroup("Character State")] public bool isOnPlatform;
		[BoxGroup("Character State")] public bool isJumping;                  //Is player jumping?
		[BoxGroup("Character State")] public bool isCrouching;                //Is player crouching?
		[BoxGroup("Character State")] public bool isHeadBlocked;
		[BoxGroup("Character State")] public bool isHanging;                  //Is player hanging?
		[BoxGroup("Character State")] public bool isHoldingSomething;
		[BoxGroup("Character State")] public bool isHoldingSomethingAbove;
		[BoxGroup("Character State")] public bool hitOverHeadLeft;
		[BoxGroup("Character State")] public bool hitOverHeadRight;
		#endregion

		[Space]
		#region hanging properties
		//can hang
		public bool canHang = true; // can the character even hang dawg and kick it with the other hanging homies?
		public float cantHangCoolDownTime = 1.5f;
		#endregion
		[Space]
		#region Move and Jump Modifiers 

		[FoldoutGroup("Movement Properties", expanded: false)]
		public float speed = 4f;                //Player speed
		[FoldoutGroup("Movement Properties")]
		public float crouchSpeedDivisor = 3f;   //Speed reduction when crouching
		[FoldoutGroup("Movement Properties")]
		public float coyoteDuration = .05f;     //How long the player can jump after falling
		[FoldoutGroup("Movement Properties")]
		public float maxFallSpeed = -25f;       //Max speed player can fall

		[FoldoutGroup("Jump Properties", expanded: false)]
		public float jumpForce = 27f;           //Initial force of jump
		[FoldoutGroup("Jump Properties")]
		public float jumpCoolDownTime = 0.6f;   //To prevent spammable jumping
		[FoldoutGroup("Jump Properties")]
		public float crouchJumpBoost = 2.5f;    //Jump boost when crouching
		[FoldoutGroup("Jump Properties")]
		public float hangingJumpForce = 31f;    //Force of wall hanging jump
		[FoldoutGroup("Jump Properties")]
		public float jumpHoldForce = 1.9f;      //Incremental force when jump is held
		[FoldoutGroup("Jump Properties")]
		public float jumpHoldDuration = .1f;    //How long the jump key can be held
		#endregion

		#region Layer Masks
		[Header("Walkable Object Layers")]

		//Layer of the ground
		protected LayerMask walkables;
		//Layer of objects to grab
		protected LayerMask grabables;
		//Interactable classified layers
		protected LayerMask interactablesLayer;
		public LayerMask groundLayer;           //Layer of the ground
		public LayerMask itemsLayer;
		public LayerMask staticInteractablesLayer;
		public LayerMask crateLayer;
		public LayerMask walkableObject;
		#endregion

		#region Interactable Object Properties
		public bool inSwitchItemProcess = false;
		public float switchItemCoolDownTime = 1.7f;
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

		public bool canInteract;                //Can player interact with an object its facing?
		[GUIColor(0.8f, 0.3f, 0.3f, .2f)]
		[PreviewField]
		public GameObject WithInArmsReach; // { get; set{ if (!isHoldingSomething) return WithInReach(); } }

		public float interactCoolDownTime = 0.5f;      //prevent spamming interaction It takes time to lift objects or interact with something

		Collider2D objectCollider;              //recognized object
		HoldableObjects objectScript;			//recognized object's script

		public Vector2 objectColliderSize;
		public bool isInteracting_Test;    //test bool
		public bool changeObjectCoolingDown;  //is object cooling down?
		public float changeObjectCoolDownTime = 1.2f;
		#endregion


		#region Debug RayCasting Properties

		[Header("Debug and Raycasting Values:")]
		public bool drawDebugRaycasts = true;   //Should the environment checks be visualized
		[FoldoutGroup("Environment Check Properties", expanded: false)]
		public float headOffset = .35f;
		[FoldoutGroup("Environment Check Properties")]
		public float footOffset = .25f;          //X Offset of feet raycast
		[FoldoutGroup("Environment Check Properties")]
		public float eyeHeight = 1.5f;          //Height of wall checks
		[FoldoutGroup("Environment Check Properties")]
		public float grabHeightLow = .5f;          //Height of grab checks
		[FoldoutGroup("Environment Check Properties")]
		public float grabHeightHigh = 1f;          //Height of grab checks
		[FoldoutGroup("Environment Check Properties")]
		public float reachOffset = .7f;         //X offset for wall grabbing
		[FoldoutGroup("Environment Check Properties")]
		public float headClearance = .5f;       //Space needed above the player's head
		[FoldoutGroup("Environment Check Properties")]
		public float breakOverHeadDistance = .1f;
		[FoldoutGroup("Environment Check Properties")]
		public float groundDistance = .1f;      //Distance player is considered to be on the ground
		[FoldoutGroup("Environment Check Properties")]
		public float grabDistance = .4f;        //The reach distance for wall grabs
		[FoldoutGroup("Environment Check Properties")]
		public float reachDistance = .9f;       //The reach distance for object grabs
		[FoldoutGroup("Environment Check Properties")]
		public float hangingDistanceFromLedge = 0.1f; //added space between ledge and sprite
		[FoldoutGroup("Environment Check Properties")]
		public float headBounce = .2f;
		#endregion


		#region Private Properties
		bool jumpCoolingDown = false;
		bool interactCoolingDown = false;
		float jumpTime;                         //Variable to hold jump duration
		float coyoteTime;                       //Variable to hold coyote duration
		const float smallAmount = .05f;         //A small amount used for hanging position
		float playerHeight;                     //Height of the player
		float originalXScale;                   //Original scale on X axis
		int direction = 1;                      //Direction player is facing


		Vector2 colliderStandSize;              //Size of the standing collider

		Vector2 colliderStandOffset;            //Offset of the standing collider
		Vector2 colliderCrouchSize;             //Size of the crouching collider
		Vector2 colliderCrouchOffset;           //Offset of the crouching collider
		#endregion
		#endregion

		#region METHODS

		#region Initialization; Awake() and Start()
		private void Awake()
		{
			//*tests:*
			isInteracting_Test = false;
			//********

			isHoldingSomething = false;
			isHoldingSomethingAbove = false;

			//place character in correct z plane
			transform.position = new Vector3(transform.position.x, transform.position.y, 2f);
	
			walkables = groundLayer;
			walkables |= walkableObject;
			walkables |= crateLayer;

			//objectCollider.enabled = false;

			grabables = crateLayer;
			grabables |= itemsLayer;
			interactablesLayer = grabables;
			interactablesLayer |= staticInteractablesLayer;
	
			changeObjectCoolingDown = false;
			canHang = true;
		}

		void Start()
		{
			//Get a reference to the required components
			input = GetComponentInParent<InputHandler>();
			rigidBody = GetComponent<Rigidbody2D>();
			bodyCollider = GetComponent<BoxCollider2D>();

			//assuming we are deselected to start
			//EnterStaticState();

			//Record the original x scale of the player
			originalXScale = transform.localScale.x;

			//Record the player's height from the collider
			playerHeight = bodyCollider.size.y;

			//Record initial collider size and offset
			colliderStandSize = bodyCollider.size;
			colliderStandOffset = bodyCollider.offset;

			//Calculate crouching collider size and offset
			colliderCrouchSize = new Vector2(bodyCollider.size.x, bodyCollider.size.y / 2f);
			colliderCrouchOffset = new Vector2(bodyCollider.offset.x, bodyCollider.offset.y / 2f);
		}
		#endregion

		#region Character Update Loops; Fixed update
		/// <summary>
		/// Fixed update Execution Order: 
		/// -objectBeingHeld? 
		///     {no:} look for and identify interactables in line of sight 
		///     {yes:} keep its position as if held || 
		/// -PhysicsCheck() - process and check engine states. 
		/// -isSelected? 
		///    {yes:} GroundMovement(); MidAirMovement() || 
		///    {no:} 
		/// -end 
		/// </summary>
		void FixedUpdate()
		{

			/** no need to hold items yet
			// any object in hands?
			//HoldingItemsCheck(); 
			*/
			//Check the environment to determine status
			PhysicsCheck();

			//if player is selected and once physics have been checked then we can continue deciding how to player moves knowing state and environment
			if (isSelected)
			{
				HandledObjectsCheck();
				//ROTATE OBJECTS IN POSSESSION
				//if selected and object held. press up or down the object being held changes position
				if (ObjectBeingHeld != null)
				{
					if (input.vertical > .2f) { isHoldingSomethingAbove = true; }
					if (input.vertical < -.2f) { isHoldingSomethingAbove = false; }
				}

				//should this be after held? instead of pressed
				if (input.changeObjectPressed && !changeObjectCoolingDown)
				{
					Debug.Log("Change object button pressed and change object cooling down is false.");
				}
				//Process ground and air movements
				GroundMovement();
				MidAirMovement();
			}
			//else we arent selected and we have landed on ground so we need to be static
			else if (!isSelected && isOnGround) EnterStaticState(); 
		}
        #endregion

        #region Selection for Player Control
        // OnCharacterChange scripts arranged as public messages to engine
        /// <summary>
        /// the initial state changes and functions launch on character change. 
        /// </summary>
        public void OnCharacterChange_Start()
		{
			ForceUnFreezeConstraints();
			Debug.Log("Character was just selected"); //make sure this doesnt double
		}
		/// <summary>
		/// state changes and functions for switching selection from current character.
		/// </summary>
		public void OnCharacterChange_End()
		{
			//Scheduled delayed actions.
			//Last possible functions relating with switching away from character
			StartCoroutine(LateDeselect());
			Debug.Log("Character was just changed from"); //make sure this doesnt double
		}

		/// <summary>
		/// forced freeze of rigidbody to use when deselecting character
		/// </summary>
		public void ForceFreezeHorizontal() => rigidBody.constraints |= RigidbodyConstraints2D.FreezePositionX;

		/// <summary>
		/// forced unfreeze of rigidbody to grant character movement again by just freezing rotation
		/// </summary>
		public void ForceUnFreezeConstraints() => rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

		/// <summary>
		/// Delayed actions for after the character is deselected.
		/// </summary>
		/// <returns></returns>
		public IEnumerator LateDeselect()
		{
			yield return new WaitForSeconds(.8f);
			ForceFreezeHorizontal();
			//			Debug.Log("Late Deselect test");
			yield break;
		}
		#endregion

		#region Physics Check
		/// <summary>
		/// Check Environment and update status. 
		/// </summary>
		void PhysicsCheck()
		{
			//set location of selected character so that they wont get stuck behind others.
			if (isSelected) { bringFront(); } else { sendBack(); };
			//Start by assuming the character isn't on the ground and the head isn't blocked
			CharacterStandingOnSurfaceCheck();
			CharacterHeadCheck();
			

			if (!isHoldingSomething) //ObjectBeingHeld)
			{//attempt a wallgrab because hands are empty
				WallGrabCheck();
			}
		}

		/// <summary>
        /// Ground Check for character. Casts rays for left and right feet and gives and updated state of character
        /// </summary>
        void CharacterStandingOnSurfaceCheck()
        {
			//assume not on ground
			isOnPlatform = false;
			isOnGround = false;
			//Cast rays for the left and right foot and parse
			RaycastHit2D leftFootCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, groundDistance);
			RaycastHit2D rightFootCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, groundDistance);
			//If either ray hit the ground, the player is on the ground
			if (leftFootCheck || rightFootCheck)
				isOnGround = true;
		}

		/// <summary>
        /// Head check. (aka Lets not put our head through things.)
        /// sends out raycasthits and lets you know how large head trauma bill will be.
        /// </summary>
        void CharacterHeadCheck() {
			isHeadBlocked = false;
			hitOverHeadLeft = false;
			hitOverHeadRight = false;

			//HEAD CHECK
			RaycastHit2D fullHeadCheck = Raycast(new Vector2(Math.Abs(direction) - 1.5f, playerHeight), Vector2.right, 1f, grabables);
			if (fullHeadCheck) Debug.Log("object hit head test");

			//this loc checks are to eventually push object a certain way to fall off head and not stick.
			RaycastHit2D leftHeadCheck = Raycast(new Vector2(-headOffset, bodyCollider.size.y), Vector2.up, breakOverHeadDistance);
			if (leftHeadCheck) //.collider.CompareTag("Holdable") )
				hitOverHeadLeft = true;
			// BreakOverHead(leftHeadCheck.collider.GetComponent<HoldableObjects>());

			RaycastHit2D rightHeadCheck = Raycast(new Vector2(headOffset, bodyCollider.size.y), Vector2.up, breakOverHeadDistance);
			if (rightHeadCheck) //.collider.CompareTag("Holdable"))
				hitOverHeadRight = true;
			//BreakOverHead(rightHeadCheck.collider.GetComponent<HoldableObjects>());


			//Cast the ray to check above the player's head
			RaycastHit2D headCheck = Raycast(new Vector2(0f, bodyCollider.size.y), Vector2.up, headClearance);

			//If that ray hits, the player's head is blocked
			if (headCheck)
			{
				isHeadBlocked = true;
				//something bonks the head, it will no longer get stuck there because we are flat headed and smooth brained.
				//if (!headCheck.collider.CompareTag("Environment") && !headCheck.collider.CompareTag("Surface") && !headCheck.collider.Equals(ObjectBeingHeld))
				if (!headCheck.collider.CompareTag("Surface") && !headCheck.collider.Equals(ObjectBeingHeld))
				{
					//slight backwards force added to prevent objects from staying on head. things should just roll off
					Rigidbody2D rb = headCheck.collider.GetComponent<Rigidbody2D>();
					rb.AddForceAtPosition(Vector2.up, new Vector2(headBounce, bodyCollider.size.y), ForceMode2D.Force);
				}
			}
		}

		void HandledObjectsCheck()
		{

			//DROP OBJECT
			if (input.dropObjectPressed)
			{
				if (ObjectBeingHeld)
				{
					DropItem(ObjectBeingHeld);
				}
			}
			//SWITCH TO INVENTORY
			if (input.changeObjectPressed)
			{
				if (!inSwitchItemProcess)
					ChangeItem();
			}

		}
		#endregion

		#region Interacting Behaviors, Item control

		/// <summary>
		/// Check to see if there is an item with in characters I
		/// Performed in Fixed update.
		/// </summary>
		void HoldingItemsCheck()
        {
			// if no object held then then look around for other items in sight. We're all grabby grabby here.
			if (ObjectBeingHeld == null)
			{
				//ensure flag set/ we realize nothing is being held
				isHoldingSomething = false;
				//look to see if there is something close by to interact with (note: using Raycast 2 to show different debug colors (cyan, magenta))
				RaycastHit2D ObjectCheckLow = Raycast2(new Vector2(footOffset * direction, grabHeightLow), new Vector2(direction, 0f), reachDistance, interactablesLayer);
				RaycastHit2D ObjectCheckHigh = Raycast2(new Vector2(footOffset * direction, grabHeightHigh), new Vector2(direction, 0f), reachDistance, interactablesLayer);

				//if something close by is found then 
				if (ObjectCheckHigh) //up high (priority)
				{                   //lets cache it until it no longer is in our vicinity
					WithInArmsReach = ObjectCheckHigh.collider.gameObject;
					//run a check to see if player has any input to interact with nearby object.

					InteractCheck();
				}
				else if (ObjectCheckLow) //or down low
				{
					//lets cache it until it no longer is in our vicinity
					WithInArmsReach = ObjectCheckLow.collider.gameObject;
					//run a check to see if player has any input to interact with nearby object.
					InteractCheck();
				}

				else //nothing found
				{
					WithInArmsReach = null;
					//if (WithInArmsReach) { }
				}
			}
		}

		//Interact:
		//here we go through all the possibilities of interaction provided the character is selected.
		void InteractCheck()
		{
			//interact not cooling down
			//double checking character is selected to prevent unneeded processing (probably redundant)
			if (isSelected && !interactCoolingDown)
			{
				//interact pressed
				if (input.interactPressed)
				{
					//Begin cooldown
					StartCoroutine(InteractCoolingDown());

					//is Interact pressed near InteractableObject while not holding something?
					if (WithInArmsReach != null && !isHoldingSomething)
					{
						print("Get component Interactable Object > Interact(this)");
						/*
						//look for the interactable object from the game object infront of character and sends this instance as a parameter when invoking interact.
						WithInArmsReach.GetComponent<InteractableObjects>().Interact(this);
						*/
					}
					else // ^no, then:
						 //Is interact pressed while holding an object?
					 if (isHoldingSomething && ObjectBeingHeld != null)
					{
						print("Get component Holdable Object > Interact(this)");
/*
						//interact with object in hands
						ObjectBeingHeld.GetComponent<HoldableObjects>().Interact(this);
*/
					}
				}
			}
		}
		/// <summary>
		/// Interact cool down coroutine
		/// </summary>
		/// <returns>jumpCollingDown = false</returns>
		IEnumerator InteractCoolingDown()
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
			if (ObjectBeingHeld != null)
			{
				Debug.Log("Engine Error PickUpHoldItem: Item being held: " + ObjectBeingHeld.ToString());
				return; //should not be allowed to finish 
			}
			ObjectBeingHeld = ItemInFrontToPickUp;
			objectCollider = ObjectBeingHeld.GetComponent<Collider2D>();
			objectScript = ObjectBeingHeld.GetComponent<HoldableObjects>();
			isHoldingSomething = true;
			WithInArmsReach = null;

			//objectCollider.enabled = true;
			//objectCollider.size = ObjectBeingHeld.GetComponent<BoxCollider2D>().size;
			//objectCollider.transform.position = ObjectBeingHeld.GetComponent<BoxCollider2D>().transform.position;
			//			objectCollider.transform.SetParent(holdPoint_Front);
		}
		/// <summary>
		/// action: Drops Item that is active in hands
		/// </summary>
		public void DropItem(GameObject _ObjectBeingHeld)
		{
			isHoldingSomethingAbove = false;
			//Transform tempTrans = objectCollider.transform;
			//	objectCollider.transform.SetParent(null);
			//objectCollider.transform.position = tempTrans.position;
			objectScript = _ObjectBeingHeld.GetComponent<HoldableObjects>();
			print("objectScript.GetPutDown();");
			// objectScript.GetPutDown();
			isHoldingSomething = false;
			objectCollider = null;
			objectScript = null;
			ObjectBeingHeld = null;
			StartCoroutine(DroppingItemCoolDown());
			Debug.Log("Object Dropped - Engine Side");
		}

		//LOOK AT: this might be extra code, 
		void BreakOverHead(HoldableObjects holdable)
		{
			print("Break over Head method   : holdable.GetPutDown();");
			//holdable.GetPutDown();
		}

		/// <summary>
		/// Delayed actions for after the character is deselected.
		/// </summary>
		/// <returns></returns>
		public IEnumerator DroppingItemCoolDown()
		{
			canHang = false;
			yield return new WaitForSeconds(cantHangCoolDownTime);
			canHang = true;
			Debug.Log("Can Hang test " + cantHangCoolDownTime + " sec");
			yield break;
		}
		#endregion

		#region Inventory Control
		/// <summary>
		/// The process of changing whats in character hand with what is in character inventory
		/// Execution Order: 
		/// -objectBeingHeld? 
		///     {yes:} keep its position as if held || 
		/// 
		///     {no:} identify interactables in line of sight 
		/// -PhysicsCheck() - process and check engine states. 
		/// -isSelected? 
		///    {yes:} GroundMovement(); MidAirMovement() || 
		///    {no:} 
		/// -end 
		/// </summary>
		public void ChangeItem()
		{
			GameObject tempItem = null;
			Debug.Log("launch Change Item Process:");
			//first lets check if we are holding any items 
			// if so are they are storeable objects?

			//begin switch item process bool
			//		inSwitchItemProcess = true;
			//if storeable item in hand looking for component then lets cache it temporarily deactivate
			if (isHoldingSomething && ObjectBeingHeld.TryGetComponent(typeof(StoreableObjects), out Component component))
			{
				//YES: get component,
				//then remove from above head if they are
				isHoldingSomethingAbove = false;
				//Cache the storeableObject of component
				StoreableObjects inHandStoreable = component.GetComponent<StoreableObjects>();
				//acknowledge
				Debug.Log("We found a type storeable Object on hand: " + inHandStoreable.ToString());

				//tell object in hand it is now in inventory
				print("inHandStoreable.PutInInventory();");
				//inHandStoreable.PutInInventory();

				//the item from hand goes to temp cache
				//assign to tempItem
				tempItem = inHandStoreable.gameObject;
				//lets deactivate 
				tempItem.SetActive(false);
				//InventoryItem.SetActive(false);
				//and state so
				isHoldingSomething = false;
			}

			//Overwrite Held Item with Inventory Item
			//always bring inventory object to hand no matter what when changing.
			ObjectBeingHeld = InventoryItem;

			Debug.Log("storeable object being held that came from inventory " + ObjectBeingHeld.ToString());

			//if an object in inventory overwrote object being held
			if (ObjectBeingHeld)
			{
				objectScript = ObjectBeingHeld.GetComponent<HoldableObjects>();
				Debug.Log("New object being held here, replaced from inventory " + ObjectBeingHeld.ToString());
				//activate
				ObjectBeingHeld.SetActive(true);
				//state
				isHoldingSomething = true;
				//locate / place
				ObjectBeingHeld.transform.position = holdPoint_Front.position;
				//tell new object that it is no longer in inventory
				print("ObjectBeingHeld.GetComponent<StoreableObjects>().RemoveFromInventory();");
				//ObjectBeingHeld.GetComponent<StoreableObjects>().RemoveFromInventory();
			}
			//replace inventory item with the game object of storeable in hand

			//not holding anything but there is something in inventory (should not happen)
			if (!isHoldingSomething && InventoryItem != null)
			{
				isHoldingSomethingAbove = false; //just in case

				Debug.Log("ERROR: There is no item in hands but in the inventory after trying to move");
				ObjectBeingHeld = InventoryItem; // retry
			}
			//now that the inventory item was migrated we can make it whatever the temp item was.
			InventoryItem = tempItem;
			//run cool down before we can run this process again

			StartCoroutine(InventorySwapCoolDown());

			//logic:
			// if so are they storeable objects?
			//YES: , then remove from above head if they are
			//cache storeable to tempVariable to store in inventory afterwards
			//Disable visibility and deactivate scripts during store_item animation
			//ELSE NO: if that item is not storeable we can either drop the item or ignore request. (for now we drop) and
			//next grab item in inventory and move it to the object being held variable,
			// replace inventory with temp storable object if one was being held
			// make item being held visible with animation to show it being removed from inventory
			//ELSE if there is no inventory object
			// change isHeld state to false and ObjectBeingHeld to null
			// replace inventory with temp storable object if one was being held.

			//Play animation and shit
		}

		/// <summary>
		/// count down timer to allow  in switch item process = false after switchItemcoolDownTime.
		/// </summary>
		/// <returns></returns>
		IEnumerator InventorySwapCoolDown()
		{
			yield return new WaitForSeconds(switchItemCoolDownTime);
			inSwitchItemProcess = false;
			yield break;
		}

		/// <summary>
		/// Delayed actions for after the character is deselected.
		/// </summary>
		/// <returns></returns>
		public IEnumerator ChangingInventoryObjects()
		{
			changeObjectCoolingDown = true;
			yield return new WaitForSeconds(changeObjectCoolDownTime);
			changeObjectCoolingDown = false;
			Debug.Log("Object change cool down complete test");
			yield break;
		}
		#endregion

		#region Hanging Actions
		/// <summary>
		/// Character grabs wall and hangs.
		/// Check to see if there is a hangalble platform by raycasting.
		/// if so determine if condition are right to hang from ledge.
		/// </summary>
		void WallGrabCheck()
		{
			//(SHOULD HANGING BE BEHIND A BUTTON PRESS? if so should this be called before this check?)

			//WALL GRAB CHECK
			//Determine the direction of the wall grab attempt
			Vector2 grabDir = new Vector2(direction, 0f);

			//Cast three rays to look for a wall grab
			RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, playerHeight), grabDir, grabDistance);

			RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, playerHeight), Vector2.down, grabDistance);

			RaycastHit2D wallCheck = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance);

			//HANGING:
			//If the player is off the ground AND is not hanging AND (interact is pressed OR is falling) AND
			//found a ledge AND found a wall AND the grab is NOT blocked
			//and is not holding something in hands...
			if (!isOnGround && !isHanging && rigidBody.velocity.y < 0f
				 && !isHoldingSomething && canHang
				 && ledgeCheck && wallCheck && !blockedCheck)
			  //|| (!isOnGround && !isHanging /* &&  rigidBody.velocity.y < 0f */
			  //&&
			  //   !isHanging && input.interactPressed
			  //    && !isHoldingSomething
			  //  && ledgeCheck && wallCheck && !blockedCheck
			  
			{
				//we have a ledge grab. Record the current position...
				Vector3 pos = transform.position;
				//...move the distance to the wall (minus a small amount)...
				pos.x += (wallCheck.distance - smallAmount - hangingDistanceFromLedge) * direction;
				//...move the player down to grab onto the ledge...
				pos.y -= ledgeCheck.distance;
				//...apply this position to the platform...
				transform.position = pos;
				//...set the rigidbody to static...
				rigidBody.bodyType = RigidbodyType2D.Static;
				//...finally, set isHanging to true
				isHanging = true;
			}
		}
		#endregion

		#region Basic Horizontal Movement
		void GroundMovement()
		{
			//If currently hanging, the player can't move to exit
			if (isHanging)
				return;

			//Handle crouching input. If holding the crouch button but not crouching, crouch
			if (input.crouchHeld && !isCrouching && isOnGround)
				Crouch();
			//Otherwise, if not holding crouch but currently crouching, stand up
			else if (!input.crouchHeld && isCrouching)
				StandUp();
			//Otherwise, if crouching and no longer on the ground, stand up
			else if (!isOnGround && isCrouching)
				StandUp();

			//Calculate the desired velocity based on inputs
			float xVelocity = speed * input.horizontal;

			//If the sign of the velocity and direction don't match, flip the character
			if (xVelocity * direction < 0f)
				FlipCharacterDirection();

			//If the player is crouching, reduce the velocity
			if (isCrouching)
				xVelocity /= crouchSpeedDivisor;

			//Apply the desired velocity 
			rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);

			//If the player is on the ground, extend the coyote time window
			if (isOnGround)
				//CoyoteTime is time that keeps you extended and gives player a
                //chance to make a last second decision
				coyoteTime = Time.time + coyoteDuration;
		}

		#endregion
		
		#region Stop Scripts

		public void EnterStaticState() => rigidBody.bodyType = RigidbodyType2D.Static;
		public void ExitStaticState() => rigidBody.bodyType = RigidbodyType2D.Dynamic;

		/// <summary>
		/// Stop Moving Character immediately while deselecting. Add delay before stopping character if they are in air to fake finishing a jump. 
		/// </summary>
		public void StopMovingChangingChar()
		{
			//In Air 
			if (!isOnGround)
			{
				StartCoroutine(StopMovingToChange());
				// small delay
				//  StopMovingToChange();
				// remove velocity - might be redundant after coroutine
				//	rigidBody.velocity = new Vector2(0, 0);
			}
			else
			{
				//remove velocity
				rigidBody.velocity = new Vector2(0, 0);
				//remove momentum by setting a finite position (dont know if this actually works)
				transform.position = new Vector3(transform.position.x, transform.position.y, 2f);
			}

		}

		/// <summary>
		/// If the character is in the air, I want to give it a more natural change feeling
		/// </summary>
		/// <returns></returns>
		public IEnumerator StopMovingToChange()
		{
			yield return new WaitForSeconds(.25f);
			rigidBody.velocity = new Vector2(0, 0);
			yield return new WaitForSeconds(.25f);

			yield break;
		}
		#endregion

		#region Orientation

		/// <summary>
		/// Bring Character in front of everyone to show that it is selected.
		/// </summary>
		public void bringFront()
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
			//			Debug.Log("Brought Forward");
		}

		/// <summary>
		/// Bring Character to background to show that it is not currently selected.
		/// This is default starting location for each player character
		/// </summary>
		public void sendBack()
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, 2f);
			//			Debug.Log("Brought Backward");
		}

		/// <summary>
		/// Change the characters direction for facing orientation
		/// </summary>
		void FlipCharacterDirection()
		{
			//Turn the character by flipping the direction
			direction *= -1;

			//Record the current scale
			Vector3 scale = transform.localScale;

			//Set the X scale to be the original times the direction
			scale.x = originalXScale * direction;

			//Apply the new scale
			transform.localScale = scale;
		}
		#endregion

		#region Advanced Movement (Jumping, Hanging, Crouching, Standing)
		/// <summary>
        /// What happens once character is in air
		/// Jumping and Hanging Controls
		/// </summary>
		void MidAirMovement()
		{
			//If Char currently is in hanging state
			if (isHanging)
			{
				//no longer jumping, we hangin
				isJumping = !isJumping;

				//DropFromLedge:
				//If crouch or down (beyond .2f threshold) is pressed...
				if (input.crouchPressed || input.vertical < -0.2f)
				{
					//let go
					isHanging = false;
					//set the rigidbody to dynamic and let gravity do the trick
					rigidBody.bodyType = RigidbodyType2D.Dynamic;
					return;
				}

				//Climb Ledge:
				//If jump is pressed and no cool down, while hanging
				if (input.jumpPressed && !jumpCoolingDown)
				{
					isOnGround = false;
					//let go of ledge
					isHanging = false;
					//rigidbody goes dynamic and apply a jump force
					rigidBody.bodyType = RigidbodyType2D.Dynamic;
					rigidBody.AddForce(new Vector2(0f, hangingJumpForce), ForceMode2D.Impulse);
					//start jump cool down to prevent double jump
					StartCoroutine(JumpCoolingDown());

					//...and tell the Audio Manager to play the jump audio
					//				AudioManager.PlayJumpAudio();

					return;
				}
			}
			//Jump:
			//If the jump key is pressed AND the player isn't already jumping AND EITHER
			//the player is on the ground or within the coyote time window...
			if (input.jumpPressed && !jumpCoolingDown && !isJumping && (isOnGround || coyoteTime > Time.time))
			{
				//check to see if crouching AND not blocked. If so
				if (isCrouching && !isHeadBlocked)
				{
					//stand up and apply a crouching jump boost
					StandUp();
					rigidBody.AddForce(new Vector2(0f, crouchJumpBoost), ForceMode2D.Impulse);
				}

				//charcter is no longer on the groud and is jumping
				isOnGround = false;
				isJumping = true;

				//record the time the player will stop being able to boost their jump...
				jumpTime = Time.time + jumpHoldDuration;

				//...add the jump force to the rigidbody...
				rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
				StartCoroutine(JumpCoolingDown());
				//...and tell the Audio Manager to play the jump audio
				//				AudioManager.PlayJumpAudio();
			}

			//Otherwise, if currently within the jump time window
			else if (isJumping)
			{
				//and the jump button is held, apply an incremental force to the rigidbody

				//	if (input.jumpHeld)
				//		rigidBody.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);

				//if jump time is past, set isJumping to false
				if (jumpTime <= Time.time)
					isJumping = false;
			}

			//If player is falling too fast, reduce the Y velocity to the max
			if (rigidBody.velocity.y < maxFallSpeed)
				rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
		}

		/// <summary>
		/// Jump cool down coroutine
		/// </summary>
		/// <returns>jumpCollingDown = false</returns>
		IEnumerator JumpCoolingDown()
		{
			jumpCoolingDown = true;
			yield return new WaitForSeconds(jumpCoolDownTime);
			jumpCoolingDown = false;
			//Debug.Log("jump Cooldown passed");
			yield return null;
		}

		/// <summary>
		/// Crouch - Setting isCrouching state true and changing collider and offset 
		/// </summary>
		void Crouch()
		{
			//The player is crouching
			isCrouching = true;

			//Apply the crouching collider size and offset
			bodyCollider.size = colliderCrouchSize;
			bodyCollider.offset = colliderCrouchOffset;
		}

		/// <summary>
		/// Stand up - checks to get out of crouch state if not crouching and head isnt blocked.
		/// </summary>
		void StandUp()
		{
			//If the player's head is blocked, they can't stand so exit
			if (isHeadBlocked)
				return;

			//The player isn't crouching
			isCrouching = false;

			//Apply the standing collider size and offset
			bodyCollider.size = colliderStandSize;
			bodyCollider.offset = colliderStandOffset;
		}
		#endregion

		#region raycasts
		//methods used to return raycasts for directional information around character

		/// <summary>
        /// Assumed Walkables layer for raycast()
        /// returns Raycast(Vector2, Vector2, float, walkables as LayerMask) 
        /// Call the overloaded Raycast() method using the ground layermask and return 
        /// </summary>
        /// <param name="offset">offset to char, Vector2 </param>
        /// <param name="rayDirection">direction of ray, Vector2 </param>
        /// <param name="length">length of line, float</param>
        /// <returns> RaycastHit2D</returns>
		RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
		{
			return Raycast(offset, rayDirection, length, walkables);
		}

		/// <summary>
		/// Green. hit: Red;
        /// Specified hits with layer mask
		/// Returns RaycastHit2D
		/// and Creates visual line in editor scene if drawDebugRaycasts
		/// </summary>
		/// <param name="offset">offset to char, Vector2 </param>
		/// <param name="rayDirection">direction of ray, Vector2 </param>
		/// <param name="length">length of line, float</param>
		/// <param name="mask">the layer raycast is looking to hit on, LayerMask</param>
		/// <returns>RaycastHit2D</returns>
		RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
		{
			//Record the player's position
			Vector2 pos = transform.position;

			//Send out the desired raycast and record the result
			RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

			//If we want to show debug raycasts in the scene...
			if (drawDebugRaycasts)
			{
				//...determine the color based on if the raycast hit...
				Color color = hit ? Color.red : Color.green;
				//...and draw the ray in the scene view
				Debug.DrawRay(pos + offset, rayDirection * length, color);
			}

			//Return the results of the raycast
			return hit;
		}

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
		RaycastHit2D Raycast2(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
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
        #endregion
    }
        #endregion
}
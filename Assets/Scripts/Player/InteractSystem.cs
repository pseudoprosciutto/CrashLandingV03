/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
{
	/// <summary>
	/// not currently implemented. meant to clean up character engine code
	///
	/// interact can happen at any time and does not need to wait for an order in engine
	/// 
	/// This system handles character interaction with interactables.
	/// </summary>
	public class InteractSystem : MonoBehaviour
	{
		InputHandler input;                     //The current inputs for the player
		CharacterEngine engine;             //engine on character to grab and set states
		InventorySystem inventory;

		bool interactCoolingDown = false;

		//default these badboys to be true so we can see where we reaching
		public bool drawDebugRaycasts= true;

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
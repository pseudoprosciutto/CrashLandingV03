using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CL03
{
	/// <summary>
    /// the interact system is used when character hits interact
	/// Handling control of the interact system on a character
	/// This is the scripts behind how the character interacts with object if in close proximity.
	/// and looking at object
    ///
    /// The character when selected will send a ray cast in the direction it is facing.
    /// If there is an object with an interactable layer that exists, then it will be interacted with.
	/// </summary>
	public class CharInteract2D : MonoBehaviour
	{
		InputHandler input;
		CharManager2D character;
		CharInventory2D inventory;

		[SerializeField]
		bool drawDebugRaycasts = true;

		[Space]
		[Title("Interact Distances")]
		public float reachDistance = .9f;       //The reach distance for object grabs
		public float grabHeightLow = .5f;          //Height of grab checks
		public float grabHeightHigh = 1f;          //Height of grab checks

		//cool down to not spam interact
		bool interactCoolingDown = false;
		public float interactCoolDownTime = .15f;      //prevent spamming interaction It takes time to lift objects or interact with something
		public GameObject WithInArmsReach; // { get; set{ if (!isHoldingSomething) return WithInReach(); } }


		//the layer for items that can be interacted with
		protected LayerMask interactablesLayer;
		protected LayerMask grabables;


		void Awake()
		{
			input = GetComponentInParent<InputHandler>();
			character = GetComponent<CharManager2D>();
			inventory = GetComponent<CharInventory2D>();

			grabables = character.crateLayer;
			grabables |= character.itemsLayer;
			interactablesLayer = grabables;
			interactablesLayer |= character.staticInteractablesLayer;
		}
		private void Update()
		{
			if (!character.isSelected) return;
			if (input.interactPressed && !interactCoolingDown)
			{
				print("Interacting...");
				StartCoroutine(InteractCoolDown());
				/**
				if (inventory.objectBeingHeld != null)
				{
					print("...within hands");
					InteractInHand();
				}

				else
				*/
				if (	ItemsCheck())
					{
						print("...with empty hands"); 
						InteractCheck();
					}
			}
			
		}
		/// <summary>
        ///
        /// (If hands are empty) Check to see if an object is in front of character to interact with
        /// </summary>
        private void InteractCheck()
        {

			//if not holding anything to interact with something not being held.
			if (WithInArmsReach != null && !inventory.isHoldingSomething)
			{
				print("interacting with object not in possession or being held");
				// WithInArmsReach.GetComponent<InteractableObjects>().Interact(engine);
			}
			//Hands full interact with whats in hands.
			else
			{
				print("there is an interaction going on but nothing is in hands or seen to interact with");
			}

			//RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);


		}

		/// <summary>
		/// Check to see if there is an item with in characters reach,
		/// and decide what the character will do with that
		///
		/// if no object held then then look around for other items in sight.
		/// We're all grabby grabby here.
		/// </summary><returns>bool true if there is an item in sight.</returns>
		private bool ItemsCheck()
		{

			//look to see if there is something close by to interact with (note: using Raycast 2 to show different debug colors (cyan, magenta))
			RaycastHit2D ObjectCheckLow = Raycast2(new Vector2(character.footOffset * character.direction, grabHeightLow), new Vector2(character.direction, 0f), reachDistance, interactablesLayer);
			RaycastHit2D ObjectCheckHigh = Raycast2(new Vector2(character.footOffset * character.direction, grabHeightHigh), new Vector2(character.direction, 0f), reachDistance, interactablesLayer);

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
			}

			//default false
			return false;
		}

		/// <summary>
        /// take the item that is held in hand (found in inventory ) and interact with it
        /// </summary>
		void InteractInHand()
		{           //Begin cooldown since interact 
			

			//if holding something and object item is seen in inventory.
			if (inventory.isHoldingSomething && inventory.objectBeingHeld != null)
			{
				print("interacting with whats in hands, command side.");

				//interact with object in hands

	//	inventory.objectBeingHeld.GetComponent<HoldableObjects>().Interact(character);

			}
		}





		/**
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
		*/

		//this makes the character have to drop an object if character goes underneath a platform and cant fit object 
		void BreakOverHead(HoldableObjects holdable)
		{
			print("Break over Head method   : holdable.GetPutDown();");
			holdable.GetPutDown();
		}



		#region coroutines
		IEnumerator InteractCoolDown()
		{
			interactCoolingDown = true;
			yield return new WaitForSeconds(interactCoolDownTime);
			interactCoolingDown = false;
			yield return null;
		}
		#endregion





		#region raycast


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
		#endregion
	}
}


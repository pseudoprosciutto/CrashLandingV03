using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CL03
{
	/// <summary>
	/// Handling control of the interact system on a character
	/// This is the scripts behind how the character interacts with object if in close proximity.
	/// and looking at object
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
		public float interactCoolDownTime = 1f;      //prevent spamming interaction It takes time to lift objects or interact with something


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


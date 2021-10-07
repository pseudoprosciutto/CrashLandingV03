/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
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
		CharacterEngine engine;             //engine on character
		bool interactCoolingDown;

		protected LayerMask grabables;
		//Interactable classified layers
		protected LayerMask interactablesLayer;
		public float interactCoolDownTime = 0.5f;      //prevent spamming interaction It takes time to lift objects or interact with something

		Collider2D objectCollider;              //recognized object
		HoldableObjects objectScript;           //recognized object's script

		public Vector2 objectColliderSize;
		public bool isInteracting_Test;    //test bool
		public bool changeObjectCoolingDown;  //is object cooling down?
		public float changeObjectCoolDownTime = 1.2f;

		void Awake()
		{
			input = GetComponentInParent<InputHandler>();
			engine = GetComponent<CharacterEngine>();
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
			if (ItemsCheck()) InteractCheck();
			if (engine.isSelected)
			{
				HandledObjectsCheck();
				//ROTATE OBJECTS IN POSSESSION
				//if selected and object held. press up or down the object being held changes position
				if (engine.ObjectBeingHeld != null)
				{
					//change position of object in hand
					if (input.vertical > .2f) { engine.isHoldingSomethingAbove = true; }
					if (input.vertical < -.2f) { engine.isHoldingSomethingAbove = false; }
				}

				//should this be after held? instead of pressed
				if (input.changeObjectPressed && !changeObjectCoolingDown)
				{
					Debug.Log("Change object button pressed and change object cooling down is false.");
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
		}



		void HandledObjectsCheck()
		{

			//DROP OBJECT
			if (input.dropObjectPressed)
			{
				if (engine.ObjectBeingHeld)
				{
					DropItem(engine.ObjectBeingHeld);
				}
			}
			//SWITCH TO INVENTORY
			if (input.changeObjectPressed)
			{
				if (!engine.inSwitchItemProcess)
					engine.ChangeItem();
			}

		}


		#region Interacting Behaviors, Item control

		/// <summary>
		/// Check to see if there is an item with in characters reach,
		/// and decide what the character will do with that
		/// Performed in Fixed update.
		/// </summary>
		private bool ItemsCheck()
		{
			// if no object held then then look around for other items in sight. We're all grabby grabby here.
			if (engine.ObjectBeingHeld == null)
			{
				//ensure flag set/ we realize nothing is being held. should be unnecessary
				engine.isHoldingSomething = false;

				//look to see if there is something close by to interact with (note: using Raycast 2 to show different debug colors (cyan, magenta))
				RaycastHit2D ObjectCheckLow = engine.Raycast2(new Vector2(engine.footOffset * engine.direction, engine.grabHeightLow), new Vector2(engine.direction, 0f), engine.reachDistance, interactablesLayer);
				RaycastHit2D ObjectCheckHigh = engine.Raycast2(new Vector2(engine.footOffset * engine.direction, engine.grabHeightHigh), new Vector2(engine.direction, 0f), engine.reachDistance, interactablesLayer);

				//if something close by is found then can (only be one object in front of character)
				if (ObjectCheckHigh) //up high (priority)
				{                   //lets cache it until it no longer is in our vicinity
					engine.WithInArmsReach = ObjectCheckHigh.collider.gameObject;
					//run a check to see if player has any input to interact with nearby object.
					//	InteractCheck();
					return true;
				}
				else if (ObjectCheckLow) //or down low
				{
					//lets cache it until it no longer is in our vicinity
					engine.WithInArmsReach = ObjectCheckLow.collider.gameObject;
					//run a check to see if player has any input to interact with nearby object.
					//	InteractCheck();
					return true;
				}
				else //nothing found
				{
					engine.WithInArmsReach = null;
					//if (WithInArmsReach) { }

				}

			}
			//default false
			return false;
		}

		//Interact:
		//here we go through all the possibilities of interaction provided the character is selected.
		void InteractCheck()
		{
			//interact not cooling down
			//double checking character is selected to prevent unneeded processing (probably redundant)
			if (engine.isSelected && !interactCoolingDown)
			{
				//interact pressed
				if (input.interactPressed)
				{
					//Begin cooldown
					StartCoroutine(InteractCoolingDown());

					//object within arms reach while not holding something?
					if (engine.WithInArmsReach != null && !engine.isHoldingSomething)
					{
						print("Empty Hands see an object " + engine.WithInArmsReach.ToString() + " . That object has been interacted with");
						//look for the interactable object from the game object infront of character and sends this instance as a parameter when invoking interact.
						engine.WithInArmsReach.GetComponent<InteractableObjects>().Interact(engine);

					}
					// ^no, then:
					//Is interact pressed while holding an object?

					else if (engine.isHoldingSomething && engine.ObjectBeingHeld != null)
					{
						print("Get component Holdable Object > Interact(this)");

						//interact with object in hands
						engine.ObjectBeingHeld.GetComponent<HoldableObjects>().Interact(engine);

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
			if (engine.ObjectBeingHeld != null)
			{
				Debug.Log("Engine Error PickUpHoldItem: Item being held: " + engine.ObjectBeingHeld.ToString());
				return; //should not be allowed to finish 
			}
			engine.ObjectBeingHeld = ItemInFrontToPickUp;
			objectCollider = engine.ObjectBeingHeld.GetComponent<Collider2D>();
			objectScript = engine.ObjectBeingHeld.GetComponent<HoldableObjects>();
			engine.isHoldingSomething = true;
			engine.WithInArmsReach = null;

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
			engine.isHoldingSomethingAbove = false;
			//Transform tempTrans = objectCollider.transform;
			//	objectCollider.transform.SetParent(null);
			//objectCollider.transform.position = tempTrans.position;
			objectScript = _ObjectBeingHeld.GetComponent<HoldableObjects>();
			print("objectScript.GetPutDown();");
			// objectScript.GetPutDown();
			engine.isHoldingSomething = false;
			objectCollider = null;
			objectScript = null;
			engine.ObjectBeingHeld = null;
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
			engine.canHang = false;
			yield return new WaitForSeconds(engine.cantHangCoolDownTime);
			engine.canHang = true;
			Debug.Log("Can Hang test " + engine.cantHangCoolDownTime + " sec");
			yield break;
		}
		#endregion

	}
}
/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Cinemachine;

/// <summary>
/// CrashLanding v03 
/// </summary>
namespace CL03
{
	/// <summary>
	/// Character Script handles character properties and states, movement, and core character functions for other components to reference.
	/// TO DO: 
	/// Going to refactor this t
	/// </summary>
	public class CharacterEngine : MonoBehaviour
	{
		#region FIELDS
		#region Component References: Rigidbody, BoxCollider, Input Handler
		public BoxCollider2D bodyCollider;             //The collider component
		Rigidbody2D rigidBody;                  //The rigidbody component
		InputHandler input;                     //The current inputs for the player
		InventorySystem inventory;              //inventory system
		HealthSystem health;					//health system with revive stuffs
		#endregion

		#region Character State bools
		[BoxGroup("Character State")] public bool isSelected;
		[BoxGroup("Character State")] public bool needsReviving;
		[Space]
		[BoxGroup("Character State")] public bool isOnGround;                 //Is the player on the ground?
		[BoxGroup("Character State")] public bool isOnPlatform;
		[BoxGroup("Character State")] public bool isRunning = false; //we are not automatically running
		[BoxGroup("Character State")] public bool isJumping;                  //Is player jumping?
		[BoxGroup("Character State")] public bool isCrouching;                //Is player crouching?
		[BoxGroup("Character State")] public bool isHeadBlocked;
		[BoxGroup("Character State")] public bool isHanging;                  //Is player hanging?
																			  //[BoxGroup("Character State")] public bool isHoldingSomething;
		[BoxGroup("Character State")] public bool objectHitCheck;										  //[BoxGroup("Character State")] public bool isHoldingSomethingAbove;
		[BoxGroup("Character State")] public bool hitOverHeadLeft;
		[BoxGroup("Character State")] public bool hitOverHeadRight;
		[BoxGroup("Character State")] public bool hitOverHeadFrontCorner;
		#endregion

		[Space]
		#region hanging and object hit properties
		//can hang
		public bool canHang = true; // can the character even hang dawg and kick it with the other hanging homies?
		public float cantHangCoolDownTime = .9f;
		[SerializeField]
		float objectHitCheckCoolDownTime =.5f;
		[SerializeField]
		bool objectHitCoolingDown = false;

		#endregion
		[Space]
		#region Move and Jump Modifiers 
		[FoldoutGroup("Movement Properties", expanded: false)]
		public float speed = 4.2f;                //Player speed
		[FoldoutGroup("Movement Properties")]
		public float walkSpeed = 4.2f;                //Player speed
		[FoldoutGroup("Movement Properties")]
		public float runSpeed = 6.2f;                //Player speed
		[FoldoutGroup("Movement Properties")]
		public float crouchSpeedDivisor = 3f;   //Speed reduction when crouching
		[FoldoutGroup("Movement Properties")]
		public float coyoteDuration = .05f;     //How long the player can jump after falling
		[FoldoutGroup("Movement Properties")]
		public float maxFallSpeed = -25f;       //Max speed player can fall

		[FoldoutGroup("Jump Properties", expanded: false)]
		public float jumpForce = 28.5f;           //Initial force of jump
		[FoldoutGroup("Jump Properties")]
		public float bootsJumpForce = 33.5f;           //Initial force of jump
		[FoldoutGroup("Jump Properties")]
		public float bootsModifier = 5f;           //Initial force of jump
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
		public LayerMask groundLayer;           //Layer of the ground
		public LayerMask walkableObject;
		public LayerMask crateLayer;

		//Interactable classified layers
		protected LayerMask interactablesLayer;
		public LayerMask itemsLayer;
		public LayerMask staticInteractablesLayer;
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
		public float objectJumpCheckHeight = 2.7f;
		[FoldoutGroup("Environment Check Properties")]
		public float objectAboveHeadForwardOffset = 1f; //Object checks forward Above head to prevent clipping object through walls
		[FoldoutGroup("Environment Check Properties")]
		public float objectAboveHeadUpwardOffset = 1f; //Object checks upward Above head to prevent clipping object through walls
		[FoldoutGroup("Environment Check Properties")]
		public float reachOffset = .7f;         //X offset for wall grabbing
		[FoldoutGroup("Environment Check Properties")]
		public float headClearance = .5f;       //Space needed above the player's head
		[FoldoutGroup("Environment Check Properties")]
		public float breakOverHeadDistance = 1f;
		[FoldoutGroup("Environment Check Properties")]
		public float groundDistance = .1f;      //Distance player is considered to be on the ground
		[FoldoutGroup("Environment Check Properties")]
		public float grabDistance = .4f;        //The reach distance for wall grabs

		[FoldoutGroup("Environment Check Properties")]
		public float hangingDistanceFromLedge = 0.1f; //added space between ledge and sprite
		[FoldoutGroup("Environment Check Properties")]
		public float headBounce = .2f;
		#endregion


		#region Private Properties
		bool jumpCoolingDown = false;

		float jumpTime;                         //Variable to hold jump duration
		float coyoteTime;                       //Variable to hold coyote duration
		const float smallAmount = .05f;         //A small amount used for hanging position
		float playerHeight;                     //Height of the player
		float originalXScale;                   //Original scale on X axis
		public int direction = 1;                      //Direction player is facing

		Collider2D colliderItem;

		Vector2 colliderStandSize;              //Size of the standing collider
		Vector2 colliderOrigStandSize;
		Vector2 colliderOrigCrouchSize;
		Vector2 colliderStandOffset;            //Offset of the standing collider
		Vector2 colliderCrouchSize;             //Size of the crouching collider
		Vector2 colliderCrouchOffset;           //Offset of the crouching collider


		#endregion
		#endregion

		#region METHODS

		#region Initialization; Awake() and Start()
		private void Awake()
		{
			
			canHang = true;

			objectHitCheck = false;

			//place character in correct z plane
			transform.position = new Vector3(transform.position.x, transform.position.y, 2f);

			//objectCollider.enabled = false;


			//walkables meaning what layer masks the character can walk on.
			walkables = groundLayer;
			walkables |= walkableObject;
			walkables |= crateLayer;
	
		}

		void Start()
		{
			//Get a reference to the required components
			input = GetComponentInParent<InputHandler>();
			rigidBody = GetComponent<Rigidbody2D>();
			bodyCollider = GetComponent<BoxCollider2D>();
			inventory = GetComponent<InventorySystem>();
			health = GetComponent<HealthSystem>();
			//Record the original x scale of the player
			originalXScale = transform.localScale.x;

			//Record the player's height from the collider
			playerHeight = bodyCollider.size.y;

			//Record initial collider size and offset
			colliderOrigStandSize = bodyCollider.size;
			colliderStandSize = colliderOrigStandSize;
			colliderStandOffset = bodyCollider.offset;

			//Calculate crouching collider size and offset
			colliderOrigCrouchSize = new Vector2(bodyCollider.size.x, bodyCollider.size.y / 2f);
			colliderCrouchSize = colliderOrigCrouchSize;
			colliderCrouchOffset = new Vector2(bodyCollider.offset.x, bodyCollider.offset.y / 2f);
		}
		#endregion

		#region Character Update Loops; Fixed update
		/// <summary>
		/// Fixed update Execution Order: 
		/// -PhysicsCheck() - process and check engine states. 
		/// -isSelected? 
		///    {yes:} GroundMovement(); MidAirMovement() || 
		///    {no but isOnGround:} EnterStaticState();
        ///    else we let the character fall until it can't
        ///    TO BE IMPLEMENTED
        ///    {else:} drop character below map, character respawns somewhere it does have a ground to land on
        ///    (safety clause will eventually be added here) 
		/// </summary>
		void FixedUpdate() {

			//Check the environment to determine status
			PhysicsCheck();

			//if player is selected and once physics have been checked then we can continue deciding how to player moves knowing state and environment
			if (isSelected && health.isAlive)
			{
				//Process ground and air movements
				GroundMovement();
				MidAirMovement();
			}
			//else we arent selected and we have landed on ground so we need to be static
			else if ((!isSelected || !health.isAlive)&& isOnGround) EnterStaticState();
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

			//make sure we are alive
			//if health says we arent alive but not in needs reviving state then change it to and set character horizontal.
			if (!health.isAlive && !needsReviving){
				needsReviving = true;
	// ****** //transform.localRotation = new Vector3(90f, 0f, 0f);
            //if we are not alive then we need to be horizontal.
			}
			//then we need to be in need reviving position and all tags need to be set on.
//might not even need rest of physics check.

			//Start by assuming the character isn't on the ground and the head isn't blocked
			CharacterStandingOnSurfaceCheck();
			CharacterHeadCheck();
			//special case for bounce boots,
			if(inventory.inventoryItem!= null)
            {
				if(inventory.inventoryItem.GetComponent<HoldableObjects>().GetItemType == ItemType.Boots)
				{
					if(!isOnGround && ObjectInBottomFrontCornerCheck())
                    {

                    }
						;
				}
            }
			//if hands are empty than we can attempt a wallgrab check.
			if (!inventory.isHoldingSomething) //ObjectBeingHeld)
			{
				WallGrabCheck();
			}
            else
			{

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
			
			//isOnPlatform = true;
		}

		/// <summary>
		/// Head check. (aka Lets not put our head through things.)
		/// sends out raycasthits and lets you know how large head trauma bill will be.
		/// </summary>
		void CharacterHeadCheck()
		{
			RaycastHit2D hitCheckObjectClearance = Raycast2(new Vector2(.2f * direction, 1.8f), new Vector2(direction, Mathf.Abs(direction)), 0.8f, walkables);

			isHeadBlocked = false;

			if (inventory.isHoldingSomethingAbove)
			{
				ObjectAboveHeadCheck();
				isHeadBlocked = true;
			}
			else if (inventory.isHoldingSomething)
			{
				ObjectInFrontCheck();
			}

			hitOverHeadLeft = false;
			hitOverHeadRight = false;

			//HEAD CHECK   ?? this one is confused with another isHeadBlocked below
			RaycastHit2D fullHeadCheck = Raycast2(new Vector2(Math.Abs(direction) - 1.5f, playerHeight), Vector2.right, 1f, walkables);
			if (fullHeadCheck) { isHeadBlocked = true; }

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
			RaycastHit2D headCheck = Raycast2(new Vector2(0f, bodyCollider.size.y), Vector2.up, headClearance, walkables);

			//If that ray hits, the player's head is blocked
			if (headCheck)
			{
				isHeadBlocked = true;
				Debug.Log("object hit head test");
				//something bonks the head, it will no longer get stuck there because we are flat headed and smooth brained.
				//if (!headCheck.collider.CompareTag("Environment") && !headCheck.collider.CompareTag("Surface") && !headCheck.collider.Equals(ObjectBeingHeld))
				if (!headCheck.collider.CompareTag("Surface") && !headCheck.collider.Equals(inventory.objectBeingHeld))
				{
					//slight backwards force added to prevent objects from staying on head. things should just roll off
					Rigidbody2D rb = headCheck.collider.GetComponent<Rigidbody2D>();
					rb.AddForceAtPosition(Vector2.up, new Vector2(headBounce, bodyCollider.size.y), ForceMode2D.Force);
				}
			}
		}

		#endregion

		#region object checks
		/// <summary>
		/// Check around object above corner head.
		/// if the object above head is not going to clear then character is stuck from moving any further
		/// </summary>
		public bool ObjectInFrontCornerCheck()
		{
			RaycastHit2D hitCheckObjectClearance = Raycast2(new Vector2(.2f * direction, 1.8f), new Vector2(direction, Mathf.Abs(direction)), 0.8f, walkables);
			if (hitCheckObjectClearance)
			{
				Debug.Log("object above corner hit check");
				return true;
			}
			return false;
		}

		/// <summary>
		/// Check around object below corner for jumping bounce.
		/// if the object is clear then can jump again
		/// </summary>
		public bool ObjectInBottomFrontCornerCheck()
		{
			RaycastHit2D hitCheckObjectClearance = Raycast2(new Vector2(.2f * direction, .3f), new Vector2(direction, -Mathf.Abs(direction)), 0.8f, walkables);
			if (hitCheckObjectClearance)
			{

				Debug.Log("object below corner hit check");
				return true;
			}
			return false;
		}


		/// <summary>
		/// Check around object above head.
		/// if the object above head is not going to clear then character is stuck from moving any further
		/// </summary>
		public void ObjectAboveHeadCheck() {
			objectHitCheck = false;
					RaycastHit2D hitCheckObjectClearance = Raycast2(new Vector2(footOffset * direction, 2.5f), new Vector2(direction, 0f), 0.2f, walkables);
				if (hitCheckObjectClearance)
				{
					Debug.Log("object above head hit check");
				objectHitCheck = true;
				}
		}

		/// <summary>
		/// Check for change item space from top of head to front
		/// if the object above head is not going to clear then character is stuck from moving any further
		/// </summary>
		public bool ObjectChangeHitFrontCheck()
		{
			RaycastHit2D hitCheckObjectClearance1 = Raycast2(new Vector2((footOffset + reachOffset + .2f) * direction, 1.25f), new Vector2(direction, 0f), 0.2f, walkables);
			RaycastHit2D hitCheckObjectClearance2 = Raycast2(new Vector2((footOffset + reachOffset + .2f) * direction, 1.25f), new Vector2(0, 1f), .4f, walkables);

			if (hitCheckObjectClearance1 || hitCheckObjectClearance2)
			{
				Debug.Log("object change check caught");
				//bounce off wall slightly
				//rigidBody.AddForce(new Vector2(-8f*direction,0f), ForceMode2D.Impulse);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Check around object above head.
		/// if the object above head is not going to clear then character is stuck from moving any further
		/// </summary>
		public void ObjectInFrontCheck()
		{
			objectHitCheck = false;
			
			RaycastHit2D hitCheckObjectClearance1 = Raycast2(new Vector2((footOffset+reachOffset+.2f )* direction, 1.25f), new Vector2(direction, 0f), 0.2f, walkables);
			RaycastHit2D hitCheckObjectClearance2 = Raycast2(new Vector2((footOffset + reachOffset + .2f) * direction, 1.25f), new Vector2(0, 1f), .4f, walkables);

			if (hitCheckObjectClearance1 || hitCheckObjectClearance2)
			{
				Debug.Log("object in front of body hit check");
				//bounce off wall slightly
				//rigidBody.AddForce(new Vector2(-8f*direction,0f), ForceMode2D.Impulse);
				objectHitCheck = true;

			}
		}
		#endregion


		#region Special Action check
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
				 && !inventory.isHoldingSomething && canHang
				 && ledgeCheck && wallCheck && !blockedCheck)
			  //|| (!isOnGround && !isHanging /* &&  rigidBody.velocity.y < 0f */
			  //&&
			  //   !isHanging && input.interactPressed
			  //    && !isHoldingSomething
			  //  && ledgeCheck && wallCheck && !blockedCheck
			  
			{
				//we have a ledge grab. Record the current position
				Vector3 pos = transform.position;
				//move the distance to the wall (minus a small amount)
				pos.x += (wallCheck.distance - smallAmount - hangingDistanceFromLedge) * direction;
				//move the player down to grab onto the ledge
				pos.y -= ledgeCheck.distance;
				//apply this position to the platform
				transform.position = pos;
				//rigidbody to static
				rigidBody.bodyType = RigidbodyType2D.Static;
				//set isHanging to true
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

			//handle run modifier
			if (input.moveModifyPressed)
				MoveModify();
			else
				speed = walkSpeed;
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

			//if object above head hit then we dont move
			if (!objectHitCheck)
				//Apply the desired velocity othewise
				rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);

			else if (objectHitCheck && !objectHitCoolingDown) //stop moving because we hit head
			{
				rigidBody.velocity = new Vector2(0, 0);
				StartCoroutine(ObjectHitCheckCoolDown());
			}
			
			//If the player is on the ground, extend the coyote time window
			if (isOnGround)
				//CoyoteTime is time that keeps you extended and gives player a
                //chance to make a last second decision
				coyoteTime = Time.time + coyoteDuration;
		}

		#endregion

		public IEnumerator ObjectHitCheckCoolDown()
        {
			objectHitCoolingDown = true;
			yield return new WaitForSeconds(objectHitCheckCoolDownTime);
				objectHitCoolingDown = false;
			yield break;
        }


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
			if (input.jumpPressed && !jumpCoolingDown && !isJumping && (isOnGround || coyoteTime > Time.time)
				&& !isHeadBlocked)
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
		/// MoveModify
		/// </summary>
		void MoveModify()
        {
			isRunning = true;
//modify bounce boots run speed here
			speed = runSpeed;
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
		public RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
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
		public RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
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
	#endregion
}
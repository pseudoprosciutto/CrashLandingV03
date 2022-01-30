/* Code amalgamated together By: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
{
	/// <summary>
	/// Character Equipment states
	/// </summary>
	public enum EquipmentState
	{
		Default,
		WearingBoots,
		WearingReceiver
	}

	/// <summary>
    /// Character Health State
    /// </summary>
	public enum CharHealthState
    {
		IsAlive,
		WillNeedRevive,
		NeedsRevive,
		BeingRevived
    }


	/// <summary>
	/// Manages the Player Character by modifying character states.
	/// </summary>
	[RequireComponent(typeof(CharPhysics2D))]
	public class CharManager2D : MonoBehaviour
	{
		/** Old Code **/
		public float jumpHeight = 1.3f;
		public float timeToJumpApex = .25f;
		float accelerationTimeAirborne = .2f;
		float accelerationTimeGrounded = .1f;
		float moveSpeed;

		float gravity;
		float jumpVelocity;
		float jumpLedgeVelocity;
		float jumpWallVelocity;
		Vector3 velocity;
		float velocityXSmoothing;

		CharPhysics2D physics;


		/**New Code **/
		InputHandler input;

		float ledgeGravity;

		public float jumpLedgeHeight = 2.25f;
		public float timeToJumpLedgeApex = .35f;


		[Header("Layer Masks")]

		//Layer of the ground
		[SerializeField]
		protected LayerMask walkables;
		
		public LayerMask crateLayer;

		//Interactable classified layers
		protected LayerMask interactablesLayer;
		public LayerMask itemsLayer;
		public LayerMask staticInteractablesLayer;
		[SerializeField]
		protected LayerMask characterLayer;
		[ReadOnly]
		public bool isHoldingSomething;

		public EquipmentState equipState;
		public CharHealthState charState = CharHealthState.IsAlive;
        #region bool States
        [BoxGroup("Character State")] public bool isSelected;
		[Space]
		[BoxGroup("Character State")] public bool canMove;
		[BoxGroup("Character State")] public bool isOnGround;                 //Is the player on the ground?
		[BoxGroup("Character State")] public bool isOnPlatform;
		[BoxGroup("Character State")] public bool isRunning = false; //we are not automatically running
		[BoxGroup("Character State")] public bool isJumping;                  //Is player jumping?
		[BoxGroup("Character State")] public bool isCrouching;                //Is player crouching?
		[BoxGroup("Character State")] public bool isHeadBlocked;
		[BoxGroup("Character State")] public bool isFrontHeadBlocked;
		[BoxGroup("Character State")] public bool isHanging;
		[Space]
		public bool knowsGravity = true;
		#endregion
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
		[FoldoutGroup("Movement Properties")]
		public float hangingJumpForce =  15f;
		[Space]

		[FoldoutGroup("Environment Check Properties", expanded: false)]
		public float headOffset = .35f;
		[FoldoutGroup("Environment Check Properties")]
		public float footOffset = .25f;          //X Offset of feet raycast
		[FoldoutGroup("Environment Check Properties")]
		public float eyeHeight = 1.5f;          //Height of wall checks
		[FoldoutGroup("Environment Check Properties")]
		public float reachOffset = .7f;         //X offset for wall grabbing
		[FoldoutGroup("Environment Check Properties")]
		public float hangingDistanceFromLedge = 0.1f; //added space between ledge and sprite
		[FoldoutGroup("Environment Check Properties")]
		public float headBounce = .2f;
		[FoldoutGroup("Environment Check Properties")]
		public float grabDistance = .4f;        //The reach distance for wall grabs


		public float jumpCoolDownTime = 0.25f;   //To prevent spammable jumping

		[Space]

		bool canBounceOffWall = false;


		[Header("Debug and Raycasting Values:")]
		public bool drawDebugRaycasts = true;   //Should the environment checks be visualized

		[Space]
		float coyoteTime;                       //Variable to hold coyote duration
		const float smallAmount = .05f;         //A small amount used for hanging position
		float playerHeight;                     //Height of the player
		float originalXScale;                   //Original scale on X axis
		public int direction = 1;                      //Direction player is facing

		#region hanging and object hit properties
		//can hang
		public bool canHang = true; // can the character even hang dawg and kick it with the other hanging homies?
		public float cantHangCoolDownTime = .9f;
		[SerializeField]
		float objectHitCheckCoolDownTime = .5f;
		[SerializeField]
		bool isOnAnotherChar;
		bool objectHitCoolingDown = false;
		bool jumpQuickCoolingDown = false;
		bool jumpCoolingDown = false;
		bool dJumpCoolingDown = false;


		Collider2D colliderItem;

		Vector2 colliderStandSize;              //Size of the standing collider
		Vector2 colliderOrigStandSize;
		Vector2 colliderOrigCrouchSize;
		Vector2 colliderStandOffset;            //Offset of the standing collider
		Vector2 colliderCrouchSize;             //Size of the crouching collider
		Vector2 colliderCrouchOffset;

		#endregion
		private void Awake()
        {
			transform.position = new Vector3(transform.position.x, transform.position.y, 2f);
		}
		void Start()
		{
			canHang = true;
			isHanging = false;
			input = GetComponentInParent<InputHandler>();
			/** Old Code **/
			physics = GetComponent<CharPhysics2D>();
			//Record the original x scale of the player
			originalXScale = transform.localScale.x;
			BoxCollider2D boxCollider = physics.boxCollider;       
			//Record the player's height from the collider
			playerHeight = boxCollider.size.y;
			//Record initial collider size and offset
			colliderOrigStandSize = boxCollider.size;
			colliderStandSize = colliderOrigStandSize;
			colliderStandOffset = boxCollider.offset;

			/*
			//Calculate crouching collider size and offset
			colliderOrigCrouchSize = new Vector2(physics.boxCollider.size.x, physics.boxCollider.size.y / 2f);
			colliderCrouchSize = colliderOrigCrouchSize;
			colliderCrouchOffset = new Vector2(physics.boxCollider.offset.x, physics.boxCollider.offset.y / 2f);
			*/

			gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			ledgeGravity = -(2 * jumpLedgeHeight) / Mathf.Pow(timeToJumpLedgeApex, 2);


			jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
			jumpLedgeVelocity = Mathf.Abs(gravity) * timeToJumpLedgeApex;
			print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);

			walkables |= physics.collisionMask;

		//	charState =  CharHealthState.IsAlive; //set in instance
		}

		//We colliding vertically?
		/// <summary>
        /// Every update:
        /// Check Collisions
        /// move sprite
        /// Parse input on character if selected.
        /// </summary>
		void Update()
		{

			//check collisions and set bool states to know how input can react on manager
			ParseVerticalCollisions();              //collisions
			//move sprite
			if (isSelected) { bringFront(); } else { sendBack(); };
			//if (interact) insn't turned on. turn it on

			//not holding something and in air, then we can wall grab check
			if (!isHoldingSomething&&!isOnGround) { WallGrabCheck(); }

			if (isSelected && charState == CharHealthState.IsAlive)
			{

				//We not hanging
				if (!isHanging)
				{
					knowsGravity = true;
                    #region horizontal
                    Vector2 directionPressed = new Vector2(input.horizontal, input.vertical);
					

					//calculate desired speed
					moveSpeed = speed;

					//are we running?
					if (input.moveModifyPressed)
					{
						moveSpeed = runSpeed;
						isRunning = true;
					}
                    else
                    {
						isRunning = false;
                    }
					//if (input.crouch ) ...
					
					float targetVelocityX = directionPressed.x * moveSpeed;
					
					velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (physics.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

					//If the sign of the velocity and direction don't match, flip the character
					if (targetVelocityX * direction < 0f)
						FlipCharacterDirection();
					#endregion

					#region Jump pressed
					//jump pressed and on ground wit
					if (input.jumpPressed
						&& (isOnGround || coyoteTime > Time.time)
						&& !jumpCoolingDown && !isJumping )
					{
                        #region DEBUG Jump apex
                        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
						#endregion
						isOnAnotherChar = false;
						isHanging = false;
						isJumping = true;
						isOnGround = false;
						velocity.y = jumpVelocity;
						StartCoroutine(JumpCoolingDown());		
					}
                    #endregion
                    //gravity always


					//If the player is on the ground, extend the coyote time window
					if (isOnGround)
						//CoyoteTime is time that keeps you extended and gives player a
						//chance to make a last second decision
						coyoteTime = Time.time + coyoteDuration;
				}
				else // we hanging
                {
					velocity.y = 0f;
					velocity.x = 0f;
					isJumping = false;
					isOnGround = false;
					//DropFromLedge:
					//If crouch or down (beyond .2f threshold) is pressed...
					if (input.crouchPressed || input.vertical < -0.2f)
					{
						//let go
						isHanging = false;
						//set the rigidbody to dynamic and let gravity do the trick
						return;
					}
					//Climb Ledge:
					//If jump is pressed and no cool down, while hanging
					if (input.jumpPressed && !jumpCoolingDown)
					{
						float ledgeGravity = -(2 * jumpLedgeHeight) / Mathf.Pow(timeToJumpLedgeApex, 2);
#region DEBUG Jump setting
						jumpLedgeVelocity = Mathf.Abs(ledgeGravity) * timeToJumpLedgeApex;
#endregion
                        isHanging = false;
						isJumping = true;
						isOnGround = false;
						//let go of ledge
						velocity.y = jumpLedgeVelocity;
						//start jump cool down to prevent double jump
						StartCoroutine(JumpCoolingDown());

						//...and tell the Audio Manager to play the jump audio
						//				AudioManager.PlayJumpAudio();

						return;
					}
				}
			}
			//do we know if we know gravity?
			if (isHanging) { knowsGravity = false; } else { knowsGravity = true; }
            #region DEBUG gravity
			//debug gravity change here
            gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			#endregion
			//know gravity?
			if (knowsGravity)ParseGravity();
			//dont fall too fast
			if (velocity.y < maxFallSpeed) ParseFallSpeedCap();
			//move character with all that info
			physics.Move(velocity * Time.deltaTime);
		}


		void ParseVerticalCollisions()
		{
				isOnGround = false;
			if (physics.collisions.above )
			{
				velocity.y = 0;
			}
			if (!jumpQuickCoolingDown && physics.collisions.below)
			{

				//RaycastHit2D leftFootCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, .1f, characterLayer);
				//RaycastHit2D rightFootCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, .1f, characterLayer);
				////If either ray hit the ground, the player is on the ground
				//if (leftFootCheck || rightFootCheck)
				//{
				//	isOnGround = false;
				//	isOnAnotherChar = true;
				//}
				//else
				//{
					isOnGround = true;
					isOnAnotherChar = false;
				
				isJumping = false;
				velocity.y = 0;
			}
		}
		
		void ParseGravity() => velocity.y += gravity * Time.deltaTime;
		void ParseFallSpeedCap() => velocity.y = maxFallSpeed;
		#region getmessages
		///we are going to get a message to tell us we have certain equipment so as not to keep doing searches.
		public void setEquipmentSateMode(EquipmentState _state)
		{
			this.equipState = _state;
		}
		///we are going to get a message to tell us we in certain state so as not to keep doing searches.
		public void setCharSateMode(CharHealthState _state)
		{
			this.charState = _state;
		}

		#endregion

		#region Stop Scripts
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
				velocity = new Vector2(0, 0);
				//remove momentum by setting a finite position (dont know if this actually works)
				transform.position = new Vector3(transform.position.x, transform.position.y, 2f);
			}
			print("char changed away from");

		}
		/// <summary>
        /// test on change
        /// </summary>
		public void StartSelectedCharacter()
        {
			print("ChangeComplete");
        }
		/// <summary>
		/// If the character is in the air, I want to give it a more natural change feeling
		/// </summary>
		/// <returns></returns>
		public IEnumerator StopMovingToChange()
		{
			yield return new WaitForSeconds(.25f);
			velocity = new Vector2(0, 0);
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
					StartCoroutine(CanHangCoolingDown());
					//let go
					isHanging = false; 
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
	//****				rigidBody.bodyType = RigidbodyType2D.Dynamic;
	//****				rigidBody.AddForce(new Vector2(0f, hangingJumpForce), ForceMode2D.Impulse);
					//start jump cool down to prevent double jump
					StartCoroutine(JumpCoolingDown());

					//...and tell the Audio Manager to play the jump audio
					//				AudioManager.PlayJumpAudio();

					return;
				}
			}

			//Otherwise, if currently within the jump time window
			else if (isJumping)
			{
				//and the jump button is held, apply an incremental force to the rigidbody

				//	if (input.jumpHeld)
				//		rigidBody.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);

				//if jump time is past, set isJumping to false
	//			if (jumpTime <= Time.time)
					isJumping = false;
			}
			// will come back to this because the player will need revival after a certain speed

			//If player is falling too fast, reduce the Y velocity to the max
	//		if (rigidBody.velocity.y < maxFallSpeed)
	//			rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
		}

		/// <summary>
		/// Jump cool down coroutine
		/// </summary>
		/// <returns>jumpCollingDown = false</returns>
		IEnumerator JumpCoolingDown()
		{
			jumpCoolingDown = true;
			jumpQuickCoolingDown = true;
			yield return new WaitForSeconds(.25f);
			jumpQuickCoolingDown = false;
			yield return new WaitForSeconds(jumpCoolDownTime);
			jumpCoolingDown = false;
			//Debug.Log("jump Cooldown passed");
			yield return null;
		}

		/// <summary>
		/// Jump cool down coroutine
		/// </summary>
		/// <returns>jumpCollingDown = false</returns>
		IEnumerator DoubleJumpCoolingDown()
		{
			dJumpCoolingDown = true;
			yield return new WaitForSeconds(1.5f);
			dJumpCoolingDown = false;
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
			if (!isOnGround && !isHanging && velocity.y < 0f
				// && !inventory.isHoldingSomething
                 && canHang
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
				//rigidBody.bodyType = RigidbodyType2D.Static;
				//set isHanging to true
				isHanging = true;
			}
		}

		IEnumerator CanHangCoolingDown ()
		{
			canHang = false;
			yield return new WaitForSeconds(.25f);
			canHang = true;
			//Debug.Log("jump Cooldown passed");
			yield return null;

		}
		#endregion

		/// <summary>
		/// Ground Check for character. Casts rays for left and right feet and gives and updated state of character
		/// </summary>
		void CharacterStandingOnSurfaceCheck()
		{
			//assume not on ground
			isOnPlatform = false;
			isOnGround = false;
			//Cast rays for the left and right foot and parse
			RaycastHit2D leftFootCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, .1f, physics.collisionMask);
			RaycastHit2D rightFootCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, .1f, physics.collisionMask);
			//If either ray hit the ground, the player is on the ground
			if (leftFootCheck || rightFootCheck)
				isOnGround = true;

			//isOnPlatform = true;
		}

		public void makeSelected() { isSelected = true; }
		public void deSelect() { isSelected = false; }


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
}
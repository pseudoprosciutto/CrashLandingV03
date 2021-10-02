/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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
		#region Component References: Rigidbody, BoxCollider, Input Handler
		BoxCollider2D bodyCollider;             //The collider component
		Rigidbody2D rigidBody;                  //The rigidbody component
		InputHandler input;                     //The current inputs for the player
		#endregion

		#region Character States


		public bool canHang = true; // can your character even hang dawg?
		public float cantHangCoolDownTime = 1.5f;
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
				coyoteTime = Time.time + coyoteDuration;
		}

		#endregion
		

	}
}
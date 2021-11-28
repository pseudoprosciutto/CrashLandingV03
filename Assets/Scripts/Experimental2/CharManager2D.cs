/* Code By: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
	/// <summary>
    /// Manages the Player Character by modifying character states.
    /// </summary>
	[RequireComponent(typeof(CharPhysics2D))]
	public class CharManager2D : MonoBehaviour
	{
		/** Old Code **/
		public float jumpHeight = 4;
		public float timeToJumpApex = .4f;
		float accelerationTimeAirborne = .2f;
		float accelerationTimeGrounded = .1f;
		float moveSpeed = 6;

		float gravity;
		float jumpVelocity;
		Vector3 velocity;
		float velocityXSmoothing;

		CharPhysics2D physics;


		/**New Code **/
		InputHandler input;


		void Start()
		{
			/** Old Code **/
			physics = GetComponent<CharPhysics2D>();

			gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
			print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);
		}

		void Update()
		{
			/** Old Code **/
			if (physics.collisions.above || physics.collisions.below)
			{
				velocity.y = 0;
			}

			Vector2 directionPressed = new Vector2(input.horizontal, input.vertical);

			if (Input.GetKeyDown(KeyCode.Space) && physics.collisions.below)
			{
				velocity.y = jumpVelocity;
			}

			float targetVelocityX = directionPressed.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (physics.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			velocity.y += gravity * Time.deltaTime;
			physics.Move(velocity * Time.deltaTime);
		}
	}
}
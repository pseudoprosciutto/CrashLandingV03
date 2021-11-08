using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03.Experimental
{
    /// <summary>
	/// Adjusts objects intended displacement based on collision detection from raycasts.
	/// collision happens using layermask
	/// </summary>
    public class CharacterPhysics2D : CharCollisionManager2D
    {





        /// <summary>
        /// move character translation via vector 3 velocity
        /// </summary>
        /// <param name="_velocity">vector 3 velocity</param>
        public void Move(Vector3 _velocity)
        {
            UpdateRaycastOrigin();
            //pass reference to effect change on _velocity
            VerticalCollisions(ref _velocity);
            transform.Translate(_velocity);
        }


        public void Move(Vector2 displacement, Vector2 input)
        {
            ResetDetection();
            objectInput = input;

            if (displacement.y < 0)
            {
                CheckSlopeDescent(ref displacement);
            }

            // Check face direction - done after slope descent in case of sliding down max slope
            if (displacement.x != 0)
            {
                faceDirection = (int)Mathf.Sign(displacement.x);
            }

            CheckHorizontalCollisions(ref displacement);

            if (displacement.y != 0)
            {
                CheckVerticalCollisions(ref displacement);
            }

            transform.Translate(displacement);

            // Reset grounded variables
            if (collisionDirection.below == true)
            {
                forceFall = false;
            }
        }

 


    }
}

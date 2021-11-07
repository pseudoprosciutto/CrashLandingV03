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
    }
}

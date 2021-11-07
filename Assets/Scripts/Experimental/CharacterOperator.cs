/*
 * See for equations/physics: https://en.wikipedia.org/wiki/Equations_of_motion
 * See: http://lolengine.net/blog/2011/12/14/understanding-motion-in-games for Verlet integration vs. Euler
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CL03;


namespace CL03.Experimental
{
    /// <summary>
    /// uses input events to operate character components
    ///
	/// gets player's intended velocity & displacement (caused by enviroment variables + user input which is taken from PlayerInput)
    /// </summary>
//	[RequireComponentInParent (typeof(InputManager))]
	[RequireComponent (typeof(CharacterPhysics2D))]
    public class CharacterOperator : MonoBehaviour
    {
        InputManager input;
        CharacterPhysics2D physics;
        public bool isSelected { get; protected set; }

        float gravity = -20;
        Vector3 velocity;

        /// <summary>
        /// grab components
        /// </summary>
        void Start()
        {
            input = GetComponentInParent<InputManager>();
            physics = GetComponent<CharacterPhysics2D>();
        }

        
        private void Update()
        {
            velocity.y += gravity * Time.deltaTime; //eventually will be gravity on physics
            physics.Move(velocity * Time.deltaTime);
        }



        /// <summary>
        /// This will set isSelected to opposite property
        /// </summary>
        public void SetSelected()
        {
            isSelected = !isSelected;
        }

        /// <summary>
        /// This will set the selected property by passing bool
        /// </summary>
        /// <param name="_">bool isSelected?</param>
        public void SetSelected(bool _)
        {
            isSelected = _;
        }
    }
}

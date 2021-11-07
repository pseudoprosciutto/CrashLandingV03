using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03.Experimental
{ 
    /// <summary>
    /// Handles Character movement
    /// </summary>
    [RequireComponent (typeof (CharacterInputHandler))]
    public class CharacterHandler : MonoBehaviour
    {
        CharacterInputHandler input;
        
        public bool isSelected { get; protected set; }

        float gravity = -20;
        Vector3 velocity;

        /// <summary>
        /// grab components
        /// </summary>
        void Start()
        {
            input = GetComponent<CharacterInputHandler>();
        }

        
        private void Update()
        {
            velocity.y += gravity * Time.deltaTime;
            Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// move character translation via vector 3 velocity
        /// </summary>
        /// <param name="velocity">vector 3 velocity</param>
        public void Move(Vector3 velocity)
        {

            transform.Translate(velocity);
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

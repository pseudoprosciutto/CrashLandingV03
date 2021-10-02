/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// CrashLanding v03 
/// </summary>
namespace CL03
{
    //This needs to go first
    [DefaultExecutionOrder(-100)]

    /// <summary>
    /// Handles input events by caching them into public variables
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        #region Move Events and Values
        [Header("Move Events")]
        public bool crouchPressed;
        public bool crouchHeld;
        public bool jumpPressed;
        public bool jumpHeld;
        [Header("Move Value")]
        public float horizontal;
        public float vertical;
        #endregion

        [Space]

        #region Action Events
        [Header("Action Events")]
        public bool interactPressed;
        public bool interactHeld;
        public bool dropObjectPressed;
        public bool dropObjectHeld;
        public bool changeObjectPressed;
        public bool changeObjectHeld;

        public bool consoleLaunchHeld;
        public bool consoleLaunchPressed;

        public bool action1Pressed;
        public bool action1Held;
        public bool action2Pressed;
        public bool action2Held;
        #endregion

        #region Character Change Events
        [Header("Character Change Events")]
        public int lastCharacterCalled;
        public bool forwardChange;
        public bool backwardChange;
        public bool chooseChar1;
        public bool chooseChar2;
        public bool chooseChar3;
        public bool chooseChar4;
        public bool chooseChar5;
        #endregion

        #region Private Fields
        private Vector2 tempMovement;
        private PlayerControls controls;
        bool readyToClear;                              //Bool used to keep input in sync
        #endregion

        //movement events
        #region BASIC MOVEMENT

        /// <summary>
        /// Directional Movement: WASD keys are hit; Vector2
        /// </summary>
        /// <param name="context">Vector 2</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            tempMovement = context.ReadValue<Vector2>();
            horizontal = tempMovement.x;
            vertical = tempMovement.y;
        }

        /// <summary>
        /// Control Key
        /// </summary>
        /// <param name="context">Button</param>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
                crouchHeld = context.ReadValueAsButton();
            else
                crouchPressed = context.ReadValueAsButton();
        }

        /// <summary>
        /// Space Bar Key
        /// </summary>
        /// <param name="context">Button</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            //           jumpPressed = jumpHeld || Keyboard.current.spaceKey.wasPressedThisFrame;
            if (context.interaction is HoldInteraction)
                jumpHeld = context.ReadValueAsButton();
            else
                jumpPressed = context.ReadValueAsButton();
        }
        #endregion
    }
}
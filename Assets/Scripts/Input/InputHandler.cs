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
        ///this needs to go first
        [DefaultExecutionOrder(-100)]

        /// <summary>
        /// Handles the input events by caching them into public variables to be accessed.
        /// </summary>
        public class InputHandler : MonoBehaviour
        {

            [Header("Basic Move States")]
            public bool crouchPressed;
            public bool crouchHeld;
            public bool jumpPressed;
            public bool jumpHeld;
        public bool moveModifyPressed;
        public bool moveModifyHeld;
            [Header("Move Value")]
            public float horizontal;
            public float vertical;
            [Space]
            [Header("Basic Action States")]
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

            [Header("Character Change")]
            public int lastCharacterCalled;
            public bool forwardChange;
            public bool backwardChange;
            public bool chooseChar1;
            public bool chooseChar2;
            public bool chooseChar3;
            public bool chooseChar4;
            public bool chooseChar5;


            private Vector2 tempMovement;
            private PlayerControls controls;

            bool readyToClear;                              //Bool used to keep input in sync

            #region -Input events-
            //movement event
            #region BASIC MOVEMENT
            public void OnMove(InputAction.CallbackContext context)
            {
                tempMovement = context.ReadValue<Vector2>();
                horizontal = tempMovement.x;
                vertical = tempMovement.y;
            }
        /// <summary>
		/// crouch event pressed or held?
		/// </summary>
		/// <param name="context">input action</param>
            public void OnCrouch(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    crouchHeld = context.ReadValueAsButton();
                else
                    crouchPressed = context.ReadValueAsButton();
            }

        /// <summary>
		/// jump event pressed or held
		/// </summary>
		/// <param name="context">input action</param>
            public void OnJump(InputAction.CallbackContext context)
            {
                //           jumpPressed = jumpHeld || Keyboard.current.spaceKey.wasPressedThisFrame;
                if (context.interaction is HoldInteraction)
                    jumpHeld = context.ReadValueAsButton();
                else
                    jumpPressed = context.ReadValueAsButton();
            }

        /// <summary>
		/// Move /Modify event pressed or held
		/// </summary>
		/// <param name="context">input action</param>
        public void OnMoveModifier(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
                moveModifyHeld = context.ReadValueAsButton();
            else
                moveModifyPressed = context.ReadValueAsButton();
        }
            #endregion

            #region INTERACT AND OBJECT ACTIONS
            public void OnInteract(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    interactHeld = context.ReadValueAsButton();
                else
                    interactPressed = context.ReadValueAsButton();
            }

            public void OnDropObject(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    dropObjectHeld = context.ReadValueAsButton();
                else
                    dropObjectPressed = context.ReadValueAsButton();
            }

            public void OnChangeObject(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    changeObjectHeld = context.ReadValueAsButton();
                else
                    changeObjectPressed = context.ReadValueAsButton();
            }
        public void OnEquipUse(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
                changeObjectHeld = context.ReadValueAsButton();
            else
                changeObjectPressed = context.ReadValueAsButton();
        }
        #endregion

        #region GENERIC ACTIONS
        public void OnAction1(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    action1Held = context.ReadValueAsButton();
                else
                    action1Pressed = context.ReadValueAsButton();
            }
            public void OnAction2(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    action2Held = context.ReadValueAsButton();
                else
                    action2Pressed = context.ReadValueAsButton();
            }

            #endregion
            #region LAUNCH CONSOLE
            public void OnConsoleLaunch(InputAction.CallbackContext context)
            {
                if (context.interaction is HoldInteraction)
                    consoleLaunchHeld = context.ReadValueAsButton();
                else
                    consoleLaunchPressed = context.ReadValueAsButton();
            }
            #endregion

        //this is in switch character controller

            #region CHARACTER CHANGE
            //public void OnChangeForward(InputAction.CallbackContext context)
            //{
            //    forwardChange = context.ReadValueAsButton();
            //}

            //public void OnChangeBackward(InputAction.CallbackContext context)
            //{
            //    backwardChange = context.ReadValueAsButton();
            //}
            //public void OnChar1(InputAction.CallbackContext context)
            //{
            //    chooseChar1= context.ReadValueAsButton();
            //    lastCharacterCalled = 1;
            //}
            //public void OnChar2(InputAction.CallbackContext context)
            //{
            //    chooseChar2 = context.ReadValueAsButton();
            //    lastCharacterCalled = 2;
            //}
            //public void OnChar3(InputAction.CallbackContext context)
            //{
            //    chooseChar3 = context.ReadValueAsButton();
            //    lastCharacterCalled = 3;
            //}
            //public void OnChar4(InputAction.CallbackContext context)
            //{
            //    chooseChar4 = context.ReadValueAsButton();
            //    lastCharacterCalled = 4;
            //}
            //public void OnChar5(InputAction.CallbackContext context)
            //{
            //    chooseChar5 = context.ReadValueAsButton();
            //    lastCharacterCalled = 5;
            //}

            #endregion



            #endregion
            private void Awake()
            {
                controls = new PlayerControls();
            }
            private void OnDisable()
            {
                controls.Disable();
            }
            public void OnEnable()
            {
                if (controls == null)
                {
                    controls = new PlayerControls();
                    // Tell the "gameplay" action map that we want to get told about
                    // when actions get triggered.
                    //                controls.Player.SetCallbacks(this);
                }
                controls.Enable();
            }

            public void ClearForCharChange()
            {
                vertical = 0f;
                horizontal = 0f;
                jumpPressed = false;
                jumpHeld = false;
                crouchPressed = false;
                crouchHeld = false;
                moveModifyHeld = false;
                moveModifyPressed = false;

                readyToClear = false;

                interactPressed = false;
                interactHeld = false;
            }


            void Update()
            {

                horizontal = tempMovement.x;
                vertical = tempMovement.y;


                // crouchPressed = crouchPressed;// || Input.GetButtonDown("Crouch");
                // crouchHeld = crouchHeld;// || Input.GetButton("Crouch");
                //Process keyboard, mouse, gamepad (etc) inputs
                // ProcessInputs();
            }

            /**
            void Update()
                {
                    horizontal = tempMovement.x;
                    vertical = tempMovement.y;
                   // crouchPressed = crouchPressed;// || Input.GetButtonDown("Crouch");
                   // crouchHeld = crouchHeld;// || Input.GetButton("Crouch");
                    //Process keyboard, mouse, gamepad (etc) inputs
                    // ProcessInputs();
             * }
            void ProcessInputs()
            {
                tempMovement = controls.Player.Move.ReadValue<Vector2>();
                horizontal = tempMovement.x;
                horizontal = tempMovement.y;

                //Accumulate button inputs
                jumpPressed = jumpPressed || Input.GetButtonDown("Jump");
                jumpHeld = jumpHeld || Input.GetButton("Jump");

                crouchPressed = crouchPressed || Input.GetButtonDown("Crouch");
                crouchHeld = crouchHeld || Input.GetButton("Crouch");
            }
            void FixedUpdate()
            {
                //In FixedUpdate() we set a flag that lets inputs to be cleared out during the 
                //next Update(). This ensures that all code gets to use the current inputs
                readyToClear = true;
            }

            void ClearInput()
            {
                //If we're not ready to clear input, exit
                if (!readyToClear)
                    return;

                //Rpublicll inputs
                horizontal = 0f;
                jumpPressed = false;
                jumpHeld = false;
                crouchPressed = false;
                crouchHeld = false;

                readyToClear = false;
            }
            */
        }
    }

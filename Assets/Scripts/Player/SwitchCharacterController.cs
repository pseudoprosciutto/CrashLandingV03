/* Code by: Matthew Sheehan */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CL03
{
    /// <summary>
    /// Assigns and manages which character is specifically controlled by player
    /// and switches characters upon input.
    ///
    /// canLookForInput guards ability to process input to change character.
    /// When a character is changed it sends a message to PlayerCharacter class to clear/stop input queue
    /// 
    /// Characters must be loaded and currentChar must be assigned for control to be given to characters.
    /// Disabling will not allow a character to be used.
    /// 
    /// </summary>

    public class SwitchCharacterController : MonoBehaviour
    {
        [SerializeField] bool canLookForInput;
        [SerializeField] CameraController CamControl;
        public GameObject[] Characters;

        public GameObject currentChar;

        [SerializeField] int selectedCharNumber;
        InputHandler input;
        // [SerializeField]  public GameObject[] CharIcons;
        // [SerializeField]    GameObject CharSelectCursor;
        private void Awake()
        {
            input = GetComponent<InputHandler>();
            canLookForInput = true;
            //this is where we gather all the ships and cache them
            for (int i = 0; i < Characters.Length; i++)
            {
                //if we can get all the characters lets make sure they are enabled.
                if (Characters[i].TryGetComponent(out CharacterEngine _charControl))
                    _charControl.enabled = true;
                else
                    _charControl.GetComponent<CharacterEngine>().enabled = false;

                //GameObject Icon = ShipIcons[i].gameObject;
            }
            selectedCharNumber = 0; //this will adjust accordingly
                                    // CharSelectCursor = CharIcons[selectedCharNumber];
                                    // CharIcons[selectedCharNumber].SendMessage("showSelector");

        }
        private void OnEnable()
        {
            canLookForInput = true;

        }
        private void OnDisable()
        {
            canLookForInput = false;
        }
        // Start is called before the first frame update
        void Start()
        {
            currentChar = Characters[0]; //first loaded
            CharacterEngine firstCharacter = currentChar.GetComponent<CharacterEngine>();
            firstCharacter.isSelected = true;
            firstCharacter.bringFront();
            //  ChangeChar(Characters[0]);
        }
        // Update is called once per frame
        void Update()
        {
            if (!canLookForInput) return;

            LookForInputToChangeChar(); //specific input to find ship
        }

        #region Character Change Input
        /** A TEST USING EVENT CALLS
        /// <summary>
        /// This was an experiment using event calls for character selection instead.
        /// If I were to call this as an event, as coded, it only acts temporarily while the key is pressed
        /// and then returns back to original selection. 
        /// </summary>
        /// <param name="context"></param>
        //  public void OnBackSelectTest(InputAction.CallbackContext context)
        //     {
        //         //      CharSelectCursor.SendMessage("removeSelector");
        //    selectedCharNumber--;
        //    int selectChar = Mathf.Abs(selectedCharNumber % Characters.Length);
        //    ChangeChar(Characters[selectChar]);
        //    //     CharSelectCursor = CharIcons[selectChar];
        //    //     CharSelectCursor.SendMessage("showSelector");
        //} */

        /// <summary>
        /// Instead of calling events, I tried this way since I only needed one single press once. I hope this holds up.
        /// </summary>
        private void LookForInputToChangeChar()
        {

            //rotate forwards through char choices
            if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.periodKey.wasPressedThisFrame)
            {
                //     CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber++;
                int selectChar = Mathf.Abs(selectedCharNumber % Characters.Length);
                ChangeChar(Characters[selectChar]);
                //     CharSelectCursor = CharIcons[selectChar];
                //     CharSelectCursor.SendMessage("showSelector");
            }

            //rotate backwards through char choices
            if (Keyboard.current.backquoteKey.wasPressedThisFrame || Keyboard.current.commaKey.wasPressedThisFrame)
            {
                //      CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber--;
                int selectChar = Mathf.Abs(selectedCharNumber % Characters.Length);
                ChangeChar(Characters[selectChar]);
                //     CharSelectCursor = CharIcons[selectChar];
                //     CharSelectCursor.SendMessage("showSelector");
            }

            //choose char 1
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                int askForChar = 0;
                if (askForChar >= Characters.Length) return;
                //    CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber = askForChar;
                ChangeChar(Characters[selectedCharNumber]);
                //    CharSelectCursor = CharIcons[selectedCharNumber];
                //    CharSelectCursor.SendMessage("showSelector");
            }
            //choose char 2
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                int askForChar = 1;
                if (askForChar >= Characters.Length) return;
                //    CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber = askForChar;
                ChangeChar(Characters[selectedCharNumber]);
                //    CharSelectCursor = CharIcons[selectedCharNumber];
                //    CharSelectCursor.SendMessage("showSelector");
            }
            //choose char 3
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                int askForChar = 2;
                if (askForChar >= Characters.Length) return;
                //     CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber = askForChar;
                ChangeChar(Characters[selectedCharNumber]);
                //     CharSelectCursor = CharIcons[selectedCharNumber];
                //     CharSelectCursor.SendMessage("showSelector");
            }
            //choose char 4
            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                int askForChar = 3;
                if (askForChar >= Characters.Length) return;
                //    CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber = askForChar;
                ChangeChar(Characters[selectedCharNumber]);
                //    CharSelectCursor = CharIcons[selectedCharNumber];
                //    CharSelectCursor.SendMessage("showSelector");
            }
            //choose char 5
            if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                int askForChar = 4;
                if (askForChar >= Characters.Length) return;
                //    CharSelectCursor.SendMessage("removeSelector");
                selectedCharNumber = askForChar;
                ChangeChar(Characters[selectedCharNumber]);
                //    CharSelectCursor = CharIcons[selectedCharNumber];
                //    CharSelectCursor.SendMessage("showSelector");
            }
        }
        #endregion

        #region Character Change
        /// <summary>
        /// Enables new selected character and disables old   theres some redundancy in code but it works 
        /// </summary>
        /// <param name="newSelectedChar"></param>
        private void ChangeChar(GameObject newSelectedChar)
        {
            //            currentChar.transform.position = new Vector3(currentChar.transform.position.x, currentChar.transform.position.y, 2f);

            //clear the input command events.
            input.ClearForCharChange();

            //           currentChar.transform.position = new Vector3(currentChar.transform.position.x, currentChar.transform.position.y, 2f);

            //    currentChar.SendMessage("stopMovingToChange");
            //            currentChar.SendMessage("stopCoroutineChange");
            var oldChar = currentChar.GetComponent<CharacterEngine>();
            input.ClearForCharChange();
            oldChar.StopMovingChangingChar();
            oldChar.sendBack();

            oldChar.isSelected = false;
//           if (!oldChar.isHanging) { oldChar.EnterStaticState(); }
            oldChar.OnCharacterChange_End();

            // modified clear input queue for char. version eventually which allows certain states to persist
            // prevents any new info since last clear. completely redundant.
            //  oldChar.input.ClearForCharChange();

            //replace current character cache with new selected character
            currentChar = newSelectedChar;
            //            currentChar.transform.position = new Vector3(currentChar.transform.position.x, currentChar.transform.position.y, 1f);
            CamControl.ChangeFollow(currentChar);
            var newChar = currentChar.GetComponent<CharacterEngine>();
            newChar.OnCharacterChange_Start();
            newChar.isSelected = true;
            if (!newChar.isHanging) { newChar.ExitStaticState(); }
            newChar.ForceUnFreezeConstraints();

            newChar.bringFront();
            //       print("remove successful.");
            //currentChar.transform.position = new Vector3(currentChar.transform.position.x, currentChar.transform.position.y,0f);
        }
        #endregion
    }
}

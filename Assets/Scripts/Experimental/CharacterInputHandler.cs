using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03.Experimental
{
    /// <summary>
    /// Manages Input system data if character is selected and handler says yes.
    /// </summary>
    [RequireComponent (typeof (CharacterHandler))]
    public class CharacterInputHandler : MonoBehaviour
    {

        CharacterHandler character;


        void Start()
        {
        character = GetComponent<CharacterHandler>();
        }

        void Update()
        {
            if (character.isSelected)
            {

            }
        }
    }
}

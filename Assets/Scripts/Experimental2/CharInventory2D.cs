using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CL03
{
    /// <summary>
    /// The inventory system for the characters.
    /// Crashlanding only has one inventory space for each character
    /// </summary>
    public class CharInventory2D : MonoBehaviour
    {
        CharManager2D character;
        CharInteract2D interact;
        Inventory inv;


        [GUIColor(0.3f, 0.3f, 0.8f, .2f)]
        [PreviewField]
        public GameObject objectBeingHeld = null; // { get; set { if(isHoldingSomething)return } }

        [Title("Inventory Item", "if null then nothing is stored for this character.", TitleAlignments.Centered)]
        [GUIColor(0.3f, 0.8f, 0.8f, .2f)]
        [PreviewField]
        public GameObject inventoryItem = null;
        //[Title("Equipment Type:")]
        //public 

        [Required]
        [ChildGameObjectsOnly]
        public Transform holdPoint_Front;  //the front location for generic object being held (maybe will need to change pivot point of object depending on sizes)
        [Required]
        [ChildGameObjectsOnly]
        public Transform holdPoint_Above;  //the above location for generic object being held

        [Header("Debugging to Start")]
        [SerializeField]
        GameObject GMHolding;
        [SerializeField]
        GameObject GMInventory;
        [SerializeField]
        GameObject GMTransition;

        public float dropItemCoolDownTime = 1f;
        public bool isDroppingItemCoolDown;

        public bool isHoldingSomething;
        public bool isHoldingSomethingAbove;
        public float changeItemCoolDownTime = .7f;
        public bool changeObjectCoolingDown;  //is object cooling down?

        // Start is called before the first frame update
        void Start()
        {

            // to be gotten from game manager through character
            // or directly loaded into character from debugger engine for
            // when scenes change and debugging this option they
            // load characters current item
            GameObject _GMHolding = GMHolding;
            GameObject _GMInventory = GMInventory;
            GameObject _GMTransition = GMTransition;
            Inventory inv = new Inventory(_GMHolding, _GMInventory, _GMTransition);

        }


        /// <summary>
        /// Each character has their own inventory.
        /// Holding space in hands
        /// Inventory space in bag
        /// Transition space inbetween
        /// </summary>
        struct Inventory
        {
            public Inventory(
                GameObject _holding,
                GameObject _inventory,
                GameObject _transition)
            {
                this.holding = _holding;
                this.inventory = _inventory;
                this.transition = _transition;
            }

            public GameObject holding;
            public GameObject inventory;
            public GameObject transition;

        }
    }
}
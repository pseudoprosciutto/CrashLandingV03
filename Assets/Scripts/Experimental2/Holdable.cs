using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CL03
{
    /// <summary>
    /// Interactable Objects which can be held.
    /// 
    /// Holdable Objects properties:
    /// isHeld state, - Can be held by character;
    /// heldby know who is holding it if any character;
    /// cooldown useage;
    /// isThrowable;
    /// isOnGround;
    /// (maybe?) isPushable; (maybe will be under heavycrate specific?)
    /// PushorPull state; (maybe will be under heavycrate specific
    /// </summary>
    public class Holdable : Interactable
    {
        //held properties
        [SerializeField]
        protected bool canBeHeld;
        [SerializeField]
        protected ItemType itemType = ItemType.Generic;
        public bool canBeStored = false; //default false
        public bool isCrate = false;
        [SerializeField]
        [ReadOnly]
        protected bool isHeld;

        [SerializeField]
        protected GameObject HeldBy; //who is holding this object?
        protected CharManager2D HeldBy_Char; //engine of object being held
        protected CharInventory2D HeldBy_Inventory; //inventory of object being held.

        [Space]
        public bool normallyThrowable = true; //ussually what an object is throwable
        [SerializeField]
        protected bool isThrowable;  /* throwing an object is currently seen as
                                      * if yes when normally is no, means its 
                                      * super heavy and need special ability to 
                                      * throw object. if no but normally is yes
                                      * then we dont throw it at all right now.
        */

        //inventory propertes

        public bool isInHands { get; protected set; } = false; //default false


        [SerializeField]
        [ReadOnly]
        [Space]
        protected bool isInInventory;

        [SerializeField]
        public bool useableInInventory = false;

        //protected Rigidbody2D rb;
        protected Collider2D objectCollider;
        protected SpriteRenderer spriteRenderer;

        protected int objectDirection = 1;
        protected float originalXScale;
        //protect set because we dont want outside changing this.

        protected bool _freezeRotation = false; //when held object must be still.

        public float gravity = 1f;
        public float objectMass;

        public bool isStaticCoolingDown = false;
        public float staticCoolDownTime = .1f;

        protected bool isOnGround; //where is this object?

        public bool isColliding;
        //public bool isPushable;
        //public bool inPushOrPullState;
        bool delayBegan = false;



        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

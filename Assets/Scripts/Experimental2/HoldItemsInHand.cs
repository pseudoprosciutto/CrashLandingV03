using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CL03
{
    public struct Inventory
    {
        public GameObject InventoryItem;
    }
    //Allows character to hold onto an object and carry it with them
    /// <summary>
    /// Looks for Input Drop Throw Inventory Swap and Inventory Use actions
    /// and will handle items while on character
    /// </summary>
    public class HoldItemsInHand : MonoBehaviour
    {
        [GUIColor(0.3f, 0.3f, 0.8f, .2f)]
        [PreviewField]
        public GameObject heldObject;
        public bool isHolding;
        public float PickUpSeconds =.05f;
        Inventory inventory;

        [Required]
        [ChildGameObjectsOnly]
        public Transform holdPoint_Front;  //the front location for generic object being held (maybe will need to change pivot point of object depending on sizes)
        [Required]
        [ChildGameObjectsOnly]
        public Transform holdPoint_Above;  //the above location for generic object being held

        /// <summary>
        /// This is sent from the interact system and already assuming there is nothing currently in hand
        /// </summary>
        /// <param name="holdable"></param>
        public void PickUpItem(HoldableR holdable)
        {
            if (isHolding) return;

            isHolding = true;
            heldObject = holdable.gameObject;
            heldObject.transform.SetParent(holdPoint_Front,true);
            StartCoroutine(MoveOverSeconds(heldObject.gameObject, holdPoint_Front.transform.position, PickUpSeconds));

            print("Hands are Holding Item!");
            //will need to set sprite to show holding object in front

        }

        public void ThrowItem(HoldableR holdable)
        {
            if (!isHolding) return;
            print("Throwing Item");
        }

        public void PlaceItemInInventory(HoldableR holdable)
        {
            if (holdable.canBeStored)
            {

                GameObject temp = inventory.InventoryItem;
                inventory.InventoryItem = holdable.gameObject;
                heldObject = temp;

            }
        }

        public void LetGoOfItem(HoldableR hodlable)
        {

        }


        
        //move something over time.
  public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
        {
            // speed should be 1 unit per second
            while (objectToMove.transform.position != end)
            {
                objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
  public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
        {
            float elapsedTime = 0;
            Vector3 startingPos = objectToMove.transform.position;
            while (elapsedTime < seconds)
            {
                objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            objectToMove.transform.position = end;
            setTransformToParent(objectToMove);
        }
        void setTransformToParent(GameObject objectMoving)
        {
            heldObject.transform.SetParent(holdPoint_Front, true);

        }
    }
}

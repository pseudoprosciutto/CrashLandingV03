using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class HiringSpot : MonoBehaviour
    {
        bool interactedwith;
        BoxCollider2D boxCollider;

        public bool isHiring = true; //can character get quest.
        public bool isActive = false; //is character doing quest.

        /// <summary>
        /// player colides   Will be a key command later
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && isHiring)
            {

                StartCoroutine(InteractWith());

            }

        }
        /// <summary>
        /// start interact cool down
        /// </summary>
        /// <returns></returns>
        IEnumerator InteractWith()
        {
            interactedwith = true;
            print("interacting");
            interact();
            yield return new WaitForSeconds(.7f);
            interactedwith = false;
        }

        /// <summary>
        /// character Interacted with this object
        /// </summary>
        public void interact()
        {
            if (isHiring)
            {
                if (QuestManager.QM.QuestsFull)
                {
                    print("Quest Log Full, cant add quest;");
                    return;
                }
                isHiring = false;
                isActive = true;
                ///will have a randomizer to create quest type but not important right now
                QuestManager.QM.AddQuest(new Quest(QuestType.Easy));
            }
            else
            {
                print("Not Hiring");
            }
        }

    }
}
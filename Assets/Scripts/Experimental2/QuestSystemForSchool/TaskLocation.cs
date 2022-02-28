using System.Collections;
using System.Collections.Generic;
using CL03;
using UnityEngine;
namespace CL03
{
    /// <summary>
    /// A location made active
    /// if location is active then it can not be used for another quest until it is deactivated;
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class TaskLocation : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        public bool isActive = false;
        Quest questParent;
        Task task;

        private void Awake()
        {
            spriteRenderer.enabled = false;
        }

        public void LoadTask(Task newTask)
        {
            if (isActive) { return; }
            task = newTask;
            EnableTask();

        }

        public void EnableTask()
        {
            spriteRenderer.enabled = true;
            isActive = true;

            print("taskActivated");
        }

        public void DisableTask()
        {
            spriteRenderer.enabled = false;
            isActive = false;

            print("taskDectivated");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && isActive)
            {
                DisableTask();
                print("collided with location");

            }

        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{

    /// <summary>
    /// Laser control that can delete itself after its short pitiful life if it hasnt had an untimely death already.
    /// </summary>
    //public delegate void OutOfBoundsHandler(); //set up a delegate to access other methods.
    public class LaserController : MonoBehaviour
    {
        #region Field Declarations
        public int projectileDirection;
        public float projectileSpeed = 3f;
        public bool isPlayers;
        public LayerMask Stoppables;

        [SerializeField] float lifeLengthInSeconds = 2f;
        [SerializeField] float ScreenWidth = 7f;
        [SerializeField] float buffer = .1f;

        Rigidbody2D rb;
        #endregion
        void Awake() => StartCoroutine(lifespan());
        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            MoveProjectile();
        }
        #region Expressions
        // Update is called once per frame
        void Update() => MoveProjectile();
        void destroySelf() => Destroy(gameObject);
        #endregion

        #region Movement
        /// <summary>
        /// dont be silly, send it
        /// </summary>
        private void MoveProjectile()
        {
            rb.velocity = Vector2.left * projectileSpeed * projectileDirection;
            //  CheckBounds(); //this is now timed death.
        }
        public void explode()
        {
            Debug.Log("Bullet Explosion Animation");
        }

        //currently just destroys until ready to figure out object pooling it
        IEnumerator lifespan()
        {
            yield return new WaitForSeconds(lifeLengthInSeconds);
            destroySelf();
            yield break;
        }
        #endregion

        void OnTriggerEnter2D(Collider2D hitInfo)
        {
            switch (hitInfo.gameObject.layer)
            {
                case 6:
                    print("hit ground layer 6");
                Destroy(gameObject);
                    break;

                default:
                    break;
            }
                /* 
                       Enemy enemy = hitInfo.GetComponent<Enemy>();
                       if (enemy != null)
                       {
                           enemy.TakeDamage(damage);
                       }
                */
                // Instantiate(impactEffect, transform.position, transform.rotation);
               // or explode();
            if (hitInfo.tag == "Character" )
            {
                print("Hit Player brah");
                Destroy(gameObject);
            }

        }
    }
}
        //private void CheckBounds()
        //{
        //    Vector2 PlayerPosition = transform.position;
                
        //}
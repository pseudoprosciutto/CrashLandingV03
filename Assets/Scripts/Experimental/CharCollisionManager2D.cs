using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03.Experimental
{

    /// <summary>
    /// Raycast detection. Char Collision Manager 2d casts rays about like it aint nothing.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharCollisionManager2D : MonoBehaviour
    {
        public LayerMask collisionMask;
        const float skinWidth = .015f;
        //how many rays
        [SerializeField]
        int horizontalRayCount = 4;
        int verticalRayCount = 4;
        //space between
        float horizontalRaySpacing;
        float verticalRaySpacing;

        BoxCollider2D collider;
        RaycastOrigins raycastOrigins;


        /// <summary>
        /// Store Corner of BoxCollider
        /// </summary>
        struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        void Start()
        {
            collider = GetComponent<BoxCollider2D>();
            CalculateRaySpacing();
        }




        /// <summary>
        /// Gets the current bounds of character collider minus skin width
        /// </summary>
        /// <returns>boundsMinusSkinWidth</returns>
        private Bounds GetBounds()
        {
            //get bounds of collider
            Bounds bounds = collider.bounds;
            //shrink bounds so inset by skinwidth
            bounds.Expand(skinWidth * -2);
            return bounds;
        }

        /// <summary>
        /// Get Corner of Raycasts
        /// </summary>
       protected void UpdateRaycastOrigin()
        {
            Bounds bounds = GetBounds();  //this looks nicer than putting method in each vector2 assignment below.
            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }


        /// <summary>
        /// Calculate and Set Count and Spacing of and between ray casts
        /// </summary>
       protected void CalculateRaySpacing() 
        {
            Bounds bounds = GetBounds();

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }


        protected void VerticalCollisions(ref Vector3 velocity)
        {
                float directionY = MathF.Sign(velocity.y); // set negative or positive
                float rayLength = MathF.Abs(velocity.y) + skinWidth;
            for (int i = 0; i < verticalRayCount; i++)
            {
                //where do rays start if moving    - Maybe will not depend on moving and always cast rays. tbd
                //ray origin equals bottom left if direction equals -1 (moving down) otherwise its top left (moving up and else case)
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                RaycastHit2d hit = CharacterPhysics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
                Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);
                //if raycast hits,
                if (hit)
                {
                    // set y velocity to move toward distance.
                    velocity.y = (hit.distance - skinWidth) * directionY;
                    //shorten distance to prevent clipping
                        rayLength.hit.distance;
                }
            }
        }


    }
}

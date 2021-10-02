using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
namespace CL03
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        Cinemachine.CinemachineVirtualCamera Cam;
        //[SerializeField] Cinemachine.CinemachineVirtualCamera[] Cams;
        GameObject CurrentChar;

        private void Awake()
        {

        }

        public void ChangeFollow(GameObject Character)
        {
            CurrentChar = Character;
            Cam.Follow = CurrentChar.transform;
        }
    }
}
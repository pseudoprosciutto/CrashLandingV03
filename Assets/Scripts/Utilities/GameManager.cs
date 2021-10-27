/* Code by: Matthew Sheehan */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace CL03
{
	/// <summary>
    /// This class or another static class will hold the values for the 
	/// characters base and with equipment modifications so the class will look for what its value is in the right position
    /// </summary>
	public class GameManager : Singleton<GameManager>
	{

		static public GameManager GM;
        int numberOfDeaths;                         //Number of times player has died
		float totalGameTime;                        //Length of the total game time

		private DateTime _sessionStartTime;
		private DateTime _sessionEndTime;

		bool isGameOver;                            //Is the game currently over?


        void Start()
        {
			//TODO:
			// - Load player save
			// - if no save, redirect player to main menu scene
			_sessionStartTime = DateTime.Now;
			Debug.Log("Game session start @: " + DateTime.Now);
		}


        //		public float deathSequenceDuration = 1.5f;  //How long player death takes before restarting

        //		List<Orb> orbs;                             //The collection of scene orbs
        //		Door lockedDoor;                            //The scene door
        //		SceneFader sceneFader;                      //The scene fader

		void Update()
		{
			//If the game is over, exit
			if (isGameOver)
				return;

			//Update the total game time and tell the UI Manager to update
			totalGameTime += Time.deltaTime;


//			UIManager.UpdateTimeUI(totalGameTime);
		}

		void OnApplicationQuit()
		{
			_sessionEndTime = DateTime.Now;
			TimeSpan timeDifference =
			_sessionEndTime.Subtract(_sessionStartTime);
			Debug.Log(
			"Game session ended @: " + DateTime.Now);
			Debug.Log(
			"Game session lasted: " + timeDifference);
		}
		void OnGUI()
		{
			if (GUILayout.Button("Next Scene"))
			{
				SceneManager.LoadScene(
				SceneManager.GetActiveScene().buildIndex + 1);
			}
		}


		public static bool IsGameOver()
		{
			//If there is no current Game Manager, return false as there is no game
			if (GM == null)
				return false;

			//Return the state of the game
			return GM.isGameOver;
		}

		public static void PlayerWon()
		{
			//If there is no current Game Manager, exit
			if (GM == null)
				return;

			//The game is now over
			GM.isGameOver = true;

			//Tell UI Manager to show the game over text and tell the Audio Manager to play
			//game over audio
//			UIManager.DisplayGameOverText();
//			AudioManager.PlayWonAudio();
		}

		void RestartScene()
		{
			//Play the scene restart audio
//			AudioManager.PlaySceneRestartAudio();

			//Reload the current scene
//			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}
}


		//public static void RegisterSceneFader(SceneFader fader)
		//{
		//	//If there is no current Game Manager, exit
		//	if (current == null)
		//		return;

		//	//Record the scene fader reference
		//	current.sceneFader = fader;
		//}

//		public static void RegisterDoor(Door door)
//		{
//			//If there is no current Game Manager, exit
//			if (current == null)
//				return;

//			//Record the door reference
//			current.lockedDoor = door;
//		}

//		public static void RegisterOrb(Orb orb)
//		{
//			//If there is no current Game Manager, exit
//			if (current == null)
//				return;

//			//If the orb collection doesn't already contain this orb, add it
//			if (!current.orbs.Contains(orb))
//				current.orbs.Add(orb);

//			//Tell the UIManager to update the orb text
////			UIManager.UpdateOrbUI(current.orbs.Count);
//		}

//		public static void PlayerGrabbedOrb(Orb orb)
//		{
//			//If there is no current Game Manager, exit
//			if (current == null)
//				return;

//			//If the orbs collection doesn't have this orb, exit
//			if (!current.orbs.Contains(orb))
//				return;

//			//Remove the collected orb
//			current.orbs.Remove(orb);

//			//If there are no more orbs, tell the door to open
//			if (current.orbs.Count == 0)
//				current.lockedDoor.Open();

//			//Tell the UIManager to update the orb text
////			UIManager.UpdateOrbUI(current.orbs.Count);
//		}

//		public static void PlayerDied()
//		{
//			//If there is no current Game Manager, exit
//			if (current == null)
//				return;

//			//Increment the number of player deaths and tell the UIManager
//			current.numberOfDeaths++;
////			UIManager.UpdateDeathUI(current.numberOfDeaths);

//			//If we have a scene fader, tell it to fade the scene out
//			if (current.sceneFader != null)
//				current.sceneFader.FadeSceneOut();

//			//Invoke the RestartScene() method after a delay
//			current.Invoke("RestartScene", current.deathSequenceDuration);
//		}
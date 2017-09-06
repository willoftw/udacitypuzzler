﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour {


	public GameObject player;
	public GameObject eventSystem;
	public GameObject startUI, restartUI;
	public GameObject startPoint, playPoint, restartPoint,entryPoint;
	public GameObject[] puzzleSpheres; //An array to hold our puzzle spheres

	public int puzzleLength = 5; //How many times we light up.  This is the difficulty factor.  The longer it is the more you have to memorize in-game.
	public float puzzleSpeed = 1f; //How many seconds between puzzle display pulses
	private int[] puzzleOrder; //For now let's have 5 orbs

	private int currentDisplayIndex = 0; //Temporary variable for storing the index when displaying the pattern
	public bool currentlyDisplayingPattern = true;
	public bool playerWon = false;

	private int currentSolveIndex = 0; //Temporary variable for storing the index that the player is solving for in the pattern.
	public GameObject failAudioHolder;
	public GameObject FailText; 
	public GameObject DoorCover;

	// Use this for initialization
	void Start () {
		puzzleOrder = new int[puzzleLength]; //Set the size of our array to the declared puzzle length
		generatePuzzleSequence (); //Generate the puzzle sequence for this playthrough.  
	}

	// Update is called once per frame
	void Update () {

	}


	public void playerSelection(GameObject sphere) {
		if(playerWon != true) { //If the player hasn't won yet
			int selectedIndex=0;
			Debug.Log (sphere.ToString());
			//Get the index of the selected object
			for (int i = 0; i < puzzleSpheres.Length; i++) { //Go through the puzzlespheres array
				if(puzzleSpheres[i] == sphere) { //If the object we have matches this index, we're good
					Debug.Log("Looks like we hit sphere: " + i);
					selectedIndex = i;
				}
			}
			solutionCheck (selectedIndex);//Check if it's correct
		}
	}

	public void solutionCheck(int playerSelectionIndex) { //We check whether or not the passed index matches the solution index
		if (playerSelectionIndex == puzzleOrder [currentSolveIndex]) { //Check if the index of the object the player passed is the same as the current solve index in our solution array
			currentSolveIndex++;
			Debug.Log ("Correct!  You've solved " + currentSolveIndex + " out of " + puzzleLength);
			if (currentSolveIndex >= puzzleLength) {
				puzzleSuccess ();
			}
		} else {
			puzzleFailure ();
		}

	}



	public void startPuzzle() { //Begin the puzzle sequence (button press)
		hideAllUI();
		FailText.SetActive (false);

		Debug.Log ("starting puzzle");

		Move(new List<Vector3> { entryPoint.transform.position, playPoint.transform.position});

//		CancelInvoke ("displayPattern");
//		InvokeRepeating("displayPattern", 1, puzzleSpeed); //Start running through the displaypattern function
		currentSolveIndex = 0; //Set our puzzle index at 0

	}
		

	void displayPattern() { //Invoked repeating.
		currentlyDisplayingPattern = true; //Let us know were displaying the pattern
		eventSystem.SetActive(false); //Disable gaze input events while we are displaying the pattern.

		if (currentlyDisplayingPattern == true) { //If we are not finished displaying the pattern
			if (currentDisplayIndex < puzzleOrder.Length) { //If we haven't reached the end of the puzzle
				Debug.Log (puzzleOrder[currentDisplayIndex] + " at index: " + currentDisplayIndex); 
				puzzleSpheres [puzzleOrder [currentDisplayIndex]].GetComponent<lightUp> ().patternLightUp (puzzleSpeed); //Light up the sphere at the proper index.  For now we keep it lit up the same amount of time as our interval, but could adjust this to be less.
				currentDisplayIndex++; //Move one further up.
			} else {
				Debug.Log ("End of puzzle display");
				currentlyDisplayingPattern = false; //Let us know were done displaying the pattern
				currentDisplayIndex = 0;
				CancelInvoke(); //Stop the pattern display.  May be better to use coroutines for this but oh well
				eventSystem.SetActive(true); //Enable gaze input when we aren't displaying the pattern.
			}
		}
	}


	public void generatePuzzleSequence() {

		int tempReference;
		for (int i = 0; i < puzzleLength; i++) { //Do this as many times as necessary for puzzle length
			tempReference = Random.Range(0, puzzleSpheres.Length); //Generate a random reference number for our puzzle spheres
			puzzleOrder [i] = tempReference; //Set the current index to our randomly generated reference number
		}
	}


	public void resetPuzzle() { //Reset the puzzle sequence for on faliure
		playerWon = false;
		FailText.SetActive (false);
		//player.transform.position = startPoint.transform.position;
		//resetGame();
	}


	public void resetGame() { // reset the entire game for on complete (button press)
		Debug.Log ("Resetting Game: " + DoorCover.transform.position.y);
		playerWon = false;
		hideAllUI();
		showStartUI();
		closeDoor ();
		iTween.MoveTo (player, 
			iTween.Hash (
				"position", startPoint.transform.position, 
				"time", 1, 
				"easetype", "linear"
			)
		);
		generatePuzzleSequence (); //Generate the puzzle sequence for this playthrough.  
	}

	public void puzzleFailure() { //Do this when the player gets it wrong
		Debug.Log("You've Failed, Resetting puzzle");
		FailText.SetActive (true);
		failAudioHolder.GetComponent<GvrAudioSource>().Play();
		Move(new List<Vector3> { entryPoint.transform.position, playPoint.transform.position});

	}
		
	public void puzzleSuccess() { //Do this when the player gets it right
		showFinishUI();
		openDoor();
		iTween.MoveTo (player, 
			iTween.Hash (
				"position", restartPoint.transform.position, 
				"time", 3, 
				"easetype", "linear",
				"oncomplete", "finishingFlourish", 
				"oncompletetarget", this.gameObject
			)
		);
	}

	public void showStartUI() {
		startUI.SetActive (true);
		restartUI.SetActive (false);
	}

	public void hideAllUI() {
		startUI.SetActive (false);
		restartUI.SetActive (false);
	}

	public void showFinishUI() {
		startUI.SetActive (false);
		restartUI.SetActive (true);
	}

	void openDoor()
	{
		iTween.MoveTo (DoorCover, 
			iTween.Hash (
				"position", new Vector3(DoorCover.transform.position.x,DoorCover.transform.position.y+4,DoorCover.transform.position.z), 
				"time", 1, 
				"easetype", "linear",
				"oncomplete", "finishingFlourish"
			)
		);
	}

	void closeDoor()
	{
		iTween.MoveTo (DoorCover, 
			iTween.Hash (
				"position", new Vector3(DoorCover.transform.position.x,DoorCover.transform.position.y-4,DoorCover.transform.position.z), 
				"time", 1, 
				"easetype", "linear",
				"oncomplete", "finishingFlourish"
			)
		);
	}

	public float speed = 5f;

	public void Move(List<Vector3> destinations) {
		var executeOnMoveFinishes = new List<System.Action>();

		for (var i = 0; i < destinations.Count; i++) {
			var a = i;
			executeOnMoveFinishes.Add(() => {
				var moveParams = iTween.Hash("easetype", "linear", "speed", speed, "position", destinations[a],
					"oncomplete", "moveComplete", "oncompletetarget", this.gameObject, "oncompleteparams", executeOnMoveFinishes);
				iTween.MoveTo(player, moveParams);
			});
		}

		moveComplete(executeOnMoveFinishes);
	}

	void moveComplete(List<System.Action> actions) {
		if (actions.Count == 0) {
			FailText.SetActive (false);

			//displayPattern();
			playerWon = false;
			CancelInvoke ("displayPattern");
			InvokeRepeating("displayPattern", 1, puzzleSpeed); //Start running through the displaypattern function
			currentSolveIndex = 0; //Set our puzzle index at 0

			//pattern replayed but orbs no longer clickable? 

		}
		if (actions.Count > 0) {
			actions[0]();
			actions.RemoveAt(0);
		}

	}

}
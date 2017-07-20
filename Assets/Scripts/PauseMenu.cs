using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

	public PlayerController playerController;
	public GameObject optionsMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Resume the game.
	/// </summary>
	public void Resume() {
		playerController.UnpauseGame();
	}

	/// <summary>
	/// Opens options menu.
	/// </summary>
	public void Options() {
		optionsMenu.SetActive(true);
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Exit the game.
	/// </summary>
	public void Quit() {
		Application.Quit();
	}
}

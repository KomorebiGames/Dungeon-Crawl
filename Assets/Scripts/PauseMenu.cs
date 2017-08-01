using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	/// <summary>
	/// The player controller.
	/// </summary>
	public PlayerController playerController;
	/// <summary>
	/// The options menu.
	/// </summary>
	public GameObject optionsMenu;

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
	/// Restart the game.
	/// </summary>
	public void Restart() {
		SceneManager.LoadScene("Main");
	}

	/// <summary>
	/// Exit the game.
	/// </summary>
	public void Quit() {
		Application.Quit();
	}
}

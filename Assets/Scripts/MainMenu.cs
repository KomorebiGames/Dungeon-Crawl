using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	/// <summary>
	/// The main menu holder.
	/// </summary>
	public GameObject mainMenuHolder;
	/// <summary>
	/// The options menu holder.
	/// </summary>
	public GameObject optionsMenuHolder;

	/// <summary>
	/// Starts the game.
	/// </summary>
	public void Play() {
		SceneManager.LoadScene("Main");
	}

	/// <summary>
	/// Exits the game.
	/// </summary>
	public void Quit() {
		Application.Quit();
	}

	/// <summary>
	/// Brings up the options menu.
	/// </summary>
	public void Options() {
		mainMenuHolder.SetActive(false);
		optionsMenuHolder.SetActive(true);
	}

	/// <summary>
	/// Returns to the main menu.
	/// </summary>
	public void BackToMain() {
		mainMenuHolder.SetActive(true);
		optionsMenuHolder.SetActive(false);
	}

	/// <summary>
	/// Sets the master volume.
	/// </summary>
	public void SetMasterVolume() {

	}

	/// <summary>
	/// Sets the music volume.
	/// </summary>
	public void SetMusicVolume() {

	}

	/// <summary>
	/// Sets the sfx volume.
	/// </summary>
	public void SetSfxVolume() {

	}

	/// <summary>
	/// Sets the mouse sensitivity.
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetMouseSensitivity(float value) {
		
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour {

	/// <summary>
	/// The player controller.
	/// </summary>
	public PlayerController playerController;
	/// <summary>
	/// The pause menu.
	/// </summary>
	public GameObject pauseMenu;

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
	/// <param name="value">Input value from the slider.</param>
	public void SetMouseSensitivity(float value) {
		playerController.mouseSensitivity = value;
	}

	/// <summary>
	/// Back this instance.
	/// </summary>
	public void Back() {
		pauseMenu.SetActive(true);
		gameObject.SetActive(false);
	}
}

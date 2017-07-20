using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour {

	public PlayerController playerController;
	public GameObject pauseMenu;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetMasterVolume() {
		
	}

	public void SetMusicVolume() {
		
	}

	public void SetSfxVolume() {
		
	}

	public void SetMouseSensitivity(float value) {
		playerController.mouseSensitivity = value;
	}

	public void Back() {
		pauseMenu.SetActive(true);
		gameObject.SetActive(false);
	}
}

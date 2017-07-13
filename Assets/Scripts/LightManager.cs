using System.Collections;
using UnityEngine;

/// <summary>
/// Light manager.
/// </summary>
public class LightManager : MonoBehaviour {

	/// <summary>
	/// The light intesity minimum.
	/// </summary>
	public float intensityMin;
	/// <summary>
	/// The light intensity max.
	/// </summary>
	public float intensityMax;
	/// <summary>
	/// The light range minimum.
	/// </summary>
	public float rangeMin;
	/// <summary>
	/// The light range max.
	/// </summary>
	public float rangeMax;
	/// <summary>
	/// The time inbetween flickers.
	/// </summary>
	public float flickerTime;

	/// <summary>
	/// The light source attached to the player.
	/// </summary>
	Light torch;
	/// <summary>
	/// The time for the next flicker to occur.
	/// </summary>
	float nextFlicker;
	/// <summary>
	/// <c>true</c> if the light is currently flickering; <c>false</c> otherwise.
	/// </summary>
	bool flickering;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		torch = gameObject.GetComponent<Light>();
		nextFlicker = Time.time + flickerTime;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		if (!flickering) {
			StartCoroutine(Flicker());
		}
	}

	/// <summary>
	/// Coroutine to gently flicker the light.
	/// </summary>
	IEnumerator Flicker() {
		float intensityOriginal = torch.intensity;
		float rangeOriginal = torch.range;
		float intensityGoal = Random.Range(intensityMin, intensityMax);
		float rangeGoal = Random.Range(rangeMin, rangeMax);

		flickering = true;

		while (Time.time < nextFlicker) {
			//
			// Interpolate smoothly to the flicker goal
			//
			float interpolate = (nextFlicker - Time.time) / flickerTime;
			torch.intensity = Mathf.Lerp(intensityOriginal, intensityGoal, interpolate);
			torch.range = Mathf.Lerp(rangeOriginal, rangeGoal, interpolate);
			yield return null;
		}

		nextFlicker = Time.time + flickerTime;
		flickering = false;
	}
}

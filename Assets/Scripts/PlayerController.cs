using System.Collections;
using UnityEngine;

/// <summary>
/// Controls players states and interactions.
/// </summary>
public class PlayerController : MonoBehaviour {

	/// <summary>
	/// The x-axis mouse sensitivity.
	/// </summary>
	public float mouseSensitivityX = 1;
	/// <summary>
	/// The y-axis mouse sensitivity.
	/// </summary>
	public float mouseSensitivityY = 1;
	/// <summary>
	/// The player walk speed.
	/// </summary>
	public float walkSpeed = 4;

	/// <summary>
	/// <c>true</c> if attacking; otherwise, <c>faclse</c>.
	/// </summary>
	bool attacking;
	/// <summary>
	/// The amount to move this frame.
	/// </summary>
	Vector3 moveAmount;
	/// <summary>
	/// The smooth move velocity.
	/// </summary>
	Vector3 smoothMoveVelocity;
	/// <summary>
	/// The vertical look rotation.
	/// </summary>
	float verticalLookRotation;
	/// <summary>
	/// The camera transform.
	/// </summary>
	Transform cameraTransform;
	/// <summary>
	/// The physics rigidbody.
	/// </summary>
	Rigidbody rigidbody;

	/// <summary>
	/// Called on instance awake.
	/// </summary>
	void Awake() {
		//
		// Lock the cursor to the middle of the screen and make invisible
		//
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cameraTransform = Camera.main.transform;
		rigidbody = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		
		//
		// Rotate the camera based on mouse movement
		//
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
		verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
		cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

		//
		// Get keyboard input
		//
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");

		//
		// Calculate the movement vector and smooth
		//
		Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);
	}

	/// <summary>
	/// Applies movement to rigidbody with a fixed delta.
	/// </summary>
	void FixedUpdate() {
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
	}

}

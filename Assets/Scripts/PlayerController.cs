using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public float walkSpeed;
	public float runSpeed;
	public float turnSpeed;

	Rigidbody myRigidbody;
	bool running;

	[SerializeField]
	AudioClip slowMoveAudio;
	AudioSource slowMoveSource;

	[SerializeField]
	AudioClip fastMoveAudio;
	AudioSource fastMoveSource;

	[SerializeField]
	AudioClip pickupAudio;
	AudioSource pickupSource;

	void Start()
	{
		myRigidbody = GetComponent<Rigidbody>();

		slowMoveSource = AudioHelper.CreateAudioSource(gameObject, slowMoveAudio);
		fastMoveSource = AudioHelper.CreateAudioSource(gameObject, fastMoveAudio);
		pickupSource = AudioHelper.CreateAudioSource(gameObject, pickupAudio);
	}

	void FixedUpdate()
	{
		if (GameplayManager.Instance.CanPlay())
		{
			float moveX = Input.GetAxisRaw("Horizontal");
			float moveZ = Input.GetAxisRaw("Vertical");

			if (moveX != 0f || moveZ != 0f)
			{
				// Shift key to run
				running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

				// Only use x and z input for directional input, not for speed
				Vector3 direction = new Vector3(moveX, 0f, moveZ);

				float speed = running ? runSpeed : walkSpeed;

				// Normalized direction * speed to get a constant speed regardless of direction
				Vector3 newPosition = transform.position + (direction.normalized * speed * Time.deltaTime);
				myRigidbody.MovePosition(newPosition);

				// Rotate player in direction of movement
				Quaternion newRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, turnSpeed * Time.deltaTime);

				if (running)
				{
					slowMoveSource.Stop();
					if (!fastMoveSource.isPlaying)
					{
						fastMoveSource.Play();
					}
				}
				else
				{
					fastMoveSource.Stop();
					if (!slowMoveSource.isPlaying)
					{
						slowMoveSource.Play();
					}
				}
			}
			else
			{
				// If you're not moving, you're definitely not running
				running = false;

				slowMoveSource.Stop();
				fastMoveSource.Stop();
			}
		}
		else
		{
			slowMoveSource.Stop();
			fastMoveSource.Stop();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (GameplayManager.Instance.CanPlay())
		{
			// Check if player has touched a pickup or reached the goal.
			if (other.CompareTag("Pickup"))
			{
				GameplayManager.Instance.OnPickup();
				Destroy(other.gameObject);
				pickupSource.Play();
			}
			else if (other.CompareTag("Goal"))
			{
				GameplayManager.Instance.OnGoal();
			}
		}
	}

	/// <summary>
	/// Is the player running?
	/// </summary>
	/// <returns><c>true</c> if the player is running; otherwise, <c>false</c>.</returns>
	public bool IsRunning()
	{
		return running;
	}
}
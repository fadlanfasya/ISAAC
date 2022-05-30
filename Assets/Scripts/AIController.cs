using UnityEngine;
using System.Collections;

/// <summary>
/// The AIController script controls the patrolling agent using a finite-state-machine. At any point
/// in time, the agent may be in one of the following states: Patrol, Alert, Investigate, Pursuit,
/// and Capture. The agent has specific behaviors for each of these states and based on the
/// situation, can transition to a different state.
/// </summary>
public class AIController : MonoBehaviour
{
	public Transform patrolPath;		// Parent transform component of the patrol path waypoints.
	public GameObject exclamationMark;	// Prefab for the alert icon.

	public float alertTime;				// Wait time when the agent is alert.
	public float investigateTime;		// Wait time when the agent reaches its suspected location.

	public Color normalColor;			// View cone color in the normal state.
	public Color suspectColor;			// View cone color when the agent suspects someone.
	public Color pursuitColor;			// View cone color when the agent pursues its target.

	enum State
	{
		Patrol,							// Agent goes from waypoint to waypoint.
		Alert,							// Agent just heard something.
		Investigate,					// Agent goes and investigate a location.
		Pursuit,						// Agent chases a target it spotted.
		Capture							// Agent captured the target.
	};

	UnityEngine.AI.NavMeshAgent agent;					// Reference to the agent's navigation component.
	AIPerception perception;			// Reference to the agent's perception component.

	Transform[] waypoints;				// An array of waypoints the agent follows during patrol.
	State state = State.Patrol;			// The AI agent's current state.
	float speed = 0f;					// The AI agent's move speed.
	float waitTime = 0f;				// How long should the agent wait? This is used during the Alert and Investigate states.
	int currentWaypoint;				// The index to our current waypoint along the patrol path.

	[SerializeField]
	AudioClip droneMoveAudio;
	AudioSource droneMoveSource;

	[SerializeField]
	AudioClip droneAlertAudio;
	AudioSource droneAlertSource;

	[SerializeField]
	AudioClip dronePursuitAudio;
	AudioSource dronePursuitSource;

	void Start()
	{
		droneMoveSource = AudioHelper.CreateAudioSource(gameObject, droneMoveAudio);
		droneAlertSource = AudioHelper.CreateAudioSource(gameObject, droneAlertAudio);
		dronePursuitSource = AudioHelper.CreateAudioSource(gameObject, dronePursuitAudio);

		// Keep references to commonly accessed components.
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		perception = GetComponentInChildren<AIPerception>();

		// By default, the agent starts in the patrol state.
		perception.SetViewConeColor(normalColor);
		state = State.Patrol;

		// Track the agent's move speed.
		speed = agent.speed;
		
		// Store the list of waypoints from our patrol path.
		waypoints = new Transform[patrolPath.childCount];
		for (int i = 0; i < waypoints.Length; ++i)
		{
			waypoints[i] = patrolPath.GetChild(i);
		}

		// Loop through all the waypoints and pick the closest one. This is where
		// we should patrol toward first.
		float closestDistance = Vector3.Distance(transform.position, waypoints[0].position);
		for (int i = 1; i < waypoints.Length; ++i)
		{
			float distance = Vector3.Distance(transform.position, waypoints[i].position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				currentWaypoint = i;
			}
		}

		// Hide the alert icon by default.
		exclamationMark.SetActive(false);
	}

	void Update()
	{
		// Call the appropriate update function based on the agent's current state.
		switch (state)
		{
			case State.Patrol:
				UpdatePatrol();
				break;
			case State.Alert:
				UpdateAlert();
				break;
			case State.Investigate:
				UpdateInvestigate();
				break;
			case State.Pursuit:
				UpdatePursuit();
				break;
			case State.Capture:
				UpdateCapture();
				break;
		}

		if (agent.speed > 0f && !droneMoveSource.isPlaying)
		{
			droneMoveSource.Play();
		}
		else if (agent.speed == 0f)
		{
			droneMoveSource.Stop();
		}
	}

	void LateUpdate()
	{
		// Check if the game has started.
		if (GameplayManager.Instance.CanPlay())
		{
			// Apply a speed scale based on whether we are facing our destination. This
			// slows down the agent until it turns toward its moving direction.
			Vector3 steeringDirection = agent.steeringTarget - transform.position;
			float dot = Vector3.Dot(steeringDirection.normalized, transform.forward);
			agent.speed = speed * Mathf.Max(dot, 0.1f);
		}
		else
		{
			// Stop the agent from moving.
			agent.speed = 0f;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// Check if we have bumped into the player.
		if (GameplayManager.Instance.CanPlay() && other.GetComponentInParent<PlayerController>())
		{
			// If so we caught the player.
			perception.SetViewConeColor(pursuitColor);
			state = State.Capture;

			// Notify the gameplay manager.
			GameplayManager.Instance.OnPlayerCaught();
		}
	}

	/// <summary>
	/// Update function during the "Patrol" state.
	/// </summary>
	void UpdatePatrol()
	{
		// *** Add your source code here ***
	}

	/// <summary>
	/// Update function during the "Alert" state.
	/// </summary>
	void UpdateAlert()
	{
		// Check if we can see the target.
		if (perception.HasConfirmedTarget())
		{
			// If we spotted the target, start pursuing it.
			agent.Resume();
			exclamationMark.SetActive(false);
			perception.SetViewConeColor(pursuitColor);
			state = State.Pursuit;
			dronePursuitSource.Play();
		}
		else
		{
			// If not, stop moving and show the alert icon. This lets the player 
			// know that the AI has suspected something and gives the player a
			// chance to run.
			agent.Stop();
			exclamationMark.SetActive(true);
			exclamationMark.transform.rotation = Quaternion.identity;

			// Check if wait time is over.
			waitTime -= Time.deltaTime;
			if (waitTime <= 0f)
			{
				// If so, start investigating.
				agent.Resume();
				exclamationMark.SetActive(false);
				perception.SetViewConeColor(suspectColor);
				waitTime = investigateTime;
				state = State.Investigate;
			}
		}
	}

	/// <summary>
	/// Update function during the "Investigate" state.
	/// </summary>
	void UpdateInvestigate()
	{
		// Check if we can see the target.
		if (perception.HasConfirmedTarget())
		{
			// If we spotted the target, start pursuing it.
			perception.SetViewConeColor(pursuitColor);
			state = State.Pursuit;
			dronePursuitSource.Play();
		}
		else
		{
			// If not, continue moving toward the last suspected location.
			agent.SetDestination(perception.GetLastPosition());

			// Check if we have arrived.
			float distance = Vector3.Distance(perception.GetLastPosition(), transform.position);
			if (distance <= agent.stoppingDistance)
			{
				// If so, wait at this location for a bit.
				waitTime -= Time.deltaTime;
				if (waitTime <= 0f)
				{
					// If wait time is over and we find nothing, return to patrolling.
					perception.SetViewConeColor(normalColor);
					state = State.Patrol;
				}
			}
		}
	}

	/// <summary>
	/// Update function during the "Pursuit" state.
	/// </summary>
	void UpdatePursuit()
	{
		// Check if we can still see the target.
		if (perception.HasConfirmedTarget())
		{
			// Keep moving toward the target's position.
			agent.SetDestination(perception.GetTarget().transform.position);
		}
		else
		{
			// If the target is out of sight, investigate where we last seen the target.
			perception.SetViewConeColor(suspectColor);
			waitTime = investigateTime;
			state = State.Investigate;
		}
	}

	/// <summary>
	/// Update function during the "Capture" state.
	/// </summary>
	void UpdateCapture()
	{
		// Stop the agent from moving.
		agent.Stop();

		// Compute the current rotation relative to our target.
		Vector3 directionToTarget = perception.GetTarget().transform.position - transform.position;
		Quaternion currentRotation = Quaternion.LookRotation(directionToTarget);

		// Use a sin function to generate a rotation that repeatedly swings left and right.
		Quaternion addedRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 5f) * 20f, Vector3.up);

		// Add the swing to our rotation to generate a new rotation.
		Quaternion newRotation = currentRotation * addedRotation;

		// Smoothly interpolate our current rotation to our new rotation.
		transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.1f);
	}
}
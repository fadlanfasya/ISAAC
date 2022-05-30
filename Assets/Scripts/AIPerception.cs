using UnityEngine;
using System.Collections;

/// <summary>
/// The AIPerception script allows an AI agent to perceive its environment through sight and sound.
/// The sight is controlled by a view cone defined by its range and angle, whereas the hearing is
/// defined by a range radius.
/// </summary>
public class AIPerception : MonoBehaviour
{
	[Range(0f, 10f)] public float viewRange;	// View range of the agent.
	[Range(5f, 180f)] public float viewAngle;	// View angle of the agent.
	[Range(0f, 10f)] public float hearingRange;	// Hearing range of the agent.
	[Range(3, 50)] public int numRays;			// Number of rays used for line-of-sight testing.

	Mesh viewCone;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;

	Vector3[] viewConeVertices;
	Vector2[] viewConeUVs;
	int[] viewConeIndices;

	Color viewConeColor;

	GameObject player;
	Vector3 lastPosition;
	float viewThreshold;
	int viewLayerMask;
	bool targetConfirmed;
	bool targetSuspected;

	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		viewCone = new Mesh();
		viewConeVertices = new Vector3[numRays + 1];
		viewConeUVs = new Vector2[numRays + 1];
		viewConeIndices = new int[(numRays - 1) * 3];

		viewConeVertices[0].Set(0f, 1f, 0f);
		viewConeUVs[0].Set(0f, 1f);
		
		for (int i = 0; i < numRays - 1; ++i)
		{
			viewConeIndices[(i * 3)] = 0;
			viewConeIndices[(i * 3) + 1] = i + 1;
			viewConeIndices[(i * 3) + 2] = i + 2;
		}

		meshFilter = GetComponent<MeshFilter>();
		meshFilter.sharedMesh = viewCone;

		meshRenderer = GetComponent<MeshRenderer>();

		// Map view angle from (0 to 180) to (1 to 0), using the dot product of normalized vectors
		// The dot product of two normalized vectors is the cosine of the angle between them
		viewThreshold = Mathf.Cos(Mathf.Deg2Rad * viewAngle * 0.5f);
		viewLayerMask = 1 << LayerMask.NameToLayer("Obstacle");
		targetConfirmed = false;
	}

	void Update()
	{
		UpdateViewCone();
		UpdateVision();
		UpdateHearing();
		UpdateTintColor();
	}

	/// <summary>
	/// Updates the position, size, and rotation of the view cone based on obstacles and our own orientation.
	/// </summary>
	void UpdateViewCone()
	{
		float currentAngle = transform.rotation.eulerAngles.y;

		float angleStep = viewAngle / (numRays - 1);
		// Cast a number of rays spread out across our desired view cone
		for (int i = 0; i < numRays; ++i)
		{
			// Angle for each ray incrementally increases across the cone (first one is on the left, last one is on the right)
			float localAngle = (viewAngle * -0.5f) + (i * angleStep);

			// Default range for this ray is the maximum view range (unless there is something in the way)
			float range = viewRange;

			// Create a ray originating from the object, rotated to the correct orientation
			Ray ray = new Ray(transform.position, Quaternion.AngleAxis(localAngle + currentAngle, Vector3.up) * Vector3.forward);

			// Check to see if there is anything in the way of this ray, in the object's view range
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, viewRange, viewLayerMask))
			{
				// Record range if there is a hit
				range = hitInfo.distance;
			}

			// Use sine and cosine to convert radial coordinates (radius, angle) to cartesian coordinates (x,z)
			// Height (y) is 1 to bring the cone off the ground
			viewConeVertices[i + 1].Set(Mathf.Sin(Mathf.Deg2Rad * localAngle) * range, 1f, Mathf.Cos(Mathf.Deg2Rad * localAngle) * range);

			// Map texture coordinates to the mesh
			float v = 1f;

			// Edges of mesh have v coordinate 0
			if (i == 0 || i == numRays - 1)
			{
				v = 0f;
			}

			// u coordinate based on viewable distance
			viewConeUVs[i + 1].Set(0.99f * (range / viewRange), v);
		}

		// Apply calculated information to mesh
		viewCone.vertices = viewConeVertices;
		viewCone.uv = viewConeUVs;
		viewCone.triangles = viewConeIndices;
		viewCone.RecalculateBounds();
	}

	/// <summary>
	/// Checks the object's range of vision to determine if there is a player object in it.
	/// </summary>
	void UpdateVision()
	{
		targetConfirmed = false;

		// *** Add your source code here ***
	}

	/// <summary>
	/// Checks if the player is within hearing range and is running, and triggers suspicion if so.
	/// </summary>
	void UpdateHearing()
	{
		targetSuspected = false;

		// Confirmed targets are automatically suspected as well
		if (targetConfirmed)
		{
			targetSuspected = true;
			return;
		}

		// Check distance to player
		Vector3 agentToPlayer = player.transform.position - transform.position;
		if (agentToPlayer.magnitude > hearingRange)
		{
			return;
		}

		// We can only hear the player if he/she is running
		if (!player.GetComponent<PlayerController>().IsRunning())
		{
			return;
		}

		lastPosition = player.transform.position;
		targetSuspected = true;
	}

	/// <summary>
	/// Updates the color of the view cone.
	/// </summary>
	void UpdateTintColor()
	{
		Color currentColor = meshRenderer.material.GetColor("_TintColor");
		meshRenderer.material.SetColor("_TintColor", Color.Lerp(currentColor, viewConeColor, Time.deltaTime * 5f));
	}

	/// <summary>
	/// Changes the target color of the view cone.
	/// The cone will quickly change color to the new one, but it is not instantaneous.
	/// </summary>
	/// <param name="color"></param>
	public void SetViewConeColor(Color color)
	{
		viewConeColor = color;
	}

	public bool HasConfirmedTarget()
	{
		return targetConfirmed;
	}

	public bool HasSuspectedTarget()
	{
		return targetSuspected;
	}

	public GameObject GetTarget()
	{
		return player;
	}

	public Vector3 GetLastPosition()
	{
		return lastPosition;
	}
}
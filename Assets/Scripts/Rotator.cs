using UnityEngine;
using System.Collections;

/// <summary>
/// Script that causes the object's transform to rotate over time.
/// </summary>
public class Rotator : MonoBehaviour
{
	public Vector3 rotateSpeed;

	void Update()
	{
		transform.Rotate(rotateSpeed * Time.deltaTime);
	}
}

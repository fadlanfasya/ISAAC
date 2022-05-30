using UnityEngine;
using System.Collections;

public class DoNotRotate : MonoBehaviour
{
	void LateUpdate()
	{
		transform.rotation = Quaternion.LookRotation(Vector3.forward);
	}
}

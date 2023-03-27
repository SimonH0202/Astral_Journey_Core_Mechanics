using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

	private void Awake()
	{
		// get a reference to our main camera
		if (cam == null)
		{
			cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
		}
	}

	void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}

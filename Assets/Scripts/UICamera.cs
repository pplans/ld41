using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour {
    	// Use this for initialization
	void Start ()
	{
		// obtain camera component so we can modify its viewport
		Camera camera = GetComponent<Camera>();
        camera.cullingMask = 1 << 8;
	}
}

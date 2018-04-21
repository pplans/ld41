using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	// set the desired aspect ratio (the values in this example are
	// hard-coded for 16:9, but you could make them into public
	// variables instead so you can set them at design time)
	const float targetaspect = 16.0f / 9.0f;

	// determine the game window's current aspect ratio
	float windowaspect = (float)Screen.width / Screen.height;

	// Use this for initialization
	void Start ()
	{

		// obtain camera component so we can modify its viewport
		Camera camera = GetComponent<Camera>();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
	private float time;

	public float sea_choppy = 4.0f;
	public float sea_level = 0.5f;
	public float sea_speed = 1.0f;
	public float sea_freq = 0.1f;

	// Use this for initialization
	void Start ()
	{

	}

	void Update()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		while (i < vertices.Length)
		{
			vertices[i].y = Mathf.Sin(WaterLib.sea_map(vertices[i], time, sea_choppy, sea_level, sea_speed, sea_freq));
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		time += Time.deltaTime;
		//if (time > Mathf.PI * 2.0f) time = 0.0f;
	}
}

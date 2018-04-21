﻿using System.Collections;
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
			vertices[i].y = WaterLib.sea_map(vertices[i], time, sea_choppy, sea_level, sea_speed, sea_freq);
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		time += Time.deltaTime;
		//if (time > Mathf.PI * 2.0f) time = 0.0f;
	}

	float getHeightAtPoint(Vector2 p)
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		Vector3[] nearestPoints = new Vector3[3];
		for(int k = 0;k < 3; ++k) nearestPoints[k] = new Vector3(100000.0f, 10000.0f, 10000.0f);
		while (i < vertices.Length)
		{
			Vector3 pv = vertices[i];
			for(int j = 0;j < 3; ++j)
			{
				if (((new Vector2(pv.x, pv.y)) - p).magnitude < ((new Vector2(nearestPoints[j].x, nearestPoints[j].y)) - p).magnitude)
				{
					nearestPoints[j] = pv;
				}
			}
			++i;
		}

		float height = 0.0f;
		for (int k = 0; k < 3; ++k) height+=nearestPoints[k].y*0.33f;
		return height;
	}
    
    public void GetSurfacePosAndNormalForWPos(Vector3 wpos, out Vector3 outwpos, out Vector3 outwnormal)
    {
        outwnormal = Vector3.up;
        outwpos = wpos;
        outwpos.y = WaterLib.sea_map(wpos, time, sea_choppy, sea_level, sea_speed, sea_freq);
    }
}

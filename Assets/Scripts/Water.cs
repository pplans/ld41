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
			vertices[i].y = WaterLib.sea_map(vertices[i], time, sea_choppy, sea_level, sea_speed, sea_freq);
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		time += Time.deltaTime;
		//if (time > Mathf.PI * 2.0f) time = 0.0f;
	}

	float getHeightAtPoint(Vector2 p, out Vector3 normal)
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
		normal.x = normal.y = normal.z = 0.0f;
		for (int k = 0; k < 3; ++k)
		{
			height += nearestPoints[k].y * 0.33f;
		}
		normal = Vector3.Cross(nearestPoints[2] - nearestPoints[0], nearestPoints[1] - nearestPoints[0]);
		Debug.DrawRay(nearestPoints[0], (nearestPoints[2] - nearestPoints[0])*10.0f);
		Debug.DrawRay(nearestPoints[0], (nearestPoints[1] - nearestPoints[0])*10.0f);
		Debug.DrawRay(new Vector3(p.x, height, p.y), normal);
		normal.y = Mathf.Abs(normal.y);

		return height;
	}
    
    public void GetSurfacePosAndNormalForWPos(Vector3 wpos, out Vector3 outwpos, out Vector3 outwnormal)
    {
		float eps = 0.1f;
		Vector3 p1 = wpos+new Vector3(-eps, 0.0f, 0.00f);
		Vector3 p2 = wpos + new Vector3(eps, 0.0f, 0.00f);
		Vector3 p3 = wpos + new Vector3(0.0f, 0.0f, eps);

		outwnormal = Vector3.up;
        outwpos = wpos;
        p1.y = WaterLib.sea_map(p1, time, sea_choppy, sea_level, sea_speed, sea_freq);
		p2.y = WaterLib.sea_map(p2, time, sea_choppy, sea_level, sea_speed, sea_freq);
		p3.y = WaterLib.sea_map(p3, time, sea_choppy, sea_level, sea_speed, sea_freq);
		outwnormal = Vector3.Cross(p3-p1, p2-p1);
		outwnormal.y = Mathf.Abs(outwnormal.y);
		outwpos.y = 0.33f * (p1.y + p2.y + p3.y);

		Debug.DrawLine(outwpos, outwpos+outwnormal * 400.0f, Color.green);

		/*outwpos = wpos;
		outwpos.y = getHeightAtPoint(new Vector3(wpos.x, wpos.z), out outwnormal);*/
	}
}

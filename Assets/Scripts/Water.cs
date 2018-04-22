using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Vertex
{
	uint index;
	Vector3 vertex;
}

public class Water : MonoBehaviour
{
	private MeshFilter m_meshFilter;
	private MeshCollider m_meshCollider;
	private Mesh m_mesh;
	private MeshRenderer m_meshRenderer;
	public Material m_material;

	private float time;

	public float sea_choppy = 4.0f;
	public float sea_level = 0.5f;
	public float sea_speed = 1.0f;
	public float sea_freq = 0.1f;
    public float sea_offset_uvfactor = 0.1f;

	public Vector3 Offset;

	// Use this for initialization
	void Start ()
	{
		Clear();
		Offset = Vector3.zero;
		Generate(5, 1.0f, -1.0f);
	}

	void Clear()
	{
	}

	void Generate(int width, float top, float bot)
	{
		m_meshFilter = GetComponent<MeshFilter>();
		m_meshRenderer = GetComponent<MeshRenderer>();
		m_meshCollider = GetComponent<MeshCollider>();
		if (m_meshFilter == null)
		{
			m_mesh = new Mesh();
			m_mesh.name = "Custom Mesh";
			m_meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		else
		{
			m_mesh = m_meshFilter.sharedMesh;
			m_mesh.name = "Custom Mesh";
		}
		if (m_meshCollider == null)
		{
			m_meshCollider = gameObject.AddComponent<MeshCollider>();
		}
		if (m_meshRenderer == null)
		{
			m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
			m_meshRenderer.material = m_material;
		}

		int amountData = width * width + width * width;
		Vector3[] vertices = new Vector3[amountData];
		Vector3[] normals = new Vector3[amountData];
		int[] triangles;
		Vector2[] uv = new Vector2[amountData];
		int k = 0;
		for (int j = 0; j < width; j++)
			for (int i = 0; i < width; i++, k++)
			{
				vertices[k] = new Vector3(
					(i - (width-1) * 0.5f) / ((width-1)*0.5f),
					top,
					(j - (width-1) * 0.5f) / ((width-1)*0.5f)
				);
				normals[k] = Vector3.up;
				uv[k] = new Vector2(
					(float)i / width,
					(float)j / width
					);
				vertices[k+width*width] = new Vector3(
					(i - (width - 1) * 0.5f) / ((width - 1) * 0.5f),
					bot,
					(j - (width - 1) * 0.5f) / ((width - 1) * 0.5f)
				);
				normals[k + width * width] = -Vector3.up;
				uv[k+width*width] = new Vector2(
					(float)i / width,
					(float)j / width
					);
			}
		m_mesh.vertices = vertices;
		int nbFaces = (width - 1) * (width - 1);
		triangles = new int[(nbFaces+nbFaces+4*width) * 6];
		int t = 0;
		for (int face = 0; face < nbFaces; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face % (width - 1) + (face / (width - 1) * width);

			triangles[t++] = i + width;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + width;
			triangles[t++] = i + width + 1;
			triangles[t++] = i + 1;
		}

		for (int face = 0; face < nbFaces; face++)
		{
			// Retrieve lower left corner from face ind
			int i = width*width+(face % (width - 1) + (face / (width - 1) * width));
			
			triangles[t++] = i;
			triangles[t++] = i + 1;
			triangles[t++] = i + width;

			triangles[t++] = i + 1;
			triangles[t++] = i + width + 1;
			triangles[t++] = i + width;
		}

		int secondsideIndex = width * width;
		// face side top
		for (int face = 0; face < width-1; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face;

			triangles[t++] = i;
			triangles[t++] = i + 1;
			triangles[t++] = i + secondsideIndex;

			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + 1;
			triangles[t++] = i + secondsideIndex + 1;
		}
		// face side bot
		for (int face = 0; face < width-1; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face+(width-1)*width;

			triangles[t++] = i;
			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + 1;

			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + secondsideIndex + 1;
			triangles[t++] = i + 1;
		}
		// face side left
		for (int face = 0; face < width-1; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face * width;

			triangles[t++] = i;
			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + width;

			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + width + secondsideIndex;
			triangles[t++] = i + width;
		}
		// face side right
		for (int face = 0; face < width-1; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face + ((face+1) * (width - 1));

			triangles[t++] = i;
			triangles[t++] = i + width;
			triangles[t++] = i + secondsideIndex;

			triangles[t++] = i + secondsideIndex;
			triangles[t++] = i + width;
			triangles[t++] = i + width + secondsideIndex;
		}

		m_mesh.uv = uv;
		m_mesh.normals = normals;
		m_mesh.triangles = triangles;
		m_mesh.RecalculateNormals();
		GetComponent<MeshFilter>().mesh = m_mesh;
	}

    void LateUpdate()
    {
        Material material = GetComponent<Renderer>().material;
        material.SetVector("_BoatPosition",new Vector4(-Offset.x, -Offset.z) * sea_offset_uvfactor);
    }

    void Update()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		while (i < vertices.Length/2)
		{
			vertices[i].y = WaterLib.sea_map(Offset + vertices[i], time, sea_choppy, sea_level, sea_speed, sea_freq);
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
        p1.y = WaterLib.sea_map(Offset + p1, time, sea_choppy, sea_level, sea_speed, sea_freq);
		p2.y = WaterLib.sea_map(Offset + p2, time, sea_choppy, sea_level, sea_speed, sea_freq);
		p3.y = WaterLib.sea_map(Offset + p3, time, sea_choppy, sea_level, sea_speed, sea_freq);
		outwnormal = Vector3.Cross(p3-p1, p2-p1);
		outwnormal.y = Mathf.Abs(outwnormal.y);
		outwpos.y = 0.33f * (p1.y + p2.y + p3.y);

		/*outwpos = wpos;
		outwpos.y = getHeightAtPoint(new Vector3(wpos.x, wpos.z), out outwnormal);*/
	}
}

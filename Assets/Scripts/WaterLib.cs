using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLib {
	public static float fract(float x)
	{
		//x = x < 0.0 ? -x : x;
		return x - Mathf.Floor(x);
	}

	public static float rounddec(float x, int n)
	{
		return Mathf.Floor(x * Mathf.Pow(10, n)) / Mathf.Pow(10, n);
	}

	// sea funcs work well

	public static float sea_hash(Vector2 p)
	{
		float h = p.x * 127.1f + p.y * 311.7f;// p.x*127.1D+p.y*311.7D;// p.x * p.y;// Vector2.Dot(p, (new Vector2(127.1, 311.7)));
												//h = Math.Sin(h) * 43758.5453123D;
		return fract(Mathf.Sin(h) * 13.5453123f);//fract(rounddec(Mathf.Sin(h), 7));
	}

	public static float sea_noise(Vector2 p)
	{
		Vector2 i = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
		Vector2 f = new Vector2(fract(p.x), fract(p.y)); // fract
		Vector2 u = new Vector2(f.x * f.x * (3.0f - 2.0f * f.x), f.y * f.y * (3.0f - 2.0f * f.y));
		return -1.0f + 2.0f * Mathf.Lerp(
							Mathf.Lerp(sea_hash(i + Vector2.zero), sea_hash(i + new Vector2(1.0f, 0.0f)), u.x),
							Mathf.Lerp(sea_hash(i + new Vector2(0.0f, 1.0f)), sea_hash(i + new Vector2(1.0f, 1.0f)), u.x),
							u.y);
	}

	// sea
	public static float sea_octave(Vector2 uv, float choppy)
	{
		uv.x += sea_noise(uv);
		uv.y += sea_noise(uv);
		Vector2 wv = new Vector2(
				1.0f - Mathf.Abs(Mathf.Sin(uv.x)),
				1.0f - Mathf.Abs(Mathf.Sin(uv.y))
				);
		Vector2 swv = new Vector2(
				Mathf.Abs(Mathf.Cos(uv.x)),
				Mathf.Abs(Mathf.Cos(uv.y))
				);
		wv.x = Mathf.Lerp(wv.x, swv.x, wv.x);
		wv.y = Mathf.Lerp(wv.y, swv.y, wv.y);
		return Mathf.Pow(1.0f - Mathf.Pow(wv.x * wv.y, 0.65f), choppy);
	}

	public static float sea_map(Vector3 p, float t, float _choppy, float SeaLevel, float SeaSpeed, float SeaFreq)
	{
		float freq = SeaFreq;
		float amp = SeaLevel;
		float choppy = _choppy;
		float speed = SeaSpeed;
		Vector2 uv = new Vector2(p.x, p.z); uv.x *= 0.75f;

		float d, h = 0.0f;
		for (int i = 0; i < 3; ++i)
		{
			d = sea_octave((new Vector2(uv.x + t * speed, uv.y + t * speed)) * freq, choppy);
			d += sea_octave((new Vector2(uv.x - t * speed, uv.y - t * speed)) * freq, choppy);
			h += d * amp;
			uv.x = uv.x * 1.6f + uv.y * 1.2f;
			uv.y = uv.x * -1.2f + uv.y * 1.6f;
			freq *= 1.9f; amp *= 0.22f;
			choppy = Mathf.Lerp(choppy, 1.0f, 0.2f);
		}
		return /*p.y - */h;
	}
}

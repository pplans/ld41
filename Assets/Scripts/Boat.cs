using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {

	public List<AudioClip> soundBank;
	private int currentSound;

	public void UpdateAudio(float pitch)
	{
		AudioSource asrc = gameObject.GetComponent<AudioSource>();
		if(!asrc.isPlaying && soundBank.Count>0)
		{
			currentSound = (currentSound + 1) < soundBank.Count ? currentSound + 1 : 0;
			asrc.clip = soundBank[currentSound];
			asrc.Play();
		}
	}
}

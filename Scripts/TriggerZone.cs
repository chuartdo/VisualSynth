using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour {

	public Color GraphicColor = Color.clear;
	public float ZoneSize = -1;
	public int   SoundIndex = -1;
	public int   ScaleIndex = -1;
	public int   TrackIndex = -1;

	public EffectCursor cursor;

	void OnTriggerEnter(Collider other) {
 		// Change style to current setting
		string msg = "Enter Zone ";
		if (cursor != null) {
			if (GraphicColor != Color.clear) {
				cursor.ChangeParticleColor (GraphicColor);
				msg += GraphicColor.ToString();
			}
			if (SoundIndex >= 0 && SoundIndex < 127) {
				cursor.ChangeInstrument (SoundIndex);
				msg += SoundIndex.ToString();
			}
			if (ScaleIndex > 0)
				cursor.ChangeScaleMode (ScaleIndex);
			if (TrackIndex >= 0) {

				int currentTrack = SynthPlayer._instance.SongIndex;
				if (TrackIndex != currentTrack)
					SynthPlayer.Play (TrackIndex);
			}
			else
				SynthPlayer.Stop ();
			
		}
		DebugText.show(msg);
		Debug.Log (msg);
	}

	void OnTriggerExit(Collider other) {
		DebugText.show ("");
 	}
		 
}

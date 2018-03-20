/*
 * Map area to trigger musical notes based on specified scale
 * Emulates a touch pad with area specified by screen boundary
 * Each note divided into individual keys where horizontal increament scale
 * and Vertical indicates descending scale
 * 
 * Created by Leon Hong Chu   @chuartdo
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class TouchPad : MonoBehaviour
{
  
    public Text infoText;

    public int scaleIndex = 0;

    private bool activate = false;
 
    static public Vector3 touchPosition;

	public int sound1 = 1;
	public int sound2 = 2;
    // Use this for initialization
    void Start()
    {
		activate = true;
		StartCoroutine(playSynthPad());
		touchPosition = new Vector3 (0, 0, 0);
    }
		
	public static float Remap (  float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

    // Update is called once per frame
	static public void UpdatePosition(float x, float y, float z)
    {
		touchPosition.x = Remap (x,   CameraBound.MIN_EDGE.x, CameraBound.MAX_EDGE.x, 0f,1f);
		touchPosition.y = Remap (y,  CameraBound.MIN_EDGE.y,CameraBound.MAX_EDGE.y, 0f,1f);
		 
		touchPosition.x = Mathf.Clamp01 (touchPosition.x);
		touchPosition.y = Mathf.Clamp01 (touchPosition.y );
     }
		
		
    readonly float PAD_RESOLUTION = 48;
    readonly int startNote =0;

    // Map to actual midi note based on pad input of -0.5 to 0.5
    int MapScaleToNote(int index, int scaleIndex)
    {
        int[] scale = ScaleReference.getScale( scaleIndex);
        int notesInScale = scale.Length;
        int octave = index / notesInScale;
        int scaleNoteIndex = index % notesInScale;
		if (scaleNoteIndex >= scale.Length || scaleNoteIndex < 0) {
			Debug.Log ("Bad index" + scaleNoteIndex);
			return 0;
		}

		int outNote = startNote + scale[scaleNoteIndex] + octave * 12;
		outNote = (int) Mathf.Clamp ((float)outNote, 0f, 127f);
		 
		return outNote;
    }

	/* Check for movements and generate sounds on vertical and horizontal movement */
    IEnumerator playSynthPad()
    {
        int lastNote = 0;
        int lastNote2 = 0;
		const int vel = 127; //(int)(touchPosition.y + 0.5f  * 127f) ;

		WaitForSeconds delaySec = new WaitForSeconds (0.1f);
        while (activate)
        {
			yield return delaySec;
          
            if (activate) {
		          
				int noteIndex  = (int)(touchPosition.x  * PAD_RESOLUTION);

				int note = MapScaleToNote(noteIndex,scaleIndex);

				//infoText.text = "x: " + touchPosition.x + " Y: " + touchPosition.y + " nidex: " + noteIndex; 


				if (lastNote != note) {
					SynthPlayer.instance().NoteON(2,note,vel, sound1 );
					SynthPlayer.instance().NoteOFF(2,lastNote);
					lastNote = note;
					infoText.text  = " Note: " + note + " Scale: " + scaleIndex;
				}


				noteIndex = (int)( (1f -  touchPosition.y ) * PAD_RESOLUTION);
				int note2 = MapScaleToNote(noteIndex, scaleIndex);

				if (lastNote2 != note2) {    
					SynthPlayer.instance().NoteON(3,note2, vel, sound2 );
					SynthPlayer.instance().NoteOFF(3,lastNote2);
					lastNote2 = note2;
 				}
            } 
        }
        UnitySynth.instance().getSynth().NoteOffAll(true);

    }


	public void ChangeProgram(int s1, int s2 = -1) {
		sound1 = s1;
		if (s2 >= 0)
			sound2 = s2;
	}

	public int ChangeScale(int index) {
		int MAX_SCALE_INDEX = System.Enum.GetValues (typeof(ScaleReference.Scale)).Length;
		if (index < 0 || index > MAX_SCALE_INDEX)
			scaleIndex = Random.Range (0, MAX_SCALE_INDEX);
		else
			scaleIndex = index;
		return scaleIndex;
	}


}


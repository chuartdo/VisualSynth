// Modified codes from CSharpSynth
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSharpSynth.Effects;
using CSharpSynth.Sequencer;
using CSharpSynth.Synthesis;
using CSharpSynth.Midi;

[RequireComponent(typeof(AudioSource))]
public class SynthPlayer : MonoBehaviour
{
    //Public
    //Folder containing midi files are inside Assets/Resources direcotry
    public string midiFilePath = "MIDI";
	//Add list of pre-packages songs files to play
	readonly static string[] songList = { "TRACK_01.MID","TRACK_02.MID","TRACK_03.MID","TRACK_04.MID" ,"TRACK_05.MID","TRACK_06.MID"};


    public bool ShouldPlayFile = false;

    //Try also: "FM Bank/fm" or "Analog Bank/analog" for some different sounds
    public string bankFilePath = "GM Bank/gm";
    public int bufferSize = 1024;
    public int midiNote = 60;
    public int midiNoteVolume = 100;
    [Range(0, 127)] //From Piano to Gunshot
    public int midiInstrument = 0;
    //Private 
    private float[] sampleBuffer;
    private float gain = 1f;
    private MidiSequencer midiSequencer;
    private StreamSynthesizer midiStreamSynthesizer;

 
	private bool playSong = false;
    // Awake is called when the script instance
    // is being loaded.

	int songIndex = 0;
	public int SongIndex {
		get{  return playSong ? songIndex : -1; }
	}


	public static SynthPlayer _instance;

	public void NoteON(int channel, int note, int volume = 120, int inst = 1)
	{
		midiStreamSynthesizer.NoteOn(channel, note, volume, inst);
	}

	public void NoteOFF( int channel ,int note, bool allOff = false)
	{
		if (allOff)
			midiStreamSynthesizer.NoteOffAll(true);
		else
			midiStreamSynthesizer.NoteOff(channel, note);
	}

	public static SynthPlayer instance()
	{
		return _instance;
	}

	public static void Play(int index = -1 ) {
		if (index < 0)
			_instance.songIndex++;
		else
			_instance.songIndex = index;
		_instance.midiSequencer.Stop(true);
		_instance.playSong = true;
	}

	public static void Stop() {
		_instance.playSong = false;
	}

    void Awake()
    {
		_instance = this;
        midiStreamSynthesizer = new StreamSynthesizer(44100, 2, bufferSize, 40);
        sampleBuffer = new float[midiStreamSynthesizer.BufferSize];
        
        midiStreamSynthesizer.LoadBank(bankFilePath);

        midiSequencer = new MidiSequencer(midiStreamSynthesizer);

        //These will be fired by the midiSequencer when a song plays. Check the console for messages if you uncomment these
        //midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler (MidiNoteOnHandler);
        //midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler (MidiNoteOffHandler);			
    }



	void Start()
	{
		string dirpath = PlayerPrefs.GetString(FileUtility.MidiPathKey);
		if (FileUtility.PathIsDirectory(dirpath))
			midiFilePath = dirpath;

		StartCoroutine(playJukebox());
	}

	public string[] songName;

	IEnumerator playJukebox()
	{
		songName = songList;

		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			if (playSong && !midiSequencer.isPlaying)
			{
				playMidi(songIndex);
			} else if (!playSong)
			{
				midiSequencer.Stop(true);
			}
		}
	}
		

	public void playMidi(int index)
	{
		midiSequencer.Stop(true);

		if (index >= songName.Length)
			index = 0;
		else if (index < 0)
			index = songName.Length - 1;
		songIndex = index;

		string slash = (midiFilePath.EndsWith("/") ? "" : "/");
		string path = midiFilePath + slash + songName[index];
		DebugText.show("Playing Song " + path);
		try {
			midiSequencer.LoadMidi(new MidiFile(path),false);
			midiSequencer.Play();
			playSong = true;
		} catch (System.Exception e) {
			DebugText.show("Error " + e);
		}

	}

	 
        // See http://unity3d.com/support/documentation/ScriptReference/MonoBehaviour.OnAudioFilterRead.html for reference code
        //	If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
        //
        //	The filter is inserted in the same order as the MonoBehaviour script is shown in the inspector. 	
        //	OnAudioFilterRead is called everytime a chunk of audio is routed thru the filter (this happens frequently, every ~20ms depending on the samplerate and platform). 
        //	The audio data is an array of floats ranging from [-1.0f;1.0f] and contains audio from the previous filter in the chain or the AudioClip on the AudioSource. 
        //	If this is the first filter in the chain and a clip isn't attached to the audio source this filter will be 'played'. 
        //	That way you can use the filter as the audio clip, procedurally generating audio.
        //
        //	If OnAudioFilterRead is implemented a VU meter will show up in the inspector showing the outgoing samples level. 
        //	The process time of the filter is also measured and the spent milliseconds will show up next to the VU Meter 
        //	(it turns red if the filter is taking up too much time, so the mixer will starv audio data). 
        //	Also note, that OnAudioFilterRead is called on a different thread from the main thread (namely the audio thread) 
        //	so calling into many Unity functions from this function is not allowed ( a warning will show up ). 	
    private void OnAudioFilterRead(float[] data, int channels)
    {
        //This uses the Unity specific float method we added to get the buffer
        midiStreamSynthesizer.GetNext(sampleBuffer);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = sampleBuffer[i] * gain;
        }
    }

    public void MidiNoteOnHandler(int channel, int note, int velocity)
    {
        Debug.Log("NoteOn: " + note.ToString() + " Velocity: " + velocity.ToString());
    }

    public void MidiNoteOffHandler(int channel, int note)
    {
        Debug.Log("NoteOff: " + note.ToString());
    }
	 
}

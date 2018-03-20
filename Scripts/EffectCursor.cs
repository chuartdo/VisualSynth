/*
 * Map area to trigger musical notes based on specified scale
 * 
 * Created by Leon Hong Chu   @chuartdo
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class EffectCursor : MonoBehaviour {
	ArrayList triggerZones;

	Color[] palette = { Color.red, Color.green, Color.magenta, Color.cyan, Color.yellow, Color.blue, Color.white, Color.gray };

	GameObject startMenu;

	Color currentColor=Color.black;
	int currentScaleIndex=13;
	int currentInstrument=1;
	float zoneSize = 5.0f;


	private SocketIOComponent socket;
	float NX=0, NY = 0, NZ=0;
	public float transitionSpeed = 2f;
	ParticleSystem[] particles;

	public bool updateCursor = false;
	public bool connected = false;
	private bool initialize = false;

	public void Start() 
	{

		triggerZones = new ArrayList ();

		SocketIOMessageCallback ();

		particles = GetComponentsInChildren<ParticleSystem> ();

		startMenu = GameObject.Find ("StartMenu");
		Cursor.visible = false;
 	}

	void Update () {
		//	transform.position =  new Vector3 (NX, NY, 0) ;
		if (connected)
			startMenu.SetActive(false);
		
		if (initialize) {
			ClearZone (true);
			startMenu.SetActive(false);
			CameraBound.Reset();
			initialize = false;
			StopPlay();
		}


		if (updateCursor) {
			// Update movement from received socket message
			Vector3 newPos = new Vector3 (NX, NY, 0);
			transform.position = Vector3.Lerp (transform.position, newPos, Time.deltaTime * transitionSpeed);

		} else {
			// Test with mouse cursor input
			MouseControl ();

			NX = transform.position.x;
			NY = transform.position.y;
		}
		CameraBound.Update (NX, NY);


		TouchPad.UpdatePosition (transform.position.x, transform.position.y, transform.position.z);

		ProcessShortCutKey ();
		SynthKeyboard ();
	}

	void SocketIOMessageCallback() {
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();

		socket.On("open", Open);
		socket.On("coord", ParseCoord);
		socket.On("status", StatusCheck);
		socket.On("calibrate", CalibrateInput);
		socket.On("graphic_style", ChangeGraphicStyle);
		socket.On("music_mode", ChangeMusicMode);
		socket.On("program_change", ChangeProgramInstrument);
		socket.On("set_trigger_zone",SetTriggerZone);
		socket.On("play", PlaySong);
		socket.On("stop", StopPlay);
		socket.On("error", Error);
		socket.On("close", Close);
	}


	Color RandomColor() {
		return palette [Random.Range (0, palette.Length)];
	}

	Color ParseColorText(string colour) {
		if ( "red".Equals(colour))
			return Color.red;
		if (colour == "blue")
			return Color.blue;
		if (colour == "green")
			return Color.green;
		if (colour == "yellow")
			return Color.yellow;
		return Color.white;
	}

	public void Open(SocketIOEvent e)
	{
		updateCursor = true;
		connected = true;
		CameraBound.Reset();
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
	}


	public void StatusCheck(SocketIOEvent e)
	{
		Debug.Log("Received check" + e.name + " " + e.data);
	}

	public void ChangeGraphicStyle(SocketIOEvent e)
	{
		Debug.Log("Received graphic style " + e.name + " " + e.data);
		JSONObject item = e.data;
		try {
		currentColor = ParseColorText (item.GetField("data").str);
		if (currentColor == Color.black)
			currentColor = RandomColor();
		} catch (System.NullReferenceException err) {
			currentColor = Color.white;
		}

		ChangeParticleColor (currentColor);
	}

	public void ChangeParticleColor(Color color) {
		DebugText.show ("Color " + color.ToString ());
		if (particles.Length > 0) {
			foreach (ParticleSystem p in particles) {
				var main = p.main;
				main.startColor = color;
			}
		}
	}

	public void SetTriggerZone(SocketIOEvent e)
	{
		AddTriggerZone ();
	}


	public void ChangeInstrument(int inst) {
		TouchPad pad = GetComponent<TouchPad> ();
		if (pad != null)
			pad.ChangeProgram (inst, inst +1);
		DebugText.show ("Sound :" + inst);
 
	}

	public void ChangeScaleMode(int mode) {

		TouchPad pad = GetComponent<TouchPad> ();
		if (pad != null)
			currentScaleIndex = pad.ChangeScale (mode);
		DebugText.show ("Scale :" + currentScaleIndex);

	}


	public void ChangeMusicMode(SocketIOEvent e)
	{
		Debug.Log("Received mode change " + e.name + " " + e.data);
		string modeStr =  e.data.GetField("data").str;

		if (modeStr == "")
			currentScaleIndex = -1;
		else if (modeStr == "major")
			currentScaleIndex = 1;
		else if (modeStr == "minor")
			currentScaleIndex = 2;
		else if (modeStr == "blue")
			currentScaleIndex = 13;
		else if (modeStr == "pentatonic")
			currentScaleIndex = 14;
		else { 
			int.TryParse(modeStr,out currentScaleIndex);
		}
		
		ChangeScaleMode(currentScaleIndex);
	}

	// Asign instrument program change number based on General Midi 1 Sound Set spec
	public void ChangeProgramInstrument(SocketIOEvent e)
	{
		Debug.Log("Received sound change " + e.name + " " + e.data);
		string instrumentStr = e.data.GetField("data").str;

		if (instrumentStr == "piano")
			currentInstrument = Random.Range (1, 8);
		else if (instrumentStr == "sax")
			currentInstrument = 66;
		else if (instrumentStr == "strings")
			currentInstrument = Random.Range (41, 48);
		else if (instrumentStr == "guitar")
			currentInstrument = Random.Range (25, 32);
		else if (instrumentStr == "synth")
			currentInstrument = Random.Range (81, 88);
		else if (instrumentStr == "percussion")
			currentInstrument = Random.Range (113, 120);
		else { 
			int.TryParse(instrumentStr,out currentInstrument);
		}

		ChangeInstrument(currentInstrument);
	}

	public void PlaySong(SocketIOEvent e = null)
	{
		SynthPlayer.Play ( );
	}

	public void StopPlay(SocketIOEvent e = null)
	{
		DebugText.show("");
		SynthPlayer.Stop ();
	 
	}



	public void ParseCoord(SocketIOEvent e)
	{
		bool update = false;
		if (e.data == null) { return; }
		//Walabot at horizontal position swap x z raw coordinate
		if (e.data.HasField ("z")) {
			e.data.GetField (ref NX, "z");  // use Z coordinate for X
		}
		if (e.data.HasField ("y")) {
			e.data.GetField (ref NY, "y");
		}
		if (e.data.HasField ("x")) {
			e.data.GetField (ref NZ, "x");
		}
		DebugText.show ("x: " + NX + " y: " + NY, 1);
		// Update camera postion to fit the bounding box
		CameraBound.Update (NX,NY);
	}
		
	public void setBound1(float x, float y) {
		return;
	}

	public void CalibrateInput(SocketIOEvent e)
	{
		initialize = true;
		Debug.Log("calibrating");
	}

	public void Error(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
	}

	public void Close(SocketIOEvent e)
	{	
		updateCursor = false;
		connected = false;
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}


	public void ClearSettings() {
		currentColor = Color.clear;
		currentInstrument = 1;
		currentScaleIndex = 0;
	}




	// Create Zones for previously saved settings

	void AddTriggerZone(  float size =10) {
		DebugText.show ("Add zone size " + size);

		size = Mathf.Clamp (size,10, 50);
		zoneSize = size;

		// Create circle at current position with specified size
		GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		circle.transform.position = new Vector3 ( NX ,NY  , 0.5F);
		circle.transform.localScale = new Vector3(size,size,1);

		circle.GetComponent<Collider> ().isTrigger = true;

		// Assign properties to trigger based on current setting
		TriggerZone trigger = circle.AddComponent<TriggerZone>();
		trigger.ZoneSize = size;
		trigger.GraphicColor = currentColor;
		trigger.SoundIndex = currentInstrument;
		trigger.ScaleIndex = currentScaleIndex;
		trigger.TrackIndex = SynthPlayer._instance.SongIndex;
		trigger.cursor = this;
		triggerZones.Add(circle);

		// Set visual rendering color
		Renderer rend = circle.GetComponent<Renderer>();	 
		rend.material.SetColor ("_TintColor", currentColor);
		rend.material.color = currentColor;

	}
	 
	void ClearZone(bool deleteAll = false) {
 			
		foreach(GameObject trigger in triggerZones.ToArray()) {
			Collider collider = trigger.GetComponent<Collider> ();

			if (deleteAll || collider.bounds.Contains(transform.position))
			{
 				triggerZones.Remove (trigger);
				Destroy (trigger, 2);
			}
		}
		 
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(CameraBound.MIN_EDGE, CameraBound.MAX_EDGE);
		Gizmos.DrawLine(CameraBound.MIN_EDGE, new Vector3(CameraBound.MAX_EDGE.x, CameraBound.MIN_EDGE.y,0));
		Gizmos.DrawLine(CameraBound.MIN_EDGE, new Vector3(CameraBound.MIN_EDGE.x, CameraBound.MAX_EDGE.y,0));
	}

	public void GoToWebLink(string link) {
		Application.OpenURL (link);
	}

	/*********************************************
	 * Keyboard / Mouse UI Controls
	 **********************************************/

	void  MouseControl() {
		// test by activate mouse
		float speed = 40;
		float horzMovement = Input.GetAxisRaw("Mouse X") * speed * Time.deltaTime;
		float vertMovement = Input.GetAxisRaw("Mouse Y") * speed * Time.deltaTime;
		transform.Translate(Vector3.right * horzMovement);
		transform.Translate(Vector3.up * vertMovement);
		if (Input.GetMouseButtonDown(0)) {
			currentColor = RandomColor ();
			ChangeParticleColor (currentColor);
			AddTriggerZone (zoneSize);
		}
		if (Input.GetMouseButtonDown (1)) {
			ClearZone ();
			StopPlay ();
		    // Random settings
			currentInstrument = Random.Range (0, 127);
			PlaySong();

		}
		var wheel = Input.GetAxis("Mouse ScrollWheel");
		if (wheel > 0f)
		{
			zoneSize += 1;
		}
		else if (wheel < 0f)
		{
			zoneSize -= 1;
		}
	}

	Color previousColor = Color.clear;
	void ProcessShortCutKey() {
		if (Input.GetKeyUp (KeyCode.M)) {
			AddTriggerZone (Random.Range (5f, 30f));
		}

		if (Input.GetKeyUp (KeyCode.Period) || Input.GetKeyUp(KeyCode.RightArrow))
			PlaySong ();

		if (Input.GetKeyUp (KeyCode.Comma))
			StopPlay ();

		if (Input.GetKeyUp (KeyCode.LeftArrow)) {
			while (currentColor == previousColor) {
				previousColor = currentColor;
				currentColor = RandomColor ();
			}
			ChangeParticleColor (currentColor);
		}

		if (Input.GetKeyUp (KeyCode.UpArrow)) {
			currentScaleIndex = Random.Range (0, 5);
			ChangeScaleMode (-1);
		}
		if (Input.GetKeyUp (KeyCode.DownArrow)) {
			currentInstrument = Random.Range (0, 127);
			ChangeInstrument (currentInstrument);
  		}
		if (Input.GetKeyUp (KeyCode.Delete)) {
			initialize = true;
		}
		
		if (Input.GetKeyUp (KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
			Application.Quit ();
		
	}


	int midiNote = 60;
  	public void SynthKeyboard() {

		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			SynthPlayer.instance().NoteOFF(0,1,true);
			midiNote -= 12;
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			SynthPlayer.instance().NoteOFF(0,1,true);
			midiNote += 12;
		}
		if (Input.GetKeyDown(KeyCode.RightShift))
		{
			SynthPlayer.instance().NoteOFF(0,1,true);
			midiNote += 12;
		}
		if (Input.GetKeyUp(KeyCode.RightShift))
		{
			SynthPlayer.instance().NoteOFF(0,1,true);
			midiNote -= 12;
		}
		if (Input.GetKeyDown(KeyCode.A))
			SynthPlayer.instance().NoteON(0, midiNote, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.A))
			SynthPlayer.instance().NoteOFF(0, midiNote);
		if (Input.GetKeyDown(KeyCode.W))
			SynthPlayer.instance().NoteON(0, midiNote + 1, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.W))
			SynthPlayer.instance().NoteOFF(0, midiNote + 1);
		if (Input.GetKeyDown(KeyCode.S))
			SynthPlayer.instance().NoteON(0, midiNote + 2, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.S))
			SynthPlayer.instance().NoteOFF(0, midiNote + 2);
		if (Input.GetKeyDown(KeyCode.E))
			SynthPlayer.instance().NoteON(0, midiNote + 3, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.E))
			SynthPlayer.instance().NoteOFF(0, midiNote + 3);
		if (Input.GetKeyDown(KeyCode.D))
			SynthPlayer.instance().NoteON(0, midiNote + 4, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.D))
			SynthPlayer.instance().NoteOFF(0, midiNote + 4);
		if (Input.GetKeyDown(KeyCode.F))
			SynthPlayer.instance().NoteON(0, midiNote + 5, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.F))
			SynthPlayer.instance().NoteOFF(0, midiNote + 5);
		if (Input.GetKeyDown(KeyCode.T))
			SynthPlayer.instance().NoteON(0, midiNote + 6, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.T))
			SynthPlayer.instance().NoteOFF(0, midiNote + 6);
		if (Input.GetKeyDown(KeyCode.G))
			SynthPlayer.instance().NoteON(0, midiNote + 7, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.G))
			SynthPlayer.instance().NoteOFF(0, midiNote + 7);
		if (Input.GetKeyDown(KeyCode.Y))
			SynthPlayer.instance().NoteON(0, midiNote + 8, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.Y))
			SynthPlayer.instance().NoteOFF(0, midiNote + 8);
		if (Input.GetKeyDown(KeyCode.H))
			SynthPlayer.instance().NoteON(0, midiNote + 9, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.H))
			SynthPlayer.instance().NoteOFF(0, midiNote + 9);
		if (Input.GetKeyDown(KeyCode.U))
			SynthPlayer.instance().NoteON(0, midiNote + 10, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.U))
			SynthPlayer.instance().NoteOFF(0, midiNote + 10);
		if (Input.GetKeyDown(KeyCode.J))
			SynthPlayer.instance().NoteON(0, midiNote + 11, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.J))
			SynthPlayer.instance().NoteOFF(0, midiNote + 11);
		if (Input.GetKeyDown(KeyCode.K))
			SynthPlayer.instance().NoteON(0, midiNote + 12, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.K))
			SynthPlayer.instance().NoteOFF(0, midiNote + 12);

		if (Input.GetKeyDown(KeyCode.O))
			SynthPlayer.instance().NoteON(0, midiNote + 13, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.O))
			SynthPlayer.instance().NoteOFF(0, midiNote + 13);

		if (Input.GetKeyDown(KeyCode.L))
			SynthPlayer.instance().NoteON(0, midiNote + 14, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.L))
			SynthPlayer.instance().NoteOFF(0, midiNote + 14);

		if (Input.GetKeyDown(KeyCode.P))
			SynthPlayer.instance().NoteON(0, midiNote + 15, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.P))
			SynthPlayer.instance().NoteOFF(0, midiNote + 15);

		if (Input.GetKeyDown(KeyCode.Semicolon))
			SynthPlayer.instance().NoteON(0, midiNote + 16, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.Semicolon))
			SynthPlayer.instance().NoteOFF(0, midiNote + 16);

		if (Input.GetKeyDown(KeyCode.Quote))
			SynthPlayer.instance().NoteON(0, midiNote + 17, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.Quote))
			SynthPlayer.instance().NoteOFF(0, midiNote + 17);


		if (Input.GetKeyDown(KeyCode.RightBracket))
			SynthPlayer.instance().NoteON(0, midiNote + 18, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.RightBracket))
			SynthPlayer.instance().NoteOFF(0, midiNote + 18);


		if (Input.GetKeyDown(KeyCode.Return))
			SynthPlayer.instance().NoteON(0, midiNote + 19, 127, currentInstrument);
		if (Input.GetKeyUp(KeyCode.Return))
			SynthPlayer.instance().NoteOFF(0, midiNote + 19);

	}

}

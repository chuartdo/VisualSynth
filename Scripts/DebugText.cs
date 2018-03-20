using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour {
	public int id = 0;
	Text textMesh;
	static DebugText _instance;
	static Text[] texts = new Text[5];


	void Awake()
	{
		if (_instance == null)
			_instance = this;
		textMesh = GetComponent<Text>();

	}

	void Start () {
		texts[id] = textMesh;
	}

	static public void show (string message, int myid = 0) {
		if (texts[myid] != null) {
			texts[myid].text = message;
		}
	}

}
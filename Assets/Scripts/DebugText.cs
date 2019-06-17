using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour {

	private Text text;

	// Start is called before the first frame update
	void Start() {
		text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update() {
		
	}

	public void Debug(string debug_message) {
		text.text += debug_message + "\n";
	}

	public void DebugNotReturn(string debug_message) {
		text.text += debug_message;
	}

	public void ClearDebug() {
		text.text = "";
	}
}

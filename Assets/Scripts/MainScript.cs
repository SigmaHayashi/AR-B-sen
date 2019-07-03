using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainScript : MonoBehaviour {

	public bool ScreenNOTSleep = true;

	public Button changeMainSceneButton;
	public Button changeTestSceneButton;

	// Use this for initialization
	void Start () {
		// 画面が消えないようにする
		if (ScreenNOTSleep) {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		else {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}

		if (changeMainSceneButton != null) {
			changeMainSceneButton.onClick.AddListener(changeMainScene);
		}
		if (changeTestSceneButton != null) {
			changeTestSceneButton.onClick.AddListener(changeTestScene);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// 戻るボタンでアプリ終了
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}
	}

	void changeMainScene() {
		SceneManager.LoadScene("AR B-sen");
	}

	void changeTestScene() {
		//SceneManager.LoadScene("Shader Test Scene");
		SceneManager.LoadScene("VICON coordinates Test Scene");
	}
}

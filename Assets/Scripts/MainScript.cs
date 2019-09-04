using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainScript : MonoBehaviour {

	public bool ScreenNOTSleep = true;

	/*
	public Button changeMainSceneButton;
	public Button changeTestSceneButton;
	*/

	private bool capture_mode = false;
	/*
	public Canvas TextCanvas;
	public Canvas ButtonCanvas;
	*/
	public GameObject MainCanvas;
	public GameObject CalibrationCanvas;
	public GameObject MyConsoleCanvas;
	public GameObject DatabaseInfoCanvas;

	private Button ChangeToCalibrationButton;
	private Button ChangeToMyConsoleButton;
	private Button ChangeToDatabaseButton;
	private List<Button> BackToMainButton = new List<Button>();

	private int CanvasState = 0;
	private Dictionary<int, GameObject> CanvasDictionary = new Dictionary<int, GameObject>();

	private Text Calibration_OffsetInfoText;
	private Text Calibration_BsenInfoText;
	private Text Calibration_CameraInfoText;

	private MyConsole mconsole;

	private Text Database_RefrigeratorGoodsTextSample;
	private Text Database_SmartPalBatteryText;
	private Text Database_WHS1InfoText;
	private GameObject Database_WaveGraph;
	private Text Database_ViconIRVSMarkerText;
	private Text Database_ViconSmartPalText;

	// Use this for initialization
	void Start () {
		// 画面が消えないようにする
		if (ScreenNOTSleep) {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		else {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}

		/*
		if (changeMainSceneButton != null) {
			changeMainSceneButton.onClick.AddListener(changeMainScene);
		}
		if (changeTestSceneButton != null) {
			changeTestSceneButton.onClick.AddListener(changeTestScene);
		}
		*/

		//CanvasをDictionaryに追加
		CanvasDictionary.Add(0, MainCanvas);
		CanvasDictionary.Add(1, CalibrationCanvas);
		CanvasDictionary.Add(2, MyConsoleCanvas);
		CanvasDictionary.Add(3, DatabaseInfoCanvas);

		//Canvas移動用ボタンを取得・設定
		ChangeToCalibrationButton = GameObject.Find("Main System/Main Canvas/Change to Calibration Button").GetComponent<Button>();
		ChangeToMyConsoleButton = GameObject.Find("Main System/Main Canvas/Change to MyConsole Button").GetComponent<Button>();
		ChangeToDatabaseButton = GameObject.Find("Main System/Main Canvas/Change to Database Button").GetComponent<Button>();
		BackToMainButton.Add(GameObject.Find("Main System/Calibration Canvas/Button Canvas/Back to Main Button").GetComponent<Button>());
		BackToMainButton.Add(GameObject.Find("Main System/MyConsole Canvas/Back to Main Button").GetComponent<Button>());
		BackToMainButton.Add(GameObject.Find("Main System/Database Info Canvas/Back to Main Button").GetComponent<Button>());

		ChangeToCalibrationButton.onClick.AddListener(ChangeToCalibration);
		ChangeToMyConsoleButton.onClick.AddListener(ChangeToMyConsole);
		ChangeToDatabaseButton.onClick.AddListener(ChangeToDatabase);
		foreach(Button button in BackToMainButton) {
			button.onClick.AddListener(BackToMain);
		}

		//Calibration Canvasのオブジェクトを取得

		//MyConsole Canvasのオブジェクトを取得

		//Database Info Canvasのオブジェクトを取得


		//Main Canvasのみ表示
		foreach (KeyValuePair<int, GameObject> Canvas in CanvasDictionary) {
			if(Canvas.Key != 0) {
				Canvas.Value.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		// 戻るボタンでアプリ終了
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}

		//5本指タッチでキャプチャモードON/OFF切り替え
		//UnityEditor上では右クリック
		if (!Application.isEditor) {
			if(Input.touchCount >= 5 && CanvasState == 0) {
				Touch touch = Input.GetTouch(Input.touchCount - 1);
				if(touch.phase == TouchPhase.Began) {
					capture_mode = !capture_mode;
					if (capture_mode) {
						/*
						TextCanvas.gameObject.SetActive(false);
						ButtonCanvas.gameObject.SetActive(false);
						*/
						MainCanvas.SetActive(false);
					}
					else {
						/*
						TextCanvas.gameObject.SetActive(true);
						ButtonCanvas.gameObject.SetActive(true);
						*/
						MainCanvas.SetActive(true);
					}
				}
			}
		}
		else {
			if (Input.GetMouseButtonDown(1) && CanvasState == 0) {
				capture_mode = !capture_mode;
				if (capture_mode) {
					MainCanvas.SetActive(false);
				}
				else {
					MainCanvas.SetActive(true);
				}
			}
		}
	}

	/**************************************************
	 * どのCanvas]を使用中か返す
	 **************************************************/
	public string CheckCanvasState() {
		return CanvasDictionary[CanvasState].name;
	}

	/**************************************************
	 * 画面の切り替え
	 **************************************************/
	void ChangeToCalibration() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[1].SetActive(true);
		CanvasState = 1;
	}

	void ChangeToMyConsole() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[2].SetActive(true);
		CanvasState = 2;
	}

	void ChangeToDatabase() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[3].SetActive(true);
		CanvasState = 3;
	}

	void BackToMain() {
		CanvasDictionary[1].SetActive(false);
		CanvasDictionary[2].SetActive(false);
		CanvasDictionary[3].SetActive(false);
		CanvasDictionary[0].SetActive(true);
		CanvasState = 0;
	}

	/**************************************************
	 * Calibration CanvasのAPI
	 **************************************************/
	

	/**************************************************
	 * MyConsole CanvasのAPI
	 **************************************************/
	

	/**************************************************
	 * Database Info CanvasのAPI
	 **************************************************/
	


	/*
	void changeMainScene() {
		SceneManager.LoadScene("AR B-sen");
	}

	void changeTestScene() {
		//SceneManager.LoadScene("Shader Test Scene");
		SceneManager.LoadScene("VICON coordinates Test Scene");
	}
	*/
}

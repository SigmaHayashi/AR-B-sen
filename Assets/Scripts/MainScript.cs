using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainScript : MonoBehaviour {

	//スクリーンが消えないようにする
	public bool ScreenNOTSleep = true;
	
	//キャプチャモードかどうか
	private bool capture_mode = false;

	//Canvasたち
	public GameObject MainCanvas;
	public GameObject CalibrationCanvas;
	public GameObject MyConsoleCanvas;
	public GameObject DatabaseInfoCanvas;

	//Canvasを遷移させるボタンたち
	private Button ChangeToCalibrationButton;
	private Button ChangeToMyConsoleButton;
	private Button ChangeToDatabaseButton;
	private List<Button> BackToMainButton = new List<Button>();

	//いまどのCanvasを使用中か示す変数，それに対応する辞書
	private int CanvasState = 0;
	private Dictionary<int, GameObject> CanvasDictionary = new Dictionary<int, GameObject>();

	//Main CanvasのUI
	private Text Main_InfoText;
	private string Main_InfoText_Buffer;

	//Calibration CanvasのUI
	private Text Calibration_OffsetInfoText;
	//private Text Calibration_BsenInfoText;
	private Text Calibration_CameraInfoText;
	private Text Calibration_DeviceInfoText;
	private string Calibration_OffsetInfoText_Buffer;
	//private string Calibration_BsenInfoText_Buffer;
	private string Calibration_CameraInfoText_Buffer;
	private string Calibration_DeviceInfoText_Buffer;
	private Button Calibration_PosXPlusButton;
	private Button Calibration_PosXMinusButton;
	private Button Calibration_PosYPlusButton;
	private Button Calibration_PosYMinusButton;
	private Button Calibration_PosZPlusButton;
	private Button Calibration_PosZMinusButton;
	private Button Calibration_RotRightButton;
	private Button Calibration_RotLeftButton;
	//private BsenCalibrationSystem CalibSystem;
	private bool Calibration_push_PoxXPlus = false;
	private bool Calibration_push_PoxXMinus = false;
	private bool Calibration_push_PoxYPlus = false;
	private bool Calibration_push_PoxYMinus = false;
	private bool Calibration_push_PoxZPlus = false;
	private bool Calibration_push_PoxZMinus = false;
	private bool Calibration_push_RotRight = false;
	private bool Calibration_push_RotLeft = false;

	//MyConsole CanvasのUI
	private MyConsole myconsole;
	private List<object> myconsole_Buffer = new List<object>();
	private bool myconsole_Delete = false;

	//Database Info CanvasのUI
	private GameObject Database_RefrigeratorGoodsTextSample;
	private Dictionary<int, GameObject> Database_RefrigeratorGoodsTextDictionary = new Dictionary<int, GameObject>();
	private Text Database_SmartPalBatteryText;
	private Text Database_WHS1InfoText;
	private GameObject Database_WHS1WaveGraph;
	private Text Database_ViconIRVSMarkerText;
	private Text Database_ViconSmartPalText;
	private Dictionary<int, string> Database_RefrigeratorGoodsText_BufferDictionary = new Dictionary<int, string>();
	private string Database_SmartPalBatteryText_Buffer;
	private string Database_WHS1InfoText_Buffer;
	private int[] Database_WHS1WaveGraph_Buffer;
	private string Database_ViconIRVSMarkerText_Buffer;
	private string Database_ViconSmartPalText_Buffer;

	// Use this for initialization
	void Start () {
		// 画面が消えないようにする
		if (ScreenNOTSleep) {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		else {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}

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

		//Main Canvasのオブジェクトを取得
		Main_InfoText = GameObject.Find("Main System/Main Canvas/Info Text").GetComponent<Text>();

		//Calibration Canvasのオブジェクトを取得，ボタンにキャリブシステムの機能を持たせる
		Calibration_OffsetInfoText = GameObject.Find("Main System/Calibration Canvas/Text Canvas/Offset Position Text").GetComponent<Text>();
		//Calibration_BsenInfoText = GameObject.Find("Main System/Calibration Canvas/Text Canvas/B-sen Position Text").GetComponent<Text>();
		Calibration_CameraInfoText = GameObject.Find("Main System/Calibration Canvas/Text Canvas/Camera Position Text").GetComponent<Text>();
		Calibration_DeviceInfoText = GameObject.Find("Main System/Calibration Canvas/Text Canvas/Device Position Text").GetComponent<Text>();

		Calibration_PosXPlusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos X+ Button").GetComponent<Button>();
		Calibration_PosXMinusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos X- Button").GetComponent<Button>();
		Calibration_PosYPlusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos Y+ Button").GetComponent<Button>();
		Calibration_PosYMinusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos Y- Button").GetComponent<Button>();
		Calibration_PosZPlusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos Z+ Button").GetComponent<Button>();
		Calibration_PosZMinusButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/pos Z- Button").GetComponent<Button>();
		Calibration_RotRightButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/rot Right Button").GetComponent<Button>();
		Calibration_RotLeftButton = GameObject.Find("Main System/Calibration Canvas/Button Canvas/rot Left Button").GetComponent<Button>();

		//CalibSystem = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
		/*
		Calibration_PosXPlusButton.onClick.AddListener(CalibSystem.onPosXplusClick);
		Calibration_PosXMinusButton.onClick.AddListener(CalibSystem.onPosXminusClick);
		Calibration_PosYPlusButton.onClick.AddListener(CalibSystem.onPosYplusClick);
		Calibration_PosYMinusButton.onClick.AddListener(CalibSystem.onPosYminusClick);
		Calibration_PosZPlusButton.onClick.AddListener(CalibSystem.onPosZplusClick);
		Calibration_PosZMinusButton.onClick.AddListener(CalibSystem.onPosZminusClick);
		Calibration_RotRightButton.onClick.AddListener(CalibSystem.onRotRightClick);
		Calibration_RotLeftButton.onClick.AddListener(CalibSystem.onRotLeftClick);
		*/
		AddTrigger(Calibration_PosXPlusButton);
		AddTrigger(Calibration_PosXMinusButton);
		AddTrigger(Calibration_PosYPlusButton);
		AddTrigger(Calibration_PosYMinusButton);
		AddTrigger(Calibration_PosZPlusButton);
		AddTrigger(Calibration_PosZMinusButton);
		AddTrigger(Calibration_RotRightButton);
		AddTrigger(Calibration_RotLeftButton);


		//MyConsole Canvasのオブジェクトを取得
		myconsole = GameObject.Find("Main System/MyConsole Canvas/Console Panel").GetComponent<MyConsole>();

		//Database Info Canvasのオブジェクトを取得
		Database_RefrigeratorGoodsTextSample = GameObject.Find("Main System/Database Info Canvas/Refrigerator Goods Info/Sample Text");
		Database_SmartPalBatteryText = GameObject.Find("Main System/Database Info Canvas/SmartPal Battery Text").GetComponent<Text>();
		Database_WHS1InfoText = GameObject.Find("Main System/Database Info Canvas/WHS1 Info/Temp and Rate Text").GetComponent<Text>();
		Database_WHS1WaveGraph = GameObject.Find("Main System/Database Info Canvas/WHS1 Info/Wave Graph");
		Database_ViconIRVSMarkerText = GameObject.Find("Main System/Database Info Canvas/VICON Info/IRVS Marker Text").GetComponent<Text>();
		Database_ViconSmartPalText = GameObject.Find("Main System/Database Info Canvas/VICON Info/SmartPal Text").GetComponent<Text>();

		//Main Canvasのみ表示
		foreach (KeyValuePair<int, GameObject> Canvas in CanvasDictionary) {
			if(Canvas.Key != 0) {
				Canvas.Value.SetActive(false);
			}
		}
	}

	//キャリブレーション用ボタンに機能を持たせる
	void AddTrigger(Button button) {
		EventTrigger trigger = button.GetComponent<EventTrigger>();
		EventTrigger.Entry entry_down = new EventTrigger.Entry();
		entry_down.eventID = EventTriggerType.PointerDown;
		EventTrigger.Entry entry_up = new EventTrigger.Entry();
		entry_up.eventID = EventTriggerType.PointerUp;
		switch (button.name.ToString()) {
			case "pos X+ Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxXPlus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxXPlus = false; });
				break;
			case "pos X- Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxXMinus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxXMinus = false; });
				break;
			case "pos Y+ Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxYPlus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxYPlus = false; });
				break;
			case "pos Y- Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxYMinus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxYMinus = false; });
				break;
			case "pos Z+ Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxZPlus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxZPlus = false; });
				break;
			case "pos Z- Button":
				entry_down.callback.AddListener((x) => { Calibration_push_PoxZMinus = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_PoxZMinus = false; });
				break;
			case "rot Right Button":
				entry_down.callback.AddListener((x) => { Calibration_push_RotRight = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_RotRight = false; });
				break;
			case "rot Left Button":
				entry_down.callback.AddListener((x) => { Calibration_push_RotLeft = true; });
				entry_up.callback.AddListener((x) => { Calibration_push_RotLeft = false; });
				break;
		}
		trigger.triggers.Add(entry_down);
		trigger.triggers.Add(entry_up);
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
						MainCanvas.SetActive(false);
					}
					else {
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
	 * どのCanvasを使用中か返す
	 **************************************************/
	public string CheckCanvasState() {
		return CanvasDictionary[CanvasState].name;
	}

	/**************************************************
	 * 画面の切り替え：Calibration Canvas
	 **************************************************/
	void ChangeToCalibration() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[1].SetActive(true);
		CanvasState = 1;

		if(Calibration_OffsetInfoText_Buffer != null) {
			Calibration_OffsetInfoText.text = Calibration_OffsetInfoText_Buffer;
			Calibration_OffsetInfoText_Buffer = null;
		}
		/*
		if(Calibration_BsenInfoText_Buffer != null){
			Calibration_BsenInfoText.text = Calibration_BsenInfoText_Buffer;
			Calibration_BsenInfoText_Buffer = null;
		}
		*/
		if (Calibration_CameraInfoText_Buffer != null) {
			Calibration_CameraInfoText.text = Calibration_CameraInfoText_Buffer;
			Calibration_CameraInfoText_Buffer = null;
		}
		if (Calibration_DeviceInfoText_Buffer != null) {
			Calibration_DeviceInfoText.text = Calibration_DeviceInfoText_Buffer;
			Calibration_DeviceInfoText_Buffer = null;
		}
	}

	/**************************************************
	 * 画面の切り替え：MyConsole Canvas
	 **************************************************/
	void ChangeToMyConsole() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[2].SetActive(true);
		CanvasState = 2;

		if (myconsole_Delete) {
			myconsole.Delete();
			myconsole_Delete = false;
		}
		foreach(object message in myconsole_Buffer) {
			myconsole.Add(message);
		}
		myconsole_Buffer = new List<object>();
	}

	/**************************************************
	 * 画面の切り替え：Database Info Canvas
	 **************************************************/
	void ChangeToDatabase() {
		CanvasDictionary[0].SetActive(false);
		CanvasDictionary[3].SetActive(true);
		CanvasState = 3;

		if(Database_RefrigeratorGoodsText_BufferDictionary.Count > 0) {
			foreach (KeyValuePair<int, string> goods_info in Database_RefrigeratorGoodsText_BufferDictionary) {
				if (Database_RefrigeratorGoodsTextDictionary.ContainsKey(goods_info.Key)) {
					Database_RefrigeratorGoodsTextDictionary[goods_info.Key].GetComponent<Text>().text = goods_info.Value;
				}
				else {
					GameObject new_text = Instantiate(Database_RefrigeratorGoodsTextSample);

					new_text.name = "Info of " + goods_info.Key.ToString();
					new_text.transform.SetParent(GameObject.Find("Main System/Database Info Canvas/Refrigerator Goods Info").transform, false);

					RectTransform sample_rect = Database_RefrigeratorGoodsTextSample.GetComponent<RectTransform>();
					float new_posY = sample_rect.anchoredPosition.y;
					foreach (GameObject goods_info_text in Database_RefrigeratorGoodsTextDictionary.Values) {
						new_posY = goods_info_text.GetComponent<RectTransform>().anchoredPosition.y;
					}
					new_posY -= sample_rect.sizeDelta.y;
					Vector2 new_pos = new Vector2(sample_rect.anchoredPosition.x, new_posY);
					new_text.GetComponent<RectTransform>().anchoredPosition = new_pos;
					new_text.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

					new_text.GetComponent<Text>().text = goods_info.Value;

					Database_RefrigeratorGoodsTextDictionary.Add(goods_info.Key, new_text);
				}
			}
		}
		Database_RefrigeratorGoodsText_BufferDictionary = new Dictionary<int, string>();

		if(Database_SmartPalBatteryText_Buffer != null) {
			Database_SmartPalBatteryText.text = Database_SmartPalBatteryText_Buffer;
		}
		Database_SmartPalBatteryText_Buffer = null;

		if(Database_ViconIRVSMarkerText_Buffer != null) {
			Database_ViconIRVSMarkerText.text = Database_ViconIRVSMarkerText_Buffer;
		}
		Database_ViconIRVSMarkerText_Buffer = null;

		if(Database_ViconSmartPalText_Buffer != null) {
			Database_ViconSmartPalText.text = Database_ViconSmartPalText_Buffer;
		}
		Database_ViconSmartPalText_Buffer = null;

		if(Database_WHS1InfoText_Buffer != null) {
			Database_WHS1InfoText.text = Database_WHS1InfoText_Buffer;
		}
		Database_WHS1InfoText_Buffer = null;

		if(Database_WHS1WaveGraph_Buffer != null) {
			List<GameObject> line_list = new List<GameObject>();
			GetAllChildren(Database_WHS1WaveGraph, ref line_list);
			foreach (GameObject obj in line_list) {
				Destroy(obj);
			}
			
			GameObject line_prefab = (GameObject)Resources.Load("UI Line");
			for (int n = 0; n < Database_WHS1WaveGraph_Buffer.Length - 1; n++) {
				GameObject line = Instantiate(line_prefab);
				line.name = "UI Line" + n.ToString();
				line.transform.SetParent(Database_WHS1WaveGraph.transform, false);

				RectTransform line_rect = line.GetComponent<RectTransform>();
				Vector2 start = new Vector2(n * 550.0f / Database_WHS1WaveGraph_Buffer.Length, Database_WHS1WaveGraph_Buffer[n] * 400.0f / 1000.0f);
				Vector2 end = new Vector2((n + 1) * 550.0f / Database_WHS1WaveGraph_Buffer.Length, Database_WHS1WaveGraph_Buffer[n + 1] * 400.0f / 1000.0f);
				Vector2 vector = new Vector2(end.x - start.x, end.y - start.y);

				float vector_size = Vector2.Distance(start, end);
				float vector_angle = Mathf.Rad2Deg * Mathf.Atan2(vector.y, vector.x);

				line_rect.anchoredPosition = start;
				line_rect.sizeDelta = new Vector2(line_rect.sizeDelta.x, vector_size * 1.1f);
				line_rect.localEulerAngles = new Vector3(0, 0, vector_angle - 90);
				line_rect.localScale = new Vector3(1, 1, 1);
			}
		}
		Database_WHS1WaveGraph_Buffer = null;
	}

	/**************************************************
	 * 画面の切り替え：Main Canvas
	 **************************************************/
	void BackToMain() {
		CanvasDictionary[1].SetActive(false);
		CanvasDictionary[2].SetActive(false);
		CanvasDictionary[3].SetActive(false);
		CanvasDictionary[0].SetActive(true);
		CanvasState = 0;

		if(Main_InfoText_Buffer != null) {
			Main_InfoText.text = Main_InfoText_Buffer;
			Main_InfoText_Buffer = null;
		}
	}

	/**************************************************
	 * Main CanvasのAPI
	 **************************************************/
	public void UpdateMainCanvasInfoText(string message) {
		if(CheckCanvasState() == MainCanvas.name) {
			Main_InfoText.text = message;
		}
		else {
			Main_InfoText_Buffer = message;
		}
	}

	/**************************************************
	 * Calibration CanvasのAPI
	 **************************************************/
	public void UpdateCalibrationInfoAll(Vector3 offset_pos, Vector3 offset_rot, 
										//Vector3 bsen_pos, Vector3 bsen_rot, 
										Vector3 camera_pos, Vector3 camera_rot,
										Vector3 device_pos, Vector3 device_rot) {
		if(CheckCanvasState() == CalibrationCanvas.name) {
			Calibration_OffsetInfoText.text = "Offset Pos: " + offset_pos.ToString("f3") + "\nOffset Rot: " + offset_rot.ToString("f2");
			//Calibration_BsenInfoText.text = "B-sen Pos: " + bsen_pos.ToString("f3") + "\nB-sen Rot: " + bsen_rot.ToString("f2");
			Calibration_CameraInfoText.text = "Camera Pos: " + camera_pos.ToString("f3") + "\nCamera Rot: " + camera_rot.ToString("f2");
			Calibration_DeviceInfoText.text = "Device Pos: " + device_pos.ToString("f3") + "\nDevice Rot: " + device_rot.ToString("f2");
		}
		else {
			Calibration_OffsetInfoText_Buffer = "Offset Pos: " + offset_pos.ToString("f3") + "\nOffset Rot: " + offset_rot.ToString("f2");
			//Calibration_BsenInfoText_Buffer = "B-sen Pos: " + bsen_pos.ToString("f3") + "\nB-sen Rot: " + bsen_rot.ToString("f2");
			Calibration_CameraInfoText_Buffer = "Camera Pos: " + camera_pos.ToString("f3") + "\nCamera Rot: " + camera_rot.ToString("f2");
			Calibration_DeviceInfoText_Buffer = "Device Pos: " + device_pos.ToString("f3") + "\nDevice Rot: " + device_rot.ToString("f2");
		}
	}

	public void UpdateCalibrationInfoOffset(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == "Calibration Canvas") {
			Calibration_OffsetInfoText.text = "Offset Pos: " + pos.ToString("f3") + "\nOffset Rot: " + rot.ToString("f2");
		}
		else {
			Calibration_OffsetInfoText_Buffer = "Offset Pos: " + pos.ToString("f3") + "\nOffset Rot: " + rot.ToString("f2");
		}
	}

	/*
	public void UpdateCalibrationInfoBsen(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == "Calibration Canvas") {
			Calibration_BsenInfoText.text = "B-sen Pos: " + pos.ToString("f3") + "\nB-sen Rot: " + rot.ToString("f2");
		}
		else {
			Calibration_BsenInfoText_Buffer = "B-sen Pos: " + pos.ToString("f3") + "\nB-sen Rot: " + rot.ToString("f2");
		}
	}
	*/

	public void UpdateCalibrationInfoCamera(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == "Calibration Canvas") {
			Calibration_CameraInfoText.text = "Camera Pos: " + pos.ToString("f3") + "\nCamera Rot: " + rot.ToString("f2");
		}
		else {
			Calibration_CameraInfoText_Buffer = "Camera Pos: " + pos.ToString("f3") + "\nCamera Rot: " + rot.ToString("f2");
		}
	}

	public void UpdateCalibrationInfoDevice(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == "Calibration Canvas") {
			Calibration_DeviceInfoText.text = "Device Pos: " + pos.ToString("f3") + "\nDevice Rot: " + rot.ToString("f2");
		}
		else {
			Calibration_DeviceInfoText_Buffer = "Device Pos: " + pos.ToString("f3") + "\nDevice Rot: " + rot.ToString("f2");
		}
	}

	public List<string> checkCalibrationCanvasButton() {
		List<string> button_push_list = new List<string>();
		if (Calibration_push_PoxXPlus) {
			button_push_list.Add(Calibration_PosXPlusButton.name);
		}
		if (Calibration_push_PoxXMinus) {
			button_push_list.Add(Calibration_PosXMinusButton.name);
		}
		if (Calibration_push_PoxYPlus) {
			button_push_list.Add(Calibration_PosYPlusButton.name);
		}
		if (Calibration_push_PoxYMinus) {
			button_push_list.Add(Calibration_PosYMinusButton.name);
		}
		if (Calibration_push_PoxZPlus) {
			button_push_list.Add(Calibration_PosZPlusButton.name);
		}
		if (Calibration_push_PoxZMinus) {
			button_push_list.Add(Calibration_PosZMinusButton.name);
		}
		if (Calibration_push_RotRight) {
			button_push_list.Add(Calibration_RotRightButton.name);
		}
		if (Calibration_push_RotLeft) {
			button_push_list.Add(Calibration_RotLeftButton.name);
		}
		return button_push_list;
	}

	/**************************************************
	 * MyConsole CanvasのAPI
	 **************************************************/
	public void MyConsole_Add(object message) {
		if(CheckCanvasState() == MyConsoleCanvas.name) {
			myconsole.Add(message);
		}
		else {
			myconsole_Buffer.Add(message);
		}
	}

	public void MyConsole_Delete() {
		if (CheckCanvasState() == MyConsoleCanvas.name) {
			myconsole.Delete();
		}
		else {
			myconsole_Delete = true;
			myconsole_Buffer = new List<object>();
		}
	}

	/**************************************************
	 * Database Info CanvasのAPI
	 **************************************************/
	public void UpdateDatabaseInfoRefrigerator(Dictionary<int, string> goods_info_dictonary) {
		if(CheckCanvasState() == DatabaseInfoCanvas.name) {
			foreach(KeyValuePair<int, string> goods_info in goods_info_dictonary) {
				if (Database_RefrigeratorGoodsTextDictionary.ContainsKey(goods_info.Key)) {
					Database_RefrigeratorGoodsTextDictionary[goods_info.Key].GetComponent<Text>().text = goods_info.Value;
				}
				else {
					GameObject new_text = Instantiate(Database_RefrigeratorGoodsTextSample);

					new_text.name = "Info of " + goods_info.Key.ToString();
					new_text.transform.SetParent(GameObject.Find("Main System/Database Info Canvas/Refrigerator Goods Info").transform, false);

					RectTransform sample_rect = Database_RefrigeratorGoodsTextSample.GetComponent<RectTransform>();
					float new_posY = sample_rect.anchoredPosition.y;
					foreach (GameObject goods_info_text in Database_RefrigeratorGoodsTextDictionary.Values) {
						new_posY = goods_info_text.GetComponent<RectTransform>().anchoredPosition.y;
					}
					new_posY -= sample_rect.sizeDelta.y;
					Vector2 new_pos = new Vector2(sample_rect.anchoredPosition.x, new_posY);
					new_text.GetComponent<RectTransform>().anchoredPosition = new_pos;
					new_text.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

					new_text.GetComponent<Text>().text = goods_info.Value;

					Database_RefrigeratorGoodsTextDictionary.Add(goods_info.Key, new_text);
				}
			}
		}
		else {
			Database_RefrigeratorGoodsText_BufferDictionary = goods_info_dictonary;
		}
	}

	public void UpdateDatabaseInfoSmartPalBattery(float battery_per) {
		if(CheckCanvasState() == DatabaseInfoCanvas.name) {
			Database_SmartPalBatteryText.text = "SmartPal Battery: " + battery_per.ToString("f1") + " [%]";
		}
		else {
			Database_SmartPalBatteryText_Buffer = "SmartPal Battery: " + battery_per.ToString("f1") + " [%]";
		}
	}

	public void UpdateDatabaseInfoViconIRVSMarker(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == DatabaseInfoCanvas.name) {
			Database_ViconIRVSMarkerText.text = "IRVS Marker\n  Pos: " + pos.ToString("f3") + "\n  Rot: " + rot.ToString("f2");
		}
		else {
			Database_ViconIRVSMarkerText_Buffer = "IRVS Marker\n  Pos: " + pos.ToString("f3") + "\n  Rot: " + rot.ToString("f2");
		}
	}

	public void UpdateDatabaseInfoViconSmartPal(Vector3 pos, Vector3 rot) {
		if (CheckCanvasState() == DatabaseInfoCanvas.name) {
			Database_ViconSmartPalText.text = "SmartPal\n  Pos: " + pos.ToString("f3") + "\n  Rot: " + rot.ToString("f2");
		}
		else {
			Database_ViconSmartPalText_Buffer = "SmartPal\n  Pos: " + pos.ToString("f3") + "\n  Rot: " + rot.ToString("f2");
		}
	}

	public void UpdateDatabaseInfoWHS1Info(float temp, int rate) {
		if (CheckCanvasState() == DatabaseInfoCanvas.name) {
			Database_WHS1InfoText.text = "Temp: " + temp.ToString("f2") + " [degC]\nRate: " + rate.ToString() + " [bpm]";
		}
		else {
			Database_WHS1InfoText_Buffer = "Temp: " + temp.ToString("f2") + " [degC]\nRate: " + rate.ToString() + " [bpm]";
		}
	}

	public void UpdateDatabaseInfoWHS1Wave(int[] wave_list) {
		if (CheckCanvasState() == DatabaseInfoCanvas.name) {
			List<GameObject> line_list = new List<GameObject>();
			GetAllChildren(Database_WHS1WaveGraph, ref line_list);
			foreach (GameObject obj in line_list) {
				Destroy(obj);
			}
			
			GameObject line_prefab = (GameObject)Resources.Load("UI Line");
			for(int n = 0; n < wave_list.Length - 1; n++) {
				GameObject line = Instantiate(line_prefab);
				line.name = "UI Line" + n.ToString();
				line.transform.SetParent(Database_WHS1WaveGraph.transform, false);

				RectTransform line_rect = line.GetComponent<RectTransform>();
				Vector2 start = new Vector2(n * 550.0f / wave_list.Length, wave_list[n] * 400.0f / 1000.0f);
				Vector2 end = new Vector2((n + 1) * 550.0f / wave_list.Length, wave_list[n + 1] * 400.0f / 1000.0f);
				Vector2 vector = new Vector2(end.x - start.x, end.y - start.y);

				float vector_size = Vector2.Distance(start, end);
				float vector_angle = Mathf.Rad2Deg * Mathf.Atan2(vector.y, vector.x);

				line_rect.anchoredPosition = start;
				line_rect.sizeDelta = new Vector2(line_rect.sizeDelta.x, vector_size * 1.1f);
				line_rect.localEulerAngles = new Vector3(0, 0, vector_angle - 90);
				line_rect.localScale = new Vector3(1, 1, 1);
			}
		}
		else {
			Database_WHS1WaveGraph_Buffer = wave_list;
		}
	}

	/*****************************************************************
	 * すべての子オブジェクトを取得
	 *****************************************************************/
	void GetAllChildren(GameObject obj, ref List<GameObject> all_children) {
		Transform children = obj.GetComponentInChildren<Transform>();
		if (children.childCount == 0) {
			return;
		}
		foreach (Transform ob in children) {
			all_children.Add(ob.gameObject);
			GetAllChildren(ob.gameObject, ref all_children);
		}
	}
}

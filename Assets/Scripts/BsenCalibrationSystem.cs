using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class BsenCalibrationSystem : MonoBehaviour {
	
	//GameObjectたち
	private GameObject bsen_model;
	private GameObject coordinates_adapter;
	private GameObject irvs_marker;

	//ボタンとテキストたち
	private Button posXplusButton;
	private Button posXminusButton;
	private Button posYplusButton;
	private Button posYminusButton;
	private Button posZplusButton;
	private Button posZminusButton;

	private Button rotXplusButton;
	private Button rotXminusButton;
	private Button rotYplusButton;
	private Button rotYminusButton;
	private Button rotZplusButton;
	private Button rotZminusButton;

	private Button autoPositioningButton;

	private Text cameraPositionText;
	private Text bsenPositionText;
	private Text debugText;

	//AugmentedImageでつかうものたち
	private List<AugmentedImage> m_AugmentedImages = new List<AugmentedImage>();
	private bool detected_marker = false;
	private AugmentedImage marker_image;

	[NonSerialized]
	public int calibration_state = 0;

	/*
	//Android Ros Socket Client関連
	private AndroidRosSocketClient wsc;
	private string srvName = "tms_db_reader";
	private TmsDBReq srvReq = new TmsDBReq();
	private string srvRes;

	float time;
	*/

	private TMSDatabaseAdapter DBAdapter;

	private ShaderChange rostms_shader;

	// Start is called before the first frame update
	// 最初の1回呼び出されるよ～
	void Start() {

		bsen_model = GameObject.Find("rostms");
		rostms_shader = bsen_model.GetComponent<ShaderChange>();

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");

		coordinates_adapter = Instantiate(prefab);
		coordinates_adapter.transform.parent = bsen_model.transform;

		ButtonTextSetting();

		calibration_state = 1;

		/*
		//ROSTMSに接続
		wsc = GameObject.Find("Android Ros Socket Client").GetComponent<AndroidRosSocketClient>();
		//srvReq.tmsdb = new tmsdb("ID_SENSOR", 7030, 3001);
		//wsc.ServiceCallerDB(srvName, srvReq);
		time = 0.0f;
		*/

		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();
	}

	// Update is called once per frame
	//ずっと繰り返し呼び出されるよ～
	void Update() {
		debug("state: " + calibration_state.ToString());
		//phase 0
		//毎回すること
		//AugmentedImageの更新
		//CameraとB-senのポジション表示
		Session.GetTrackables<AugmentedImage>(m_AugmentedImages, TrackableQueryFilter.Updated);

		cameraPositionText.text = "Camera Position : " + Camera.main.transform.position.ToString("f2") + "\n";
		cameraPositionText.text += "Camera Rotation : " + Camera.main.transform.eulerAngles.ToString();

		bsenPositionText.text = "B-sen Position : " + bsen_model.transform.localPosition.ToString("f2") + "\n";
		bsenPositionText.text += "B-sen Rotation : " + bsen_model.transform.eulerAngles.ToString();

		switch (calibration_state) {
			//phase1
			//ROSTMSにアクセスしてマーカーの座標取得
			//irvs_marker = Instantiate(prefab); //マーカーの座標にオブジェクトを配置
			case 1:
				/*
				time += Time.deltaTime;
				if(time > 3.0f) {
					time = 0.0f;
					Debug.Log("Retry...");

					wsc.Connect();

					wsc.ServiceCallerDB(srvName, srvReq);
				}
				if(wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
					srvRes = wsc.GetSrvResMsg();
					Debug.Log("ROS: " + srvRes);

					ServiceResponseDB responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);

					//位置を取得＆変換
					Vector3 marker_position = new Vector3((float)responce.values.tmsdb[0].x, (float)responce.values.tmsdb[0].y, (float)responce.values.tmsdb[0].z);
					marker_position = Ros2UnityPosition(marker_position);
					marker_position.z += 0.2f;
					Debug.Log("Marker Pos: " + marker_position);

					//回転を取得＆変換
					Vector3 marker_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
					marker_euler = Ros2UnityRotation(marker_euler);
					//Debug.Log("Marker rot raw: " + marker_euler);

					marker_euler *= -1.0f;
					marker_euler.x = 0.0f;
					marker_euler.z = 0.0f;
					Debug.Log("Marker rot: " + marker_euler);

					//回転をモデルに適用
					bsen_model.transform.eulerAngles = marker_euler;

					//位置をモデル上のマーカーに適用
					GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
					irvs_marker = Instantiate(prefab);
					irvs_marker.transform.parent = GameObject.Find("rostms/world_link").transform;
					irvs_marker.transform.localPosition = marker_position;

					//回転軸をマーカーの位置に合わせる
					GameObject world_link = GameObject.Find("rostms/world_link");
					world_link.transform.localPosition = marker_position * -1;

					time = 0.0f;
					calibration_state = 2;
				}
				*/
				if (!DBAdapter.access_db) {
					IEnumerator coroutine = DBAdapter.ReadMarkerPos();
					StartCoroutine(coroutine);
				}
				if (DBAdapter.success_access) {
					ServiceResponseDB responce = DBAdapter.responce;
					DBAdapter.FinishReadData();
					
					//位置を取得＆変換
					Vector3 marker_position = new Vector3((float)responce.values.tmsdb[0].x, (float)responce.values.tmsdb[0].y, (float)responce.values.tmsdb[0].z);
					marker_position = Ros2UnityPosition(marker_position);
					marker_position.z += 0.2f;
					Debug.Log("Marker Pos: " + marker_position);

					//回転を取得＆変換
					Vector3 marker_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
					marker_euler = Ros2UnityRotation(marker_euler);
					//Debug.Log("Marker rot raw: " + marker_euler);

					marker_euler *= -1.0f;
					marker_euler.x = 0.0f;
					marker_euler.z = 0.0f;
					Debug.Log("Marker rot: " + marker_euler);

					//回転をモデルに適用
					bsen_model.transform.eulerAngles = marker_euler;

					//位置をモデル上のマーカーに適用
					GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
					irvs_marker = Instantiate(prefab);
					irvs_marker.transform.parent = GameObject.Find("rostms/world_link").transform;
					irvs_marker.transform.localPosition = marker_position;

					//回転軸をマーカーの位置に合わせる
					GameObject world_link = GameObject.Find("rostms/world_link");
					world_link.transform.localPosition = marker_position * -1;

					calibration_state = 2;
				}
				break;

			//phase2
			//画像認識待ち
			//画像認識できたらその場で自動キャリブレーション
			case 2:
			if (!detected_marker) {
				foreach (var image in m_AugmentedImages) {
					if (image.TrackingState == TrackingState.Tracking) {
						detected_marker = true;
						marker_image = image;

						autoPositioning();

						rostms_shader.alpha = 0.6f;
						rostms_shader.ChangeColors();

						calibration_state = 3;
					}
				}
			}
			break;
		}
	}
	
	/*****************************************************************
	 * ボタンとテキストを取得
	 * ボタンにクリック時の動作を設定
	 *****************************************************************/
	void ButtonTextSetting() {
		posXplusButton = GameObject.Find("Main System/Button Canvas/pos X+ Button").GetComponent<Button>();
		posXminusButton = GameObject.Find("Main System/Button Canvas/pos X- Button").GetComponent<Button>();
		posYplusButton = GameObject.Find("Main System/Button Canvas/pos Y+ Button").GetComponent<Button>();
		posYminusButton = GameObject.Find("Main System/Button Canvas/pos Y- Button").GetComponent<Button>();
		posZplusButton = GameObject.Find("Main System/Button Canvas/pos Z+ Button").GetComponent<Button>();
		posZminusButton = GameObject.Find("Main System/Button Canvas/pos Z- Button").GetComponent<Button>();

		rotXplusButton = GameObject.Find("Main System/Button Canvas/rot X+ Button").GetComponent<Button>();
		rotXminusButton = GameObject.Find("Main System/Button Canvas/rot X- Button").GetComponent<Button>();
		rotYplusButton = GameObject.Find("Main System/Button Canvas/rot Y+ Button").GetComponent<Button>();
		rotYminusButton = GameObject.Find("Main System/Button Canvas/rot Y- Button").GetComponent<Button>();
		rotZplusButton = GameObject.Find("Main System/Button Canvas/rot Z+ Button").GetComponent<Button>();
		rotZminusButton = GameObject.Find("Main System/Button Canvas/rot Z- Button").GetComponent<Button>();

		posXplusButton.onClick.AddListener(onPosXplusClick);
		posXminusButton.onClick.AddListener(onPosXminusClick);
		posYplusButton.onClick.AddListener(onPosYplusClick);
		posYminusButton.onClick.AddListener(onPosYminusClick);
		posZplusButton.onClick.AddListener(onPosZplusClick);
		posZminusButton.onClick.AddListener(onPosZminusClick);

		rotXplusButton.onClick.AddListener(onRotXplusClick);
		rotXminusButton.onClick.AddListener(onRotXminusClick);
		rotYplusButton.onClick.AddListener(onRotYplusClick);
		rotYminusButton.onClick.AddListener(onRotYminusClick);
		rotZplusButton.onClick.AddListener(onRotZplusClick);
		rotZminusButton.onClick.AddListener(onRotZminusClick);

		cameraPositionText = GameObject.Find("Main System/Text Canvas/Camera Position Text").GetComponent<Text>();
		bsenPositionText = GameObject.Find("Main System/Text Canvas/B-sen Position Text").GetComponent<Text>();
		debugText = GameObject.Find("Main System/Text Canvas/Debug Text").GetComponent<Text>();
	}

	/*****************************************************************
	 * 自動キャリブレーション
	 *****************************************************************/
	void autoPositioning() {
		if (detected_marker) {
			if (marker_image.TrackingState == TrackingState.Tracking) {
				Quaternion new_rot = new Quaternion();
				new_rot = marker_image.CenterPose.rotation;
				new_rot *= Quaternion.Euler(0, 0, 90);
				new_rot *= Quaternion.Euler(90, 0, 0);

				Vector3 new_euler = new_rot.eulerAngles;
				new_euler.x = 0.0f;
				new_euler.z = 0.0f;

				//bsen_model.transform.eulerAngles = new_euler;
				bsen_model.transform.eulerAngles += new_euler;
				
				Vector3 image_position = marker_image.CenterPose.position;
				Vector3 real_position = irvs_marker.transform.position;
				Vector3 offset_vector = image_position - real_position;

				Vector3 temp_room_position = bsen_model.transform.position;
				temp_room_position += offset_vector;
				bsen_model.transform.position = temp_room_position;

				//debugText.text = "Auto Positioning DONE";
				debug("Auto Positioning DONE");
			}
		}
	}

	/*****************************************************************
	 * ボタン押したときの動作
	 *****************************************************************/
	void onAutoPositioningClick() {
		autoPositioning();
	}

	void onPosXplusClick() {
		Vector3 tmp = new Vector3(0.025f, 0.0f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	void onPosXminusClick() {
		Vector3 tmp = new Vector3(-0.025f, 0.0f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	void onPosYplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.025f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	void onPosYminusClick() {
		Vector3 tmp = new Vector3(0.0f, -0.025f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	void onPosZplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, 0.025f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}
	void onPosZminusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, -0.025f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}


	void onRotXplusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.x += 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	void onRotXminusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.x -= 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	void onRotYplusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.y += 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	void onRotYminusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.y -= 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	void onRotZplusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.z += 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	void onRotZminusClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.z -= 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}


	void debug(string message) {
		if(debugText != null) {
			debugText.text = message;
		}
	}
	
	/*****************************************************************
	 * ROSの座標系（右手系）からUnityの座標系（左手系）への変換
	 *****************************************************************/
	private Vector3 Ros2UnityPosition(Vector3 input) {
		return new Vector3(-input.y, input.z, input.x);// (-pos_y, pos_z, pos_x)
	}

	private Vector3 Ros2UnityRotation(Vector3 input) {
		return new Vector3(input.y, -input.z, -input.x);// (-pos_y, pos_z, pos_x)
	}

	private float Rad2Euler(float input) {
		return input * (180.0f / Mathf.PI);
	}
}

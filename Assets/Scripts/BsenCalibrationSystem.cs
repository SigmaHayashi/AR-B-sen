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

	//UI制御用
	private MainScript mainSystem;

	private Vector3 not_offset_pos, not_offset_rot;

	//AugmentedImageでつかうものたち
	private List<AugmentedImage> m_AugmentedImages = new List<AugmentedImage>();
	private bool detected_marker = false;
	private AugmentedImage marker_image;
	
	private bool finish_calibration = false;
	private int calibration_state = 0;

	public bool CheckFinishCalibration() {
		return finish_calibration;
	}
	

	private TMSDatabaseAdapter DBAdapter;

	private ShaderChange rostms_shader;

	// Start is called before the first frame update
	// 最初の1回呼び出されるよ～
	void Start() {
		mainSystem = GameObject.Find("Main System").GetComponent<MainScript>();

		bsen_model = GameObject.Find("rostms");
		rostms_shader = bsen_model.GetComponent<ShaderChange>();

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");

		coordinates_adapter = Instantiate(prefab);
		coordinates_adapter.transform.parent = bsen_model.transform;

		//ButtonTextSetting();

		calibration_state = 1;

		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();
	}

	// Update is called once per frame
	//ずっと繰り返し呼び出されるよ～
	void Update() {
		//debug("state: " + calibration_state.ToString());
		//mainSystem.UpdateMainCanvasInfoText("state : " + calibration_state.ToString());
		switch (calibration_state) {
			case 0:
				mainSystem.UpdateMainCanvasInfoText("Fail to Start");
				break;
			case 1:
				mainSystem.UpdateMainCanvasInfoText("Start app");
				break;
			case 2:
				mainSystem.UpdateMainCanvasInfoText("Access to Database");
				break;
			case 3:
				mainSystem.UpdateMainCanvasInfoText("Please Look [IRVS Marker]");
				break;
			case 4:
				mainSystem.UpdateMainCanvasInfoText("Ready to AR B-sen");
				break;
			default:
				mainSystem.UpdateMainCanvasInfoText("Error : " + calibration_state.ToString());
				break;
		}

		//phase 0
		//毎回すること
		//AugmentedImageの更新
		if (!Application.isEditor) {
			Session.GetTrackables<AugmentedImage>(m_AugmentedImages, TrackableQueryFilter.Updated);
		}
		
		//CameraとB-senのポジション表示
		mainSystem.UpdateCalibrationInfoCamera(Camera.main.transform.position, Camera.main.transform.eulerAngles);
		mainSystem.UpdateCalibrationInfoBsen(bsen_model.transform.position, bsen_model.transform.eulerAngles);

		//どれだけ手動キャリブしてるか表示
		Vector3 offset_pos = bsen_model.transform.position - not_offset_pos;
		Vector3 offset_rot = bsen_model.transform.eulerAngles - not_offset_rot;
		mainSystem.UpdateCalibrationInfoOffset(offset_pos, offset_rot);

		//自動キャリブ終了前
		if (!CheckFinishCalibration()) {
			switch (calibration_state) {
				//DBにアクセス開始
				case 1:
					if (!DBAdapter.CheckWaitAnything()) {
						IEnumerator coroutine = DBAdapter.ReadMarkerPos();
						StartCoroutine(coroutine);
						calibration_state = 2;
					}
					break;

				//DBのデータをもとにモデルの位置＆回転を変更
				case 2:
					if (DBAdapter.CheckSuccess()) {
						ServiceResponseDB responce = DBAdapter.GetResponce();
						DBAdapter.FinishReadData();
					
						//位置を取得＆変換
						Vector3 marker_position = new Vector3((float)responce.values.tmsdb[0].x, (float)responce.values.tmsdb[0].y, (float)responce.values.tmsdb[0].z);
						marker_position = Ros2UnityPosition(marker_position);
						marker_position.z += 0.25f;
						Debug.Log("Marker Pos: " + marker_position);
						mainSystem.MyConsole_Add("Marker Pos: " + marker_position);

						//回転を取得＆変換
						Vector3 marker_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
						marker_euler = Ros2UnityRotation(marker_euler);

						marker_euler *= -1.0f;
						marker_euler.x = 0.0f;
						marker_euler.y -= 1.5f;
						marker_euler.z = 0.0f;
						Debug.Log("Marker Rot: " + marker_euler);
						mainSystem.MyConsole_Add("Marker Rot: " + marker_euler);

						mainSystem.UpdateDatabaseInfoViconIRVSMarker(marker_position, marker_euler);

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

						calibration_state = 3;
					}
					break;

				//画像認識したらキャリブレーションしてモデルを表示
				//UnityEditor上ではここはスキップ
				case 3:
					if (!detected_marker) {
						foreach (var image in m_AugmentedImages) {
							if (image.TrackingState == TrackingState.Tracking) {
								detected_marker = true;
								marker_image = image;

								autoPositioning();

								rostms_shader.alpha = 0.6f;
								rostms_shader.ChangeColors();

								calibration_state = 4;
								finish_calibration = true;
							}
						}
					}
					if (Application.isEditor) {
						rostms_shader.alpha = 0.6f;
						rostms_shader.ChangeColors();

						calibration_state = 4;
						finish_calibration = true;
					}

					//自動キャリブ終了時の位置と回転を保存
					not_offset_pos = bsen_model.transform.position;
					not_offset_rot = bsen_model.transform.eulerAngles;
					break;
			}
		}
		else { //手動キャリブ
			manualCalibration();
		}
	}

	/*****************************************************************
	 * 自動キャリブレーション
	 *****************************************************************/
	void autoPositioning() {
		//画像認識ができたら
		if (detected_marker) {
			if (marker_image.TrackingState == TrackingState.Tracking) {
				//画像の回転を取得し，手前をX軸，鉛直方向をY軸にするように回転
				Quaternion new_rot = new Quaternion();
				new_rot = marker_image.CenterPose.rotation;
				new_rot *= Quaternion.Euler(0, 0, 90);
				new_rot *= Quaternion.Euler(90, 0, 0);

				//傾きはないものとする
				Vector3 new_euler = new_rot.eulerAngles;
				new_euler.x = 0.0f;
				new_euler.z = 0.0f;
				
				//モデルを画像の向きをもとに回転
				bsen_model.transform.eulerAngles += new_euler;
				
				//Unity空間における画像の位置，VICONから得たマーカーの座標からどれだけずれてるか計算
				Vector3 image_position = marker_image.CenterPose.position;
				Vector3 real_position = irvs_marker.transform.position;
				Vector3 offset_vector = image_position - real_position;

				//どれだけずれてるかの値からモデルを移動
				Vector3 temp_room_position = bsen_model.transform.position;
				temp_room_position += offset_vector;
				bsen_model.transform.position = temp_room_position;

				//debug("Auto Positioning DONE");
				mainSystem.UpdateMainCanvasInfoText("Auto Positioning DONE");
			}
		}
	}

	/*****************************************************************
	 * ボタン押したときの動作
	 *****************************************************************/
	public void onPosXplusClick() {
		Vector3 tmp = new Vector3(0.025f, 0.0f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onPosXminusClick() {
		Vector3 tmp = new Vector3(-0.025f, 0.0f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onPosYplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.025f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onPosYminusClick() {
		Vector3 tmp = new Vector3(0.0f, -0.025f, 0.0f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onPosZplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, 0.025f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onPosZminusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, -0.025f);
		coordinates_adapter.transform.localPosition = tmp;

		tmp = coordinates_adapter.transform.position;
		bsen_model.transform.position = tmp;
	}

	public void onRotRightClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.y += 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	public void onRotLeftClick() {
		Vector3 tmp = bsen_model.transform.eulerAngles;
		tmp.y -= 0.25f;
		bsen_model.transform.eulerAngles = tmp;
	}

	private void manualCalibration() {
		foreach (string button_name in mainSystem.checkCalibrationCanvasButton()) {
			Vector3 tmp = new Vector3();
			switch (button_name) {
				case "pos X+ Button":
					tmp = new Vector3(0.1f * Time.deltaTime, 0, 0);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "pos X- Button":
					tmp = new Vector3(-0.1f * Time.deltaTime, 0, 0);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "pos Y+ Button":
					tmp = new Vector3(0, 0.1f * Time.deltaTime, 0);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "pos Y- Button":
					tmp = new Vector3(0, -0.1f * Time.deltaTime, 0);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "pos Z+ Button":
					tmp = new Vector3(0, 0, 0.1f * Time.deltaTime);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "pos Z- Button":
					tmp = new Vector3(0, 0, -0.1f * Time.deltaTime);
					coordinates_adapter.transform.localPosition = tmp;

					tmp = coordinates_adapter.transform.position;
					bsen_model.transform.position = tmp;
					break;
				case "rot Right Button":
					tmp = bsen_model.transform.eulerAngles;
					tmp.y += 0.5f * Time.deltaTime;
					bsen_model.transform.eulerAngles = tmp;
					break;
				case "rot Left Button":
					tmp = bsen_model.transform.eulerAngles;
					tmp.y -= 0.5f * Time.deltaTime;
					bsen_model.transform.eulerAngles = tmp;
					break;
			}
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

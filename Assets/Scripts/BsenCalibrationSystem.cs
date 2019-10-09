﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class BsenCalibrationSystem : MonoBehaviour {

	//GameObjectたち
	private GameObject arcore_device;
	private GameObject bsen_model;
	private GameObject coordinates_adapter;
	private GameObject irvs_marker;

	//UI制御用
	private MainScript mainSystem;

	private Vector3 not_offset_pos, not_offset_rot;
	
	public Vector3 offset_vicon_pos;

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

		arcore_device = GameObject.Find("ARCore Device");

		bsen_model = GameObject.Find("rostms");
		rostms_shader = bsen_model.GetComponent<ShaderChange>();

		//GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");

		//coordinates_adapter = Instantiate(prefab);
		coordinates_adapter = Instantiate(new GameObject());
		coordinates_adapter.name = "Coordinates Adapter";
		//coordinates_adapter.transform.parent = bsen_model.transform;
		coordinates_adapter.transform.SetParent(bsen_model.transform, false);

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
						//marker_position.z += 0.25f;
						marker_position += offset_vicon_pos;
						Debug.Log("Marker Pos: " + marker_position);
						mainSystem.MyConsole_Add("Marker Pos: " + marker_position);

						//回転を取得＆変換
						Vector3 marker_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
						marker_euler = Ros2UnityRotation(marker_euler);

						//marker_euler *= -1.0f;
						marker_euler.x = 0.0f;
						marker_euler.z = 0.0f;
						marker_euler.y += 0.0f; //回転微調整用
						Debug.Log("Marker Rot: " + marker_euler);
						mainSystem.MyConsole_Add("Marker Rot: " + marker_euler);

						mainSystem.UpdateDatabaseInfoViconIRVSMarker(marker_position, marker_euler);

						//回転をモデルに適用
						//bsen_model.transform.eulerAngles = marker_euler;

						//位置と回転をモデル上のマーカーに適用
						//GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
						//irvs_marker = Instantiate(prefab);
						irvs_marker = Instantiate(new GameObject());
						irvs_marker.name = "IRVS Marker";
						//irvs_marker.transform.parent = GameObject.Find("rostms/world_link").transform;
						irvs_marker.transform.SetParent(GameObject.Find("rostms/world_link").transform, false);
						irvs_marker.transform.localPosition = marker_position;
						irvs_marker.transform.localEulerAngles = marker_euler;
						
						//回転軸をマーカーの位置に合わせる
						//GameObject world_link = GameObject.Find("rostms/world_link");
						//world_link.transform.localPosition = marker_position * -1;

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
				/*
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
				*/

				/*
				//画像の回転を取得し，手前をX軸，鉛直上方向をY軸にするように回転
				Quaternion image_rot_new = marker_image.CenterPose.rotation;
				image_rot_new *= Quaternion.Euler(0, 0, 90);
				image_rot_new *= Quaternion.Euler(90, 0, 0);

				//傾きをなくす
				Vector3 image_euler_new = image_rot_new.eulerAngles;
				image_euler_new.x = 0.0f;
				image_euler_new.z = 0.0f;

				//画像の位置と回転，カメラの位置と回転を取得
				Vector3 image_pos = marker_image.CenterPose.position;
				Vector3 image_euler = image_euler_new;
				Vector3 cam_pos = Camera.main.transform.position;
				Vector3 cam_euler = Camera.main.transform.eulerAngles;

				mainSystem.MyConsole_Add("Image Pos: " + image_pos.ToString());
				mainSystem.MyConsole_Add("Image Eul: " + image_euler.ToString());
				mainSystem.MyConsole_Add("Camera Pos: " + cam_pos.ToString());
				mainSystem.MyConsole_Add("Camera Eul: " + cam_euler.ToString());
				
				//そのときのカメラの位置・画像の位置に空オブジェクトを配置し，親子関係にする
				GameObject image_object = Instantiate(new GameObject());
				image_object.transform.position = image_pos;
				image_object.transform.eulerAngles = image_euler;

				GameObject camera_object = Instantiate(new GameObject());
				camera_object.transform.position = cam_pos;
				camera_object.transform.eulerAngles = cam_euler;

				camera_object.transform.SetParent(image_object.transform, true);

				//あるべき回転とのずれを計算し，回転する
				//image_object.transform.eulerAngles = irvs_marker.transform.eulerAngles;

				//あるべき位置とのずれを計算し，移動する
				//image_object.transform.position = irvs_marker.transform.position;

				//仮の画像オブジェクトをあるべき回転・位置に変更
				image_object.transform.eulerAngles = irvs_marker.transform.eulerAngles;
				image_object.transform.position = irvs_marker.transform.position;

				mainSystem.MyConsole_Add("New Image Pos: " + image_object.transform.position.ToString());
				mainSystem.MyConsole_Add("New Image Eul: " + image_object.transform.eulerAngles.ToString());

				//仮のカメラオブジェクトの位置からローカル位置を引いた場所にデバイスを移動
				//Vector3 arcore_device_euler = arcore_device_object.transform.eulerAngles;
				//arcore_device_euler.x = 0.0f;
				//arcore_device_euler.z = 0.0f;
				arcore_device.transform.position = camera_object.transform.position - Camera.main.transform.localPosition;
				//arcore_device.transform.eulerAngles = arcore_device_euler;

				//仮のカメラオブジェクトとデバイスオブジェクトを作成して逆親子関係にする
				GameObject arcore_device_object = Instantiate(new GameObject());
				arcore_device_object.transform.position = arcore_device.transform.position;
				arcore_device_object.transform.eulerAngles = arcore_device.transform.eulerAngles;

				camera_object.transform.position = Camera.main.transform.position;
				camera_object.transform.eulerAngles = Camera.main.transform.eulerAngles;

				arcore_device_object.transform.SetParent(camera_object.transform, true);

				//camera_object.transform.eulerAngles = image_euler - image_euler_new;
				//camera_object.transform.eulerAngles = image_euler_new - image_euler;
				camera_object.transform.eulerAngles = irvs_marker.transform.eulerAngles - image_euler;

				arcore_device.transform.position = arcore_device_object.transform.position;
				//arcore_device.transform.eulerAngles = arcore_device_object.transform.eulerAngles;
				Vector3 arcore_device_euler = arcore_device_object.transform.eulerAngles;
				arcore_device_euler.x = 0.0f;
				arcore_device_euler.z = 0.0f;
				arcore_device.transform.eulerAngles = arcore_device_euler;
				*/

				//画像の位置・回転を取得
				Vector3 image_pos = marker_image.CenterPose.position;
				Quaternion image_rot = marker_image.CenterPose.rotation;

				//画像を回転して，手前をX軸，鉛直上向きをY軸にする
				image_rot *= Quaternion.Euler(0, 0, 90);
				image_rot *= Quaternion.Euler(90, 0, 0);
				
				//画像傾きをなくす（水平に配置されてると仮定）
				Vector3 image_eul = image_rot.eulerAngles;
				image_eul.x = 0.0f;
				image_eul.z = 0.0f;

				//デバイスの位置・回転を取得
				Vector3 device_pos = arcore_device.transform.position;
				Vector3 device_eul = arcore_device.transform.eulerAngles;

				//カメラの位置・回転を取得
				Vector3 camera_pos = Camera.main.transform.position;
				Vector3 camera_eul = Camera.main.transform.eulerAngles;

				//座標計算用の仮のオブジェクトをそれぞれ作成
				//GameObject image_object = Instantiate(new GameObject());
				GameObject image_object = new GameObject();
				image_object.transform.position = image_pos;
				image_object.transform.eulerAngles = image_eul;

				//GameObject device_object = Instantiate(new GameObject());
				GameObject device_object = new GameObject();
				device_object.transform.position = device_pos;
				device_object.transform.eulerAngles = device_eul;

				//GameObject camera_object = Instantiate(new GameObject());
				GameObject camera_object = new GameObject();
				camera_object.transform.position = camera_pos;
				camera_object.transform.eulerAngles = camera_eul;

				//親子関係を，画像＞デバイス＞カメラにする
				camera_object.transform.SetParent(device_object.transform, true);
				device_object.transform.SetParent(image_object.transform, true);

				//仮の画像オブジェクトをあるべき位置・回転に変更
				image_object.transform.position = irvs_marker.transform.position;
				image_object.transform.eulerAngles = irvs_marker.transform.eulerAngles;

				//デバイスを仮のデバイスの位置・回転に変更
				arcore_device.transform.position = device_object.transform.position;
				arcore_device.transform.eulerAngles = device_object.transform.eulerAngles;
			}
		}
	}

	/*****************************************************************
	 * ボタン押したときの動作
	 *****************************************************************/
	/*
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
	*/

	private void manualCalibration() {
		foreach (string button_name in mainSystem.checkCalibrationCanvasButton()) {
			Vector3 tmp = new Vector3();
			switch (button_name) {
				/*
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
				*/
				case "pos X+ Button":
				tmp = new Vector3(0.1f * Time.deltaTime, 0, 0);
				//tmp += arcore_device.transform.position;
				arcore_device.transform.position += tmp;
				break;
				case "pos X- Button":
				tmp = new Vector3(-0.1f * Time.deltaTime, 0, 0);
				arcore_device.transform.position += tmp;
				break;
				case "pos Y+ Button":
				tmp = new Vector3(0, 0.1f * Time.deltaTime, 0);
				arcore_device.transform.position += tmp;
				break;
				case "pos Y- Button":
				tmp = new Vector3(0, -0.1f * Time.deltaTime, 0);
				arcore_device.transform.position += tmp;
				break;
				case "pos Z+ Button":
				tmp = new Vector3(0, 0, 0.1f * Time.deltaTime);
				arcore_device.transform.position += tmp;
				break;
				case "pos Z- Button":
				tmp = new Vector3(0, 0, -0.1f * Time.deltaTime);
				arcore_device.transform.position += tmp;
				break;
				/*
				case "rot Right Button":
				//arcore_device.transform.position += Camera.main.transform.position;
				tmp = new Vector3(0, 5.0f * Time.deltaTime, 0);
				arcore_device.transform.eulerAngles += tmp;
				break;
				case "rot Left Button":
				//arcore_device.transform.position += Camera.main.transform.position;
				tmp = new Vector3(0, -5.0f * Time.deltaTime, 0);
				arcore_device.transform.eulerAngles += tmp;
				break;
				*/
				case "rot Right Button": {
					GameObject camera_object = new GameObject();
					camera_object.transform.position = Camera.main.transform.position;
					camera_object.transform.eulerAngles = Camera.main.transform.eulerAngles;

					GameObject device_object = new GameObject();
					device_object.transform.position = arcore_device.transform.position;
					device_object.transform.eulerAngles = arcore_device.transform.eulerAngles;

					device_object.transform.SetParent(camera_object.transform, true);

					camera_object.transform.eulerAngles += new Vector3(0, 2.0f * Time.deltaTime, 0);

					arcore_device.transform.position = device_object.transform.position;
					arcore_device.transform.eulerAngles = device_object.transform.eulerAngles;
					break;
				}
				case "rot Left Button": {
					GameObject camera_object = new GameObject();
					camera_object.transform.position = Camera.main.transform.position;
					camera_object.transform.eulerAngles = Camera.main.transform.eulerAngles;

					GameObject device_object = new GameObject();
					device_object.transform.position = arcore_device.transform.position;
					device_object.transform.eulerAngles = arcore_device.transform.eulerAngles;

					device_object.transform.SetParent(camera_object.transform, true);

					camera_object.transform.eulerAngles += new Vector3(0, -2.0f * Time.deltaTime, 0);

					arcore_device.transform.position = device_object.transform.position;
					arcore_device.transform.eulerAngles = device_object.transform.eulerAngles;
					break;
				}
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

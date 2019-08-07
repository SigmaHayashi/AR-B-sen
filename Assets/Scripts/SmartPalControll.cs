using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryData {
	public float battery;
}

public class SmartPalControll : MonoBehaviour {

	private TMSDatabaseAdapter DBAdapter;
	private float time_pos = 0.0f;
	private float time_bat = 0.0f;
	bool finish_battery_text = false;
	GameObject Battery_3DText;
	
	private BsenCalibrationSystem calib_system;

	// Start is called before the first frame update
	void Start() {
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();

		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
	}

	// Update is called once per frame
	void Update() {
		if (calib_system.CheckFinishCalibration()) {
			/*
			time += Time.deltaTime;
			if (!DBAdapter.CheckWaitAnything() && time > 1.0f) {
				time = 0.0f;
				IEnumerator coroutine = DBAdapter.ReadSmartPalPos();
				StartCoroutine(coroutine);
			}
			
			if (DBAdapter.CheckReadSmartPalPos()) {
				if (DBAdapter.CheckAbort()) {
					DBAdapter.ConfirmAbort();
				}
				
				if (DBAdapter.CheckSuccess()) {
					ServiceResponseDB responce = DBAdapter.GetResponce();
					DBAdapter.FinishReadData();

					Vector3 sp5_pos = new Vector3((float)responce.values.tmsdb[0].x, (float)responce.values.tmsdb[0].y, (float)responce.values.tmsdb[0].z);
					sp5_pos = Ros2UnityPosition(sp5_pos);
					sp5_pos.y = 0.0f;
					sp5_pos.z += 0.25f;

					Vector3 sp5_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
					sp5_euler = Ros2UnityRotation(sp5_euler);
					sp5_euler.x = 0.0f;
					sp5_euler.z = 0.0f;

					transform.localPosition = sp5_pos;
					transform.localEulerAngles = sp5_euler;
					Debug.Log(responce.values.tmsdb[0].name + " pos: " + sp5_pos);
					Debug.Log(responce.values.tmsdb[0].name + " eul: " + sp5_euler);
				}
			}
			*/
			PositionTracking();
			UpdateBatteryInformation();
		}
	}

	/*****************************************************************
	 * DBからVICONのデータを取得してポジショントラッキング
	 *****************************************************************/
	private void PositionTracking() {
		time_pos += Time.deltaTime;
		if (!DBAdapter.CheckWaitAnything() && time_pos > 1.0f) {
			time_pos = 0.0f;
			IEnumerator coroutine = DBAdapter.ReadSmartPalPos();
			StartCoroutine(coroutine);
			//StartCoroutine(coroutine);
		}

		if (DBAdapter.CheckReadSmartPalPos()) {
			if (DBAdapter.CheckAbort()) {
				DBAdapter.ConfirmAbort();
			}

			if (DBAdapter.CheckSuccess()) {
				ServiceResponseDB responce = DBAdapter.GetResponce();
				DBAdapter.FinishReadData();

				Vector3 sp5_pos = new Vector3((float)responce.values.tmsdb[0].x, (float)responce.values.tmsdb[0].y, (float)responce.values.tmsdb[0].z);
				sp5_pos = Ros2UnityPosition(sp5_pos);
				sp5_pos.y = 0.0f;
				sp5_pos.z += 0.25f;

				Vector3 sp5_euler = new Vector3(Rad2Euler((float)responce.values.tmsdb[0].rr), Rad2Euler((float)responce.values.tmsdb[0].rp), Rad2Euler((float)responce.values.tmsdb[0].ry));
				sp5_euler = Ros2UnityRotation(sp5_euler);
				sp5_euler.x = 0.0f;
				sp5_euler.z = 0.0f;

				transform.localPosition = sp5_pos;
				transform.localEulerAngles = sp5_euler;
				Debug.Log(responce.values.tmsdb[0].name + " pos: " + sp5_pos);
				Debug.Log(responce.values.tmsdb[0].name + " eul: " + sp5_euler);
			}
		}
	}

	/*****************************************************************
	 * DBからバッテリー情報を取得して表示
	 *****************************************************************/
	private void UpdateBatteryInformation() {
		time_bat += Time.deltaTime;
		if(!DBAdapter.CheckWaitAnything() && time_bat > 1.0f) {
			time_bat = 0.0f;
			IEnumerator coroutine = DBAdapter.ReadBattery();
			StartCoroutine(coroutine);
		}

		if (DBAdapter.CheckReadBattery()) {
			if (DBAdapter.CheckAbort()) {
				DBAdapter.ConfirmAbort();
			}

			if (DBAdapter.CheckSuccess()) {
				ServiceResponseDB responce = DBAdapter.GetResponce();
				DBAdapter.FinishReadData();

				BatteryData battery_data = JsonUtility.FromJson<BatteryData>(responce.values.tmsdb[0].etcdata);
				float battery_per = battery_data.battery * 100;
				Debug.Log("SmartPal Battery: " + battery_per + "[%]");

				if (!finish_battery_text) {
					Battery_3DText = (GameObject)Instantiate(Resources.Load("3D Text"));
					Battery_3DText.transform.parent = transform;
					Battery_3DText.transform.localPosition = new Vector3(0.0f, 1.4f, 0.0f);
					TextMesh textmesh = Battery_3DText.GetComponent<TextMesh>();
					textmesh.fontSize = 60;
					textmesh.color = new Color(0, 0, 0);
					textmesh.text = "Battery: " + battery_per.ToString() + "[%]";

					finish_battery_text = true;
				}
				else {
					Battery_3DText.GetComponent<TextMesh>().text = "Battery: " + battery_per.ToString() + "[%]";
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

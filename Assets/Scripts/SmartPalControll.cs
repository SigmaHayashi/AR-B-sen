using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartPalControll : MonoBehaviour {

	private float time = 0.0f;
	private TMSDatabaseAdapter DBAdapter;
	//private bool searching = false;

	private BsenCalibrationSystem calib_system;

	// Start is called before the first frame update
	void Start() {
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();

		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
	}

	// Update is called once per frame
	void Update() {
		if (calib_system.CheckFinishCalibration()) {
			time += Time.deltaTime;
			//if (!DBAdapter.access_db && time > 1.0f) {
			//if (!DBAdapter.wait_anything && time > 1.0f) {
			//if (!DBAdapter.CheckReadSmartPalPos() && time > 1.0f) {
			if (!DBAdapter.CheckWaitAnything() && time > 1.0f) {
				time = 0.0f;
				IEnumerator coroutine = DBAdapter.ReadSmartPalPos();
				StartCoroutine(coroutine);
				//Debug.Log("SmartPal Pos Database Access");
				//searching = true;
			}

			//if (searching) {
			//if (DBAdapter.wait_anything) {
			if (DBAdapter.CheckReadSmartPalPos()) {
				//if (DBAdapter.abort_access) {
				if (DBAdapter.CheckAbort()) {
					DBAdapter.ConfirmAbort();
					//Debug.Log("SmartPal Aborted");
					//searching = false;
				}

				//if (DBAdapter.success_access) {
				if (DBAdapter.CheckSuccess()) {
					//Debug.Log("SP5 data get!!");
					//ServiceResponseDB responce = DBAdapter.responce;
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
					//Debug.Log("SmartPal OK");
					//searching = false;
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

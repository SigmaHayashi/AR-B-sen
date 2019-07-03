using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VICONCoordinatesTest : MonoBehaviour {

	public Text VICONDataText;
	public Text ModelDataText;

	//Android Ros Socket Client関連
	private AndroidRosSocketClient wsc;
	private string srvName = "tms_db_reader";
	private TmsDBReq srvReq = new TmsDBReq();
	private string srvRes;

	float time;

	// Start is called before the first frame update
	void Start() {
		//ROSTMSに接続
		wsc = GameObject.Find("Android Ros Socket Client").GetComponent<AndroidRosSocketClient>();
		srvReq.tmsdb = new tmsdb("ID_SENSOR", 7030, 3001);
		//wsc.ServiceCallerDB(srvName, srvReq);
		time = 0.0f;
	}

	// Update is called once per frame
	void Update() {
		time += Time.deltaTime;

		if (time > 0.2f) {
			time = 0.0f;

			wsc.Connect();

			wsc.ServiceCallerDB(srvName, srvReq);
		}
		if (wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
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
			VICONDataText.text = "Database: " + marker_euler.ToString();

			marker_euler = Ros2UnityRotation(marker_euler);

			//Debug.Log("Marker rot raw: " + marker_euler);

			/*
			Quaternion marker_rot = Quaternion.Euler(marker_euler);
			marker_rot *= Quaternion.Euler(0, 0, 180);
			marker_euler = marker_rot.eulerAngles;
			marker_euler.x = 0.0f;
			marker_euler.z = 0.0f;
			Debug.Log("Marker rot: " + marker_euler);
			*/

			//回転をモデルに適用
			transform.eulerAngles = marker_euler;

			Debug.Log("Model Euler: " + marker_euler);
			ModelDataText.text = "Model: " + marker_euler.ToString();

			//位置をモデル上のマーカーに適用
			/*
			GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
			irvs_marker = Instantiate(prefab);
			irvs_marker.transform.parent = GameObject.Find("rostms/world_link").transform;
			irvs_marker.transform.localPosition = marker_position;
			*/

		}
	}

	private void OnApplicationQuit() {
		wsc.Close();
	}

	/*****************************************************************
	 * ROSの座標系（右手系）からUnityの座標系（左手系）への変換
	 *****************************************************************/
	private Vector3 Ros2UnityPosition(Vector3 input) {
		return new Vector3(-input.y, input.z, input.x);// (-pos_y, pos_z, pos_x)
	}

	private Vector3 Ros2UnityRotation(Vector3 input) {
		//return new Vector3(input.y, -input.z, -input.x);// (-pos_y, pos_z, pos_x)
		return new Vector3(input.y, -input.z, -input.x);// (-pos_y, pos_z, pos_x)
	}

	private float Rad2Euler(float input) {
		return input * (180.0f / Mathf.PI);
	}
}

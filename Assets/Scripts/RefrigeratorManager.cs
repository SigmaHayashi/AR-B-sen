using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefrigeratorManager : MonoBehaviour {

	//GameObjectたち
	private GameObject refrigerator;
	private GameObject ar_camera;
	
	private GameObject coordinates_adapter;

	private GameObject greentea;
	private GameObject cancoffee;
	private GameObject soysauce;
	private List<GameObject> goods_list = new List<GameObject>();

	//Android Ros Socket Client関連
	private AndroidRosSocketClient wsc;
	private string srvName = "tms_db_reader";
	private TmsDBReq srvReq = new TmsDBReq();
	private string srvRes;

	float time;
	private BsenCalibrationSystem calib_system;

	// 距離の制御
	private float distance;

	//public Text debug_text;

	// Start is called before the first frame update
	void Start() {
		refrigerator = GameObject.Find("refrigerator_link");
		ar_camera = GameObject.Find("First Person Camera");

		greentea = GameObject.Find("greentea_bottle_x_link");
		cancoffee = GameObject.Find("cancoffee_x_link");
		soysauce = GameObject.Find("soysauce_bottle_black_x_link");

		goods_list.Add(greentea);
		goods_list.Add(cancoffee);
		goods_list.Add(soysauce);

		greentea.SetActive(false);
		cancoffee.SetActive(false);
		soysauce.SetActive(false);

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
		coordinates_adapter = (GameObject)Instantiate(prefab, this.transform);
		coordinates_adapter.transform.parent = refrigerator.transform;
		
		//ROSTMSに接続
		wsc = GameObject.Find("Android Ros Socket Client").GetComponent<AndroidRosSocketClient>();
		srvReq.tmsdb = new tmsdb("PLACE", 2009);
		//wsc.ServiceCallerDB(srvName, srvReq);

		time = 0.0f;
		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
	}

	// Update is called once per frame
	void Update() {
		coordinates_adapter.transform.localPosition = new Vector3(-0.23f, 0.0f, -0.3f);
		distance = CalcDistance(coordinates_adapter, ar_camera);
		//debug_text.text = distance.ToString("f2");
		//Debug.Log(distance.ToString("f2"));

		if(distance < 1.5f) {
			greentea.SetActive(true);
			cancoffee.SetActive(true);
			soysauce.SetActive(true);
		}
		else {
			greentea.SetActive(false);
			cancoffee.SetActive(false);
			soysauce.SetActive(false);
		}
		
		if(calib_system.calibration_state > 1) {
			time += Time.deltaTime;
			if(time > 1.0f) {
				time = 0.0f;
				Debug.Log("Retry...");

				wsc.Connect();

				wsc.ServiceCallerDB(srvName, srvReq);
			}
			if(wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
				srvRes = wsc.GetSrvResMsg();
				Debug.Log("ROS: " + srvRes);

				ServiceResponseDB responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);
			
				foreach(tmsdb data in responce.values.tmsdb) {
					if(data.x != -1 && data.y != -1 && data.z != -1) {
						//Debug.Log(data.name);
						//Debug.Log(data.x + ", " + data.y + ", " + data.z);

						foreach(GameObject goods in goods_list) {
							if(goods.name.IndexOf(data.name) != -1) {
								Vector3 place = new Vector3((float)data.x, (float)data.y, (float)data.z);
								place = Ros2UnityPosition(place);
								Debug.Log(data.name + " pos: " + place.ToString("f2"));

								goods.transform.localPosition = place;
							}
						}
					}
				}
			}
		}
	}

	float CalcDistance(GameObject obj_a, GameObject obj_b) {
		Vector3 obj_a_pos = obj_a.transform.position;
		Vector3 obj_b_pos = obj_b.transform.position;
		return Mathf.Sqrt(Mathf.Pow((obj_a_pos.x - obj_b_pos.x), 2) + Mathf.Pow((obj_a_pos.z - obj_b_pos.z), 2));
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

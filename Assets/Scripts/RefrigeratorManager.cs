using System;
using System.Linq;
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
	private bool[] goods_state = new bool[3];
	
	//Android Ros Socket Client関連
	private AndroidRosSocketClient wsc;
	private string srvName = "tms_db_reader";
	private TmsDBReq srvReq = new TmsDBReq();
	private string srvRes;

	float time;
	private BsenCalibrationSystem calib_system;

	// 距離の制御
	private float distance;
	private float distance_old;

	[SerializeField]
	private float distance_to_display = 1.5f;

	//ShaderChange
	private ShaderChange refrigerator_shaderchange;
	private ShaderChange rostms_shaderchange;
	private bool change_goods_shader = false;
	private bool finish_coroutine = true;

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
		goods_state[0] = false;
		goods_state[1] = false;
		goods_state[2] = false;

		foreach(GameObject goods in goods_list) {
			goods.AddComponent<ShaderChange>();
		}
		
		/*
		greentea.SetActive(false);
		cancoffee.SetActive(false);
		soysauce.SetActive(false);
		*/

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
		coordinates_adapter = (GameObject)Instantiate(prefab, this.transform);
		coordinates_adapter.transform.parent = refrigerator.transform;
		
		//ROSTMSに接続
		wsc = GameObject.Find("Android Ros Socket Client").GetComponent<AndroidRosSocketClient>();
		srvReq.tmsdb = new tmsdb("PLACE", 2009);
		//wsc.ServiceCallerDB(srvName, srvReq);

		time = 0.0f;

		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();

		refrigerator.AddComponent<ShaderChange>();
		refrigerator_shaderchange = refrigerator.GetComponent<ShaderChange>();

		rostms_shaderchange = GameObject.Find("rostms").GetComponent<ShaderChange>();
	}


	// Update is called once per frame
	void Update() {
		//CoordinatesAdapterの位置を調整してカメラとの距離を計算
		coordinates_adapter.transform.localPosition = new Vector3(-0.23f, 0.0f, -0.3f);
		distance_old = distance;
		distance = CalcDistance(coordinates_adapter, ar_camera);

		if (!change_goods_shader) {
			foreach(GameObject goods in goods_list) {
				ShaderChange shaderchange = goods.GetComponent<ShaderChange>();
				shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
			}
			change_goods_shader = true;
		}

		//距離が閾値以下でデータベースのstateが1だったら表示，違ったら非表示
		//foreach(GameObject goods in goods_list) {
			/*
			if (goods_state[goods_list.IndexOf(goods)] && distance < 1.5f) {
				goods.SetActive(true);
			}
			else {
				goods.SetActive(false);
			}
			*/
			/*
			if(calib_system.calibration_state > 2) {
				if (goods_state[goods_list.IndexOf(goods)] && distance < distance_to_display) {
					if(distance_old >= distance_to_display) {
						refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
						//StartCoroutine(refrigerator_shaderchange.ChangeShaderCoroutine(Shader.Find("Custom/Transparent")));
						refrigerator_shaderchange.alpha = 0.4f;
						refrigerator_shaderchange.ChangeColors();
					}

					//goods.SetActive(true);
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.4f;
					goods_shaderchange.ChangeColors();
				}
				else {
					//goods.SetActive(false);
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
				
					if (distance_old < distance_to_display && distance >= distance_to_display) {
						refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/ARTransparent"));
						refrigerator_shaderchange.alpha = rostms_shaderchange.alpha;
						refrigerator_shaderchange.ChangeColors();
					}
				}
			}
		}
		*/

		if(calib_system.calibration_state > 2 && finish_coroutine) {
			//近づいたとき
			if (distance < distance_to_display && distance_old >= distance_to_display) {
				refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
				refrigerator_shaderchange.alpha = 0.4f;
				refrigerator_shaderchange.ChangeColors();

				/*
				foreach(GameObject goods in goods_list) {
					if (goods_state[goods_list.IndexOf(goods)]) {
						ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
						goods_shaderchange.alpha = 0.4f;
						goods_shaderchange.ChangeColors();
					}
				}
				*/
				IEnumerator coroutine = AppearSlowly();
				StartCoroutine(coroutine);
			}
			//遠ざかったとき
			/*
			else if (distance >= distance_to_display && distance_old < distance_to_display) {
				foreach(GameObject goods in goods_list) {
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
				}

				refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/ARTransparent"));
				refrigerator_shaderchange.alpha = rostms_shaderchange.alpha;
				refrigerator_shaderchange.ChangeColors();
			}
			*/
			//遠くにいるとき
			else if(distance >= distance_to_display) {
				foreach (GameObject goods in goods_list) {
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
				}
				if(refrigerator_shaderchange.shader_now != Shader.Find("Custom/ARTransparent")){
					refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/ARTransparent"));
					refrigerator_shaderchange.alpha = rostms_shaderchange.alpha;
					refrigerator_shaderchange.ChangeColors();
				}
			}
			//ずっと近くにいるとき
			else if (distance < distance_to_display && distance_old < distance_to_display){
				foreach(GameObject goods in goods_list) {
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					if (goods_state[goods_list.IndexOf(goods)]) {
						goods_shaderchange.alpha = 0.4f;
					}
					else {
						goods_shaderchange.alpha = 0.0f;
					}
					goods_shaderchange.ChangeColors();
				}
			}
			//ずっと遠くにいるとき
			/*
			else {
				foreach (GameObject goods in goods_list) {
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
				}
			}
			*/
		}

			//データベースとの通信
		if (calib_system.calibration_state > 1) {
			time += Time.deltaTime;
			if(time > 1.0f) {
				time = 0.0f;
				//Debug.Log("Retry...");

				wsc.Connect();

				wsc.ServiceCallerDB(srvName, srvReq);
			}
			if(wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
				srvRes = wsc.GetSrvResMsg();
				Debug.Log("ROS: " + srvRes);

				ServiceResponseDB responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);
			
				foreach(tmsdb data in responce.values.tmsdb) {
					//Debug.Log(data.name);
					//Debug.Log(data.x + ", " + data.y + ", " + data.z);

					foreach(GameObject goods in goods_list) {
						if(goods.name.IndexOf(data.name) != -1) {
							//if(data.x != -1 && data.y != -1 && data.z != -1) {
							if(data.state == 1) {
								goods_state[goods_list.IndexOf(goods)] = true;
								Vector3 place = new Vector3((float)data.x, (float)data.y, (float)data.z);
								place = Ros2UnityPosition(place);
								Debug.Log(data.name + " pos: " + place.ToString("f2"));

								goods.transform.localPosition = place;
							}
							else {
								goods_state[goods_list.IndexOf(goods)] = false;
							}
						}
					}
				}
			}
		}
	}

	IEnumerator AppearSlowly() {
		finish_coroutine = false;
		for (int i = 0; i < 5; i++) {
			yield return null;
		}

		/*
		foreach (GameObject goods in goods_list) {
			if (goods_state[goods_list.IndexOf(goods)]) {
				ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
				goods_shaderchange.alpha = 0.4f;
				goods_shaderchange.ChangeColors();
				for (int i = 0; i < 5; i++) {
					yield return null;
				}
			}
		}
		*/
		Dictionary<float, int> object_dictionary = new Dictionary<float, int>();
		foreach (GameObject goods in goods_list) {
			object_dictionary.Add(CalcDistance(ar_camera, goods), goods_list.IndexOf(goods));
		}

		var sorted = object_dictionary.OrderBy((x) => x.Key);

		foreach(KeyValuePair<float, int> goods_dictionary in sorted) {
			int goods_num = goods_dictionary.Value;
			if (goods_state[goods_num]) {
				ShaderChange goods_shaderchange = goods_list[goods_num].GetComponent<ShaderChange>();
				goods_shaderchange.alpha = 0.4f;
				goods_shaderchange.ChangeColors();
				for (int i = 0; i < 5; i++) {
					yield return null;
				}
			}
		}

		finish_coroutine = true;
	}

	/*****************************************************************
	 * オブジェクトどうしの距離を計算
	 *****************************************************************/
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

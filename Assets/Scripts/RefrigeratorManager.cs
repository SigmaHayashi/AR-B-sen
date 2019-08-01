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
	//private List<GameObject> goods_list = new List<GameObject>();
	private Dictionary<int, GameObject> goods_object_dictionary = new Dictionary<int, GameObject>();
	//private bool[] goods_state = new bool[3];
	private Dictionary<int, bool> goods_state_dictionary = new Dictionary<int, bool>();
	
	//TMSDB関連
	private float time_1 = 0.0f;
	private float time_2 = 0.0f;
	private TMSDatabaseAdapter DBAdapter;
	private List<int> id_list = new List<int>();

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

		/*
		goods_list.Add(greentea);
		goods_list.Add(cancoffee);
		goods_list.Add(soysauce);
		goods_state[0] = false;
		goods_state[1] = false;
		goods_state[2] = false;
		*/
		goods_object_dictionary.Add(7004, greentea);
		goods_object_dictionary.Add(7006, cancoffee);
		goods_object_dictionary.Add(7009, soysauce);
		goods_state_dictionary.Add(7004, false);
		goods_state_dictionary.Add(7006, false);
		goods_state_dictionary.Add(7009, false);

		//foreach (GameObject goods in goods_list) {
		foreach (GameObject goods in goods_object_dictionary.Values) {
			goods.AddComponent<ShaderChange>();
		}

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
		coordinates_adapter = (GameObject)Instantiate(prefab, this.transform);
		coordinates_adapter.transform.parent = refrigerator.transform;
		
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();

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
			//foreach(GameObject goods in goods_list) {
			foreach (GameObject goods in goods_object_dictionary.Values) {
				ShaderChange shaderchange = goods.GetComponent<ShaderChange>();
				shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
			}
			change_goods_shader = true;
		}

		//距離が閾値以下でデータベースのstateが1だったら表示，違ったら非表示
		//if(calib_system.calibration_state > 2 && finish_coroutine) {
		//if (calib_system.finish_calibration && finish_coroutine) {
		if (calib_system.CheckFinishCalibration() && finish_coroutine) {
			//近づいたとき
			if (distance < distance_to_display && distance_old >= distance_to_display) {
				refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
				refrigerator_shaderchange.alpha = 0.4f;
				refrigerator_shaderchange.ChangeColors();
				
				IEnumerator coroutine = AppearSlowly();
				StartCoroutine(coroutine);
			}
			//遠くにいるとき
			else if(distance >= distance_to_display) {
				//foreach (GameObject goods in goods_list) {
				foreach (GameObject goods in goods_object_dictionary.Values) {
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
				/*
				foreach (GameObject goods in goods_list) {
					ShaderChange goods_shaderchange = goods.GetComponent<ShaderChange>();
					if (goods_state[goods_list.IndexOf(goods)]) {
						goods_shaderchange.alpha = 0.4f;
					}
					else {
						goods_shaderchange.alpha = 0.0f;
					}
					goods_shaderchange.ChangeColors();
				}
				*/
				foreach(KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
					ShaderChange goods_shaderchange = goods.Value.GetComponent<ShaderChange>();
					if (goods_state_dictionary[goods.Key]) {
						goods_shaderchange.alpha = 0.4f;
					}
					else {
						goods_shaderchange.alpha = 0.0f;
					}
					goods_shaderchange.ChangeColors();
				}
			}
		}

		if (calib_system.CheckFinishCalibration()) {
			time_1 += Time.deltaTime;
			if (!DBAdapter.CheckWaitAnything() && time_1 > 1.0f) {
				time_1 = 0.0f;
				IEnumerator coroutine = DBAdapter.GetRefrigeratorItem();
				StartCoroutine(coroutine);
			}
			
			if (DBAdapter.CheckGetRefrigeratorItem()) {
				if (DBAdapter.CheckAbort()) {
					DBAdapter.ConfirmAbort();
				}
				
				if (DBAdapter.CheckSuccess()) {
					id_list = new List<int>();

					ServiceResponseDB responce = DBAdapter.GetResponce();
					DBAdapter.FinishReadData();
					foreach (tmsdb data in responce.values.tmsdb) {
						//Debug.Log(data.name);
						//Debug.Log(data.x + ", " + data.y + ", " + data.z);
						if (data.sensor == 3018) {
							//foreach (GameObject goods in goods_list) {
							foreach (KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
								//if (goods.name.IndexOf(data.name) != -1) {
								if (goods.Value.name.IndexOf(data.name) != -1) {
									if (data.state == 1) {
										//goods_state[goods_list.IndexOf(goods)] = true;
										goods_state_dictionary[goods.Key] = true;
										Vector3 place = new Vector3((float)data.x, (float)data.y, (float)data.z);
										place = Ros2UnityPosition(place);
										Debug.Log(data.name + " pos: " + place.ToString("f2"));

										//goods.transform.localPosition = place;
										goods.Value.transform.localPosition = place;

										id_list.Add(data.id);
									}
									else {
										//goods_state[goods_list.IndexOf(goods)] = false;
										goods_state_dictionary[goods.Key] = false;
									}
								}
							}
						}
					}
				}
			}

			time_2 += Time.deltaTime;
			if(!DBAdapter.CheckWaitAnything() && time_2 > 1.0f) {
				time_2 = 0.0f;
				DBAdapter.GiveItemIDList(id_list);
				IEnumerator coroutine = DBAdapter.ReadExpiration();
				StartCoroutine(coroutine);
			}
			if (DBAdapter.CheckReadExpiration()) {
				if (DBAdapter.CheckAbort()) {
					DBAdapter.ConfirmAbort();
				}
				if (DBAdapter.CheckSuccess()) {
					Dictionary<int, string> expiration_dictionary = DBAdapter.ReadExpirationData();
					DBAdapter.FinishReadData();
					foreach(KeyValuePair<int, string> item in expiration_dictionary) {
						Debug.Log("id: " + item.Key + ", " + item.Value);
					}
				}
			}
		}
	}
	
	/*****************************************************************
	 * オブジェクトをカメラから近い順にゆっくり表示するコルーチン
	 *****************************************************************/
	IEnumerator AppearSlowly() {
		finish_coroutine = false;
		for (int i = 0; i < 5; i++) {
			yield return null;
		}

		//Dictionary<float, int> object_dictionary = new Dictionary<float, int>();
		Dictionary<int, float> goods_distance_dictionary = new Dictionary<int, float>();
		/*
		foreach (GameObject goods in goods_list) {
			object_dictionary.Add(CalcDistance(ar_camera, goods), goods_list.IndexOf(goods));
		}
		*/
		foreach (KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
			//object_dictionary.Add(CalcDistance(ar_camera, goods.Value), goods.Key);
			goods_distance_dictionary.Add(goods.Key, CalcDistance(ar_camera, goods.Value));
		}

		//var sorted = object_dictionary.OrderBy((x) => x.Key);
		var sorted = goods_distance_dictionary.OrderBy((x) => x.Value);

		//foreach (KeyValuePair<float, int> goods_dictionary in sorted) {
		foreach (KeyValuePair<int, float> goods in sorted) {
			//int goods_num = goods_dictionary.Value;
			int goods_num = goods.Key;
			//if (goods_state[goods_num]) {
			if (goods_state_dictionary[goods_num]) {
				//ShaderChange goods_shaderchange = goods_list[goods_num].GetComponent<ShaderChange>();
				ShaderChange goods_shaderchange = goods_object_dictionary[goods_num].GetComponent<ShaderChange>();
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

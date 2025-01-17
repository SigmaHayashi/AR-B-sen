﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//消費期限を取得する用のクラス
public class ExpirationData {
	public string expiration;
}

//物品情報をまとめるクラス
//Dictionary統合のため
class GoodsData {
	//public int id;
	public string name;
	public int state;
	public Vector3 pos;
	public string expiration;
	public GameObject obj;
	public GameObject text3d;
	public bool state_bool;
}

public class RefrigeratorManager : MonoBehaviour {

	//UI更新用
	private MainScript mainSystem;

	//GameObjectたち
	private GameObject refrigerator;
	private GameObject ar_camera;
	
	private GameObject coordinates_adapter;

	private GameObject greentea;
	private GameObject cancoffee;
	private GameObject soysauce;

	private Dictionary<int, GoodsData> goods_data_dictionary = new Dictionary<int, GoodsData>();
	
	//TMSDB関連
	private float time_1 = 0.0f;
	private float time_2 = 0.0f;
	private TMSDatabaseAdapter DBAdapter;
	private List<int> id_list = new List<int>();

	private BsenCalibrationSystem calib_system;

	// 距離の制御
	private float distance;
	private float distance_old;

	//ShaderChange
	private ShaderChange refrigerator_shaderchange;
	private ShaderChange rostms_shaderchange;
	private bool change_goods_shader = false;
	private bool finish_coroutine = true;


	// Start is called before the first frame update
	void Start() {
		//各オブジェクトの取得
		mainSystem = GameObject.Find("Main System").GetComponent<MainScript>();

		refrigerator = GameObject.Find("refrigerator_link");
		ar_camera = GameObject.Find("First Person Camera");

		greentea = GameObject.Find("greentea_bottle_x_link");
		cancoffee = GameObject.Find("cancoffee_x_link");
		soysauce = GameObject.Find("soysauce_bottle_black_x_link");

		//オブジェクトを辞書に登録
		goods_data_dictionary.Add(7004, new GoodsData());
		goods_data_dictionary.Add(7006, new GoodsData());
		goods_data_dictionary.Add(7009, new GoodsData());
		goods_data_dictionary[7004].obj = greentea;
		goods_data_dictionary[7006].obj = cancoffee;
		goods_data_dictionary[7009].obj = soysauce;

		//オブジェクトにShader変更スクリプトを追加
		//オブジェクトの状態をfalseに
		//オブジェクトに消費期限表示用の3Dテキスト追加
		foreach (GoodsData goods in goods_data_dictionary.Values) {
			goods.obj.AddComponent<ShaderChange>();

			goods.state_bool = false;

			goods.text3d = (GameObject)Instantiate(Resources.Load("TextMeshPro"));
			goods.text3d.transform.SetParent(goods.obj.transform, false);
			goods.text3d.transform.localPosition = new Vector3(0.0f, 0.15f, 0.0f);
			TextMeshPro TMP = goods.text3d.GetComponent<TextMeshPro>();
			TMP.fontSize = 0.6f;
			TMP.text = "";
			goods.text3d.SetActive(false);
		}

		//位置合わせするや～つを配置
		coordinates_adapter = Instantiate(new GameObject());
		coordinates_adapter.name = "Coordinates Adapter";
		coordinates_adapter.transform.SetParent(refrigerator.transform, false);

		//データベースと通信するやつ
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();

		//キャリブシステム
		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();

		//冷蔵庫にもShader変更スクリプトを追加
		refrigerator.AddComponent<ShaderChange>();
		refrigerator_shaderchange = refrigerator.GetComponent<ShaderChange>();

		//rostmsのShader変更スクリプト
		rostms_shaderchange = GameObject.Find("rostms").GetComponent<ShaderChange>();
	}


	// Update is called once per frame
	void Update() {
		if (!mainSystem.finish_read_config) {
			return;
		}

		//CoordinatesAdapterの位置を調整してカメラとの距離を計算
		coordinates_adapter.transform.localPosition = new Vector3(-0.23f, 0.0f, -0.3f);
		distance_old = distance;
		distance = CalcDistance(coordinates_adapter, ar_camera);

		//最初の1回Shaderを変更する
		if (!change_goods_shader) {
			foreach(GoodsData goods in goods_data_dictionary.Values) {
				ShaderChange shaderchange = goods.obj.GetComponent<ShaderChange>();
				shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
			}
			change_goods_shader = true;
		}

		//距離が閾値以下でデータベースのstateが1だったら表示，違ったら非表示
		if (calib_system.CheckFinishCalibration() && finish_coroutine) {
			//近づいたとき
			if (distance < mainSystem.GetConfig().refrigerator_distance && distance_old >= mainSystem.GetConfig().refrigerator_distance) {
				refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
				refrigerator_shaderchange.alpha = 0.4f;
				refrigerator_shaderchange.ChangeColors();
				
				IEnumerator coroutine = AppearSlowly();
				StartCoroutine(coroutine);
			}
			//遠くにいるとき
			else if (distance >= mainSystem.GetConfig().refrigerator_distance) {
				foreach (GoodsData goods in goods_data_dictionary.Values) {
					ShaderChange goods_shaderchange = goods.obj.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
					goods.text3d.SetActive(false);
				}
				if(refrigerator_shaderchange.shader_now != Shader.Find("Custom/ARTransparent")){
					refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/ARTransparent"));
					refrigerator_shaderchange.alpha = rostms_shaderchange.alpha;
					refrigerator_shaderchange.ChangeColors();
				}
			}
			//ずっと近くにいるとき
			else if (distance < mainSystem.GetConfig().refrigerator_distance && distance_old < mainSystem.GetConfig().refrigerator_distance) {
				foreach (GoodsData goods in goods_data_dictionary.Values) {
					ShaderChange goods_shaderchange = goods.obj.GetComponent<ShaderChange>();
					if (goods.state_bool) {
						goods_shaderchange.alpha = 0.4f;
						goods.text3d.SetActive(true);
					}
					else {
						goods_shaderchange.alpha = 0.0f;
						goods.text3d.SetActive(false);
					}
					goods_shaderchange.ChangeColors();
				}
			}
		}

		//画像認識による自動キャリブレーションが終わった後に実行
		if (calib_system.CheckFinishCalibration()) {
			//冷蔵庫に入っている物品のデータを取得
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
						if(data.sensor == 3018 && goods_data_dictionary.ContainsKey(data.id)) {
							GoodsData goods = goods_data_dictionary[data.id];
							Vector3 place = new Vector3((float)data.x, (float)data.y, (float)data.z);
							place = Ros2UnityPosition(place);
							if (data.state == 1) {
								goods.state_bool = true;

								Debug.Log(data.name + " pos: " + place.ToString("f2"));
								mainSystem.MyConsole_Add(data.name + " pos: " + place.ToString("f2"));
								goods.obj.transform.localPosition = place;

								id_list.Add(data.id);
							}
							else {
								goods.state_bool = false;
							}

							if(goods.name == null) {
								goods.name = data.name;
							}
							if(goods.expiration == null) {
								goods.expiration = "don't know";
							}
							goods.state = data.state;
							goods.pos = place;
						}
					}
				}
			}

			//冷蔵庫に入っている物品の消費期限を取得
			time_2 += Time.deltaTime;
			if(!DBAdapter.CheckWaitAnything() && time_2 > 5.0f) {
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
					foreach (KeyValuePair<int, string> goods in expiration_dictionary) {
						ExpirationData expiration_data = JsonUtility.FromJson<ExpirationData>(goods.Value);
						string expiration = expiration_data.expiration;
						Debug.Log("id: " + goods.Key + ", name: " + goods_data_dictionary[goods.Key].name + ", expiration: " + expiration);
						mainSystem.MyConsole_Add("id: " + goods.Key + ", name: " + goods_data_dictionary[goods.Key].name + ", expiration: " + expiration);
						
						goods_data_dictionary[goods.Key].text3d.GetComponent<TextMeshPro>().text = expiration;
						
						goods_data_dictionary[goods.Key].expiration = expiration;
					}
				}
			}

			//UIを更新する
			Dictionary<int, string> goods_info_string_dictionary = new Dictionary<int, string>();
			foreach (KeyValuePair<int, GoodsData> goods in goods_data_dictionary) {
				string info = goods.Value.name + ", "
					+ goods.Key.ToString() + ", "
					+ goods.Value.state.ToString() + ", "
					+ goods.Value.pos.ToString("f3") + ", "
					+ goods.Value.expiration;
				goods_info_string_dictionary.Add(goods.Key, info);
			}
			mainSystem.UpdateDatabaseInfoRefrigerator(goods_info_string_dictionary);
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

		Dictionary<int, float> goods_distance_dictionary = new Dictionary<int, float>();
		foreach (KeyValuePair<int, GoodsData> goods in goods_data_dictionary) {
			goods_distance_dictionary.Add(goods.Key, CalcDistance(ar_camera, goods.Value.obj));
		}

		var sorted = goods_distance_dictionary.OrderBy((x) => x.Value);

		foreach (KeyValuePair<int, float> goods in sorted) {
			if (goods_data_dictionary[goods.Key].state_bool) {
				ShaderChange goods_shaderchange = goods_data_dictionary[goods.Key].obj.GetComponent<ShaderChange>();
				goods_shaderchange.alpha = 0.4f;
				goods_shaderchange.ChangeColors();
				
				goods_data_dictionary[goods.Key].text3d.SetActive(true);

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

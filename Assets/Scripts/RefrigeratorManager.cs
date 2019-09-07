using System;
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

//物品の情報を入れておくクラス
class GoodsInfo {
	public string name;
	public int id;
	public int state;
	public Vector3 pos;
	public string expiration;
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
	private Dictionary<int, GameObject> goods_object_dictionary = new Dictionary<int, GameObject>();
	private Dictionary<int, bool> goods_state_dictionary = new Dictionary<int, bool>();

	private Dictionary<int, GameObject> goods_3dtext_dictionary = new Dictionary<int, GameObject>();

	private Dictionary<int, GoodsInfo> goods_info_dictionary = new Dictionary<int, GoodsInfo>();
	
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
		goods_object_dictionary.Add(7004, greentea);
		goods_object_dictionary.Add(7006, cancoffee);
		goods_object_dictionary.Add(7009, soysauce);
		goods_state_dictionary.Add(7004, false);
		goods_state_dictionary.Add(7006, false);
		goods_state_dictionary.Add(7009, false);

		//オブジェクトにShader変更スクリプトを追加
		foreach (GameObject goods in goods_object_dictionary.Values) {
			goods.AddComponent<ShaderChange>();
		}

		//位置合わせするや～つを配置
		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
		coordinates_adapter = (GameObject)Instantiate(prefab, this.transform);
		coordinates_adapter.transform.parent = refrigerator.transform;
		
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
		//CoordinatesAdapterの位置を調整してカメラとの距離を計算
		coordinates_adapter.transform.localPosition = new Vector3(-0.23f, 0.0f, -0.3f);
		distance_old = distance;
		distance = CalcDistance(coordinates_adapter, ar_camera);

		//最初の1回Shaderを変更する
		if (!change_goods_shader) {
			foreach (GameObject goods in goods_object_dictionary.Values) {
				ShaderChange shaderchange = goods.GetComponent<ShaderChange>();
				shaderchange.ChangeShader(Shader.Find("Custom/Transparent"));
			}
			change_goods_shader = true;
		}

		//距離が閾値以下でデータベースのstateが1だったら表示，違ったら非表示
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
				foreach (KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
					ShaderChange goods_shaderchange = goods.Value.GetComponent<ShaderChange>();
					goods_shaderchange.alpha = 0.0f;
					goods_shaderchange.ChangeColors();
					Change3DTextActive(goods.Key, false);
				}
				if(refrigerator_shaderchange.shader_now != Shader.Find("Custom/ARTransparent")){
					refrigerator_shaderchange.ChangeShader(Shader.Find("Custom/ARTransparent"));
					refrigerator_shaderchange.alpha = rostms_shaderchange.alpha;
					refrigerator_shaderchange.ChangeColors();
				}
			}
			//ずっと近くにいるとき
			else if (distance < distance_to_display && distance_old < distance_to_display){
				foreach(KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
					ShaderChange goods_shaderchange = goods.Value.GetComponent<ShaderChange>();
					if (goods_state_dictionary[goods.Key]) {
						goods_shaderchange.alpha = 0.4f;
						Change3DTextActive(goods.Key, true);
					}
					else {
						goods_shaderchange.alpha = 0.0f;
						Change3DTextActive(goods.Key, false);
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
						if (data.sensor == 3018) {
							foreach (KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
								if (goods.Value.name.IndexOf(data.name) != -1) {
									Vector3 place = new Vector3((float)data.x, (float)data.y, (float)data.z);
									place = Ros2UnityPosition(place);
									if (data.state == 1) {
										goods_state_dictionary[goods.Key] = true;

										Debug.Log(data.name + " pos: " + place.ToString("f2"));
										mainSystem.MyConsole_Add(data.name + " pos: " + place.ToString("f2"));
										goods.Value.transform.localPosition = place;

										id_list.Add(data.id);
									}
									else {
										goods_state_dictionary[goods.Key] = false;
									}

									if (!goods_info_dictionary.ContainsKey(goods.Key)) {
										goods_info_dictionary.Add(goods.Key, new GoodsInfo());
										goods_info_dictionary[goods.Key].id = data.id;
										goods_info_dictionary[goods.Key].name = data.name;
										goods_info_dictionary[goods.Key].expiration = "don't know";
									}
									goods_info_dictionary[goods.Key].state = data.state;
									goods_info_dictionary[goods.Key].pos = place;
								}
							}
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
					foreach(KeyValuePair<int, string> item in expiration_dictionary) {
						ExpirationData expiration_data = JsonUtility.FromJson<ExpirationData>(item.Value);
						string expiration = expiration_data.expiration;
						Debug.Log("id: " + item.Key + ", name: " + goods_object_dictionary[item.Key].name + ", expiration: " + expiration);
						mainSystem.MyConsole_Add("id: " + item.Key + ", name: " + goods_object_dictionary[item.Key].name + ", expiration: " + expiration);

						if (goods_3dtext_dictionary.ContainsKey(item.Key)) {
							goods_3dtext_dictionary[item.Key].GetComponent<TextMeshPro>().text = expiration;
						}
						else {
							GameObject new_3dtext = (GameObject)Instantiate(Resources.Load("TextMeshPro"));
							goods_3dtext_dictionary.Add(item.Key, new_3dtext);
							goods_3dtext_dictionary[item.Key].transform.SetParent(goods_object_dictionary[item.Key].transform, false);
							goods_3dtext_dictionary[item.Key].transform.localPosition = new Vector3(0.0f, 0.15f, 0.0f);
							TextMeshPro TMP = goods_3dtext_dictionary[item.Key].GetComponent<TextMeshPro>();
							TMP.fontSize = 0.6f;
							TMP.text = expiration;
							Change3DTextActive(item.Key, false);
						}

						goods_info_dictionary[item.Key].expiration = expiration;
					}
				}
			}

			//UIを更新する
			Dictionary<int, string> goods_info_string_dictionary = new Dictionary<int, string>();
			foreach(GoodsInfo goods_info in goods_info_dictionary.Values) {
				string info = goods_info.name + ", "
					+ goods_info.id.ToString() + ", "
					+ goods_info.state.ToString() + ", "
					+ goods_info.pos.ToString("f3") + ", "
					+ goods_info.expiration;
				goods_info_string_dictionary.Add(goods_info.id, info);
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
		foreach (KeyValuePair<int, GameObject> goods in goods_object_dictionary) {
			goods_distance_dictionary.Add(goods.Key, CalcDistance(ar_camera, goods.Value));
		}

		var sorted = goods_distance_dictionary.OrderBy((x) => x.Value);

		foreach (KeyValuePair<int, float> goods in sorted) {
			if (goods_state_dictionary[goods.Key]) {
				ShaderChange goods_shaderchange = goods_object_dictionary[goods.Key].GetComponent<ShaderChange>();
				goods_shaderchange.alpha = 0.4f;
				goods_shaderchange.ChangeColors();

				Change3DTextActive(goods.Key, true);

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
	 * 消費期限を表示する3Dテキストの表示非表示切り替え
	 *****************************************************************/
	private void Change3DTextActive(int id, bool active) {
		if (goods_3dtext_dictionary.ContainsKey(id)) {
			if (active) {
				goods_3dtext_dictionary[id].SetActive(true);
			}
			else {
				goods_3dtext_dictionary[id].SetActive(false);
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

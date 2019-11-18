using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//WHS1のデータを取得するためのクラス
public class WHS1Data {
	public float temp;
	public int rate;
	public int[] wave;
}

public class WHS1Viewer : MonoBehaviour {

	//UI制御用
	private MainScript mainSystem;
	
	//データベースと通信するやつ
	private float time = 0.0f;
	private TMSDatabaseAdapter DBAdapter;
	
	//キャリブシステム
	private BsenCalibrationSystem calib_system;

	//3DText
	private GameObject WHS1_3D_Text;
	private TextMeshPro WHS1_3D_TextMeshPro;

	//心拍波形のグラフ
	private GameObject wave_graph;

	private bool init_this_system = false;

	// Start is called before the first frame update
	void Start() {
		//各種オブジェクトを取得
		mainSystem = GameObject.Find("Main System").GetComponent<MainScript>();
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();
		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
	}


	// Update is called once per frame
	void Update() {
		if (!mainSystem.finish_read_config) {
			return;
		}

		//キャリブが終わってから
		if (calib_system.CheckFinishCalibration()) {
			if (!init_this_system) {
				InitThisSystem();
			}
			
			WHS1DataUpdate();
		}
	}

	/*****************************************************************
	 * 3Dテキストオブジェクトを生成
	 *****************************************************************/
	private void InitThisSystem() {
		GameObject prefab = (GameObject)Resources.Load("TextMeshPro");
		WHS1_3D_Text = Instantiate(prefab);
		WHS1_3D_Text.transform.SetParent(GameObject.Find("rostms/world_link/bsen_room_link/bed_link").transform, false);
		WHS1_3D_Text.transform.localPosition = new Vector3(0.0f, 0.7f, 0.0f);
		
		WHS1_3D_TextMeshPro = WHS1_3D_Text.GetComponent<TextMeshPro>();
		WHS1_3D_TextMeshPro.text = "";

		wave_graph = new GameObject("Wave Graph");
		wave_graph.transform.SetParent(GameObject.Find("rostms/world_link/bsen_room_link/bed_link").transform, false);
		wave_graph.transform.localPosition = new Vector3(0.0f, 1.2f, 0.0f);
		wave_graph.transform.localEulerAngles = new Vector3(0, 0, 0);
		wave_graph.transform.localScale = new Vector3(-1, 1, 1);
		wave_graph.AddComponent<LookAtMainCamera>();

		init_this_system = true;
	}

	/*****************************************************************
	 * 表示する文字を更新
	 *****************************************************************/
	private void WHS1DataUpdate() {
		time += Time.deltaTime;
		if (!DBAdapter.CheckWaitAnything() && time > 0.2f) {
			time = 0.0f;
			IEnumerator coroutine = DBAdapter.ReadWHS1();
			StartCoroutine(coroutine);
		}

		if (DBAdapter.CheckReadWHS1()) {
			if (DBAdapter.CheckAbort()) {
				DBAdapter.ConfirmAbort();
			}

			if (DBAdapter.CheckSuccess()) {
				ServiceResponseDB responce = DBAdapter.GetResponce();
				DBAdapter.FinishReadData();

				Debug.Log(responce.values.tmsdb[0].note);
				//mainSystem.MyConsole_Add(responce.values.tmsdb[0].note);

				WHS1Data whs1_data = JsonUtility.FromJson<WHS1Data>(responce.values.tmsdb[0].note);
				Debug.Log("Temp: " + whs1_data.temp.ToString("f1"));
				mainSystem.MyConsole_Add("Temp: " + whs1_data.temp.ToString("f1"));
				Debug.Log("Rate: " + whs1_data.rate);
				mainSystem.MyConsole_Add("Rate: " + whs1_data.rate);
				string debug_string = null;
				foreach (int count in whs1_data.wave) {
					debug_string += count + ",";
				}
				Debug.Log("Wave: " + debug_string);
				//mainSystem.MyConsole_Add("Wave: " + debug_string);

				WHS1_3D_TextMeshPro.text = "Temp: " + whs1_data.temp.ToString("f1") + "[degC]\n";
				WHS1_3D_TextMeshPro.text += "Rate: " + whs1_data.rate.ToString() + "[bpm]";

				UpdateWaveGraph(whs1_data.wave);
				
				mainSystem.UpdateDatabaseInfoWHS1Info(whs1_data.temp, whs1_data.rate);
				mainSystem.UpdateDatabaseInfoWHS1Wave(whs1_data.wave);
			}
		}

		//カメラと近いときに表示
		if (WHS1_3D_Text != null) {
			if (CalcDistance(Camera.main.gameObject, WHS1_3D_Text) < mainSystem.GetConfig().whs1_distance) {
				WHS1_3D_Text.SetActive(true);
			}
			else {
				WHS1_3D_Text.SetActive(false);
			}
		}
	}

	/*****************************************************************
	 * 心拍波形を更新
	 *****************************************************************/
	private void UpdateWaveGraph(int[] wave_list) {
		string debug_string = null;
		foreach (int count in wave_list) {
			debug_string += count + ",";
		}
		Debug.Log("Update Wave Graph: " + debug_string);
		//mainSystem.MyConsole_Add("Update Wave Graph: " + debug_string);
		
		if (CalcDistance(Camera.main.gameObject, wave_graph) < mainSystem.GetConfig().whs1_distance) {
			wave_graph.SetActive(true);

			List<GameObject> line_list = new List<GameObject>();
			GetAllChildren(wave_graph, ref line_list);
			foreach (GameObject obj in line_list) {
				Destroy(obj);
			}
			
			GameObject line_prefab = (GameObject)Resources.Load("Line");
			for (int n = 0; n < wave_list.Length - 1; n++) {
				GameObject line = Instantiate(line_prefab);
				line.name = "Line" + n.ToString();
				line.transform.SetParent(wave_graph.transform, false);
				line.transform.localPosition = new Vector3(0, 0, 0);
				line.transform.localEulerAngles = new Vector3(0, 0, 0);
				line.transform.localScale = new Vector3(1, 1, 1);

				LineRenderer line_rend = line.GetComponent<LineRenderer>();
				line_rend.SetPosition(0, new Vector3(n * 0.01f - 0.5f, ((float)wave_list[n] - 500) / 1500, 0));
				line_rend.SetPosition(1, new Vector3((n + 1) * 0.01f - 0.5f, ((float)wave_list[n + 1] - 500) / 1500, 0));
			}
		}
		else {
			wave_graph.SetActive(false);
		}
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
	 * すべての子オブジェクトを取得
	 *****************************************************************/
	void GetAllChildren(GameObject obj, ref List<GameObject> all_children) {
		Transform children = obj.GetComponentInChildren<Transform>();
		if (children.childCount == 0) {
			return;
		}
		foreach (Transform ob in children) {
			all_children.Add(ob.gameObject);
			GetAllChildren(ob.gameObject, ref all_children);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WHS1Data {
	public float temp;
	public int rate;
	public int[] wave;
}

public class WHS1Viewer : MonoBehaviour {
	
	private float time = 0.0f;
	private TMSDatabaseAdapter DBAdapter;
	
	private BsenCalibrationSystem calib_system;

	private GameObject WHS1_3D_Text;
	//private TextMesh WHS1_3D_TextMesh;
	private TextMeshPro WHS1_3D_TextMeshPro;

	private GameObject wave_graph;

	private bool init_this_system = false;

	// Start is called before the first frame update
	void Start() {
		DBAdapter = GameObject.Find("Database Adapter").GetComponent<TMSDatabaseAdapter>();
		calib_system = GameObject.Find("B-sen Calibration System").GetComponent<BsenCalibrationSystem>();
	}


	// Update is called once per frame
	void Update() {
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
		//GameObject prefab = (GameObject)Resources.Load("3D Text");
		GameObject prefab = (GameObject)Resources.Load("TextMeshPro");
		WHS1_3D_Text = Instantiate(prefab);
		/*
		WHS1_3D_Text.transform.parent = GameObject.Find("rostms/world_link").transform;
		WHS1_3D_Text.transform.localPosition = new Vector3(-2.7f, 0.7f, 10.8f);
		*/
		WHS1_3D_Text.transform.parent = GameObject.Find("rostms/world_link/bsen_room_link/bed_link").transform;
		WHS1_3D_Text.transform.localPosition = new Vector3(0.0f, 0.7f, 0.0f);

		/*
		WHS1_3D_TextMesh = WHS1_3D_Text.GetComponent<TextMesh>();
		WHS1_3D_TextMesh.text = "";
		*/
		WHS1_3D_TextMeshPro = WHS1_3D_Text.GetComponent<TextMeshPro>();
		WHS1_3D_TextMeshPro.text = "";

		wave_graph = new GameObject("Wave Graph");
		/*
		wave_graph.transform.parent = GameObject.Find("rostms/world_link").transform;
		wave_graph.transform.localPosition = new Vector3(-2.7f, 1.0f, 10.8f);
		*/
		wave_graph.transform.parent = GameObject.Find("rostms/world_link/bsen_room_link/bed_link").transform;
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

				WHS1Data whs1_data = JsonUtility.FromJson<WHS1Data>(responce.values.tmsdb[0].note);
				Debug.Log("Temp: " + whs1_data.temp);
				Debug.Log("Rate: " + whs1_data.rate);
				string debug_string = null;
				foreach (int count in whs1_data.wave) {
					debug_string += count + ",";
				}
				Debug.Log("Wave: " + debug_string);

				/*
				WHS1_3D_TextMesh.text = "Temp: " + whs1_data.temp.ToString() + "[degC]\n";
				WHS1_3D_TextMesh.text += "Rate: " + whs1_data.rate.ToString() + "[bpm]";
				*/
				WHS1_3D_TextMeshPro.text = "Temp: " + whs1_data.temp.ToString() + "[degC]\n";
				WHS1_3D_TextMeshPro.text += "Rate: " + whs1_data.rate.ToString() + "[bpm]";

				UpdateWaveGraph(whs1_data.wave);
			}
		}

		if (WHS1_3D_Text != null) {
			if (CalcDistance(Camera.main.gameObject, WHS1_3D_Text) < 3.0f) {
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

		/*
		List<GameObject> line_list = new List<GameObject>();
		GetAllChildren(wave_graph, ref line_list);
		foreach(GameObject obj in line_list) {
			Destroy(obj);
		}

		for(int n = 0; n < wave_list.Length - 1; n++) {	
			GameObject line = new GameObject("Line" + n.ToString());
			line.transform.parent = wave_graph.transform;
			line.transform.localPosition = new Vector3(0, 0, 0);
			line.transform.localEulerAngles = new Vector3(0, 0, 0);
			line.transform.localScale = new Vector3(1, 1, 1);

			LineRenderer line_rend = line.AddComponent<LineRenderer>();
			line_rend.useWorldSpace = false;
			line_rend.positionCount = 2;
			line_rend.startWidth = 0.1f;
			line_rend.endWidth = 0.1f;
			line_rend.widthMultiplier = 0.1f;
			line_rend.SetPosition(0, new Vector3(n * 0.01f - 0.5f, (float)wave_list[n] / 1000, 0));
			line_rend.SetPosition(1, new Vector3((n + 1) * 0.01f - 0.5f, (float)wave_list[n + 1] / 1000, 0));
		}
		*/
		
		if (CalcDistance(Camera.main.gameObject, wave_graph) < 3.0f) {
			wave_graph.SetActive(true);

			List<GameObject> line_list = new List<GameObject>();
			GetAllChildren(wave_graph, ref line_list);
			foreach (GameObject obj in line_list) {
				Destroy(obj);
			}

			/*
			for (int n = 0; n < wave_list.Length - 1; n++) {
				GameObject line = new GameObject("Line" + n.ToString());
				line.transform.parent = wave_graph.transform;
				line.transform.localPosition = new Vector3(0, 0, 0);
				line.transform.localEulerAngles = new Vector3(0, 0, 0);
				line.transform.localScale = new Vector3(1, 1, 1);

				LineRenderer line_rend = line.AddComponent<LineRenderer>();
				line_rend.useWorldSpace = false;
				line_rend.positionCount = 2;
				line_rend.startWidth = 0.1f;
				line_rend.endWidth = 0.1f;
				line_rend.widthMultiplier = 0.1f;
				line_rend.SetPosition(0, new Vector3(n * 0.01f - 0.5f, (float)wave_list[n] / 1000, 0));
				line_rend.SetPosition(1, new Vector3((n + 1) * 0.01f - 0.5f, (float)wave_list[n + 1] / 1000, 0));
				}
			}
			*/
			GameObject line_prefab = (GameObject)Resources.Load("Line");
			for (int n = 0; n < wave_list.Length - 1; n++) {
				GameObject line = Instantiate(line_prefab);
				line.name = "Line" + n.ToString();
				line.transform.parent = wave_graph.transform;
				line.transform.localPosition = new Vector3(0, 0, 0);
				line.transform.localEulerAngles = new Vector3(0, 0, 0);
				line.transform.localScale = new Vector3(1, 1, 1);

				LineRenderer line_rend = line.GetComponent<LineRenderer>();
				/*
				line_rend.SetPosition(0, new Vector3(n * 0.01f - 0.5f, (float)wave_list[n] / 1000, 0));
				line_rend.SetPosition(1, new Vector3((n + 1) * 0.01f - 0.5f, (float)wave_list[n + 1] / 1000, 0));
				*/
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

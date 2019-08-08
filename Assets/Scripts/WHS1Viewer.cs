using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	private TextMesh WHS1_3D_TextMesh;

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

			/*
			time += Time.deltaTime;
			if (!DBAdapter.CheckWaitAnything() && time > 0.3f) {
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
					foreach(int count in whs1_data.wave) {
						debug_string += count + ",";
					}
					Debug.Log("Wave: " + debug_string);

					WHS1_3D_TextMesh.text = "Temp: " + whs1_data.temp.ToString() + "[degC]\n";
					WHS1_3D_TextMesh.text += "Rate: " + whs1_data.rate.ToString() + "[bpm]";
				}
			}
			*/
			WHS1DataUpdate();
		}
	}

	private void InitThisSystem() {
		GameObject prefab = (GameObject)Resources.Load("3D Text");
		WHS1_3D_Text = Instantiate(prefab);
		WHS1_3D_Text.transform.parent = GameObject.Find("rostms/world_link").transform;
		WHS1_3D_Text.transform.localPosition = new Vector3(-2.7f, 0.7f, 10.8f);

		WHS1_3D_TextMesh = WHS1_3D_Text.GetComponent<TextMesh>();
		WHS1_3D_TextMesh.text = "";

		init_this_system = true;
	}

	private void WHS1DataUpdate() {
		time += Time.deltaTime;
		if (!DBAdapter.CheckWaitAnything() && time > 0.3f) {
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

				WHS1_3D_TextMesh.text = "Temp: " + whs1_data.temp.ToString() + "[degC]\n";
				WHS1_3D_TextMesh.text += "Rate: " + whs1_data.rate.ToString() + "[bpm]";
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
	 * オブジェクトどうしの距離を計算
	 *****************************************************************/
	float CalcDistance(GameObject obj_a, GameObject obj_b) {
		Vector3 obj_a_pos = obj_a.transform.position;
		Vector3 obj_b_pos = obj_b.transform.position;
		return Mathf.Sqrt(Mathf.Pow((obj_a_pos.x - obj_b_pos.x), 2) + Mathf.Pow((obj_a_pos.z - obj_b_pos.z), 2));
	}
}

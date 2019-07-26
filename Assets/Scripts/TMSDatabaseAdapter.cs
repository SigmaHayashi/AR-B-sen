using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMSDatabaseAdapter : MonoBehaviour {

	//Android Ros Socket Client関連
	private AndroidRosSocketClient wsc;
	private string srvName = "tms_db_reader";
	private TmsDBReq srvReq = new TmsDBReq();
	private string srvRes;

	private float time = 0.0f;

	[NonSerialized] public bool access_db = false;
	[NonSerialized] public bool success_access = false;
	[NonSerialized] public bool abort_access = false;

	private bool read_marker_pos = false;
	private bool get_refrigerator_item = false;
	private bool read_smartpal_pos = false;

	[NonSerialized] public ServiceResponseDB responce;

	// Start is called before the first frame update
	void Start() {
		//ROSTMSに接続
		wsc = GameObject.Find("Android Ros Socket Client").GetComponent<AndroidRosSocketClient>();
		//srvReq.tmsdb = new tmsdb("ID_SENSOR", 7030, 3001);
		//wsc.ServiceCallerDB(srvName, srvReq);
		//time = 0.0f;
	}

	// Update is called once per frame
	void Update() {
		if (wsc.conneciton_state == wscCONST.STATE_DISCONNECTED) {
			time += Time.deltaTime;
			if (time > 5.0f) {
				time = 0.0f;

				wsc.Connect();
			}
		}

		if (wsc.conneciton_state == wscCONST.STATE_CONNECTED) {
			if (access_db) {
				if (read_marker_pos) {
					time += Time.deltaTime;
					if (time > 1.0f) {
						time = 0.0f;
						srvReq.tmsdb = new tmsdb("ID_SENSOR", 7030, 3001);
						wsc.ServiceCallerDB(srvName, srvReq);
					}
					if (wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
						srvRes = wsc.GetSrvResMsg();
						Debug.Log("ROS: " + srvRes);

						responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);

						success_access = true;
						read_marker_pos = false;
					}
				}

				if (get_refrigerator_item) {
					time += Time.deltaTime;
					if(time > 1.0f) {
						time = 0.0f;
						//srvReq.tmsdb = new tmsdb("PLACE", 2009);
						//wsc.ServiceCallerDB(srvName, srvReq);

						abort_access = true;
						get_refrigerator_item = false;
					}
					if (wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
						srvRes = wsc.GetSrvResMsg();
						Debug.Log("ROS: " + srvRes);

						responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);

						success_access = true;
						get_refrigerator_item = false;
					}
				}

				if (read_smartpal_pos) {
					time += Time.deltaTime;
					if (time > 0.5f) {
						time = 0.0f;

						abort_access = true;
						read_smartpal_pos = false;
					}
					if (wsc.IsReceiveSrvRes() && wsc.GetSrvResValue("service") == srvName) {
						srvRes = wsc.GetSrvResMsg();
						Debug.Log("ROS: " + srvRes);

						responce = JsonUtility.FromJson<ServiceResponseDB>(srvRes);

						success_access = true;
						read_smartpal_pos = false;
					}
				}
			}
		}
	}

	public void FinishReadData() {
		success_access = false;
	}

	public void ConfirmAbort() {
		abort_access = false;
	}


	public IEnumerator ReadMarkerPos() {
		/*
		if (access_db) {
			yield return null;
		}
		*/

		access_db = read_marker_pos = true;

		time = 0.0f;
		srvReq.tmsdb = new tmsdb("ID_SENSOR", 7030, 3001);
		wsc.ServiceCallerDB(srvName, srvReq);

		while (read_marker_pos) {
			yield return null;
		}

		while (success_access) {
			yield return null;
		}
		access_db = false;
	}

	public IEnumerator GetRefrigeratorItem() {
		/*
		if (access_db) {
			yield return null;
		}
		*/

		access_db = get_refrigerator_item = true;

		time = 0.0f;
		srvReq.tmsdb = new tmsdb("PLACE", 2009);
		wsc.ServiceCallerDB(srvName, srvReq);

		while (get_refrigerator_item) {
			yield return null;
		}

		while (success_access || abort_access) {
			yield return null;
		}
		access_db = false;
	}

	public IEnumerator ReadSmartPalPos() {
		access_db = read_smartpal_pos = true;

		time = 0.0f;
		srvReq.tmsdb = new tmsdb("ID_SENSOR", 2003, 3001);
		wsc.ServiceCallerDB(srvName, srvReq);

		while (read_smartpal_pos) {
			yield return null;
		}

		while (success_access || abort_access) {
			yield return null;
		}
		access_db = false;
	}
}

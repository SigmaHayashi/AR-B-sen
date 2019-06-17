using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefrigeratorManager : MonoBehaviour {

	private GameObject refrigerator;
	private GameObject ar_camera;
	
	private GameObject coordinates_adapter;

	private GameObject greentea;
	private GameObject cancoffee;
	private GameObject soysauce;

	private float distance;

	//public Text debug_text;

	// Start is called before the first frame update
	void Start() {
		refrigerator = GameObject.Find("refrigerator_link");
		ar_camera = GameObject.Find("First Person Camera");

		greentea = GameObject.Find("greentea_bottle_x_link");
		cancoffee = GameObject.Find("cancoffee_x_link");
		soysauce = GameObject.Find("soysauce_bottle_black_x_link");

		greentea.SetActive(false);
		cancoffee.SetActive(false);
		soysauce.SetActive(false);

		GameObject prefab = (GameObject)Resources.Load("Coordinates Adapter");
		coordinates_adapter = (GameObject)Instantiate(prefab, this.transform);
		coordinates_adapter.transform.parent = refrigerator.transform;
	}

	// Update is called once per frame
	void Update() {
		//distance = Vector3.Distance(refrigerator.transform.position, ar_camera.transform.position);
		//distance = Mathf.Sqrt(Mathf.Pow((refrigerator_pos.x - ar_camera.transform.position.x), 2) + Mathf.Pow((refrigerator_pos.z - ar_camera.transform.position.z), 2));
		coordinates_adapter.transform.localPosition = new Vector3(-0.23f, 0.0f, -0.3f);
		distance = CalcDistance(coordinates_adapter, ar_camera);

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

		//debug_text.text = distance.ToString("f2");
	}

	float CalcDistance(GameObject obj_a, GameObject obj_b) {
		Vector3 obj_a_pos = obj_a.transform.position;
		Vector3 obj_b_pos = obj_b.transform.position;
		return Mathf.Sqrt(Mathf.Pow((obj_a_pos.x - obj_b_pos.x), 2) + Mathf.Pow((obj_a_pos.z - obj_b_pos.z), 2));
	}
}

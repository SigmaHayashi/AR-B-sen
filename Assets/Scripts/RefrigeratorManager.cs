using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefrigeratorManager : MonoBehaviour {

	private GameObject refrigerator;
	private GameObject ar_camera;

	private GameObject greentea;
	private GameObject cancoffee;
	private GameObject soysauce;

	private float distance;

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
	}

	// Update is called once per frame
	void Update() {
		distance = Vector3.Distance(refrigerator.transform.position, ar_camera.transform.position);

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
	}
}

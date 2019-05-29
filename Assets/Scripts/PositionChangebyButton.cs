using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionChangebyButton : MonoBehaviour {

	public GameObject whichObject;

	public Button posXplusButton;
	public Button posXminusButton;
	public Button posYplusButton;
	public Button posYminusButton;
	public Button posZplusButton;
	public Button posZminusButton;

	public Button rotXplusButton;
	public Button rotXminusButton;
	public Button rotYplusButton;
	public Button rotYminusButton;
	public Button rotZplusButton;
	public Button rotZminusButton;

	// Use this for initialization
	void Start () {
		posXplusButton.onClick.AddListener(onPosXplusClick);
		posXminusButton.onClick.AddListener(onPosXminusClick);
		posYplusButton.onClick.AddListener(onPosYplusClick);
		posYminusButton.onClick.AddListener(onPosYminusClick);
		posZplusButton.onClick.AddListener(onPosZplusClick);
		posZminusButton.onClick.AddListener(onPosZminusClick);

		rotXplusButton.onClick.AddListener(onRotXplusClick);
		rotXminusButton.onClick.AddListener(onRotXminusClick);
		rotYplusButton.onClick.AddListener(onRotYplusClick);
		rotYminusButton.onClick.AddListener(onRotYminusClick);
		rotZplusButton.onClick.AddListener(onRotZplusClick);
		rotZminusButton.onClick.AddListener(onRotZminusClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void onPosXplusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.x += 0.05f;
		whichObject.transform.position = tmp;
	}

	void onPosXminusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.x -= 0.05f;
		whichObject.transform.position = tmp;
	}

	void onPosYplusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.y += 0.05f;
		whichObject.transform.position = tmp;
	}

	void onPosYminusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.y -= 0.05f;
		whichObject.transform.position = tmp;
	}

	void onPosZplusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.z += 0.05f;
		whichObject.transform.position = tmp;
	}

	void onPosZminusClick() {
		Vector3 tmp = whichObject.transform.position;
		tmp.z -= 0.05f;
		whichObject.transform.position = tmp;
	}


	void onRotXplusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.x += 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}

	void onRotXminusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.x -= 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}

	void onRotYplusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.y += 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}

	void onRotYminusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.y -= 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}

	void onRotZplusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.z += 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}

	void onRotZminusClick() {
		Vector3 tmp = whichObject.transform.eulerAngles;
		tmp.z -= 1.0f;
		whichObject.transform.eulerAngles = tmp;
	}
}

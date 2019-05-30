﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class PositionChangebyButton : MonoBehaviour {

	private GameObject whichObject;
	private GameObject childObject;

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

	public Button autoPositioningButton;

	public Text cameraPositionText;
	public Text bsenPositionText;
	public Text debugText;

	private List<AugmentedImage> m_AugmentedImages = new List<AugmentedImage>();

	private Dictionary<int, GameObject> m_dictionary = new Dictionary<int, GameObject>();

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

		autoPositioningButton.onClick.AddListener(onAutoPositioningClick);

		whichObject = GameObject.Find("rostms");
		childObject = GameObject.Find("rostms/world_link");
	}
	
	// Update is called once per frame
	void Update () {
		cameraPositionText.text = "Camera Position : " + Camera.main.transform.position.ToString() + "\n";
		cameraPositionText.text += "Camera Rotation : " + Camera.main.transform.eulerAngles.ToString();

		bsenPositionText.text = "B-sen Position : " + whichObject.transform.position.ToString() + "\n";
		bsenPositionText.text += "B-sen Rotation : " + whichObject.transform.eulerAngles.ToString();


		Session.GetTrackables<AugmentedImage>(m_AugmentedImages, TrackableQueryFilter.Updated);

		GameObject tmp_object = null;
		var image = m_AugmentedImages[0];
		m_dictionary.TryGetValue(image.DatabaseIndex, out tmp_object);

		if(image.TrackingState == TrackingState.Tracking && tmp_object == null) {
			m_dictionary.Add(image.DatabaseIndex, whichObject);

			debugText.text = "Auto Positioning READY";
		}
	}

	void autoPositioning() {
		GameObject tmp_object = null;
		var image = m_AugmentedImages[0];
		m_dictionary.TryGetValue(image.DatabaseIndex, out tmp_object);
		if(image.TrackingState == TrackingState.Tracking && tmp_object != null) {
			//GameObject tmp_object2 = new GameObject();
			//tmp_object2.transform.position = image.CenterPose.position;
			//tmp_object2.transform.eulerAngles = image.CenterPose.rotation.eulerAngles;

			Vector3 tmp_euler = image.CenterPose.rotation.eulerAngles;
			//Vector3 tmp_euler = tmp_object2.transform.eulerAngles;
			tmp_euler.x = 0.0f;
			tmp_euler.y += 90.0f;
			tmp_euler.z = 0.0f;
			//tmp_object2.transform.eulerAngles = tmp_euler;

			Vector3 temp_position = image.CenterPose.position;
			//Vector3 temp_position = tmp_object2.transform.position;
			Vector3 temp_position_sub = new Vector3(-6.2f, 1.6f, 9.2f);
			temp_position_sub -= temp_position;

			whichObject.transform.eulerAngles = tmp_euler;
			childObject.transform.localPosition = temp_position_sub;
		}
	}

	void onAutoPositioningClick() {
		autoPositioning();
	}

	void onPosXplusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.x += 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.x += 0.05f;
		childObject.transform.localPosition = tmp;
	}

	void onPosXminusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.x -= 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.x -= 0.05f;
		childObject.transform.localPosition = tmp;
	}

	void onPosYplusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.y += 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.y += 0.05f;
		childObject.transform.localPosition = tmp;
	}

	void onPosYminusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.y -= 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.y -= 0.05f;
		childObject.transform.localPosition = tmp;
	}

	void onPosZplusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.z += 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.z += 0.05f;
		childObject.transform.localPosition = tmp;
	}
	void onPosZminusClick() {
		/*
		Vector3 tmp = whichObject.transform.position;
		tmp.z -= 0.05f;
		whichObject.transform.position = tmp;
		*/

		Vector3 tmp = childObject.transform.localPosition;
		tmp.z -= 0.05f;
		childObject.transform.localPosition = tmp;
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
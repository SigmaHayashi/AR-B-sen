using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class PositionChangebyButton : MonoBehaviour {

	private GameObject whichObject;
	//private GameObject childObject;
	private GameObject coordinatesAdapter;
	private GameObject ballObject;

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

	private bool detected_marker = false;
	private AugmentedImage marker;

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
		//childObject = GameObject.Find("rostms/world_link");
		coordinatesAdapter = GameObject.Find("rostms/CoordinatesAdapter");
		ballObject = GameObject.Find("rostms/world_link/MarkerPositionMemo");
	}
	
	// Update is called once per frame
	void Update () {
		cameraPositionText.text = "Camera Position : " + Camera.main.transform.position.ToString() + "\n";
		cameraPositionText.text += "Camera Rotation : " + Camera.main.transform.eulerAngles.ToString();
		
		bsenPositionText.text = "B-sen Position : " + whichObject.transform.localPosition.ToString() + "\n";
		bsenPositionText.text += "B-sen Rotation : " + whichObject.transform.eulerAngles.ToString();

		Session.GetTrackables<AugmentedImage>(m_AugmentedImages, TrackableQueryFilter.Updated);

		if (!detected_marker) {
			foreach(var image in m_AugmentedImages) {
				if(image.TrackingState == TrackingState.Tracking) {
					detected_marker = true;
					marker = image;
				}
			}
		}
	}

	/*****************************************************************
	 * 自動キャリブレーション
	 *****************************************************************/
	void autoPositioning() {
		if (detected_marker) {
			if(marker.TrackingState == TrackingState.Tracking) {
				Quaternion new_rot = new Quaternion();
				new_rot = marker.CenterPose.rotation;
				new_rot *= Quaternion.Euler(0, 0, 90);
				new_rot *= Quaternion.Euler(90, 0, 0);

				Vector3 new_euler = new_rot.eulerAngles;
				new_euler.x = 0.0f;
				new_euler.z = 0.0f;

				whichObject.transform.eulerAngles = new_euler;

				Vector3 marker_position = marker.CenterPose.position;
				Vector3 ball_position = ballObject.transform.position;
				Vector3 offset_vector = marker_position - ball_position;

				Vector3 temp_room_position = whichObject.transform.position;
				temp_room_position += offset_vector;
				whichObject.transform.position = temp_room_position;

				debugText.text = "Auto Positioning DONE";
			}
		}
	}

	/*****************************************************************
	 * ボタン押したとき
	 *****************************************************************/
	void onAutoPositioningClick() {
		autoPositioning();
	}

	void onPosXplusClick() {
		Vector3 tmp = new Vector3(0.1f, 0.0f, 0.0f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
		whichObject.transform.position = tmp;
	}

	void onPosXminusClick() {
		Vector3 tmp = new Vector3(-0.1f, 0.0f, 0.0f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
		whichObject.transform.position = tmp;
	}

	void onPosYplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.1f, 0.0f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
		whichObject.transform.position = tmp;
	}

	void onPosYminusClick() {
		Vector3 tmp = new Vector3(0.0f, -0.1f, 0.0f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
		whichObject.transform.position = tmp;
	}

	void onPosZplusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, 0.1f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
		whichObject.transform.position = tmp;
	}
	void onPosZminusClick() {
		Vector3 tmp = new Vector3(0.0f, 0.0f, -0.1f);
		coordinatesAdapter.transform.localPosition = tmp;

		tmp = coordinatesAdapter.transform.position;
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

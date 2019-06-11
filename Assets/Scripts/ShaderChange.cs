using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderChange : MonoBehaviour {

	public Text debug_text;

	Renderer[] renderers;
	Material[] mats;

	private List<Color> origin_colors = new List<Color>();

	float alpha = 0.5f;

	// Start is called before the first frame update
	void Start() {
		debug_text.text += "Start\n";

		//ChangeShader(cube, "Materials/SemiTransparent");
		//ChangeColor(cube, 0.5f);
		
		renderers = GetComponentsInChildren<Renderer>();
		debug_text.text += "renderers\n";

		ChangeShader();
		debug_text.text += "ChangeShader\n";

		SaveColors();
		debug_text.text += "SaveColors\n";

		ChangeColors();
		debug_text.text += "ChangeColors\n";
	}

	// Update is called once per frame
	void Update() {
		foreach(Renderer ren in renderers) {
			mats = ren.materials;
			for(int i = 0; i < ren.materials.Length; i++) {
				mats[i].shader = Shader.Find("Custom/SemiTransparent");
			}
		}
	}

	void ChangeShader() {

	}

	void SaveColors() {
		foreach(Renderer ren in renderers) {
			mats = ren.materials;
			for(int i = 0; i < ren.materials.Length; i++) {
				origin_colors.Add(mats[i].color);
			}
		}
	}

	void ChangeColors() {
		int id = 0;
		foreach(Renderer ren in renderers) {
			mats = ren.materials;
			for(int i = 0; i < ren.materials.Length; i++) {
				Color tmp_color = origin_colors[id++];
				tmp_color.a = alpha;
				mats[i].SetColor("_Color", tmp_color);
			}
			ren.materials = mats;
		}
	}

	/*
	void ChangeShader(GameObject targetObject, string shader_name_to, string shader_name_from = "") {
		foreach (Transform transform in targetObject.GetComponentInChildren<Transform>(true)) {
			if(transform.GetComponent<Renderer>() != null) {
				var materials = transform.GetComponent<Renderer>().materials;
				for(int i = 0; i< materials.Length; i++) {
					Material material = materials[i];
					if(shader_name_from == "") {
						material.shader = Shader.Find(shader_name_to);
						//material.shader = semitransparent;
					}
					else {
						if(material.shader.name == shader_name_from) {
							material.shader = Shader.Find(shader_name_to);
							//material.shader = semitransparent;
						}
					}
				}
			}
		}
	}

	void ChangeColor(GameObject targetObject, float alpha) {
		//int renderers_id = 0;
		foreach(Transform transform in targetObject.GetComponentInChildren<Transform>(true)) {
			Renderer renderer = transform.GetComponent<Renderer>();
			if(renderer != null) {
				mats = renderer.materials;
				for(int i = 0; i < renderer.materials.Length; i++) {
					Color tmp_color = mats[0].color;
					tmp_color.a = alpha;
					mats[i].SetColor("_Color", tmp_color);
				}
				renderer.materials = mats;
			}
		}
	}
	*/
}

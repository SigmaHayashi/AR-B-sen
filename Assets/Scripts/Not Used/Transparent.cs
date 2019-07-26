using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transparent : MonoBehaviour { 

	public Text debug_text;

	Renderer[] renderers;
	Material[] mats;

	private List<Color> origin_colors = new List<Color>();

	public float alpha;

	// Start is called before the first frame update
	void Start() {
		renderers = GetComponentsInChildren<Renderer>();
		debug("renderers");

		ChangeShader();
		debug("ChangeShader");

		SaveColors();
		debug("SaveColors");

		ChangeColors();
		debug("ChangeColors");
	}

	// Update is called once per frame
	void Update() {

	}

	void ChangeShader() {
		debug(Shader.Find("Custom/Transparent").name.ToString());

		foreach (Renderer ren in renderers) {
			mats = ren.materials;
			for (int i = 0; i < ren.materials.Length; i++) {
				mats[i].shader = Shader.Find("Custom/Transparent");
			}
		}
	}

	void SaveColors() {
		foreach (Renderer ren in renderers) {
			mats = ren.materials;
			for (int i = 0; i < ren.materials.Length; i++) {
				origin_colors.Add(mats[i].color);
			}
		}
	}

	void ChangeColors() {
		int id = 0;
		foreach (Renderer ren in renderers) {
			mats = ren.materials;
			for (int i = 0; i < ren.materials.Length; i++) {
				Color tmp_color = origin_colors[id++];
				tmp_color.a = alpha;
				mats[i].SetColor("_Color", tmp_color);
			}
			ren.materials = mats;
		}
	}

	void debug(string message) {
		if (debug_text != null) {
			debug_text.text += message + "\n";
		}
	}
}

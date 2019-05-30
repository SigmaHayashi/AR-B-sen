using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestChildren : MonoBehaviour {

	public GameObject child1;
	public string child1Name;

	// Use this for initialization
	void Start () {
		child1.name = child1Name;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

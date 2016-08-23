using UnityEngine;
using System.Collections;

public class CameraHandler : MonoBehaviour {
	public float offset = 10f;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.MoveTowards (transform.position, transform.position + (Vector3.one * offset), Time.deltaTime);
	}
}

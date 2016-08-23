using UnityEngine;
using System.Collections;

public class EndlessProduct : MonoBehaviour {
	public int ID;							//ID del producto al cual representa

	public int maxProducts = 4;				//Cantidad maxima de productos

	void Awake(){

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Asignar estado del producto
	public void SetupProduct () {
		ID = Random.Range (0, maxProducts);

		GetComponent<SpriteRenderer> ().sprite = EndlessController.instance.GetSprite (ID);
	}
}

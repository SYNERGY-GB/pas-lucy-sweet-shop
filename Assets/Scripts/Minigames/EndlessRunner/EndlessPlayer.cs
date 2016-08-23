using UnityEngine;
using System.Collections;

public class EndlessPlayer : MonoBehaviour {
	private Rigidbody2D playerR;								//Referencia interna del rigidbody

	public float jumpForce;										//Fuerza de salto

	private Vector2 savedVelocity;								//Velocidad guardada en pausa
	private float savedGravity;									//Gravedad guardada en pausa

	void Awake(){
		playerR = GetComponent<Rigidbody2D> ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space) && !EndlessController.instance.OnHold()) {
			playerR.AddForce (jumpForce * Vector2.up);
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.CompareTag ("EndlessProduct")) {
			EndlessController.instance.UpdateUI (col.gameObject.GetComponent<EndlessProduct> ().ID);

			//Debug.Log (col.gameObject.GetComponent<EndlessProduct> ().ID);
			Destroy (col.gameObject);
		}
	}

	//Pausa el movimiento del jugador
	public void Pause () {
		//Velocidad
		savedVelocity = playerR.velocity;
		playerR.velocity = Vector2.zero;

		//Gravedad
		savedGravity = playerR.gravityScale;
		playerR.gravityScale = 0f;
	}

	//Reanudar el movmiento del jugador
	public void Resume () {
		playerR.velocity = savedVelocity;
		playerR.gravityScale = savedGravity;
	}
}

using UnityEngine;
using System.Collections;

public class BlockbreakerPlayer : MonoBehaviour {
	private Rigidbody2D playerR;			//Referencia interna del rigidbody2D

	public float playerCornerCoord = 3f;	//Coordenada limite de movimiento del jugador
	public float speed = 10f;				//Velocidad del jugador

	private Vector3 targetPos;				//Posicion objetivo / Usada para moverse usando rigidbody2D

	private bool onStop;					//Bandera: Indica si se esta en pausa

	private Vector3 startPosition;			//Posicion inicial

	void Awake () {
		//Obtener referenceia del rigidbody
		playerR = GetComponent<Rigidbody2D> ();

		startPosition = transform.position;
	}

	// Use this for initialization
	void Start () {
		//Inicializar posicion
		targetPos = transform.position;

		onStop = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (onStop) return;

		//Recibe Input del jugador / va a la izquierda
		if (Input.GetKey (KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { //izquierda
			targetPos -= Vector3.right * speed * Time.deltaTime; 	//Aplicar velocidad
			targetPos.x = Mathf.Clamp(targetPos.x, -playerCornerCoord, playerCornerCoord);		//Limitar movimiento
		}
		else if (Input.GetKey (KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { //derecha
			targetPos += Vector3.right * speed * Time.deltaTime;	//Aplicar velocidad
			targetPos.x = Mathf.Clamp(targetPos.x, -playerCornerCoord, playerCornerCoord);		//Limitar movimiento
		}

		//Mover jugador
		playerR.MovePosition (targetPos);
	}

	public void Stop () {
		onStop = true;
	}

	public void Resume () {
		onStop = false;
	}

	public void Restart () {
		transform.position = startPosition;
	}
}

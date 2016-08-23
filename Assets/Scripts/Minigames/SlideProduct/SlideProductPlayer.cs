using UnityEngine;
using System.Collections;

public class SlideProductPlayer : MonoBehaviour {
	//private bool startMove;		//Indica a update que se debe empezar el movimiento a la posicion objetiva
	private bool onMove;			//Indica si se esta en movimiento

	private Vector3 targetPos; 		//Indica posicion final del movimiento
	public float moveSpeed = 7f;	//Velocidad de movimiento del item

	public static bool onWin;		//Flag: Indica que se gano el juego

	private int productType;		//Tipo de producto
	
	// Use this for initialization
	void Start () {
		targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (onMove && !SlideProductController.instance.CheckPause() && !SlideProductController.instance.CheckMenu()) {	//Mover el jugador si se esta en movimiento
			//Interpola el movimiento entre la posicion actual del jugador y la objetivo
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

			//Checkea si la posicion del jugador es la objetivo
			if(Vector3.Distance(transform.position, targetPos) < Mathf.Epsilon){
				//Resetear movimiento
				onMove = false;
			}
		}
	}

	//Activa el movimiento del item a la posicion objetiva
	public void MoveToTarget(Vector3 newPos) {
		if (newPos.z == 1f) //newPos es vec3(1f, 1f, 1f) / No se encontro obstaculo valido
			return;

		//Asignar estado para movimiento
		onMove = true;
		targetPos = newPos;

		//Debug.Log("Vec3: " + auxV3);
		if(MusicController.instance != null){
			MusicController.instance.PlayMinigameSound (0);
		}
	}

	//Checkea si el jugador esta en movimiento
	public bool isMoving(){
		return onMove;
	}

	//Asignar al jugador nuevo producto a representar
	public void SetID(int newID){
		GetComponent<SpriteRenderer> ().sprite = SlideProductController.instance.GetSprite (newID);

		productType = newID;
	}

	//Retona el ID del producto actual
	public int GetProductID(){
		return productType;
	}

	//Colocar jugador en la posicion objetivo
	public void SetPosition(Vector3 newPos){
		transform.position = newPos;
	}

	//Retorna la posicion actual del item
	public Vector3 GetPosition(){
		return transform.position;
	}

	//Retorna la posicion redondeada actual del item
	public Vector3 GetRoundedPosition(){
		return new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
	}
}

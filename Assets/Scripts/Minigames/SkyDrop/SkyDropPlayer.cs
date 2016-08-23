using UnityEngine;
using System.Collections;

public class SkyDropPlayer : MonoBehaviour {
	private Rigidbody2D playerR;			//Referencia interna del rigidbody2D

	public float playerCornerCoord = 3f;	//Coordenada limite de movimiento del jugador
	public float speed = 10f;				//Velocidad del jugador

	private Vector3 targetPos;				//Posicion objetivo / Usada para moverse usando rigidbody2D

	private Animator animPlayer;			//Referencia interna del animator

	//Referencia interna de SkyDropController
	public SkyDropController skyDropController;

	void Awake(){
		//Obtener referencias
		playerR = GetComponent<Rigidbody2D> ();
		animPlayer = GetComponent<Animator> ();
	}

	// Use this for initialization
	void Start () {
		//Inicializar posicion
		targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (skyDropController.OnHold ()) { //No mover si se esta en pausa o menu
			playerR.velocity = Vector2.zero;
			return;
		}

		//Recibe Input del jugador / va a la izquierda
		if (Input.GetKey (KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { //izquierda
			targetPos -= Vector3.right * speed * Time.deltaTime; 	//Aplicar velocidad
			targetPos.x = Mathf.Clamp(targetPos.x, -playerCornerCoord, playerCornerCoord);		//Limitar movimiento

            //animPlayer.SetTrigger("MoveLeft");
		}
		else if (Input.GetKey (KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { //derecha
			targetPos += Vector3.right * speed * Time.deltaTime;	//Aplicar velocidad
			targetPos.x = Mathf.Clamp(targetPos.x, -playerCornerCoord, playerCornerCoord);		//Limitar movimiento

            //animPlayer.SetTrigger("MoveRight");
		}
        else {
            //animPlayer.SetTrigger("Stop");
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Pause();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            Resume();
        }

		//Mover jugador
		playerR.MovePosition (targetPos);
	}

	void OnTriggerEnter2D(Collider2D col){
		if (col.CompareTag ("SkyBoxItem")) { 	//Checkea si se colisiono con una caja
			Destroy(col.gameObject);			//Destruye la caja

			//animPlayer.SetTrigger ("ObjectAcquired");

			//Actualizar contador en la UI
			skyDropController.UpdateUI(col.gameObject.GetComponent<SkyDropBox>().GetID());

			//Sound
			if(MusicController.instance != null){
				MusicController.instance.PlayMinigameSound (0);
			}
		}
	}

    public void Pause() {
        animPlayer.speed = 0f;
    }

    public void Resume() {
        animPlayer.speed = 1f;
    }
}

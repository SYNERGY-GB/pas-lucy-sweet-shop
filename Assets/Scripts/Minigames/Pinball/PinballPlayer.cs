using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PinballPlayer : MonoBehaviour {
	private bool onRotation;				//Dice si se esta rotando la direccion inicial del movimiento
	private bool onPower;					//Dice si se esta asignando la velocidad inicial

	//0 - Cookie, 1 - Cake, 2 - Chocolate, 3 - Cupcake
	public int type;						//Tipo de dulce actual
	public int maxType = 4;					//Cantidad maxima del tipo de dulce

	public Transform dirT;					//Referencia del sprite que indica la direccion
	public float angle = 45f;				//Angulo de variacion
	private bool goingLeft;					//Verifica si se esta rotando la direccion hacia la derecha

	public float rotationSpeed = 10f;		//Velocidad del movimiento de la direccion

	private float currentAngle;				//Contiene el angulo el cual se movera el producto

	public Slider powerSlider;				//Slider de donde se obtiene la velocidad del producto
	private bool powerUp;					//Indica si el poder esta aumentando
	public float powerSpeed = 7f;			//Velocidad de la barra de poder

	private Rigidbody2D playerR;			//Referencia interna del rigidbody

	public Transform targetT;				//Referencia del sprite que representa el objetivo
	public Vector3[] targetPosArray;		//Arreglo que contiene cada posicion objetivo posible
	private int targetID;					//ID del arreglo de posiciones objetivo

	private bool onActive;					//Bandera que indica si se esta activo el update

	public Button restartButton;			//Referencia del boton de restaurar producto

	BaseEventData bd;						//Auxiliar para quitar el focus a un boton

    private Vector2 savedVelocity;          //Velocidad guardada antes de una pausa

    //Referencia interna del controlador de pinball
    public PinballController pinballController;

	void Awake() {
		//Obtener referencias
		playerR = GetComponent<Rigidbody2D> ();

		bd = new BaseEventData(EventSystem.current); 
		bd.selectedObject = null;
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (!onActive || pinballController.OnHold())
			return;

		//Chequear si el jugador presiono espacio
		//Debug.Log (onRotation + " " + onPower);
		if (Input.GetKeyDown (KeyCode.Space)) {
			//Colocar rotacion
			if(onRotation){
				onRotation = false; onPower = true;

				currentAngle = dirT.localRotation.z * 2f; //Asignar rotacion
				//Debug.Log("Rotacion: " + currentAngle * Mathf.Rad2Deg);
			}
			//Colocar velocidad inicial
			else if(onPower){
				onPower = false;

				//Asignar velocidad
				playerR.velocity = new Vector2(Mathf.Sin(-currentAngle) * powerSlider.value, 
				                               Mathf.Cos( currentAngle) * powerSlider.value) * 10f;

				//Habilitar boton de restaurar
				//restartButton.gameObject.SetActive(true);
				//restartButton.OnDeselect(bd); //Quita el focus al boton

                //Esconder UI
                pinballController.setActiveGameUI(false);
			}
		}

		//Mover la direccion de un lado a otro
		if (onRotation) {
			//Verificar si ya se llego al angulo objetivo
			//Debug.Log((dirT.localRotation.z * Mathf.Rad2Deg) + " : " + (angle / 2f));
			if(Mathf.Abs((dirT.localRotation.z * Mathf.Rad2Deg) + (angle / 2f)) < 1f && !goingLeft) {
				goingLeft = true;
			}
			else if(Mathf.Abs((dirT.localRotation.z * Mathf.Rad2Deg) - (angle / 2f)) < 1f && goingLeft) {
				goingLeft = false;
			}

			//Realizar interpolacion
			dirT.localRotation = Quaternion.RotateTowards(dirT.localRotation, 
			                                              Quaternion.Euler(new Vector3(0f, 0f, (goingLeft) ? angle : -angle)),
			                 							  rotationSpeed * Time.deltaTime);
		}

		//Sube o baja la cantidad de poder
		if (onPower) {
			//Verificar si debe bajar el valor de la barra
			if(Mathf.Abs(powerSlider.value - 1f) <= Mathf.Epsilon && powerUp) {
				powerUp = false;
			}
			//Verificar si se debe subir el valor de la barra
			else if (powerSlider.value <= Mathf.Epsilon && !powerUp) {
				powerUp = true;
			}

			//Realizar interpolacion
			powerSlider.value = Mathf.MoveTowards(powerSlider.value, (powerUp) ? 1f : 0f, powerSpeed * Time.deltaTime);
		}

		//Verifica si la velocidad es muy baja y no se esta rotando o en poder
        if(!onPower && !onRotation && Mathf.Abs(playerR.velocity.magnitude) < 1f) { //positive magnitude < 1.0f
            Debug.Log("Stop Bouncing: " + playerR.velocity + " mag: " + playerR.velocity.magnitude);
            SetupNextProduct(false);
        }
	}

	//Es llamada cuando se colisiona con algun collisionador trigger (gatillo)
	void OnTriggerEnter2D(Collider2D col) {
		if(col.CompareTag("PinballTarget")) {	//Si choco con el objetivo
            if (MusicController.instance != null) {
                MusicController.instance.PlayMinigameSound(1); //Cash sound
            }
			SetupNextProduct(true);
		}
	}

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("PinballObstacle")) {
            if (MusicController.instance != null) {
                MusicController.instance.PlayMinigameSound(0); //Bounce sound
            }
        }
    }

    //void OnCollisionEnter2D(Collision2D col) {
    //    if (col.gameObject.CompareTag("PinballObstacle")) {
    //        Debug.Log("HERE");

    //        Vector2 obstacleDir = playerR.transform.position - col.gameObject.transform.position; //Direccion de obstaculo a pelota
    //        obstacleDir.Normalize();

    //        Vector2 newDir = (obstacleDir + playerR.velocity.normalized) / 2f; //Nueva direccion a rebotar

    //        //Fuerza actual
    //        float currentForce = Mathf.Sqrt(playerR.velocity.x * playerR.velocity.x + playerR.velocity.y * playerR.velocity.y);

    //        //Colocar nueva velocidad
    //        playerR.velocity = newDir * (currentForce - 0.04f); //Restar la friccion al chocar a la fuerza actual
    //    }
    //}

	void RandomSortTarget() {
		Vector3 newTarget; int pos;
		//Ordena los elementos del arreglo de objetivos aleatoriamente
		for (int i = 0; i < targetPosArray.Length; i++) {
			pos = Random.Range(0, targetPosArray.Length);

			//Swap de posiciones
			newTarget = targetPosArray[pos];
			targetPosArray[pos] = targetPosArray[i];
			targetPosArray[i] = newTarget;
		}
	}

	public void InitScene(){
		//Inicializar estado del jugador
		onRotation = true;
		onPower = false;
		
		goingLeft = false;
		
		powerSlider.value = 0f;
		powerUp = true;
		
		//Random Sort de posiciones objetivos
		RandomSortTarget ();
		//Colocar target
		targetID = 0;
		targetT.position = targetPosArray [targetID];

		//Activar update
		onActive = true;

		//Asignar producto aleatorio
		type = Random.Range (0, maxType);
		GetComponent<SpriteRenderer> ().sprite = PinballController.instance.GetSprite (type);

		//Inhabilitar boton de restaurar
		restartButton.gameObject.SetActive(false);
	}

	//Asignar nuevo producto
	public void SetupNextProduct(bool targetReached){
        if (targetReached) {
            PinballController.instance.UpdateUI(type); //Actualizar UI
        }

        //Mostrar UI de minijuego
        pinballController.setActiveGameUI(true);

		//Inicializar estado del jugador
		onRotation = true;
		onPower = false;
		
		goingLeft = false;
		
		powerSlider.value = 0f;
		powerUp = true;

		//Coloca al producto en la posicion inicial
		PinballController.instance.RestartPlayerPos ();

		//Resetear velocidad
		playerR.velocity = Vector3.zero;

		//Colocar target
		targetID = (targetID < targetPosArray.Length) ? targetID + 1 : 0;
		targetT.position = targetPosArray [targetID];
		
		//Asignar producto aleatorio
		type = Random.Range (0, maxType);
        
        //Comprobar que hay disponibilidad del producto
        int[] _currentInv = pinballController.GetBoxCounter();

        if (_currentInv[type] > 0) { //Hay disponibilidad
            //Do nothing here...
        }
        else { //Buscar linealmente un producto disponible
            type = -1;
            for (int i = 0; i < _currentInv.Length; i++) {
                if (_currentInv[i] > 0) {
                    type = i; break;
                }
            }
        }

        //En caso de no haber encontrado producto disponible, terminar el juego
        if(type < 0) {
            pinballController.OnWin(true);
        }

		GetComponent<SpriteRenderer> ().sprite = PinballController.instance.GetSprite (type);

		//Inhabilitar boton de restaurar
		restartButton.gameObject.SetActive(false);
	}

	//Asigna nueva posicion
	public void SetPosition(Vector3 pos) {
		transform.position = pos;
	}

	//Retorna la posicion del producto
	public Vector3 GetPosition(){
		return transform.position;
	}

	//Asigna una nueva rotacion
	public void SetRotation(Vector3 rot){
		transform.localRotation = Quaternion.Euler (rot);
	}

    //Detener movimiento de la pelota
    public void Stop() {
        savedVelocity = playerR.velocity;
        playerR.velocity = Vector2.zero;
    }

    //Reanudar movimiento de la pelota
    public void Resume() {
        playerR.velocity = savedVelocity;
    }
}

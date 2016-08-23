using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndlessController : MonoBehaviour {
	public static EndlessController instance;	//Singleton

	public GameObject productPf;				//Referencia de los prefabs producto, ostaculo y piso
	public GameObject obstaclePf;
	public GameObject floorPf;

	public float floorWidth;					//Ancho del piso
	public float cameraWidth;					//Ancho de la camara

	public Transform playerT;					//Referencia al transform del jugador

	public Sprite[] allSprites;					//Arreglo de todos los sprites

	public float floorHeight = -5f;				//Altura del piso
	public float[] productHeight;				//Altura del producto
	public float obstacleHeight = -4.1f;		//Altura del obstaculo

	public float proximityTolerance;			//Tolerancia entre la distancia de los productos con respecto a los obstaculos
	private Vector3 lastProductPos;				//Posicion del ultimo producto en la escena
	private Vector3 lastObstaclePos;			//Posicion del ultimo obstaculo en la escena

	public Transform parentAll;					//Padre de todos los elementos en la escena

    public Rigidbody2D floorR;                  //Referencia del rigidbody del objeto a mover

    public float acceleration = 0.01f;          //Aceleracion
    public float speed = 5f;                    //Velocidad de movimiento
	private float initialSpeed;					//Velocidad inicial

	public int maxElementsOnScene = 20;			//Maxima cantidad de elementos en la escena
	private GameObject[] elementsGO;			//Referencia interna de todos los prefabs en la escena
	private int elementID;						//Posicion actual en el arreglo de elementos

	public float spawnTimeProduct = 4f;			//Tiempo de aparacion entre productos
	public float spawnTimeObstacle = 6f;		//Tiempo de aparacion entre obstaculos
	private float nextProduct, nextObstacle;	//Tiempo para el siguiente producto y obstaculo respectivo

	private Vector3 lastFloorPos;				//Ultima posicion asignada
	private float floorCant;					//Cantidad total de prefabs piso puestos en la escena

	private float savedVelocity;				//Velocidad antes de la pausa

	public EndlessPlayer endlessPlayer;			//Referencia interna del script del jugador

	//Template Begin
	public Text[] boxCantText;					//Texto UI, Arreglo de todos los textos de cajas de la interfaz derecha
	private int[] boxCounter;					//Contador de cuantas cajas el jugador a recolectado

	public Text timeTextUI;						//Texto de tiempo UI
	private float startTime;					//Tiempo de inicio
	private float offsetTime;					//Tiempo en estado de pausa
	private float offsetGameDuration;			//Tiempo acumulado en estado de pausa
	public float gameDurationTime = 80f;		//Tiempo de duracion

	public float delayOnWin = 4f;				//Delay para ir a la siguiente escena

	public GameObject introUI;					//UI de inicio
	public GameObject endUI;					//UI de finalizacion del juego
	public GameObject pauseUI;					//UI de minijuego pausado

	public float slowUpdateDelta = 0.2f;		//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;					//Referencia interna de Coroutine

	public GameObject confirmUI;				//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;						//Identifica a quien le pertenece el mensaje de confirmacion

	//Tamaño inicial de la animacion de los numeros de la derecha
	public Vector3 rightNumberStartScale = new Vector3(1.5f, 1.5f, 1f);
	public float rightNumberSpeed = 10f;		//Velocidad de animacion de numeros de la derecha

	private bool onPause;						//Flag: Indica si el juego esta en pausa
	private bool onMenu;						//Flag: Indica si el juego esta mostrando algun menu

	private GameObject auxGO;					//GameObject auxiliar
	private float timerAux;						//Timer (float) auxiliar
	//Template End

    void Awake() {
		//Logica del singleton
		if (instance == null) {
			instance = this;
		}
		else if(this != instance) {
			Destroy(this.gameObject);
		}

		//Inicializar el contador interno de cajas recolectadas
		boxCounter = new int[boxCantText.Length];

		//Inicializar textos de las UI
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].text = "0";
		}
		timeTextUI.text = "00:00";

		lastFloorPos = Vector3.zero;
		floorWidth = floorPf.transform.localScale.x;
		floorCant = 0;

		elementsGO = new GameObject[maxElementsOnScene];
    }

	// Use this for initialization
	void Start () {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Disable / Enable Music
		if (MusicController.instance != null) {
			if (MusicController.instance.MusicStatus ()) {
				//disableSoundGO.SetActive (false);
			} 
			else {
				//disableSoundGO.SetActive (true);
			}

			//Setup Minigame Music
			MusicController.instance.ChangeMusic(2);
		} 
		else {
			//disableSoundGO.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (onMenu) //Si el focus esta en el juego
			return;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			onPause = !onPause;

			if (onPause) {
				timerAux = Time.time;

				//Guardar velocidad
				savedVelocity = speed;
				floorR.velocity = Vector2.zero;

				//Detener actividad del jugador
				endlessPlayer.Pause ();

				//Mostrar UI Pausa
				pauseUI.SetActive(true);
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				offsetGameDuration += offsetTime;

				//Actualizar tiempos
				nextProduct += offsetTime;
				nextObstacle += offsetTime;

				//Restaurar velocidad
				floorR.velocity = Vector2.left * speed;

				//Reanudar actividad del jugador
				endlessPlayer.Resume ();

				//Esconder UI Pausa
				pauseUI.SetActive(false);
			}
		}

		//Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}

		if (!onPause) {
			//Apply accelaration
			speed += acceleration * Time.deltaTime;
			floorR.velocity = Vector2.left * speed;

			//Chequear spawn del proximo producto
			if(Time.time > nextProduct) {
				nextProduct = Time.time + spawnTimeProduct;

				AddNewProduct ();
			}

			//Chequear spawn del proximo obstaculo
			if(Time.time > nextObstacle) {
				nextObstacle = Time.time + spawnTimeObstacle;

				AddNewObstacle ();
			}
		}

		if (Mathf.Abs(parentAll.position.x) > floorCant * floorWidth - 14f) { //Verificar si se debe agregar nuevo piso // -5f => Offset
			AddNewFloor ();
		}
	}

	public Sprite GetSprite (int val) {
		return allSprites [val];
	}

	public void OnWin () {
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;

		//Volcar data a GameController
		if(GameController.instance != null) 
			GameController.instance.SetProductCounter (boxCounter);
	}

	//Inicializa el minijuego
	void InitGame(){
		//Set speed
		initialSpeed = speed;
		floorR.velocity = Vector2.left * speed;

		//Set timers
		startTime = Time.time;
		nextProduct = Time.time + spawnTimeProduct;
		nextObstacle = Time.time + spawnTimeObstacle;

		//Inicializar variables
		timeTextUI.text = "0.0s";
		offsetTime = offsetGameDuration = 0f;

		//Set focus 
		onMenu = onPause = false;

		//Start time
		slowCor = StartCoroutine (SlowUpdate ());
	}

	void AddNewObstacle () {
		elementID = (elementID + 1 >= maxElementsOnScene) ? 0 : elementID + 1;

		if (elementsGO [elementID] != null) { //Destruir elemento correspondiente
			Destroy (elementsGO [elementID]);
		}

		lastObstaclePos = new Vector3 (Mathf.Abs (parentAll.position.x) + cameraWidth + 2f, obstacleHeight);

		//Si el obstaculo se colocara encima de un producto, no colocar nada
		if (Vector3.Distance(lastObstaclePos, lastProductPos) < proximityTolerance) {
			lastObstaclePos = Vector3.zero;
			return;
		}

		elementsGO [elementID] = Instantiate (obstaclePf, Vector3.zero, Quaternion.identity) as GameObject;
		elementsGO [elementID].transform.SetParent (parentAll);
		elementsGO [elementID].transform.localPosition = lastObstaclePos;
	}

	void AddNewProduct () {
		elementID = (elementID + 1 >= maxElementsOnScene) ? 0 : elementID + 1;

		if (elementsGO [elementID] != null) { //Destruir elemento correspondiente
			Destroy (elementsGO [elementID]);
		}

		lastProductPos = new Vector3 (Mathf.Abs (parentAll.position.x) + cameraWidth + 2f, productHeight [Random.Range (0, productHeight.Length)]);

		//Si el obstaculo se colocara encima de un producto, no colocar nada
		if (Vector3.Distance(lastObstaclePos, lastProductPos) < proximityTolerance) {
			lastProductPos = Vector3.zero;
			return;
		}

		elementsGO [elementID] = Instantiate (productPf, Vector3.zero, Quaternion.identity) as GameObject;
		elementsGO [elementID].transform.SetParent (parentAll);
		elementsGO [elementID].transform.localPosition = lastProductPos;

		elementsGO [elementID].GetComponent<EndlessProduct> ().SetupProduct ();
	}

	//Agrega nuevo piso
	void AddNewFloor () {
		elementID = (elementID + 1 >= maxElementsOnScene) ? 0 : elementID + 1;

		if (elementsGO [elementID] != null) { //Destruir elemento correspondiente
			Destroy (elementsGO [elementID]);
		}

		elementsGO [elementID] = Instantiate (floorPf, Vector3.zero, Quaternion.identity) as GameObject;
		elementsGO [elementID].transform.SetParent (parentAll);
		elementsGO [elementID].transform.localPosition = new Vector3 (lastFloorPos.x, floorHeight);

		floorCant += 1;

		//Posicion del siguiente piso
		lastFloorPos = new Vector3 (elementsGO [elementID].transform.localPosition.x + floorWidth, floorHeight);
	}

	public void OnClickPlay(){
		//Hide UIs
		introUI.SetActive (false);

		//Start Game
		InitGame ();

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickContinue(){
		//Hide UIs
		introUI.SetActive (false);
		endUI.SetActive (false);

		//Quitar referencia del singleton
		instance = null;

		//Cargar escena de decisiones
		SceneManager.LoadScene("(3) DecisionsV4");

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	public void UpdateUI(int ID) {
		//Incrementar contador de cajas y actualizar UI
		boxCounter [ID]++;
		boxCantText [ID].text = boxCounter [ID].ToString("d0");

		//Asignar nueva escala para la animacion
		boxCantText [ID].transform.localScale = rightNumberStartScale;
	}

	//Usado para saber si se esta en algun menu o en pausa
	public bool OnHold() {
		return onMenu || onPause;
	}

	public bool OnPause() {
		return onPause;
	}

	public bool OnMenu() {
		return onMenu;
	}

	//Retraso para cargar siguiente interfaz
	IEnumerator DelayOnWin() {
		//Animacion de Fade

		yield return new WaitForSeconds(delayOnWin);

		//Load new level
		//Application.LoadLevel ();
	}

	IEnumerator SlowUpdate(){
		float aux;
		while (true) {
			yield return new WaitForSeconds (slowUpdateDelta);	//Esperar tiempo

			if(!onPause) {
				//Mostrar tiempo restante
				aux = gameDurationTime - (Time.time - startTime - offsetGameDuration);
				aux = (aux < 0) ? 0f : aux;
				timeTextUI.text = aux.ToString("f1") + "s";

				//Termino el tiempo
				if(aux <= 0f) {
					OnWin();
					yield break;
				}
			}
		}
	}

	public void OnClickPauseContinue(){
		//Actualizar tiempo de juego && tiempo de spawn de la caja
		offsetTime = Time.time - timerAux;
		offsetGameDuration += offsetTime;
		//gameDurationTime += offsetTime;

		//Esconder UI Pausa
		pauseUI.SetActive(false);

		//Revert Pause
		onPause = false;

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickPauseRestartGame() {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Detener SlowUpdate
		if (slowCor != null) StopCoroutine (slowCor);

		//Inicializar texto
		timeTextUI.text = "0.0s";

		//Resetear contador de productos
		for(int i = 0; i < boxCounter.Length; i++) {
			boxCounter [i] = 0;
			boxCantText[i].text = "0";
		}

		//Esconder UI Pausa
		pauseUI.SetActive(false);

		//Revert Pause
		onPause = false;

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickPauseQuitMiniGame() {
		//Inicializar confirmacion
		confirmID = 1;
		confirmUI.SetActive (true);

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickPauseExitGame() {
		//Inicializar confirmacion
		confirmID = 2;
		confirmUI.SetActive (true);

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickConfirmYes(){
		confirmUI.SetActive (false);
		if (confirmID == 1) { //Salir del minijuego
			//Volcar data a GameController
			if(GameController.instance != null) 
				GameController.instance.SetProductCounter (boxCounter);

			//Quitar referencia del singleton
			instance = null;

			//***DO CLEANING HERE IF NECESARY

			//Cargar escena de decisiones
			SceneManager.LoadScene("(3) DecisionsV4");
		}
		else if(confirmID == 2) { //Salir del juego
			//Quitar referencia del singleton
			instance = null;

			//***DO CLEANING HERE IF NECESARY

			//Cargar menu principal
			SceneManager.LoadScene("(0) DemoMainMenuV4");
		}

		//Esconder UI Pausa
		pauseUI.SetActive (false);

		//Revert Pause
		onPause = false;

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickConfirmNo(){
		//Esconder y revertir confirmacion
		confirmID = -1;
		confirmUI.SetActive (false);

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}
}

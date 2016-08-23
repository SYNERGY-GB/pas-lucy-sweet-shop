using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TapProductController : MonoBehaviour {
	public static TapProductController instance;//Singleton

	public Vector3[] spawnPos;					//Posicion del spawn de los productos
	private bool[] spawnOcupied;				//Indica si la posicion esta ocupada por algun producto
	public GameObject spawnPf;					//Prefab que indica el spawn de los productos
	public Transform spawnPosParent;			//Padre de la zona de aparacion de productos

	public Text timeTextUI;						//Texto de tiempo UI
	private float startTime;					//Tiempo de inicio
	private float offsetTime;					//Tiempo en estado de pausa
	public float gameDurationTime = 80f;		//Duracion total del juego en segundos
    private float timerAux;                     //Auxiliar para calcular el tiempo en pausa
    private float offsetGameDuration;           //Offset de la duracion del juego en el menu pausa

	public float productSpawnInterval;			//Intervalo de tiempo entre la aparacion de los productos
	private float nextSpawnTime;				//Tiempo para hacer aparecer el siguiente prodcuto

	public float slowUpdateDelta = 0.2f;		//Ratio ejecucion en segundos de SlowUpdate

	private Ray rayShoot;						//Rayo lanzado desde el mouse hacia la caja
	private RaycastHit2D rayInfo;				//Informacion del rayo lanzado

	public GameObject productPf;				//Prefab del producto a mostrar

	public Sprite[] boxSpritesArray;			//Referencia de las imagenes para las cajas

	public Text[] boxCantText;					//Texto UI, Arreglo de todos los textos de cajas de la interfaz derecha
	private int[] boxCounter;					//Contador de cuantas cajas el jugador a recolectado

	private bool onPause;						//Flag: Indica si el juego esta en pausa
	private bool onMenu;						//Internal Flag / indica si el juego esta mostrando algun menu
	private bool onWin;							//Internal Flag / indica si se termino el juego

	public GameObject introUI;					//UI de inicio
	public GameObject endUI;					//UI de finalizacion del juego
	public GameObject pauseUI;					//UI de minijuego pausado

    public GameObject confirmUI;                //UI de confirmar
    private int confirmID;                      //ID auxiliar de cual interfaz esta en estado de confirmar

    //Tamaño inicial de la animacion de los numeros de la derecha
    public Vector3 rightNumberStartScale = new Vector3(1.5f, 1.5f, 1f);
    public float rightNumberSpeed = 10f;		//Velocidad de animacion de numeros de la derecha

    private Coroutine slowCor;                  //Referencia a Slow Update

	private GameObject auxGO;					//Variable auxiliar GameObject
	private int auxInt;							//Variable auxiliar Int
	private int spawnID;						//Auxiliar: dice cual fue el ID del ultimo elemento en pantalla

	void Awake() {
		//Logica del singleton
		if (instance == null) {
			instance = this;
		} 
		else if(this != instance) {
			Destroy(this.gameObject);
		}

		//Inicializar contadores
		spawnOcupied = new bool[spawnPos.Length];
		boxCounter = new int[boxCantText.Length];

        //Inicializar textos de las UI
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].text = "0";
		}
		timeTextUI.text = "00:00";
	}

	// Use this for initialization
	void Start () {
		//InitGame ();//Dibuja elementos de la escena

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
		//Debug.Log ("Time: " + Time.time);
		if (onMenu) return;

        if (Input.GetKeyDown (KeyCode.Escape)) {
			onPause = !onPause;

			if (onPause) {
				timerAux = Time.time;

                //Pausar productos
                PauseAllProducts();

				//Mostrar UI Pausa
				pauseUI.SetActive(true);
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				nextSpawnTime += offsetTime;
				offsetGameDuration += offsetTime;
				//gameDurationTime += offsetTime;

                //Reaunudar productos
                ResumeAllProducts();

				//Esconder UI Pausa
				pauseUI.SetActive(false);
			}
		}

		if (Time.time > nextSpawnTime && !onPause) {//Tiempo de spawn del siguiente producto
			nextSpawnTime = Time.time + productSpawnInterval;

			//Instanciar nuevo producto
			SpawnBox();
		}

        //Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}

		if (Input.GetMouseButtonDown (0) && !onPause) {
			//Definicion del rayo lanzado desde la camara hacia la caja
			rayShoot = Camera.main.ScreenPointToRay(Input.mousePosition);

			//Se lanza un rayo desde el origen y la direcion especificada
			rayInfo = Physics2D.Raycast(rayShoot.origin, rayShoot.direction, 1000f);
			if(rayInfo.collider != null) { //Se lanza el rayo
				if(rayInfo.collider.CompareTag("TapProductBox")) {
					//Obtener el ID del producto tocado
					auxInt = rayInfo.collider.gameObject.GetComponent<TapProductBox>().GetID();

					//Actualizar contadores
					UpdateUI(auxInt);

					//Posicion desocupada
					spawnOcupied[rayInfo.collider.gameObject.GetComponent<TapProductBox>().GetPositionID()] = false;

					//Destruir producto
					Destroy(rayInfo.collider.gameObject);

                    //Reproducir sonido
                    if (MusicController.instance != null) {
                        MusicController.instance.PlayMinigameSound(0);
                    }
				}
			}
		}
	}

	void InitGame() {
		//Set timer
		//Asignar tiempo inicial y tiempo para mostrar el siguiente producto
		startTime = Time.time;
        offsetGameDuration = offsetTime = 0f;
		nextSpawnTime = startTime + productSpawnInterval;
		//offset = 0;
		
		//Set focus 
		onMenu = onPause = false;
		introUI.SetActive (false);
		endUI.SetActive (false);
		pauseUI.SetActive (false);
		
		//Start time
		slowCor = StartCoroutine (SlowUpdate ());

		//Init spawn zones
		for (int i = 0; i < spawnPos.Length; i++) {
			auxGO = Instantiate(spawnPf, spawnPos[i], Quaternion.identity) as GameObject;
			auxGO.transform.parent = spawnPosParent;
            //auxGO.transform.localPosition = spawnPos[i];
		}

        //Abrir todos los espacios disponibles para el spawn de productos
        for (int i = 0; i < spawnOcupied.Length; i++) {
            spawnOcupied[i] = false;
        }

		//Init Text UI
		//Obtener valores del GameController
		boxCounter = new int[boxSpritesArray.Length];
		for(int i = 0; i < boxCounter.Length; i++) {
			boxCounter[i] = 0;
		}

		//Asignar valores predeterminados
		for(int i = 0; i < boxCounter.Length; i++) {
			boxCantText[i].text = boxCounter[i].ToString("d0");
		}
	}

	void SpawnBox(){
		Vector3 rndV3 = RandomPosition ();
		if (rndV3 == -Vector3.one) //Position not found
			return;

		auxGO = Instantiate (productPf, rndV3, Quaternion.identity) as GameObject;
		auxGO.gameObject.GetComponent<TapProductBox>().SetPositionID(spawnID);
	}

	Vector3 RandomPosition() {
		int rnd = Random.Range (0, spawnPos.Length); //Obtiene una posicion aleatoria
		//Verificar si la posicion no esta ocupada si lo esta buscar linealmente una posicion vacia
		if (!spawnOcupied [rnd]) {
			spawnOcupied[rnd] = true;
			spawnID = rnd;
			return spawnPos[rnd];
		}

		rnd = Random.Range (0, spawnPos.Length); //Obtiene una segunda posicion aleatoria
		//Verificar si la posicion no esta ocupada si lo esta buscar linealmente una posicion vacia
		if (!spawnOcupied [rnd]) {
			spawnOcupied[rnd] = true;
			spawnID = rnd;
			return spawnPos[rnd];
		}

		//Si encuentra nueva posicion la retorna
		for (int i = 0; i < spawnPos.Length; i++) {
			if(!spawnOcupied[i]){
				spawnOcupied[i] = true;
				spawnID = i;
				return spawnPos[i];
			}
		}

		//Returns this if no position is found
		return -Vector3.one;
	}

	//Retorna el sprite correspondiente al ID
	public Sprite GetSprite(int ID) {
		return boxSpritesArray [ID];
	}

	public void UpdateUI(int ID) {
		//Incrementar contador de cajas y actualizar UI
		boxCounter [ID]++;
		boxCantText [ID].text = boxCounter [ID].ToString("d0");

        //Asignar nueva escala para la animacion
        boxCantText[ID].transform.localScale = rightNumberStartScale;
	}

	//Asignar espacio libre
	public void SetFree(int ID){
		spawnOcupied [ID] = false;
	}

	public void OnWin(){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;
		
		//Volcar data a GameController
		if(GameController.instance != null) 
			GameController.instance.SetProductCounter (boxCounter);
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
		
		//Cargar escena de decisiones
		SceneManager.LoadScene ("(3) DecisionsV4");

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

    //Pausa todos los productos en la escena
    void PauseAllProducts() {
        //int ount = 0;
        foreach(GameObject A in GameObject.FindGameObjectsWithTag("TapProductBox")){
            //Debug.Log(A.transform.position);
            A.GetComponent<TapProductBox>().Pause();
        }
    }

    //Reunuda todos los objetos en la escena
    void ResumeAllProducts() {
        foreach(GameObject A in GameObject.FindGameObjectsWithTag("TapProductBox")){
            A.GetComponent<TapProductBox>().Resume();
        }
    }

    //Destruye todos los productos en la escena
    void DestroyAllProducts() {
        foreach(GameObject A in GameObject.FindGameObjectsWithTag("TapProductBox")){
            Destroy(A);
        }
    }

	IEnumerator SlowUpdate(){
		float aux, aux2;
		while (true) {
			yield return new WaitForSeconds (slowUpdateDelta);	//Esperar tiempo
			
			if (!onPause && !onMenu) {
                //Mostrar tiempo restante
                aux = gameDurationTime - (Time.time - startTime - offsetGameDuration);
                aux = (aux < 0) ? 0f : aux;
                aux2 = ((aux - Mathf.Floor(aux)) * 100f);
                timeTextUI.text = aux.ToString("00") + ":" + aux2.ToString("00");
				
				//Termino el tiempo
				if (aux <= 0f) {
					OnWin ();
					Debug.Log("Finish");
					yield break;
				}
			}
		}
	}

    public void OnClickPauseContinue(){
        //Actualizar tiempo de juego && tiempo de spawn de la caja
        offsetTime = Time.time - timerAux;
        nextSpawnTime += offsetTime;
        offsetGameDuration += offsetTime;
        //gameDurationTime += offsetTime;

        //Reaunudar productos
        ResumeAllProducts();

        //Esconder UI Pausa
        pauseUI.SetActive(false);

        //Revert Pause
        onPause = false;

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	public void OnClickPauseRestartGame() {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Limpiar escena
        DestroyAllProducts();

		//Detener SlowUpdate
		if (slowCor != null) StopCoroutine (slowCor);
		
		//Inicializar texto
		timeTextUI.text = "00:00";

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

			//Cargar escena de decisiones
            SceneManager.LoadScene("(3) DecisionsV4");
		}
		else if(confirmID == 2) { //Salir del juego
			//Quitar referencia del singleton
			instance = null;

			//Cargar menu principal
            SceneManager.LoadScene("(0) DemoMainMenuV5");
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

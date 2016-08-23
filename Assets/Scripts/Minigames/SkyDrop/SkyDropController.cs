using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkyDropController : MonoBehaviour {
	public static SkyDropController instance;	//Singleton

	public Text[] boxCantText;					//Texto UI, Arreglo de todos los textos de cajas de la interfaz derecha
	private int[] boxCounter;					//Contador de cuantas cajas el jugador a recolectado

	public Text timeTextUI;						//Texto de tiempo UI
	private float startTime;					//Tiempo de inicio
	private float offsetTime;					//Tiempo en estado de pausa
	private float offsetGameDuration;			//Tiempo acumulado en estado de pausa
	private float nextBoxTime;					//Tiempo para hacer spawn de la siguiente caja
	public float fallDelayTime = 3f;			//Tiempo de spawn entre cajas
	public float gameDurationTime = 80f;		//Duracion total del juego en segundos

	public float delayOnWin = 4f;				//Delay para ir a la siguiente escena

	public GameObject introUI;					//UI de inicio
	public GameObject endUI;					//UI de finalizacion del juego
	public GameObject pauseUI;					//UI de minijuego pausado

	public float topPositionBox = 7f;			//Altura del spawn de la caja
	public float cornerPosBox = 6f;				//Coordenada que limita el ancho de la zona de spawn de la caja
	public GameObject boxPf;					//Prefab de la caja

	public Sprite[] boxSpritesArray;			//Referencia de las imagenes para las cajas

	public float slowUpdateDelta = 0.2f;		//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;					//Referencia interna de Coroutine

	public GameObject confirmUI;				//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;						//Identifica a quien le pertenece el mensaje de confirmacion
	
	//Rango de movimiento del tiempo limite
	public Vector3 timeLimitRangePosition = new Vector3(0.3f, 0.4f, 1f);
	public float timeLimitAnimSpeed = 20f;		//Velocidad de la animacion de tiempo limite
	private bool timeLimitAnim;					//Flag: Indica si es hora de animar el texto de tiempo limite
	private Vector3 timeLimitPivot;				//Punto por el cual se mueve el texto de tiempo limite
	private Vector3 timeLimitTargetPos;			//Posicion objetivo a mover el texto tiempo limite

	private bool onPause;						//Flag: Indica si el juego esta en pausa
	private bool onMenu;						//Flag: Indica si el juego esta mostrando algun menu

	//Tamaño inicial de la animacion de los numeros de la derecha
	public Vector3 rightNumberStartScale = new Vector3(1.5f, 1.5f, 1f);
	public float rightNumberSpeed = 10f;		//Velocidad de animacion de numeros de la derecha

	private GameObject auxGO;					//GameObject auxiliar
	private float timerAux;						//Timer(float) auxiliar

    public SkyDropPlayer skyDropPlayer;         //Referencia al script del jugador

	void Awake(){
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
	}

	// Use this for initialization
	void Start () {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Inicializar pivote tiempo limite
		timeLimitPivot = timeTextUI.transform.localPosition;

		if (GameController.instance != null) {
			//Satisfaccion del cliente
			//gameDurationTime *= GameController.instance.GetSatisfaction ();
		}

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

                //Pause player
                skyDropPlayer.Pause();

				//Mostrar UI Pausa
				pauseUI.SetActive(true);
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				nextBoxTime += offsetTime;
				offsetGameDuration += offsetTime;
				//gameDurationTime += offsetTime;

                //Resume player
                skyDropPlayer.Resume();

				//Esconder UI Pausa
				pauseUI.SetActive(false);
			}
		}

		if (nextBoxTime < Time.time && !onPause) { 		//Spawn caja en la parte de arriba
			nextBoxTime = Time.time + fallDelayTime;	//Asignar nuevo tiempo para el siguiente spawn

			SpawnBox(); //Agregar nueva caja a la escena
		}

		//Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}

		//Animacion del tiempo limite
		if (timeLimitAnim) {
			//Debug.Log ("TimeText: " + Vector3.Distance(timeTextUI.transform.position, timeLimitTargetPos));
			timeTextUI.transform.localPosition = Vector3.MoveTowards(timeTextUI.transform.localPosition, timeLimitTargetPos, timeLimitAnimSpeed * Time.deltaTime);

			if(Vector3.Distance(timeTextUI.transform.localPosition, timeLimitTargetPos) < Mathf.Epsilon * 10f) { //Verificar si llego a la posicion objetivo
				SetTargetTimeLimit();
			}
		}
	}

	void SetTargetTimeLimit(){
		timeLimitTargetPos = new Vector3 (timeLimitPivot.x + Random.Range (-timeLimitRangePosition.x, timeLimitRangePosition.x), 
		                                  timeLimitPivot.y + Random.Range (-timeLimitRangePosition.y, timeLimitRangePosition.y),
		                                  timeLimitPivot.z);
	}

	public void OnWin(){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;

		//Volcar data a GameController
		if(GameController.instance != null) 
			GameController.instance.SetProductCounter (boxCounter);
	}

	//Inicializa el minijuego
	void InitGame(){
		//Set timer
		startTime = Time.time;
		nextBoxTime = Time.time + fallDelayTime;

		//Inicializar variables
		timeTextUI.text = "00:00";
		offsetTime = offsetGameDuration = 0f;

		//Set focus 
		onMenu = onPause = false;

		//Start time
		slowCor = StartCoroutine (SlowUpdate ());
	}

	public void OnClickPlay(){
		//Hide UIs
		introUI.SetActive (false);

		//Start Game
		InitGame ();

		//Sound
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

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	//Crea nueva caja
	void SpawnBox(){
		auxGO = Instantiate (boxPf, new Vector3 (Random.Range (-cornerPosBox, cornerPosBox), topPositionBox, 0f), Quaternion.identity) as GameObject;
	}

	public void UpdateUI(int ID) {
		//Incrementar contador de cajas y actualizar UI
		boxCounter [ID]++;
		boxCantText [ID].text = boxCounter [ID].ToString("d0");

		//Asignar nueva escala para la animacion
		boxCantText [ID].transform.localScale = rightNumberStartScale;
	}

	public Sprite GetSprite(int ID) {
		return boxSpritesArray [ID];
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
		float aux, aux2;
		while (true) {
			yield return new WaitForSeconds (slowUpdateDelta);	//Esperar tiempo

			if(!onPause) {
				//Mostrar tiempo restante
				aux = gameDurationTime - (Time.time - startTime - offsetGameDuration);
				aux = (aux < 0) ? 0f : aux;
				aux2 = ((aux - Mathf.Floor(aux)) * 100f);
				timeTextUI.text = aux.ToString ("00") + ":" + aux2.ToString("00");

				//Activar animacion de tiempo limite
				if(aux < 6f && !timeLimitAnim) {
					SetTargetTimeLimit();
					timeLimitAnim = true;
				}

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
		nextBoxTime += offsetTime;
		offsetGameDuration += offsetTime;
		//gameDurationTime += offsetTime;

        //Resume player
        skyDropPlayer.Resume();
		
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

		//Restaurar estado del jugador
		skyDropPlayer.Resume ();

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

			//Eliminar cajas restantes
			foreach(GameObject A in GameObject.FindGameObjectsWithTag("SkyBoxItem")){
				Destroy(A);
			}

			//Cargar escena de decisiones
            SceneManager.LoadScene("(3) DecisionsV4");
		}
		else if(confirmID == 2) { //Salir del juego
			//Quitar referencia del singleton
			instance = null;

			//Eliminar cajas restantes
			foreach(GameObject A in GameObject.FindGameObjectsWithTag("SkyBoxItem")){
				Destroy(A);
			}

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

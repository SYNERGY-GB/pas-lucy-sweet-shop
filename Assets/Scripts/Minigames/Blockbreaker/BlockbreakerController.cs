using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BlockbreakerController : MonoBehaviour {
	public static BlockbreakerController instance;		//Singleton

	public Text[] boxCantText;							//Texto UI, Arreglo de todos los textos de cajas de la interfaz derecha
	private int[] boxCounter;							//Contador de cuantas cajas el jugador a recolectado

	public Text timeTextUI;								//Texto de tiempo UI
	private float startTime;							//Tiempo de inicio
	private float offsetTime;							//Tiempo en estado de pausa
	public float gameDurationTime = 80f;				//Duracion total del juego en segundos

	public float delayOnWin = 4f;						//Delay para ir a la siguiente escena

	public GameObject introUI;							//UI de inicio
	public GameObject endUI;							//UI de finalizacion del juego
	public GameObject pauseUI;							//UI de minijuego pausado

	public Sprite[] boxSpritesArray;					//Referencia de las imagenes para las cajas

	public float slowUpdateDelta = 0.2f;				//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;							//Referencia interna de Coroutine

	public GameObject confirmUI;						//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;								//Identifica a quien le pertenece el mensaje de confirmacion

	private bool onPause;								//Flag: Indica si el juego esta en pausa
	private bool onMenu;								//Flag: Indica si el juego esta mostrando algun menu

	//Tamaño inicial de la animacion de los numeros de la derecha
	public Vector3 rightNumberStartScale = new Vector3(1.5f, 1.5f, 1f);
	public float rightNumberSpeed = 10f;				//Velocidad de animacion de numeros de la derecha

	private GameObject auxGO;							//GameObject auxiliar
	private float timerAux;								//Timer(float) auxiliar

	public float ballForce;								//Fuerza inicial de la pelota

	//Referencias de la pelota y el jugador
	public BlockbreakerBall ballScript;
	public BlockbreakerProduct[] productScript;
	public BlockbreakerPlayer playerScript;

	void Awake () {
		//Logica del singleton
		if (instance == null) {
			instance = this;

			//Inicializar el contador interno de cajas recolectadas
			boxCounter = new int[boxCantText.Length];
		}
		else if(this != instance) {
			Destroy(this.gameObject);
		}

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

				//Mostrar UI Pausa
				pauseUI.SetActive(true);

				//Pause all product blocks
				for (int i = 0; i < productScript.Length; i++) {
					productScript [i].Stop ();
				}

				//Pause ball
				ballScript.Stop();

				//Pause player
				playerScript.Stop();
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				gameDurationTime += offsetTime;

				//Esconder UI Pausa
				pauseUI.SetActive(false);

				//Resume all product blocks
				for (int i = 0; i < productScript.Length; i++) {
					productScript [i].Resume ();
				}

				//Resume ball
				ballScript.Resume();

				//Resume player
				playerScript.Resume();
			}
		}

		//Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}
	}

	public void OnWin(){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;

		//Pause all product blocks
		for (int i = 0; i < productScript.Length; i++) {
			productScript [i].Stop ();
		}

		//Pause ball
		ballScript.Stop();

		//Pause player
		playerScript.Stop();

		//Volcar data a GameController
		if(GameController.instance != null) 
			GameController.instance.SetProductCounter (boxCounter);
	}

	//Inicializa el minijuego
	void InitGame(){
		//Set timer
		startTime = Time.time;	

		//Set focus 
		onMenu = onPause = false;

		//Start time
		slowCor = StartCoroutine (SlowUpdate ());

		//Agregar fuerza de la pelota
		ballScript.AddForce (ballForce);

		//Resume all product blocks
		for (int i = 0; i < productScript.Length; i++) {
			productScript [i].Resume ();
		}

		//Resume ball
		ballScript.Resume();

		//Resume player
		playerScript.Resume();
	}

	public void OnClickPlay(){
		//Hide UIs
		introUI.SetActive (false);

		//Start Game
		InitGame ();

		//Sonido
		if(MusicController.instance != null) {
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
		yield return new WaitForSeconds(delayOnWin);
	}

	IEnumerator SlowUpdate(){
		float aux, aux2;
		while (true) {
			yield return new WaitForSeconds (slowUpdateDelta);	//Esperar tiempo

			if(!onPause) {
				//Mostrar tiempo restante
				aux = gameDurationTime - (Time.time - startTime);
				aux = (aux < 0) ? 0f : aux;
				aux2 = ((aux - Mathf.Floor(aux)) * 100f);
				timeTextUI.text = aux.ToString ("00") + ":" + aux2.ToString("00");

				//Termino el tiempo
				if(aux <= 0f) {
					OnWin();
					yield break;
				}
			}
		}
	}

	public void OnClickPauseContinue(){
		//Actualizar tiempo de juego & tiempo de spawn de la caja
		offsetTime = Time.time - timerAux;
		gameDurationTime += offsetTime;

		//Esconder UI Pausa
		pauseUI.SetActive(false);

		//Revert Pause
		onPause = false;

		//Resume all product blocks
		for (int i = 0; i < productScript.Length; i++) {
			productScript [i].Resume ();
		}

		//Resume ball
		ballScript.Resume();

		//Resume player
		playerScript.Resume();

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

		//Restart all objects
		ballScript.Restart ();
		playerScript.Restart ();
		for (int i = 0; i < productScript.Length; i++) {
			productScript [i].Restart ();
		}

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

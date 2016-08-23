using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PinballController : MonoBehaviour {
	public static PinballController instance;	//Singleton
	
	public Text[] boxCantText;					//Texto UI, Arreglo de todos los textos de cajas de la interfaz derecha
	private int[] boxCounter;					//Contador de cuantas cajas el jugador a recolectado
	
	public Text timeTextUI;						//Texto de tiempo UI
	private float startTime;					//Tiempo de inicio
	private float offsetTime;					//Tiempo en estado de pausa
	public float gameDurationTime = 80f;		//Duracion total del juego en segundos
	
	public float delayOnWin = 4f;				//Delay para ir a la siguiente escena
	
	public GameObject introUI;					//UI de inicio
	public GameObject endUI;					//UI de finalizacion del juego
	public GameObject pauseUI;					//UI de minijuego pausado
	
	public Sprite[] boxSpritesArray;			//Referencia de las imagenes para las cajas
	
	public float slowUpdateDelta = 0.2f;		//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;					//Referencia interna de Coroutine
	
	public GameObject confirmUI;				//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;						//Identifica a quien le pertenece el mensaje de confirmacion
	
	private bool onPause;						//Flag: Indica si el juego esta en pausa
	private bool onMenu;						//Flag: Indica si el juego esta mostrando algun menu
	
	private GameObject auxGO;					//GameObject auxiliar
	private float timerAux;						//Timer(float) auxiliar

	public Transform playerT;					//Referencia del transform del producto
	public Vector3 playerStartPos;				//Posicion de inicio del producto

    public Text endUIText;						//Texto de la UI de fin de minijuego

	public PinballPlayer pinballPlayer;			//Referencia interna del controlador de productos

	//Tamaño inicial de la animacion de los numeros de la derecha
	public Vector3 rightNumberStartScale = new Vector3(1.5f, 1.5f, 1f);
	public float rightNumberSpeed = 10f;		//Velocidad de animacion de numeros de la derecha

    public GameObject gameUIGO;                 //Referencia interna a los elementos de interfaz del juego pinball
	
	void Awake(){
		//Logica del singleton
		if (instance == null) {
			instance = this;
		}
		else if(this != instance) {
			Destroy(this.gameObject);
		}
	}
	
	// Use this for initialization
	void Start () {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Obtener valores del GameController
		if (GameController.instance != null) {
			boxCounter = GameController.instance.GetInventory ();

			//Aplicar satisfaccion del cliente
			gameDurationTime *= GameController.instance.GetSatisfaction ();
		}
		else {
            boxCounter = new int[boxSpritesArray.Length];
			for(int i = 0; i < boxCounter.Length; i++) {
                boxCounter[i] = 1;
			}
		}

        //Inicializar textos de las UI
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].text = boxCounter[i].ToString("d0");
		}

		//Inicializar texto
		timeTextUI.text = "00:00";

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

                //Detener pelota
                pinballPlayer.Stop();
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				gameDurationTime += offsetTime;
				
				//Esconder UI Pausa
				pauseUI.SetActive(false);

                //Detener pelota
                pinballPlayer.Resume();
			}
		}

		//Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}
	}
	
	public void OnWin(bool isInvEmpty){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;

        if (isInvEmpty) {
			endUIText.text = "Inventory is empty.";
		}

        //Detener pelota
        pinballPlayer.Stop();
		
        //Volcar data a GameController
		if (GameController.instance != null) {
            GameController.instance.SetProductCounter(boxCounter); //Update sold
		}
	}
	
	//Inicializa el minijuego
	void InitGame(){
		//Set timer
		startTime = Time.time;
		
		//Set focus 
		onMenu = onPause = false;
		
		//Start time
		slowCor = StartCoroutine (SlowUpdate ());

		//Coloca al jugador en la posicion inicial
		RestartPlayerPos ();

		//Inicializar escena
		pinballPlayer.InitScene ();
	}

	public void RestartPlayerPos() {
		//Colocar producto en posicion
		playerT.position = playerStartPos;
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
        SceneManager.LoadScene("(5) SellReportV1");

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}
	
	public void UpdateUI(int ID) {
		//Incrementar contador de cajas y actualizar UI
		boxCounter [ID]--;
		boxCantText [ID].text = boxCounter [ID].ToString("d0");

		//Asignar nueva escala para la animacion
		boxCantText [ID].transform.localScale = rightNumberStartScale;

        int _count = 0;
        for (int i = 0; i < boxCounter.Length; i++) {
            _count += boxCounter[i];
        }

        //Si counter < 1 no hay objetos que vender
        if (_count < 1) {
            OnWin(true);
        }
	}

    public Sprite GetSprite(int ID) {
		return boxSpritesArray [ID];
	}

    public int[] GetBoxCounter() {
        return boxCounter;
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
				aux = gameDurationTime - (Time.time - startTime);
				aux = (aux < 0) ? 0f : aux;
				aux2 = ((aux - Mathf.Floor(aux)) * 100f);
				timeTextUI.text = aux.ToString ("00") + ":" + aux2.ToString("00");
				
				//Termino el tiempo
				if(aux <= 0f) {
					OnWin(false);
					yield break;
				}
			}
		}
	}
	
	public void OnClickPauseContinue(){
		//Actualizar tiempo de juego && tiempo de spawn de la caja
		offsetTime = Time.time - timerAux;
		gameDurationTime += offsetTime;
		
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
		timeTextUI.text = "00:00";
		
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
		int[] newInv;

		confirmUI.SetActive (false);
		if (confirmID == 1) { //Salir del minijuego
			//Volcar data a GameController
			if(GameController.instance != null)  {
                GameController.instance.SetProductCounter(boxCounter); //Update sold
            }

			//Quitar referencia del singleton
			instance = null;

			//Cargar escena de decisiones
            SceneManager.LoadScene("(5) SellReportV1");
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

	//Resetea el producto actual
	public void OnClickRestartProduct(){
		pinballPlayer.SetupNextProduct (false);

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

    public void setActiveGameUI(bool status) {
        gameUIGO.SetActive(status);
    }
}

﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MinigameController : MonoBehaviour {
	public static MinigameController instance;	//Singleton

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

	public Sprite[] boxSpritesArray;			//Referencia de las imagenes para las cajas

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
		timeTextUI.text = "0.0s";
	}

	// Use this for initialization
	void Start () {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;
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
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				offsetGameDuration += offsetTime;

				//Esconder UI Pausa
				pauseUI.SetActive(false);
			}
		}

		//Animacion de numeros de la interfaz derecha
		for(int i = 0; i < boxCantText.Length; i++) {
			boxCantText[i].transform.localScale = Vector3.Lerp(boxCantText[i].transform.localScale, Vector3.one, rightNumberSpeed * Time.deltaTime);
		}
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
		//Set timer
		startTime = Time.time;

		//Inicializar variables
		timeTextUI.text = "0.0s";
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
	}

	public void OnClickContinue(){
		//Hide UIs
		introUI.SetActive (false);
		endUI.SetActive (false);

		//Quitar referencia del singleton
		instance = null;

		//Cargar escena de decisiones
		SceneManager.LoadScene("(3) DecisionsV4");
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
	}

	public void OnClickPauseQuitMiniGame() {
		//Inicializar confirmacion
		confirmID = 1;
		confirmUI.SetActive (true);
	}

	public void OnClickPauseExitGame() {
		//Inicializar confirmacion
		confirmID = 2;
		confirmUI.SetActive (true);
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
	}

	public void OnClickConfirmNo(){
		//Esconder y revertir confirmacion
		confirmID = -1;
		confirmUI.SetActive (false);
	}
}

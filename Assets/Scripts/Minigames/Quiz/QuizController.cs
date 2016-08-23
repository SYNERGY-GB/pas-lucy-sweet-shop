using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QuizController : MonoBehaviour {
	//Start Template
	public static QuizController instance;		//Singleton
	
	public Text[] productCantText;				//Texto UI, Arreglo de todos los textos de los productos de la interfaz derecha
	private int[] productCounter;				//Contador de cuantos productos el jugador ha vendido
	
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

	private int productType;					//Tipo de producto

	public Text endUIText;						//Texto de la UI de fin de minijueg
	//End template

	private float[] currentPrices;				//Referencia interna de los precios

	public Text questionText;					//Texto UI de preguntas
	public Text[] answerText;					//Arreglo de Texto UI de respuestas

	//0 - Plus / 1 - Minus / 2 - Mult / 3 - Question
	public GameObject[] mathSymbolPf;			//Arreglo de simbolos matematicos
	//0 - Cookie / 1 - Cake / 2 - Chocolate / 3 - Cupcake
	public GameObject[] sweetArrayPf;			//Arreglo de dulces

	private int questionAsked = 0;				//Preguntas hechas
	private int questionCorrect;				//Preguntas correctas

	public int sweetsMax = 3;					//Cantidad de sweets maximo en una pregunta
	private float currentAnswer;				//Contiene la respuesta de la pregunta actual
	private int currentCorrect;					//Contiene el ID del boton de la respuesta correcta

	public Transform quizObjectParent;			//Padre de los elementos del quiz
	public float offsetX, offsetY;				//Offset de la ubicacion de los elementos de la pregunta
	private List<GameObject> goList;			//Lista interna de todos los elementos de la pregunta

	public Vector3 endHelpUIPos;				//Posicion final de la interfaz de ayuda
	public Vector3 startHelpUIPos;				//Posicion inicial de la interfaz de ayuda
	public float helpUIHideSpeed = 12f;			//Velocidad para esconder la interfaz de ayuda
	private bool helpIsHiding;					//Indica si la ayuda se esta escondiendo
	public GameObject helpUI;					//Interfaz de ayuda
	public Text[] priceHelpText;				//Texto UI de precio de objetos

	void Awake(){
		//Logica del singleton
		if (instance == null) {
			instance = this;
		}
		else if(this != instance) {
			Destroy(this.gameObject);
		}

		//Inicializar lista de gameObjects
		goList = new List<GameObject> ();
	}

	// Use this for initialization
	void Start () {
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;
		
		//Inicializar texto
		timeTextUI.text = "00:00";

		//Cantidad de preguntas
		questionCorrect = 0; questionAsked = 0;

		//Actualizar UI
		questionText.text = "Question# " + questionAsked.ToString("d0");

		//Obtener precios
		if (GameController.instance != null)
			currentPrices = GameController.instance.GetSellPriceArray ();
		else {
			currentPrices = new float[4];
			for(int i = 0; i < 4; i++){
				currentPrices[i] = i + 1f;
			}
		}

		//Obtener valores del GameController
		if (GameController.instance != null) {
			productCounter = GameController.instance.GetInventory ();
		}
		else {
			productCounter = new int[boxSpritesArray.Length];
			for(int i = 0; i < productCounter.Length; i++) {
				productCounter[i] = 1;
			}
		}

		//Setup Right UI
		for (int i = 0; i < productCantText.Length; i++) {
			productCantText[i].text = "x" + productCounter[i].ToString("d0");
		}

		//Setup help menu
		startHelpUIPos = helpUI.transform.position;
		endHelpUIPos = new Vector3 (helpUI.transform.position.x, helpUI.transform.position.y + 150f, helpUI.transform.position.z);
		helpIsHiding = false;

		for (int i = 0; i < priceHelpText.Length; i++) {
			priceHelpText [i].text = currentPrices [i].ToString ("f2");
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
				
				//Mostrar UI Pausa
				pauseUI.SetActive(true);
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				gameDurationTime += offsetTime;
				
				//Esconder UI Pausa
				pauseUI.SetActive(false);
			}
		}

		if (helpIsHiding) {
			helpUI.transform.position = Vector3.MoveTowards (helpUI.transform.position, endHelpUIPos, helpUIHideSpeed * Time.deltaTime);
		} 
		else {
			helpUI.transform.position = Vector3.MoveTowards (helpUI.transform.position, startHelpUIPos, helpUIHideSpeed * Time.deltaTime);
		}
	}

	public void OnWin(bool isInvEmpty){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;
		
		if (isInvEmpty) {
			endUIText.text = "Inventory is empty.";
		}
		
		//Volcar data a GameController
		if (GameController.instance != null) {
			//Obtener inventario viejo
			int[] newProduct = GameController.instance.GetInventory ();
			
			//Actualizar inventario
			GameController.instance.SetInventory (productCounter);
			
			//Guardar productos vendidos
			for (int i = 0; i < newProduct.Length; i++) {
				newProduct[i] -= productCounter[i];
			}
			GameController.instance.SetProductCounter (newProduct);
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

		//Colocar pregunta
		InitQuestion ();
	}

	//Inicializa la pregunta
	void InitQuestion(){
		int[] sweetID, mathID;
		float answer, aux = 0;
		string[] formula = new string[sweetsMax * 2 - 1];
		int i, j, k = 0; int correctID;

		//Preguntas hechas
		questionAsked++;

		//Actualizar UI
		questionText.text = "Question# " + questionAsked.ToString("d0");

		if (goList != null) { //Borrar elementos de la interfaz y logica
			for(i = 0; i < goList.Count; i++) {
				Destroy(goList[i]);
			}
			goList.Clear();
	    }

		//Cantidad total de simbolos
		//totalSymbol = new bool[sweetsMax * 2 - 1];

		//Asignar pregunta
		//Asignar IDs
		sweetID = new int[sweetsMax];
		mathID = new int[sweetsMax - 1];

		//Asignar dulce & simbolo matematico
		sweetID[0] = Random.Range(0, currentPrices.Length);
		formula[k++] = currentPrices[sweetID[0]].ToString("f2");
		for (i = 1, j = 0; i < sweetID.Length; i++, j++) {
			//Asignar simbolo
			mathID[j] = Random.Range(0, mathSymbolPf.Length - 2); //Quitamos el simbolo de pregunta e igual
			if(mathID[j] == 0) 		formula[k++] = "+";
			else if(mathID[j] == 1) formula[k++] = "-";
			else if(mathID[j] == 2) formula[k++] = "x";

			//Asignar dulce
			sweetID[i] = Random.Range(0, currentPrices.Length);
			formula[k++] = currentPrices[sweetID[i]].ToString("f2");
		}
		//Debug.Log ("Formula: " + formula[0] + formula[1] + formula[2] + formula[3] + formula[4]);

		//Resolver multiplicaciones
		for (i = 0; i < formula.Length; i++) {
			if(formula[i] == "x") { //Si se encontro una multiplicacion
				//Buscar numero anterior
				for(j = i - 1; j > -1; j--) {
					if(formula[j] != "+" && formula[j] != "-" && formula[j] != "x" && formula[j] != "@") {
						break;
					}
				}

				//Buscar numero posterior
				for(k = i + 1; k < formula.Length; k++) {
					if(formula[k] != "+" && formula[k] != "-" && formula[k] != "x" && formula[k] != "@") {
						break;
					}
				}

				//Almacenar multiplicacion
				aux = float.Parse(formula[j]) * float.Parse(formula[k]);

				//Modificar formula
				formula[i] = aux.ToString("f2");
				formula[j] = formula[k] = "@";
			}
		}

		//Resolver sumas y restas
		for (i = 0; i < formula.Length; i++) {
			if(formula[i] == "+" || formula[i] == "-") {
				//Buscar numero anterior
				for(j = i - 1; j > -1; j--) {
					if(formula[j] != "+" && formula[j] != "-" && formula[j] != "x" && formula[j] != "@") {
						break;
					}
				}
				
				//Buscar numero posterior
				for(k = i + 1; k < formula.Length; k++) {
					if(formula[k] != "+" && formula[k] != "-" && formula[k] != "x" && formula[k] != "@") {
						break;
					}
				}

				//Realizar suma o resta
				aux = (formula[i] == "+") ? float.Parse(formula[j]) + float.Parse(formula[k]) : 
											float.Parse(formula[j]) - float.Parse(formula[k]);

				//Modificar formula
				formula[i] = aux.ToString("f2");
				formula[j] = formula[k] = "@";
			}
		}

		//Asignar resultado final
		currentAnswer = aux;
		//Debug.Log ("Answer: " + currentAnswer);

		//Colocar elementos de la pregunta
		//Asumiendo que todos los elementos tienen las mismas dimensiones
		goList.Add(Instantiate(sweetArrayPf[sweetID[0]], new Vector3(offsetX + formula.Length / 2f - formula.Length, offsetY, 0f), Quaternion.identity) as GameObject);
		goList [goList.Count - 1].transform.SetParent(quizObjectParent);
		for (i = 1, j = 0, aux = 60; i < sweetID.Length; i++, j++, aux += 120) {
			//Agregar simbolo
			goList.Add (Instantiate (mathSymbolPf [mathID [j]], new Vector3 (offsetX + formula.Length / 2f - formula.Length + aux, offsetY, 0f), Quaternion.identity) as GameObject);
			goList [goList.Count - 1].transform.SetParent(quizObjectParent);

			//Agregar dulce
			goList.Add (Instantiate (sweetArrayPf [sweetID [i]], new Vector3 (offsetX + formula.Length / 2f - formula.Length + aux + 60, offsetY, 0f), Quaternion.identity) as GameObject);
			goList [goList.Count - 1].transform.SetParent(quizObjectParent);
		}
		//Colocar signo de igual
		goList.Add (Instantiate (mathSymbolPf [mathSymbolPf.Length - 1], new Vector3 (offsetX + formula.Length / 2f - formula.Length + aux, offsetY, 0f), Quaternion.identity) as GameObject);
		goList [goList.Count - 1].transform.SetParent(quizObjectParent);
		//Colocar signo de interrogacion
		goList.Add (Instantiate (mathSymbolPf [mathSymbolPf.Length - 2], new Vector3 (offsetX + formula.Length / 2f - formula.Length + aux + 60, offsetY, 0f), Quaternion.identity) as GameObject);
		goList [goList.Count - 1].transform.SetParent(quizObjectParent);

		//Colocar respuestas
		correctID = Random.Range (0, answerText.Length);
		for (i = 0; i < answerText.Length; i++) {
			if(correctID == i) { //Respuesta correcta
				answerText[i].text = currentAnswer.ToString("f2");
				currentCorrect = i;
			}
			else { //Respuesta incorrecta
				aux = Random.Range(Random.Range(-4f, -1f), Random.Range(1f, 4f));
				aux = (Mathf.Abs(aux) < 1f) ? aux = Random.Range(3f, 6f) : aux;

				answerText[i].text = (aux + currentAnswer).ToString("f2");
			}
		}

		//Asignar nuevo producto a jugador
		productType = Random.Range (0, boxSpritesArray.Length); bool productFound = false;
		//Chequear si el producto existe en inventario, si no buscar linealmente alguno disponible
		if (productCounter [productType] <= 0) {
			for (i = 0; i < boxSpritesArray.Length; i++) {
				if (productCounter [i] > 0) { //Chequeo si el producto actual se puede vender
					productType = i;
					productFound = true; break;
				}
			}
			
			//No existen productos para vender
			if(!productFound) {
				OnWin(true);
			}
		}
	}

	//Se activa cuando se hace clic en una respuesta
	public void OnClickAnswer(int ID){
		if (ID == currentCorrect) { //Respuesta correcta
			UpdateUI (productType);

			questionCorrect++;

			//Desactivar interfaz de ayuda  
			if (questionCorrect >= 3 && helpUI.active) {
				helpUI.SetActive (false);
			}
		}

		//Next questions
		InitQuestion ();

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	//Esconder / Aparecer la interfaz de ayuda
	public void OnClickHelp(){
		helpIsHiding = !helpIsHiding;

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	//Pasar pregunta
	public void OnClickNextQuestion () {
		InitQuestion ();

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
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
		Application.LoadLevel ("(5) SellReportV1");

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}
	
	public void UpdateUI(int ID) {
		//Reducir el contador de productos y actualizar UI
		productCounter [ID]--;
		productCantText [ID].text = "x" + productCounter [ID].ToString("d0");
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
				aux = gameDurationTime - (Time.time - startTime);
				timeTextUI.text = aux.ToString("f1") + "s";
				
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
		timeTextUI.text = "0.0s";
		
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
				GameController.instance.SetProductCounter (productCounter);
			
			//Quitar referencia del singleton
			instance = null;
			
			//Eliminar cajas restantes
			foreach(GameObject A in GameObject.FindGameObjectsWithTag("SkyBoxItem")){
				Destroy(A);
			}
			
			//Cargar escena de decisiones
			Application.LoadLevel ("(3) DecisionsV1");
		}
		else if(confirmID == 2) { //Salir del juego
			//Quitar referencia del singleton
			instance = null;
			
			//Eliminar cajas restantes
			foreach(GameObject A in GameObject.FindGameObjectsWithTag("SkyBoxItem")){
				Destroy(A);
			}
			
			//Cargar menu principal
			Application.LoadLevel("(0) DemoMainMenuV1");
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

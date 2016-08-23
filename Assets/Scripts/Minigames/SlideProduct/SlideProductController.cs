using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SlideProductController : MonoBehaviour {
	public static SlideProductController instance; //Singleton

	public Transform targetStart;			//Transform del producto
	public Vector3 targetEnd;				//Posicion del objetivo final del juego
	private bool movingToTarget;			//Flag: Indica que el producto se esta moviendo al punto final

	public Text[] productCantText;			//Texto UI, Arreglo de todos los textos de los productos de la interfaz derecha
	private int[] productCounter;			//Contador de cuantos productos el jugador ha vendido

	public GameObject changeLayoutButton;	//Referencia del boton Change Layout

	public Text timeTextUI;					//Texto de tiempo UI
	private float startTime;				//Tiempo de inicio
	private float offsetTime;				//Tiempo en estado de pausa
	private float offsetGameDuration;		//Tiempo acumulado en estado de pausa
	public float gameDurationTime = 80f;	//Duracion total del juego en segundos

	public float slowUpdateDelta = 0.2f;	//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;				//Referencia de coroutina

	public GameObject confirmUI;			//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;					//Identifica a quien le pertenece el mensaje de confirmacion

	public Sprite[] boxSpritesArray;		//Referencia de las imagenes para las cajas

	public Transform obstacleParent;		//Padre de obstaculos
	private List<Vector3> obstaclePos;		//Arreglo de posicion de los obstaculos
	public GameObject obstaclePf;			//Prefab de obstaculos
	public GameObject targetPf;				//Prefab de objetivo

	public GameObject introUI;				//UI de inicio
	public GameObject endUI;				//UI de finalizacion del juego
	public GameObject pauseUI;				//UI de minijuego pausado

	public Text endUIText;					//Texto de la UI de fin de minijuego

	public Vector2 wallSizeCoord;			//Limite en X y en Y de la interfaz en coordenadas de mundo

	public SlideProductPlayer playerScript; //Referencia interna del script del jugador

	private bool onPause;					//Flag: Indica si el juego esta en pausa
	private bool onMenu;					//Internal Flag / indica si el juego esta mostrando algun menu
	private bool onWin;						//Internal Flag / indica si se termino el juego

	private Vector3 auxV3;					//Auxiliar Vector3
	private GameObject auxGO;				//Auxiliar GameObject
	private int[][] sceneMap;			    //Auxiliar del mapa de la escena
	private List<GameObject> sceneElements;	//Todos los elementos en la escena
	private float timerAux;					//Auxliar de tiempo de pausa

    public GameObject hintPf;               //Prefab de la pista
    private List<Vector3> hintPos;			//Posicion a donde colocar la pista
    private List<GameObject> hintGO;        //Referencia de las pistas en escena
	public float hintDelay = 5f;			//Tiempo de aparacion de la pista
	private float hintTime;					//Tiempo total de aparicion de la pista
    public int hintLevel = 2;               //Cantidad total de pistas a mostrar

	void Awake(){
		//Logica del Singleton
		if (instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(this);
		}
	}

	// Use this for initialization
	void Start () {
		//Preinicializar el juego
		PreInitGame ();

		//Obtener valores del GameController
		if (GameController.instance != null) {
			productCounter = GameController.instance.GetInventory ();

			//Satisfaccion del cliente
			gameDurationTime *= GameController.instance.GetSatisfaction ();
		}
		else {
			productCounter = new int[boxSpritesArray.Length];
			for(int i = 0; i < productCounter.Length; i++) {
				productCounter[i] = 1;
			}
		}

		//Setup Right UI
		for (int i = 0; i < productCantText.Length; i++) {
			productCantText[i].text = productCounter[i].ToString("d0");
		}

		if (GameController.instance != null) {
			//Satisfaccion del cliente
			gameDurationTime *= GameController.instance.GetSatisfaction ();
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
		if (Input.GetMouseButtonDown (0) && !onPause) {	//Checkear si se realizo algun click izquierdo del mouse
			if (!playerScript.isMoving()) {				//Si el jugador no esta en movimiento realizar movimiento
				//Asignar posicion aproximada del mouse a auxV3
				auxV3 = Input.mousePosition; //auxV3.z = 1f;
				auxV3 = Camera.main.ScreenToWorldPoint(auxV3);
				auxV3 = new Vector3(Mathf.Round(auxV3.x), Mathf.Round(auxV3.y), Mathf.Round(auxV3.z));

				//Calcular objetivo del movimiento
				playerScript.MoveToTarget(CalculateEndPos(auxV3));
			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			onPause = !onPause;
			
			if (onPause) {
				timerAux = Time.time;
				
				//Mostrar UI Pausa
				pauseUI.SetActive(true);

				//Inhabilitar boton de cambio de layout
				changeLayoutButton.SetActive(false);
			}
			else {
				//Actualizar tiempo de juego && tiempo de spawn de la caja
				offsetTime = Time.time - timerAux;
				offsetGameDuration += offsetTime;
				//gameDurationTime += offsetTime;
				
				//Esconder UI Pausa
				pauseUI.SetActive(false);

				//Habilitar boton de cambio de layout
				changeLayoutButton.SetActive(true);
			}
		}

		if (targetEnd == targetStart.position && !onMenu && !onPause) {
            if (MusicController.instance != null) {
                MusicController.instance.PlayMinigameSound(1); //Cash Sound
            }

			UpdateUI(playerScript.GetProductID());
			TargetReached();
		}
	}

	public void OnClickPlay(){
		//Inicializar minijuego
		InitGame ();
		SetupScene ();

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}
	}

	public void OnClickContinue(){
		//Cargar siguiente interfaz
		SceneManager.LoadScene ("(5) SellReportV1");

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	//Chequea el estado de la pausa
	public bool CheckPause(){
		return onPause;
	}

	//Chequea si se esta en algun menu
	public bool CheckMenu(){
		return onMenu;
	}

	void PreInitGame(){
		//Inicializar texto de la interfaz derecha
		timeTextUI.text = "0.0s";
		
		introUI.SetActive (true);
		endUI.SetActive (false);
		pauseUI.SetActive (false);
		
		//Activo porque se ve introUI;
		onMenu = true;

		//Esconder boton de cambiar layout
		changeLayoutButton.SetActive (false);

        //Inicializar pistas
        hintPos = new List<Vector3>();
        hintGO = new List<GameObject>();
	}

	//Inicializa el minijuego
	void InitGame(){
		//Set timer
		startTime = Time.time;
		offsetTime = offsetGameDuration = 0f;
		timeTextUI.text = "0.0s";
		
		//Set focus 
		onMenu = onPause = false;
		introUI.SetActive (false);
		endUI.SetActive (false);
		pauseUI.SetActive (false);
		
		//Start time
		slowCor = StartCoroutine (SlowUpdate ());

		//Mostrar boton de cambiar layout
		changeLayoutButton.SetActive (true);
	}

	public void UpdateUI(int ID) {
		//Reducir el contador de productos y actualizar UI
		productCounter [ID]--;
		productCantText [ID].text = productCounter [ID].ToString("d0");
	}

	//Obtiene el obstaculo mas cercano alineado con el jugador / CC: retorna vec3(1f, 1f, 1f)
	Vector3 CalculateEndPos(Vector3 mousePos) {
		Vector3 near = Vector3.one;						//Obstaculo mas cercano al item/jugador
		float nearDist = Mathf.Infinity;				//Distancia mas cercana
		float currentDist;								//Auxiliar de distancia actual calculada

		bool checkX, checkGreater = false; 

		if (mousePos == playerScript.GetPosition ()) {	//Clicked on player
			return near;
		}

		if (mousePos.x == playerScript.GetPosition().x) {		//Checkear en X
			checkX = true;	//Alineado en X
			//Checkear si es mayor que
			checkGreater = (mousePos.y > playerScript.GetPosition().y) ? true : false;
		}
		else if (mousePos.y == playerScript.GetPosition().y) {	//Checkear en Y
			checkX = false; //Alineado en Y
			//Checkear si es mayor que
			checkGreater = (mousePos.x > playerScript.GetPosition().x) ? true : false;
		}
		else { //El mouse no esta alineado con el jugador
			return near;
		}

		for (int i = 0; i < obstaclePos.Count; i++) {	//Buscar cual es el obstaculo mas cercano al item
			if(checkX && obstaclePos[i].x == playerScript.GetPosition().x) { //El mouse esta alineado en X
				if(checkGreater && obstaclePos[i].y > playerScript.GetPosition().y){ //Producto esta antes que obstaculo
					currentDist = Vector3.Distance(obstaclePos[i], playerScript.GetPosition());

					//Si la distancia actual es menor que la menor de las distancias calculadas
					//entonces el obstaculo actual es el mas cercano
					if(currentDist < nearDist) {
						near = new Vector3(obstaclePos[i].x, obstaclePos[i].y - 1f, obstaclePos[i].z); //Posicion objetivo
						nearDist = currentDist;
					}
				}
				else if(!checkGreater && obstaclePos[i].y < playerScript.GetPosition().y){	//Producto esta despues del obstaculo
					currentDist = Vector3.Distance(obstaclePos[i], playerScript.GetPosition());
					
					//Si la distancia actual es menor que la menor de las distancias calculadas
					//entonces el obstaculo actual es el mas cercano
					if(currentDist < nearDist) {
						near = new Vector3(obstaclePos[i].x, obstaclePos[i].y + 1f, obstaclePos[i].z); //Posicion objetivo
						nearDist = currentDist;
					}
				}
			}
			else if (!checkX && obstaclePos[i].y == playerScript.GetPosition().y){ //El mouse esta alineado en Y
				if(checkGreater && obstaclePos[i].x > playerScript.GetPosition().x){ //Producto esta antes que obstaculo
					currentDist = Vector3.Distance(obstaclePos[i], playerScript.GetPosition());
					
					//Si la distancia actual es menor que la menor de las distancias calculadas
					//entonces el obstaculo actual es el mas cercano
					if(currentDist < nearDist) {
						near = new Vector3(obstaclePos[i].x - 1f, obstaclePos[i].y, obstaclePos[i].z); //Posicion objetivo
						nearDist = currentDist;
					}
				}
				else if(!checkGreater && obstaclePos[i].x < playerScript.GetPosition().x){	//Producto esta despues del obstaculo
					currentDist = Vector3.Distance(obstaclePos[i], playerScript.GetPosition());
					
					//Si la distancia actual es menor que la menor de las distancias calculadas
					//entonces el obstaculo actual es el mas cercano
					if(currentDist < nearDist) {
						near = new Vector3(obstaclePos[i].x + 1f, obstaclePos[i].y, obstaclePos[i].z); //Posicion objetivo
						nearDist = currentDist;
					}
				}
			}
		}

		//Test si se debe mover al target final
		if (nearDist > Vector3.Distance (targetEnd, playerScript.GetPosition ())) { //Comprobar que el target esta mas cerca que el obstaculo mas proximo
			if (mousePos.x == playerScript.GetPosition ().x && mousePos.x == targetEnd.x) { 		//Esta alineado en X
				//Producto esta antes de target y mouse
				if (playerScript.GetPosition ().y < targetEnd.y && playerScript.GetPosition ().y < mousePos.y) {
					near = targetEnd;
					movingToTarget = true;
				}
			//Producto esta despues de target y mouse
			else if (playerScript.GetPosition ().y > targetEnd.y && playerScript.GetPosition ().y > mousePos.y) {
					near = targetEnd;
					movingToTarget = true;
				}
			} else if (mousePos.y == playerScript.GetPosition ().y && mousePos.y == targetEnd.y) { 	//Esta alineado en Y
				//Producto esta antes de target y mouse
				if (playerScript.GetPosition ().x < targetEnd.x && playerScript.GetPosition ().x < mousePos.x) {
					near = targetEnd;
					movingToTarget = true;
				}
			//Producto esta despues de target y mouse
			else if (playerScript.GetPosition ().x > targetEnd.x && playerScript.GetPosition ().x > mousePos.x) {
					near = targetEnd;
					movingToTarget = true;
				}
			}
		}

		//Test si debe colisionar con alguna pared
		if (near == Vector3.one) {
			if(mousePos.x == playerScript.GetPosition().x) { //Alineado en X
				if(mousePos.y > playerScript.GetPosition().y) {	//Posicion del jugador esta antes que mouse
					//Mover hacia pared de arriba
					near = new Vector3(mousePos.x,  wallSizeCoord.y - 1f, 0f);
				}
				else {
					//Mover hacia pared de abajo
					near = new Vector3(mousePos.x, -wallSizeCoord.y + 1f, 0f);
				}
			}
			else if(mousePos.y == playerScript.GetPosition().y) { //Alineado en Y
				if(mousePos.x > playerScript.GetPosition().x) {	//Posicion del jugador esta antes que mouse
					//Mover hacia pared derecha
					near = new Vector3( wallSizeCoord.x - 1f, mousePos.y, 0f);
				}
				else {
					//Mover hacia pared izquierda
					near = new Vector3(-wallSizeCoord.x + 1f, mousePos.y, 0f);
				}
			}
		}

		return near;
	}

	//Checkea las paredes y retorna la nueva direccion si existe una pared como obstaculo
	int CheckWall (int currentDir) {
		if (currentDir == 0 && (wallSizeCoord.y - 1f - playerScript.GetPosition ().y) < Mathf.Epsilon) { //Si tiene pared arriba ir abajo
			currentDir = 2;
		}
		else if(currentDir == 2 && (wallSizeCoord.y + 1f - playerScript.GetPosition ().y) < Mathf.Epsilon) { //Si tiene pared abajo ir arriba
			currentDir = 0;
		}
		else if(currentDir == 1 && (wallSizeCoord.x - 1f - playerScript.GetPosition ().x) < Mathf.Epsilon) { //Si tiene pared a la derecha ir a la izquierda
			currentDir = 3;
		}
		else if(currentDir == 3 && (wallSizeCoord.x + 1f - playerScript.GetPosition ().x) < Mathf.Epsilon) { //Si tiene pared a la izquierda ir a la derecha
			currentDir = 1;
		}

		return currentDir;
	}

	//Genera un mapa del nivel de forma procedural
	void GenerateProceduralLevel(int difficulty = 1) {
		Vector3 startPos = targetStart.position + new Vector3(wallSizeCoord.x, wallSizeCoord.y, 0f); //Posicion inicial (offset aplicado)
		Vector3 auxStartPos;
		int currentDir;	//Direccion actual
		float rndChange = 0f; 	//Chance de cambiar direccion del camino
		float rndRatio = 0.1f;	//Incremento de probabilidad de cambio de direccion
		float pivotPos;			//Posicion pivote
		bool endFound;			//Bandera que indica si se termino de marcar un camino
        int pathMarkCount = 0;  //Cantidad de caminos hechos
        bool genHint = false;    //Bandera que indica si se debe agregar una pista a la lista de pistas

		//Inicializacion del mapa que cubre la escena jugable
		//0 - Puede ir obstaculo / 1 - Parte del camino / 2 - Debe ir obstaculo
		sceneMap = new int[(int)(wallSizeCoord.x) * 2][];
		for (int i = 0; i < sceneMap.Length; i++) {
			sceneMap[i] = new int[(int)(wallSizeCoord.y) * 2];
		}

		//Get random linear direction / 0 - Up / 1 - Right / 2 - Down / 3 - Left
		currentDir = Random.Range (0, 4);

		//Checkear si el producto puede moverse en esta direccion (Checkear paredes)
		currentDir = CheckWall (currentDir);

		//Buscar N(difficulty) lineas y colocar obstaculos
		while (difficulty > 0) {
			rndChange = 0;
			if (currentDir == 0 || currentDir == 2) { //Wi area
				rndRatio = 1f / wallSizeCoord.y;
			}
			else if (currentDir == 1 || currentDir == 3) {
				rndRatio = 1f / wallSizeCoord.x;
			}

            //Comprobar si se debe agregar una pista
            genHint = false;
            if (pathMarkCount < hintLevel) genHint = true;

			auxStartPos = startPos;
			if (!MakePath (currentDir, out rndChange, rndRatio, out startPos, rndChange, auxStartPos, genHint)) {
				//Debug.Log ("Here");
				//Cambiar la direccion a la opuesta
				if(currentDir == 0) currentDir = 2;
				else if(currentDir == 1) currentDir = 3;
				else if(currentDir == 2) currentDir = 0;
				else if(currentDir == 3) currentDir = 1;

				MakePath (currentDir, out rndChange, rndRatio, out startPos, rndChange, auxStartPos, genHint);
			}
            //Se marco un camino
            pathMarkCount++;

			//Cambiar la direccion a una perpendicular
			if (currentDir == 0 || currentDir == 2) {
				//Cambiar direccion (derecha/izquierda)
				if (Random.Range (0f, 1f) < 0.5f) { //Cambiar a derecha
					currentDir = 1;
				} else { //Izquierda
					currentDir = 3;
				}
			} 
			else if (currentDir == 1 || currentDir == 3) {
				//Cambiar direccion (Wi/arae)
				if(Random.Range(0f, 1f) < 0.5f) { //Cambiar wi
					currentDir = 0;
				}
				else { //arae
					currentDir = 2;
				}
			}

			//Checkear direcccion y cambiarla si es debido
			currentDir = CheckWall (currentDir);

			//Marcar obstaculo obligatorio //Up / Down / wi arae wi wi arae
			if(currentDir == 1 || currentDir == 3) {
				if(startPos.y - 1f > -1f) {
					if(sceneMap[(int)startPos.x][(int)startPos.y - 1] != 1) { //Si la posicion de abajo esta libre marcar obstaculo
						sceneMap[(int)startPos.x][(int)startPos.y - 1] = 2;
					}
				}
				if(startPos.y + 1f < sceneMap[0].Length) {
					if(sceneMap[(int)startPos.x][(int)startPos.y + 1] != 1) { //Si la posicion de arriba esta libre marcar obstaculo
						sceneMap[(int)startPos.x][(int)startPos.y + 1] = 2;
					}
				}
			}
			else if(currentDir == 0 || currentDir == 2) {
				if(startPos.x - 1f > -1f) {
					if(sceneMap[(int)startPos.x - 1][(int)startPos.y] != 1) { //Si la posicion de la derecha esta libre marcar obstaculo
						sceneMap[(int)startPos.x - 1][(int)startPos.y] = 2;
					}
				}
				if(startPos.x + 1f < sceneMap.Length) {
					if(sceneMap[(int)startPos.x + 1][(int)startPos.y] != 1) { //Si la posicion de la izquierda esta libre marcar obstaculo
						sceneMap[(int)startPos.x + 1][(int)startPos.y] = 2;
					}
				}
			}
			//Reduce dificultad
			difficulty--;
		}

		//Posicion final
		targetEnd = new Vector3(startPos.x - wallSizeCoord.x, startPos.y - wallSizeCoord.y, startPos.z);

		float rndObstacleSpawn = 0.05f;

		//Añadir obstaculos a la lista
		for (int i = 1; i < sceneMap.Length; i++) {
			for (int j = 1; j < sceneMap[i].Length; j++) {
				/*if((targetEnd.x == i - wallSizeCoord.x && targetEnd.y == j - wallSizeCoord.y) || 
				   (targetStart.position.x == i - wallSizeCoord.x && targetStart.position.y == j - wallSizeCoord.y)) {
					continue;
				}
				else */if(sceneMap[i][j] == 2) {
					obstaclePos.Add(new Vector3(i - wallSizeCoord.x, j - wallSizeCoord.y));
				}
				else if(Random.Range(0f, 1f) < rndObstacleSpawn && sceneMap[i][j] != 1){
					obstaclePos.Add(new Vector3(i - wallSizeCoord.x, j - wallSizeCoord.y));
				}
			}
		}
	}

	public void OnWin(bool isInvEmpty){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
		onMenu = true;

		if (isInvEmpty) {
			endUIText.text = "Inventory is empty.";
		}

		//Esconder boton de cambiar layout
		changeLayoutButton.SetActive (false);

		//Volcar data a GameController
		if (GameController.instance != null) {
            GameController.instance.SetProductCounter(productCounter); //Update sold
		}
	}

	public void TargetReached(){
		if (onMenu) //Chequea si se esta en algun menu
			return;

		//Clear position list
		obstaclePos.Clear ();

		//Destruir objetos en la escena
		foreach (GameObject A in sceneElements) {
			Destroy(A);
		}

         //Destruir pistas
        foreach (GameObject A in hintGO) {
            Destroy(A);
        }
        hintGO.Clear(); hintPos.Clear();

		//Asignar nuevo producto a jugador
		int typeID = Random.Range (0, boxSpritesArray.Length); bool productFound = false;
		//Chequear si el producto existe en inventario, si no buscar linealmente alguno disponible
		if (productCounter [typeID] > 0) {
			playerScript.SetID (typeID);
		}
		else {
			for (int i = 0; i < boxSpritesArray.Length; i++) {
				if (productCounter [i] > 0) { //Chequeo si el producto actual se puede vender
					playerScript.SetID (i);
					productFound = true; break;
				}
			}
			
			//No existen productos para vender
			if(!productFound) {
				OnWin(true);
			}
		}

		//Generar nuevo mapa
		GenerateProceduralLevel (4);

		//Instancia los obstaculos
		for(int i = 0; i < obstaclePos.Count; i++) {
			auxGO = Instantiate(obstaclePf, obstaclePos[i], Quaternion.identity) as GameObject;
			auxGO.GetComponent<SlideProductObstacle> ().ID = i;
			auxGO.transform.parent = obstacleParent;
			
			//Asignar referencia interna de los objetos de la escena
			sceneElements.Add (auxGO);
		}
		
		//Instanciar objetivo
		auxGO = Instantiate (targetPf, targetEnd, Quaternion.identity) as GameObject;
		sceneElements.Add (auxGO);
	}

	public void SetupScene () {
		//Verificar si existen elementos en la escena
		if (obstaclePos != null) {
			obstaclePos.Clear ();
		}
		if (sceneElements != null) {
			foreach (GameObject A in sceneElements) {
				Destroy(A);
			}
		}

		obstaclePos = new List<Vector3> (); //Inicializar lista de posicion de obstaculos
		sceneElements = new List<GameObject> (); //Lista de elementos en la escena

		//Generar nuevo mapa
		GenerateProceduralLevel (4);

		//Asignar nuevo producto a jugador
		int typeID = Random.Range (0, boxSpritesArray.Length); bool productFound = false;
		//Chequear si el producto existe en inventario, si no buscar linealmente alguno disponible
		if (productCounter [typeID] > 0) {
			playerScript.SetID (typeID);

			Debug.Log ("Product " + typeID);
		}
		else {
			for (int i = 0; i < boxSpritesArray.Length; i++) {
				if (productCounter [i] > 0) { //Chequeo si el producto actual se puede vender
					playerScript.SetID (i);
					productFound = true; break;
				}
			}

			//No existen productos para vender
			if(!productFound) {
				OnWin(true);
			}
		}

		//Instancia los obstaculos
		for(int i = 0; i < obstaclePos.Count; i++) {
			auxGO = Instantiate(obstaclePf, obstaclePos[i], Quaternion.identity) as GameObject;
			auxGO.transform.parent = obstacleParent;

			//Asignar referencia interna de los objetos de la escena
			sceneElements.Add (auxGO);
		}

		//Instanciar objetivo
		auxGO = Instantiate (targetPf, targetEnd, Quaternion.identity) as GameObject;
		sceneElements.Add (auxGO);

        //Instanciar pistas
        for (int i = 0; i < hintPos.Count; i++) {
            hintGO.Add(Instantiate(hintPf, hintPos[i] - new Vector3(wallSizeCoord.x, wallSizeCoord.y, 0f), Quaternion.identity) as GameObject);
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
				timeTextUI.text = aux.ToString ("00") + ":" + aux2.ToString("00");
				
				//Termino el tiempo
				if (aux <= 0f) {
					OnWin (false);
					Debug.Log("Finish");
					yield break;
				}
			}
		}
	}

	public Sprite GetSprite(int ID) {
		return boxSpritesArray [ID];
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

		//Mostrar boton de cambiar layout
		changeLayoutButton.SetActive (true);

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
		timeTextUI.text = "0s";

		//Esconder UI Pausa
		pauseUI.SetActive(false);

		//Resetear posicion del producto
		targetEnd = targetStart.position = Vector3.zero;
		playerScript.MoveToTarget (targetEnd);

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
                GameController.instance.SetProductCounter(productCounter); //Update sold
            }

			//Quitar referencia del singleton
			instance = null;

			//Destroy Player Controller
			Destroy(playerScript.gameObject);

			//Cargar escena de decisiones
            SceneManager.LoadScene("(5) SellReportV1");
		}
		else if(confirmID == 2) { //Salir del juego
			//Quitar referencia del singleton
			instance = null;

			//Destroy Player Controller
			Destroy(playerScript.gameObject);

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

	public void OnClickNewMap(){
		Debug.Log ("Generate new map");

		if (onMenu) //Chequea si se esta en algun menu
			return;
		
		//Clear position list
		obstaclePos.Clear ();
		
		//Destruir objetos en la escena
		foreach (GameObject A in sceneElements) {
			Destroy(A);
		}

        //Destruir pistas
        foreach (GameObject A in hintGO) {
            Destroy(A);
        }
        hintGO.Clear(); hintPos.Clear();
		
		//Asignar nuevo producto a jugador (si existe)
		int typeID = Random.Range (0, boxSpritesArray.Length); bool productFound = false;
		//Chequear si el producto existe en inventario, si no buscar linealmente alguno disponible
		if (productCounter [typeID] > 0) {
			playerScript.SetID (typeID);
		}
		else {
			for (int i = 0; i < boxSpritesArray.Length; i++) {
				if (productCounter [i] > 0) { //Chequeo si el producto actual se puede vender
					playerScript.SetID (i);
					productFound = true; break;
				}
			}
		}
		
		//Generar nuevo mapa
		GenerateProceduralLevel (4);
		
		//Instancia los obstaculos
		for(int i = 0; i < obstaclePos.Count; i++) {
			auxGO = Instantiate(obstaclePf, obstaclePos[i], Quaternion.identity) as GameObject;
			auxGO.transform.parent = obstacleParent;
			
			//Asignar referencia interna de los objetos de la escena
			sceneElements.Add (auxGO);
		}
		
		//Instanciar objetivo
		auxGO = Instantiate (targetPf, targetEnd, Quaternion.identity) as GameObject;
		sceneElements.Add (auxGO);

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	bool MakePath(int currentDir, out float rndChange, float rndRatio, out Vector3 startPos, float rndChangeInit, Vector3 startPosInit, bool markHint){
		bool endFound = false;
		rndChange = rndChangeInit; 	//Inicializar rnd change
		startPos = startPosInit;	//Inicializar start Pos

		if(currentDir == 0) { //Up - hacia arriba - wi
			for (int i = (int)startPos.y; i < sceneMap[0].Length; i++) { //Lock X Coordinate / Move on Y axis
				if(sceneMap[(int)startPos.x][i] == 2){	//Si en la posicion ya esta un obstaculo
					break;
				}

				if (sceneMap [(int)startPos.x] [i] == 1) {//Ya existe un camino que pasa por aqui
					rndChange += rndRatio;
					continue;
				}

				sceneMap[(int)startPos.x][i] = 1; //Marcar posicion actual

				//Cambiar de direccion usando probabilidades (funcion uniforme)
				if(rndChange > Random.Range(0f, 1f)) {
					//Asignar nuevo comienzo
					startPos = new Vector3(startPos.x, i, startPos.z);

                    //Colocar pista
                    if (markHint) {
                        markHint = false;
                        hintPos.Add(new Vector3(startPos.x, i));
                    }

					endFound = true;
					break;
				}
				else {
					rndChange += rndRatio;
				}
			}
		}
		else if(currentDir == 2) { //Down - hacia abajo - arae
			for (int i = (int)startPos.y; i > 1; i--) { //Lock X Coordinate / Move on Y axis
				if(sceneMap[(int)startPos.x][i] == 2){	//Si la posicion ya esta un obstaculo
					break;
				}

				if (sceneMap [(int)startPos.x] [i] == 1) {//Ya existe un camino que pasa por aqui
					rndChange += rndRatio;
					continue;
				}

				sceneMap[(int)startPos.x][i] = 1; //Marcar posicion actual

				//Cambiar de direccion usando probabilidades (funcion uniforme)
				if(rndChange > Random.Range(0f, 1f)) {
					//Asignar nuevo comienzo
					startPos = new Vector3(startPos.x, i, startPos.z);

                    //Colocar pista
                    if (markHint) {
                        markHint = false;
                        hintPos.Add(new Vector3(startPos.x, i));
                    }

					endFound = true;
					break;
				}
				else {
					rndChange += rndRatio;
				}
			}
			//Debug.Log("Dir: " + currentDir + " " + difficulty + ": " + new Vector3(startPos.x - wallSizeCoord.x, startPos.y - wallSizeCoord.y, startPos.z));
		}
		else if(currentDir == 1) { //Derecha - hacia la derecha
			for (int i = (int)startPos.x; i < sceneMap.Length; i++) { //Lock Y Coordinate / Move on X axis
				if(sceneMap[i][(int)startPos.y] == 2){	//Si la posicion ya esta un obstaculo
					break;
				}

				if (sceneMap[i][(int)startPos.y] == 1) {//Ya existe un camino que pasa por aqui
					rndChange += rndRatio;
					continue;
				}

				sceneMap[i][(int)startPos.y] = 1; //Marcar posicion actual

				//Cambiar de direccion usando probabilidades (funcion uniforme)
				if(rndChange > Random.Range(0f, 1f)) {
					//Asignar nuevo comienzo
					startPos = new Vector3(i, startPos.y, startPos.z);

                    //Colocar pista
                    if (markHint) {
                        markHint = false;
                        hintPos.Add(new Vector3(i, startPos.y));
                    }

					endFound = true;
					break;
				}
				else {
					rndChange += rndRatio;
				}
			}
			//Debug.Log("Dir: " + currentDir + " " + difficulty + ": " + new Vector3(startPos.x - wallSizeCoord.x, startPos.y - wallSizeCoord.y, startPos.z));
		}
		else if(currentDir == 3) { //Izquierda - hacia la izquierda
			for (int i = (int)startPos.x; i > 1; i--) { //Lock Y Coordinate / Move on X axis
				if(sceneMap[i][(int)startPos.y] == 2){	//Si la posicion ya esta un obstaculo
					break;
				}

				if (sceneMap[i][(int)startPos.y] == 1) {//Ya existe un camino que pasa por aqui
					rndChange += rndRatio;
					continue;
				}

                sceneMap[i][(int)startPos.y] = 1; //Marcar posicion actual

				//Cambiar de direccion usando probabilidades (funcion uniforme)
				if(rndChange > Random.Range(0f, 1f)) {
					//Asignar nuevo comienzo
					startPos = new Vector3(i, startPos.y, startPos.z);

                     //Colocar pista
                    if (markHint) {
                        markHint = false;
                        hintPos.Add(new Vector3(i, startPos.y));
                    }

					endFound = true;
					break;
				}
				else {
					rndChange += rndRatio;
				}
			}
			//Debug.Log("Dir: " + currentDir + " " + difficulty + ": " + new Vector3(startPos.x - wallSizeCoord.x, startPos.y - wallSizeCoord.y, startPos.z));
		}

		return endFound;
	}
}

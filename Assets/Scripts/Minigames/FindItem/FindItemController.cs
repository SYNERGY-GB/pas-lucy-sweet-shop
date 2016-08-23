using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class FindItemController : MonoBehaviour {
	public GameObject itemPf;						//Prefabs de objetos a mostrar
	public Sprite[] itemSprite;					 	//Sprites para los objetos
	public Color[] colorItem;						//Posible color para los objetos
	public int itemCant = 16;						//Cantidad de objetos en cada linea del estante
	public int shelfCant = 3;						//Cantidad de lineas en el estante
	public float horizontalSize = 0.765f;			//Tamaño horizontal del prefab
	public float verticalSize = 0.5f;				//Tamaño vertical del prefab
	public float offsetX = -0.85f;					//Offset horizontal de los objetos en la escena

	public GameObject shelfPf;						//Prefab de la linea del estante
	public Vector3[] shelfPos;						//Posicion de cada linea del estante
	public Vector2[] shelfSize;						//Tamaño horizontal de la line del estante

	private GameObject auxGO;						//Auxiliar de GameObject
	//private FindItemStats[] itemArray;			//Referencia interna de todos los items
	private bool[] itemUsedID;						//ID de objeto usados

	private Ray rayShoot;							//Disparo del rayo
	private RaycastHit2D rayInfo;					//Informacion de la colision rayo-collider

	//Posicion inicial de los objetos a mostrar
	public Vector2 startPosLookingItems = new Vector2 (6.3f, -5.3f);
	public float timeSpawn = 5f;					//Tiempo entre spawn de productos
	public float timeProduct = 7f;					//Tiempo de busqueda de productos
	public int maxCantLookingProducts = 5;			//Cantidad maxima de productos a buscar
	public GameObject inGameUI;						//Interfaz estatica de buscar item
	public float moveRatioinGameUI;					//Distancia a mover la interfaz buscar item
	public float distLookingItems = 1f;				//Distancia entre los objetos a buscar
	public float lookingItemSpeed = 7f;				//Velocidad con la que se reacomodan los objetos a buscar

	private List<GameObject> lookingItems;			//Objetos buscados
	private List<float> lookingItemLifetime;		//Tiempo de vida de los objetos buscados
	private List<Vector3> lookingItemTarget;		//Posicion objetivo de los objetos (usado para animar)

	private float startTime;						//Tiempo de inicio del minijuego
	private float nextSpawn;						//Tiempo para la aparicion del siguiente objeto

	//Template Start
	public static FindItemController instance;		//Singleton

	public Text[] productCantText;					//Texto UI, Arreglo de todos los textos de los productos de la interfaz derecha
	private int[] productCounter;					//Contador de cuantos productos el jugador ha vendido

	public Text timeTextUI;							//Texto de tiempo UI
	private float offsetTime;						//Tiempo en estado de pausa
	public float gameDurationTime = 20f;			//Duracion total del juego en segundos

	public GameObject introUI;						//UI de inicio
	public GameObject endUI;						//UI de finalizacion del juego
	public GameObject pauseUI;						//UI de minijuego pausado

	public Sprite[] boxSpritesArray;				//Referencia de las imagenes para las cajas

	public float slowUpdateDelta = 0.1f;			//Ratio ejecucion en segundos de SlowUpdate
	private Coroutine slowCor;						//Referencia interna de Coroutine

	public GameObject confirmUI;					//UI para confirmar que se quiere salir del minijuego/juego
	private int confirmID;							//Identifica a quien le pertenece el mensaje de confirmacion

	public Text endUIText;							//Texto de la UI de fin de minijueg

	private bool onPause;							//Flag: Indica si el juego esta en pausa
	private bool onMenu;							//Flag: Indica si el juego esta mostrando algun menu

	private float timerAux;							//Auxiliar del tiempo
	//Template End

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
		//Init Template
		//Colocar UIs visibles
		introUI.SetActive (true);
		endUI.SetActive (false);
		onMenu = true;

		//Inicializar texto
		timeTextUI.text = "0.0s";

		//Obtener valores del GameController
		if (GameController.instance != null) {
			productCounter = GameController.instance.GetInventory ();

			//Aplicar satisfaccion del cliente
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
		//End Template

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

	//Inicializa el minijuego
	void InitGame(){
		//Inicializar tiempo de busqueda entre productos
		startTime = Time.time;
		nextSpawn = startTime + timeSpawn;

		//Set focus 
		onMenu = onPause = false;

		//Start time
		slowCor = StartCoroutine (SlowUpdate ());

		//Inicializa los minijuegos
		lookingItems = new List<GameObject> (); //Inicializar lista de items a buscar
		lookingItemLifetime = new List<float> ();
		lookingItemTarget = new List<Vector3> ();

		InitScene ();
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

		//Detectar clic del mouse
		if (Input.GetMouseButtonDown(0)) {
			rayShoot = Camera.main.ScreenPointToRay (Input.mousePosition); //Convertir en un rayo la posicion del mouse

			//Verificar si se cliqueo en algun objeto
			rayInfo = Physics2D.Raycast(rayShoot.origin, rayShoot.direction, 100f, 1 << LayerMask.NameToLayer("FindItem"));
			if (rayInfo.collider != null) { //Hubo colision
				//Debug.Log("ColID: " + rayInfo.collider.gameObject.GetComponent<FindItemStats>().uniqueID);

				for (int i = 0; i < lookingItems.Count; i++) {
					if (lookingItems [i].GetComponent<FindItemStats> ().uniqueID == 
						rayInfo.collider.gameObject.GetComponent<FindItemStats> ().uniqueID) { //Se encontro objeto

                        if (MusicController.instance != null) {
                            MusicController.instance.PlayMinigameSound(1); //Cash Sound
                        }

                        //Obtener producto correspondiente y actualizar interfaz
                        UpdateUI(rayInfo.collider.gameObject.GetComponent<FindItemStats>().typeID % productCounter.Length); 

						//Quitar objeto encontrado de la lista de productos a buscar
						Destroy(lookingItems[i]);

						lookingItems.RemoveAt(i);
						lookingItemLifetime.RemoveAt (i);
						lookingItemTarget.RemoveAt (i);

						//Recalculate target positions of objects to find
						CalculateTargetPos ();
					}
				}
			}
		}

		if (Time.time > nextSpawn) { //Colocar nuevo producto a buscar
			nextSpawn = Time.time + timeSpawn;

			PutLookingItem();
		}

		//Recolocar los objetos en su posicion respectiva
		for(int i = 0; i < lookingItemTarget.Count; i++) {
			lookingItems [i].gameObject.transform.position = Vector3.MoveTowards (lookingItems [i].gameObject.transform.position, lookingItemTarget [i], lookingItemSpeed * Time.deltaTime);
		}
	}

	void InitScene() {
		int currentCant = 0; //cantidad actual de objetos asignados
		int rndID0, rndID1, genID; //ID aleatorio del arreglo

		//Inicializar arreglo de objetos
		//itemArray = new FindItemStats[itemCant * shelfCant];
		itemUsedID = new bool[itemSprite.Length * colorItem.Length];

        Debug.Log("Sprite Size: " + itemSprite.Length + " Color Size: " + colorItem.Length);

		//Asignar sprites y colores unicos
		while (currentCant < itemCant * shelfCant) {
			rndID0 = Random.Range (0, itemSprite.Length);
			rndID1 = Random.Range (0, colorItem.Length);

			//Obtiene un ID para el arreglo de bools
			genID = rndID1 * itemSprite.Length + rndID0;

			//Verificar si se ha asignado un objeto con esta combinacion
			if (!itemUsedID [genID]) {
				PutItem (currentCant, rndID0, rndID1, genID);

				itemUsedID[genID] = true;
			} 
			else {
				//Buscar un espacio desocupado
				for (int i = 0; i < itemUsedID.Length; i++) {
					if (!itemUsedID [i]) {
						rndID1 = i / itemSprite.Length;
						rndID0 = i % itemSprite.Length;
						PutItem (currentCant, rndID0, rndID1, i);

						itemUsedID[i] = true;
						break;
					}
				}
			}

			currentCant++;
		}
		//Colocar primer producto a buscar
        PutLookingItem();
	}

	void PutLookingItem() {
		int rndID0, rndID1, genID, count; //ID aleatorio del arreglo
		bool found;	//auxiliar, valida si se encontro un objeto
        bool existOnList, existOnShelf, existOnInventory;
        int[] inv = new int[4];  //Referencia del inventario actual

		//Verificar que existen menos objetos a buscar que la cantidad maxima
		if(lookingItems.Count >= maxCantLookingProducts) return;

        genID = -1; found = false; count = 0;
        do {
            //Generar ID en el rango valido
            genID = (genID < 0) ? Random.Range(0, itemSprite.Length * colorItem.Length) : genID = (genID + 1) % (itemSprite.Length * colorItem.Length);

            existOnList = CheckItemExistOnList(genID); //Buscar en la lista de items por encontrar (Debe ser unico)
            existOnShelf = CheckItemExistOnShelf(genID); //Busca en los estantes (Debe estar en los estantes)
            existOnInventory = CheckItemExistOnInventory(genID); //Busca la disponibilidad en el iventario (Debe existir al menos 1 item del producto en inventario)

            count++;
        } while ((existOnList || !existOnShelf || !existOnInventory) && count < itemSprite.Length * colorItem.Length);

        //No se encontro item para agregar
        if (count >= itemSprite.Length * colorItem.Length) return;

        rndID1 = genID / itemSprite.Length;
        rndID0 = genID % itemSprite.Length;

		//Instanciar nuevo objeto a buscar
		auxGO = Instantiate (itemPf, new Vector3 (startPosLookingItems.x - distLookingItems * lookingItems.Count, startPosLookingItems.y), Quaternion.identity) as GameObject;

		//Inicializar stats
		//Cambiar Sprite
		auxGO.GetComponent<SpriteRenderer>().sprite = itemSprite[rndID0];

		//Cambiar Color
		auxGO.GetComponent<SpriteRenderer>().color = colorItem[rndID1];

		//Colocar IDs
		FindItemStats findIS = auxGO.GetComponent<FindItemStats>();
		findIS.typeID = rndID0; findIS.colorID = rndID1; findIS.uniqueID = genID;

		//Agregar a la lista de objetos buscados
		lookingItems.Add (auxGO);
		lookingItemLifetime.Add (Time.time + timeProduct);
		lookingItemTarget.Add (new Vector3 (startPosLookingItems.x - distLookingItems * lookingItemTarget.Count, startPosLookingItems.y));
    }

    bool CheckItemExistOnList(int val) {
        for(int i = 0; i < lookingItems.Count; i++) {
            //Si se encontro dos ID iguales
			if (lookingItems [i].GetComponent<FindItemStats> ().uniqueID == val){
                return true;
            }
        }
        return false;
    }

    bool CheckItemExistOnShelf(int val) {
        return itemUsedID[val];
    }

    bool CheckItemExistOnInventory(int val) {
        int[] _inv = new int[4];
        int _count = 0;

        //Load product values
        _inv[0] = productCounter[0];
        _inv[1] = productCounter[1];
        _inv[2] = productCounter[2];
        _inv[3] = productCounter[3];

        int _itemID = (val % itemSprite.Length) % _inv.Length;

        //Chequea que el item esta en el inventario
        bool _exist = (_inv[_itemID] > 0) ? true : false;

        //Comprobar que hay suficiente espacio para comprar el item deseado
        for (int i = 0; i < lookingItems.Count; i++) {
            if (lookingItems[i].GetComponent<FindItemStats>().typeID % _inv.Length == _itemID) {
                _count++;
            }
        }

        //Comprobar que se puede colocar el producto en la lista de buscados
        bool _available = (_inv[_itemID] - _count > 0) ? true : false;

        return _available && _exist;
    }

    void PutItem(int ID, int spriteID, int colorID, int uniqueID) {
		//Cambiar el pivote de centro a esquina izquierda
		//Debug.Log(ID / itemCant);
		float startX = shelfPos[Mathf.FloorToInt(ID / itemCant)].x - (shelfSize[Mathf.FloorToInt(ID / itemCant)].x / 2f), 
			  startY = shelfPos[Mathf.FloorToInt(ID / itemCant)].y + verticalSize + shelfSize[Mathf.FloorToInt(ID / itemCant)].y;
		//float offsetX = (shelfSize[Mathf.FloorToInt(ID / itemCant)].x / itemCant / 2f);

		//Colocar objeto en la escena
		auxGO = Instantiate(itemPf, new Vector3(startX + offsetX + horizontalSize * Mathf.FloorToInt(ID % itemCant), startY, 0f), Quaternion.identity) as GameObject;

		//Cambiar Sprite
		auxGO.GetComponent<SpriteRenderer>().sprite = itemSprite[spriteID];

		//Cambiar Color
		auxGO.GetComponent<SpriteRenderer>().color = colorItem[colorID];

		//Colocar IDs
		FindItemStats findIS = auxGO.GetComponent<FindItemStats>();
		findIS.typeID = spriteID; findIS.colorID = colorID; findIS.uniqueID = uniqueID;
	}

	//Recalcula la posicion objetivo de los productos
	void CalculateTargetPos() {
		for (int i = 0; i < lookingItemTarget.Count; i++) {
			lookingItemTarget [i] = new Vector3 (startPosLookingItems.x - distLookingItems * i, startPosLookingItems.y);
		}
	}

	//Chequea si el ID coincide con algunos de los objetos buscados
	bool CheckLookingItem(int uniqueID) {
		for (int i = 0; i < lookingItems.Count; i++) {
			if (lookingItems [i].GetComponent<FindItemStats>().uniqueID == uniqueID) return true;
		}
		return false;
	}

	public void OnWin(bool isInvEmpty){
		//Colocar interfaz de fin activa
		endUI.SetActive (true);
        onMenu = true; onPause = true;

		if (isInvEmpty) {
			endUIText.text = "Inventory is empty.";
		}

		//Volcar data a GameController
		if (GameController.instance != null) {
            GameController.instance.SetProductCounter(productCounter); //Update sold
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
        SceneManager.LoadScene("(5) SellReportV1");

		//Sonido
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

	public void UpdateUI(int ID) {
		//Reducir el contador de productos y actualizar UI
		productCounter [ID]--;
		productCantText [ID].text = productCounter [ID].ToString("d0");

        int _count = 0;
        for (int i = 0; i < productCounter.Length; i++) {
            _count += productCounter[i];
        }

        //Si counter < 1 no hay objetos que vender
        if (_count < 1) {
            OnWin(true);
        }
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
	/*IEnumerator DelayOnWin() {
		//Animacion de Fade

		yield return new WaitForSeconds(delayOnWin);

		//Load new level
		//Application.LoadLevel ();
	}*/

	IEnumerator SlowUpdate(){
		float aux, aux2;
		while (true) {
			yield return new WaitForSeconds (slowUpdateDelta);	//Esperar tiempo

			if(!onPause) {
				//Mostrar tiempo restante
				aux = gameDurationTime - (Time.time - startTime);// - offsetGameDuration);
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
		int[] newInv;

		confirmUI.SetActive (false);
		if (confirmID == 1) { //Salir del minijuego
			//Volcar data a GameController
			if(GameController.instance != null)  {
                GameController.instance.SetProductCounter(productCounter); //Update sold
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
}

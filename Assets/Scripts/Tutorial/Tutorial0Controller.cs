using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tutorial0Controller : MonoBehaviour {
	public static Tutorial0Controller instance;				//Singleton
	public static bool onTutorial;							//Si se esta en tutorial

	private GameObject tutorial0GO;							//Referencia del GameObject del tutorial

	public int currentTutorial;								//ID del tutorial actual

	public float delayOnLetters;							//Delay de aparacion de las letras

	public Vector2[] tutorial0PortraitPos;					//Arreglo de posiciones del retrato
	private int tutoPortraitID0;							//ID del arreglo de posiciones del retrato del tutorial 0
	public Vector2[] tutorial0DialogPos;					//Arreglo de posiciones del dialogo
	private int tutoDialogID0;								//ID del arreglo de posiciones del dialogo del tutorial 0
	public string[] tuto0Talk;								//Arreglo de texto dicho por Lucy
	private int tuto0TalkID;								//ID del arreglo de texto dicho por Lucy

	public Text currentTextUI;								//UI Text actual a usar

	private int currentStatus;								//Estatus actual

	private bool lucyTalking;								//Indica si Lucy esta hablando

	private string auxString;								//String auxiliar
	public GameObject portraitGO;							//Referencia del retrato
	public GameObject dialogGO;								//Referencia del cuadro de dialogo
	public GameObject maskGO;								//Referencia de la mascara

	public GameObject buttonNext;							//Referencia al boton de siguiente

	private Coroutine letterCor;							//Coroutine reference

	public float disspearSpeed = 8f;						//Velocidad de desaparicion
	public float delayOnDissapear = 2f;						//Retraso para desaparecer el tutorial
	private bool dissapearAnim;								//Indica si se realiza la animacion de desaparecer

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
		dissapearAnim = false;
	}
	
	// Update is called once per frame
	void Update () {
		//if (GameController.instance == null) return;

		if (Input.GetMouseButtonDown (0) && lucyTalking) { //End current text
			if (letterCor != null) {
				StopCoroutine (letterCor);
			}

			currentTextUI.text = "";
			for (int i = 0; i < auxString.Length; i++) { //Colocar todo el texto
				currentTextUI.text += (auxString [i] == '|') ? '\n' : auxString[i];
			}

			if (tuto0TalkID != 2 && tuto0TalkID != 4 && tuto0TalkID != 6 && tuto0TalkID != 8) {
				ButtonStatus (true);
			}

			lucyTalking = false;
			tuto0TalkID++;

			//Inicializar tiempo de espera para desaparecer
			if (tuto0TalkID	 == 5) {
				StartCoroutine (DisappearDelay ());
			}
		}

		if (dissapearAnim) {
			this.transform.localScale = Vector3.MoveTowards (this.transform.localScale, Vector3.zero, disspearSpeed * Time.deltaTime);
		}
	}

	public void PrepareTutorial0(int initial){
		onTutorial = true;
		currentTutorial = 0;
		tutoPortraitID0 = tutoDialogID0 = 0;
		currentStatus = 0;
		tuto0TalkID = initial;

		lucyTalking = true;

		//Show UI
		this.transform.localScale = Vector3.one;

		//Init texto
		currentTextUI.text = "";

		//Desactivar boton de siguiente
		ButtonStatus(false);

		//Empezar delay entre letras
		letterCor = StartCoroutine (LetterDelay ());

		//Start Positions
		portraitGO.transform.localPosition = new Vector3(tutorial0PortraitPos[tutoPortraitID0].x, tutorial0PortraitPos[tutoPortraitID0].y, 0f);
		dialogGO.transform.localPosition = new Vector3 (tutorial0DialogPos [tutoDialogID0].x, tutorial0DialogPos [tutoDialogID0].y, 0f);

		tutoDialogID0 = tutoPortraitID0 = 1;
	}

	public void OnClickNext(){
		if (tuto0TalkID == 1 || tuto0TalkID == 2) {
			lucyTalking = true;

			//Siguiente bloque
			tuto0TalkID = 2;

			//Init texto
			currentTextUI.text = "";

			//Desactivar boton de siguiente
			ButtonStatus(false);

			//Quitar mascara
			maskGO.SetActive(false);

			//Empezar delay entre letras
			letterCor = StartCoroutine (LetterDelay ());
		}
		else if (tuto0TalkID == 4 || tuto0TalkID == 6 || tuto0TalkID == 8) {
			lucyTalking = true;

			//Init texto
			currentTextUI.text = "";

			//Desactivar boton de siguiente
			ButtonStatus(false);

			//Quitar mascara
			maskGO.SetActive(false);

			//Empezar delay entre letras
			letterCor = StartCoroutine (LetterDelay ());
		}

        //Play Sound
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }
	}

	public void OnClickButton(int ID){
		if (ID == 0) {	//Payment done
			if (letterCor != null) {
				StopCoroutine (letterCor);
			}
			lucyTalking = true;

			//Siguente bloque
			tuto0TalkID = 3;

			//Init texto
			currentTextUI.text = "";

			//Desactivar boton de siguiente
			ButtonStatus(false);

			//Colocar mascara
			maskGO.SetActive(true);

			//Empezar delay entre letras
			letterCor = StartCoroutine (LetterDelay ());

			//Positions
			portraitGO.transform.localPosition = new Vector3(tutorial0PortraitPos[tutoPortraitID0].x, tutorial0PortraitPos[tutoPortraitID0].y, 0f);
			dialogGO.transform.localPosition = new Vector3 (tutorial0DialogPos [tutoDialogID0].x, tutorial0DialogPos [tutoDialogID0].y, 0f);
		} 
		else if (ID == 1) { //Credit
			if (letterCor != null) {
				StopCoroutine (letterCor);
			}
			lucyTalking = true;

			//Siguente bloque
			tuto0TalkID = 5;

			//Init texto
			currentTextUI.text = "";

			//Desactivar boton de siguiente
			ButtonStatus(false);

			//Colocar mascara
			maskGO.SetActive(true);

			//Empezar delay entre letras
			letterCor = StartCoroutine (LetterDelay ());
		}
		else {	//Cancel
			if (letterCor != null) {
				StopCoroutine (letterCor);
			}
			lucyTalking = true;

			//Siguente bloque
			tuto0TalkID = 7;

			//Init texto
			currentTextUI.text = "";

			//Desactivar boton de siguiente
			ButtonStatus(false);

			//Colocar mascara
			maskGO.SetActive(true);

			//Empezar delay entre letras
			letterCor = StartCoroutine (LetterDelay ());
		}
	}

	//Activa o desactiva el boton de siguiente
	void ButtonStatus(bool val){
		buttonNext.SetActive (val);
	}

	IEnumerator LetterDelay() {
		char auxChar; //Auxiliar
		//Inicializar variables
		int i = 0;

		//Asignar oracion
		auxString = tuto0Talk [tuto0TalkID];

		//Asignar letra por letra
		while (i < auxString.Length) {
			auxChar = (auxString[i] == '|') ? '\n' : auxString [i]; i++;
			currentTextUI.text += auxChar;

			//Esperar siguiente intervalo de tiempo
			yield return new WaitForSeconds (delayOnLetters);
		}

		//Lucy termina este bloque de texto
		if(tuto0TalkID != 2 && tuto0TalkID != 4 && tuto0TalkID != 6 && tuto0TalkID != 8) {
			ButtonStatus (true);
		}

		lucyTalking = false;
		tuto0TalkID++;

		if (tuto0TalkID == 5) {
			StartCoroutine (DisappearDelay ());
		}
	}

	IEnumerator DisappearDelay(){
		onTutorial = false;

		yield return new WaitForSeconds (delayOnDissapear);

		dissapearAnim = true;
	}
}

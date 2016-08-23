using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour {
	public GameObject introUI;				//Padre de la UI Intro
	public GameObject mainUI;				//Padre de los botones del main

	private float startTime;				//Tiempo de inicio del juego
	
	public Image welcomeDialogBox;			//Cuadro de dialogo inicial de Lucy
	public Text welcomeTextUI;				//Texto de bienvenida

	public Text introDialogTextUI;			//Texto de introduccion de nuevo juego
	public string[] introDialogText;		//Arreglo de textos de dialogo de introduccion
	private int introDialogID;				//ID de dialogo de introduccion
	private Coroutine letterDelay;			//Coroutine de retraso de las letras del intro

	private bool lucyTalking;				//Flag: Indica si Lucy esta hablando

	public float delayOnLetters = 0.01f;	//Tiempo entre aparacion de las letras

	private string auxString;				//String base a mostrar en el cuadro de dialogo inicial

	public bool showContinueButton;			//Indica si hay que mostrar el boton de continuar

    public Animator lucyAnimation;          //Referencia  interna al animator de Lucy

    public GameObject disableMusicGO;       //Imagen que indica si se esta escuchando o no musica/sonido

    public GameObject continueUIButton;     //Boton de continuar

    public GameObject confirmUI;            //UI de confirmacion de estado - Eliminar Save Data
	
	// Use this for initialization
	void Start () {
		//Inicializar variables
		introDialogID = 0;

		startTime = Time.time; //Set de tiempo inicial

		showContinueButton = false;

        if (MusicController.instance.MusicStatus()) {
            disableMusicGO.SetActive(false);
        }

        if (GameController.instance != null) {
            if (PlayerPrefs.GetInt("Month", -1) == -1) {
                continueUIButton.SetActive(false);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		/*if (animateDialogBox) { //Animacion de cuadro de dialogo
			//Desde escala = (0,0,0) a (1,1,1)
			welcomeDialogBox.rectTransform.localScale = Vector3.MoveTowards(welcomeDialogBox.rectTransform.localScale, 
			                                                                Vector3.one, 
			                                                                welcomeDialogSpeed * Time.deltaTime);
		}
		else if(Time.time > startTime + welcomeDialogDelay && !animateDialogBox) { //Comprobar si debe aparecer el cuadro de dialogo inicial
			animateDialogBox = true;

			StartCoroutine(LetterDelayIntro());
		}*/
	}

	public void OnClickStartNewGame(){
		//Cargar Introduccion
		//Application.LoadLevel ("(1) DemoIntroV1");

		Debug.Log ("New Game");

		//Hide Main UI
		mainUI.SetActive (false);

		//Mostrar dialogo de introduccion
		introUI.SetActive (true);

		//Texto introductorio
		letterDelay = StartCoroutine(LetterDelayNewGame());
		lucyTalking = true;

        //Animacion de Lucy hablando
        lucyAnimation.SetTrigger("Talk");

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayButtonSound ();
		}

        //Delete previous player prefs
        PlayerPrefs.DeleteAll();
	}

    public void OnClickContinueSavedGame() {
        //******DELAY
        if (GameController.instance == null) return;

        string sceneToLoad = GameController.instance.LoadDatafromPlayerPref();
        SceneManager.LoadScene(sceneToLoad);
    }

	//Se invoca cuando se clickea el boton settings
	public void OnClickSettings(){
		Debug.Log ("Settings");
	}

	public void OnClickContinue(){
		if (lucyTalking) { //Si Lucy esta hablando este boton muestra el texto completo
			lucyTalking = false;

            //Animacion de Lucy en espera
            lucyAnimation.SetTrigger("Idle");
			
			//Detener retraso de las letras
			StopCoroutine(letterDelay);
			introDialogTextUI.text = introDialogText[introDialogID++];
			introDialogTextUI.text = introDialogTextUI.text.Replace("|", "\n");

			return;
		}

		//Chequea si todavia hay texto que mostrar si no carga la siguiente escena
		if (introDialogID < introDialogText.Length) {
			letterDelay = StartCoroutine (LetterDelayNewGame ());
			lucyTalking = true;

            //Animacion de Lucy hablando
            lucyAnimation.SetTrigger("Talk");
		} 
		else {
			//Cargar nueva escena
			SceneManager.LoadScene("(1) MiniGame1V6");
		}

		//Sound
		if(MusicController.instance != null){
			MusicController.instance.PlayContinueSound ();
		}
	}

    //IEnumerator LetterDelayIntro(){
    //    //Asignar texto de bienvenida
    //    auxString = welcomeTextUI.text;
    //    welcomeTextUI.text = "";

    //    int i = 0;

    //    //Asignar letra por letra
    //    while (i < auxString.Length) {
    //        welcomeTextUI.text += auxString[i];
    //        i++;

    //        //Esperar siguiente intervalo de tiempo
    //        yield return new WaitForSeconds(delayOnLetters);
    //    }
    //}

	IEnumerator LetterDelayNewGame() {
		char auxChar; //Auxiliar

		//Asignar texto de introduccion
		introDialogTextUI.text = "";

		//Inicializar variables
		int i = 0;

		//Asignar oracion
		auxString = introDialogText[introDialogID];

		//Asignar letra por letra
		while (i < auxString.Length) {
			auxChar = (auxString[i] == '|') ? '\n' : auxString [i]; i++;
			introDialogTextUI.text += auxChar;
			
			//Esperar siguiente intervalo de tiempo
			yield return new WaitForSeconds (delayOnLetters);
		}

		//Lucy termina este bloque de texto
		lucyTalking = false;
		introDialogID++;

        //Animacion de Lucy en espera
        lucyAnimation.SetTrigger("Idle");
	}

    //Click en el boton de musica
    public void OnClickMusic() {
        if (MusicController.instance.MusicStatus()) {
            MusicController.instance.MuteMusic();

            disableMusicGO.SetActive(true);
        }
        else {
            MusicController.instance.PlayMusic();

            disableMusicGO.SetActive(false);
        }
    }

    public void OnClickClearSaveData() {
        confirmUI.SetActive(true);
    }

    public void OnClickConfirmYes() {
        confirmUI.SetActive(false);

        //Eliminar save data
        PlayerPrefs.DeleteAll();

        //Verificar que no existen entradas en PlayerPrefs y esconder el boton de continuar
        if (GameController.instance != null) {
            if (PlayerPrefs.GetInt("Month", -1) == -1) {
                continueUIButton.SetActive(false);
            }
        }
    }

    public void OnClickConfirmNo() {
        confirmUI.SetActive(false);
    }
}

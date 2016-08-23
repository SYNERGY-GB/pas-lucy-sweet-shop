using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndMonthController : MonoBehaviour {
	public Text cashTextUI;				//UI de Texto de capital disponible
	public Text taxTextUI;				//UI de Texto impuestos
	public Text netTextUI;				//UI de Texto ingreso neto
    public Text salesRevenueTextUI;     //UI de Texto ingreso de ventas
    public Text incomeNoTaxTextUI;      //UI de Texto ingreso sin impuestos

    public Text taxTitleTextUI;         //UI de Texto titulo de impuestos

    //Referencia interna de la animacion para cargar el siguiente mes
    public NextMonthAnimationController nextMAnimControl;
    public float nextSceneDelay = 2f;   //Tiempo de retraso para cargar la siguiente escena

    //Referencia al controlador de logs
    public BalanceLogController balanceLogController;

    public GameObject disableMusicGO;       //Referencia al objeto que indica si se esta reproduciendo musica o no

    public Animator endMonthAnimator;       //Referencia al animator de la UI principal de fin de mes

    public GameObject exitGameGO;           //Referencia interna de la interfaz de confirmacion para salir del juego.

	// Use this for initialization
	void Start () {
		InitEndMonth ();

        if (MusicController.instance != null) {
            if (MusicController.instance.MusicStatus()) {
                disableMusicGO.SetActive(false);
            }
            else {
                disableMusicGO.SetActive(true);
            }
		}

        //Mostrar UI
        endMonthAnimator.SetTrigger("Show");

        //Esconder otras UI
        exitGameGO.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Inicializar interfaz
	void InitEndMonth(){
		if (GameController.instance != null) {
            //Ingreso de ventas
            salesRevenueTextUI.text = GameController.instance.GetSalesRevenue().ToString("f2");

            //Ingreso sin impuestos
            incomeNoTaxTextUI.text = GameController.instance.GetSalesRevenue().ToString("f2");

            //Impuestos
            //taxTitleTextUI.text = "Taxes (%" + (GameController.instance.GetTaxRate() * 100f).ToString("f2") + ")";
            //taxTextUI.text = (GameController.instance.GetSalesRevenue() * (1f - GameController.instance.GetTaxRate())).ToString("f2");
            taxTextUI.text = (GameController.instance.GetTaxRate() * 100f).ToString("f0") + "%";

            //Mostrar ingreso neto
            netTextUI.text = (GameController.instance.GetSalesRevenue() * (1f - GameController.instance.GetTaxRate())).ToString("f2");

			//Actualizar capital del negocio
            GameController.instance.SetMoney(GameController.instance.GetMoney() - (GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate()));

            //Dinero total
            cashTextUI.text = GameController.instance.GetMoney().ToString("f2");

            //Balance Log
			balanceLogController.SetupLog("Taxes", "Cash", GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate());
		
			//Update bussiness status
			GameController.instance.AddActiveEntry ("Cash", -(GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate()));
			//GameController.instance.AddActiveEntry ("Taxes",  GameController.instance.GetSalesRevenue () * GameController.instance.GetTaxRate());
			GameController.instance.AddEquityEntry ("Common Stock", -(GameController.instance.GetSalesRevenue () * GameController.instance.GetTaxRate()));

			GameController.instance.UpdateActive(-(GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate())); //Active
			//GameController.instance.UpdatePassive(priceTotal); //Liability
			GameController.instance.UpdateLiability(-(GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate())); //Equity

            //Add new Month
            GameController.instance.AddNewMonth();
		}
	}

	public void OnClickNextMonth () {
        if (GameController.instance != null) GameController.instance.currentMonth += 1;
        DecisionsController.fromEndMonth = true;
        Debug.Log("Test Event - End Month: " + DecisionsController.fromEndMonth);

        //Esconder UI
        endMonthAnimator.SetTrigger("Hide");

        nextMAnimControl.StartAnimation();
        StartCoroutine(LoadDelay());

        //Sonido
		if(MusicController.instance != null) {
			MusicController.instance.PlayButtonSound ();
		}
	}

    IEnumerator LoadDelay() {
        yield return new WaitForSeconds(nextSceneDelay);

		if (GameController.instance != null) {
            Debug.Log("Month: " + GameController.instance.currentMonth);
			if (GameController.instance.currentMonth == 13) {
				SceneManager.LoadScene ("(8) EndGameV1");
                yield break;
			}
		}
        SceneManager.LoadScene("(3) DecisionsV4");
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

    public void OnClickExit() {
        exitGameGO.SetActive(true);
    }

    public void OnClickYesExit() {
        SceneManager.LoadScene("(0) DemoMainMenuV5");
    }

    public void OnClickNoExit() {
        exitGameGO.SetActive(false);
    }
}

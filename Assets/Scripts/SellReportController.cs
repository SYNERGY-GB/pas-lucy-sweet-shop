using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SellReportController : MonoBehaviour {
	public Text[] inventoryTextUI;			//Arreglo de UI de texto de cantidad de productos restantes en el inventario
	public Text[] soldTextUI;				//Arreglo de UI de texto de productos vendidos
	public Text[] priceTextUI;				//Arreglo de UI de texto de precio de los productos

	//ID = 0 - Cookie / 1 - Cake / 2 - Chocolate / 3 - Cupcake
	private float[] productPrice;			//Arreglo del precio de cada producto

	private int[] productAmountCant;		//Arreglo cantidad de productos vendidos

	public Text priceTotalUI;				//UI Texto de precio total ganado
	//public Text incomeTotalUI;				//UI Texto de ganancia + dinero actual

	public Text soldTotalUI;				//UI Texto cantidad de productos vendidos

    //Referencia interna del controlador de logs
    public BalanceLogController balanceLogController;

    public GameObject disableMusicGO;       //Referencia al objeto que indica si se esta reproduciendo musica o no

	//public Text invTotalUI;				//UI Texto cantidad de inventario total

    public Animator sellReportAnimator;     //Referencia del animator de la UI principal del reporte de ventas
    public float timeDelayNextScene = 1.8f; //Tiempo en segundos de cuanto se tarda en pasar a la siguiente escena

    public GameObject exitGameGO;           //Referencia interna de la interfaz de confirmacion para salir del juego.

	// Use this for initialization
	void Start () {
		//Inicializa tabla de reportes
		InitReportTable ();

		if (MusicController.instance != null) {
			//Setup Management Music
			MusicController.instance.ChangeMusic (1);

            if (MusicController.instance.MusicStatus()) {
                disableMusicGO.SetActive(false);
            }
            else {
                disableMusicGO.SetActive(true);
            }
		}

        //Mostrar UI
        sellReportAnimator.SetTrigger("Show");

        //Esconder otras UI
        exitGameGO.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void InitReportTable(){
		if (GameController.instance == null)
			return;

		float baseIncome = 0f;
        int invNewProductCount = 0;
		int[] invOld = GameController.instance.GetInventory ();
		int[] invNew = GameController.instance.GetProductCounterInt ();
		float[] price = GameController.instance.GetSellPriceArray ();

        ////Actualizar UI de inventario (inventory)
        //for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
        //    inventoryTextUI[i].text = inv[i].ToString("d0");
        //}

		//Actualizar UI de vendido (sold)
		for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
			soldTextUI[i].text = (invOld[i] - invNew[i]).ToString("d0");
            invNewProductCount += invOld[i] - invNew[i]; //Cuenta total de cuantos productos quedan en el inventario
		}
        GameController.instance.SetInventory(invNew); //Actualizar inventario

		//Actualizar UI de precio (price)
		for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
			//Debug.Log("Parse: " + int.Parse(soldTextUI[i].text));
			//Debug.Log (" >" + soldTextUI[i].text + "< Length: " + soldTextUI[i].text.Length);
			priceTextUI[i].text = (int.Parse(soldTextUI[i].text) * price[i]).ToString("f2");
		}

		float totalAux = 0f;
		//Actualizar precio total
		for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
			totalAux += float.Parse(priceTextUI[i].text);
			baseIncome += (float.Parse(soldTextUI[i].text) > 0f) ? float.Parse(soldTextUI[i].text) * GameController.instance.GetProductBaseBuyPrice(i) : 0f;
		}
		priceTotalUI.text = totalAux.ToString("f2");
        GameController.instance.SetSalesRevenue(totalAux);

		//Actualizar vendido total
		totalAux = 0f;
		for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
			totalAux += float.Parse(soldTextUI[i].text);
		}
		soldTotalUI.text = totalAux.ToString("f0");

        ////Actualizar inventario total
        //totalAux = 0f;
        //for (int i = 0; i < GameController.instance.GetMaxProducts(); i++) {
        //    totalAux += int.Parse(inventoryTextUI[i].text);
        //}
        //invTotalUI.text = totalAux.ToString("f0");

		//Ganancia + balance actual
		GameController.instance.SetMoney (float.Parse (priceTotalUI.text) + GameController.instance.GetMoney ());
		//incomeTotalUI.text = GameController.instance.GetMoney ().ToString("f2");

        //Setup balance log
		balanceLogController.SetupLog("Cash", "Inventory", float.Parse(priceTotalUI.text));

        //Update bussiness status
		GameController.instance.AddActiveEntry ("Cash", float.Parse(priceTotalUI.text));
		GameController.instance.AddActiveEntry ("Inventory", -baseIncome);
		GameController.instance.AddEquityEntry ("Common Stock", float.Parse(priceTotalUI.text) - baseIncome);

		//Actualizar status de la empresa
		GameController.instance.UpdateActive(float.Parse(priceTotalUI.text) - baseIncome); //Active
		//GameController.instance.UpdatePassive(priceTotal); //Liability
		GameController.instance.UpdateLiability(float.Parse(priceTotalUI.text) - baseIncome); //Equity
	}

	//Al hacer clic en el boton EndMonth se ejecuta esta funcion
	public void OnClickEndMonth() {
        //Esconder UI
        sellReportAnimator.SetTrigger("Hide");

        StartCoroutine(delayToNextScene());

        //Sonido
		if(MusicController.instance != null) {
			MusicController.instance.PlayButtonSound ();
		}
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

    IEnumerator delayToNextScene() {
        yield return new WaitForSeconds(timeDelayNextScene);

        SceneManager.LoadScene("(6) EndMonthV1");
    }
}

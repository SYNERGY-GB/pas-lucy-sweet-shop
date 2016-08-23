using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DecisionsController : MonoBehaviour {
	public static bool fromEndMonth;		//Bandera que indica si se viene desde la interfaz de eventos
    public static bool fromLoadData;        //Bandera que indica si se viene desde la data guarda de una partida anterior

	public GameObject checkOutUI;			//Padre de la interfaz Check Out
	public GameObject decisionsUI;			//Padre de la interfaz Management
	public GameObject balanceUI;			//Padre de la interfaz Balance Sheet
	public GameObject inventoryUI;			//Padre de la interfaz Inventory

	//ID = 0 - Cookie / 1 - Cake / 2 - Chocolate / 3 - Cupcake
	private float[] productPrice;			//Arreglo del precio de cada producto
	public Text[] productPriceText;			//Arreglo de UI texto de precio

	private int[] productAmountCant;		//Arreglo cantidad de productos obtenidos
	public InputField[] productAmountInput;	//Arreglo de elementos UI input para cantidad de productos a comprar

	private int[] inventoryCounter;			//Arreglo de cantidad de productos en el inventario
	private int[] maxInventory;				//Arreglo de capacidad maxima del inventario
	public Text[] maxInventoryText;			//Arreglo de elementos UI texto del maximo del inventario para cada producto

	public Text[] inventoryUIAmount;		//Arreglo UI de cantidad de inventario
	public Text[] inventoryUIBuyPrice;		//Arreglo UI de precio de compra
	public Text[] inventoryUIProfit;		//Arreglo UI de profit (venta - compra = profit)
    public InputField[] inventoryUISell;    //Arreglo UI de campo de precio de venta
    public Slider inventoryUISatisfaction;  //Slider de satisfaccion de clientes
    public Text inventoryUISatisText;       //Texto de satisfaccion del cliente
	//Ratio de aumento/decremento
	public float inventoryIncreaseRatio = 1f;

	private float priceTotal;				//Total a pagar
	public Text priceTotalText;				//UI Text para pagar el total

	private int amountTotal;				//Cantidad total de productos a comprar
	public Text amountTotalText;			//UI Text de total de productos a comprar

	public Text deductedTotalText;			//UI Text total de cuanto dinero queda disponible restando el pago a realizar

	public Text sellProductsText;			//UI Text texto de boton de venta
    public Text sellProductsShadowText;		//UI Text texto de la sombra del boton de venta

	public Toggle cashToggle;				//UI Toggle usar capital para comprar
	public Toggle creditToggle;				//UI Toggle usar credito para comprar

	public float creditProbability = 0.9f;	//Probabilidad de adquirir el credito

    public GameObject creditAskUI;          //UI de pedir credito
    public Text creditAskInterest;          //UI Text de interes del credito a padir
    public Text creditAskDebit;             //UI Text de total de deuda a pagar
	public GameObject creditAskYes;			//UI credito aprovado
	public GameObject creditAskNo;			//UI credito negado

	public GameObject creditUI;				//UI de credito
	public Text creditTextTitleUI;			//UI Text de la deuda actual
	public InputField creditDebtInputUI;	//UI Input Field
	private float creditDebtInput;			//Cantidad de credito a pagar

	public GraphController graphController;	//Controlador del grafo
    public TopUIController topUIController; //Controlador del la interfaz de arriba
	public EventController eventController;	//Controlador de los eventos
    public BalanceLogController balanceLogController; //Controlador de los logs de balance

	public Animator billAnimator;			//Referencia interna del animator de la factura
	public Animator mainAnimator;			//Referencia interna del animator de la tablet y el notepad
    public Animator balanceSheetAnimator;   //Referencia interna del animator de la hoja de balance
    public Animator inventoryAnimator;      //Referencia interna del animator del inventario
	public Animator creditAnimator;			//Referencia interna del animator de credito

	public float mainAnimationDelay = 1f;	//Delay de la animacion principal

	public GameObject disableSoundGO;		//Imagen que indica si se escucha musica o no
    public GameObject blackBackgroundGO;    //Imagen de fondo que bloquea el input del mouse ademas de oscurecer la UI

    public GameObject exitGameGO;           //Referencia interna de la interfaz de confirmacion para salir del juego.

    public GameObject alertUIGO;            //Interfaz de alerta
    public Text alertUIText;                //Texto descriptivo de la interfaz de alerta

	void Awake() {
		
	}

	// Use this for initialization
	void Start () {
        int[] _product;

		checkOutUI.SetActive (true);
		decisionsUI.SetActive (true);
		balanceUI.SetActive (true);
        inventoryUI.SetActive(true);
		creditUI.SetActive (true);

        blackBackgroundGO.SetActive(false);
        exitGameGO.SetActive(false);
        alertUIGO.SetActive(false);

		creditAskUI.SetActive(false);

		if (fromEndMonth) {
			checkOutUI.SetActive (false);
			//decisionsUI.SetActive (true);

			//Animacion de aparicion
			mainAnimator.SetTrigger ("Show");

			sellProductsText.text = "Buy";
            sellProductsShadowText.text = "Buy";

			graphController.LoadValues ();

            if (GameController.instance != null) {
			    if (GameController.instance.TestEvent(fromEndMonth) && !fromLoadData) {
				    eventController.ActiveEvent ();
			    }
		    }
		} 
		else { //Mostrar checkout
			//decisionsUI.SetActive (false);

			//Animacion de aparacion
			billAnimator.SetTrigger("Show");

			//Animacion de esconder
			//mainAnimator.SetTrigger("Hide");

			sellProductsText.text = "Sell";
            sellProductsShadowText.text = "Sell";
		}

        if (fromLoadData) {
            fromLoadData = false;
            checkOutUI.SetActive(false);
            //decisionsUI.SetActive (true);

            //Animacion de aparicion
            mainAnimator.SetTrigger("Show");

            //Esconder otras UIs
            billAnimator.SetTrigger("Hide");

            //Preparar UI
            graphController.LoadValues ();
        }

		//Asignar cantidad de productos a comprar
		if (GameController.instance != null) {
            //Mostrar tutorial
			if(Tutorial0Controller.instance != null && GameController.instance.GetCurrentMonth() == 1){
                bool _found = false;
                _product = GameController.instance.GetProductCounterInt();
                for (int i = 0; i < _product.Length; i++) { //Chequear si el jugador obtuvo productos
                    if (_product[i] > 0) _found = true;
                    _product[i] = 1;
                }

                if (!_found) { //Si no obtuvo productos
                    GameController.instance.SetProductCounter(_product);

                    Tutorial0Controller.instance.PrepareTutorial0(1);
                }
                else { //Si obtuvo al menos uno
                    Tutorial0Controller.instance.PrepareTutorial0(0);
                }
			}

			productAmountCant = GameController.instance.GetProductCounterInt ();
			productPrice = GameController.instance.GetBuyPriceArray ();

			inventoryCounter = GameController.instance.GetInventory ();
			maxInventory = GameController.instance.GetMaxInventory ();

			for (int i = 0; i < productAmountInput.Length; i++) {
				//Asignar maximo de producto que se pueden obtener
				maxInventory [i] -= inventoryCounter [i];
				maxInventoryText [i].text = maxInventory [i].ToString ("d2");

				//Asignar valores maximos de compra de cada producto
				productAmountCant [i] = (productAmountCant [i] < maxInventory [i]) ? productAmountCant [i] : maxInventory [i];
				productAmountInput [i].text = productAmountCant [i].ToString ("d2");

				//Asignar precio de cada producto
				productPriceText [i].text = productPrice [i].ToString ("f2");
			}

			//Calcular valor total a pagar
			CalculateTotalValues ();

			//Inicializar capital
            topUIController.SetMoney(GameController.instance.GetMoney ());

			//Inicializar mes actual
			topUIController.SetMonth(GameController.instance.GetCurrentMonth ());

			//Satisfaccion del cliente
			inventoryUISatisfaction.value = GameController.instance.GetSatisfaction();
		} 
		else {
			//Inicializar capital
            topUIController.SetMoney(1f);

			//Inicializar mes actual
			topUIController.SetMonth(15);

			//Mostrar tutorial
			//Tutorial0Controller.instance.PrepareTutorial0 (0);

			//Satisfaccion del cliente
			inventoryUISatisfaction.value = 1f;
		}
		//Inicializar credito
		/*currentCreditText.text = "$" + GameController.instance.GetMoney ().ToString () + 
								 "/+" + (GameController.instance.GetCreditRatio () * 100f).ToString () + "%";*/

		//Inicializar toggle UIs
		//cashToggle.isOn = false;
		//creditToggle.isOn = false;

		//Disable / Enable Music
		if (MusicController.instance != null) {
			if (MusicController.instance.MusicStatus ()) {
				disableSoundGO.SetActive (false);
			} 
			else {
				disableSoundGO.SetActive (true);
			}

			//Setup Management Music
			MusicController.instance.ChangeMusic(1);
		} 
		else {
			disableSoundGO.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown (0)) {
        //    Debug.Log (Input.mousePosition);
        //}
	}

	//Se usa para realizar el pago para comprar los productos
	public void OnClickPayment () {
		if (Tutorial0Controller.onTutorial) {
			Tutorial0Controller.instance.OnClickButton (0);
		}

        //Chequear si se tiene suficiente dinero para comprar
        if(GameController.instance != null) {
            if (priceTotal > GameController.instance.GetMoney()) {
                Debug.Log("Not enough money");
                OnAlertActive(true, "Not enough money");
                return;
            }
        }

		//Debug.Log ("GM: " + GameController.instance);
		if (GameController.instance != null) {
            Debug.Log("Test Event: " + fromEndMonth);
			if (GameController.instance.TestEvent(fromEndMonth)) {
				eventController.ActiveEvent ();
			}
		}

		//Esconder Check Out interfaz / Mostrar Desiciones
		//decisionsUI.SetActive (true);
		//checkOutUI.SetActive (false);
		billAnimator.SetTrigger ("Hide");
		StartCoroutine (DecisionDelay ());

		//Debug.Log("Log: " + productAmountInput[0].text);

		//Actualizar inventario
		if(GameController.instance == null) return;
		int[] newInv = GameController.instance.GetInventory ();
		for (int i = 0; i < newInv.Length; i++) {
			newInv[i] += int.Parse(productAmountInput[i].text);
		}
		GameController.instance.SetInventory (newInv);

        //Reducir dinero usado
        GameController.instance.SetMoney(GameController.instance.GetMoney() - priceTotal);
        topUIController.UpdateMoney(-priceTotal);

        //Mostrar Balance Log
        balanceLogController.SetupLog("Cash", "Inventory", priceTotal);

		//Actualizar status de la empresa
        //GameController.instance.UpdatePassive(priceTotal); //Liability
        //GameController.instance.UpdateLiability(-priceTotal); //Equity

        GameController.instance.AddActiveEntry("Cash", -priceTotal);
        GameController.instance.AddActiveEntry("Inventory", priceTotal);

		graphController.LoadValues ();
	}

	public void OnClickCredit(){
		if (Tutorial0Controller.onTutorial) {
			Tutorial0Controller.instance.OnClickButton (1);
			return;
		}

        //Sonido de boton
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }

        //Activar evento al finalizar el flujo del credito **CHANGE THIS**
		/*if (GameController.instance != null) {
			if (GameController.instance.GetCurrentMonth () == 2) {
				eventController.ActiveEvent ();
			}
		}*/

        creditAskUI.SetActive(true);

        SetupCreditAskUI();
	}
    
    //Se pide el credito
    public void OnClickAskCredit() {
        //Si la cantidad de credito a pedir es menor al equity "Common Stock" de la empresa del mes actual
		if (GameController.instance.GetDebt () + priceTotal * (1f + GameController.instance.GetCreditRatio ()) < GameController.instance.GetEquity("Common Stock")) { //Credito aprobado
			creditAskYes.SetActive(true);

            //Actualizar inventario
            int[] newInv = GameController.instance.GetInventory ();
		    for (int i = 0; i < newInv.Length; i++) {
			    newInv[i] += int.Parse(productAmountInput[i].text);
		    }
		    GameController.instance.SetInventory (newInv);

			//Añadir deuda
			GameController.instance.SetDebt (GameController.instance.GetDebt () + priceTotal * (1f + GameController.instance.GetCreditRatio ()));

			//Mostrar Balance Log
			balanceLogController.SetupLog("Notes Payable", "Inventory", priceTotal * (1f + GameController.instance.GetCreditRatio ()));

            GameController.instance.AddActiveEntry("Inventory", priceTotal);
            GameController.instance.AddLiabilityEntry("Credit Debt", priceTotal * (1f + GameController.instance.GetCreditRatio ()));
            GameController.instance.AddEquityEntry("Common Stock", -priceTotal * (GameController.instance.GetCreditRatio()));

			//Actualizar status de la empresa
            GameController.instance.UpdateActive(priceTotal); //Assets
            GameController.instance.UpdatePassive(priceTotal * (1f + GameController.instance.GetCreditRatio())); //Liability
            GameController.instance.UpdateLiability(-priceTotal * (GameController.instance.GetCreditRatio())); //Equity
		}
		else { //Credito negado
			creditAskNo.SetActive(true);
		}

        if (GameController.instance != null) {
			if (GameController.instance.TestEvent(fromEndMonth)) {
				eventController.ActiveEvent ();
			}
		}

        //Sonido de boton
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }
    }

    //Se cancela la accion de pedir un credito
    public void OnClickBackCredit() {
        creditAskUI.SetActive(false);

        //Sonido de boton
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }
    }

    //Se mostro el mensaje y se presiono continuar
    public void OnClickCreditContinue() {
        if(creditAskYes.activeInHierarchy){
		    creditAskYes.SetActive (false);
		    creditAskNo.SetActive (false);
		    creditAskUI.SetActive (false);

		    //Esconder Check Out interfaz / Mostrar Desiciones
		    //decisionsUI.SetActive (true);
		    //checkOutUI.SetActive (false);
		    billAnimator.SetTrigger ("Hide");
		    StartCoroutine (DecisionDelay ());

		    if (GameController.instance != null) {
			    if (GameController.instance.TestEvent(fromEndMonth)) {
				    eventController.ActiveEvent ();
			    }
		    }
        }
        else if (creditAskNo.activeInHierarchy) {
            creditAskYes.SetActive(false);
            creditAskNo.SetActive(false);
            creditAskUI.SetActive(false);
        }

        //Sonido de boton
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }

        graphController.LoadValues();
    }

	//Si el jugador no quiere comprar nada
	public void OnClickCancelPayment(){
		if (Tutorial0Controller.onTutorial) {
			Tutorial0Controller.instance.OnClickButton (2);
			return;
		}

		if (GameController.instance != null) {
			if (GameController.instance.TestEvent(fromEndMonth)) {
				eventController.ActiveEvent ();
			}
		}

		//Esconder Check Out interfaz / Mostrar Desiciones
		//decisionsUI.SetActive (true);
		//checkOutUI.SetActive (false);
		billAnimator.SetTrigger ("Hide");
		StartCoroutine (DecisionDelay ());

        //Sonido de boton
        if (MusicController.instance != null) {
            MusicController.instance.PlayButtonSound();
        }

		graphController.LoadValues ();
	}

	//Mostrar / Esconder interfaz de credito
	public void OnClickPayCredit(){
		//creditUI.SetActive (!creditUI.active);
		creditAnimator.SetTrigger("Show");
		blackBackgroundGO.SetActive (true);

		SetupPayCreditUI ();
	}

	public void OnClickPayExitCredit(){
		creditAnimator.SetTrigger ("Hide");
		blackBackgroundGO.SetActive (false);
	}

	//Se llama cuando se clikea "Sell Products"
	public void OnClickSellProducts(){
		//Cargar minijuego
		if(fromEndMonth) { //Compra
			fromEndMonth = false;
			SceneManager.LoadScene(GameController.instance.GetBuyMinigameName());
		}
		else { //Venta
			fromEndMonth = false;
			SceneManager.LoadScene(GameController.instance.GetSellMinigameName());
		}
	}

	//Validar que existe solo un metado de pago seleccionado
	public void OnClickToggle(int ID){
		if (ID == 0 && cashToggle.isOn && creditToggle.isOn) {
			creditToggle.isOn = false;
		}
		else if (ID == 1 && cashToggle.isOn && creditToggle.isOn) {
			cashToggle.isOn = false;
		}
	}

	//Verifica que el cambio de la cantidad introducida es valido
	public void OnValueEndInputField(int ID){
		Debug.Log ("Log: " + productAmountInput [ID].text + " " + null);

		//Si es null o vacio
		if (productAmountInput [ID].text == "") {
			productAmountInput[ID].text = "00";
		}
		//Si es menor que 0
		else if(int.Parse (productAmountInput [ID].text) < 0) {
			productAmountInput[ID].text = "00";
		}
		//Si es mayor que la cantidad maxima
		else if (productAmountCant [ID] < int.Parse (productAmountInput [ID].text)) {
			productAmountInput[ID].text = productAmountCant[ID].ToString("d2");
		}
		else {
			productAmountInput[ID].text = int.Parse (productAmountInput [ID].text).ToString("d2");
		}

		CalculateTotalValues ();
	}

	void CalculateTotalValues(){
		//Total a pagar & cantidad de productos a comprar
		priceTotal = 0f; amountTotal = 0;
		for(int i = 0; i < productAmountInput.Length; i++) {
			priceTotal += float.Parse (productAmountInput [i].text) * productPrice[i];

			amountTotal += int.Parse (productAmountInput [i].text);
		}
		priceTotalText.text = priceTotal.ToString("f2");
		amountTotalText.text = amountTotal.ToString("d2");

		//Asignar precio deductivo
		deductedTotalText.text = (GameController.instance.GetMoney () - priceTotal).ToString("f2");
	}

	public void OnClickIncreaseAmount(int ID){
        //Si se esta en tutorial, no modificar estos valores
        if (Tutorial0Controller.onTutorial) return;

		if (productAmountCant [ID] >= int.Parse (productAmountInput [ID].text) + 1) {
            //Modify amount to buy
			productAmountInput[ID].text = (int.Parse (productAmountInput [ID].text) + 1).ToString("d2");

            //Recalculate totals
            CalculateTotalValues();
		}
	}

	public void OnClickDecreaseAmount(int ID){
        //Si se esta en tutorial, no modificar estos valores
        if (Tutorial0Controller.onTutorial) return;

		if (0 <= int.Parse (productAmountInput [ID].text) - 1) {
            //Modify amount to buy
			productAmountInput[ID].text = (int.Parse (productAmountInput [ID].text) - 1).ToString("d2");

            //Recalculate totals
            CalculateTotalValues();
		}
	}

	public void OnClickBalance(){
		//balanceUI.SetActive (true);

        //Setup balanceSheet
		balanceUI.GetComponent<BalanceSheetController>().ResetPivots();
        balanceUI.GetComponent<BalanceSheetController>().SetupBalanceSheet();

        balanceSheetAnimator.SetTrigger("Show");
        blackBackgroundGO.SetActive(true);
	}

	public void OnClickExitBalance(){
		//balanceUI.SetActive (false);

        balanceSheetAnimator.SetTrigger("Hide");
        blackBackgroundGO.SetActive(false);
	}

	//Muestra inventario
	public void OnClickInventory(){
		//decisionsUI.SetActive (false);
		//inventoryUI.SetActive (true);

        mainAnimator.SetTrigger("Hide");
        inventoryAnimator.SetTrigger("Show");

		SetupInventory ();
        SetupSatisfaction ();
	}

	//Esconde inventario
	public void OnClickBackInventory(){
		//decisionsUI.SetActive (true);

        //mainAnimator.SetTrigger("Show");
		StartCoroutine(DecisionDelay());
        inventoryAnimator.SetTrigger("Hide");

		//inventoryUI.SetActive (false);
	}

	//Asignar valores al inventario
	void SetupInventory(){
		int[] inventoryCant;
		float[] inventoryPriceBuy, inventoryPriceSell;

		if(GameController.instance != null) {
			inventoryCant = GameController.instance.GetInventory ();

			for (int i = 0; i < inventoryCant.Length; i++) { //Cantidad de productos
				inventoryUIAmount [i].text = inventoryCant [i].ToString ("d0");
			}

            inventoryPriceSell = GameController.instance.GetSellPriceArray();	//Asignar precio de venta
			for(int i = 0; i < inventoryUIProfit.Length; i++){
                inventoryUISell[i].text = inventoryPriceSell[i].ToString("f2");
			}

            inventoryPriceBuy = GameController.instance.GetBuyPriceArray(); 	//Asignar precio de compra
			for(int i = 0; i < inventoryUIBuyPrice.Length; i++){
                inventoryUIBuyPrice[i].text = inventoryPriceBuy[i].ToString("f2");
			}

            //inventoryPrices = GameController.instance.GetSellPriceArray ();	//Asignar profit
			for(int i = 0; i < inventoryUISell.Length; i++){
                inventoryUIProfit[i].text = (inventoryPriceSell[i] - inventoryPriceBuy[i]).ToString("f2");
			}
		}
	}

    void SetupCreditAskUI() {
        if (GameController.instance != null) {
            creditAskInterest.text = "Interest: +" + (GameController.instance.GetCreditRatio() * 100f).ToString("f0") + "%";
            creditAskDebit.text = "Debt: $" + (priceTotal * (1f + GameController.instance.GetCreditRatio())).ToString("f2");
        }
        else {
            creditAskInterest.text = "Interest: +124%";
            creditAskDebit.text = "Debt: $1234.56";
        }
    }

	void SetupPayCreditUI(){
		if (GameController.instance != null) {
			creditTextTitleUI.text = "Debt: $" + GameController.instance.GetDebt ().ToString("f2");

			//Cantidad de credito a pagar
			creditDebtInput = (GameController.instance.GetDebt () / 2f);
			creditDebtInputUI.text = "$" + creditDebtInput.ToString ("f2");
		}
	}

	//Aumenta la cantidad de credito a pagar
	public void OnClickIncreaseCreditPayAmount(float inc) {
		if (GameController.instance == null) {
			Debug.Log ("No Increment: " + inc);
			return;
		}

		creditDebtInput += inc;

		if (creditDebtInput > GameController.instance.GetDebt ()) {
			creditDebtInput = GameController.instance.GetDebt ();
		}
		creditDebtInputUI.text = "$" + creditDebtInput.ToString ("f2");
	}

	//Reduce la cantidad de credito a pagar
	public void OnClickDecreaseCreditPayAmount(float inc) {
		if (GameController.instance == null) {
			Debug.Log ("No Decrement: " + inc);
			return;
		}

		creditDebtInput -= inc;

		if (creditDebtInput < 0f) {
			creditDebtInput = 0f;
		}
		creditDebtInputUI.text = "$" + creditDebtInput.ToString ("f2");
	}

	//Se invoca cuando se termina de editar el texto de cantidad a pagar
	public void OnEndEditCreditPayAmount() {
		//Debug.Log ("Pay Amount: " + creditDebtInputUI.text);
		creditDebtInput = float.Parse (creditDebtInputUI.text);
		if (creditDebtInput > GameController.instance.GetDebt ()) {
			creditDebtInput = GameController.instance.GetDebt ();
		}
		else if (creditDebtInput < 0f) {
			creditDebtInput = 0f;
		}

		creditDebtInputUI.text = "$" + creditDebtInput.ToString ("f2");
	}

	//Cuando el jugador el monto establecido en el Input Field
	public void OnClickPayCreditOnCreditUI() {
		if (creditDebtInput <= 0f)
			return;

		if (GameController.instance != null) {
			if (GameController.instance.GetDebt () <= 0f) //Si no existe deuda
				return;

            topUIController.UpdateMoney(-creditDebtInput);

			//Mostrar Balance Log
			balanceLogController.SetupLog("Cash", "Notes Payable", creditDebtInput);

			GameController.instance.SetMoney (GameController.instance.GetMoney () - creditDebtInput);
			GameController.instance.SetDebt (GameController.instance.GetDebt () - creditDebtInput);

            GameController.instance.AddLiabilityEntry("Credit Debt", -creditDebtInput);
			GameController.instance.AddActiveEntry("Cash", -creditDebtInput);

			GameController.instance.UpdateActive(-creditDebtInput); //Active
			GameController.instance.UpdatePassive(-creditDebtInput); //Liability
			//GameController.instance.UpdateLiability(-(GameController.instance.GetSalesRevenue() * GameController.instance.GetTaxRate())); //Equity

            //GameController.instance.AddEquityEntry("Common Stock", creditDebtInput);
		}

		//Esconder UI
		//creditUI.SetActive (false);
		creditAnimator.SetTrigger("Hide");
		blackBackgroundGO.SetActive (false);

		//Recargar grafico
		graphController.LoadValues ();
	}

	IEnumerator DecisionDelay(){
		//Debug.Log ("Gotta make it");
		yield return new WaitForSeconds (mainAnimationDelay);

		mainAnimator.SetTrigger ("Show");
	}

    //Click en el boton de musica
    public void OnClickMusic() {
        if (MusicController.instance.MusicStatus()) {
            MusicController.instance.MuteMusic();

            disableSoundGO.SetActive(true);
        }
        else {
            MusicController.instance.PlayMusic();

            disableSoundGO.SetActive(false);
        }
    }

    //Satisfaction Functions
    void SetupSatisfaction() {
        if(GameController.instance != null) {
            inventoryUISatisfaction.value = GameController.instance.GetSatisfaction();
        }

        inventoryUISatisText.text = "Customer Satisfaction: " + (100f * inventoryUISatisfaction.value).ToString("f0") + "%";
    }

    public void OnChangeSatisfactionValue() {
        //if (GameController.instance != null) {
        //    GameController.instance.SetSatisfaction(inventoryUISatisfaction.value);
        //}

        Debug.Log("ChangeSatisfactionValue: " + (100f * GameController.instance.GetSatisfaction()).ToString("f0"));

        inventoryUISatisText.text = "Customer Satisfaction: " + (100f * GameController.instance.GetSatisfaction()).ToString("f0") + "%";
    }

	//Aumenta el precio del producto indicado
	public void OnClickIncreasePrice(int ID) {
		//Check borderline case (Check if reached maximum decresable value)
		if(inventoryUISatisfaction.value <= 0.1f) return;

        //Change input field value
        inventoryUISell[ID].text = (float.Parse(inventoryUISell[ID].text) + inventoryIncreaseRatio).ToString("f2");

        //Change profit value
        inventoryUIProfit[ID].text = (float.Parse(inventoryUISell[ID].text) - float.Parse(inventoryUIBuyPrice[ID].text)).ToString("f2");

		//Asign new price for selling
		if (GameController.instance != null) {
			GameController.instance.SetSellPrice (ID, float.Parse (inventoryUISell [ID].text));

			//Change satisfaction value (on price increase lower satisfaction)
			float totalBase = 0, totalNew = 0;
			for (int i = 0; i < inventoryUISell.Length; i++) {
				totalBase += GameController.instance.GetProductBaseSellPrice (i);
				totalNew += float.Parse (inventoryUISell [i].text);
			}

            //Change satisfaction
            if(totalBase < totalNew) {
                GameController.instance.SetSatisfaction(GameController.instance.GetSatisfaction() - 0.01f);
                inventoryUISatisfaction.value = Mathf.Clamp01(GameController.instance.GetSatisfaction());
            }

			if (GameController.instance.GetSatisfaction () < 0.1f) { //Satisfaction is less that 10% -> set to 10%
				GameController.instance.SetSatisfaction(0.1f);
				inventoryUISatisfaction.value = 0.1f;
			}

			OnChangeSatisfactionValue ();
		}
	}

	//Reduce el precio del producto indicado
	public void OnClickDecreasePrice(int ID) {
		//Check borderline case (Check if reached maximum decresable value)
		//if(inventoryUISatisfaction.value >= 1.5f) return;

        //Change input field value
        inventoryUISell[ID].text = (float.Parse(inventoryUISell[ID].text) - inventoryIncreaseRatio).ToString("f2");

        //Change profit value
        inventoryUIProfit[ID].text = (float.Parse(inventoryUISell[ID].text) - float.Parse(inventoryUIBuyPrice[ID].text)).ToString("f2");

		//Asign new price for selling
		if (GameController.instance != null) {
			GameController.instance.SetSellPrice (ID, float.Parse (inventoryUISell [ID].text));

			//Change satisfaction value (on price increase lower satisfaction)
			float totalBase = 0, totalNew = 0;
			for (int i = 0; i < inventoryUISell.Length; i++) {
				totalBase += GameController.instance.GetProductBaseSellPrice (i);
				totalNew += float.Parse (inventoryUISell [i].text);
			}

            //Change satisfaction
			GameController.instance.SetSatisfaction (GameController.instance.GetSatisfaction() + 0.01f);
            inventoryUISatisfaction.value = Mathf.Clamp01(GameController.instance.GetSatisfaction());

			if (GameController.instance.GetSatisfaction () > 1f) { //Satisfaction is more that 100% -> set to 100%
				//GameController.instance.SetSatisfaction(1f);
				inventoryUISatisfaction.value = 1f;
			}

			OnChangeSatisfactionValue ();
		}
	}

    public void OnClickExit() {
        exitGameGO.SetActive(true);
    }

    public void OnClickYesExit() {
        //Save Game
        GameController.instance.SaveDatatoPlayerPref();

        SceneManager.LoadScene("(0) DemoMainMenuV5");
    }

    public void OnClickNoExit() {
        exitGameGO.SetActive(false);
    }

    public void OnAlertActive(bool status, string description = "") {
        alertUIGO.SetActive(status);
        alertUIText.text = description;
    }

    public void OnAlertContinue() {
        alertUIGO.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	public static GameController instance;		//Singleton

    private float currentDebt;                  //Deuda de credito
	private float moneyCapital;					//Dinero total del negocio

	private List<int> productCounter;			//Lista de cantidad de productos para comprar

	private List<int> inventoryMax;				//Lista de cantidad maxima de productos en el inventario
	private List<int> inventoryCounter;			//Lista de cantidad de productos en el inventario

	//ID = 0 - Cookie / 1 - Cake / 2 - Chocolate / 3 - Cupcake
	public float[] productSellPrice;			//Arreglo del precio de venta de cada producto
	public float[] productBuyPrice;				//Arreglo de precio de compra de cada producto
	private float[] productBaseSellPrice;		//Arreglo de precios bases de venta y compra respectivamente
	private float[] productBaseBuyPrice;		
	
	private int maxProducts = 4;				//Cantidad maxima de productos disponibles

	private float bankCreditRate;				//Porcentaje de credito / Range(0f, 1f)

	private float taxRate;						//Porcentaje de impuestos / Range(0f, 1f)

	public int currentMonth = 1;				//Mes actual

	public string[] minigameBuyName;			//Nombre de las escenas contenedoras de minijuegos de compra
	public string[] minigameSellName;			//Nombre de las escenas contenedoras de minijuegos de venta

	private List<float> creditList;				//Lista de todos los creditos que posee el jugador

    private float salesRevenue;                 //Contiene los ingresos de ventas del mes actual

	private float currentProbEvent;				//Probabilidad de que un evento ocurra
	public float eventProRatio;					//Ratio de incremento de la probabilidad de aparicion de un evento

	//Lista de valores activos, pasivos y patrimonio
	public List<float> activeTotal, passiveTotal, liabilityTotal;

    //Listas de activos
    private List<string> activeName;
    private List<float> activeValue;

    //Listas de liability
    private List<string> liabilityName;
    private List<float> liabilityValue;

    //Listas de equity
    private List<string> equityName;
    private List<float> equityValue;

    //Ratio de satisfaccion del cliente
    //[Range(0f, 1f)]
    private float clientSatisfactionRatio = 1f;

	void Awake() {
		//Logica del singleton
		if (instance == null) {
			instance = this;
		}
		else if(this != instance) {
			Destroy(this.gameObject);
		}

		//Mantener instancia durante la sesion de juego
		DontDestroyOnLoad (this.gameObject);

		//Inicializar inventario
		inventoryCounter = new List<int> ();
		inventoryMax = new List<int> ();
		for (int i = 0; i < 4; i++) {
			inventoryCounter.Add(0);
			inventoryMax.Add(9000000);
		}

		//Dinero inicial
		moneyCapital = 500f;

		//Inicializar porcentaje de credito
		bankCreditRate = 0.05f;

		//Porcentaje de impuestos
		taxRate = 0.12f;

		//Inicializar listas
		creditList = new List<float>();

		activeTotal = new List<float> ();
		passiveTotal = new List<float> ();
		liabilityTotal = new List<float> ();

		//Update values
		AddNewMonth();
		activeTotal [currentMonth - 1] += moneyCapital;
		liabilityTotal [currentMonth - 1] += moneyCapital;

		//Probabilidad de que ocurra un evento
		currentProbEvent = eventProRatio = 0.5f;

        //Listas de activos
        activeName = new List<string>();
        activeValue = new List<float>();

        //Listas de liabilities
        liabilityName = new List<string>();
        liabilityValue = new List<float>();

        //Listas de equity
        equityName = new List<string>();
        equityValue = new List<float>();

        //Inicializar status
        AddActiveEntry("Cash", moneyCapital);
        AddEquityEntry("Common Stock", moneyCapital);

		//Inicializar precios bases
		productBaseBuyPrice = new float[productSellPrice.Length];
		productBaseSellPrice = new float[productBaseBuyPrice.Length];

		for (int i = 0; i < productBaseBuyPrice.Length && productSellPrice.Length == productBuyPrice.Length; i++) {
			productBaseBuyPrice [i] = productBuyPrice [i];
			productBaseSellPrice [i] = productSellPrice [i];
		}
	}

	// Use this for initialization
	void Start () {

	}

	//Retorna capital actual del negocio
	public float GetMoney(){
		return moneyCapital;
	}

	//Asigna nuevo valor a capital del negocio
	public void SetMoney(float val) {
		moneyCapital = val;
	}

	//Retorna porcentaje de credito / Range(0f, 1f)
	public float GetCreditRatio() {
		return bankCreditRate;
	}

	//Asigna nuevo valor de procentaje de credito / Range(0f, 1f)
	public void SetCreditRatio(float val) {
		bankCreditRate = val;
	}

	//Actualiza el contador de cuantos productos existen
	public void SetProductCounter(int[] productA){
		productCounter = new List<int> ();	//Inicializa arreglo
		
		//Volcar la data al arreglo interno
		for (int i = 0; i < productA.Length; i++) {
			productCounter.Add(productA[i]);
		}
	}
	
	//Obtiene el arreglo con los productos / retorna int[]
	public int[] GetProductCounterInt(){
		int[] productAux = new int[productCounter.Count]; //Auxiliar de tipo int[]

		//Asignar data al arreglo auxiliar
		for (int i = 0; i < productAux.Length; i++) {
			productAux[i] = productCounter[i];
		}

		return productAux;
	}

	//Obtiene el arreglo con los productos / retorna List<int>
	public List<int> GetProductCounterList(){
		return productCounter;
	}

	//Retorna el precio del producto
	public float GetBuyPrice(int ID){
		return productBuyPrice [ID];
	}

	public float[] GetBuyPriceArray(){
		return productBuyPrice;
	}

	public float GetProductBaseBuyPrice(int ID){
		return productBaseBuyPrice [ID];
	}

	public float GetProductBaseSellPrice(int ID){
		return productBaseSellPrice [ID];
	}

	//Asigna un nuevo valor al producto especifico
	public void SetBuyPrice(int ID, float value){
		productBuyPrice [ID] = value;
	}
		
	//Retorna el precio del producto
	public float GetSellPrice(int ID){
		return productSellPrice [ID];
	}

	public float[] GetSellPriceArray(){
		return productSellPrice;
	}

	//Asigna un nuevo valor al producto especifico
	public void SetSellPrice(int ID, float value){
		productSellPrice [ID] = value;
	}

	//Actualiza el inventario
	public void SetInventory(int[] invProduct) {
		inventoryCounter = new List<int> ();

		for (int i = 0; i < invProduct.Length; i++) {
			inventoryCounter.Add(invProduct[i]);
		}
	}

	//Retorna un arreglo con el inventario
	public int[] GetInventory() {
		int[] invCounter = new int[inventoryCounter.Count];

		for (int i = 0; i < invCounter.Length; i++) {
			invCounter[i] = inventoryCounter[i];
		}

		return invCounter;
	}

	//Retorna un arreglo con la capacidad maxima del inventario
	public int[] GetMaxInventory(){
		int[] maxInv = new int[inventoryMax.Count];

		for (int i = 0; i < maxInv.Length; i++) {
			maxInv[i] = inventoryMax[i];
		}

		return maxInv;
	}

	//Actualiza la capacidad maxima de productos que se pueden almacenar
	public void SetMaxInventory(int[] maxInventory){
		inventoryMax = new List<int> ();

		for (int i = 0; i < maxInventory.Length; i++) {
			inventoryMax.Add(maxInventory[i]);
		}
	}

	//Retorna el porcentanje actual de los impuestos / Range(0f, 1f)
	public float GetTaxRate(){
		return taxRate;
	}

	//Actualiza el porcentaje actual de los impuestos / Range(0f, 1f)
	public void SetTaxRate(float val){
		taxRate = val;
	}

	//Retornar cantidad maxima de productos a usar
	public int GetMaxProducts() {
		return maxProducts;
	}

	//Actualizar productos
	public void UpdateMaxProducts(int val) {
		maxProducts += val;
	}

	//Nombre de venta de minijuegos
	public string GetSellMinigameName () {
		if (currentMonth - 1 < minigameSellName.Length) { //Get minigames in order for first few months
			return minigameSellName [currentMonth - 1];
		}
		return minigameSellName [Random.Range(0, minigameSellName.Length)]; //Get a random minigame for the next months
	}

	//Nombre de compra de minijuegos
	public string GetBuyMinigameName () {
		if (currentMonth - 1 < minigameBuyName.Length) { //Get minigames in order for first few months
			return minigameBuyName [currentMonth - 1];
		}
        return minigameBuyName[Random.Range(0, minigameBuyName.Length)]; //Get a random minigame for the next months
	}

	//Agrega credito a la lista
	public void AddCredit(float val){
		creditList.Add (val);
	}

	//Retorna una lista con todos los creditos del jugador
	public List<float> GetCreditList(){
		return creditList;
	}

	//Retorna un arreglo con todos los creditos del jugador
	public float[] GetCreditArray() {
		float[] credit = new float[creditList.Count];

		//Agregar elementos al arreglo
		for (int i = 0; i < creditList.Count; i++) {
			credit [i] = creditList [i];
		}

		return credit;
	}

	//Quita credito de la lista
	public void RemoveCredit(int ID){
		creditList.RemoveAt (ID);
	}

	//Retorna el mes actual
	public int GetCurrentMonth(){
		return currentMonth;
	}

	//Debug: Asigna un mes especifico
	public void SetCurrentMonth(int val){
		currentMonth = val;
	}

    //Asignar nuevo valor para sales revenue
    public void SetSalesRevenue(float val) {
        salesRevenue = val;
    }

    //Retorna sales revenue
    public float GetSalesRevenue() {
        return salesRevenue;
    }

    //Asigna nuevo valor a la deuda del cliente
    public void SetDebt(float val) {
        currentDebt = val;
    }

    //Retorna el monto de la deuda actual
    public float GetDebt() {
        return currentDebt;
    }

	//Añade una nueva entrada en cada valor de la formula
	public void AddNewMonth() {
		activeTotal.Add ((activeTotal.Count > 0) ? activeTotal[activeTotal.Count -1] : 0f);
        passiveTotal.Add ((passiveTotal.Count > 0) ? passiveTotal[passiveTotal.Count - 1] : 0f);
        liabilityTotal.Add ((liabilityTotal.Count > 0) ? liabilityTotal[liabilityTotal.Count -1] : 0f);
	}

	//Actualiza Actives
	public void UpdateActive(float val){
		activeTotal [currentMonth - 1] += val;
	}

	//Actualiza Liability
	public void UpdatePassive(float val){
		passiveTotal [currentMonth - 1] += val;
	}

	//Actualiza Equity
	public void UpdateLiability(float val){
		liabilityTotal [currentMonth - 1] += val;
	}

	//Retorna lista de activos
	public List<float> GetActiveList(){
		return activeTotal;
	}

	//Retorna lista de pasivos
	public List<float> GetPassiveList(){
		return passiveTotal;
	}

	//Retorna lista de patrimonio
	public List<float> GetLiabilityList(){
		return liabilityTotal;
	}

	//Test si se debe mostrar un evento en este momento
	public bool TestEvent(bool onSell){
		//Comprobar que se esta en el mes especifico donde ocurre un evento
		if ((currentMonth == 2 || currentMonth == 4 || currentMonth == 6 || currentMonth == 8 || currentMonth == 10) && onSell) {
			return true;
		}

		return false;
	}

    public void AddActiveEntry(string name, float val){
        int savedPos = -1;

        for (int i = 0; i < activeName.Count; i++) {
            if (activeName[i] == name) {
                savedPos = i; break;
            }
        }

        if (savedPos != -1) { //Existe
            activeValue[savedPos] += val;
        }
        else { //No existe
            activeName.Add(name);
            activeValue.Add(val);
        }
    }

	public float GetActive(string name){
		for (int i = 0; i < activeName.Count; i++) {
			if (activeName [i] == name)
				return activeValue [i];
		}
		return 0;
	}

    public List<string> GetActiveName() {
        return activeName;
    }

    public List<float> GetActiveValue() {
        return activeValue;
    }

    public void AddLiabilityEntry(string name, float val){
        int savedPos = -1;

        for (int i = 0; i < liabilityName.Count; i++) {
            if (liabilityName[i] == name) {
                savedPos = i; break;
            }
        }

        if (savedPos != -1) { //Existe
            liabilityValue[savedPos] += val;
        }
        else { //No existe
            liabilityName.Add(name);
            liabilityValue.Add(val);
        }
    }

	public float GetLiability(string name){
		for (int i = 0; i < liabilityName.Count; i++) {
			if (liabilityName [i] == name)
				return liabilityValue [i];
		}

		return 0;
	}

    public List<string> GetLiabilityName() {
        return liabilityName;
    }

    public List<float> GetLiabilityValue() {
        return liabilityValue;
    }

    public void AddEquityEntry(string name, float val){
        int savedPos = -1;

        for (int i = 0; i < equityName.Count; i++) {
            if (equityName[i] == name) {
                savedPos = i; break;
            }
        }

        if (savedPos != -1) { //Existe
            equityValue[savedPos] += val;
        }
        else { //No existe
            equityName.Add(name);
            equityValue.Add(val);
        }
    }
		
	public float GetEquity(string name){
		for (int i = 0; i < equityName.Count; i++) {
			if (equityName [i] == name)
				return equityValue [i];
		}

		return 0;
	}

    public List<string> GetEquityName() {
        return equityName;
    }

    public List<float> GetEquityValue() {
        return equityValue;
    }

    //Ratio de satisfaccion
    public float GetSatisfaction() {
        return clientSatisfactionRatio;
    }

    public void SetSatisfaction(float val) {
        clientSatisfactionRatio = val;
    }

    public void SaveDatatoPlayerPref() {
        //Mes actual
        PlayerPrefs.SetInt("Month", currentMonth);

        //Dinero
        PlayerPrefs.SetFloat("Money", moneyCapital);

        //Deuda
        PlayerPrefs.SetFloat("Debt", currentDebt);

        for (int i = 0; i < inventoryCounter.Count; i++) {
            //Inventory status
            PlayerPrefs.SetInt("MaxInv" + i.ToString("d0"), inventoryMax[i]);
            PlayerPrefs.SetInt("CurInv" + i.ToString("d0"), inventoryCounter[i]);

            //Precio de venta
            PlayerPrefs.SetFloat("SellPrice" + i.ToString("d0"), productSellPrice[i]);

            //Precio de compra
            PlayerPrefs.SetFloat("BuyPrice" + i.ToString("d0"), productBuyPrice[i]);

            //Precios bases
            PlayerPrefs.SetFloat("SellBasePrice" + i.ToString("d0"), productBaseSellPrice[i]);
            PlayerPrefs.SetFloat("BuyBasePrice" + i.ToString("d0"), productBaseBuyPrice[i]);

            //Productos a comprar / vendidos
            PlayerPrefs.SetInt("ProductCounter" + i.ToString("d0"), productCounter[i]);
        }

        //Maxima cantidad de productos
        PlayerPrefs.SetInt("MaxProduct", maxProducts);

        //Interes del banco
        PlayerPrefs.SetFloat("BankRate", bankCreditRate);

        //Impuesto de venta de fin de mes
        PlayerPrefs.SetFloat("TaxRate", taxRate);

        //Totales de activos, liabilities y equity
        for (int i = 0; i < activeTotal.Count; i++) {
            PlayerPrefs.SetFloat("ActiveTotal" + i.ToString("d0"), activeTotal[i]);
            PlayerPrefs.SetFloat("LiabilityTotal" + i.ToString("d0"), passiveTotal[i]);
            PlayerPrefs.SetFloat("EquityTotal" + i.ToString("d0"), liabilityTotal[i]);
        }

        //Estatus de la empresa
        for (int i = 0; i < activeName.Count; i++) {
            PlayerPrefs.SetString("ActiveName" + i.ToString("d0"), activeName[i]);
            PlayerPrefs.SetFloat("ActiveValue" + i.ToString("d0"), activeValue[i]);
        }

        for (int i = 0; i < liabilityName.Count; i++) {
            PlayerPrefs.SetString("LiabilityName" + i.ToString("d0"), liabilityName[i]);
            PlayerPrefs.SetFloat("LiabilityValue" + i.ToString("d0"), liabilityValue[i]);
        }

        for (int i = 0; i < equityName.Count; i++) {
            PlayerPrefs.SetString("EquityName" + i.ToString("d0"), equityName[i]);
            PlayerPrefs.SetFloat("EquityValue" + i.ToString("d0"), equityValue[i]);
        }

        //Satisfaccion del cliente
        PlayerPrefs.SetFloat("ClientSatisfaction", clientSatisfactionRatio);

        //Ultima escena
        PlayerPrefs.SetString("Scene", SceneManager.GetActiveScene().name);

        //On Sell / On Buy
        PlayerPrefs.SetInt("OnSell", (DecisionsController.fromEndMonth) ? 1 : 0);
    }

    public string LoadDatafromPlayerPref() {
        //Mes actual
        currentMonth = PlayerPrefs.GetInt("Month", 0);

        //Dinero
        moneyCapital = PlayerPrefs.GetFloat("Money", 0f);

        //Deuda
        currentDebt = PlayerPrefs.GetFloat("Debt", 0f);

        //Maxima cantidad de productos
        maxProducts = PlayerPrefs.GetInt("MaxProduct", 0);

        inventoryMax = new List<int>();
        inventoryCounter = new List<int>();

        productSellPrice = new float[maxProducts];
        productBuyPrice = new float[maxProducts];
        productBaseSellPrice = new float[maxProducts];
        productBaseBuyPrice = new float[maxProducts];

        productCounter = new List<int>();

        for (int i = 0; i < maxProducts; i++) {
            //Inventory status
            inventoryMax.Add(PlayerPrefs.GetInt("MaxInv" + i.ToString("d0"), 0));
            inventoryCounter.Add(PlayerPrefs.GetInt("CurInv" + i.ToString("d0"), 0));

            //Precio de venta
            productSellPrice[i] = PlayerPrefs.GetFloat("SellPrice" + i.ToString("d0"), 0f);

            //Precio de compra
            productBuyPrice[i] = PlayerPrefs.GetFloat("BuyPrice" + i.ToString("d0"), 0f);

            //Precios bases
            productBaseSellPrice[i] = PlayerPrefs.GetFloat("SellBasePrice" + i.ToString("d0"), 0f);
            productBaseBuyPrice[i] = PlayerPrefs.GetFloat("BuyBasePrice" + i.ToString("d0"), 0f);

            //Productos a comprar / vendidos
            productCounter.Add(PlayerPrefs.GetInt("ProductCounter" + i.ToString("d0"), 0));
        }

        //Interes del banco
        bankCreditRate = PlayerPrefs.GetFloat("BankRate", 0f);

        //Impuesto de venta de fin de mes
        taxRate = PlayerPrefs.GetFloat("TaxRate", 0f);

        //Totales de activos, liabilities y equity
        activeTotal = new List<float>();
        passiveTotal = new List<float>();
        liabilityTotal = new List<float>();

        for (int i = 0; PlayerPrefs.GetFloat("ActiveTotal" + i.ToString("d0"), 0f) != 0f; i++) {
            activeTotal.Add(PlayerPrefs.GetFloat("ActiveTotal" + i.ToString("d0"), 0f));
            passiveTotal.Add(PlayerPrefs.GetFloat("LiabilityTotal" + i.ToString("d0"), 0f));
            liabilityTotal.Add(PlayerPrefs.GetFloat("EquityTotal" + i.ToString("d0"), 0f));
        }

        //Estatus de la empresa
        activeName = new List<string>();
        activeValue = new List<float>();
        for (int i = 0; PlayerPrefs.GetString("ActiveName" + i.ToString("d0"), "None") != "None"; i++) {
            activeName.Add(PlayerPrefs.GetString("ActiveName" + i.ToString("d0"), "None"));
            activeValue.Add(PlayerPrefs.GetFloat("ActiveValue" + i.ToString("d0"), 0f));            
        }

        liabilityName = new List<string>();
        liabilityValue = new List<float>();
        for (int i = 0; PlayerPrefs.GetString("LiabilityName" + i.ToString("d0"), "None") != "None"; i++) {
            liabilityName.Add(PlayerPrefs.GetString("LiabilityName" + i.ToString("d0"), "None"));
            liabilityValue.Add(PlayerPrefs.GetFloat("LiabilityValue" + i.ToString("d0"), 0f));
        }

        equityName = new List<string>();
        equityValue = new List<float>();
        for (int i = 0; PlayerPrefs.GetString("EquityName" + i.ToString("d0"), "None") != "None"; i++) {
            equityName.Add(PlayerPrefs.GetString("EquityName" + i.ToString("d0"), "None"));
            equityValue.Add(PlayerPrefs.GetFloat("EquityValue" + i.ToString("d0"), 0f));
        }

        //Satisfaccion del cliente
        clientSatisfactionRatio = PlayerPrefs.GetFloat("ClientSatisfaction", 1f);

        //On Sell / On Buy
        DecisionsController.fromEndMonth = (PlayerPrefs.GetInt("OnSell", -1) == 1) ? true : false;
        DecisionsController.fromLoadData = true;

        //Ultima escena
        return PlayerPrefs.GetString("Scene", "(0) DemoMainMenuV5");
    }
}

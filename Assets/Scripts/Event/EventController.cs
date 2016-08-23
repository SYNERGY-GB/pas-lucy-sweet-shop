using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EventController : MonoBehaviour {
    public EventList eventScriptableObjectData;         //Data que contiene todos los eventos

	public GameObject eventUI;							//Padre de UI de eventos

    public static List<int> eventDone;					//Lista de eventos ocurridos / True -> Ya ocurrio, False -> CC

	//Requerimientos del evento
	private struct eventReqData {
		public float cash, debt, equity;
		public int[] productCant;
		public bool req; //True si existen requerimientos

        public bool specialReq;
        public int specialReqID;
	};

	//Strings colocados en los botones
	private struct eventOptionData {
		public List<string> textOnButton;

        public List<string> textOnResponse;
        public List<string> instructionResponse;
	};

    private int totalEvent;                             //Cantidad total de eventos cargados
    private int eventID;                                //ID del evento actual

	private List<eventReqData> eventReq;				//Lista de requerimientos de los eventos
	private List<string> textTitle;						//Lista de texto de titulos de cada evento
	private List<string> textToShow;					//Lista de texto a mostrar de cada evento
	private List<eventOptionData> eventOp;				//Lista de opciones y respuestas del evento

    public GameObject[] buttonOption;                   //Lista de botones de opciones

    private bool eventReady;                            //Flag indica si ya se pueden acceder a los eventos

    public Animator eventAnim;                          //Referencia interna del animator de evento
    public GameObject blackBackground;                  //Referencia interna del fondo negro

    public Text titleTextUI;                            //UI de Texto del titulo
    public Text descriptionTextUI;                      //UI de Texto de la descripcion

    public BalanceLogController balanceLogController;   //Referencia interna del log

    public GameObject eventResponse;                    //Referencia de la interfaz de respuesta de un evento
    public Text eventResponseDescription;               //UI de Texto de la respuesta del evento

    public DecisionsController decisionController;      //Referencia interna del controlador del menu de deciones
    public GraphController graphController;             //Referencia interna del controlador del grafico

	void Awake(){
        //eventAnim.SetTrigger("Hide");
        eventResponse.SetActive(false);

        if (eventDone == null) {
            eventDone = new List<int>();
        }	}

	// Use this for initialization
	void Start () {
		Debug.Log (Application.dataPath);

        eventReady = false;
        LoadEventScriptableObject();
        eventReady = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//Shows event
	public void ActiveEvent() {
        if (!eventReady) {
            StartCoroutine(ActivateEventDelay());
            return;
        }

        Debug.Log("On Active Event");

        bool eventFlag = false;
        eventReqData eventReqAux;
        eventOptionData eventOpAux;

        //Debug.Log("blackBackground Here");

        //Choose event to do
        eventID = Random.Range(0, totalEvent);

        //Buscar si el evento ya ocurrio
        for (int i = 0; i < eventDone.Count; i++) {
            if (eventDone[i] == eventID) {
                eventFlag = true; break;
            }
        }

        //Si ya ocurrio buscar uno nuevo linealmente
        if (eventFlag) {
            for (int i = 0; i < totalEvent; i++) {
                eventFlag = false;
                for (int j = 0; j < eventDone.Count; j++) {
                    if (i == eventDone[j]) {
                        eventFlag = true; break;
                    }
                }

                if (!eventFlag) { //Encontrado
                    eventID = i; break;
                }
            }
        }
        
        //Comprobar si cumple con los requerimientos
        eventReqAux = eventReq[eventID];
        if(eventReqAux.req){
            if(GameController.instance != null){
                if(eventReqAux.cash > GameController.instance.GetMoney() ||
                   eventReqAux.debt > GameController.instance.GetDebt() ||
                   eventReqAux.equity > GameController.instance.GetLiabilityList()[GameController.instance.GetCurrentMonth() -1]) {
                    Debug.Log("Event Requirements not met - Event ID: " + eventID);
                    return;
                }

                for(int i = 0; i < GameController.instance.GetInventory().Length; i++){
                    if(eventReqAux.productCant[i] > GameController.instance.GetInventory()[i]){
                        Debug.Log("Event Requirements not met - Event ID: " + eventID);
                        return;
                    }
                }
            }
        }

        //Para requerimientos especiales
        if (eventReqAux.specialReq) {
            if (eventReqAux.specialReqID == 12) { //If false return
                if (!SpecialRequirement12()) return;
            }
        }

        //Colocar evento como realizado
        eventDone.Add(eventID);

        //Cargar textos
        titleTextUI.text = textTitle[eventID];

        descriptionTextUI.text = textToShow[eventID];

        //Configurar botones
        eventOpAux = eventOp[eventID];
        for (int i = 0; i < buttonOption.Length; i++) {
            if(i < eventOpAux.textOnButton.Count){
                buttonOption[i].SetActive(true);
                buttonOption[i].GetComponentInChildren<Text>().text = eventOpAux.textOnButton[i];
            }
            else {
                buttonOption[i].SetActive(false);
            }
        }

        //Show UI
        eventAnim.SetTrigger("Show");
        blackBackground.SetActive(true);
	}

    //Hides event
    public void DesactivateEvent() {
        eventAnim.SetTrigger("Hide");
        blackBackground.SetActive(false);

        graphController.LoadValues();
        decisionController.OnChangeSatisfactionValue(); //Se pudo haber cambiado la satisfaccion del cliente
	}

	//Click en la primera opcion
	public void OnClickOption0(){
        eventOptionData eventOpAux = eventOp[eventID];
        string[] lineAux;

        //Obtener instruccion
        lineAux = eventOpAux.instructionResponse[0].Split('+');

        //Ejecutar instruccion
        DoInstruction(lineAux);

        //Colocar descripcion
        eventResponseDescription.text = eventOpAux.textOnResponse[0];
        eventResponse.SetActive(true);
	}

	//Click en la segunda opcion
	public void OnClickOption1(){
        eventOptionData eventOpAux = eventOp[eventID];
        string[] lineAux;

        //Obtener instruccion
        lineAux = eventOpAux.instructionResponse[1].Split('+');

        //Ejecutar instruccion
        DoInstruction(lineAux);

        //Colocar descripcion
        eventResponseDescription.text = eventOpAux.textOnResponse[1];
        eventResponse.SetActive(true);
	}

	//Click en la tercera opcion
	public void OnClickOption3(){
        eventOptionData eventOpAux = eventOp[eventID];
        string[] lineAux;

        //Obtener instruccion
        lineAux = eventOpAux.instructionResponse[2].Split('+');

        //Ejecutar instruccion
        DoInstruction(lineAux);

        //Colocar descripcion
        eventResponseDescription.text = eventOpAux.textOnResponse[2];
        eventResponse.SetActive(true);
	}

	//Click en la cuarta opcion
	public void OnClickOption4(){
        eventOptionData eventOpAux = eventOp[eventID];
        string[] lineAux;

        //Obtener instruccion
        lineAux = eventOpAux.instructionResponse[3].Split('+');

        //Ejecutar instruccion
        DoInstruction(lineAux);

        //Colocar descripcion
        eventResponseDescription.text = eventOpAux.textOnResponse[3];
        eventResponse.SetActive(true);
	}

    void DoInstruction(string[] instrucLine) {
        if (GameController.instance == null) return;

        if (int.Parse(instrucLine[0]) == 0) { //Cash
            GameController.instance.SetMoney(GameController.instance.GetMoney() + float.Parse(instrucLine[1]));

            if (float.Parse(instrucLine[1]) > 0f) { //Possitive
                GameController.instance.AddActiveEntry("Cash", float.Parse(instrucLine[1]));
                GameController.instance.AddEquityEntry("Common Stock", float.Parse(instrucLine[1]));
            }
            else { //Negative
                GameController.instance.AddActiveEntry("Cash", float.Parse(instrucLine[1]));
                GameController.instance.AddEquityEntry("Common Stock", float.Parse(instrucLine[1]));
            }
            //Actualizar status de la empresa
            GameController.instance.UpdateActive(float.Parse(instrucLine[1])); //Active
            //GameController.instance.UpdatePassive(priceTotal); //Liability
            GameController.instance.UpdateLiability(float.Parse(instrucLine[1])); //Equity

            //Mostrar Balance Log
            balanceLogController.SetupLog("Cash", "Common Stock", float.Parse(instrucLine[1]));
        }
        else if (int.Parse(instrucLine[0]) == 1) { //Customer satisfaction
            //Increase customer satisfaction by %
            GameController.instance.SetSatisfaction(GameController.instance.GetSatisfaction() * (1f + float.Parse(instrucLine[1])));
        }
        else if (int.Parse(instrucLine[0]) == 2) { //Do Nothing
            
        }
        //Specials
        else if (int.Parse(instrucLine[0]) == 10) {
            Effect10();
        }
        else if (int.Parse(instrucLine[0]) == 11) {
            Effect11();
        }
        else if (int.Parse(instrucLine[0]) == 12) {
            Effect12(float.Parse(instrucLine[1]));
        }
        else if (int.Parse(instrucLine[0]) == 13) {
            Effect13();
        }
        else if (int.Parse(instrucLine[0]) == 14) {
            Effect14(int.Parse(instrucLine[1]));
        }
        else if (int.Parse(instrucLine[0]) == 15) {
            Effect15();
        }
        else if (int.Parse(instrucLine[0]) == 16) {
            Effect16();
        }
        //DesactivateEvent();
    }

    public void OnClickEventResponseContinue() {
        DesactivateEvent();
        eventResponse.SetActive(false);
    }

    //Cargar eventos desde un objeto scriptable
    public void LoadEventScriptableObject() {
        List<EventItem> eList = eventScriptableObjectData.eventList;
        List<string> auxString0, auxString1, auxString2;
        eventOptionData eventOpAux;
        eventReqData eventReqAux;

        textTitle = new List<string>();
        textToShow = new List<string>();
        eventOp = new List<eventOptionData>();
        eventReq = new List<eventReqData>();

        totalEvent = eList.Count;

        for (int i = 0; i < eList.Count; i++) {
            //Agregar titulo
            textTitle.Add(eList[i].title);

            //Agregar texto descriptivo
            textToShow.Add(eList[i].description);

            //Agregar texto de las opciones
			auxString0 = new List<string>();
            auxString1 = new List<string>();
            auxString2 = new List<string>();
			for(int j = 0; j < eList[i].textOnButton.Count; j++){
                //Texto del boton
				auxString0.Add (eList[i].textOnButton[j]);

                //Texto de respuesta
                auxString1.Add (eList[i].textOnResponse[j]);

                //String de instruccion a ejecutar
                auxString2.Add (eList[i].instructionResponse[j]);
			}
            eventOpAux = new eventOptionData();
            eventOpAux.textOnButton = auxString0;
            eventOpAux.textOnResponse = auxString1;
            eventOpAux.instructionResponse = auxString2;
            eventOp.Add(eventOpAux);

            //Colocar requerimientos si existen
            eventReqAux = new eventReqData();
            eventReqAux.cash = eventReqAux.debt = eventReqAux.equity = 0f;
            eventReqAux.productCant = new int[4];
            eventReqAux.req = eList[i].req;
            if (eventReqAux.req) {
                if (eList[i].moneyReq > 0f) { //Requiere cash
                    eventReqAux.cash += eList[i].moneyReq;
                }
                
                if (eList[i].debtReq > 0f) { //Requiere deuda
                    eventReqAux.debt += eList[i].debtReq;
                }

                if (eList[i].equityReq > 0f) { //Requiere equity
                    eventReqAux.equity += eList[i].equityReq;
                }
                
                for (int j = 0; j < eList[i].productReq.Length; j++) { //Requiere cantidad de producto
                    // 1 - ID / 2 - Cantidad
                    eventReqAux.productCant[j] += eList[i].productReq[j];
                }
            }

            //Colocar requerimientos especiales si existen
            eventReqAux.specialReq = eList[i].specialReq;
            if (eventReqAux.specialReq) {
                eventReqAux.specialReqID = eList[i].specialReqID;
            }

            eventReq.Add(eventReqAux);
        }
    }

	//Cargar la data de los eventos
	public void LoadEvents(){
		int offset = 0;
        string lineAux;
        string[] lineInstruc;
		List<string> auxString0, auxString1, auxString2;
        eventOptionData eventOpAux;
        eventReqData eventReqAux;

		//Initialize
		auxString0 = new List<string>();
        auxString1 = new List<string>();
        auxString2 = new List<string>();
		textTitle = new List<string> ();
		textToShow = new List<string> ();
		eventOp = new List<eventOptionData> ();
        eventReq = new List<eventReqData> ();

		//Read text file
        string text, filePath;
        #if UNITY_EDITOR
            filePath = Application.dataPath + "\\Event.txt";
		    text = System.IO.File.ReadAllText(filePath);
        #endif
        #if UNITY_WEBGL
            //text = System.IO.File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Event.txt"));
            //filePath = Application.persistentDataPath + "/Event.txt";
            filePath = "../Event.txt";
            //if (filePath.Contains("://")) {
            //    //WWW www = new WWW(filePath);
            //    //filePath = www.text;
            //    filePath = filePath.Remove(0, 11); // file:///C:/
            //    Debug.Log("New File Path: " + filePath);
            //}
            text = System.IO.File.ReadAllText(filePath);
        #endif
		string[] lines = text.Split ('\n');
		totalEvent = int.Parse (lines [0]);

		Debug.Log ("Eventos, Linea 1: " + lines [1]);

        offset = 0;
		for (int i = 0; i < totalEvent; i++) {
			//Agregar titulo
			textTitle.Add(lines[offset + 1]);

			//Agregar texto descriptivo
			textToShow.Add(lines[offset + 2]);

			//Agregar texto de las opciones
			auxString0 = new List<string>();
            auxString1 = new List<string>();
            auxString2 = new List<string>();
			for(int j = 0; j < int.Parse(lines[offset + 3]); j++){
                //Texto del boton
				auxString0.Add (lines [(offset + 4) + (j * 3)]);

                //Texto de respuesta
                auxString1.Add (lines [(offset + 5) + (j * 3)]);

                //String de instruccion a ejecutar
                auxString2.Add (lines [(offset + 6) + (j * 3)]);
			}
            eventOpAux = new eventOptionData();
            eventOpAux.textOnButton = auxString0;
            eventOpAux.textOnResponse = auxString1;
            eventOpAux.instructionResponse = auxString2;
            eventOp.Add(eventOpAux);

            offset += int.Parse(lines[offset + 3]) * 3 + 3 + 1;

            //Colocar requerimientos si existen
            eventReqAux = new eventReqData();
            eventReqAux.cash = eventReqAux.debt = 0f;
            eventReqAux.productCant = new int[4];
            eventReqAux.req = int.Parse(lines[offset]) > 0;
            if (eventReqAux.req) {
                for (int j = 0; j < int.Parse(lines[offset]); j++) {
                    lineAux = lines[offset + j + 1]; //Debug.Log("Instrucc0 " + lineAux);

                    lineInstruc = lineAux.Split('+');
                    //Debug.Log("Instrucc " + lineAux);
                    if (int.Parse(lineInstruc[0]) == 0) { //Requiere cash
                        eventReqAux.cash += float.Parse(lineInstruc[1]);
                    }
                    else if (int.Parse(lineInstruc[0]) == 1) { //Requiere deuda
                        eventReqAux.debt += float.Parse(lineInstruc[1]);
                    }
                    else if (int.Parse(lineInstruc[0]) == 2) { //Requiere cantidad de producto
                        // 1 - ID / 2 - Cantidad
                        eventReqAux.productCant[int.Parse(lineInstruc[1])] += int.Parse(lineInstruc[2]);
                    }
                }
            }
            eventReq.Add(eventReqAux);

            offset += int.Parse(lines[offset]);
		}
	}

    void Effect10() {
        if (GameController.instance != null) {
            for (int i = 0; i < GameController.instance.GetInventory().Length; i++) { //Reduce prices by 5%
                GameController.instance.SetSellPrice(i, GameController.instance.GetSellPrice(i) * 0.95f);
            }

            //Increase customer satisfaction by 20%
            GameController.instance.SetSatisfaction(GameController.instance.GetSatisfaction() + 0.2f);
        }
    }

    void Effect11() {
        if (GameController.instance != null) {
            GameController.instance.SetMoney(GameController.instance.GetMoney() -200f);

            GameController.instance.AddActiveEntry("Cash", -200f);
            GameController.instance.AddEquityEntry("Common Stock", -200f);

            //Mostrar Balance Log
            balanceLogController.SetupLog("Cash", "Common Stock", -200f);

            //Increase customer satisfaction by 10%
            GameController.instance.SetSatisfaction(GameController.instance.GetSatisfaction() + 0.1f);

            //Actualizar status de la empresa
            GameController.instance.UpdateActive(-200f); //Active
            //GameController.instance.UpdatePassive(priceTotal); //Liability
            GameController.instance.UpdateLiability(-200f); //Equity
        }
    }

    void Effect12(float satIncrease) {
        if (GameController.instance != null) {
            //Quitar cupcakes del inventario
            int[] inv = GameController.instance.GetInventory();
            inv[3] -= 3;
            GameController.instance.SetInventory(inv);

            //Increase customer satisfaction by %
            GameController.instance.SetSatisfaction(GameController.instance.GetSatisfaction() + satIncrease);

            //Status de la empresa
            GameController.instance.AddActiveEntry("Inventory", -(GameController.instance.GetSellPrice(3) * 3f));
            GameController.instance.AddEquityEntry("Common Stock", -(GameController.instance.GetSellPrice(3) * 3f));

            //Mostrar Balance Log
            balanceLogController.SetupLog("Common Stock", "Inventory", (GameController.instance.GetSellPrice(3) * 3f));

            //Actualizar status de la empresa
            GameController.instance.UpdateActive(-(GameController.instance.GetSellPrice(3) * 3f)); //Active
            //GameController.instance.UpdatePassive(priceTotal); //Liability
            GameController.instance.UpdateLiability(-(GameController.instance.GetSellPrice(3) * 3f)); //Equity
        }
    }

    void Effect13() {
        //Status de la empresa
        GameController.instance.AddActiveEntry("Equipment", 300f);
        GameController.instance.AddLiabilityEntry("Notes Payable", 300f);

        //Mostrar Balance Log
        balanceLogController.SetupLog("Equipment", "Notes Payable", 300f);

        //Actualizar status de la empresa
        GameController.instance.UpdateActive(300f); //Active
        GameController.instance.UpdatePassive(300f); //Liability
        //GameController.instance.UpdateLiability(-300f); //Equity
    }

    void Effect14(int reduceCakeAmount) {
        if(reduceCakeAmount > 0) {
            //Quitar cake del inventario
            int[] inv = GameController.instance.GetInventory();
            inv[2] -= reduceCakeAmount;
            GameController.instance.SetInventory(inv);

            //Status de la empresa
            GameController.instance.AddActiveEntry("Inventory", -(GameController.instance.GetSellPrice(2)));
            GameController.instance.AddEquityEntry("Common Stock", -(GameController.instance.GetSellPrice(2)));
            GameController.instance.AddActiveEntry("Cash", (GameController.instance.GetSellPrice(2)));
            GameController.instance.AddLiabilityEntry("Unearned earnings", (GameController.instance.GetSellPrice(2)));

            //Actualizar status de la empresa
            //GameController.instance.UpdateActive((GameController.instance.GetSellPrice(2))); //Active
            GameController.instance.UpdatePassive((GameController.instance.GetSellPrice(2))); //Liability
            GameController.instance.UpdateLiability(-(GameController.instance.GetSellPrice(2))); //Equity
        }
        else {
            GameController.instance.AddActiveEntry("Cash", (GameController.instance.GetSellPrice(2)));
            GameController.instance.AddLiabilityEntry("Unearned earnings", (GameController.instance.GetSellPrice(2)));

            //Actualizar status de la empresa
            GameController.instance.UpdateActive((GameController.instance.GetSellPrice(2))); //Active
            //GameController.instance.UpdatePassive(priceTotal); //Liability
            GameController.instance.UpdateLiability((GameController.instance.GetSellPrice(2))); //Equity
        }

        //Mostrar Balance Log
        balanceLogController.SetupLog("Cash", "Unearned earnings", GameController.instance.GetSellPrice(2));
    }

    void Effect15() {
        //Status de la empresa
        GameController.instance.AddLiabilityEntry("Rent Payable", 50f);
        GameController.instance.AddEquityEntry("Common Stock", -50f);

        //Mostrar Balance Log
        balanceLogController.SetupLog("Common Stock", "Rent Payable", 50f);

        //Actualizar status de la empresa
        //GameController.instance.UpdateActive(-(GameController.instance.GetSellPrice(3) * 3f)); //Active
        GameController.instance.UpdatePassive(50f); //Liability
        GameController.instance.UpdateLiability(-50f); //Equity
    }

    void Effect16() {
        //Quitar cookies y ice creams del inventario
        int[] inv = GameController.instance.GetInventory();
        inv[0] -= 8; //Cookies
        inv[1] -= 4; //Ice creams
        GameController.instance.SetInventory(inv);

        float totalSoldPrice = GameController.instance.GetSellPrice(0) * 8 + GameController.instance.GetSellPrice(1) * 4;

        //Status de la empresa
        GameController.instance.AddActiveEntry("Cash", totalSoldPrice);
        GameController.instance.AddActiveEntry("Inventory", -totalSoldPrice);

        //Mostrar Balance Log
        balanceLogController.SetupLog("Cash", "Inventory", totalSoldPrice);
    }

    IEnumerator ActivateEventDelay() {
        yield return new WaitForSeconds(0.2f);

        ActiveEvent();
    }

    bool SpecialRequirement12() {
        if(GameController.instance == null) return true;

        //Se puede realizar evento
        return GameController.instance.GetLiabilityValue()[GameController.instance.GetCurrentMonth() - 1] > GameController.instance.GetSellPrice(3) * 3;
    }
}

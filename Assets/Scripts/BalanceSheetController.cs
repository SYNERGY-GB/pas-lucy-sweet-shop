using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BalanceSheetController : MonoBehaviour {
	public GameObject entryTitlePf;								//Prefab de un entry en alguna de las tablas
	public GameObject entryAmountPf;							//Prefab de un entry en alguna de las tablas

	public Vector2 startActiveTitlePosition;					//Posicion de inicio de la lista de titulos de activos
	public Vector2 startActiveAmountPosition;					//Posicion de inicio de la lista de titulos de activos
	private Vector2 startSavedActiveTitle;						//Posicion guardada del titulo y la cantidad
	private Vector2 startSavedActiveAmount;

    public Vector2 startLiabilityTitlePosition;					//Posicion de inicio de la lista de titulos de liabilities
    public Vector2 startLiabilityAmountPosition;				//Posicion de inicio de la lista de titulos de liabilities
	private Vector2 startSavedLiabilityTitle;					//Posicion guardada del titulo y la cantidad
	private Vector2 startSavedLiabilityAmount;

    public Vector2 startEquityTitlePosition;					//Posicion de inicio de la lista de titulos de equity
    public Vector2 startEquityAmountPosition;				    //Posicion de inicio de la lista de titulos de equity
	private Vector2 startSavedEquityTitle;						//Posicion guardada del titulo y la cantidad
	private Vector2 startSavedEquityAmount;

	public float offssetAsset;									//Offset en Y de distancia entre assets entries

	private List<GameObject> titleAsset;						//Listas de nombres y cantidades de activos
    private List<GameObject> amountAsset;

    private List<GameObject> titleLiability;					//Listas de nombres y cantidades de liabilities
    private List<GameObject> amountLiability;

    private List<GameObject> titleEquity;						//Listas de nombres y cantidades de equity
    private List<GameObject> amountEquity;

    public Text totalAssetsText, totalLiaEquText;               //Texto UI de total de activos y liabilites + equity
    private float totalA, totalLE;

    public Text monthText;                                      //Texto UI del mes actual

	public Transform parentAsset;								//Padre de los textos
    public Transform parentLiability;
    public Transform parentEquity;

	private int counter = 0;

	void Awake(){
		//Save starting positions
		startSavedActiveTitle = startActiveTitlePosition;
		startSavedActiveAmount = startActiveAmountPosition;

		startSavedLiabilityTitle = startLiabilityTitlePosition;
		startSavedLiabilityAmount = startLiabilityAmountPosition;

		startSavedEquityTitle = startEquityTitlePosition;
		startSavedEquityAmount = startEquityAmountPosition;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKeyDown (KeyCode.F)) { //Add Inventory
			AddFieldActive ("Inventory" + (counter++).ToString(), 100f);
		}
		if (Input.GetKeyDown (KeyCode.G)) {	//Add Cash
			AddFieldActive ("Cash", 300f);
		}*/
	}

    public void SetupBalanceSheet() {
        //Auxiliaries lists
        List<string> name;
        List<float> value;

        totalA = totalLE = 0;
        RemoveData();

        if (GameController.instance != null) {
            //Setup actives
            name = GameController.instance.GetActiveName();
            value = GameController.instance.GetActiveValue();

            for (int i = 0; i < name.Count; i++) {
                AddFieldActive(name[i], value[i]);
            }

            //Setup liabilities
            name = GameController.instance.GetLiabilityName();
            value = GameController.instance.GetLiabilityValue();

            for (int i = 0; i < name.Count; i++) {
                AddFieldLiability(name[i], value[i]);
            }

            //Setup equity
            name = GameController.instance.GetEquityName();
            value = GameController.instance.GetEquityValue();

            for (int i = 0; i < name.Count; i++) {
                AddFieldEquity(name[i], value[i]);
            }

            //Setup Totals
            totalAssetsText.text = "$" + totalA.ToString("f2");
            totalLiaEquText.text = "$" + totalLE.ToString("f2");

            //Month
            monthText.text = "Month " + GameController.instance.GetCurrentMonth().ToString("d2") + "/12";
        }
        else {
            name = new List<string>();
            value = new List<float>();

            //Setup Totals
            totalAssetsText.text = "$0.00";
            totalLiaEquText.text = "$0.00";

            //Month
            monthText.text = "14/12";
        }
    }

    public void RemoveData() {
        if(titleAsset != null){
            for (int i = 0; i < titleAsset.Count; i++) {
                Destroy(titleAsset[i]);
                Destroy(amountAsset[i]);
            }
            titleAsset.Clear();
            amountAsset.Clear();
        }

        if(titleLiability != null){
            for (int i = 0; i < titleLiability.Count; i++) {
                Destroy(titleLiability[i]);
                Destroy(amountLiability[i]);
            }
            titleLiability.Clear();
            amountLiability.Clear();
        }

        if(titleEquity != null){
            for (int i = 0; i < titleEquity.Count; i++) {
                Destroy(titleEquity[i]);
                Destroy(amountEquity[i]);
            }
            titleEquity.Clear();
            amountEquity.Clear();
        }

        //Initialize Lists
        titleAsset = new List<GameObject>();
        amountAsset = new List<GameObject>();

        titleLiability = new List<GameObject>();
        amountLiability = new List<GameObject>();

        titleEquity = new List<GameObject>();
        amountEquity = new List<GameObject>();
    }

	void AddFieldActive (string name, float cant) {
        totalA += cant; //Add to total count

		//Search if entry alredy exist
		for(int i = 0; i < titleAsset.Count; i++) {
			if (titleAsset [i].GetComponent<Text> ().text == name) { //Update amount
				amountAsset[i].GetComponent<Text>().text = (float.Parse(amountAsset[i].GetComponent<Text>().text) + cant).ToString("f2");
				return;
			}
		}

		//New entry
		//Title
		titleAsset.Add(Instantiate(entryTitlePf, startSavedActiveTitle, Quaternion.identity) as GameObject);
		titleAsset [titleAsset.Count - 1].GetComponent<Text> ().text = name; //Colocar nombre
		titleAsset [titleAsset.Count - 1].transform.SetParent (parentAsset); //Asignar padre
		titleAsset[titleAsset.Count - 1].transform.localPosition = startSavedActiveTitle; //Colocar posicion
		startSavedActiveTitle = new Vector2(startSavedActiveTitle.x, startSavedActiveTitle.y - offssetAsset);

		//Amount
		amountAsset.Add(Instantiate(entryAmountPf, startSavedActiveAmount, Quaternion.identity) as GameObject);
        amountAsset[amountAsset.Count - 1].GetComponent<Text>().text = "$" + cant.ToString("f2"); //Colocar cantidad
		amountAsset [amountAsset.Count - 1].transform.SetParent (parentAsset); //Asignar padre
		amountAsset[amountAsset.Count - 1].transform.localPosition = startSavedActiveAmount; //Colocar posicion
		startSavedActiveAmount = new Vector2(startSavedActiveAmount.x, startSavedActiveAmount.y - offssetAsset);
	}

    void AddFieldLiability (string name, float cant) {
        totalLE += cant; //Add to total count

		//Search if entry alredy exist
		for(int i = 0; i < titleLiability.Count; i++) {
			if (titleLiability [i].GetComponent<Text> ().text == name) { //Update amount
                amountLiability[i].GetComponent<Text>().text = (float.Parse(amountLiability[i].GetComponent<Text>().text) + cant).ToString("f2");
				return;
			}
		}

		//New entry
		//Title
		titleLiability.Add(Instantiate(entryTitlePf, startSavedLiabilityTitle, Quaternion.identity) as GameObject);
        titleLiability[titleLiability.Count - 1].GetComponent<Text>().text = name; //Colocar nombre
        titleLiability[titleLiability.Count - 1].transform.SetParent(parentLiability); //Asignar padre
		titleLiability[titleLiability.Count - 1].transform.localPosition = startSavedLiabilityTitle; //Colocar posicion
		startSavedLiabilityTitle = new Vector2(startSavedLiabilityTitle.x, startSavedLiabilityTitle.y - offssetAsset);

		//Amount
		amountLiability.Add(Instantiate(entryAmountPf, startSavedLiabilityAmount, Quaternion.identity) as GameObject);
        amountLiability[amountLiability.Count - 1].GetComponent<Text>().text = "$" + cant.ToString("f2"); //Colocar cantidad
        amountLiability[amountLiability.Count - 1].transform.SetParent(parentLiability); //Asignar padre
		amountLiability[amountLiability.Count - 1].transform.localPosition = startSavedLiabilityAmount; //Colocar posicion
		startSavedLiabilityAmount = new Vector2(startSavedLiabilityAmount.x, startSavedLiabilityAmount.y - offssetAsset);
	}

    void AddFieldEquity (string name, float cant) {
        totalLE += cant; //Add to total count

		//Search if entry alredy exist
		for(int i = 0; i < titleEquity.Count; i++) {
			if (titleEquity [i].GetComponent<Text> ().text == name) { //Update amount
                amountEquity[i].GetComponent<Text>().text = (float.Parse(amountEquity[i].GetComponent<Text>().text) + cant).ToString("f2");
				return;
			}
		}

		//New entry
		//Title
		titleEquity.Add(Instantiate(entryTitlePf, startSavedEquityTitle, Quaternion.identity) as GameObject);
        titleEquity[titleEquity.Count - 1].GetComponent<Text>().text = name; //Colocar nombre
        titleEquity[titleEquity.Count - 1].transform.SetParent(parentEquity); //Asignar padre
		titleEquity[titleEquity.Count - 1].transform.localPosition = startSavedEquityTitle; //Colocar posicion
		startSavedEquityTitle = new Vector2(startSavedEquityTitle.x, startSavedEquityTitle.y - offssetAsset);

		//Amount
		amountEquity.Add(Instantiate(entryAmountPf, startSavedEquityAmount, Quaternion.identity) as GameObject);
        amountEquity[amountEquity.Count - 1].GetComponent<Text>().text = "$" + cant.ToString("f2"); //Colocar cantidad
        amountEquity[amountEquity.Count - 1].transform.SetParent(parentEquity); //Asignar padre
		amountEquity[amountEquity.Count - 1].transform.localPosition = startSavedEquityAmount; //Colocar posicion
		startSavedEquityAmount = new Vector2(startSavedEquityAmount.x, startSavedEquityAmount.y - offssetAsset);
	}

	//Reset pivots for balance sheet entries
	public void ResetPivots(){
		startSavedActiveTitle = startActiveTitlePosition;
		startSavedActiveAmount = startActiveAmountPosition;

		startSavedLiabilityTitle = startLiabilityTitlePosition;
		startSavedLiabilityAmount = startLiabilityAmountPosition;

		startSavedEquityTitle = startEquityTitlePosition;
		startSavedEquityAmount = startEquityAmountPosition;
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TopUIController : MonoBehaviour {
    public GameObject topUIAll;                             //Gameobject que contiene todos los elementos de la UI

    public Text monthUI;                                    //Referencia interna del texto del mes
    public Text cashUI;                                     //Referencia interna del texto del dinero disponible

    private float currentMoney;                             //Dinero mostrado
    private int currentMonth;                               //Mes mostrado

    public float moneyUpdateTime = 5f;             	        //Velocidad de animacion de actualizacion del dinero
    private float targetMoney;                              //Usando en la animacion del dinero

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		currentMoney = Mathf.MoveTowards (currentMoney, targetMoney, (targetMoney / moneyUpdateTime) * Time.deltaTime);
        cashUI.text = "Cash: " + currentMoney.ToString("f2");
	}

    //Agregar dinero
    public void UpdateMoney(float val) {
        targetMoney += val;
    }

    //Añadir un mes
    public void UpdateMonth() {
        currentMonth++;
        monthUI.text = "Month: " + currentMonth.ToString("d0") + "/12";
    }

    //Asignar dinero
    public void SetMoney(float val) {
        currentMoney = targetMoney = val;
        cashUI.text = "Cash: " + currentMoney.ToString("f2");
    }

    //Asignar mes
    public void SetMonth(int val) {
        currentMonth = val;
        monthUI.text = "Month: " + currentMonth.ToString("d0") + "/12";
    }
}

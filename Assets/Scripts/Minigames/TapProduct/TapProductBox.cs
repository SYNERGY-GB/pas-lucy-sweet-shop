using UnityEngine;
using System.Collections;

public class TapProductBox : MonoBehaviour {
	//Definicion de los distintos tipos de cajas
	//0 - Cookie, 1 - Cake, 2 - Chocolate, 3 - Cupcake
	public int type;					//Tipo de caja actual
	public int maxType = 4;				//Cantidad maxima del tipo de caja

	public float lifetime = 1.6f;		//Tiempo de vida del producto
	
	public float endTime;				//Tiempo para eliminar el producto
	private float offsetTime;			//Cuenta el tiempo de la pausa
    private float auxTimer;             //Auxiliar para calcular el tiempo dentro de la pausa

    private bool onPause;               //Indica si se esta en pausa

	private int ID;						//ID interno, referente a la posicion en la escena

    void Awake() {
        //Pre init variables
        onPause = false;
        offsetTime = 0f;
    }

	// Use this for initialization
	void Start () {
		//Asignar tiempo de vida
		endTime = Time.time + lifetime;

		//Init producto
		SetBoxType ();
	}
	
	// Update is called once per frame
	void Update () {
		//Si se llega al tiempo maximo de vida eliminar caja
		if (Time.time > endTime && !onPause) {
			TapProductController.instance.SetFree(ID);

			Destroy(this.gameObject);
		}
	}

	//Inicializa la caja acorde a los parametros recibidos del controlador
	public void SetBoxType(){
		//Asignar tipo aleatorio
		type = Random.Range (0, maxType);
		
		//Asignar sprite
		GetComponent<SpriteRenderer> ().sprite = TapProductController.instance.GetSprite (type);
	}

	public int GetID() {
		return type;
	}

	//Obtener posicion del ID
	public int GetPositionID() {
		return ID;
	}

	//Asignar posicion del ID
	public void SetPositionID(int newID) {
		ID = newID;
	}

    //Pausar producto
    public void Pause() {
        onPause = true;

        auxTimer = Time.time;
    }

    //Reaunudar producto
    public void Resume() {
        offsetTime = Time.time - auxTimer;
        endTime += offsetTime;

        onPause = false;
    }
}

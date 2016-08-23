using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkyDropBox : MonoBehaviour {
	//Definicion de los distintos tipos de cajas
	//0 - Cookie, 1 - Cake, 2 - Chocolate, 3 - Cupcake
	public int type;					//Tipo de caja actual
	public int maxType = 4;				//Cantidad maxima del tipo de caja

	private Rigidbody2D boxR;			//Referencia interna de Rigidbody2D

	public float speed = 4f;			//Velocidad de la caja
	public float lifetime = 7f;			//Tiempo de vida de la caja

	private float endTime;				//Tiempo para eliminar la caja
	private float offsetTime;			//Cuenta el tiempo de la pausa

	private bool setPause;				//Indica si se asigno el estado de pausa
	private float timerAux;				//Timer(float) auxiliar

	public bool rotateAround;			//Permite que el objeto rote mientras este en la escena
	private float rotateSpeed;			//Velocidad de rotacion

    private float savedAngularVel;      //Velocidad angular guardada

	void Awake(){
		//Inicializar Referencias
		boxR = GetComponent<Rigidbody2D> ();

		//Inicializar Caja
		SetBoxType ();
	}

	// Use this for initialization
	void Start () {
		//Asignar velocidad en Y
		boxR.velocity = -Vector3.up * speed;

		//Colocar tiempo de destruccion de la caja en la escena
		endTime = Time.time + lifetime;

		//Inicializar otros
		setPause = false;
        savedAngularVel = 0f;

		if (rotateAround) {
			//Velocidad de rotacion
			rotateSpeed = Random.Range (15f, 30f);
			boxR.angularVelocity = rotateSpeed;

			//Rotacion inicial
			transform.localRotation = Quaternion.Euler (new Vector3 (0f, 0f, Random.Range (0f, 180f)));
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Si no existe instancia controladora
		//if (SkyDropController.instance == null) Destroy (this.gameObject);

		if (SkyDropController.instance.OnPause ()) { //Checkear si se esta en pausa
			if(!setPause) {	//Guardar el tiempo de cuando se realizo la pausa
				boxR.velocity = Vector2.zero;

                if (rotateAround) { 
                    savedAngularVel = boxR.angularVelocity;
                    boxR.angularVelocity = 0f;
                }

				setPause = true;
				timerAux = Time.time;
			}
		}
		else {
			if(setPause) {	//Reiniciar el tiempo de cuando se realizo la pausa
				boxR.velocity = -Vector3.up * speed;

                if (rotateAround) {
                    boxR.angularVelocity = savedAngularVel;
                }

				setPause = false;
				offsetTime = Time.time - timerAux;
				endTime += offsetTime;
			}
		}

		//Checkear si se elimina la caja en este frame
		if ((Time.time > endTime || SkyDropController.instance.OnMenu()) && !setPause) {
			Destroy(this.gameObject);
		}
	}

	//Inicializa la caja acorde a los parametros recibidos del controlador
	public void SetBoxType(){
		//Si no existe instancia controladora
		if (SkyDropController.instance == null) return;

		//Asignar tipo aleatorio
		type = Random.Range (0, maxType);

		//Asignar sprite
		GetComponent<SpriteRenderer> ().sprite = SkyDropController.instance.GetSprite (type);
		/*switch(type) {
		case 0: //Cookie - Yellow
			GetComponent<SpriteRenderer> ().color = Color.yellow;
			break;
		case 1: //Cake - White
			GetComponent<SpriteRenderer> ().color = Color.yellow;
			break;
		case 2: //Chocolate - Black
			GetComponent<SpriteRenderer> ().color = Color.black;
			break;
		case 3: //Cupcake - Blue
			GetComponent<SpriteRenderer> ().color = Color.blue;
			break;
		}*/
	}

	public int GetID(){
		return type;
	}
}

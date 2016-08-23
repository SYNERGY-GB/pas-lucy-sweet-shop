using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GraphController : MonoBehaviour {
	public GameObject botTextPf;				//Prefab del texto que estara abajo del grafico
	public GameObject barGraphPf;				//Prefab del grafo de barras

	public Scrollbar graphViewScroll;			//Referencia al scrollbar de la zona donde se agregar los elementos del grafico

	public Transform parentAll;					//Padre del texto de abajo y las barras

	public float barMaxSize;					//Tamaño maximo de la barra

	public float parentSizeTolerance = 390f;	//Tolerancia del tamaño del padre

	public Vector3 startGraphPos;				//Posicion de inicio del grafico (coordenadas de pantalla)
    private Vector3 savedStartPos;              //Contiene la posicion inicial del grafico
	public float graphOffsetBar = 30f;			//Offset entre el texto y la barra
	public float graphOffset = 120f;			//Offset entre elementos del grafico
	private int elementCount;					//Cantidad de elementos compuestos (3 barras + 1 texto)
	public float sizeOfElement = 120f;			//Tamaño de un elemento

	public float offsetSizeIncrease = 270f;		//Offset para empezar a incrementar el tamaño de la zona deslizable

	public int ratioXAxis;						//Separacion de los valores en X y Y respectivamente
	public Text[] textUILeft;					//Texto en la parte izquierda del grafico

	//Valores a mostrar en X y Y
	private List<float> valuesActive, valuesPassive, valuesLiability;

	private List<GameObject> graphElements;		//Lista de elementos del grafico

	private bool onGraphMovement;				//Indica si se esta moviendo la zona desplazable
	public float graphViewScrollTime = 0.8f;	//Velocidad de animacion de movimiento del scrollbar

	void Awake(){
		//Inicializar listas
		/*valuesActive = new List<float> ();
		valuesPassive = new List<float> ();
		valuesLiability = new List<float> ();

		valuesActive.Add (170f);
		valuesActive.Add (50f);
		valuesActive.Add (170f);
		valuesActive.Add (30f);
		valuesActive.Add (55f);
		valuesActive.Add (30f);
		valuesActive.Add (55f);

		valuesPassive.Add (100f);
		valuesPassive.Add (10f);
		valuesPassive.Add (100f);
		valuesPassive.Add (10f);
		valuesPassive.Add (45f);
		valuesPassive.Add (10f);
		valuesPassive.Add (45f);

		valuesLiability.Add (70f);
		valuesLiability.Add (40f);
		valuesLiability.Add (70f);
		valuesLiability.Add (20f);
		valuesLiability.Add (10f);
		valuesLiability.Add (20f);
		valuesLiability.Add (10f);*/

		//LoadValues (valuesActive, valuesPassive, valuesLiabilie);

        if (GameController.instance != null) {
            valuesActive = GameController.instance.GetActiveList();
            valuesPassive = GameController.instance.GetPassiveList();
            valuesLiability = GameController.instance.GetLiabilityList();
        }
        else {
            valuesActive = new List<float>();
            valuesPassive = new List<float>();
            valuesLiability = new List<float>();

			valuesActive.Add (170f);
			valuesActive.Add (50f);
			valuesActive.Add (170f);
			valuesActive.Add (30f);
			valuesActive.Add (55f);
			valuesActive.Add (30f);
			valuesActive.Add (55f);

			valuesPassive.Add (100f);
			valuesPassive.Add (10f);
			valuesPassive.Add (100f);
			valuesPassive.Add (10f);
			valuesPassive.Add (45f);
			valuesPassive.Add (10f);
			valuesPassive.Add (45f);

			valuesLiability.Add (70f);
			valuesLiability.Add (40f);
			valuesLiability.Add (70f);
			valuesLiability.Add (20f);
			valuesLiability.Add (10f);
			valuesLiability.Add (20f);
			valuesLiability.Add (10f);
        }

		//Initialize scrollable area
		graphViewScroll.value = 0f;
		onGraphMovement = true;
	}

	// Use this for initialization
	void Start () {
		//SetupGraph ();
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown (0)) {
        //    Debug.Log(Input.mousePosition);
        //}

		//Movimiento del scrollbar
		if (onGraphMovement) {
			graphViewScroll.value = Mathf.MoveTowards (graphViewScroll.value, 1f, (graphViewScrollTime * valuesActive.Count / 5f) * Time.deltaTime);

			if (Mathf.Abs(graphViewScroll.value - 1f) < Mathf.Epsilon) onGraphMovement = false;
		}
	}

	public void LoadValues(/*List<float> valAc, List<float> valPa, List<float> valLi*/){
		float maxValueY = 0f;
		float sizeY0, sizeY1;
        Vector3 savedPos;

        Debug.Log("Graph is here");

        if (graphElements == null) graphElements = new List<GameObject>();

        if (GameController.instance != null) { //Load values
            valuesActive = GameController.instance.GetActiveList();
            valuesPassive = GameController.instance.GetPassiveList();
            valuesLiability = GameController.instance.GetLiabilityList();
        }

        if (graphElements.Count > 0) {
            startGraphPos = savedStartPos;
        }
        else {
            savedStartPos = startGraphPos;
        }

        for (int i = 0; i < graphElements.Count; i++) { //If there are elements to delete
            Destroy(graphElements[i]);
        }
        graphElements.Clear();

		for (int i = 0; i < valuesActive.Count; i++) { //Buscar el mayor valor
			if (maxValueY < valuesActive [i]) {
				maxValueY = valuesActive [i];
			}
		}

		//Comprobar tamano del padre
		//Debug.Log("offsetSizeIncrease - valuesActive.Count * graphOffset: " + (offsetSizeIncrease - valuesActive.Count * graphOffset).ToString());
		if (offsetSizeIncrease - valuesActive.Count * graphOffset < 0) { //Incrementar el tamaño de la zona deslizable
			parentAll.GetComponent<RectTransform>().sizeDelta = parentAll.GetComponent<RectTransform>().sizeDelta + new Vector2(Mathf.Abs((offsetSizeIncrease - (valuesActive.Count * graphOffset))), 0f);

			//Ajustar punto pivote
			startGraphPos += new Vector3((offsetSizeIncrease - (valuesActive.Count * graphOffset)) / 2f, 0f, 0f);
		}

		for (int i = 0, j = 1; i < valuesActive.Count; i++) {
			//Colocar texto de mes
			graphElements.Add(Instantiate(botTextPf, startGraphPos, Quaternion.identity) as GameObject);
			graphElements [graphElements.Count - 1].GetComponent<Text> ().text = (j++).ToString ("d0");
			graphElements [graphElements.Count - 1].transform.SetParent (parentAll);
			graphElements [graphElements.Count - 1].transform.localPosition = startGraphPos;

			//Colocar activos
			graphElements.Add(Instantiate(barGraphPf, startGraphPos, Quaternion.identity) as GameObject);
			graphElements [graphElements.Count - 1].transform.SetParent (parentAll);
			sizeY0 = (valuesActive [i] * barMaxSize) / maxValueY;
			graphElements [graphElements.Count - 1].GetComponent<RectTransform> ().sizeDelta = new Vector2 (40f, sizeY0);
			graphElements [graphElements.Count - 1].transform.localPosition = new Vector3 (startGraphPos.x - 20f, startGraphPos.y + graphOffsetBar + (sizeY0 / 2f));
			graphElements [graphElements.Count - 1].GetComponent<Image> ().color = new Color (82f / 255f, 215f / 255f, 82f / 255f, 1f);//Color.green;

			//Colocar patrimonio
			graphElements.Add(Instantiate(barGraphPf, startGraphPos, Quaternion.identity) as GameObject);
			graphElements [graphElements.Count - 1].transform.SetParent (parentAll);
			sizeY1 = (valuesLiability[i] * barMaxSize) / maxValueY;
			graphElements [graphElements.Count - 1].GetComponent<RectTransform> ().sizeDelta = new Vector2 (40f, sizeY1);
			graphElements [graphElements.Count - 1].transform.localPosition = new Vector3 (startGraphPos.x + 20f, startGraphPos.y + graphOffsetBar + (sizeY1 / 2f));
			graphElements [graphElements.Count - 1].GetComponent<Image> ().color = new Color (247f / 255f, 242f / 255f, 58f / 255f, 1f);//Color.yellow;
			savedPos = graphElements [graphElements.Count - 1].transform.localPosition;

			//Colocar pasivo
			graphElements.Add(Instantiate(barGraphPf, startGraphPos, Quaternion.identity) as GameObject);
			graphElements [graphElements.Count - 1].transform.SetParent (parentAll);
			sizeY0 = sizeY0 - sizeY1;
			graphElements [graphElements.Count - 1].GetComponent<RectTransform> ().sizeDelta = new Vector2 (40f, sizeY0);
			graphElements [graphElements.Count - 1].transform.localPosition = new Vector3 (savedPos.x, savedPos.y + ((sizeY1 + sizeY0) / 2f));
			graphElements [graphElements.Count - 1].GetComponent<Image> ().color = new Color (203f / 255f, 31f / 255f, 52f / 255f, 1f);//Color.red;

			//Nueva posicion pivote
			startGraphPos = new Vector3 (startGraphPos.x + graphOffset, startGraphPos.y);
		}

        //Asignar texto de la izquierda
        for (int i = 0; i < textUILeft.Length; i++) {
            textUILeft[i].text = ((i * maxValueY) / (textUILeft.Length - 1f)).ToString("f1");
        }

		/*for (int i = 0; i < graphElements.Count; i++) {
			graphElements [i].transform.SetParent (parentAll);
		}*/
	}

    public void SetupGraph() {
		
	}
}

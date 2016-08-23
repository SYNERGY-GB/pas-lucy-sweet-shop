using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NextMonthAnimationController : MonoBehaviour {
    public float speed = 18f;                   //Velocidad de la animacion
    public float fadeLifetime = 10f;            //Duracion del fadeIn

    public GameObject imgParent;                //Padre de la imagen
    public Image imgFade;                       //Referencia a la imagen a hacer fade

    private bool fadeIn;                        //Indica si se esta en fadeIn
    //Colores a realizar la animacion
    private Color targetIn = new Color(1f, 1f, 1f, 1f);
    private Color targetOu = new Color(1f, 1f, 1f, 0f);

    void Awake() {
        imgParent.SetActive(false);
        imgFade.color = targetOu;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (fadeIn) { //Ir de alpha = 0f -> 1f
            imgFade.color = Color.Lerp(imgFade.color, targetIn, speed * Time.deltaTime);
        }
        else { //Alpha = 1f -> 0f
            imgFade.color = Color.Lerp(imgFade.color, targetOu, speed * Time.deltaTime);

            if (imgFade.color.a < 0.15f) { //Esconder Animacion
                imgParent.SetActive(false);
            }
        }
	}

    public void StartAnimation() {
        imgParent.SetActive(true);
        fadeIn = true;

        StartCoroutine(ChangeFade());
    }

    IEnumerator ChangeFade() {
        yield return new WaitForSeconds(fadeLifetime);

        fadeIn = false;
    }
}

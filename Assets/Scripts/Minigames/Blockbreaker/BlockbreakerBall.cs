using UnityEngine;
using System.Collections;

public class BlockbreakerBall : MonoBehaviour {
	private bool onStop;

    //Variables de guardado local de datos
	private Vector2 savedVelocity;
    private float savedForce;
    private Vector2 savedInitialPosition;

    public float timeRestartDelay = 0.5f;           //Tiempo de espera para el reinicio de la pelota

	// Use this for initialization
	void Start () {
        savedInitialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Agregar fuerza a la pelota
	public void AddForce(float force) {
        float ang = (Random.Range(0f, 90f) - 45f) * Mathf.Deg2Rad; //Ajustar angulo inicial
        savedVelocity = new Vector2(Mathf.Sin(ang), Mathf.Cos(ang)) * force;
        savedForce = force;
        GetComponent<Rigidbody2D>().velocity = savedVelocity;
	}

	int RndSign () {
		return (Random.Range (0, 2) == 0) ? 1 : -1;
	}

	void OnCollisionEnter2D (Collision2D col) {
		int localID;

		if (col.gameObject.CompareTag ("BlockbreakerProduct") && !onStop) {
			//Obtener ID y actualizar UI
			localID = col.gameObject.GetComponent<BlockbreakerProduct> ().ID;
			BlockbreakerController.instance.UpdateUI (localID);

            GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity.normalized * savedForce;

            if (MusicController.instance != null) {
                MusicController.instance.PlayMinigameSound(0);
            }
		}
        else if (col.gameObject.CompareTag("BlockbreakerWall") || col.gameObject.CompareTag("BlockbreakerPlayer")) {
            GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity.normalized * savedForce;

            if (MusicController.instance != null) {
                MusicController.instance.PlayMinigameSound(0);
            }
        }
        else if (col.gameObject.CompareTag("BlockbreakerOut")) { //Outside of bounds
            //Restart Ball
            transform.position = savedInitialPosition;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            StartCoroutine(RestartDelay());
        }
	}

	public void Stop () {
		onStop = true;

		savedVelocity = GetComponent<Rigidbody2D> ().velocity;
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
	}

	public void Resume () {
		onStop = false;

		GetComponent<Rigidbody2D> ().velocity = savedVelocity;
	}

	public void Restart () {
		transform.position = Vector3.zero;
		GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
	}

    IEnumerator RestartDelay() {
        yield return new WaitForSeconds(timeRestartDelay);

        AddForce(savedForce);
    }
}

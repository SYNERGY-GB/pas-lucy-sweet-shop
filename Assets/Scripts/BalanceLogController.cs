using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BalanceLogController : MonoBehaviour {
    public Text accountName0UI, accountName1UI;             //Texto UI de los nombres de las cuentas
    public Text debitUI, creditUI;                          //Texto UI de las cantidades en las cuentas

    public float lifetime = 8f;                             //Tiempo visible de la UI
    private Coroutine lifetimeCor;                          //Coroutina que maneja el tiempo de aparacion de la UI

    public float hideSpeed = 10f;                           //Velocidad para esconder la UI
    private bool hideAnim;                                  //Referencia interna para esconder la UI

    void Update() {
        if (hideAnim) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, hideSpeed * Time.deltaTime);
        }
    }

    //Asignar valores a la UI
    public void SetupLog(string nameDebit, string nameCredit, float val) {
        //Mostrar UI
        hideAnim = false;
        transform.localScale = Vector3.one;

        accountName0UI.text = nameDebit;
        accountName1UI.text = nameCredit;

        debitUI.text = creditUI.text = val.ToString("f2");

        //Activar tiempo de aparacion
        if (lifetimeCor != null) StopCoroutine(lifetimeCor);
        lifetimeCor = StartCoroutine(ShowUI());
    }

    public void OnClickExitBalanceLog() {
        transform.localScale = Vector3.zero;

        //Detener Coroutine
        if (lifetimeCor != null) StopCoroutine(lifetimeCor);
    }

    //Maneja el tiempo de visibilidad de la UI
    IEnumerator ShowUI() {
        yield return new WaitForSeconds(lifetime);

        hideAnim = true;
    }
}

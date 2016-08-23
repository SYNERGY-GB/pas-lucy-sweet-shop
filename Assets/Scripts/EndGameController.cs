using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameController : MonoBehaviour {
    public Text activeTotalUI;
    public Text liabilityTotalUI;
    public Text equityTotalUI;

    public Text lucyTextUI;

    public GameObject disableSoundGO;

	// Use this for initialization
	void Start () {
        if (GameController.instance != null) {
            float res = 0;

            //Initialize active value
            foreach (float act in GameController.instance.GetActiveList()) {
                res += act;
            }
            activeTotalUI.text = res.ToString("f2");

            //Liability
            res = 0;
            foreach (float lia in GameController.instance.GetLiabilityList()) {
                res += lia;
            }
            liabilityTotalUI.text = res.ToString("f2");

            //Equity
            res = 0;
            foreach (float equ in GameController.instance.GetEquityValue()) {
                res += equ;
            }
            equityTotalUI.text = res.ToString("f2");

            //Colocar texto de acuerdo a cuanto equity se tiene
            if (res < 0f) {
                lucyTextUI.text = "This business has no future... I should close this and call it bankrupt";
            }
            else if(res < 200f) {
                lucyTextUI.text = "We aren't growing... we should invest our money elsewhere before its to late";
            }
            else if (res < 500f) {
                lucyTextUI.text = "We have less Equity that when we began... We should be more careful when choosing opportinities";
            }
            else if (res < 3000f) {
                lucyTextUI.text = "This is a slow growing business, I am happy with it but I think we might missed some great deals";
            }
            else {
                lucyTextUI.text = "Excelent Job!. Our Equity grew so much. You have what it takes to be a Business Manager, Congratulations!";
            }
        }
        else {
            lucyTextUI.text = "None";
        }

        //Disable / Enable Music
		if (MusicController.instance != null) {
			if (MusicController.instance.MusicStatus ()) {
				disableSoundGO.SetActive (false);
			} 
			else {
				disableSoundGO.SetActive (true);
			}

			//Setup Management Music
			MusicController.instance.ChangeMusic(1);
		} 
		else {
			disableSoundGO.SetActive (false);
		}
	}

    public void OnClickPlayAgain() {
        //Load initial Scene
        SceneManager.LoadScene("(0) DemoMainMenuV5");
    }
}

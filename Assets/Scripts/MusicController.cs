using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour {
    public static MusicController instance;     //Singleton

    //0 - Main Menu / 1 - Management / 2 - Minigame
    public AudioClip[] musicBg;

    //0 - Button / 1 - Continue / 2.. - Minigame
    public AudioClip[] soundEf;

    private AudioSource audioSourceMusic;       //Referencia interna de la musica y sonido
    public AudioSource audioSourceSound;        

    private bool playMusic;                     //Indica si se esta escuchando musica

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if(instance != this) {
            Destroy(this.gameObject);
        }

        playMusic = true;

        audioSourceMusic = GetComponent<AudioSource>();
    }

    //Cambia la musica de fondo
    public void ChangeMusic(int ID) {
		if (ID >= musicBg.Length)
			return;

        audioSourceMusic.clip = musicBg[ID];
        if (playMusic) audioSourceMusic.Play();
    }

    public bool MusicStatus() {
        return playMusic;
    }

    public void PlayMusic() {
        playMusic = true;
        audioSourceMusic.Play();
    }

    public void MuteMusic() {
        playMusic = false;
        audioSourceMusic.Stop();
    }

    public void PlayButtonSound() {
        if (playMusic) {
            audioSourceSound.clip = soundEf[0];
			audioSourceSound.Play ();
        }
    }

    public void PlayContinueSound() {
        if (playMusic) {
            audioSourceSound.clip = soundEf[1];
			audioSourceSound.Play ();
        }
    }

    public void PlayMinigameSound(int ID) {
        if (playMusic) {
            audioSourceSound.clip = soundEf[ID + 2];
			audioSourceSound.Play ();
        }
    }
}

using UnityEngine;
using System.Collections;

public class BlockbreakerProduct : MonoBehaviour {
	public int ID;
	public bool rndProduct;

	public float timeBeetweenProducts = 5f;
	private float nextChange;
	private float startTime;
	private float offsetTime;

	private bool onStop;

	public SpriteRenderer spriteR;

	private float auxTimer;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
		offsetTime = auxTimer = 0f;
		nextChange = startTime + timeBeetweenProducts;

		ID = (rndProduct) ? Random.Range (0, BlockbreakerController.instance.boxSpritesArray.Length) : ID;
		spriteR.sprite = BlockbreakerController.instance.GetSprite (ID);

		onStop = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (onStop) {
			return;
		}

		if (rndProduct) { //Random product
			if(Time.time - offsetTime > nextChange) {
				nextChange = Time.time + timeBeetweenProducts;

				ID = Random.Range (0, BlockbreakerController.instance.boxSpritesArray.Length);
				spriteR.sprite = BlockbreakerController.instance.GetSprite (ID);
			}
		}
	}

	public void Stop () {
		onStop = true;

		auxTimer = Time.time;
	}

	public void Resume () {
		onStop = false;

		offsetTime = Time.time - auxTimer;
	}

	public void Restart () {
		startTime = Time.time;
		offsetTime = auxTimer = 0f;
		nextChange = startTime + timeBeetweenProducts;

		ID = (rndProduct) ? Random.Range (0, BlockbreakerController.instance.boxSpritesArray.Length) : ID;
		spriteR.sprite = BlockbreakerController.instance.GetSprite (ID);
	}
}

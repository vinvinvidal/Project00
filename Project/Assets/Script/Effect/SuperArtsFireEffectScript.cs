using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperArtsFireEffectScript : GlobalClass
{
	//メインカメラ
	GameObject MainCamera;

	//プレイヤーキャラクター
	GameObject PlayerCharacter;

	//マテリアル
	Material FireMaterial;

	void Start()
    {
		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//プレイヤーキャラクター取得
		PlayerCharacter = GameManagerScript.Instance.GetPlayableCharacterOBJ();

		//マテリアル取得
		FireMaterial = gameObject.GetComponent<Renderer>().material;

		//透明度をゼロにしておく
		FireMaterial.SetFloat("_FadeCount" , 0);
	}

	void Update()
    {
		//ポジションをカメラの前方に配置
		transform.position = (MainCamera.transform.forward * 575);

		//プレイアブルキャラクターに向ける
		transform.LookAt(PlayerCharacter.transform);
	}

	public void FadeIn(float s)
	{
		StartCoroutine(FadeInCoroutine(s));
	}
	private IEnumerator FadeInCoroutine(float s)
	{
		float fade = 0;

		while(FireMaterial == null)
		{
			yield return null;
		}

		while(fade < 1)
		{
			fade += Time.deltaTime * s;

			FireMaterial.SetFloat("_FadeCount", fade);

			yield return null;
		}

		FireMaterial.SetFloat("_FadeCount", 1);
	}

	public void FadeOut(float s)
	{
		StartCoroutine(FadeOutCoroutine(s));
	}
	private IEnumerator FadeOutCoroutine(float s)
	{
		float fade = 1;

		while (FireMaterial == null)
		{
			yield return null;
		}

		while (fade > 0)
		{
			fade -= Time.deltaTime * s;

			FireMaterial.SetFloat("_FadeCount", fade);

			yield return null;

		}

		FireMaterial.SetFloat("_FadeCount", 0);

		Destroy(gameObject);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShaderScript : GlobalClass
{
	//マテリアル
	private Material FireMaterial;

	//YXオフセット移動値
	private float TexTureX_Offset = 0;

	//Yオフセット移動値
	private float TexTureY_Offset = 0;

	//消えはじめるまでの時間
	public float KeepTime = 0;

	//消える速度
	public float VanishTime = 0.05f;

	//Y移動速度
	public float Y_Speed;

	//X移動速度
	public float X_Speed;

	//ゆらめきの速さ
	public float FireBeat = 1;

	//ゆらめきの滑らかさ
	public float FireSmooth = 1;

	//ゆらめきの大きさ
	public float FireStrength = 1;

	void Start()
    {
		if(gameObject.GetComponent<ParticleSystem>() == null)
		{
			//マテリアル取得
			FireMaterial = gameObject.GetComponent<Renderer>().material;
		}
		else
		{
			//マテリアル取得
			FireMaterial = gameObject.GetComponent<ParticleSystemRenderer>().material;
		}

		//ゆらめきの速さセット
		FireMaterial.SetFloat("FireBeat", FireBeat);

		//ゆらめきの滑らかさセット
		FireMaterial.SetFloat("FireSmooth", FireSmooth);

		//ゆらめきの大きさセット
		FireMaterial.SetFloat("FireStrength", FireStrength);

		//消火コルーチン呼び出し、時間が0なら消さない
		if(KeepTime != 0f)
		{
			StartCoroutine(VanishCoroutine());
		}
	}

    void Update()
    {
		//オフセットカウントアップ
		TexTureX_Offset += Time.deltaTime;
		TexTureY_Offset -= Time.deltaTime * (Mathf.PerlinNoise(-Time.time, Time.time) + 0.5f);

		//火を動かす
		FireMaterial.SetTextureOffset("_FireMainTex", new Vector2(TexTureX_Offset * X_Speed, 0));
		FireMaterial.SetTextureOffset("_FireNormalTex", new Vector2(0, TexTureY_Offset * Y_Speed));		
	}

	private IEnumerator VanishCoroutine()
	{
		//指定された時間だけ待機
		yield return new WaitForSeconds(KeepTime);		

		float VanishNum = 1;

		while(VanishNum > -1)
		{
			VanishNum -= VanishTime;

			FireMaterial.SetTextureOffset("_FireVanishTex", new Vector2(0, VanishNum));

			yield return null;
		}

		Destroy(gameObject);
	}
}

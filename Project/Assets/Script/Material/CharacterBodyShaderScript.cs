using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyShaderScript : GlobalClass
{
	//マテリアル
	private Material BodyMaterial;

	//ベーステクスチャ
	public Texture2D _TexBase;

	//ノーマルマップ
	public Texture2D _TexNormal;

	//ハイライトのテクスチャ
	public Texture2D _TexHiLight;

	//ハイライトのmatcap
	public Texture2D _HiLightMatCap;

	//線画テクスチャ
	public Texture2D _TexLine;

	//ディレクショナルライトのトランスフォーム
	private Transform LightTransform;

	void Start()
    {
		//マテリアル取得
		BodyMaterial = transform.GetComponent<Renderer>().material;

		//ディレクショナルライトのトランスフォーム取得
		LightTransform = GameObject.Find("OutDoorLight").transform;

		//マテリアルにテクスチャを渡す
		BodyMaterial.SetTexture("_TexBase", _TexBase);
		BodyMaterial.SetTexture("_TexNormal", _TexNormal);
		BodyMaterial.SetTexture("_TexHiLight", _TexHiLight);
		BodyMaterial.SetTexture("_HiLightMatCap", _HiLightMatCap);
		BodyMaterial.SetTexture("_TexLine", _TexLine);
	}

    void Update()
    {
		//ディレクショナルライトの行列をシェーダーに渡す
		BodyMaterial.SetMatrix("_LightMatrix", LightTransform.worldToLocalMatrix);
	}

	//ブラー演出
	public void BlurEffect(float t, float l, Vector3 v)
	{
		//ブラーを延ばすベクトルをシェーダーに渡す
		BodyMaterial.SetVector("VartexVector", v);

		//ブラー伸縮コルーチン呼び出し
		StartCoroutine(BlurEffectCoroutine(t,l));
	}
	private IEnumerator BlurEffectCoroutine(float t, float l)
	{
		//経過時間
		float BlurTime = 0;
		
		//経過時間まで回す
		while(BlurTime < t)
		{
			//ブラー用変数をシェーダーに渡す
			BodyMaterial.SetFloat("_BlurNum", BlurTime * l * Random.Range(0.5f, 1.5f) / t);

			//経過時間カウントアップ
			BlurTime += Time.deltaTime;

			//１フレーム待機
			yield return null;
		}

		//経過時間リセット
		BlurTime = 0;

		//経過時間まで回す
		while (BlurTime < t)
		{
			//ブラー用変数をシェーダーに渡す
			BodyMaterial.SetFloat("_BlurNum", (1 - (BlurTime / t)) * l * Random.Range(0.5f, 1.5f));

			//経過時間カウントアップ
			BlurTime += Time.deltaTime;

			//１フレーム待機
			yield return null;
		}

		//ブラー用変数をちゃんと0にしてシェーダーに渡す
		BodyMaterial.SetFloat("_BlurNum", 0);
	}
}

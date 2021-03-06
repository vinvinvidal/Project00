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

	//線画テクスチャ
	public Texture2D _TexLine;

	//ハイライトのmatcap
	public Texture2D _HiLightMatCap;

	//ディレクショナルライトのトランスフォーム
	private Transform LightTransform;

	void Start()
    {
		//マテリアル取得
		BodyMaterial = transform.GetComponent<Renderer>().material;

		//ディレクショナルライトのトランスフォーム取得
		LightTransform = GameObject.Find("OutDoorLight").transform;

		//統合用テクスチャ
		Texture2D _TexAtlas = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//使用するテクスチャを配列に入れる
		Texture2D[] Textures = { _TexBase, _TexLine, _TexNormal, _TexHiLight, _HiLightMatCap };

		//テクスチャ統合、Rectを受け取る
		Rect[] TexRect = _TexAtlas.PackTextures(Textures, 0, 512, true);

		//マテリアルに統合テクスチャを渡す
		BodyMaterial.SetTexture("_TexAtlas", _TexAtlas);

		//ベーステクスチャのRectを渡す
		BodyMaterial.SetVector("_TexBaseRectPos", TexRect[0].position);
		BodyMaterial.SetVector("_TexBaseRectSize", TexRect[0].size);

		//線画テクスチャのRectを渡す
		BodyMaterial.SetVector("_TexLineRectPos", TexRect[1].position);
		BodyMaterial.SetVector("_TexLineRectSize", TexRect[1].size);

		//法線テクスチャのRectを渡す
		BodyMaterial.SetVector("_TexNormalRectPos", TexRect[2].position);
		BodyMaterial.SetVector("_TexNormalRectSize", TexRect[2].size);

		//ハイライトテクスチャのRectを渡す
		BodyMaterial.SetVector("_TexHiLightRectPos", TexRect[3].position);
		BodyMaterial.SetVector("_TexHiLightRectSize", TexRect[3].size);

		//マットキャップテクスチャのRectを渡す
		BodyMaterial.SetVector("_TexMatCapRectPos", TexRect[4].position);
		BodyMaterial.SetVector("_TexMatCapRectSize", TexRect[4].size);

		//BodyMaterial.SetTexture("_TexBase", _TexBase);
		//BodyMaterial.SetTexture("_TexLine", _TexLine);
		//BodyMaterial.SetTexture("_TexNormal", _TexNormal);
		//BodyMaterial.SetTexture("_TexHiLight", _TexHiLight);
		//BodyMaterial.SetTexture("_HiLightMatCap", _HiLightMatCap);
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

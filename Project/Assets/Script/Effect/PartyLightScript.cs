using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyLightScript : GlobalClass
{
	//テクスチャ
	public Texture2D Texture;

	//色を抽出するグラデーション
	public Gradient Grad;

	//マテリアル
	private Material PartyLightMaterial;

	//使用カラー
	private Color LightColor;

	//サインカーブ用カウント
	private float SinCount;

	//照明タイプ
	public int TypeNum;

	//ZText
	public int Ztest;

	void Start()
    {
		//マテリアル取得
		PartyLightMaterial = transform.GetComponent<Renderer>().material;

		//テクスチャを渡す
		PartyLightMaterial.SetTexture("_MainTex", Texture);

		//Z_TESTを渡す
		PartyLightMaterial.SetFloat("_ZTest", Ztest);

		//グラデーションから色を抽出
		LightColor = Grad.Evaluate(Random.Range(0f, 1f));

		switch (TypeNum)
		{
			//回転
			case 0:

				//初期値をランダムにする
				SinCount = Random.Range(0f, 100f);

				//フェードインコルーチン呼び出し
				StartCoroutine(FadeInCoroutine());

				break;

			//固定
			case 1:

				//シェーダーに色を渡す
				PartyLightMaterial.SetColor("_LightColor", LightColor);

				break;

			default:
				break;
		}


	}

    void Update()
    {
		switch (TypeNum)
		{
			case 0:

				//サインカーブカウントアップ
				SinCount += Time.deltaTime;

				//回転
				transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Sin(SinCount * 3) * 75, Mathf.Sin(SinCount) * 180, 0));

				break;

			default:
				break;
		}
	}

	//フェードインコルーチン
	private IEnumerator FadeInCoroutine()
	{
		//カウント宣言
		float TempCount = 0;

		while(TempCount < 1)
		{
			//カウントアップ
			TempCount += Time.deltaTime * 2;

			//グラデーションからランダム色を抽出しシェーダーに渡す
			PartyLightMaterial.SetColor("_LightColor", new Color(Mathf.Lerp(0,LightColor.r,TempCount), Mathf.Lerp(0, LightColor.g, TempCount), Mathf.Lerp(0, LightColor.b, TempCount)));

			//１フレーム待機
			yield return null;
		}

		//シェーダーに色を渡す
		PartyLightMaterial.SetColor("_LightColor", LightColor);
	}
}

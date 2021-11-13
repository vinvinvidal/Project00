using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyLightScript : GlobalClass
{
	//色を抽出するグラデーション
	public Gradient Grad;

	//マテリアル
	private Material PartyLightMaterial;

	//使用カラー
	private Color LightColor;

	//サインカーブ用カウント
	private float SinCount;

    void Start()
    {
		//マテリアル取得
		PartyLightMaterial = transform.GetComponent<Renderer>().material;

		//色を抽出しシェーダーに渡す
		LightColor = Grad.Evaluate(Random.Range(0f, 1f));

		//初期値をランダムにする
		SinCount = Random.Range(0f, 100f);

		//フェードインコルーチン呼び出し
		StartCoroutine(FadeInCoroutine());
	}

    void Update()
    {
		//サインカーブカウントアップ
		SinCount += Time.deltaTime;

		//回転
		transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Sin(SinCount * 5) * 75, Mathf.Sin(SinCount * 3) * 180, 0));
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

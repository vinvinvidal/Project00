using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface LightColorChangeScriptInterface : IEventSystemHandler
{
	//ライトを点ける
	void LightOn(float t, float n, Action act);

	//ライトを消す
	void LightOff(float t, float n, float i, Action act);
}
public class LightColorChangeScript : GlobalClass, LightColorChangeScriptInterface
{
	//ライトカラーグラデーション
	public Gradient LightColorGradient = new Gradient();

	//ライトコンポーネント
	private Light LightComp;

	//元のライトカラー
	private Color LightColor;

	void Start()
    {
		//ライトコンポーネント取得
		LightComp = gameObject.GetComponent<Light>();

		//元のライトカラー取得
		LightColor = LightComp.color;
	}

	//ライトを点ける
	public void LightOn(float t, float n, Action act)
	{
		//コルーチン呼び出し
		StartCoroutine(LightOnCoroutine(t, n, act));
	}

	//ライトを消す
	public void LightOff(float t, float n, float i, Action act)
	{
		//コルーチン呼び出し
		StartCoroutine(LightOffCoroutine(t, n, i, act));
	}

	//ライトを点けるコルーチン
	private IEnumerator LightOnCoroutine(float t, float n, Action act)
	{
		//ライト係数
		float LightNum = 0;

		//時間
		float FadeTime = 0;

		while(LightNum < 1)
		{
			//経過時間算出
			FadeTime += Time.deltaTime;

			//ライト係数算出
			LightNum = FadeTime / t;

			//ライトカラーに反映
			LightComp.color = LightColorGradient.Evaluate(LightNum * n) * LightColor;

			//1フレーム待機
			yield return null;
		}

		//ライトカラーを目的の位置にする
		LightComp.color = LightColor;

		//匿名関数実行
		act();
	}

	//ライトを消すコルーチン
	private IEnumerator LightOffCoroutine(float t, float n, float i, Action act)
	{
		//ライト係数
		float LightNum = 1;

		//時間
		float FadeTime = t;

		while (LightNum > 0)
		{
			//経過時間算出
			FadeTime -= Time.deltaTime;

			//ライト係数算出
			LightNum = FadeTime / t;

			//ライトカラーに反映
			LightComp.color = LightColorGradient.Evaluate(LightNum * n + i);

			//1フレーム待機
			yield return null;
		}

		//ライトカラーを目的の位置にする
		LightComp.color = LightColorGradient.Evaluate(i);

		//匿名関数実行
		act();
	}
}

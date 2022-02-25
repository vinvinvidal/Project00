using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface LightColorChangeScriptInterface : IEventSystemHandler
{
	//ライトをカラーを変える
	void LightChange(float t, float n, Action act);

	//ライトグラデーションを切り替える
	void GradientChange(int i, float p);
}
public class LightColorChangeScript : GlobalClass, LightColorChangeScriptInterface
{
	//ライトカラーグラデーションList
	public List<Gradient> LightColorGradientList = new List<Gradient>();

	//ライトコンポーネント
	private Light LightComp;

	//現在のグラデーションポジション
	private float LightColorPos;

	//現在使用しているグラデーションインデックス
	private int GradientIndex = 0;

	void Start()
    {
		//ライトコンポーネント取得
		LightComp = gameObject.GetComponent<Light>();
	}

	//ライトグラデーションを切り替える
	public void GradientChange(int i, float p)
	{
		//グラデーションインデックス更新
		GradientIndex = i;

		//グラデーションポジション更新
		LightColorPos = p;

		//ライトカラー設定
		LightComp.color = LightColorGradientList[GradientIndex].Evaluate(LightColorPos);
	}

	//ライトをカラーを変える
	public void LightChange(float t, float n, Action act)
	{
		//コルーチン呼び出し
		StartCoroutine(LightChangeCoroutine(t, n, act));
	}

	//ライトをカラーを変えるコルーチン
	private IEnumerator LightChangeCoroutine(float t, float n, Action act)
	{
		//受け取った値と現在値の差を求める
		float LightNum = n - LightColorPos;

		//経過時間
		float FadeTime = 0;

		//フェード時間中ループ
		while (FadeTime < t)
		{
			//経過時間更新
			FadeTime += Time.deltaTime;

			//ライトカラーに反映
			LightComp.color = LightColorGradientList[GradientIndex].Evaluate(LightColorPos + (LightNum * FadeTime / t));

			//1フレーム待機
			yield return null;
		}

		//ライトカラーポジションを更新
		LightColorPos = n;

		//ライトカラーに反映
		LightComp.color = LightColorGradientList[GradientIndex].Evaluate(LightColorPos);

		//匿名関数実行
		act();
	}
}

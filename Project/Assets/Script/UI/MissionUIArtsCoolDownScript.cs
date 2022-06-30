using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionUIArtsCoolDownScript : GlobalClass
{
	//テキストコンポーネント
	private Text ShowText;

	//イメージコンポーネント
	private Image ShowImage;

	//セットされている技
	public ArtsClass Arts;

	//色に使うグラデーション
	public Gradient Gradient = new Gradient();

	private void Start()
    {
		//テキストコンポーネント取得
		ShowText = GetComponentInChildren<Text>();

		//イメージコンポーネント取得
		ShowImage = GetComponent<Image>();

		//最初は白にしとく
		ShowImage.color = Color.white;

		//グラデーションの色
		GradientColorKey[] colorKey = new GradientColorKey[2];

		//グラデーションの透明度
		GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];

		//グラデーションの色設定
		colorKey[0].color = Color.black;
		colorKey[0].time = 0.0f;
		colorKey[1].color = new Color(0.75f, 0.75f, 1);
		colorKey[1].time = 1.0f;

		//グラデーションの透明度設定
		alphaKey[0].alpha = 1.0f;
		alphaKey[0].time = 0.0f;

		//グラデーションに反映
		Gradient.SetKeys(colorKey, alphaKey);

		//テキストを更新
		ShowText.text = Arts.NameC;
	}

	private void Update()
    {
		//クールダウンが終わった
		if(!Arts.CoolDownFlag)
		{
			//白にする
			ShowImage.color = Color.white;
		}
		else
		{
			//色を反映
			ShowImage.color = Gradient.Evaluate(Mathf.InverseLerp(Arts.MaxCoolDownTime, 0, Arts.CoolDownTime));	
		}
	}
}

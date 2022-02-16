using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class B_GaugeScript : GlobalClass
{
	//バリバリゲージの色に使うグラデーション
	public Gradient B_GaugeGradient = new Gradient();

	//バリバリゲージのImage
	private Image B_GaugeImage;

	//バリバリゲージのTransform
	private RectTransform B_GaugeTransform;

	//バリバリゲージのMAX値
	private float B_GaugeMAX;

	//キャラクタースクリプトを持っているオブジェクト
	private GameObject PlayerCharacter;

	void Start()
    {
		//バリバリゲージImage取得
		B_GaugeImage = gameObject.GetComponent<Image>();

		//バリバリゲージのTransform取得
		B_GaugeTransform = gameObject.GetComponent<RectTransform>();

		//キャラクタースクリプトを持っているオブジェクト取得
		PlayerCharacter = gameObject.transform.root.gameObject;

		//バリバリゲージのMAX値取得
		B_GaugeMAX = B_GaugeTransform.rect.height;
	}
	
	void Update()
    {
		/*
		//バリバリゲージの色を反映
		B_GaugeImage.color = B_GaugeGradient.Evaluate(PlayerCharacter.GetComponent<PlayerScript>().B_Gauge);

		//バリバリゲージの長さを反映
		B_GaugeTransform.sizeDelta = new Vector2(B_GaugeTransform.rect.width, B_GaugeMAX * PlayerCharacter.GetComponent<PlayerScript>().B_Gauge);
	*/
	}
}

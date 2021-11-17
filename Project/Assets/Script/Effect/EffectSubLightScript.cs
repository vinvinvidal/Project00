using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSubLightScript : GlobalClass
{
	//サブライト
	private Light SubLight;

	//フレア
	private LensFlare SubFlare;

	//減光時間
	public float DimmingTime;

	//ライトの大きさ
	private float LightRange;

	//フレアの強さ
	private float FlareRange;

	//グラデーション
	public Gradient Grad;

	//開始時間
	private float StartTime;

	//係数
	private float LightNum = 0;

	void Start()
    {
		//サブライト取得
		SubLight = GetComponent<Light>();

		//フレア取得
		SubFlare = GetComponentInChildren<LensFlare>();

		//開始時間取得
		StartTime = Time.time;

		//ライトの大きさ取得
		LightRange = SubLight.range;

		//フレアの大きさ取得
		if (SubFlare != null)
		{
			FlareRange = SubFlare.brightness;
		}		

		//コルーチン呼び出し
		StartCoroutine(SubLightCoroutine());		
	}

	private IEnumerator SubLightCoroutine()
	{
		while(LightNum < 1)
		{
			LightNum = (Time.time - StartTime) / DimmingTime;

			SubLight.range = Mathf.Lerp(LightRange, 0, LightNum);

			SubLight.color = Grad.Evaluate(LightNum);

			if (SubFlare != null)
			{
				SubFlare.brightness = Mathf.Lerp(FlareRange, 0, LightNum);
			}

			yield return null;
		}

		Destroy(gameObject);
	}
}

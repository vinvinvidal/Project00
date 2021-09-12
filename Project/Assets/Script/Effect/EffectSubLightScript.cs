using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSubLightScript : MonoBehaviour
{
	//サブライト
	private Light SubLight;

	//フレア
	private LensFlare SubFlare;

	//減光具合
	public float DimmingTime;

	//開始時間
	private float StartTime;

	void Start()
    {
		//サブライト取得
		SubLight = GetComponent<Light>();

		SubFlare = GetComponentInChildren<LensFlare>();

		//開始時間取得
		StartTime = Time.time;
	}

    void Update()
    {
		SubLight.range = Mathf.Lerp(SubLight.range, 0, (Time.time - StartTime) / DimmingTime) ;

		if(SubFlare != null)
		{
			SubFlare.brightness = Mathf.Lerp(SubFlare.brightness, 0, (Time.time - StartTime) / DimmingTime);
		}		
	}
}

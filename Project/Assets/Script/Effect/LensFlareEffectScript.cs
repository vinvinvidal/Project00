using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface LensFlareEffectScriptInterface : IEventSystemHandler
{
	//レンズフレア演出
	void LensFlareEffect(float t);
}
public class LensFlareEffectScript : GlobalClass, LensFlareEffectScriptInterface
{
	public void LensFlareEffect(float t)
	{
		StartCoroutine(LensFlareEffectCoroutine(t));
	}
	private IEnumerator LensFlareEffectCoroutine(float t)
	{
		//レンズフレア有効化
		gameObject.GetComponent<LensFlare>().enabled = true;

		//時間
		float EffectTime = 0;

		//係数
		float EffectNum = 0;

		while(EffectTime < t)
		{
			//時間加算
			EffectTime += Time.deltaTime;

			//係数算出
			EffectNum = EffectTime / t;

			//オブジェクト移動
			gameObject.transform.localPosition = new Vector3(Mathf.Lerp(-5f , 5f , EffectNum) , Mathf.Lerp(-1, 1, EffectNum) , 10);

			//１フレーム待機
			yield return null;
		}

		//レンズフレア無効化
		gameObject.GetComponent<LensFlare>().enabled = false;
	}
}

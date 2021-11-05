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
	//レンズフレアコンポーネント
	private LensFlare Lens;

	private void Start()
	{
		//レンズフレアコンポーネント取得
		Lens = gameObject.GetComponent<LensFlare>();
	}

	public void LensFlareEffect(float t)
	{
		StartCoroutine(LensFlareEffectCoroutine(t));
	}
	private IEnumerator LensFlareEffectCoroutine(float t)
	{
		//レンズフレア有効化
		Lens.enabled = true;

		//レンズフレアの明るさキャッシュ
		float TempBrightness = Lens.brightness;
			
		//時間
		float EffectTime = 0;

		//係数
		float EffectNum = 0;

		//サインカーブ生成用変数
		float SinTime = 0;

		while(EffectTime < t)
		{
			//時間加算
			EffectTime += Time.deltaTime;

			//係数算出
			EffectNum = EffectTime / t;

			//オブジェクト移動
			gameObject.transform.localPosition = new Vector3(0 , Mathf.Lerp(2f, 0f, EffectNum) , 10);

			//サインカーブ生成用変数カウントアップ
			SinTime += Time.deltaTime;

			//明るさをサインカーブでクロスフェードさせる
			Lens.brightness = TempBrightness * Mathf.Sin(2 * Mathf.PI * (1 / t) * 0.5f * SinTime);

			//１フレーム待機
			yield return null;
		}

		//明るさを戻す
		Lens.brightness = TempBrightness;

		//レンズフレア無効化
		gameObject.GetComponent<LensFlare>().enabled = false;
	}
}

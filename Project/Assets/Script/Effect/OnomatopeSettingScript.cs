using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnomatopeSettingScript : GlobalClass
{
	//メインカメラ
	private Camera MainCamera;

	//Rectトランスフォーム
	private RectTransform Rect;

	//オノマトペ表示
	public void ShowOnomatope(float EffectTime,GameObject Target)
	{
		//表示
		gameObject.GetComponent<Image>().enabled = true;

		if(Target != null)
		{
			//メインカメラ取得
			MainCamera = DeepFind(GameManagerScript.Instance.GetCameraOBJ(), "MainCamera").GetComponent<Camera>();

			//Rectトランスフォーム取得
			Rect = gameObject.GetComponent<RectTransform>();
		}

		//コルーチン呼び出し
		StartCoroutine(ShowOnomatopeCoroutine(EffectTime, Target));
	}
	private IEnumerator ShowOnomatopeCoroutine(float EffectTime, GameObject Target)
	{
		//経過時間宣言
		float StartTime = 0;

		//引数で受け取った持続時間までループ
		while (StartTime < EffectTime)
		{
			if (!GameManagerScript.Instance.PauseFlag)
			{
				if(Target != null)
				{
					Rect.position = MainCamera.WorldToScreenPoint(Target.transform.position);
				}

				//経過時間カウントアップ
				StartTime += Time.deltaTime;
			}

			//１フレーム待機
			yield return null;
		}

		//自身を削除
		Destroy(gameObject);
	}
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnomatopeScript : GlobalClass
{
	//メインカメラ
	private Camera MainCamera;

	//Rectトランスフォーム
	private RectTransform Rect;

	//表示する場所のオフセット
	private Vector3 PosOffset;

	//攻撃ヒットオノマトペ表示
	public void ShowAttackHitOnomatope(Vector3 TargetPos)
	{
		//メインカメラ取得
		MainCamera = DeepFind(GameManagerScript.Instance.GetCameraOBJ(), "MainCamera").GetComponent<Camera>();

		//Rectトランスフォーム取得
		Rect = gameObject.GetComponent<RectTransform>();

		//スケールをゼロにする
		Rect.localScale = Vector3.one * 0.1f;

		//ランダムな値をオフセット値に取る
		PosOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), 0);

		//コルーチン呼び出し
		StartCoroutine(ShowAttackHitOnomatopeCoroutine(TargetPos));
	}
	private IEnumerator ShowAttackHitOnomatopeCoroutine(Vector3 TargetPos)
	{
		//経過時間宣言
		float StartTime = 0;

		//表示位置に移動
		Rect.position = MainCamera.WorldToScreenPoint(TargetPos + PosOffset);

		//画像表示
		gameObject.GetComponent<Image>().enabled = true;

		//拡大
		while (Rect.localScale.x < 1)
		{
			Rect.localScale *= 1.5f;

			//１フレーム待機
			yield return null;
		}

		//スケールを1にする
		Rect.localScale = Vector3.one;

		//引数で受け取った持続時間までループ
		while (StartTime < 0.5f)
		{
			if (!GameManagerScript.Instance.PauseFlag)
			{
				//ランダムで座標を変更して震わせる
				Rect.position = MainCamera.WorldToScreenPoint(TargetPos + PosOffset + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
				
				//経過時間カウントアップ
				StartTime += Time.deltaTime;
			}

			//１フレーム待機
			yield return null;
		}

		//縮小
		while (Rect.localScale.x > 0.1f)
		{
			Rect.localScale *= 0.5f;

			//１フレーム待機
			yield return null;
		}

		//自身を削除
		Destroy(gameObject);
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnomatopeScript : GlobalClass
{
	//メインカメラ
	private Camera MainCamera;

	//Rectトランスフォーム
	private RectTransform Rect;

	//キャンバスRectトランスフォーム
	private RectTransform ParentRect;

	//キャンバススケーラー
	private CanvasScaler ParentScaler;

	//スクリーン座標オフセット
	private Vector3 Offset;

	//スプライトサイズ
	private Vector2 Size; 

	//攻撃ヒットオノマトペ表示
	public void ShowAttackHitOnomatope(Vector3 TargetPos)
	{
		//メインカメラ取得
		MainCamera = GameManagerScript.Instance.GetMainCameraOBJ().GetComponent<Camera>();

		//Rectトランスフォーム取得
		Rect = gameObject.GetComponent<RectTransform>();

		//キャンバスRectトランスフォーム取得
		ParentRect = gameObject.transform.parent.GetComponent<RectTransform>();

		//キャンバススケーラー取得
		ParentScaler = gameObject.transform.parent.GetComponent<CanvasScaler>();

		//スプライトサイズキャッシュ
		Size = Rect.sizeDelta;

		//スプライトサイズを小さくする
		Rect.sizeDelta *= 0.1f;

		//ランダムな値をオフセット値に取る
		TargetPos += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f), Random.Range(-0.5f, 0.5f));

		//コルーチン呼び出し
		StartCoroutine(ShowAttackHitOnomatopeCoroutine(TargetPos));
	}
	private IEnumerator ShowAttackHitOnomatopeCoroutine(Vector3 TargetPos)
	{
		//経過時間宣言
		float StartTime = 0;

		//表示位置に移動
		Rect.position = UIPosition(ParentScaler, ParentRect, TargetPos);

		//画像表示
		gameObject.GetComponent<Image>().enabled = true;

		//拡大
		while (Rect.sizeDelta.x < Size.x * 1.5f)
		{
			Rect.sizeDelta *= 1.5f;

			//１フレーム待機
			yield return null;
		}

		//キャンバス比率を元にスケールを設定
		Rect.sizeDelta = Size;

		//引数で受け取った持続時間までループ
		while (StartTime < 0.5f)
		{
			if (!GameManagerScript.Instance.PauseFlag)
			{
				//ランダムで座標を変更して震わせる
				Rect.position = UIPosition(ParentScaler, ParentRect, TargetPos + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));

				//経過時間カウントアップ
				StartTime += Time.deltaTime;
			}

			//１フレーム待機
			yield return null;
		}

		//縮小
		while (Rect.sizeDelta.x > 0.1f)
		{
			Rect.sizeDelta *= 0.5f;

			//１フレーム待機
			yield return null;
		}

		//自身を削除
		Destroy(gameObject);
	}
}

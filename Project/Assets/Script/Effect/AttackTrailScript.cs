using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrailScript : GlobalClass
{
	//マテリアル
	private Material Mat;

	//テクスチャアニメーション用Vector2
	private Vector2 ScaleVec = new Vector2(0,1);
	private Vector2 OffsetVec = Vector2.zero;

	//速度
	public float EffectSpeed;

	void Start()
    {
		//マテリアル取得
		Mat = GetComponent<Renderer>().material;

		//コルーチン呼び出し
		StartCoroutine(AttackTrailCoroutine());
	}
	private IEnumerator AttackTrailCoroutine()
	{
		//テクスチャを拡大
		while(ScaleVec.x < 1)
		{
			//ポーズ中は処理しない
			if (!GameManagerScript.Instance.PauseFlag)
			{
				//スケール加算
				ScaleVec.x += EffectSpeed;

				//テクスチャに反映
				Mat.SetTextureScale("_TexParticle", ScaleVec);
			}

			//1フレーム待機
			yield return null;
		}

		//テクスチャを縮小
		while (ScaleVec.x > 0)
		{
			//ポーズ中は処理しない
			if (!GameManagerScript.Instance.PauseFlag)
			{
				//スケール減算
				ScaleVec.x -= EffectSpeed;

				//位置移動
				OffsetVec.x += EffectSpeed;

				//テクスチャに反映
				Mat.SetTextureScale("_TexParticle", ScaleVec);
				Mat.SetTextureOffset("_TexParticle", OffsetVec);
			}

			//1フレーム待機
			yield return null;
		}

		//オブジェクト削除
		Destroy(gameObject);
	}
}

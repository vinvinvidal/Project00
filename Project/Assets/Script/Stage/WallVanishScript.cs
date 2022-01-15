using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallVanishScript : GlobalClass
{
	//マテリアル
	private Material mat;

	//消滅用数値
	private float VanishCount;

	void Start()
    {
		//消滅用数値の初期値をランダムに設定
		VanishCount = Random.Range(-10, 0);

		//マテリアル取得
		mat = GetComponentInChildren<Renderer>().material;

		//描画順を変更
		mat.renderQueue = 3000;

		//レンダラーのシャドウを切る
		GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		GetComponentInChildren<Renderer>().receiveShadows = false;
	}

    void Update()
    {
		//シェーダーに消滅用数値を渡す
		mat.SetFloat("_VanishNum", VanishCount);

		//消滅用数値カウントアップ
		VanishCount += Time.deltaTime * 10;

		//消えたら自身を削除
		if(VanishCount > 20)
		{
			Destroy(gameObject);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallVanishScript : GlobalClass
{
	//マテリアル
	private Material mat;

	//消滅用数値
	private float VanishCount = 0;

	void Start()
    {
		//消滅用数値の初期値をランダムに設定
		//VanishCount = Random.Range(-10, 0);

		//マテリアル取得
		mat = GetComponentInChildren<Renderer>().material;

		//描画順を変更
		//mat.renderQueue = 3000;

		//レンダラーのシャドウを切る
		GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		GetComponentInChildren<Renderer>().receiveShadows = false;

		GetComponentInChildren<Renderer>().gameObject.layer = LayerMask.NameToLayer("OutDoor");

		mat.SetTextureScale("_VanishTex", new Vector2(Screen.width / mat.GetTexture("_VanishTex").width, Screen.height / mat.GetTexture("_VanishTex").height) * GameManagerScript.Instance.ScreenResolutionScale);
	}

    void Update()
    {
		//シェーダーに消滅用数値を渡す
		mat.SetFloat("_VanishNum", VanishCount / 1.5f);

		//消滅用数値カウントアップ
		VanishCount += Time.deltaTime;

		//消えたら自身を削除
		if(VanishCount > 2)
		{
			Destroy(gameObject);
		}
	}
}

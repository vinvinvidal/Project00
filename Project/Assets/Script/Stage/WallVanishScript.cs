using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallVanishScript : GlobalClass
{
	//マテリアル
	private Material mat;

	//経過時間
	private float VanishCount = 0;

	//消滅までの時間
	private float VanishTime = 2;

	void Start()
    {
		//マテリアル取得
		mat = GetComponentInChildren<Renderer>().material;

		//レンダラーのシャドウを切る
		GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		GetComponentInChildren<Renderer>().receiveShadows = false;

		GetComponentInChildren<Renderer>().gameObject.layer = LayerMask.NameToLayer("OutDoor");

		mat.SetTexture("_VanishTexture", GameManagerScript.Instance.VanishTextureList[0]);

		mat.SetTextureScale("_VanishTexture", new Vector2(Screen.width / GameManagerScript.Instance.VanishTextureList[0].width, Screen.height / GameManagerScript.Instance.VanishTextureList[0].height) * GameManagerScript.Instance.ScreenResolutionScale);

		mat.renderQueue = 3000;
	}

    void Update()
    {
		//消滅用数値カウントアップ
		VanishCount += Time.deltaTime;

		mat.SetTexture("_VanishTexture", GameManagerScript.Instance.VanishTextureList[(int)Mathf.Ceil(Mathf.Lerp(0, GameManagerScript.Instance.VanishTextureList.Count - 1, VanishCount / VanishTime))]);

		//消えたら自身を削除
		if (VanishCount > VanishTime)
		{
			Destroy(gameObject);
		}
	}
}

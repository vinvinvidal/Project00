using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallVanishScript : GlobalClass
{
	void Start()
	{
		//消失用関数呼び出し
		ObjectVanish(gameObject, 2, 0,

		//事前処理
		(List<Renderer> R) =>
		{
			//レンダラーを回す
			foreach (Renderer i in R)
			{
				//レンダラーのシャドウを切る
				i.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				i.receiveShadows = false;

				//レイヤーを変えてアウトラインを切る
				i.gameObject.layer = LayerMask.NameToLayer("OutDoor");

				//描画順を変えて後ろオブジェクトの影とかをちゃんと見えるようにする
				foreach (Material ii in i.sharedMaterials.Where(a => a != null))
				{
					//マテリアルの描画順を変更
					ii.renderQueue = 3000;
				}
			}
		},
		//事後処理
		(List<Renderer> R) =>
		{
			//消えたら自身を削除
			Destroy(gameObject);
		});
	}

	/*
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
	}*/
}

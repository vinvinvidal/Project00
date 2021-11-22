using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapShaderScript : GlobalClass
{
	//ミニマップマテリアル
	Material MiniMapMaterial;

	//プレイヤーキャラクター
	GameObject PlayerCharacter;

    void Start()
    {
		//ミニマップマテリアル取得
		MiniMapMaterial = gameObject.GetComponent<Renderer>().material;

		//コルーチン呼び出し
		StartCoroutine(MiniMapCoroutine());
	}

	private IEnumerator MiniMapCoroutine()
	{
		//プレイヤーキャラクターを取得するまで待機
		while(PlayerCharacter == null)
		{
			//プレイヤーキャラクターを取得
			PlayerCharacter = GameManagerScript.Instance.GetPlayableCharacterOBJ();

			//１フレーム待機
			yield return null;
		}

		while(PlayerCharacter != null)
		{
			//シェーダーにプレイヤーキャラクターの位置を渡す
			MiniMapMaterial.SetFloat("_PlayerCharacterPos", PlayerCharacter.transform.position.y);

			//１フレーム待機
			yield return null;
		}
	}
}

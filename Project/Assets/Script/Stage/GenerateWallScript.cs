using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWallScript : GlobalClass
{
	//リジッドボディ
	private Rigidbody R_body;

	private void Start()
	{
		//リジッドボディ取得
		R_body = GetComponent<Rigidbody>();

		//物理を切る
		R_body.isKinematic = true;

		//壁生成コルーチン呼び出し
		//StartCoroutine(GenerateWallCoroutine());
	}

	private IEnumerator GenerateWallCoroutine()
	{

		//壁にするために水平方向の移動をフリーズ
		R_body.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

		//ちょっと動かす為に1秒待機
		yield return new WaitForSeconds(1);

		//停止するまでループ
		while (R_body.velocity.sqrMagnitude > 0.001f)
		{
			yield return null;			
		}

		//物理を切る
		R_body.isKinematic = true;

		//フリーズポジションを解除
		R_body.constraints = RigidbodyConstraints.None;

		//このスクリプトを無効化
		GetComponent<GenerateWallScript>().enabled = false;
	}
}

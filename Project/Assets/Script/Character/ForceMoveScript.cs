using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ForceMoveScript : GlobalClass
{
	//キャラクターオブジェクト
	GameObject CharacterOBJ;

	//このスクリプトが付いているオブジェクトのレイヤー名
	string LayerName;

	void Start()
	{
		//プレイヤーキャラクターオブジェクト取得
		CharacterOBJ = transform.root.gameObject;

		//レイヤー名取得
		LayerName = LayerMask.LayerToName(CharacterOBJ.layer);

		//コライダの設定をキャラクターコントローラから求める
		GetComponentInChildren<CapsuleCollider>().center = CharacterOBJ.GetComponentInChildren<CharacterController>().center;
		GetComponentInChildren<CapsuleCollider>().radius = CharacterOBJ.GetComponentInChildren<CharacterController>().radius + 0.1f;
		GetComponentInChildren<CapsuleCollider>().height = CharacterOBJ.GetComponentInChildren<CharacterController>().height + 0.5f;
	}
	/*
	//強制移動コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{		
		if(LayerName == "Player")
		{
			//プレイヤーキャラクターオブジェクトにヒットしたコライダを送る
			ExecuteEvents.Execute<PlayerScriptInterface>(CharacterOBJ, null, (reciever, eventData) => reciever.ForceMoveHit(true, Hit));
		}
		else if(LayerName == "Enemy")
		{
			//敵キャラクターオブジェクトにヒットしたコライダを送る
			ExecuteEvents.Execute<EnemyCharacterInterface>(CharacterOBJ, null, (reciever, eventData) => reciever.ForceMoveHit(true, Hit));
		}
	}
	//強制移動コライダーが外れた時に呼び出される
	private void OnTriggerExit(Collider Hit)
	{
		if (LayerName == "Player")
		{
			//プレイヤーキャラクターオブジェクトにnullを送る
			ExecuteEvents.Execute<PlayerScriptInterface>(CharacterOBJ, null, (reciever, eventData) => reciever.ForceMoveHit(false, Hit));
		}
		else if (LayerName == "Enemy")
		{
			//敵キャラクターオブジェクトにnullを送る
			ExecuteEvents.Execute<EnemyCharacterInterface>(CharacterOBJ, null, (reciever, eventData) => reciever.ForceMoveHit(false, Hit));
		}
	}*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyWeaponColScript : GlobalClass
{
	//コライダ
	private MeshCollider WeaponCol;

	//リジッドボディ
	private Rigidbody RBody;

	//当たった攻撃クラス
	public EnemyAttackClass UseArts;

	//投げた敵オブジェクト
	public GameObject Enemy;

	void Start()
	{
		//コライダ取得
		WeaponCol = gameObject.GetComponent<MeshCollider>();

		//リジッドボディ取得
		RBody = gameObject.GetComponent<Rigidbody>();
	}

	//コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//攻撃が有効か判定する変数宣言
		bool AttackEnable = false;

		//プレイヤーにヒットした時の処理
		if (LayerMask.LayerToName(Hit.gameObject.layer) == "PlayerDamageCol")
		{
			//プレイヤーキャラクターのスクリプトを呼び出して攻撃が有効か判定
			ExecuteEvents.Execute<PlayerScriptInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => AttackEnable = reciever.AttackEnable(false));

			//攻撃が有効ならプレイヤー側の処理を呼び出す
			if (AttackEnable)
			{
				//プレイヤーキャラクターのスクリプトを呼び出す
				ExecuteEvents.Execute<PlayerScriptInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.HitEnemyAttack(UseArts, Enemy, gameObject));
			}
		}

		//攻撃が有効、もしくはプレイヤーのダメージコライダ以外に当たった
		if (AttackEnable || LayerMask.LayerToName(Hit.gameObject.layer) != "PlayerDamageCol")
		{
			//コライダを物理化
			WeaponCol.isTrigger = false;

			//レイヤーを物理化
			gameObject.layer = LayerMask.NameToLayer("PhysicOBJ");

			//背景に当たった
			if (LayerMask.LayerToName(Hit.gameObject.layer) == "TransparentFX")
			{
				//オブジェクトに消失用スクリプト追加
				gameObject.AddComponent<WallVanishScript>();

				//自身をゲームマネージャーのListから消す
				GameManagerScript.Instance.AllEnemyWeaponList.Remove(gameObject);
			}
			else
			{
				//RigidBodyの補完を有効化
				RBody.interpolation = RigidbodyInterpolation.Interpolate;

				//跳ね返りの加速度を加える
				RBody.AddForce((gameObject.transform.position - Hit.gameObject.transform.root.gameObject.transform.position).normalized * 10, ForceMode.Impulse);
			}
		}
	}
}

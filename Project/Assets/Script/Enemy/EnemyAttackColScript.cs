using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface EnemyAttackCollInterface : IEventSystemHandler
{
	//コライダ出現処理
	void ColStart(int n, EnemyAttackClass arts);

	//スケベ攻撃コライダ出現処理
	void H_ColStart();

	//コライダ終了処理
	void ColEnd();
}

public class EnemyAttackColScript : GlobalClass, EnemyAttackCollInterface
{
	//攻撃用コライダ
	private BoxCollider AttackCol;

	//当たった攻撃
	private EnemyAttackClass HitArts;

	//スケベ攻撃フラグ
	private bool H_AttackFlag = false;

	private void Start()
	{
		//攻撃用コライダ取得
		AttackCol = gameObject.GetComponent<BoxCollider>();
	}

	//攻撃コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//攻撃が有効か判定する変数宣言
		bool AttackEnable = false;

		//プレイヤーキャラクターのスクリプトを呼び出して攻撃が有効か判定
		ExecuteEvents.Execute<PlayerScriptInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => AttackEnable = reciever.AttackEnable(H_AttackFlag));

		//攻撃が有効なら処理
		if (AttackEnable)
		{
			//プレイヤーキャラクターのスクリプトを呼び出す
			ExecuteEvents.Execute<PlayerScriptInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.HitEnemyAttack(HitArts, gameObject.transform.root.gameObject , H_AttackFlag));

			//スケベ攻撃の場合は敵側のスクリプトを呼び出す
			if (H_AttackFlag)
			{
				//敵キャラクターのスクリプトを呼び出す
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.H_AttackHit());
			}
		}
	}

	//コライダ出現処理、メッセージシステムから呼び出される
	public void ColStart(int n, EnemyAttackClass arts)
	{
		//スケベ攻撃フラグを下ろす
		H_AttackFlag = false;

		//攻撃コライダ有効化
		AttackCol.enabled = true;

		//攻撃をキャッシュ
		HitArts = arts;

		//受け取ったintでコライダの出現位置を決める
		switch (n)
		{
			case 0:

				//出現位置・前方
				AttackCol.center = new Vector3(0, 0.5f, 1.5f);

				//大きさ
				AttackCol.size = new Vector3(0.5f, 1f, 1f);
				break;

			default: break;

		}
	}

	//スケベ攻撃コライダ出現処理、メッセージシステムから呼び出される
	public void H_ColStart()
	{
		//スケベ攻撃フラグを立てる
		H_AttackFlag = true;

		//攻撃コライダ有効化
		AttackCol.enabled = true;

		//出現位置設定
		AttackCol.center = new Vector3(0, 0.5f, 1f);

		//大きさ設定
		AttackCol.size = new Vector3(0.5f, 1f, 1f);
	}

	//コライダ終了処理処理、メッセージシステムから呼び出される
	public void ColEnd()
	{
		//コライダを非アクティブ化
		AttackCol.enabled = false;

		//コライダを待機位置に移動
		AttackCol.center = Vector3.zero;

		//コライダの半径をデフォルトに戻す
		AttackCol.size = new Vector3(1, 1, 1);

		//当たった攻撃を初期化
		HitArts = null;
	}
}




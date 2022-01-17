using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowWeaponScript : GlobalClass
{
	//コライダ
	private MeshCollider WeaponCol;

	//リジッドボディ
	private Rigidbody RBody;

	//当たった攻撃クラス
	public EnemyAttackClass UseArts;

	//投げた敵オブジェクト
	public GameObject Enemy;

	//当たった時に表示するエフェクト
	public GameObject HitEffect;

	//ダメージ
	public int Damage;

	void Start()
	{
		//コライダ取得
		WeaponCol = gameObject.GetComponent<MeshCollider>();

		//リジッドボディ取得
		RBody = gameObject.GetComponent<Rigidbody>();
	}

	//オブジェクトを無害化する
	public void PhysicOBJ()
	{
		//コライダを物理化
		WeaponCol.isTrigger = false;

		//レイヤーを物理化
		gameObject.layer = LayerMask.NameToLayer("PhysicOBJ");
	}

	//オブジェクトをプレイヤーの武器にする
	public void PlyaerAttack()
	{
		//コライダをトリガー化
		WeaponCol.isTrigger = true;

		//レイヤーをプレイヤーの攻撃にする
		gameObject.layer = LayerMask.NameToLayer("PlayerWeaponCol");
	}

	//コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//攻撃が有効か判定する変数宣言
		bool AttackEnable = false;

		//プレイヤーにヒットした
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
		//敵に当たった
		else if(LayerMask.LayerToName(Hit.gameObject.layer) == "EnemyDamageCol")
		{
			//オブジェクトを無害化
			PhysicOBJ();

			//速度をリセット
			RBody.velocity = Vector3.zero;

			//跳ね返りの加速度を加える
			RBody.AddForce((gameObject.transform.position - Hit.gameObject.transform.root.gameObject.transform.position).normalized * 5, ForceMode.Impulse);

			//オブジェクトに消失用スクリプト追加
			gameObject.AddComponent<WallVanishScript>();

			//自身をゲームマネージャーのListから消す
			GameManagerScript.Instance.AllEnemyWeaponList.Remove(gameObject);

			//ヒットエフェクトインスタンス生成
			GameObject TempHitEffect = Instantiate(HitEffect);

			//子にする
			TempHitEffect.transform.parent = gameObject.transform;

			//PRS設定
			TempHitEffect.transform.localPosition = Vector3.zero;
			TempHitEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
			ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { UseArts.PlyaerUseKnockBackVec }, new List<int>() { UseArts.PlyaerUseDamage }, new List<int>() { UseArts.PlyaerUseDamageType }), 0));
		}
		//床に落ちた
		else if(LayerMask.LayerToName(Hit.gameObject.layer) == "TransparentFX")
		{
			//オブジェクトに消失用スクリプト追加
			gameObject.AddComponent<WallVanishScript>();

			//自身をゲームマネージャーのListから消す
			GameManagerScript.Instance.AllEnemyWeaponList.Remove(gameObject);

			//オブジェクトを無害化
			PhysicOBJ();

			//速度を変えてバウンドさせる
			RBody.velocity = new Vector3(RBody.velocity.x * 0.5f, 2.5f, RBody.velocity.z * 0.5f);
		}
	}
}

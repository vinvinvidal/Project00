using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface ThrowWeaponScriptInterface : IEventSystemHandler
{
	//ポーズ処理
	void Pause(bool b);
}

public class ThrowWeaponScript : GlobalClass, ThrowWeaponScriptInterface
{
	//コライダ
	private MeshCollider WeaponCol;

	//リジッドボディ
	private Rigidbody RBody;

	//当たった攻撃クラス
	public EnemyAttackClass UseArts { get; set; }

	//投げた敵オブジェクト
	public GameObject Enemy { get; set; }

	//当たった時に表示するエフェクト
	public GameObject HitEffect { get; set; }

	//プレイヤーにストックされているフラグ
	//public bool StockFlag { get; set; } = false;

	//賎祓いフラグ
	private bool Special110Flag = false;

	//プレイヤーにストックされた時のポジション
	public Vector3 StockPosition;

	//ポーズ時の加速値キャッシュ
	private Vector3 Vvelocity = new Vector3();

	//ポーズ時の回転値キャッシュ
	private Vector3 Avelocity = new Vector3();

	void Start()
	{
		//コライダ取得
		WeaponCol = gameObject.GetComponent<MeshCollider>();

		//リジッドボディ取得
		RBody = gameObject.GetComponent<Rigidbody>();
	}

	//ポーズ処理
	public void Pause(bool b)
	{
		//ポーズ実行
		if(b)
		{
			//加速度キャッシュ
			Vvelocity = RBody.velocity;

			//回転値キャッシュ
			Avelocity = RBody.angularVelocity;

			//一時停止
			RBody.Sleep();
		}
		//ポーズ解除
		else
		{
			//再生
			RBody.WakeUp();

			//加速度適応
			RBody.velocity = Vvelocity;

			//回転値適応
			RBody.angularVelocity = Avelocity;
		}
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

	//賎祓い用処理
	public void SpecialArts110(List<GameObject> EnemyList)
	{
		//賎祓いフラグを立てる
		Special110Flag = true;

		//プレイヤーの武器にする
		PlyaerAttack();

		//飛び道具のRigidBody有効化
		RBody.isKinematic = false;

		//コルーチン呼び出し
		StartCoroutine(SpecialArts110Coroutine(EnemyList));
	}
	private IEnumerator SpecialArts110Coroutine(List<GameObject> EnemyList)
	{
		//ループカウント
		int count = 0;

		//開始時間キャッシュ
		float StartTime = Time.time;

		//周囲の敵を回す
		foreach(var i in EnemyList)
		{
			//速度をリセット
			RBody.velocity = Vector3.zero;

			//飛び道具を回転させる
			RBody.AddTorque(transform.right, ForceMode.Impulse);

			//当たるまで待機
			while (((i.transform.position + (Vector3.up * 0.75f)) - transform.position).sqrMagnitude > 1.5f)
			{
				//飛び道具を敵に飛ばす
				RBody.AddForce(((i.transform.position + (Vector3.up * 0.75f)) - transform.position).normalized * 5, ForceMode.Impulse);

				//1フレーム待機
				yield return null;
			}

			//ヒットエフェクトインスタンス生成
			GameObject TempHitEffect = Instantiate(HitEffect);

			//子にする
			TempHitEffect.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(TempHitEffect);

			//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
			ExecuteEvents.Execute<EnemyCharacterInterface>(i, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { UseArts.PlyaerUseKnockBackVec }, new List<float>() { UseArts.PlyaerUseDamage }, new List<int>() { 1 }, new List<int>() { UseArts.PlyaerUseDamageType }, new List<int>() { 1 }, new List<int>() { 0 }), 0));
			
			//最後の一匹かどうか
			if (EnemyList.Count - count == 1)
			{
				//消失処理
				BrokenWeapon();
			}
			else
			{
				//ループカウントアップ
				count++;
			}			
		}
	}

	//オブジェクトを叩きつけた時に呼ばれる
	public void BrokenWeapon()
	{
		//自身をゲームマネージャーのListから消す
		GameManagerScript.Instance.AllEnemyWeaponList.Remove(gameObject);

		//親を解除
		transform.parent = null;

		//オブジェクトを無害化
		PhysicOBJ();

		//飛び道具のRigidBody有効化
		RBody.isKinematic = false;

		//速度をリセット
		RBody.velocity = Vector3.zero;

		//ちょい浮かす
		RBody.AddForce(Vector3.up * 5, ForceMode.Impulse);

		//回転させる
		RBody.AddTorque((transform.right + transform.up) * 5, ForceMode.Impulse);

		//オブジェクトに消失用スクリプト追加
		gameObject.AddComponent<WallVanishScript>();
	}

	//コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//攻撃が有効か判定する変数宣言
		bool AttackEnable = false;

		//賎祓い用処理
		if (Special110Flag)
		{
			//何もしない
		}
		//プレイヤーにヒットした
		else if (LayerMask.LayerToName(Hit.gameObject.layer) == "PlayerDamageCol")
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

			//トランスフォームリセット
			ResetTransform(TempHitEffect);

			//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
			ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { UseArts.PlyaerUseKnockBackVec }, new List<float>() { UseArts.PlyaerUseDamage }, new List<int>() { 1 }, new List<int>() { UseArts.PlyaerUseDamageType }, new List<int>() { 1 }, new List<int>() { 0 }), 0));
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

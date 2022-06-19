using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface Character2WeaponColInterface : IEventSystemHandler
{
	//コライダのアクティブを切り替える
	void SwitchCol(bool b, bool special);

	//ポーズ処理
	void Pause(bool b);
}

public class Character2WeaponColScript : GlobalClass, Character2WeaponColInterface
{
	//コライダ
	private SphereCollider WeaponCol;

	//キャラクターオブジェクト
	private GameObject CharacterOBJ;

	//爆発エフェクトオブジェクト
	private GameObject BombEffect;

	//ワイヤーヒットオブジェクト
	private GameObject WireEffect;

	//時限爆発コルーチン
	private Coroutine BombCoroutine = null;

	//ワイヤーか爆弾か
	public int WeaponIndex;

	//特殊攻撃判別bool
	public bool SpecialBool = false;

	//ポーズ時の加速値キャッシュ
	private Vector3 Vvelocity = new Vector3();

	//ポーズ時の回転値キャッシュ
	private Vector3 Avelocity = new Vector3();

	//リジッドボディ取得
	private Rigidbody RBody;

	void Start()
	{
		//コライダ取得
		WeaponCol = GetComponent<SphereCollider>();

		//キャラクターオブジェクト取得
		CharacterOBJ = gameObject.transform.root.gameObject;

		//爆発エフェクト取得
		BombEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "BombEffect").ToArray()[0];

		//ワイヤーヒットオブジェクト
		WireEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "HitEffect23").ToArray()[0];

		//爆弾の場合処理
		if (WeaponIndex == 1)
		{
			//リジッドボディ取得
			RBody = gameObject.GetComponent<Rigidbody>();
		}
		//爆発の場合処理
		if (WeaponIndex == 2)
		{
			//最初は消しとくコルーチン呼び出し
			StartCoroutine(BombColCoroutine());
		}
	}

	//ポーズ処理
	public void Pause(bool b)
	{
		//爆弾かどうか
		if(WeaponIndex == 1)
		{
			//ポーズ実行
			if (b)
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
	}

	//爆弾が地面に当たったら呼ばれる
	void OnCollisionEnter(Collision collision)
	{
		//コリジョンを小さくして地面に転がす
		WeaponCol.radius = 0.07f;

		//時限爆発コルーチン呼び出し
		if(BombCoroutine == null)
		{
			BombCoroutine = StartCoroutine(BombCountDownCoroutine());
		}		
	}

	//攻撃コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//ワイヤー
		if(WeaponIndex == 0)
		{
			//コライダを無効化
			WeaponCol.enabled = false;

			//コライダが敵に当たった
			if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
			{
				//特殊攻撃
				if(SpecialBool)
				{
					//架空の技を作成
					ArtsClass TempArts = MakeInstantArts(new List<Color>() { new Color(0, 0, 0, 0) }, new List<float>() { 0 }, new List<int>() { 0 }, new List<int>() { 41 }, new List<int>() { 0 }, new List<int>() { 8 });

					//攻撃判定bool
					bool TempBool = false;

					//攻撃が有効か判定
					ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => TempBool = reciever.AttackEnable(TempArts, 0));

					//有効なら処理実行
					if (TempBool && Hit.gameObject.transform.root.gameObject.GetComponent<EnemyCharacterScript>().Life >= 0)
					{
						//当たった敵をロック対象にする
						CharacterOBJ.GetComponent<Character2WeaponMoveScript>().LockEnemy = Hit.gameObject.transform.root.gameObject;

						//敵当たりフラグを立てる
						CharacterOBJ.GetComponent<Character2WeaponMoveScript>().EnemyHitFlag = true;

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(TempArts, 0));
					}
					//無効なら壁に当たった事にして巻き戻す
					else
					{
						//壁当たりフラグを立てる
						CharacterOBJ.GetComponent<Character2WeaponMoveScript>().WallHitFlag = true;
					}
				}
				//通常攻撃
				else
				{
					//架空の技を作成
					ArtsClass TempArts = MakeInstantArts(new List<Color>() { new Color(0, 0, 0, 0) }, new List<float>() { 0 }, new List<int>() { 0 }, new List<int>() { 4 }, new List<int>() { 1 }, new List<int>() { 8 });

					//攻撃判定bool
					bool TempBool = false;

					//攻撃が有効か判定
					ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => TempBool = reciever.AttackEnable(TempArts, 0));

					//有効なら処理実行
					if (TempBool && Hit.gameObject.transform.root.gameObject.GetComponent<EnemyCharacterScript>().Life >= 0)
					{
						//エフェクト生成
						GameObject TempEfffect = Instantiate(WireEffect);

						//位置を設定
						TempEfffect.transform.position = gameObject.transform.position;

						//当たった敵をロック対象にする
						CharacterOBJ.GetComponent<Character2WeaponMoveScript>().LockEnemy = Hit.gameObject.transform.root.gameObject;

						//ワイヤーヒット関数呼び出し
						CharacterOBJ.GetComponent<Character2WeaponMoveScript>().HitWire(Hit.gameObject.transform.root.gameObject);

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(TempArts, 0));
					}
				}
			}
			//コライダが壁に当たった
			else if (Hit.gameObject.layer == LayerMask.NameToLayer("TransparentFX"))
			{
				//壁当たりフラグを立てる
				CharacterOBJ.GetComponent<Character2WeaponMoveScript>().WallHitFlag = true;
			}
		}
		//爆弾
		else if (WeaponIndex == 1)
		{
			//コライダが敵に当たった
			if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
			{
				//コライダを無効化
				WeaponCol.enabled = false;

				//敵当たりフラグを立てる
				CharacterOBJ.GetComponent<Character2WeaponMoveScript>().EnemyHitFlag = true;

				//爆発エフェクト生成
				GameObject TempEfffect = Instantiate(BombEffect);

				//位置を設定
				TempEfffect.transform.position = gameObject.transform.position;

				//時限爆発コルーチンを止める
				if(BombCoroutine != null)
				{
					StopCoroutine(BombCoroutine);
				}

				//時限爆発コルーチン初期化
				BombCoroutine = null;

				//自身をリストから削除
				GameManagerScript.Instance.AllPlayerWeaponList.Remove(gameObject);

				//自身を削除
				Destroy(gameObject);
			}
		}
		//爆発
		else if (WeaponIndex == 2)
		{
			//コライダが敵に当たった
			if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
			{
				//架空の技を作成
				ArtsClass TempArts = MakeInstantArts(new List<Color>() { new Color(0, 2, 0, 0.1f) }, new List<float>() { 1 }, new List<int>() { 1 }, new List<int>() { 11 }, new List<int>() { 1 }, new List<int>() { 0 });

				//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
				ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(TempArts, 0));
			}
		}
	}

	//時限爆発コルーチン
	public IEnumerator BombCountDownCoroutine()
	{
		//開始時間更新
		float BombTime = Time.time;

		//待機
		while (BombTime + 3 > Time.time)
		{
			if (GameManagerScript.Instance.PauseFlag)
			{
				BombTime += Time.deltaTime;
			}

			yield return null;
		}

		//爆発エフェクト生成
		GameObject TempEfffect = Instantiate(BombEffect);

		//位置を設定
		TempEfffect.transform.position = gameObject.transform.position;

		//自身をリストから削除
		GameManagerScript.Instance.AllPlayerWeaponList.Remove(gameObject);

		//自身を削除
		Destroy(gameObject);
	}

	//コライダのアクティブを切り替える
	public void SwitchCol(bool b, bool special)
	{
		WeaponCol.enabled = b;

		SpecialBool = special;
	}

	//爆発のコライダ無効化コルーチン
	private IEnumerator BombColCoroutine()
	{
		//開始時間キャッシュ
		float BombTime = Time.time;

		//経過時間が過ぎるまでループ
		while(BombTime + 0.05f > Time.time)
		{
			yield return null;
		}

		//コライダ無効化
		WeaponCol.enabled = false;
	}
}

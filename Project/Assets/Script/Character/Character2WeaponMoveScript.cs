using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface Character2WeaponMoveInterface : IEventSystemHandler
{
	//ワイヤーを動かす
	void MoveWire(int n);

	//燐糞を生成する
	void CreateBomb();

	//燐糞のアタッチ先と位置を設定する
	void SettingBombPos(Transform Atc, Vector3 Pos);

	//蔓鱗粉攻撃用
	void SpecialBombMove();

	//特殊攻撃失敗処理
	void SpecialAttackMiss();
}

public class Character2WeaponMoveScript : GlobalClass, Character2WeaponMoveInterface
{
	//ボーンList
	public List<GameObject> BoneList = new List<GameObject>();

	//燐糞オブジェクト
	public GameObject BombOBJ;

	//燐糞オブジェクトインスタンス
	private GameObject BombInst;

	//ロック中の敵
	public GameObject LockEnemy = null;

	//敵にヒットしたフラグ
	public bool EnemyHitFlag = false;

	//壁にヒットしたフラグ
	public bool WallHitFlag = false;

	// Start is called before the first frame update
	void Start()
	{

	}
	// Update is called once per frame
	void Update()
    {

    }

	//燐糞を生成する
	public void CreateBomb()
	{
		//オブジェクト初期化
		BombInst = null;

		//オブジェクトインスタンス生成
		BombInst = Instantiate(BombOBJ);

		//レンダラー有効化
		foreach(var i in BombInst.GetComponentsInChildren<Renderer>())
		{
			if(i.name.Contains("Mesh"))
			{
				i.enabled = true;
			}
		}		

		//トランスフォームリセット
		ResetTransform(BombInst);
	}

	//燐糞のアタッチ先と位置を設定する
	public void SettingBombPos(Transform Atc, Vector3 Pos)
	{
		if(BombInst != null)
		{
			//親を設定
			BombInst.transform.parent = Atc;

			//トランスフォームリセット
			ResetTransform(BombInst);

			//位置を設定
			BombInst.transform.localPosition = Pos;
		}
	}

	//燐糞に着火する
	public void BombIgnition()
	{
		if (BombInst != null)
		{
			BombInst.GetComponentInChildren<ParticleSystem>().Play();
		}
	}

	//蔓燐糞攻撃用
	public void SpecialBombMove()
	{
		if(BombInst != null)
		{
			//トランスフォームリセット
			ResetTransform(BombInst);

			//親を解除
			BombInst.transform.parent = null;

			//フラグ初期化
			EnemyHitFlag = false;

			//移動コルーチン呼び出し
			StartCoroutine(SpecialBombMoveCoroutine());
		}
	}
	private IEnumerator SpecialBombMoveCoroutine()
	{
		//モーション停止
		gameObject.GetComponent<Animator>().SetFloat("SpecialAttackSpeed", 0.1f);

		//コライダ有効化
		BombInst.GetComponent<Character2WeaponColScript>().SwitchCol(true);

		//時間をキャッシュ
		float BombTime = Time.time;

		while (!EnemyHitFlag && (BombTime + 1.5f) > Time.time)
		{
			//1フレーム待機
			yield return null;

			//燐糞移動
			BombInst.transform.position += (BoneList[5].transform.position - BombInst.transform.position).normalized * ((BoneList[4].transform.position - BombInst.transform.position).magnitude + 1) * 3 * Time.deltaTime;
		}

		//コライダ無効化
		BombInst.GetComponent<Character2WeaponColScript>().SwitchCol(false);

		//モーション再生
		gameObject.GetComponent<Animator>().SetFloat("SpecialAttackSpeed", 1);

		//オブジェクト削除
		Destroy(BombInst);

		//ワイヤー巻き戻し
		MoveWire(2);
	}

	//特殊攻撃失敗処理
	public void SpecialAttackMiss()
	{
		//コライダ無効化
		ExecuteEvents.Execute<Character2WeaponColInterface>(BoneList[5], null, (reciever, eventData) => reciever.SwitchCol(false));

		//フラグを立てて壁に当たった事にする
		WallHitFlag = true;
	}

	//ワイヤーを動かす
	public void MoveWire(int n)
	{
		//右手で武器を握る
		if (n == 0)
		{
			//フラグリセット
			EnemyHitFlag = false;
			WallHitFlag = false;

			//親を右手にする
			BoneList[5].transform.parent = DeepFind(gameObject, "R_HandBone").transform;

			//トランスフォームリセット
			ResetTransform(BoneList[5]);
		}
		//武器を投げる
		else if (n == 1)
		{
			//一旦収納位置に戻す
			MoveWire(100);

			//親を自分の直下する
			BoneList[5].transform.parent = gameObject.transform;
			
			if (LockEnemy != null)
			{
				//武器投げコルーチン呼び出し
				StartCoroutine(MoveWireCoroutine(DeepFind(LockEnemy, "NeckBone").transform.position));
			}
			else
			{
				//武器投げコルーチン呼び出し
				StartCoroutine(MoveWireCoroutine(transform.forward * 100));
			}			
		}
		//武器を巻き戻す
		else if (n == 2)
		{
			//コルーチン呼び出し
			StartCoroutine(ReturnWireCoroutine());
		}
		//蔓燐糞用
		else if (n == 3)
		{
			//親を右腕にする
			BoneList[4].transform.parent = DeepFind(gameObject, "R_HandBone").transform;

			//他はしまう
			BoneList[3].transform.parent = BoneList[2].transform;
			BoneList[2].transform.parent = BoneList[1].transform;
			BoneList[1].transform.parent = BoneList[0].transform;

			//トランスフォームリセット
			ResetTransform(BoneList[4]);
			ResetTransform(BoneList[3]);
			ResetTransform(BoneList[2]);
			ResetTransform(BoneList[1]);
		}
		//収納する
		else if (n == 100)
		{
			for (int count = 5; count >= 1; count--)
			{
				//先端から一つずつ親を設定する
				BoneList[count].transform.parent = BoneList[count - 1].transform;

				//トランスフォームリセット
				ResetTransform(BoneList[count]);
			}
		}
	}

	//特殊攻撃成功処理
	public void SpecialAttackHit()
	{
		//特殊攻撃成功処理呼び出し
		gameObject.GetComponent<PlayerScript>().SpecialAttackHit(LockEnemy);

		//収納位置に戻す
		MoveWire(100);

		//敵の首にアタッチ
		BoneList[5].transform.parent = DeepFind(LockEnemy, "NeckBone").transform;

		//左手にアタッチ
		BoneList[4].transform.parent = DeepFind(gameObject, "L_HandBone").transform;

		//右手にアタッチ
		BoneList[3].transform.parent = DeepFind(gameObject, "R_HandBone").transform;

		//トランスフォームリセット
		ResetTransform(BoneList[5]);
		ResetTransform(BoneList[4]);
		ResetTransform(BoneList[3]);
		ResetTransform(BoneList[2]);
		ResetTransform(BoneList[1]);
		ResetTransform(BoneList[0]);
	}

	//武器巻き戻しコルーチン
	private IEnumerator ReturnWireCoroutine()
	{
		//親を解除
		BoneList[5].transform.parent = null;

		//巻き戻し
		while ((BoneList[4].transform.position - BoneList[5].transform.position).sqrMagnitude > 0.1f)
		{
			BoneList[5].transform.position += (BoneList[4].transform.position - BoneList[5].transform.position).normalized * 30 * Time.deltaTime;

			yield return null;
		}

		//収納位置に戻す
		MoveWire(100);
	}

	//投げコルーチン
	private IEnumerator MoveWireCoroutine(Vector3 pos)
	{
		//コライダ有効化
		ExecuteEvents.Execute<Character2WeaponColInterface>(BoneList[5], null, (reciever, eventData) => reciever.SwitchCol(true));

		//武器移動
		while ((gameObject.transform.position - BoneList[5].transform.position).sqrMagnitude < 150 && !WallHitFlag)
		{
			//1フレーム待機
			yield return null;

			//敵に当たった
			if (EnemyHitFlag)
			{
				//特殊攻撃成功処理実行
				SpecialAttackHit();

				//ループを抜けて処理をスキップ
				goto WeaponEnemyHit;
			}
			else
			{
				BoneList[5].transform.position += (pos - BoneList[4].transform.position).normalized * 30 * Time.deltaTime;
			}			
		}

		//コライダ無効化
		ExecuteEvents.Execute<Character2WeaponColInterface>(BoneList[5], null, (reciever, eventData) => reciever.SwitchCol(false));

		//巻き戻し
		while ((BoneList[0].transform.position - BoneList[5].transform.position).sqrMagnitude > 1)
		{
			BoneList[5].transform.position += (BoneList[0].transform.position - BoneList[5].transform.position).normalized * 50 * Time.deltaTime;

			yield return null;
		}

		//収納位置に戻す
		MoveWire(100);

		//敵に当たった時に抜ける先
		WeaponEnemyHit:;
	}
}

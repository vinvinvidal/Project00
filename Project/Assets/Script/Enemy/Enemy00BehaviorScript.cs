using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

//メッセージシステムでイベントを受け取るためのインターフェイス、敵ビヘイビアは全て共通のインターフェイスにする
public interface EnemyBehaviorInterface : IEventSystemHandler
{
	//ポーズ処理
	void Pause(bool b);

	//プレイヤーキャラクターを更新する
	void SetPlayerCharacter(GameObject c);

	//使用している攻撃を受け取る
	void SetArts(EnemyAttackClass Arts);

	//行動Listを送る
	List<EnemyBehaviorClass> GetBehaviorList();
}

public class Enemy00BehaviorScript : GlobalClass, EnemyBehaviorInterface
{
	//自身のアニメーターコントローラ
	private Animator CurrentAnimator;
	
	//自身のEnemyCharacterScrpt
	private EnemyCharacterScript EnemyScript;

	//自身のEnemySettingScript
	private EnemySettingScript SettingScript;

	//接近距離
	private float ChaseDistance;

	//攻撃距離
	private float AttackDistance;

	//仲間避け距離
	private float AroundDistance;

	//走り接近距離
	private float RunDistance;

	//最小待機時間
	float MinWaitTime = 0.5f;

	//最大待機時間
	float MaxWaitTime = 2;

	//プレイヤーキャラクター
	private GameObject PlayerCharacter;

	//プレイヤーキャラクターとの水平距離
	private float PlayerHorizontalDistance;

	//プレイヤーキャラクターとの垂直距離
	private float PlayerverticalDistance;

	//プレイヤーキャラクターとの角度
	private float PlayerAngle;

	//武器オブジェクト
	GameObject WeaponOBJ = null;

	//使用している攻撃
	EnemyAttackClass UseArts = null;

	//行動List	
	private List<EnemyBehaviorClass> EnemyBehaviorList = new List<EnemyBehaviorClass>();

	//ポーズフラグ
	private bool PauseFlag = false;

	void Start()
	{
		//アニメーターコントローラ取得
		CurrentAnimator = GetComponent<Animator>();

		//自身のEnemyCharacterScript取得
		EnemyScript = gameObject.GetComponent<EnemyCharacterScript>();

		//自身のEnemySettingScript取得
		SettingScript = gameObject.GetComponent<EnemySettingScript>();

		//接近距離
		ChaseDistance = 2;

		//攻撃距離
		AttackDistance = 2.5f;

		//仲間避け距離
		AroundDistance = 3f;

		//走り接近距離
		RunDistance = 5;

		//プレイヤーキャラクターとの水平距離初期化
		PlayerHorizontalDistance = 0f;

		//プレイヤーキャラクターとの垂直距離初期化
		PlayerverticalDistance = 0f;

		//プレイヤーキャラクター、とりあえずnull
		PlayerCharacter = null;

		///---行動追加---///

		//待機
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Wait", 10, () =>
		{
			//待機コルーチン呼び出し
			StartCoroutine(WaitCoroutine());

		}, () =>
		//可能条件
		{
			//いつでもOK
			return true;

		}));

		//追跡
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Chase", 50, () =>
		{
			//追跡コルーチン呼び出し
			StartCoroutine(ChaseCoroutine());

		}, () =>
		//可能条件
		{
			//出力用変数宣言
			bool re = false;

			//プレイヤーキャラクターと離れている
			if (PlayerHorizontalDistance > ChaseDistance)
			{
				re = true;
			}

			//出力
			return re;

		}));

		//仲間避け
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Around", 20, () =>
		{
			//仲間避けコルーチン呼び出し
			StartCoroutine(AroundCoroutine());

		}, () =>
		//可能条件
		{
			//出力用変数宣言
			bool re = false;

			//存在する全ての敵を回す
			foreach (GameObject e in GameManagerScript.Instance.AllActiveEnemyList)
			{
				//敵が存在している
				if (e != null)
				{
					//自身は見ない
					if (GameManagerScript.Instance.AllActiveEnemyList.IndexOf(e) != EnemyScript.ListIndex)
					{
						//近くに敵がいるか判定
						if ((gameObject.transform.position - e.transform.position).sqrMagnitude < Mathf.Pow(AroundDistance, 2))
						{
							//居たらフラグを立ててブレイク
							re = true;

							break;
						}
					}
				}
			}

			//出力
			return re;

		}));

		//攻撃00
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Attack00", 50, () =>
		//攻撃00の処理
		{
			//攻撃00コルーチン呼び出し
			StartCoroutine(Attack00Coroutine());

		}, () =>
		//攻撃00の条件
		{
			//出力用変数宣言
			bool re = false;

			//画面内に入っているかbool取得
			ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => re = reciever.GetOnCameraBool());

			//画面内に入っていたら処理
			if (re)
			{
				//高低差が無く射程距離内で攻撃中じゃなくてスケベ中じゃない
				if (Mathf.Abs(PlayerverticalDistance) < 0.25f && PlayerHorizontalDistance > 0 && PlayerHorizontalDistance < AttackDistance && !GameManagerScript.Instance.H_Flag)
				{
					re = true;
				}
				else
				{
					re = false;
				}
			}

			//出力
			return re;

		}));

		//攻撃01
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Attack01", 500, () =>
		//攻撃01の処理
		{
			//攻撃01コルーチン呼び出し
			StartCoroutine(Attack01Coroutine());

		}, () =>
		//攻撃01の条件
		{
			//出力用変数宣言
			bool re = false;

			//画面内に入っているかbool取得
			ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => re = reciever.GetOnCameraBool());

			//画面内に入っていたら処理
			if (re)
			{
				//距離が離れていてスケベ中じゃない
				if (PlayerHorizontalDistance > 5f && !GameManagerScript.Instance.H_Flag)
				{
					re = true;
				}
				else
				{
					re = false;
				}
			}

			//出力
			return re;

		}));

		//スケベ攻撃
		EnemyBehaviorList.Add(new EnemyBehaviorClass("H_Attack", 500, () =>
		//スケベ攻撃の処理
		{
			//スケベ攻撃コルーチン呼び出し
			StartCoroutine(H_AttackCoroutine());

		}, () =>
		//スケベ攻撃の条件
		{
			//出力用変数宣言
			bool re = false;

			//性的表現スイッチ
			if (GameManagerScript.Instance.SexualSwicth)
			{
				//画面内に入っているかbool取得
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => re = reciever.GetOnCameraBool());

				//画面内に入っていたら処理
				if (re)
				{
					//高低差が無く射程距離でスケベ中じゃない
					if (Mathf.Abs(PlayerverticalDistance) < 0.25f && PlayerHorizontalDistance < AttackDistance && !GameManagerScript.Instance.H_Flag)
					{
						re = true;
					}
					else
					{
						re = false;
					}
				}
			}

			//出力
			return re;
		}));

		//拉致
		EnemyBehaviorList.Add(new EnemyBehaviorClass("Abduction", 100000, () =>
		//拉致の処理
		{
			//拉致コルーチン呼び出し
			StartCoroutine(AbductionCoroutine());

		}, () =>
		//拉致の条件
		{
			//出力用変数宣言
			bool re = false;

			//ダウンしているキャラクターがいる
			if (GameManagerScript.Instance.DownCharacterList.Count != 0)
			{	
				//他に拉致している敵がいない
				if (GameManagerScript.Instance.AllActiveEnemyList.Where(a => a != null).Where(a => a.GetComponent<EnemyCharacterScript>().Abduction_Flag).ToList().Count == 0)
				{
					//自分が一番近い
					re = true;

						/*
						GameManagerScript.Instance.AllActiveEnemyList
						//nullチェック
						.Where(e => e != null)
						//ダメージを受けてない
						.Where(e => !e.GetComponent<EnemyCharacterScript>().DamageFlag)
						//距離でソート
						.OrderBy(e => (e.transform.position - GameManagerScript.Instance.DownCharacterList[Random.Range(0, GameManagerScript.Instance.DownCharacterList.Count - 1)].transform.position).sqrMagnitude)
						//一番近いのが自分か
						.ToList()[0] == gameObject;
						*/
				}				
			}		

			//出力
			return re;
		}));
	}

	void Update()
    {
		if(EnemyScript.BattleFlag)
		{
			//常にプレイヤーキャラクターとの水平距離を測定する
			PlayerHorizontalDistance = HorizontalVector(PlayerCharacter, gameObject).magnitude;

			//常にプレイヤーキャラクターとの高低差を測定する
			PlayerverticalDistance = Mathf.Abs(PlayerCharacter.transform.position.y - gameObject.transform.position.y);

			//常にプレイヤーキャラクターとの角度を測定する、高低差は無視
			PlayerAngle = Vector3.Angle(gameObject.transform.forward, HorizontalVector(PlayerCharacter, gameObject));
		}
	}

	//待機コルーチン
	IEnumerator WaitCoroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//待機時間設定
		float WaitTime = UnityEngine.Random.Range(MinWaitTime, MaxWaitTime);

		//プレイヤーキャラクターがいる方向
		Vector3 PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

		//待機時間が終わるまでループ
		while (WaitTime > 0 && EnemyScript.BehaviorFlag)
		{
			//待機時間を減らす
			if (!PauseFlag)
			{
				WaitTime -= Time.deltaTime;
			}

			//プレイヤーが正面に以内
			if (PlayerAngle > 45)
			{
				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Walk", true);

				//プレイヤーが正面にくるまでループ
				while (PlayerAngle > 5)
				{
					//プレイヤーキャラクターがいる方向測定
					PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

					//プレイヤーキャラクターに向ける
					EnemyScript.BehaviorRotate = PlayerVec;

					//行動不能になったらブレイク
					if (!EnemyScript.BehaviorFlag)
					{
						break;
					}

					//1フレーム待機
					yield return null;
				}

				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				//回転値初期化
				EnemyScript.BehaviorRotate *= 0;
			}
			else
			{
				//回転値初期化
				EnemyScript.BehaviorRotate *= 0;
			}

			//1フレーム待機
			yield return null;
		}

		//回転値初期化
		EnemyScript.BehaviorRotate *= 0;

		//フラグを下す
		EnemyScript.BehaviorFlag = false;
	}

	//追跡コルーチン
	IEnumerator ChaseCoroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//プレイヤーキャラクターに接近するまでループ
		while (PlayerHorizontalDistance > ChaseDistance)
		{
			//戦闘中フラグが降りたらブレーク
			if(!EnemyScript.BattleFlag)
			{
				break;
			}

			//プレイヤーとの距離によって走りと歩き切り替える
			if (PlayerHorizontalDistance > RunDistance)
			{
				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Run", true);

				//アニメーターフラグを下す
				CurrentAnimator.SetBool("Walk", false);
			}
			else
			{
				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Walk", true);

				//アニメーターフラグを下す
				CurrentAnimator.SetBool("Run", false);
			}

			//プレイヤーキャラクターに向かう移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(PlayerCharacter, gameObject).normalized;

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = HorizontalVector(PlayerCharacter, gameObject);

			//存在する全ての敵を回す
			foreach (GameObject e in GameManagerScript.Instance.AllActiveEnemyList)
			{
				//敵が存在している
				if (e != null)
				{
					//自身は見ない
					if (GameManagerScript.Instance.AllActiveEnemyList.IndexOf(e) != EnemyScript.ListIndex)
					{
						//近くに敵がいるか判定
						if ((gameObject.transform.position - e.transform.position).sqrMagnitude < Mathf.Pow(AroundDistance * 0.5f, 2))
						{
							//居たら仲間避けコルーチン呼び出してブレイク
							StartCoroutine(AroundCoroutine());

							goto ChaseBreak;
						}
					}
				}
			}

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//待機コルーチン呼び出し
		StartCoroutine(WaitCoroutine());

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Run", false);

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;

		//ループを抜ける先、余計な処理を避ける
		ChaseBreak:;
	}

	//仲間避けコルーチン
	IEnumerator AroundCoroutine()
	{
		//移動加速度をリセット
		EnemyScript.BehaviorMoveVec = Vector3.zero;

		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Run", false);

		//一番近い敵代入変数
		GameObject NearEnemy = null;

		//敵との距離、とりあえず最低値を入れておく
		float NearEnemyDistance = Mathf.Pow(AroundDistance, 2);

		//存在する全ての敵を回す
		foreach (GameObject e in GameManagerScript.Instance.AllActiveEnemyList)
		{
			//敵が存在している
			if (e != null)
			{
				//自身は見ない
				if (GameManagerScript.Instance.AllActiveEnemyList.IndexOf(e) != EnemyScript.ListIndex)
				{
					//一番近い敵を探す
					if ((gameObject.transform.position - e.transform.position).sqrMagnitude < NearEnemyDistance)
					{
						//敵との距離更新
						NearEnemyDistance = (gameObject.transform.position - e.transform.position).sqrMagnitude;

						//一番近い敵更新
						NearEnemy = e;
					}
				}
			}
		}

		//近くに仲間がいたら避ける
		while (NearEnemy != null)
		{
			//敵との距離更新
			NearEnemyDistance = (gameObject.transform.position - NearEnemy.transform.position).sqrMagnitude;

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = HorizontalVector(PlayerCharacter, gameObject);

			//近い敵を避ける移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(gameObject, NearEnemy).normalized;

			//最低値より離れたらブレイク
			if (!EnemyScript.BehaviorFlag || !EnemyScript.BattleFlag || NearEnemyDistance > Mathf.Pow(AroundDistance, 2) )
			{
				break;
			}

			//１フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Walk", false);

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;

		//待機コルーチン呼び出し
		StartCoroutine(WaitCoroutine());
	}

	//攻撃00コルーチン
	IEnumerator Attack00Coroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//プレイヤーキャラクターがいる方向測定
		Vector3 PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//プレイヤーが正面にくるまでループ
		while (PlayerAngle > 0.1f)
		{
			//プレイヤーキャラクターがいる方向測定
			PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = PlayerVec;

			//行動不能、もしくは射程外になったらブレイク
			if (!EnemyScript.BehaviorFlag || PlayerHorizontalDistance > AttackDistance)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				goto Attack00Break;
			}

			//1フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Attack", true);

		//攻撃ステート再生速度リセット
		CurrentAnimator.SetFloat("AttackSpeed", 1.0f);

		//モーション名を敵スクリプトに送る
		ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetAttackMotion("00"));

		//ステートを反映させる為に１フレーム待つ
		yield return null;

		//フラグが降りるまで待機
		while (EnemyScript.CurrentState.Contains("Attack"))
		{
			//待機
			yield return null;
		}

		//ループを抜ける先、余計な処理を避ける
		Attack00Break:;

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;
	}

	//攻撃01コルーチン
	IEnumerator Attack01Coroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//バトルフィールドオブジェクト宣言
		GameObject BattleFieldOBJ = null;

		//バトルフィールドオブジェクト取得
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => BattleFieldOBJ = reciever.GetBattleFieldOBJ());

		//壁オブジェクトを全て取得
		List<GameObject> WallOBJList = new List<GameObject>(DeepFind(BattleFieldOBJ, "WallOBJ").GetComponentsInChildren<Transform>().Where(a => a.gameObject.layer == LayerMask.NameToLayer("PhysicOBJ")).Select(b => b.gameObject).ToList());

		//最も近い壁オブジェクト宣言
		GameObject WallOBJ = null;

		//武器オブジェクト初期化
		WeaponOBJ = null;

		//壁オブジェクトとの距離宣言
		float OBJDistance = 10000;

		//壁オブジェクトを回して最も近い壁オブジェクトを調べる
		foreach (GameObject i in WallOBJList)
		{
			//高さが違う奴は無視
			if(i.transform.position.y - gameObject.transform.position.y > 0.1 && i.transform.position.y - gameObject.transform.position.y < 2)
			{
				//距離測定
				if (OBJDistance > Vector3.SqrMagnitude(i.transform.position - gameObject.transform.position))
				{
					//壁オブジェクトとの距離更新
					OBJDistance = Vector3.SqrMagnitude(i.transform.position - gameObject.transform.position);

					//最も近い壁オブジェクト更新
					WallOBJ = i;
				}
			}
		}

		//壁オブジェクトがある方向測定
		Vector3 WallVec = HorizontalVector(WallOBJ, gameObject);

		//壁オブジェクトとの角度
		float WallAngle = 180;

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//壁オブジェクトが正面にくるまでループ
		while (WallAngle > 0.1f)
		{
			//壁オブジェクトがある方向測定
			WallVec = HorizontalVector(WallOBJ, gameObject);

			//角度測定
			WallAngle = Vector3.Angle(gameObject.transform.forward, WallVec);

			//壁オブジェクトに向ける
			EnemyScript.BehaviorRotate = WallVec;

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				goto Attack01Break;
			}

			//1フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Run", true);

		//ステートを反映させる為に１フレーム待つ
		yield return null;

		//壁に接近するまでループ
		while (OBJDistance > 5)
		{
			//壁オブジェクトに向かう移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(WallOBJ, gameObject).normalized;

			//壁オブジェクトとの距離測定
			OBJDistance = Vector3.SqrMagnitude(WallOBJ.transform.position - gameObject.transform.position);

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Run", false);

				goto Attack01Break;
			}

			//1フレーム待機
			yield return null;
		}

		//壁オブジェクトに向かう移動ベクトルリセット
		EnemyScript.BehaviorMoveVec *= 0;

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Run", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Attack", true);

		//攻撃ステート再生速度リセット
		CurrentAnimator.SetFloat("AttackSpeed", 1.0f);

		//モーション名を敵スクリプトに送る
		ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetAttackMotion("01"));

		//モーションの再生時間宣言
		float MotionTime = 0;

		//モーションがある程度再生されるまで待つ、決め打ちなのでよろしくない
		while(MotionTime < 0.333f)
		{
			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Attack", false);

				goto Attack01Break;
			}

			//モーションの再生時間カウントアップ
			MotionTime += Time.deltaTime;

			//１フレーム待機
			yield return null;
		}

		//壁オブジェクトのインスタンスを作って武器化
		WeaponOBJ = Instantiate(WallOBJ);

		//持たせる
		WeaponOBJ.transform.parent = DeepFind(gameObject, "R_HandBone").transform;

		//レイヤーを変えてアウトラインをつける
		WeaponOBJ.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Enemy");

		//机
		if (WeaponOBJ.name.Contains("Desk"))
		{
			//壁オブジェクトトランスフォーム設定
			WeaponOBJ.transform.localPosition = new Vector3(-0.15f, 0.75f, -0.25f);
			WeaponOBJ.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, -90));
		}
		//椅子
		else
		{
			//壁オブジェクトトランスフォーム設定
			WeaponOBJ.transform.localPosition = new Vector3(-0.5f, 0.4f, -0.25f);
			WeaponOBJ.transform.localRotation = Quaternion.Euler(new Vector3(-180, -90, 90));
		}

		//プレイヤーキャラクターがいる方向測定
		Vector3 PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

		//プレイヤーが正面にくるまでループ
		while (PlayerAngle > 0.1f)
		{
			//プレイヤーキャラクターがいる方向測定
			PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = PlayerVec;

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Attack", false);

				goto Attack01Break;
			}

			//1フレーム待機
			yield return null;
		}

		//プレイヤーが遠い、もしくは画面内にいない場合移動させる
		while (PlayerHorizontalDistance > 5 || !EnemyScript.GetOnCameraBool())
		{
			//プレイヤーキャラクターに向かう移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(PlayerCharacter, gameObject).normalized * EnemyScript.MoveSpeed;

			//サインカーブで歩行アニメーションと移動値を合わせる
			EnemyScript.BehaviorMoveVec *= Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.75f * EnemyScript.SinCount));

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = HorizontalVector(PlayerCharacter, gameObject);

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Attack", false);

				goto Attack01Break;
			}

			//1フレーム待機
			yield return null;
		}

		//攻撃モーション再生
		EnemyScript.JampMotionFrame(110);

		//フラグが降りるまで待機
		while (EnemyScript.CurrentState.Contains("Attack"))
		{
			//待機
			yield return null;

			//もし歩きループでハマってたら再度攻撃モーション再生
			if(CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f && CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
			{
				//攻撃モーション再生
				EnemyScript.JampMotionFrame(110);
			}	
		}

		//ループを抜ける先、余計な処理を避ける
		Attack01Break:;

		//武器オブジェクトをまだ持ってたら消す
		if (WeaponOBJ != null)
		{
			if(WeaponOBJ.transform.root.gameObject == gameObject)
			{
				//親を解除
				WeaponOBJ.transform.parent = null;

				//オブジェクトに消失用スクリプト追加
				WeaponOBJ.AddComponent<WallVanishScript>();

				//物理挙動有効化
				WeaponOBJ.GetComponent<Rigidbody>().isKinematic = false;

				//外側に力を与えて飛ばす
				WeaponOBJ.GetComponent<Rigidbody>().AddForce((WeaponOBJ.transform.position - gameObject.transform.position + Vector3.up) * 2.5f, ForceMode.Impulse);
			}
		}

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;
	}

	//スケベ攻撃コルーチン
	IEnumerator H_AttackCoroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//プレイヤーキャラクターがいる方向測定
		Vector3 PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//プレイヤーが正面にくるまでループ
		while (PlayerAngle > 0.1f)
		{
			//プレイヤーキャラクターがいる方向測定
			PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

			//プレイヤーキャラクターに向ける
			EnemyScript.BehaviorRotate = PlayerVec;

			//行動不能、もしくは射程外になったらブレイク
			if (!EnemyScript.BehaviorFlag || PlayerHorizontalDistance > AttackDistance)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				goto H_AttackBreak;
			}
			
			//1フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("H_Try", true);

		//攻撃ステート再生速度リセット
		CurrentAnimator.SetFloat("AttackSpeed", 1.0f);

		//ステートを反映させる為に１フレーム待つ
		yield return null;

		//フラグが降りるまで待機
		while (EnemyScript.CurrentState.Contains("H_"))
		{
			//待機
			yield return null;
		}

		//ループを抜ける先、余計な処理を避ける
		H_AttackBreak:;

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;
	}

	//拉致コルーチン
	IEnumerator AbductionCoroutine()
	{
		//拉致フラグを立てる
		EnemyScript.Abduction_Flag = true;

		//行動中フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//拉致るキャラクター
		GameObject TargetCharacter = null;

		//拉致るキャラクターとの距離
		float TargetDistance = 1000;

		//拉致るキャラクターを取得、一番近いやつ。なんかLinqでやるとエラーになるのでこれでやる
		foreach (var i in GameManagerScript.Instance.DownCharacterList)
		{
			if(i != null)
			{
				if(TargetDistance > HorizontalVector(i, gameObject).sqrMagnitude)
				{
					TargetDistance = HorizontalVector(i, gameObject).sqrMagnitude;

					TargetCharacter = i;
				}				
			}
		}

		//拉致する場所
		Vector3 AbductionPos = TargetCharacter.transform.position + (TargetCharacter.transform.forward * 0.25f);

		//キャラクターとの角度
		float CharacterAngle = 180;

		//キャラクターとの距離
		float CharacterDistance = 10000;

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//キャラクターが正面にくるまでループ
		while (CharacterAngle > 0.1f)
		{
			//キャラクターとの角度測定
			CharacterAngle = Vector3.Angle(gameObject.transform.forward, HorizontalVector(AbductionPos, gameObject));

			//キャラクターに向ける
			EnemyScript.BehaviorRotate = HorizontalVector(AbductionPos, gameObject);

			//行動不能になるかキャラクターが起き上がったらレイク
			if (!EnemyScript.BehaviorFlag || !GameManagerScript.Instance.DownCharacterList.Any(a => a == TargetCharacter))
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				goto AbductionBreak;
			}

			//1フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下す
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Run", true);

		//ステートを反映させる為に１フレーム待つ
		yield return null;

		//キャラクターに接近するまでループ
		while (CharacterDistance > 0.01f)
		{
			//移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(AbductionPos, gameObject).normalized * Mathf.Min(CharacterDistance * 10, 1);

			//キャラクターとの距離測定
			CharacterDistance = Vector3.Distance(AbductionPos, gameObject.transform.position);

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag || !GameManagerScript.Instance.DownCharacterList.Any(a => a == TargetCharacter))
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Run", false);

				goto AbductionBreak;
			}

			//1フレーム待機
			yield return null;
		}

		//アニメーターのフラグを下す
		CurrentAnimator.SetBool("Run", false);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//角度初期化
		CharacterAngle = 180;

		//キャラクターが正面にくるまでループ
		while (CharacterAngle > 0.1f)
		{
			//キャラクターとの角度測定
			CharacterAngle = Vector3.Angle(gameObject.transform.forward, -TargetCharacter.transform.forward);

			//キャラクターに向ける
			EnemyScript.BehaviorRotate = -TargetCharacter.transform.forward;

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag || !GameManagerScript.Instance.DownCharacterList.Any(a => a == TargetCharacter))
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Walk", false);

				goto AbductionBreak;
			}

			//1フレーム待機
			yield return null;
		}

		//復活までの時間が十分にあるかチェック
		if(TargetCharacter.GetComponent<PlayerScript>().RevivalTime > 0.5f)
		{
			//アニメーターのフラグを下す
			CurrentAnimator.SetBool("Walk", false);

			//アニメーターフラグを立てる
			CurrentAnimator.SetBool("Abduction", true);
		}
		else
		{
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("Abduction", false);

			goto AbductionBreak;
		}

		//拉致成功までループ
		while(!EnemyScript.AbductionSuccess_Flag)
		{
			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				//アニメーターのフラグを下ろす
				CurrentAnimator.SetBool("Abduction", false);

				goto AbductionBreak;
			}

			//1フレーム待機
			yield return null;
		}

		//プレイヤーキャラクターの拉致られ処理呼び出し
		TargetCharacter.GetComponent<PlayerScript>().PlayerAbduction(gameObject);

		//ゲームマネージャーのListから自身を削除
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.RemoveAllActiveEnemyList(EnemyScript.ListIndex));

		//オブジェクト消失処理呼び出し
		EnemyScript.AbductionVanis(1, 0.75f);

		//キャラクター消失処理呼び出し
		TargetCharacter.GetComponent<PlayerScript>().ChangeVanish("1,0.75");

		//ループを抜ける先
		AbductionBreak:;

		//アニメーターのフラグを下す
		CurrentAnimator.SetBool("Walk", false);

		//アニメーターのフラグを下す
		CurrentAnimator.SetBool("Run", false);

		//アニメーターのフラグを下ろす
		CurrentAnimator.SetBool("Abduction", false);

		//行動中フラグを下ろす
		EnemyScript.BehaviorFlag = false;

		//拉致フラグを下す
		EnemyScript.Abduction_Flag = false;

		//待機
		yield return null;	
	}

	//プレイヤーキャラクターを更新する
	public void SetPlayerCharacter(GameObject c)
	{
		PlayerCharacter = c;
	}

	//使用している攻撃を受け取る
	public void SetArts(EnemyAttackClass Arts)
	{
		UseArts = Arts;
	}

	//持っている武器を投げる
	public void WeaponThrow(float s)
	{
		//親を解除
		WeaponOBJ.transform.parent = null;

		//武器用コライダトリガー化
		WeaponOBJ.GetComponent<MeshCollider>().isTrigger = true;

		//レイヤーを武器化
		WeaponOBJ.layer = LayerMask.NameToLayer("EnemyWeaponCol");

		//武器化スクリプトを有効化
		WeaponOBJ.GetComponent<ThrowWeaponScript>().enabled = true;

		//武器に技情報を渡す
		WeaponOBJ.GetComponent<ThrowWeaponScript>().UseArts = UseArts;

		//武器に自身を渡す
		WeaponOBJ.GetComponent<ThrowWeaponScript>().Enemy = gameObject;

		//物理挙動有効化
		WeaponOBJ.GetComponent<Rigidbody>().isKinematic = false;

		//武器の回転制限を解除
		WeaponOBJ.GetComponent<Rigidbody>().angularDrag = 0;

		//トルクを与えて回転
		WeaponOBJ.GetComponent<Rigidbody>().AddTorque(gameObject.transform.right * 10 , ForceMode.Impulse);

		//プレイヤーに向かって飛ばす
		WeaponOBJ.GetComponent<Rigidbody>().AddForce(((PlayerCharacter.transform.position + Vector3.up) - WeaponOBJ.transform.position).normalized * s, ForceMode.Impulse);

		//ゲームマネージャーに飛び道具を送る
		GameManagerScript.Instance.AllEnemyWeaponList.Add(WeaponOBJ);
	}

	//ポーズ処理
	public void Pause(bool b)
	{
		//ポーズフラグ引数で受け取ったboolをフラグに代入
		PauseFlag = b;
	}

	//行動リストを返す
	public List<EnemyBehaviorClass> GetBehaviorList()
	{
		return EnemyBehaviorList;
	}
}

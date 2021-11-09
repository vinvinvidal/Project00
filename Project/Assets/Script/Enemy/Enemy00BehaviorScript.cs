using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス、敵ビヘイビアは全て共通のインターフェイスにする
public interface EnemyBehaviorInterface : IEventSystemHandler
{
	//ポーズ処理
	void Pause(bool b);
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

	//最小待機時間
	float MinWaitTime = 0.5f;

	//最大待機時間
	float MaxWaitTime = 2;

	//プレイヤーキャラクター
	private GameObject PlayerCharacter;

	//プレイヤーキャラクターとの距離
	private float PlayerDistance;

	//プレイヤーキャラクターとの角度
	private float PlayerAngle;

	//行動List	
	private List<EnemyBehaviorClass> EnemyBehaviorList = new List<EnemyBehaviorClass>();

	//準備完了フラグ
	private bool AllReadyFlag = false;

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

		//プレイヤーキャラクターとの距離初期化
		PlayerDistance = 0f;

		//プレイヤーキャラクター、とりあえずnull
		PlayerCharacter = null;

		//プレイヤーキャラクターをコルーチンで確実に取得する
		StartCoroutine(SetPlayerCharacterCoroutine());

		//準備完了待ちコルーチン呼び出し
		StartCoroutine(AllReadyFlagCoroutine());

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
			if (PlayerDistance > ChaseDistance)
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
				//射程距離で攻撃中じゃなくてスケベ中じゃない
				if (PlayerDistance < AttackDistance && !GameManagerScript.Instance.H_Flag)
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
		EnemyBehaviorList.Add(new EnemyBehaviorClass("H_Attack", 50000, () =>
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
					//射程距離でスケベ中じゃない
					if (PlayerDistance < AttackDistance && !GameManagerScript.Instance.H_Flag)
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

		//スクリプトに全ての行動Listを送る
		ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetBehaviorList(EnemyBehaviorList));
		
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
					EnemyScript.RotateVec = PlayerVec;

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
				EnemyScript.RotateVec *= 0;
			}
			else
			{
				//回転値初期化
				EnemyScript.RotateVec *= 0;
			}

			//1フレーム待機
			yield return null;
		}

		//回転値初期化
		EnemyScript.RotateVec *= 0;

		//フラグを下す
		EnemyScript.BehaviorFlag = false;
	}

	//追跡コルーチン
	IEnumerator ChaseCoroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

		//プレイヤーキャラクターに接近するまでループ
		while (PlayerDistance > ChaseDistance)
		{
			//プレイヤーキャラクターに向かう移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(PlayerCharacter, gameObject).normalized;

			//プレイヤーキャラクターに向ける
			EnemyScript.RotateVec = HorizontalVector(PlayerCharacter, gameObject);

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

		//フラグを下ろす
		EnemyScript.BehaviorFlag = false;

		//ループを抜ける先、余計な処理を避ける
		ChaseBreak:;
	}

	//仲間避けコルーチン
	IEnumerator AroundCoroutine()
	{
		//フラグを立てる
		EnemyScript.BehaviorFlag = true;

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Walk", true);

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
			EnemyScript.RotateVec = HorizontalVector(PlayerCharacter, gameObject);

			//近い敵を避ける移動ベクトル算出
			EnemyScript.BehaviorMoveVec = HorizontalVector(gameObject, NearEnemy).normalized;

			//最低値より離れたらブレイク
			if (!EnemyScript.BehaviorFlag || NearEnemyDistance > Mathf.Pow(AroundDistance, 2))
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
			EnemyScript.RotateVec = PlayerVec;

			//行動不能、もしくは射程外になったらブレイク
			if (!EnemyScript.BehaviorFlag || PlayerDistance > AttackDistance)
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

		//待機する
		StartCoroutine(WaitCoroutine());
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
			EnemyScript.RotateVec = PlayerVec;

			//行動不能、もしくは射程外になったらブレイク
			if (!EnemyScript.BehaviorFlag || PlayerDistance > AttackDistance)
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

		//待機する
		StartCoroutine(WaitCoroutine());
	}

	void Update()
    {
		//常にプレイヤーキャラクターとの距離を測定する、高低差は無視
		PlayerDistance = HorizontalVector(PlayerCharacter, gameObject).magnitude;

		//常にプレイヤーキャラクターとの角度を測定する、高低差は無視
		PlayerAngle = Vector3.Angle(gameObject.transform.forward, HorizontalVector(PlayerCharacter, gameObject));
	}

	//プレイヤーキャラクター取得コルーチン
	IEnumerator SetPlayerCharacterCoroutine()
	{
		//プレイヤーキャラクターを取得するまでループ
		while (PlayerCharacter == null)
		{
			//プレイヤーキャラクター取得
			PlayerCharacter = EnemyScript.PlayerCharacter;

			//1フレーム待機
			yield return null;
		}
	}

	//準備完了待ちコルーチン
	IEnumerator AllReadyFlagCoroutine()
	{
		//準備完了するまでループ
		while (!AllReadyFlag)
		{
			//セッティングスクリプトからフラグを参照
			AllReadyFlag = SettingScript.AllReadyFlag;

			//1フレーム待機
			yield return null;
		}
	}

	//ポーズ処理
	public void Pause(bool b)
	{
		//ポーズフラグ引数で受け取ったboolをフラグに代入
		PauseFlag = b;
	}
}

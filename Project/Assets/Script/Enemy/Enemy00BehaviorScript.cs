using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy00BehaviorScript : GlobalClass
{
	//行動構造体
	private struct BehaviorStruct
	{
		//この行動の名前
		public string Name;

		//この行動の優先度
		public int Priority;

		//この行動の実行条件
		public Func<bool> BehaviorConditions;

		//この行動の処理
		public Action BehaviorAction;

		//コンストラクタ
		public BehaviorStruct(string n, int p, Action BA, Func<bool> BC)
		{
			Name = n;
			Priority = p;
			BehaviorConditions = BC;
			BehaviorAction = BA;
		}
	}

	//キャラクターコントローラ
	private CharacterController CharaController;

	//自身のアニメーターコントローラ
	private Animator CurrentAnimator;

	//自身のEnemyCharacterScrpt
	private EnemyCharacterScript EnemyScript;

	//自身のEnemySettingScript
	private EnemySettingScript SettingScript;

	//移動速度
	private float MoveSpeed;

	//行動移動ベクトル
	private Vector3 BehaviorMoveVec;

	//仲間避け移動ベクトル
	private Vector3 AroundMoveVec;

	//モーション依存の移動ベクトル
	private Vector3 MotionMoveVec;

	//最終的な移動ベクトル
	private Vector3 MoveVec;

	//体を向ける方向
	private Vector3 RotateVec;

	//旋回速度
	private float TurnSpeed;

	//接近距離
	private float ChaseDistance;

	//仲間避け距離
	private float AroundDistance;

	//最小待機時間
	float MinWaitTime = 1;

	//最大待機時間
	float MaxWaitTime = 5;

	//歩調に合わせるサインカーブ生成に使う数
	float SinCount = 0;

	//プレイヤーキャラクター
	private GameObject PlayerCharacter;

	//プレイヤーキャラクターとの距離
	private float PlayerDistance;

	//プレイヤーキャラクターとの角度
	private float PlayerAngle;

	//行動構造体List
	private List<BehaviorStruct> BehaviorList = new List<BehaviorStruct>();

	//現在行っている行動フラグを持つDic
	private Dictionary<string, bool> NowBehaviorDic = new Dictionary<string, bool>();

	//準備完了フラグ
	private bool AllReadyFlag = false;

	void Start()
    {
		//キャラクターコントローラ取得
		CharaController = gameObject.GetComponent<CharacterController>();

		//アニメーターコントローラ取得
		CurrentAnimator = GetComponent<Animator>();

		//自身のEnemyCharacterScript取得
		EnemyScript = gameObject.GetComponent<EnemyCharacterScript>();

		//自身のEnemySettingScript取得
		SettingScript = gameObject.GetComponent<EnemySettingScript>();

		//プレイヤーキャラクター、とりあえずnull
		PlayerCharacter = null;

		//プレイヤーキャラクターをコルーチンで確実に取得する
		StartCoroutine(SetPlayerCharacterCoroutine());

		//移動速度取得
		MoveSpeed = EnemyScript.MoveSpeed;

		//行動移動ベクトル初期化
		BehaviorMoveVec = Vector3.zero;

		//最終的な移動ベクトル初期化
		MoveVec = Vector3.zero;

		//旋回速度取得
		TurnSpeed = EnemyScript.TurnSpeed;

		//接近距離
		ChaseDistance = 2;

		//仲間避け距離
		AroundDistance = 3f;

		//モーション依存の移動値初期化
		MotionMoveVec = Vector3.zero;

		//プレイヤーキャラクターとの距離初期化
		PlayerDistance = 0f;

		//準備完了待ちコルーチン呼び出し
		StartCoroutine(AllReadyFlagCoroutine());

		//待機
		BehaviorList.Add(new BehaviorStruct("Wait", 10, () =>
		{
			//待機コルーチン呼び出し
			StartCoroutine(WaitCoroutine());

		},()=>
		//待機の条件
		{
			//いつでもOK
			return true;

		} ));

		//追跡
		BehaviorList.Add(new BehaviorStruct("Chase", 50, () =>
		//追跡の処理
		{
			//追跡コルーチン呼び出し
			StartCoroutine(ChaseCoroutine());

		}, () =>
		//追跡の条件
		{
			//出力用変数宣言
			bool re = false;

			//プレイヤーキャラクターと離れている
			if(PlayerDistance > ChaseDistance)
			{
				re = true;
			}
			
			//出力
			return re;

		}));

		//仲間避け
		BehaviorList.Add(new BehaviorStruct("Around", 20, () =>
		//仲間避けの処理
		{
			//仲間避けコルーチン呼び出し
			StartCoroutine(AroundCoroutine());

		}, () =>
		//仲間避けの条件
		{
			//出力用変数宣言
			bool re = false;

			//攻撃中じゃない
			if(!NowBehaviorDic["Attack00"])
			{
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
			}

			//出力
			return re;

		}));

		//攻撃00
		BehaviorList.Add(new BehaviorStruct("Attack00", 20, () =>
		//攻撃00の処理
		{
			//攻撃00コルーチン呼び出し
			StartCoroutine(Attack00Coroutine());

		}, () =>
		//攻撃00の条件
		{
			//出力用変数宣言
			bool re = false;

			//射程距離で攻撃中じゃなくて大体正面にいる
			if (PlayerDistance < ChaseDistance + 1 && PlayerAngle < 90 && !NowBehaviorDic["Attack00"])
			{
				re = true;
			}

			//出力
			return re;

		}));

		//行動Listを回してNowBehaviorDic作成
		foreach(BehaviorStruct i in BehaviorList)
		{
			NowBehaviorDic.Add(i.Name , false);
		}
	}

	void Update()
    {
		//常にプレイヤーキャラクターとの距離を測定する、高低差は無視
		PlayerDistance = HorizontalVector(PlayerCharacter, gameObject).magnitude;

		//常にプレイヤーキャラクターとの角度を測定する、高低差は無視
		PlayerAngle = Vector3.Angle(gameObject.transform.forward, HorizontalVector(PlayerCharacter, gameObject));

		//歩調に合わせるサインカーブ生成に使う数カウントアップ
		SinCount += Time.deltaTime;

		//行動可能条件
		if (AllReadyFlag && EnemyScript.BehaviorFlag)
		{
			//各移動ベクトルを合成
			MoveVec = (((BehaviorMoveVec.normalized + AroundMoveVec.normalized * 0.25f).normalized * MoveSpeed) + MotionMoveVec);

			//移動値の有無でアニメーションを切り替える、攻撃中は無視
			if ((MoveVec != Vector3.zero || RotateVec != Vector3.zero) && !NowBehaviorDic["Attack00"])
			{
				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Walk", true);

				//移動方向と体の向きを比較して歩きモーションをブレンド
				if(MoveVec != Vector3.zero)
				{	
					CurrentAnimator.SetFloat("Side_Walk", Mathf.Lerp(CurrentAnimator.GetFloat("Side_Walk"), (Vector3.Angle(transform.right, MoveVec.normalized) - 90) / 90, 0.25f));			
				}
			}
			else
			{
				//アニメーターのフラグを下す
				CurrentAnimator.SetBool("Walk", false);
			}

			//移動
			CharaController.Move(MoveVec * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.75f * SinCount)) * Time.deltaTime);

			//回転値が入っていたら回転
			if(RotateVec != Vector3.zero)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(RotateVec), TurnSpeed * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.75f * SinCount) + 0.1f) * Time.deltaTime);
			}

			//行動抽選
			if (NowBehaviorDic.All(i => !i.Value))
			{
				//開始可能行動List
				List<BehaviorStruct> TempBehavioerList = new List<BehaviorStruct>(BehaviorList.Where(b => b.BehaviorConditions()).ToList());

				//行動比率
				int BehaviorRatio = 0;

				//抽選番号
				int BehaviorLottery = 0;

				//開始可能な行動がある
				if(TempBehavioerList.Count > 0)
				{ 
					//行動比率合計
					foreach (BehaviorStruct i in TempBehavioerList)
					{
						//発生比率を加算
						BehaviorRatio += i.Priority;
					}

					//抽選番号設定
					BehaviorLottery = UnityEngine.Random.Range(1 , BehaviorRatio + 1);

					//行動比率初期化
					BehaviorRatio = 0;

					//行動判定
					foreach (BehaviorStruct i in TempBehavioerList)
					{
						//比率を合計していく
						BehaviorRatio += i.Priority;

						//乱数がどの範囲にあるか判定
						if (BehaviorRatio >= BehaviorLottery)
						{
							//処理実行
							i.BehaviorAction();

							//ループをブレイク
							break;
						}
					}
				}
			}
		}
	}

	//待機コルーチン
	IEnumerator WaitCoroutine()
	{
		//フラグを立てる
		NowBehaviorDic["Wait"] = true;

		//待機時間設定
		float WaitTime = UnityEngine.Random.Range(MinWaitTime , MaxWaitTime);

		//プレイヤーキャラクターがいる方向
		Vector3 PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

		//待機時間が終わるまでループ
		while (WaitTime > 0)
		{
			//待機時間を減らす
			WaitTime -= Time.deltaTime;

			if(PlayerAngle > 45)
			{
				while (PlayerAngle > 5)
				{
					//プレイヤーキャラクターがいる方向測定
					PlayerVec = HorizontalVector(PlayerCharacter, gameObject);

					//プレイヤーキャラクターに向ける
					RotateVec = PlayerVec;

					//行動不能になったらブレイク
					if (!EnemyScript.BehaviorFlag)
					{
						break;
					}

					//1フレーム待機
					yield return null;
				}

				//回転値初期化
				RotateVec *= 0;
			}
			else
			{
				//回転値初期化
				RotateVec *= 0;
			}

			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//回転値初期化
		RotateVec *= 0;

		//フラグを下す
		NowBehaviorDic["Wait"] = false;

		//モーション依存の移動値初期化
		MotionMoveVec *= 0;
	}

	//追跡コルーチン
	IEnumerator ChaseCoroutine()
	{
		//フラグを立てる
		NowBehaviorDic["Chase"] = true;

		//プレイヤーキャラクターに接近するまでループ
		while (PlayerDistance > ChaseDistance)
		{
			//プレイヤーキャラクターに向かう移動ベクトル算出
			BehaviorMoveVec = HorizontalVector(PlayerCharacter, gameObject);
			
			//プレイヤーキャラクターに向ける
			RotateVec = BehaviorMoveVec;

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

		//多重ループを抜ける先、待機コルーチンを避ける
		ChaseBreak:;

		//フラグを下す
		NowBehaviorDic["Chase"] = false;

		//移動ベクトル初期化
		BehaviorMoveVec *= 0;

		//モーション依存の移動値初期化
		MotionMoveVec *= 0;
	}

	//仲間避けコルーチン
	IEnumerator AroundCoroutine()
	{
		//フラグを立てる
		NowBehaviorDic["Around"] = true;

		//一番近い敵代入変数
		GameObject NearEnemy = null;

		//敵との距離
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
			RotateVec = HorizontalVector(PlayerCharacter, gameObject);

			//近い敵を避ける移動ベクトル算出
			AroundMoveVec = HorizontalVector(gameObject, NearEnemy);
			
			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag || NearEnemyDistance > Mathf.Pow(AroundDistance, 2))
			{
				break;
			}

			//チョイ待機
			yield return null;
		}

		//フラグを下す
		NowBehaviorDic["Around"] = false;

		//移動ベクトル初期化
		AroundMoveVec *= 0;

		//モーション依存の移動値初期化
		MotionMoveVec *= 0;

		//待機コルーチン呼び出し
		StartCoroutine(WaitCoroutine());
	}

	//攻撃00コルーチン
	IEnumerator Attack00Coroutine()
	{
		//フラグを立てる
		NowBehaviorDic["Attack00"] = true;

		//移動値をリセット
		MoveVec *= 0;

		//プレイヤーキャラクターに向ける
		RotateVec = HorizontalVector(PlayerCharacter, gameObject);
		
		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("Attack", true);

		//モーション名を敵スクリプトに送る
		ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetAttackMotion("00"));

		//フラグが降りるまで待機
		while(CurrentAnimator.GetBool("Attack"))
		{
			//行動不能になったらブレイク
			if (!EnemyScript.BehaviorFlag)
			{
				break;
			}

			//待機
			yield return null;
		}

		//フラグを下ろす
		NowBehaviorDic["Attack00"] = false;

		//モーション依存の移動値初期化
		MotionMoveVec *= 0;
	}

	//歩調に合わせるサインカーブ生成に使う数リセット、アニメーションクリップから呼ばれる
	public void SetSinCount()
	{
		SinCount = 0;
	}

	//攻撃用移動、プレイヤーを補足する、前後だけ、アニメーションクリップから呼ばれる
	public void StartAttackMove(float f)
	{
		//プレイヤーに向ける
		RotateVec = HorizontalVector(PlayerCharacter, gameObject);

		//加速度をセット
		MotionMoveVec = transform.forward * f;
	}

	//モーション中に移動させる、前後だけ、アニメーションクリップから呼ばれる
	public void StartBehaviorMove(float f)
	{
		MotionMoveVec = transform.forward * f;
	}
	//モーション中の移動を終わる、アニメーションクリップから呼ばれる
	public void EndBehaviorMove()
	{
		MotionMoveVec *= 0;
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
}

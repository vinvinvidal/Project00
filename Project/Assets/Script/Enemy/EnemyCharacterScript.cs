using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface EnemyCharacterInterface : IEventSystemHandler
{
	//ダメージモーションListを受け取る、セッティングスクリプトから呼ばれる
	void SetDamageAnimList(List<AnimationClip> i);

	//攻撃情報Listを受け取る、セッティングスクリプトから呼ばれる
	void SetAttackClassList(List<EnemyAttackClass> i);

	//アニメーターの攻撃モーションを切り替える、ビヘイビアスクリプトから呼ばれる
	void SetAttackMotion(string n);

	//プレイヤーからの攻撃を受けた時の処理
	void PlayerAttackHit(ArtsClass i, int c);

	//引数でプレイヤーキャラクターを受け取る
	void SetPlayerCharacter(GameObject c);

	//ポーズ処理
	void Pause(bool b);

	//自身がカメラの画角に入っているか返す
	bool GetOnCameraBool();

	//自身が死んでるか返す
	bool GetDestroyFlag();

	//当たった攻撃が有効かを返す
	bool AttackEnable(ArtsClass Arts, int n);

	//ホールド解除
	void HoldBreak(float t);
}

//敵の基本制御スクリプト
public class EnemyCharacterScript : GlobalClass, EnemyCharacterInterface
{
	//プレイヤーキャラクター
	public GameObject PlayerCharacter { get; set; }

	//OnCamera判定用スクリプトを持っているオブジェクト
	private GameObject OnCameraObject;

	//自身を追加したマネージャーのリストのインデックス
	public int ListIndex { get; set; }

	//体力
	public int Life { get; set; }

	//気絶値
	public float Stun { get; set; }

	//気絶値キャッシュ
	private float StunMax;

	//移動速度
	public float MoveSpeed { get; set; }

	//旋回速度
	public float TurnSpeed { get; set; }

	//ダウン時間
	public float DownTime { get; set; }

	//ダメージ用ヒットコライダ
	private BoxCollider DamageCol;

	//ダメージ用ヒットコライダのセンター
	private Vector3 DamageColCenter;

	//ダメージ用ヒットコライダのサイズ
	private Vector3 DamageColSize;

	//攻撃用コライダを持っているオブジェクト
	private GameObject AttackCol;

	//使用している攻撃
	private EnemyAttackClass UseArts;

	//自身のキャラクターコントローラ
	private CharacterController CharaController;

	//キャラクターコントローラのセンター
	private Vector3 CharaConCenter;

	//キャラクターコントローラの高さ
	private float CharaConHeight;

	//キャラクターコントローラの半径
	private float CharaConRad;

	//キャラクターコントローラの皮
	private float CharaConSkin;

	//自身のアニメーターコントローラ
	private Animator CurrentAnimator;

	//オーバーライドアニメーターコントローラ、アニメーションクリップ差し替え時に使う
	public AnimatorOverrideController OverRideAnimator { get; set; }

	//アニメーターのレイヤー0に存在する全てのステート名
	private List<string> AllStates = new List<string>();

	//アニメーターのレイヤー0に存在する全てのトランジション名
	private List<string> AllTransitions = new List<string>();

	//現在のステート
	private string CurrentState;

	//全ての攻撃情報List
	public List<EnemyAttackClass> AttackClassList { get; set; }

	//全てのダメージモーションList
	public List<AnimationClip> DamageAnimList { get; set; }

	//ダメージモーションを制御するステート
	private int DamageState = 0;

	//全ての移動値を合算した移動ベクトル
	private Vector3 MoveMoment;

	//ノックバックの移動ベクトル
	private Vector3 KnockBackVec;

	//ダメージモーション中の移動ベクトル
	private Vector3 DamageMoveVec;

	//ホールド中の移動ベクトル
	private Vector3 HoldVec;

	//特殊攻撃中の移動ベクトル
	public Vector3 SpecialMoveVec { get; set; }

	//地面との距離
	private float GroundDistance;

	//レイが当たったオブジェクトの情報を格納
	private RaycastHit RayHit;

	//キャラクターの接地判定をするレイの発射位置
	private Vector3 RayPoint;

	//キャラクターの接地判定をするレイの大きさ
	private float RayRadius;

	//重力加速度
	private float Gravity;

	//重力補正値
	private float GraityCorrect;

	//強制移動ベクトル
	private Vector3 ForceMoveVector;

	//接地フラグ
	public bool OnGround { get; set; }

	//ノックバックフラグ
	public bool KnockBackFlag { get; set; }

	//ダメージモーション中フラグ
	public bool DamageFlag { get; set; }

	//キャラクターに触れているフラグ
	public bool ContactCharacterFlag { get; set; }

	//行動処理可能フラグ
	public bool BehaviorFlag { get; set; }

	//スタン状態フラグ
	public bool StunFlag { get; set; }

	//ダウン状態フラグ
	public bool DownFlag { get; set; }

	//ホールドダメージ状態フラグ
	public bool HoldFlag { get; set; }

	//特殊攻撃状態フラグ
	public bool SpecialFlag { get; set; }

	//打ち上げ状態フラグ
	public bool RiseFlag { get; set; }

	//死んでるかフラグ
	public bool DestroyFlag { get; set; }

	//ポーズフラグ
	public bool PauseFlag { get; set; }

	//吹っ飛び状態フラグ
	public bool BlownFlag { get; set; }

	//壁激突フラグ
	public bool WallClashFlag { get; set; }

	//ダウン状態で当たってもそのままモーション再生する攻撃List
	private List<int> DownEnableAttakList;

	//打ち上げ状態で当たってもそのままモーション再生する攻撃List
	private List<int> RiseEnableAttakList;

	//モーションにノイズを加える量
	private float AnimNoiseVol = 5;

	//モーションにノイズを加えるボーンList
	private List<GameObject> AnimNoiseBone = new List<GameObject>();

	//モーションノイズのランダムシードList
	private List<Vector3> AnimNoiseSeedList = new List<Vector3>();

	//打ち上げ時にモーションにノイズを加えるボーンList
	private List<GameObject> RiseAnimNoiseBone = new List<GameObject>();

	//打ち上げ時にモーションノイズのランダムシードList
	private List<Vector3> RiseAnimNoiseSeedList = new List<Vector3>();

	void Start()
	{
		//プレイヤーキャラクター取得
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => PlayerCharacter = reciever.GetPlayableCharacterOBJ());

		//OnCamera判定用スクリプトを持っているオブジェクトを検索して取得
		foreach (Transform i in GetComponentsInChildren<Transform>())
		{
			if (i.GetComponent<OnCameraScript>() != null)
			{
				OnCameraObject = i.gameObject;
			}
		}

		//敵の管理をするマネージャーが持っているリストに自身を追加、戻り値でリストのインデックスを受け取る
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => ListIndex = reciever.AddAllActiveEnemyList(gameObject));

		//接地フラグ初期化
		OnGround = true;

		//ノックバックフラグ初期化
		KnockBackFlag = false;

		//ダメージモーション中フラグ初期化
		DamageFlag = false;

		//キャラクターに触れているフラグ初期化
		ContactCharacterFlag = false;

		//行動可能フラグ初期化
		BehaviorFlag = true;

		//スタン状態フラグ初期化
		StunFlag = false;

		//ダウン状態フラグ初期化
		DownFlag = false;

		//ホールドダメージ状態フラグ初期化
		HoldFlag = false;

		//打ち上げ状態フラグ初期化
		RiseFlag = false;

		//特殊攻撃フラグ初期化
		SpecialFlag = false;

		//死んでるかフラグ初期化
		DestroyFlag = false;

		//ポーズフラグ初期化
		PauseFlag = false;

		//吹っ飛び状態フラグ初期化
		BlownFlag = false;

		//壁激突フラグ初期化
		WallClashFlag = false;

		//ダメージ用ヒットコライダ取得
		DamageCol = DeepFind(gameObject , "EnemyDamageCol").GetComponent<BoxCollider>();

		//ダメージ用ヒットコライダのセンターをキャッシュ
		DamageColCenter = DamageCol.center;

		//ダメージ用ヒットコライダのサイズをキャッシュ
		DamageColSize = DamageCol.size;

		//攻撃用コライダを持っているオブジェクト取得
		AttackCol = DeepFind(gameObject, "EnemyAttackCol");

		//自身のキャラクターコントローラ取得
		CharaController = GetComponent<CharacterController>();

		//キャラクターコントローラのセンターをキャッシュ
		CharaConCenter = CharaController.center;

		//キャラクターコントローラの高さをキャッシュ
		CharaConHeight = CharaController.height;

		//キャラクターコントローラの半径をキャッシュ
		CharaConRad = CharaController.radius;

		//キャラクターコントローラの皮をキャッシュ
		CharaConSkin = CharaController.skinWidth;

		//自身のアニメーターコントローラ取得
		CurrentAnimator = GetComponent<Animator>();

		//オーバーライドアニメーターコントローラ初期化
		OverRideAnimator = new AnimatorOverrideController();

		//オーバーライドアニメーターコントローラに元アニメーターコントローラをコピー
		OverRideAnimator.runtimeAnimatorController = CurrentAnimator.runtimeAnimatorController;

		//気絶値をキャッシュ、最大値に使う
		StunMax = Stun;

		//全ての移動値を合算した移動ベクトル初期化
		MoveMoment = Vector3.zero;

		//ノックバックの移動ベクトル初期化
		KnockBackVec = Vector3.zero;

		//ダメージモーション中の移動ベクトル初期化
		DamageMoveVec = Vector3.zero;

		//特殊攻撃移動ベクトル初期化
		SpecialMoveVec = Vector3.zero;

		//ホールド中の移動ベクトル初期化
		HoldVec = Vector3.zero;

		//地面との距離初期化
		GroundDistance = 0.0f;

		//キャラクターの接地判定をするレイの発射位置、キャラクターコントローラから算出
		//RayPoint = new Vector3(0, CharaController.height, CharaController.center.z);
		RayPoint = new Vector3(0, 1, 0);

		//キャラクターの接地判定をするレイの大きさ、キャラクターコントローラから算出
		//RayRadius = new Vector3(CharaController.radius, CharaController.height * 0.5f, CharaController.radius);
		RayRadius = 0.5f;

		//重力加速度初期化
		Gravity = Physics.gravity.y * Time.deltaTime;

		//重力補正値初期化
		GraityCorrect = 0;

		//強制移動ベクトル初期化
		ForceMoveVector = Vector3.zero;

		//ダウン時間初期化
		DownTime = 0;

		//ダウン状態で当たってもそのままモーション再生する攻撃List初期化
		DownEnableAttakList = new List<int>();

		//打ち上げ攻撃
		DownEnableAttakList.Add(11);

		//胸倉ホールド攻撃
		DownEnableAttakList.Add(30);

		//打ち上げ状態で当たってもそのままモーション再生する攻撃List初期化
		RiseEnableAttakList = new List<int>();

		//吹っ飛ばし攻撃
		RiseEnableAttakList.Add(2);

		//叩きつけ攻撃
		RiseEnableAttakList.Add(6);
		RiseEnableAttakList.Add(7);

		//香港スピン攻撃
		RiseEnableAttakList.Add(13);

		//胸倉ホールド攻撃
		RiseEnableAttakList.Add(30);

		//全てのステート名を手動でAdd、アニメーターのステート名は外部から取れない
		AllStates.Add("Idling");
		AllStates.Add("Walk");
		AllStates.Add("Attack");
		AllStates.Add("DownLanding");
		AllStates.Add("Damage00");
		AllStates.Add("Damage01");
		AllStates.Add("Special");
		AllStates.Add("HoldDamage");
		AllStates.Add("Down_Prone");
		AllStates.Add("Down_Supine");
		AllStates.Add("GetUp_Prone");
		AllStates.Add("GetUp_Supine");

		//全てのステートとトランジションをListにAdd
		foreach (string i in AllStates)
		{
			//AllStatesを2重で回す
			foreach (string ii in AllStates)
			{
				//全てのステート名の組み合わせのトランジション名をAddする、存在しないトランジションもあるが気にしない
				AllTransitions.Add(i + " -> " + ii);
			}
		}

		//モーションにノイズを加えるボーンListに手動でAdd
		foreach (Transform i in gameObject.GetComponentsInChildren<Transform>())
		{
			if (i.name.Contains("Bone"))
			{

				if (i.name.Contains("Spine") ||
					i.name.Contains("Shoulder") ||
					i.name.Contains("Arm") ||
					i.name.Contains("Elbow") ||
					i.name.Contains("Hand") ||
					i.name.Contains("Neck") ||
					i.name.Contains("Head")
				)
				{
					//条件に合ったボーンをListにAdd
					AnimNoiseBone.Add(i.gameObject);

					//同じ数だけランダムシードを作成
					AnimNoiseSeedList.Add(new Vector3(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f)));
				}
			}

			//指
			if (i.name.Contains("Thumb") ||
					i.name.Contains("First") ||
					i.name.Contains("Middle") ||
					i.name.Contains("Ring") ||
					i.name.Contains("Pinky"))
			{
				//条件に合ったボーンをListにAdd
				AnimNoiseBone.Add(i.gameObject);

				//同じ数だけランダムシードを作成
				AnimNoiseSeedList.Add(new Vector3(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f)));
			}

			//打ち上げ時
			if (i.name.Contains("Leg") ||
					i.name.Contains("Knee") ||
					i.name.Contains("Pelvis"))
			{
				//条件に合ったボーンをListにAdd
				RiseAnimNoiseBone.Add(i.gameObject);

				//同じ数だけランダムシードを作成
				RiseAnimNoiseSeedList.Add(new Vector3(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f)));
			}
		}
	}

	void LateUpdate()
	{
		//ループカウント
		int count = 0;

		//揺らし具合を設定
		AnimNoiseVol = 1f;

		//打ち上げ
		if (RiseFlag || HoldFlag || BlownFlag)
		{
			//打ち上げ時に揺らすポーンListを回す
			foreach (GameObject i in RiseAnimNoiseBone)
			{
				//揺らし具合を設定
				AnimNoiseVol = 5f;

				//ノイズ生成
				Vector3 NoiseVec1 = new Vector3
				(
					Mathf.PerlinNoise(Time.time * 0.25f * AnimNoiseVol, RiseAnimNoiseSeedList[count].x) - 0.5f,
					Mathf.PerlinNoise(Time.time * 0.25f * AnimNoiseVol, RiseAnimNoiseSeedList[count].y) - 0.5f,
					Mathf.PerlinNoise(Time.time * 0.25f * AnimNoiseVol, RiseAnimNoiseSeedList[count].z) - 0.5f
				);

				//ノイズ生成
				Vector3 NoiseVec2 = new Vector3
				(
					Mathf.PerlinNoise(Time.time * AnimNoiseVol, RiseAnimNoiseSeedList[count].x) - 0.5f,
					Mathf.PerlinNoise(Time.time * AnimNoiseVol, RiseAnimNoiseSeedList[count].y) - 0.5f,
					Mathf.PerlinNoise(Time.time * AnimNoiseVol, RiseAnimNoiseSeedList[count].z) - 0.5f
				);

				//打ち上げ揺らし具合を設定
				if (RiseFlag)
				{
					//地面から離れているほど揺らす
					AnimNoiseVol = 3f * GroundDistance;

					//ボーンにパーリンノイズの回転を加えて揺らす
					i.transform.localRotation *= Quaternion.Euler((NoiseVec1 + NoiseVec2 * 0.25f) * 5f * AnimNoiseVol);
				}
				//ホールド時は小刻みに揺らす
				else if(HoldFlag || BlownFlag)
				{
					AnimNoiseVol = 10f;

					//ボーンにパーリンノイズの回転を加えて揺らす
					i.transform.localRotation *= Quaternion.Euler((NoiseVec1 + NoiseVec2) * AnimNoiseVol);
				}			

				//カウントアップ
				count++;
			}

			//LateUpdateで動かすとコンストレイントが利かないので手動で足首を繋げる
			DeepFind(gameObject, "R_FootBone").transform.position = DeepFind(gameObject, "R_LowerLegBone_end").transform.position;
			DeepFind(gameObject, "L_FootBone").transform.position = DeepFind(gameObject, "L_LowerLegBone_end").transform.position;
		}

		//ループカウント
		count = 0;

		//揺らすポーンListを回す
		foreach (GameObject i in AnimNoiseBone)
		{
			//ノイズ生成
			Vector3 NoiseVec1 = new Vector3
			(
				Mathf.PerlinNoise(Time.time * 0.25f, AnimNoiseSeedList[count].x) - 0.5f,
				Mathf.PerlinNoise(Time.time * 0.25f, AnimNoiseSeedList[count].y) - 0.5f,
				Mathf.PerlinNoise(Time.time * 0.25f, AnimNoiseSeedList[count].z) - 0.5f
			);

			//ノイズ生成
			Vector3 NoiseVec2 = new Vector3
			(
				Mathf.PerlinNoise(Time.time, AnimNoiseSeedList[count].x) - 0.5f,
				Mathf.PerlinNoise(Time.time, AnimNoiseSeedList[count].y) - 0.5f,
				Mathf.PerlinNoise(Time.time, AnimNoiseSeedList[count].z) - 0.5f
			);

			//ボーンにパーリンノイズの回転を加えて揺らす
			i.transform.localRotation *= Quaternion.Euler((NoiseVec1 + NoiseVec2 * 0.25f) * 5f * AnimNoiseVol);

			//カウントアップ
			count++;
		}
	}

	void Update()
	{
		if (!PauseFlag)
		{
			//アニメーションステートを監視する関数呼び出し
			StateMonitor();

			//接地判定用のRayを飛ばす関数呼び出し
			GroundRayCast();

			//周囲の敵にめり込まないようにする関数
			EnemyAround();

			//移動制御関数呼び出し
			MoveFunc();

			//死亡監視関数呼び出し
			DeadFunc();

			//行動可能判定関数呼び出し
			BehaviorFund();
		}
	}

	//敵めり込み防止関数
	public void EnemyAround()
	{
		//強制移動ベクトル初期化
		ForceMoveVector *= 0;

		if (!HoldFlag && !SpecialFlag && !DownFlag)
		{
			//近くにプレイヤーがいたら処理
			if (HorizontalVector(gameObject, PlayerCharacter).sqrMagnitude < 1f && gameObject.transform.position.y - PlayerCharacter.transform.position.y < 1f && gameObject.transform.position.y - PlayerCharacter.transform.position.y > -0.1f)
			{
				//敵と自分までのベクトルで強制移動
				ForceMoveVector += CharaController.transform.position - new Vector3(PlayerCharacter.transform.position.x, transform.position.y, PlayerCharacter.transform.position.z);
			}
		}
	}

	//行動可能判定関数
	private void BehaviorFund()
	{
		BehaviorFlag =
			CurrentState.Contains("Idling") ||
			CurrentState.Contains("Walk") ||
			CurrentState.Contains("Attack");
	}

	//接地判定用のRayを飛ばす関数
	private void GroundRayCast()
	{
		//レイを飛ばして接地判定をしてフラグを切り替える
		if (Physics.SphereCast(transform.position + RayPoint, RayRadius, Vector3.down, out RayHit, Mathf.Infinity, LayerMask.GetMask("TransparentFX")))
		{
			//地面との距離に値を入れる
			GroundDistance = RayHit.distance - RayRadius;

			if (GroundDistance < 0.1f || CharaController.isGrounded)
			{
				OnGround = true;
			}
			else
			{
				OnGround = false;
			}
		}
		else
		{
			OnGround = false;
		}

		//アニメーターに接地フラグを送る
		CurrentAnimator.SetBool("Ground", OnGround);
	}

	//移動制御関数
	void MoveFunc()
	{
		//特殊攻撃を受けている
		if(SpecialFlag)
		{
			MoveMoment = SpecialMoveVec * Time.deltaTime;
		}
		//攻撃を喰らった瞬間のノックバック
		else if (KnockBackFlag)
		{
			MoveMoment = KnockBackVec * Time.deltaTime;
		}
		//ダメージモーション依存の移動
		else if (DamageFlag)
		{
			MoveMoment = DamageMoveVec * Time.deltaTime;
		}
		//ホールドダメージ状態
		else if (HoldFlag)
		{
			MoveMoment = HoldVec * Time.deltaTime;
		}
		else
		{
			MoveMoment *= 0;
		}

		//条件で重力加速度を増減させる
		if ((!OnGround && (PlayerCharacter.transform.position - transform.position).sqrMagnitude > Mathf.Pow(3, 2)) || CurrentState.Contains("DownLanding"))
		{
			Gravity += Physics.gravity.y * 2 * Time.deltaTime;

			GraityCorrect = 0;
		}
		else if (!OnGround)
		{
			Gravity += Physics.gravity.y * Time.deltaTime;

			GraityCorrect = 2;
		}
		else
		{
			Gravity = Physics.gravity.y * Time.deltaTime;

			GraityCorrect = 0;
		}

		//強制移動の値を入れる
		MoveMoment += ForceMoveVector * MoveSpeed * Time.deltaTime;

		//重力加速度を加える
		MoveMoment.y += (Gravity + GraityCorrect) * Time.deltaTime;

		//移動
		CharaController.Move(MoveMoment);
	}

	//当たった攻撃が有効かを返す関数
	public bool AttackEnable(ArtsClass Arts, int n)
	{
		//出力用変数宣言
		bool re = true;

		//ヒット条件を満たしているか判定
		if
		(
			//ダウン中にダウンに当たらない攻撃が当たった
			(DownFlag && Arts.DownEnable[n] != 1)
			||
			//ホールド状態じゃないのに、ホールド追撃専用技が当たった
			(!HoldFlag && Arts.ColType[n] == 6)
			||
			//地上限定の技が空中で当たった
			(!OnGround && Arts.ColType[n] == 8)
		)
		{
			re = false;
		}

		//出力
		return re;
	}

	//プレイヤーからの攻撃を受けた時の処理
	public void PlayerAttackHit(ArtsClass Arts, int n)
	{
		//ライフを減らす
		Life -= Arts.Damage[n];

		//タメ攻撃の係数を掛ける
		Life -= Arts.ChargeDamage[n] * Arts.ChargeLevel;

		//攻撃用コライダを無効化
		EndAttackCol();

		//気絶値を減らす
		Stun -= Arts.Stun[n];
	
		//ダメージフラグ管理関数呼び出し
		DamageFlagFunc(Arts, n);

		//ノックバックコルーチン呼び出し
		StartCoroutine(DamageKnockBack(Arts, n));
	}

	//ダメージフラグ管理関数
	private void DamageFlagFunc(ArtsClass Arts, int n)
	{
		//ダメージステートカウントアップ
		DamageState++;

		//現在のアニメーターの遷移フラグを立てる
		CurrentAnimator.SetBool("Damage0" + DamageState % 2, true);

		//次の遷移フラグを下ろす
		CurrentAnimator.SetBool("Damage0" + (DamageState + 1) % 2, false);

		//アニメーターの攻撃フラグを下ろす
		CurrentAnimator.SetBool("Attack", false);

		//ちゃんと技がある
		if (Arts != null)
		{
			//再生するモーションを食らった技や状況で切り替えるインデックスをキャッシュ
			int UseIndex = Arts.AttackType[n];

			//打ち上げられている状態で食らった
			if (RiseFlag && !RiseEnableAttakList.Any(a => a == Arts.AttackType[n]))
			{
				//打ち上げモーションに切り替え
				UseIndex = 11;
			}
			//ダウンしている状態でダウンに当たる攻撃が当たった
			if (DownFlag && !DownEnableAttakList.Any(a => a == Arts.AttackType[n]))
			{
				//ダウンタイム更新
				DownTime = 2;

				//叩きつけモーションに切り替え
				if (CurrentState.Contains("Supine") || CurrentState.Contains("DownLanding"))
				{
					UseIndex = 7;
				}
				else if (CurrentState.Contains("Prone"))
				{
					UseIndex = 6;
				}
			}
			//打ち上げられてないけど空中技が当たった
			else if (!RiseFlag && Arts.AttackType[n] == 20)
			{
				UseIndex = 3;
			}
			//胸倉掴みホールド
			else if (UseIndex == 30)
			{
				//立ち胸倉
				OverRideAnimator["Hold_Void"] = DamageAnimList[UseIndex];

				//仰向け胸倉
				if (CurrentAnimator.GetBool("Down_Supine") && !RiseFlag)
				{
					OverRideAnimator["Hold_Void"] = DamageAnimList[31];
				}
				//うつ伏せ胸倉
				else if (CurrentAnimator.GetBool("Down_Prone") && !RiseFlag)
				{
					OverRideAnimator["Hold_Void"] = DamageAnimList[32];
				}
			}

			//使用するモーションに差し替え
			OverRideAnimator["Damage_Void_" + DamageState % 2] = DamageAnimList[UseIndex];

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

			//ダメージフラグを下す
			DamageFlag = false;

			//打ち上げフラグを下ろす
			RiseFlag = false;

			//ホールドフラグを下ろす
			HoldFlag = false;

			//壁激突フラグを下す
			WallClashFlag = false;

			//特殊攻撃フラグを下す
			SpecialFlag = false;

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Prone", false);

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Supine", false);

			//アニメーターのダウン着地フラグを下ろす
			CurrentAnimator.SetBool("DownLanding", false);

			//ダウンしない攻撃遷移判定
			if (UseIndex == 0 || UseIndex == 3)
			{
				//特に何もしない
			}
			//うつ伏せダウン遷移判定
			else if (UseIndex == 1 || UseIndex == 4 || UseIndex == 6)
			{
				//アニメーターのダウンフラグを立てる
				CurrentAnimator.SetBool("Down_Prone", true);

				//靴の高さを表すskinWidthを消す
				CharaControllerReset("Skin");
			}
			//仰向けダウン遷移判定
			else if (UseIndex == 7)
			{
				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Down_Supine", true);

				//靴の高さを表すskinWidthを消す
				CharaControllerReset("Skin");
			}
			//打ち上げ判定
			else if (UseIndex == 11)
			{
				//打ち上げフラグを立てる
				RiseFlag = true;

				//重力をリセット
				Gravity = 0;

				//アニメーターのダウン着地フラグを立てる
				CurrentAnimator.SetBool("DownLanding", true);

				//アニメーターのフラグを立てる
				CurrentAnimator.SetBool("Down_Supine", true);

				//靴の高さを表すskinWidthを消す
				CharaControllerReset("Skin");
			}
			//香港スピン
			else if (UseIndex == 12 || UseIndex == 13)
			{
				//重力をリセット
				Gravity = 0;

				//アニメーターのダウンフラグを立てる
				CurrentAnimator.SetBool("Down_Prone", true);

				//靴の高さを表すskinWidthを消す
				CharaControllerReset("Skin");
			}
			//吹っ飛びダウン遷移判定
			else if (UseIndex == 2 || UseIndex == 5)
			{
				//吹っ飛びフラグを立てる
				BlownFlag = true;

				//アニメーターのダウンフラグを立てる
				CurrentAnimator.SetBool("Down_Prone", true);

				//靴の高さを表すskinWidthを消す
				CharaControllerReset("Skin");

				//アニメーションの再生速度を落とす
				CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 0.05f);

				//キャラクターの方を向く
				transform.rotation = Quaternion.LookRotation(HorizontalVector(PlayerCharacter, gameObject));

				//ちょっと浮かす
				CharaController.Move(Vector3.up * Time.deltaTime * 3);
			}
			//ホールドダメージ
			else if (Arts.AttackType[n] == 30)
			{
				//ホールドダメージフラグを立てる
				HoldFlag = true;

				//キャラクターの方を向く
				transform.rotation = Quaternion.LookRotation(HorizontalVector(PlayerCharacter, gameObject));

				//アニメーターのホールドダメージフラグを立てる
				CurrentAnimator.SetBool("HoldDamage", true);

				//アニメーターのダメージフラグを下す
				CurrentAnimator.SetBool("Damage00", false);

				//アニメーターのダメージフラグを下す
				CurrentAnimator.SetBool("Damage01", false);

				//ホールド持続コルーチン呼び出し
				StartCoroutine(HoldWaitCoroutine(Arts.HoldPosList[n]));
			}
		}
		//技無しで0なら壁激突
		else if (n == 0)
		{
			//キャラクターコントローラの大きさを元に戻す
			CharaControllerReset("Reset");

			//吹っ飛ばしフラグを下す
			BlownFlag = false;

			//打ち上げフラグを下す
			RiseFlag = false;

			//壁激突フラグを立てる
			WallClashFlag = true;

			//使用するモーションに差し替え
			OverRideAnimator["Damage_Void_" + DamageState % 2] = DamageAnimList[9];

			//再生フレームを最初にする
			CurrentAnimator.Play("Damage0" + DamageState % 2, 0, 0);

			//再生ステートのスピードを戻す
			CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 1);

			//遷移フラグを下ろす
			CurrentAnimator.SetBool("Damage0" + DamageState % 2, false);

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Prone", false);

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Supine", false);
		}
	}

	//アニメーションステートを監視する関数
	private void StateMonitor()
	{
		//キャッシュしているステートと現在のステートを比較
		if (CurrentState != NowStateToString())
		{
			//ステートが変化したらキャッシュを更新
			CurrentState = NowStateToString();

			//ダメージモーションになった瞬間の処理
			if (CurrentState.Contains("-> Damage"))
			{
				//ダメージフラグを立てる
				DamageFlag = true;

				//ダメージモーション依存の移動値を初期化
				DamageMoveVec *= 0;

				//ダメージ遷移フラグを下ろす
				CurrentAnimator.SetBool("Damage00", false);
				CurrentAnimator.SetBool("Damage01", false);

				//状況によって本体のコリジョンを切り替える
				if (OnGround && !DownFlag)
				{
					//キャラクターコントローラの大きさを元に戻す
					CharaControllerReset("Reset");
				}
				else if (DownFlag && !BlownFlag)
				{
					//キャラクターコントローラの大きさを変える
					CharaControllerReset("Down");
				}
				else if (RiseFlag)
				{
					//キャラクターコントローラの大きさを変える
					CharaControllerReset("Rise");
				}
			}
			//ダウン着地になった瞬間の処理
			else if (CurrentState.Contains("-> DownLanding"))
			{
				//打ち上げフラグを下ろす
				RiseFlag = false;

				//アニメーターのダウン着地フラグを下ろす
				CurrentAnimator.SetBool("DownLanding", false);
			}				
			//ダウンになった瞬間の処理
			else if (CurrentState.Contains("-> Down"))
			{
				//打ち上げフラグを下ろす
				RiseFlag = false;

				//キャラクターコントローラの大きさを変える
				CharaControllerReset("Down");

				//ダウン継続中じゃない
				if(!DownFlag)
				{
					//ダウン制御コルーチン呼び出し
					StartCoroutine(DownCoroutine());
				}
			}
			//起き上がりになった瞬間の処理
			else if (CurrentState.Contains("-> GetUp"))
			{
				//キャラクターコントローラの大きさを元に戻す
				CharaControllerReset("Reset");
			}
			//アイドリングになった瞬間の処理
			else if (CurrentState.Contains("-> Idling"))
			{
				//フラグ状態をまっさらに戻す関数呼び出し
				ClearFlag();
			}
			//ホールドになった瞬間の処理
			else if (CurrentState == "HoldDamage")
			{
				//ホールドベクトルを初期化
				HoldVec *= 0;
			}
			//特殊攻撃を喰らった瞬間の処理
			else if (CurrentState == "Special")
			{
				//攻撃遷移フラグを下す
				CurrentAnimator.SetBool("Attack", false);
			}
		}
	}

	//ダウン制御コルーチン
	IEnumerator DownCoroutine()
	{
		//ダウンフラグを立てる
		DownFlag = true;

		//ダウンタイム設定
		DownTime = 2;

		//ダウンしている、打ち上げられてない、ホールドされてない、ダウンタイムがある
		while (DownFlag  && !RiseFlag && !HoldFlag && DownTime > 0)
		{
			//ダウン時間カウントダウン
			DownTime -= Time.deltaTime;

			//1フレーム待機
			yield return null;
		}

		//ダウンフラグを下す
		DownFlag = false;

		//ダウンから打ち上げならダウンフラグは下さない
		if(!RiseFlag)
		{
			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Prone", false);

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Supine", false);
		}
	}

	//ノックバック処理
	IEnumerator DamageKnockBack(ArtsClass Arts, int n)
	{
		//ノックバックフラグを入れる
		KnockBackFlag = true;

		//ベクトル代入用変数
		Vector3 tempVector = Vector3.zero;

		//持続時間代入用変数
		float tempTime = 0.1f;

		//経過時間を宣言
		float t = 0.0f;

		//打ち上げ時に地上の通常技が当たった場合
		if (RiseFlag && (Arts.AttackType[n] == 0 || Arts.AttackType[n] == 3))
		{
			//ちょっと浮かす
			tempVector = Vector3.up;
		}
		//打ち上げられていないのに空中通常技が当たった場合
		else if(!RiseFlag && Arts.AttackType[n] == 20)
		{
			//後ろにノックバック
			tempVector = Vector3.forward;
		}
		//普通に当たった
		else
		{
			//受け取ったノックバックベクトルを代入
			tempVector = new Vector3(Arts.KnockBackVec[n].r, Arts.KnockBackVec[n].g, Arts.KnockBackVec[n].b);

			//持続時間代入
			tempTime = Arts.KnockBackVec[n].a;
		}

		//通常攻撃
		if(Arts.ColType[n] != 7 && Arts.ColType[n] != 8)
		{
			//プレイヤーキャラクターの正面ベクトルを基準にノックバックベクトルを求める
			KnockBackVec = Quaternion.LookRotation(PlayerCharacter.transform.forward, transform.up) * tempVector;
		}
		//範囲攻撃
		else
		{
			//プレイヤーキャラクターから敵本体までのベクトルを基準にノックバックベクトルを求める
			KnockBackVec = Quaternion.LookRotation(transform.position - PlayerCharacter.transform.position, transform.up).normalized * tempVector;
		}

		//経過時間が過ぎるか地上に降りるまで待機
		while (t < tempTime || !OnGround)
		{
			//経過時間更新
			t += Time.deltaTime;

			//壁に当たったらブレイク
			if (WallClashFlag)
			{
				//壁激突フラグを下す
				WallClashFlag = false;

				//ループを抜ける
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//ノックバックフラグを切る
		KnockBackFlag = false;

		//吹っ飛び中なら
		if(BlownFlag)
		{
			//吹っ飛ばしフラグを下す
			BlownFlag = false;

			//着地アニメーション開始位置までフレームをジャンプ
			CurrentAnimator.Play("Damage0" + DamageState % 2, 0, 0.1f);
		}

		//モーション再生スピードを戻す
		CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 1);

		//ノックバックの移動ベクトル初期化
		KnockBackVec *= 0;

		//死んだら
		if (Life <= 0)
		{
			//死亡フラグを立てる
			DestroyFlag = true;
		}

		//コルーチンを抜ける
		yield break;
	}

	//ホールド持続コルーチン
	IEnumerator HoldWaitCoroutine(Vector3 pos)
	{
		//変数宣言
		Vector3 HoldPos = Vector3.zero;

		//ホールド持続待機
		while (HoldFlag)
		{
			//ホールドポジション更新
			HoldPos = (PlayerCharacter.transform.right * pos.x) + (PlayerCharacter.transform.up * pos.y) + (PlayerCharacter.transform.forward * pos.z);

			//ホールドベクトルを設定
			HoldVec = ((PlayerCharacter.transform.position + HoldPos) - gameObject.transform.position) * 20;

			//1フレーム待機
			yield return null;
		}

		if(CurrentState == "HoldDamage")
		{
			//ループ終わりまでジャンプ、決め打ちは良くないかも
			CurrentAnimator.Play("HoldDamage", 0, 0.6f);
		}

		//アニメーターのホールドフラグを下ろす
		CurrentAnimator.SetBool("HoldDamage", false);

		//キャラクターコントローラの大きさを元に戻す
		CharaControllerReset("Reset");

		//ホールドベクトル初期化
		HoldVec *= 0;
	}

	//ホールド解除
	public void HoldBreak(float t)
	{
		//ホールドされていたら
		if(HoldFlag)
		{
			//ループ終わりまでジャンプ、決め打ちは良くないかも
			CurrentAnimator.Play("HoldDamage", 0, 0.6f);

			//ホールドブレイクコルーチン呼び出し
			StartCoroutine(HoldBreakCoroutine(t));
		}

		//ホールドフラグを下す
		HoldFlag = false;
	}

	//ホールドブレイクコルーチン
	IEnumerator HoldBreakCoroutine(float t)
	{
		//開始時刻キャッシュ
		float BreakTime = Time.time;

		//フラグを立てる
		DamageFlag = true;

		//経過時間待機
		while (BreakTime + t > Time.time)
		{
			//後ろ下げ
			DamageMoveVec = -transform.forward * 3.5f;

			//1フレーム待機
			yield return null;
		}

		//フラグを下ろす
		DamageFlag = false;

		//ダメージモーション中の移動ベクトル初期化
		DamageMoveVec *= 0;
	}

	//死亡監視関数
	private void DeadFunc()
	{
		//死んだらノックアウト処理
		if (DestroyFlag)
		{
			//ゲームマネージャーのListから自身を削除
			ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.RemoveAllActiveEnemyList(ListIndex));

			//オブジェクト削除
			Destroy(gameObject);
		}
	}

	//ダメージモーション中に移動させる、前後だけ、アニメーションクリップから呼ばれる
	public void StartDamageMove(float f)
	{
		DamageMoveVec = transform.forward * f;
	}
	//ダメージモーション中の移動を終わる、アニメーションクリップから呼ばれる
	public void EndDamageMove()
	{
		DamageMoveVec *= 0;
	}

	//ダメージ時にアニメーションフレーム操作する、アニメーションクリップから呼ばれる
	public void DamageMotionJump(float f)
	{
		if (HoldFlag)
		{
			CurrentAnimator.Play("HoldDamage", 0, f);
		}
	}

	//空中で叩きつけを喰らった時に着地を待つ、アニメーションクリップから呼ばれる
	public void FallWait()
	{
		StartCoroutine(FallWaitCoroutine());
	}
	IEnumerator FallWaitCoroutine()
	{
		//モーションを止める
		CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 0f);

		//着地待機
		while (!OnGround)
		{
			//1フレーム待機
			yield return null;
		}

		//着地したらモーションを再生
		CurrentAnimator.SetFloat("DamageMotionSpeed0", 1);
		CurrentAnimator.SetFloat("DamageMotionSpeed1", 1);
	}

	//キャラクターコントローラコライダヒット
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//吹っ飛ばし攻撃で壁に叩きつけられた
		if (BlownFlag && KnockBackFlag && Vector3.Dot(hit.normal.normalized, KnockBackVec.normalized) < -0.8f && GroundDistance < 1.5f)
		{
			//壁に合わせて回転、水平を保つ為にyは回さない
			transform.LookAt(new Vector3(hit.normal.x, 0, hit.normal.z) + transform.position);

			//処理を１フレーム待つためにコルーチン呼び出し
			StartCoroutine(WallClashCoroutine());
		}
	}
	IEnumerator WallClashCoroutine()
	{
		//1フレーム待機
		yield return null;

		//フラグ処理関数呼び出し
		DamageFlagFunc(null, 0);
	}

	//現在のステートを文字列で返す関数
	private string NowStateToString()
	{
		//Return用変数宣言
		string re = null;

		//まずトランジション名で調べる
		foreach (string i in AllTransitions)
		{
			if (CurrentAnimator.GetAnimatorTransitionInfo(0).IsName(i))
			{
				re = i;
			}
		}

		//遷移中じゃなければステート名を入れる
		if (re == null)
		{
			foreach (string i in AllStates)
			{
				if (CurrentAnimator.GetCurrentAnimatorStateInfo(0).IsName(i))
				{
					re = i;
				}
			}
		}

		//出力
		return re;
	}

	//フラグ状態をまっさらに戻す関数
	private void ClearFlag()
	{
		//ダウンタイム初期化
		DownTime = 0;

		//ダウンフラグを下す
		DownFlag = false;

		//ダメージモーションフラグを下ろす
		DamageFlag = false;

		//ホールドダメージ状態を下す
		HoldFlag = false;

		//吹っ飛びフラグを下す
		BlownFlag = false;

		//特殊攻撃フラグを下す
		SpecialFlag = false;

		//アニメーターのダウンフラグを下す
		CurrentAnimator.SetBool("Down_Prone", false);

		//アニメーターのダウンフラグを下す
		CurrentAnimator.SetBool("Down_Supine", false);

		//ダメージモーション再生スピードを戻す
		CurrentAnimator.SetFloat("DamageMotionSpeed0", 1);
		CurrentAnimator.SetFloat("DamageMotionSpeed1", 1);

		//ダメージモーション中の移動ベクトル初期化
		DamageMoveVec *= 0;

		//キャラクターコントローラの大きさを元に戻す
		CharaControllerReset("Reset");
	}

	//キャラクターコントローラの設定を変える関数
	private void CharaControllerReset(string FlagName)
	{
		switch(FlagName)
		{
			case "Reset":

				//キャラクターコントローラの大きさを戻す
				CharaController.center = CharaConCenter;
				CharaController.height = CharaConHeight;
				CharaController.radius = CharaConRad;
				CharaController.skinWidth = CharaConSkin;

				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = 0.5f;

				//ダメージ用コライダの大きさを戻す
				DamageCol.center = DamageColCenter;
				DamageCol.size = DamageColSize;

				break;

			case "Down":

				//キャラクターコントローラの大きさを小さくする
				CharaController.height = 0.1f;
				CharaController.radius = 0.1f;
				CharaController.center = new Vector3(0, CharaController.radius * 0.5f + CharaController.skinWidth, 0);

				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = 0.1f;

				//ダメージ用コライダの大きさを小さくする
				DamageCol.size = new Vector3(1, 0.5f, 1);
				DamageCol.center = new Vector3(0,DamageCol.size.y * 0.5f,0);				

				break;

			case "Rise":

				//キャラクターコントローラの大きさを小さくする
				CharaController.height = 0.1f;
				CharaController.radius = 0.1f;
				CharaController.center = new Vector3(0, CharaController.radius * 0.5f + CharaController.skinWidth, 0);
				
				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = 0.1f;
				
				//ダメージ用コライダの大きさを戻す
				DamageCol.center = DamageColCenter;
				DamageCol.size = DamageColSize;

				break;

			case "Skin":

				//靴の高さを表すskinWidthを消す
				CharaController.skinWidth = 0.1f;

				break;

			default:
				break;
		}

	}

	//自身がカメラの画角に入っているか返す
	public bool GetOnCameraBool()
	{
		//返り値代入用変数
		float OnCameraTime = 0;

		//retuin用変数
		bool re = true;

		//カメラに映っていた時刻取得
		ExecuteEvents.Execute<OnCameraScriptInterface>(OnCameraObject, null, (reciever, eventData) => OnCameraTime = reciever.GetOnCameraTime());

		//現在の時刻と比較、完全に同時刻にはならないので、処理落ちも考慮してラグを吸収する
		if (Time.time - OnCameraTime > 0.1f * (GameManagerScript.Instance.FrameRate / GameManagerScript.Instance.FPS))
		{
			re = false;
		}

		//出力
		return re;
	}

	//スタンを開始する
	public void StunStart()
	{
		//スタン中は再度入らないようにする
		if(!StunFlag)
		{
			//スタン持続コルーチン呼び出し
			StartCoroutine(StunStartCoroutine());
		}
	}
	IEnumerator StunStartCoroutine()
	{
		//スタンフラグを立てる
		StunFlag = true;

		//気絶値がそのまま持続時間になるので、最低値を底上げ
		if (Stun > -20)
		{
			Stun = -20;
		}

		//気絶持続
		while (Stun < 0 && StunFlag)
		{
			Stun += 10 * Time.deltaTime;

			//1フレーム待機
			yield return null;
		}

		//モーションをアイドリングに戻す
		if(StunFlag)
		{
			float stunblend = 1;

			while (stunblend > 0 && StunFlag)
			{
				stunblend -= Time.deltaTime;

				//スタンドモーションを再生
				CurrentAnimator.SetFloat("StunBlend", stunblend);

				//1フレーム待機
				yield return null;
			}
		}

		//スタンを解除する
		StunEnd();
	}

	//スタンを解除する
	public void StunEnd()
	{
		//スタン値を戻す
		Stun = StunMax;

		//スタンフラグを下ろす
		StunFlag = false;

		//移動値をリセット
		DamageMoveVec *= 0;

		//スタンドモーションを再生
		CurrentAnimator.SetFloat("StunBlend" , 0f);
	}

	//攻撃コライダ移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartAttackCol(int n)
	{
		//攻撃用コライダーのコライダ移動関数呼び出し、インデックスとコライダ移動タイプを渡す
		ExecuteEvents.Execute<EnemyAttackCollInterface>(AttackCol, null, (reciever, eventData) => reciever.ColStart(n, UseArts));
	}

	//攻撃コライダ終了処理、アニメーションクリップのイベントから呼ばれる
	private void EndAttackCol()
	{
		//コライダ移動終了処理関数呼び出し
		ExecuteEvents.Execute<EnemyAttackCollInterface>(AttackCol, null, (reciever, eventData) => reciever.ColEnd());
	}

	//攻撃フラグを下ろす、アニメーションクリップから呼ばれる
	public void AttackEnd()
	{
		CurrentAnimator.SetBool("Attack" , false);
	}

	//アニメーターの攻撃モーションを切り替える、ビヘイビアスクリプトから呼ばれる
	public void SetAttackMotion(string n)
	{
		//使用した攻撃をキャッシュ
		UseArts = AttackClassList.Where(a => a.AttackID.Contains(n)).ToList()[0];

		//使用するモーションに差し替え
		OverRideAnimator["Attack_Void"] = UseArts.Anim;

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//ダメージモーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetDamageAnimList(List<AnimationClip> i)
	{
		//受け取ったListを変数に格納
		DamageAnimList = new List<AnimationClip>(i);
	}

	//攻撃モーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetAttackClassList(List<EnemyAttackClass> i)
	{
		//受け取ったListを変数に格納
		AttackClassList = new List<EnemyAttackClass>(i);
	}

	//自身が死んでるか返す
	public bool GetDestroyFlag()
	{
		return DestroyFlag;
	}

	//ポーズ処理
	public void Pause(bool b)
	{
		//ポーズフラグ引数で受け取ったboolをフラグに代入
		PauseFlag = b;
	}

	//プレイヤーキャラクターをセットする、キャラ交代した時にMissionManagerから呼ばれる
	public void SetPlayerCharacter(GameObject c)
	{
		PlayerCharacter = c;
	}
}


/*
gameObject.GetComponent<CharacterController>().enabled = false;

foreach (Transform i in GetComponentsInChildren<Transform>())
{
	if (i.name.Contains("Bone"))
	{
		i.gameObject.AddComponent<SphereCollider>().radius = 0.05f;
		i.gameObject.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
		i.gameObject.GetComponent<Rigidbody>().drag = 1;
		i.gameObject.GetComponent<Rigidbody>().AddForce(KnockBackVec, ForceMode.Impulse);
	}
}

foreach (Rigidbody i in GetComponentsInChildren<Rigidbody>())
{
	//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
	switch (i.name)
	{
		case "SpineBone.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "PelvisBone").GetComponent<Rigidbody>();
			break;

		case "SpineBone.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "SpineBone.000").GetComponent<Rigidbody>();
			break;

		case "SpineBone.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "SpineBone.001").GetComponent<Rigidbody>();
			break;

		case "NeckBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "SpineBone.002").GetComponent<Rigidbody>();
			break;

		case "HeadBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "NeckBone").GetComponent<Rigidbody>();
			break;

		case "L_ShoulderBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "SpineBone.002").GetComponent<Rigidbody>();
			break;

		case "R_ShoulderBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "SpineBone.002").GetComponent<Rigidbody>();
			break;

		case "L_UpperArmBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_ShoulderBone").GetComponent<Rigidbody>();
			break;

		case "R_UpperArmBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_ShoulderBone").GetComponent<Rigidbody>();
			break;

		case "L_ElbowBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_UpperArmBone").GetComponent<Rigidbody>();
			break;

		case "R_ElbowBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_UpperArmBone").GetComponent<Rigidbody>();
			break;

		case "L_LowerArmBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_ElbowBone").GetComponent<Rigidbody>();
			break;

		case "R_LowerArmBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_ElbowBone").GetComponent<Rigidbody>();
			break;

		case "L_WristBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_LowerArmBone").GetComponent<Rigidbody>();
			break;

		case "R_WristBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_LowerArmBone").GetComponent<Rigidbody>();
			break;

		case "L_HandBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_WristBone").GetComponent<Rigidbody>();
			break;

		case "R_HandBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_WristBone").GetComponent<Rigidbody>();
			break;





		case "L_Thumb.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HandBone").GetComponent<Rigidbody>();
			break;

		case "L_First.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HandBone").GetComponent<Rigidbody>();
			break;

		case "L_Middle.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HandBone").GetComponent<Rigidbody>();
			break;

		case "L_Ring.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HandBone").GetComponent<Rigidbody>();
			break;

		case "L_Pinky.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HandBone").GetComponent<Rigidbody>();
			break;





		case "L_Thumb.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Thumb.000").GetComponent<Rigidbody>();
			break;

		case "L_First.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_First.000").GetComponent<Rigidbody>();
			break;

		case "L_Middle.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Middle.000").GetComponent<Rigidbody>();
			break;

		case "L_Ring.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Ring.000").GetComponent<Rigidbody>();
			break;

		case "L_Pinky.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Pinky.000").GetComponent<Rigidbody>();
			break;




		case "L_Thumb.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Thumb.001").GetComponent<Rigidbody>();
			break;

		case "L_First.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_First.001").GetComponent<Rigidbody>();
			break;

		case "L_Middle.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Middle.001").GetComponent<Rigidbody>();
			break;

		case "L_Ring.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Ring.001").GetComponent<Rigidbody>();
			break;

		case "L_Pinky.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Pinky.001").GetComponent<Rigidbody>();
			break;


		case "L_First.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_First.002").GetComponent<Rigidbody>();
			break;

		case "L_Middle.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Middle.002").GetComponent<Rigidbody>();
			break;

		case "L_Ring.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Ring.002").GetComponent<Rigidbody>();
			break;

		case "L_Pinky.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_Pinky.002").GetComponent<Rigidbody>();
			break;










		case "R_Thumb.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HandBone").GetComponent<Rigidbody>();
			break;

		case "R_First.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HandBone").GetComponent<Rigidbody>();
			break;

		case "R_Middle.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HandBone").GetComponent<Rigidbody>();
			break;

		case "R_Ring.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HandBone").GetComponent<Rigidbody>();
			break;

		case "R_Pinky.000":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HandBone").GetComponent<Rigidbody>();
			break;





		case "R_Thumb.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Thumb.000").GetComponent<Rigidbody>();
			break;

		case "R_First.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_First.000").GetComponent<Rigidbody>();
			break;

		case "R_Middle.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Middle.000").GetComponent<Rigidbody>();
			break;

		case "R_Ring.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Ring.000").GetComponent<Rigidbody>();
			break;

		case "R_Pinky.001":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Pinky.000").GetComponent<Rigidbody>();
			break;




		case "R_Thumb.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Thumb.001").GetComponent<Rigidbody>();
			break;

		case "R_First.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_First.001").GetComponent<Rigidbody>();
			break;

		case "R_Middle.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Middle.001").GetComponent<Rigidbody>();
			break;

		case "R_Ring.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Ring.001").GetComponent<Rigidbody>();
			break;

		case "R_Pinky.002":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Pinky.001").GetComponent<Rigidbody>();
			break;


		case "R_First.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_First.002").GetComponent<Rigidbody>();
			break;

		case "R_Middle.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Middle.002").GetComponent<Rigidbody>();
			break;

		case "R_Ring.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Ring.002").GetComponent<Rigidbody>();
			break;

		case "R_Pinky.003":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_Pinky.002").GetComponent<Rigidbody>();
			break;










		case "L_HipBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "PelvisBone").GetComponent<Rigidbody>();
			break;

		case "R_HipBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "PelvisBone").GetComponent<Rigidbody>();
			break;

		case "L_UpperLegBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_HipBone").GetComponent<Rigidbody>();
			break;

		case "R_UpperLegBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_HipBone").GetComponent<Rigidbody>();
			break;

		case "L_KneeBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_UpperLegBone").GetComponent<Rigidbody>();
			break;

		case "R_KneeBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_UpperLegBone").GetComponent<Rigidbody>();
			break;

		case "L_LowerLegBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_KneeBone").GetComponent<Rigidbody>();
			break;

		case "R_LowerLegBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_KneeBone").GetComponent<Rigidbody>();
			break;

		case "L_FootBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_LowerLegBone").GetComponent<Rigidbody>();
			break;

		case "R_FootBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_LowerLegBone").GetComponent<Rigidbody>();
			break;

		case "L_ToeBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "L_FootBone").GetComponent<Rigidbody>();
			break;

		case "R_ToeBone":
			i.gameObject.AddComponent<CharacterJoint>().connectedBody = DeepFind(gameObject, "R_FootBone").GetComponent<Rigidbody>();
			break;

	}
}

foreach(CharacterJoint i in gameObject.GetComponentsInChildren<CharacterJoint>())
{
	SoftJointLimit tempjoint = new SoftJointLimit();

	tempjoint.limit = 1f;

	i.swing1Limit = tempjoint;
	i.swing2Limit = tempjoint;
	i.highTwistLimit = tempjoint;
	i.lowTwistLimit = tempjoint;
}*/

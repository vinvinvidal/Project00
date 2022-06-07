using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface EnemyCharacterInterface : IEventSystemHandler
{
	//行動Listを受け取る
	void SetBehaviorList(List<EnemyBehaviorClass> i);

	//ダメージモーションListを受け取る、セッティングスクリプトから呼ばれる
	void SetDamageAnimList(List<AnimationClip> i);

	//スケベヒットモーションListを受け取る、セッティングスクリプトから呼ばれる
	void SetH_HitAnimList(List<AnimationClip> i);

	//スケベヒットモーションListを受け取る、セッティングスクリプトから呼ばれる
	void SetH_AttackAnimList(List<AnimationClip> i);

	//スケベブレイクモーションListを受け取る、セッティングスクリプトから呼ばれる
	void SetH_BreakAnimList(List<AnimationClip> i);

	//攻撃情報Listを受け取る、セッティングスクリプトから呼ばれる
	void SetAttackClassList(List<EnemyAttackClass> i);

	//アニメーターの攻撃モーションを切り替える、ビヘイビアスクリプトから呼ばれる
	void SetAttackMotion(string n);

	//戦闘開始処理
	void BattleStart();

	//プレイヤーからの攻撃を受けた時の処理
	void PlayerAttackHit(ArtsClass i, int c);

	//超必殺技を受けた時の処理
	void SuperArtsHit(int n, int dwn);

	//スケベ攻撃が当たった時に呼ばれる
	void H_AttackHit(string ang , int men, GameObject Player);

	//スケベ攻撃遷移関数
	void H_Transition(string Act);

	//元のスケベステートに戻る関数
	void H_ReturnState();

	//スケベ攻撃が解除された時に呼ばれる
	void H_Break(string location);

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

	//当たった超必殺技が有効かを返す
	bool SuperEnable(int n);

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
	public float Life { get; set; }

	//気絶値
	public float Stun { get; set; }

	//興奮度
	public float Excite { get; set; } = 0.3f;

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

	//触れた他の敵
	private GameObject HitEnemy;

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
	public string CurrentState { get; set; }

	//全ての行動List
	public List<EnemyBehaviorClass> BehaviorList { get; set; }

	//全ての攻撃情報List
	public List<EnemyAttackClass> AttackClassList { get; set; }

	//全てのダメージモーションList
	public List<AnimationClip> DamageAnimList { get; set; }

	//全てのスケベヒットモーションList
	public List<AnimationClip> H_HitAnimList { get; set; }

	//全てのスケベアタックモーションList
	public List<AnimationClip> H_AttackAnimList { get; set; }

	//全てのスケベブレイクモーションList
	public List<AnimationClip> H_BreakAnimList { get; set; }

	//ダメージモーションを制御するステート
	private int DamageState = 0;

	//スケベモーションを制御するステート
	private int H_State = 0;

	//現在のスケベ状況
	private string H_Location;

	//全ての移動値を合算した移動ベクトル
	private Vector3 MoveMoment;

	//体を向けるベクトル
	public Vector3 RotateVec { get; set; }

	//ビヘイビアから設定される回転値
	public Vector3 BehaviorRotate { get; set; }

	//イベント中の回転値
	private Vector3 EventRotate = Vector3.zero;

	//ノックバックの移動ベクトル
	private Vector3 KnockBackVec;

	//ダメージモーション中の移動ベクトル
	private Vector3 DamageMoveVec;

	//ホールド中の移動ベクトル
	private Vector3 HoldVec;

	//超必殺技中の移動ベクトル
	public Vector3 SuperMoveVec { get; set; }

	//特殊攻撃中の移動ベクトル
	public Vector3 SpecialMoveVec { get; set; }

	//行動中の移動ベクトル
	public Vector3 BehaviorMoveVec { get; set; }

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

	//歩調に合わせるサインカーブ生成に使う数
	public float SinCount { get; set; } = 0;

	//接地フラグ
	public bool OnGround { get; set; }

	//ノックバックフラグ
	public bool KnockBackFlag { get; set; }

	//ダメージモーション中フラグ
	public bool DamageFlag { get; set; }

	//キャラクターに触れているフラグ
	public bool ContactCharacterFlag { get; set; }

	//スケベフラグ
	public bool H_Flag { get; set; }

	//スタン状態フラグ
	public bool StunFlag { get; set; }

	//ダウン状態フラグ
	public bool DownFlag { get; set; }

	//ホールドダメージ状態フラグ
	public bool HoldFlag { get; set; }

	//超必殺技ダメージ状態フラグ
	public bool SuperFlag { get; set; }

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

	//行動中フラグ
	public bool BehaviorFlag { get; set; } = false;

	//戦闘中フラグ
	public bool BattleFlag { get; set; } = false;

	//ダウンしない攻撃List
	private List<int> NotDownAttackList;

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

	//足元の衝撃エフェクト
	private GameObject FootImpactEffect;

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

		//スケベフラグ初期化
		H_Flag = false;

		//スタン状態フラグ初期化
		StunFlag = false;

		//ダウン状態フラグ初期化
		DownFlag = false;

		//ホールドダメージ状態フラグ初期化
		HoldFlag = false;

		//超必殺技ダメージ状態フラグ初期化
		SuperFlag = false;

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
		DamageCol = DeepFind(gameObject, "EnemyDamageCol").GetComponent<BoxCollider>();

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

		//強制移動ベクトル初期化
		ForceMoveVector = Vector3.zero;

		//ホールド中の移動ベクトル初期化
		HoldVec = Vector3.zero;

		//超必殺技中の移動ベクトル初期化
		SuperMoveVec = Vector3.zero;

		//行動中の移動ベクトル初期化
		BehaviorMoveVec = Vector3.zero;

		//地面との距離初期化
		GroundDistance = 0.0f;

		//キャラクターの接地判定をするレイの発射位置、キャラクターコントローラから算出
		//RayPoint = new Vector3(0, CharaController.height, CharaController.center.z);
		RayPoint = new Vector3(0, 2, 0);

		//キャラクターの接地判定をするレイの大きさ、キャラクターコントローラから算出
		//RayRadius = new Vector3(CharaController.radius, CharaController.height * 0.5f, CharaController.radius);
		RayRadius = 1f;

		//重力加速度初期化
		Gravity = Physics.gravity.y * Time.deltaTime;

		//重力補正値初期化
		GraityCorrect = 0;

		//ダウン時間初期化
		DownTime = 0;



		//ダウンしない攻撃List初期化
		NotDownAttackList = new List<int>();

		NotDownAttackList.Add(0);
		NotDownAttackList.Add(3);
		NotDownAttackList.Add(9);



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



		//足元の衝撃エフェクト取得
		FootImpactEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "FootImpact").ToArray()[0];

		//全てのステート名を手動でAdd、アニメーターのステート名は外部から取れない
		AllStates.Add("AnyState");
		AllStates.Add("Idling");
		AllStates.Add("Walk");
		AllStates.Add("Run");
		AllStates.Add("Attack");
		AllStates.Add("H_Try");
		AllStates.Add("H_Hit");
		AllStates.Add("H_Attack00");
		AllStates.Add("H_Attack01");
		AllStates.Add("H_Break");
		AllStates.Add("DownLanding");
		AllStates.Add("Damage00");
		AllStates.Add("Damage01");
		AllStates.Add("Special");
		AllStates.Add("HoldDamage");
		AllStates.Add("Down_Prone");
		AllStates.Add("Down_Supine");
		AllStates.Add("GetUp_Prone");
		AllStates.Add("GetUp_Supine");
		AllStates.Add("SuperDamage");

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

		//アイドリングモーションを変更
		CurrentAnimator.SetFloat("IdlingBlend", Random.Range(2, 4));

		//アニメーター停止
		CurrentAnimator.speed = 0;
	}

	void LateUpdate()
	{
		if (!H_Flag)
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
					else if (HoldFlag || BlownFlag)
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
	}

	void Update()
	{
		if (!PauseFlag)
		{
			//アニメーションステートを監視する関数呼び出し
			StateMonitor();

			//接地判定用のRayを飛ばす関数呼び出し
			GroundRayCast();

			//他キャラめり込み防止関数呼び出し
			EnemyAround();

			//移動制御関数呼び出し
			MoveFunc();

			//戦闘中だけ行動抽選
			if (BattleFlag)
			{
				//行動抽選関数呼び出し
				BehaviorFunc();
			}
		}
	}

	//状態フラグを全て下す関数
	private void FlagReset()
	{
		DownFlag = false;

		SpecialFlag = false;

		HoldFlag = false;

		KnockBackFlag = false;

		DamageFlag = false;

		SuperFlag = false;

		H_Flag = false;

		BehaviorFlag = false;
	}

	//移動制御関数
	void MoveFunc()
	{
		//歩調に合わせるサインカーブ生成に使う数カウントアップ
		SinCount += Time.deltaTime;

		//移動値初期化
		MoveMoment *= 0;

		//回転値初期化
		RotateVec *= 0;

		//イベント中
		if(GameManagerScript.Instance.EventFlag)
		{
			//回転値
			RotateVec = EventRotate;
		}
		//ダウン状態
		else if (DownFlag)
		{
			MoveMoment *= 0;
		}
		//特殊攻撃を受けている
		else if (SpecialFlag)
		{
			MoveMoment = SpecialMoveVec * Time.deltaTime;
		}
		//ホールドダメージ状態
		else if (HoldFlag)
		{
			MoveMoment = HoldVec * Time.deltaTime;
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
		//超必殺技中の移動
		else if (SuperFlag)
		{
			MoveMoment = SuperMoveVec * Time.deltaTime;
		}
		//スケベ中の移動
		else if (H_Flag)
		{
			if (H_Location.Contains("Back"))
			{
				//プレイヤーと位置を合わせる
				MoveMoment = ((PlayerCharacter.transform.position - (PlayerCharacter.transform.forward * 0.25f)) - gameObject.transform.position) * Time.deltaTime * 20;
			}
			else
			{
				//プレイヤーと位置を合わせる
				MoveMoment = ((PlayerCharacter.transform.position - (PlayerCharacter.transform.forward * -0.25f)) - gameObject.transform.position) * Time.deltaTime * 20;
			}

			//プレイヤーと高さを合わせる
			MoveMoment.y = (PlayerCharacter.transform.position - gameObject.transform.position).y * Time.deltaTime * 20;
		}
		//行動中の移動
		else if (BehaviorFlag)
		{
			//移動値
			MoveMoment = BehaviorMoveVec * MoveSpeed * Time.deltaTime;

			//回転値
			RotateVec = BehaviorRotate;
		}
		
		//条件で重力加速度を増減させる
		if (H_Flag)
		{
			//重力を打ち消す
			GraityCorrect = -Gravity;
		}
		else if ((!OnGround && (PlayerCharacter.transform.position - transform.position).sqrMagnitude > Mathf.Pow(3, 2)) || CurrentState.Contains("DownLanding"))
		{
			Gravity += Physics.gravity.y * 2 * Time.deltaTime;

			GraityCorrect = 0;
		}
		else if (!OnGround && !DownFlag)
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

		//回転値が入っていたら回転
		if (RotateVec != Vector3.zero)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(RotateVec), TurnSpeed * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.75f * SinCount) + 0.1f) * Time.deltaTime);
		}

		//移動
		CharaController.Move(MoveMoment);
	}

	//行動抽選関数
	private void BehaviorFunc()
	{
		//行動中ではなく、アイドリング中
		if (!BehaviorFlag && CurrentState == "Idling")
		{
			//開始可能な行動を抽出
			List<EnemyBehaviorClass> TempBehavioerList = new List<EnemyBehaviorClass>(BehaviorList.Where(b => b.BehaviorConditions()).ToList());

			//行動比率
			int BehaviorRatio = 0;

			//抽選番号
			int BehaviorLottery = 0;

			//開始可能な行動がある
			if (TempBehavioerList.Count > 0)
			{
				//行動比率合計
				foreach (EnemyBehaviorClass i in TempBehavioerList)
				{
					if (i.Name.Contains("H_"))
					{
						//スケベ攻撃だったら興奮度を加味して発生比率を加算
						BehaviorRatio += i.Priority * (int)(Excite * 10);
					}
					else
					{
						//発生比率を加算
						BehaviorRatio += i.Priority;
					}
				}

				//比率の合計値から抽選番号設定
				BehaviorLottery = UnityEngine.Random.Range(1, BehaviorRatio + 1);

				//行動比率初期化
				BehaviorRatio = 0;

				//行動抽選
				foreach (EnemyBehaviorClass i in TempBehavioerList)
				{
					//比率を合計していく
					if (i.Name.Contains("H_"))
					{
						//スケベ攻撃だったら興奮度を加味して発生比率を加算
						BehaviorRatio += i.Priority * (int)(Excite * 10);
					}
					else
					{
						//発生比率を加算
						BehaviorRatio += i.Priority;
					}

					//乱数がどの範囲にあるか判定、当選した行動を実行
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
	//他キャラめり込み防止関数
	private void EnemyAround()
	{
		//強制移動ベクトル初期化
		ForceMoveVector *= 0;

		//スケベ、もしくは超必殺技中じゃない
		if (!H_Flag && !SuperFlag)
		{
			//他の敵を避ける処理
			if (HitEnemy != null)
			{
				//水平方向に３メートル以上離れている
				if (HorizontalVector(gameObject, HitEnemy).sqrMagnitude > 3)
				{
					HitEnemy = null;
				}
				//上にいる時に動かす
				else if (gameObject.transform.position.y > HitEnemy.gameObject.transform.position.y)
				{
					ForceMoveVector = HorizontalVector(gameObject, HitEnemy).normalized;

					CharaControllerReset("Rise");
				}
			}
			//プレイヤーを避ける処理
			if (!HoldFlag && !SpecialFlag && !DownFlag)
			{
				if (HorizontalVector(gameObject, PlayerCharacter).sqrMagnitude < 1f && gameObject.transform.position.y - PlayerCharacter.transform.position.y < 1f && gameObject.transform.position.y - PlayerCharacter.transform.position.y > -0.1f)
				{
					//敵と自分までのベクトルで強制移動
					ForceMoveVector += HorizontalVector(transform.gameObject, PlayerCharacter);
				}
			}
		}
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

	//当たった超必殺技が有効かを返す
	public bool SuperEnable(int n)
	{
		//出力用変数宣言
		bool re = true;

		//状況判定
		switch (n)
		{
			//地上で立ち
			case 0:
				re =
					OnGround &&
					!DownFlag;
				break;
			default:
				break;
		}

		//出力
		return re;
	}

	//当たった攻撃が有効かを返す関数
	public bool AttackEnable(ArtsClass Arts, int n)
	{
		//出力用変数宣言
		bool re = true;

		//ヒット条件を満たしているか判定
		if(
			//死んでる
			DestroyFlag
			||
			//ダウン中にダウンに当たらない攻撃が当たった
			(DownFlag && Arts.DownEnable[n] != 1)
			||
			//ホールド状態じゃないのに、ホールド追撃専用技が当たった
			(!HoldFlag && Arts.ColType[n] == 6)
			||
			//地上限定の技が空中で当たった
			(!OnGround && Arts.ColType[n] == 8)
			||
			//ダウン中にダウン専用攻撃が当たった
			(DownFlag && Arts.ColType[n] == 9)
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
		//喰らった技のダメージを受け取る
		float Damage = Arts.Damage[n];

		//プレイヤーが武器をストックしている
		if (PlayerCharacter.GetComponent<SpecialArtsScript>().StockWeapon != null && Arts.NameC != "")
		{			
			//ストック武器のダケージを加算
			Damage += PlayerCharacter.GetComponent<SpecialArtsScript>().StockWeapon.GetComponent<ThrowWeaponScript>().UseArts.PlyaerUseDamage * 0.1f;
		}

		//ライフを減らす
		Life -= Damage;

		//タメ攻撃の係数を掛ける
		Life -= Arts.ChargeDamage[n] * PlayerCharacter.GetComponent<PlayerScript>().ChargeLevel;

		//ライフが無くなった
		if (Life < 0)
		{
			//致命不可だったらライフをゼロに保つ
			if (Arts.Deadly[n] == 0)
			{
				Life = 0;
			}
			else
			{
				//死亡フラグを立てる
				DestroyFlag = true;

				//コライダ無効化
				DamageCol.enabled = false;

				//ゲームマネージャーのListから自身を削除
				ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.RemoveAllActiveEnemyList(ListIndex));

				//オブジェクト削除コルーチン呼び出し
				StartCoroutine(VanishCoroutine());
			}
		}

		//攻撃用コライダを無効化
		EndAttackCol();

		//これ以上イベントを起こさないために攻撃ステートを一時停止
		CurrentAnimator.SetFloat("AttackSpeed", 0.0f);

		//アニメーターの攻撃フラグを下ろす
		CurrentAnimator.SetBool("Attack", false);

		//ホールドフラグを下ろす
		HoldFlag = false;

		//スケベフラグを下す
		H_Flag = false;

		//気絶値を減らす
		Stun -= Arts.Stun[n];

		//ダメージモーション管理関数呼び出し
		DamageMotionFunc(Arts, n);
	}

	//超必殺技を受けた時の処理
	public void SuperArtsHit(int n, int dwn)
	{
		//遷移フラグを立てる
		CurrentAnimator.SetBool("SuperDamage", true);

		//引数で技後のダウンフラグを立てる関数呼び出し
		SetDownFlag(dwn);

		//プレイヤーに向ける
		transform.LookAt(HorizontalVector(PlayerCharacter, gameObject));

		//状態フラグをリセット
		FlagReset();

		//超必殺技フラグを立てる
		SuperFlag = true;

		//使用するモーションに差し替え
		OverRideAnimator["Super_void"] = DamageAnimList[50 + n];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//引数で技後のダウンフラグを立てる
	public void SetDownFlag(int dwn)
	{
		//引数で技後のダウンフラグを立てる
		if (dwn == 0)
		{
			CurrentAnimator.SetBool("Down_Prone", true);
			CurrentAnimator.SetBool("Down_Supine", false);
		}
		else
		{
			CurrentAnimator.SetBool("Down_Supine", true);
			CurrentAnimator.SetBool("Down_Prone", false);
		}
	}

	//ダメージモーション管理関数
	private void DamageMotionFunc(ArtsClass Arts, int n)
	{
		//ダメージステートカウントアップ
		DamageState++;

		//現在のアニメーターの遷移フラグを立てる
		CurrentAnimator.SetBool("Damage0" + DamageState % 2, true);

		//次の遷移フラグを下ろす
		CurrentAnimator.SetBool("Damage0" + (DamageState + 1) % 2, false);

		//使用するダメージステートのスピードを戻す
		CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 1);

		//これ以上イベントを起こさないために使わないダメージステートを一時停止
		CurrentAnimator.SetFloat("DamageMotionSpeed" + (DamageState + 1) % 2, 0);

		//ちゃんと技がある
		if (Arts != null)
		{
			//再生するモーションを食らった技や状況で切り替えるインデックスをキャッシュ
			int UseIndex = Arts.AttackType[n];

			//死んでるけどダウンしない攻撃を喰らった
			if (DestroyFlag && NotDownAttackList.Any(a => a == Arts.AttackType[n]))
			{
				//ダウンモーションに切り替え
				UseIndex = 1;
			}
			//打ち上げられている状態で食らった
			else if (RiseFlag && !RiseEnableAttakList.Any(a => a == Arts.AttackType[n]))
			{
				//打ち上げモーションに切り替え
				UseIndex = 11;
			}
			//ダウンしている状態でダウンに当たる攻撃が当たった
			else if (DownFlag && !DownEnableAttakList.Any(a => a == Arts.AttackType[n]))
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

			//ホールド攻撃処理
			if (Arts.AttackType[n] == 30)
			{
				//状態フラグをリセット
				FlagReset();

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

				//アニメーターのダウン着地フラグを下ろす
				CurrentAnimator.SetBool("DownLanding", false);

				//アニメーターのダウンフラグを下ろす
				CurrentAnimator.SetBool("Down_Supine", false);

				//アニメーターのダウンフラグを下ろす
				CurrentAnimator.SetBool("Down_Prone", false);

				//ホールド持続コルーチン呼び出し
				StartCoroutine(HoldWaitCoroutine(Arts.HoldPosList[n]));
			}
			else
			{
				//ノックバックコルーチン呼び出し
				StartCoroutine(DamageKnockBack(Arts, n));
			}
		}
		//技無しでインデックスが0なら壁激突
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

	//ダメージ状態のフラグを切り替える、アニメーションクリップから呼ばれる
	public void DamageStateFlag(int state)
	{
		//アニメーターのダウン着地フラグを下ろす
		CurrentAnimator.SetBool("DownLanding", false);

		//アニメーターのダウンフラグを下ろす
		CurrentAnimator.SetBool("Down_Supine", false);

		//アニメーターのダウンフラグを下ろす
		CurrentAnimator.SetBool("Down_Prone", false);

		//ダウンしない攻撃遷移判定
		if (state == 0)
		{
			//特に何もしない
		}
		//うつ伏せダウン遷移判定
		else if (state == 1)
		{
			//アニメーターのダウンフラグを立てる
			CurrentAnimator.SetBool("Down_Prone", true);
		}
		//吹っ飛びうつ伏せダウン遷移判定
		else if (state == 2)
		{
			//吹っ飛びフラグを立てる
			BlownFlag = true;

			//アニメーターのダウンフラグを立てる
			CurrentAnimator.SetBool("Down_Prone", true);

			//アニメーションの再生速度を落とす
			CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 0.05f);

			//キャラクターの方を向く
			transform.rotation = Quaternion.LookRotation(HorizontalVector(PlayerCharacter, gameObject));

			//ちょっと浮かす
			CharaController.Move(Vector3.up * Time.deltaTime * 3);
		}
		//仰向けダウン遷移判定
		else if (state == 3)
		{
			//アニメーターのフラグを立てる
			CurrentAnimator.SetBool("Down_Supine", true);
		}
		//打ち上げ判定
		else if (state == 4)
		{
			//打ち上げフラグを立てる
			RiseFlag = true;

			//重力をリセット
			Gravity = 0;

			//アニメーターのダウンフラグを立てる
			CurrentAnimator.SetBool("Down_Supine", true);

			//キャラクターコントローラの大きさを変える
			CharaControllerReset("Rise");

			//打ち上げてから少し待ってからフラグを立てるコルーチン呼び出し
			StartCoroutine(DownLandingFlagCoroutine());
		}
		//香港スピン
		else if (state == 5)
		{
			//すぐにダウン状態にする
			DownFlag = true;

			//ダウン制御コルーチン呼び出し
			StartCoroutine(DownCoroutine());

			//重力をリセット
			Gravity = 0;

			//アニメーターのダウンフラグを立てる
			CurrentAnimator.SetBool("Down_Prone", true);

			//キャラクターコントローラの大きさを変える
			CharaControllerReset("Down");
		}

		//ノックバックフラグを入れる
		KnockBackFlag = true;
	}

	//打ち上げてから少し待ってからフラグを立てる
	IEnumerator DownLandingFlagCoroutine()
	{
		//現在時刻をキャッシュ
		float temptime = Time.time;

		//チョイ待つ
		while (Time.time - temptime < 0.25f)
		{
			if (PauseFlag)
			{
				//ポーズ中はキャッシュを更新
				temptime += Time.deltaTime;
			}

			//1フレーム待機
			yield return null;
		}

		//アニメーターのダウン着地フラグを立てる
		CurrentAnimator.SetBool("DownLanding", true);
	}

	//ノックバック処理
	IEnumerator DamageKnockBack(ArtsClass Arts, int n)
	{
		//1フレーム待機
		yield return null;

		//ノックバックフラグが立つまで待機
		while (PauseFlag || !KnockBackFlag)
		{
			//1フレーム待機
			yield return null;
		}

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
		else if (!RiseFlag && Arts.AttackType[n] == 20)
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
		if (Arts.ColType[n] != 7 && Arts.ColType[n] != 8 && Arts.ColType[n] != 4 && Arts.ColType[n] != 5)
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
		while (KnockBackFlag)
		{
			//経過時間更新
			t += Time.deltaTime;

			//ポーズ中は持続時間更新
			if (PauseFlag)
			{
				tempTime += Time.deltaTime;
			}

			//壁に当たったらブレイク
			if (WallClashFlag)
			{
				//壁激突フラグを下す
				WallClashFlag = false;

				//ループを抜ける
				break;
			}
			else if (t > tempTime && OnGround)
			{
				//ループを抜ける
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//ノックバックフラグを切る
		KnockBackFlag = false;

		//吹っ飛び中なら
		if (BlownFlag)
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
		
		//コルーチンを抜ける
		yield break;
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

				//行動フラグを下ろす
				BehaviorFlag = false;

				//ダメージモーション依存の移動値を初期化
				DamageMoveVec *= 0;

				//ダメージ遷移フラグを下ろす
				CurrentAnimator.SetBool("Damage00", false);
				CurrentAnimator.SetBool("Damage01", false);

				//少しの間ダメージコライダ無効化
				StartCoroutine(DamageColEnable());
			}
			//ダウン着地になった瞬間の処理
			else if (CurrentState == "DownLanding")
			{
				//打ち上げフラグを下ろす
				RiseFlag = false;

				//アニメーターのダウン着地フラグを下ろす
				CurrentAnimator.SetBool("DownLanding", false);

				//ダウン継続中じゃない
				if (!DownFlag)
				{
					//ダウン制御コルーチン呼び出し
					StartCoroutine(DownCoroutine());
				}
			}
			//ダウンになった瞬間の処理
			else if (CurrentState.Contains("Down"))
			{
				//打ち上げフラグを下ろす
				RiseFlag = false;

				//ダウン継続中じゃない
				if (!DownFlag)
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
			//攻撃になった瞬間の処理
			else if (CurrentState.Contains("-> Attack"))
			{
				//アニメーターの攻撃フラグを下ろす
				CurrentAnimator.SetBool("Attack", false);
			}
			//スケベ攻撃攻撃になった瞬間の処理
			else if (CurrentState.Contains("-> H_Try"))
			{
				//アニメーターのスケベ攻撃フラグを下ろす
				CurrentAnimator.SetBool("H_Try", false);
			}
			//スケベ攻撃が当たった瞬間の処理
			else if (CurrentState.Contains("-> H_Hit"))
			{
				//アニメーターのスケベ攻撃ヒットフラグを下ろす
				CurrentAnimator.SetBool("H_Hit", false);
			}
			//スケベ攻撃になった瞬間の処理
			else if (CurrentState.Contains("-> H_Attack"))
			{
				//アニメーターのスケベ攻撃フラグを下ろす
				CurrentAnimator.SetBool("H_Attack00", false);
				CurrentAnimator.SetBool("H_Attack01", false);

				//スケベステートカウントアップ
				H_State++;
			}
			//スケベ攻撃が解除された瞬間の処理
			else if (CurrentState.Contains("-> H_Break"))
			{
				//アニメーターのスケベ解除フラグを下ろす
				//CurrentAnimator.SetBool("H_Break", false);

				//行動フラグを下す
				BehaviorFlag = false;

				//キャラクターコントローラを設定
				CharaControllerReset("Reset");
			}
			//ホールドになった瞬間の処理
			else if (CurrentState == "HoldDamage")
			{
				//アニメーターのホールドダメージフラグを下ろす
				CurrentAnimator.SetBool("HoldDamage", false);

				//打ち上げフラグを下ろす
				RiseFlag = false;

				//行動フラグを下ろす
				BehaviorFlag = false;

				//ホールドベクトルを初期化
				HoldVec *= 0;
			}
			//特殊攻撃を喰らった瞬間の処理
			else if (CurrentState == "Special")
			{
				//遷移フラグを下ろす
				CurrentAnimator.SetBool("Special", false);

				//行動フラグを下ろす
				BehaviorFlag = false;
			}
			//超必殺技を喰らった瞬間の処理
			else if (CurrentState == "SuperDamage")
			{
				//遷移フラグを下ろす
				CurrentAnimator.SetBool("SuperDamage", false);

				//行動フラグを下ろす
				BehaviorFlag = false;

				//移動ベクトルを初期化
				SuperMoveVec *= 0;
			}

			//スケベ解除から遷移する瞬間の処理
			if (CurrentState.Contains("H_Break ->"))
			{
				//スケベフラグを下す
				H_Flag = false;

				//アニメーターのスケベ解除フラグを下ろす
				CurrentAnimator.SetBool("H_Break", false);
			}
			//超必殺技から遷移する瞬間の処理
			else if (CurrentState.Contains("SuperDamage ->"))
			{
				//超必殺技フラグを下す
				SuperFlag = false;
			}
			//ダウン状態から抜けた瞬間の処理
			else if (CurrentState.Contains("Down_Prone ->"))
			{
				//ダウンフラグを下す
				DownFlag = false;

				//アニメーターのダウンフラグを下す
				CurrentAnimator.SetBool("Down_Prone", false);
			}
			//ダウン状態から抜けた瞬間の処理
			else if (CurrentState.Contains("Down_Supine ->"))
			{
				//ダウンフラグを下す
				DownFlag = false;

				//アニメーターのダウンフラグを下す
				CurrentAnimator.SetBool("Down_Supine", false);
			}
			//ホールド状態から抜けた瞬間の処理
			else if (CurrentState.Contains("HoldDamage ->"))
			{
				//ホールドダメージ状態を下す
				HoldFlag = false;
			}
		}

		//スケベ攻撃中の処理
		if (CurrentState.Contains("H_Attack"))
		{
			//アニメーション再生速度にノイズを加える
			CurrentAnimator.SetFloat("H_Speed", Mathf.PerlinNoise(Time.time * 2.5f, -Time.time) + 0.5f);
		}
		//歩き中の処理
		else if (CurrentState.Contains("Walk"))
		{
			//サインカーブで歩行アニメーションと移動値を合わせる
			BehaviorMoveVec *= Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.75f * SinCount));

			//移動方向で歩行アニメーションをブレンド
			if (MoveMoment != Vector3.zero || RotateVec != Vector3.zero)
			{
				CurrentAnimator.SetFloat("Side_Walk", Mathf.Lerp(CurrentAnimator.GetFloat("Side_Walk"), (Vector3.Angle(transform.right, MoveMoment.normalized) - 90) / 90, 0.25f));
			}
		}
		//走り中の処理
		else if (CurrentState == "Run")
		{
			//サインカーブで歩行アニメーションと移動値を合わせる
			BehaviorMoveVec *= (Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 1.5f * SinCount)) * 2) + 2.5f;
		}
	}

	//現在のモーションを引数で指定されたフレームまでジャンプさせる、アニメーションクリップから呼ばれる
	public void JampMotionFrame(int t)
	{
		CurrentAnimator.Play(CurrentState, 0, t / (CurrentAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length * GameManagerScript.Instance.FrameRate));
	}

	//ダウン制御コルーチン
	IEnumerator DownCoroutine()
	{
		//ダウンフラグを立てる
		DownFlag = true;

		//ダウンタイム設定
		DownTime = 2;

		//キャラクターコントローラの大きさを変える
		CharaControllerReset("Down");

		//ダウンしている、打ち上げられてない、ホールドされてない、ダウンタイムがある
		while (DownFlag && !RiseFlag && !HoldFlag && DownTime > 0)
		{
			//ダウン時間カウントダウン
			if (!PauseFlag && !DestroyFlag)
			{
				DownTime -= Time.deltaTime;
			}

			//1フレーム待機
			yield return null;
		}

		//ダウンフラグを下す
		DownFlag = false;

		//ダウンから打ち上げならダウンフラグは下さない
		if (!RiseFlag)
		{
			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Prone", false);

			//アニメーターのダウンフラグを下す
			CurrentAnimator.SetBool("Down_Supine", false);
		}
	}

	//ホールド持続コルーチン
	IEnumerator HoldWaitCoroutine(Vector3 pos)
	{
		//変数宣言
		Vector3 HoldPos = Vector3.zero;

		//ホールド持続待機
		while (HoldFlag)
		{
			if(pos != Vector3.zero)
			{ 
				//ホールドポジション更新
				HoldPos = (PlayerCharacter.transform.right * pos.x) + (PlayerCharacter.transform.up * pos.y) + (PlayerCharacter.transform.forward * pos.z);

				//ちょっとゆらす
				HoldPos += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

				//ホールドベクトルを設定
				HoldVec = ((PlayerCharacter.transform.position + HoldPos) - gameObject.transform.position) * 20;
			}

			//1フレーム待機
			yield return null;
		}

		if (CurrentState == "HoldDamage")
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
		if (HoldFlag)
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

		//状態フラグをリセット
		FlagReset();

		//フラグを立てる
		DamageFlag = true;

		//経過時間待機
		while (BreakTime + t > Time.time)
		{
			//ポーズ中は待機時間更新
			if (PauseFlag)
			{
				BreakTime += Time.deltaTime;
			}

			//後ろ下げ
			DamageMoveVec = (-transform.forward * 2.5f) + Vector3.down;

			//1フレーム待機
			yield return null;
		}

		//フラグを下ろす
		DamageFlag = false;

		//ダメージモーション中の移動ベクトル初期化
		DamageMoveVec *= 0;

		//死んでたらダウン
		if(DestroyFlag)
		{
			//ダメージステートカウントアップ
			DamageState++;

			//現在のアニメーターの遷移フラグを立てる
			CurrentAnimator.SetBool("Damage0" + DamageState % 2, true);

			//次の遷移フラグを下ろす
			CurrentAnimator.SetBool("Damage0" + (DamageState + 1) % 2, false);

			//使用するダメージステートのスピードを戻す
			CurrentAnimator.SetFloat("DamageMotionSpeed" + DamageState % 2, 1);

			//これ以上イベントを起こさないために使わないダメージステートを一時停止
			CurrentAnimator.SetFloat("DamageMotionSpeed" + (DamageState + 1) % 2, 0);

			//使用するモーションに差し替え
			OverRideAnimator["Damage_Void_" + DamageState % 2] = DamageAnimList[1];

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
		}
	}

	//行動中に移動させる、前後だけ、アニメーションクリップから呼ばれる
	public void StartBehaviorMove(float f)
	{
		BehaviorMoveVec = transform.forward * f;
	}
	//行動中の移動を終わる、アニメーションクリップから呼ばれる
	public void EndBehaviorMove()
	{
		BehaviorMoveVec *= 0;
	}

	//プレイヤーを正面に捉える、アニメーションクリップから呼ばれる
	public void SearchPlayer(float t)
	{
		//コルーチン呼び出し
		StartCoroutine(SearchPlayerCoroutine(t));
	}
	private IEnumerator SearchPlayerCoroutine(float t)
	{
		//経過時間
		float WaitTime = 0;

		//待機時間経過までループ
		while (WaitTime < t)
		{
			//経過時間カウントアップ
			WaitTime += Time.deltaTime;

			//プレイヤーキャラクターに向ける
			BehaviorRotate = HorizontalVector(PlayerCharacter, gameObject);

			//行動不能になったらブレイク
			if (!BehaviorFlag)
			{
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//回転値リセット
		BehaviorRotate *= 0;
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
		if(!OnGround)
		{
			StartCoroutine(FallWaitCoroutine());
		}		
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

		//架空の技を渡して技が当たった事にする
		PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 0, 0.1f) }, new List<float>() { 10 }, new List<int>() { 1 }, new List<int>() { 6 }, new List<int>() { 0 }, new List<int>() { 0 }), 0);

		//エフェクトのインスタンスを生成
		GameObject TempAttackEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect" + PlayerCharacter.GetComponent<CharacterSettingScript>().ID + "1").ToArray()[0]);

		//自身の子にする
		TempAttackEffect.transform.parent = gameObject.transform;

		//位置を設定
		TempAttackEffect.transform.localPosition = new Vector3(0, 0, 0);

		//回転値を設定
		TempAttackEffect.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
	}

	//キャラクターコントローラコライダヒット
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//吹っ飛ばし攻撃で壁に叩きつけられた
		if (BlownFlag && KnockBackFlag && Vector3.Dot(hit.normal.normalized, KnockBackVec.normalized) < -0.8f && GroundDistance < 1.5f)
		{
			//壁に合わせて回転、水平を保つ為にyは回さない
			transform.LookAt(new Vector3(hit.normal.x, 0, hit.normal.z) + transform.position);

			//モーションを再生スピードをリセット
			CurrentAnimator.SetFloat("DamageMotionSpeed0", 1);
			CurrentAnimator.SetFloat("DamageMotionSpeed1", 1);

			//壁当たりモーション再生
			DamageMotionFunc(null, 0);
		}
		else if (LayerMask.LayerToName(hit.gameObject.layer) == "Enemy" && gameObject.transform.position.y > hit.gameObject.transform.position.y)
		{
			HitEnemy = hit.gameObject;
		}
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

		//ダメージフラグを下ろす
		DamageFlag = false;

		//ノックバックフラグを下ろす
		KnockBackFlag = false;

		//スケベフラグを下す
		H_Flag = false;

		//吹っ飛びフラグを下す
		BlownFlag = false;

		//打ち上げフラグを下ろす
		RiseFlag = false;

		//壁激突フラグを下す
		WallClashFlag = false;

		//特殊攻撃フラグを下す
		SpecialFlag = false;

		//アニメーターの攻撃フラグを下ろす
		CurrentAnimator.SetBool("Attack", false);

		//アニメーターのスケベ攻撃攻撃を下ろす
		CurrentAnimator.SetBool("H_Try", false);

		//アニメーターのダウン着地フラグを下ろす
		CurrentAnimator.SetBool("DownLanding", false);

		//ダメージモーション再生スピードを戻す
		CurrentAnimator.SetFloat("DamageMotionSpeed0", 1);
		CurrentAnimator.SetFloat("DamageMotionSpeed1", 1);

		//ダメージモーション中の移動ベクトル初期化
		DamageMoveVec *= 0;

		//行動中の移動ベクトル初期化
		BehaviorMoveVec *= 0;

		//キャラクターコントローラの大きさを元に戻す
		CharaControllerReset("Reset");

		//攻撃用コライダを無効化
		EndAttackCol();
	}

	//ダメージを受けたら少しの間ダメージコライダを無効化する
	private IEnumerator DamageColEnable()
	{
		//コライダ無効化
		DamageCol.enabled = false;

		//チョイ待機
		yield return new WaitForSeconds(0.05f);

		//コライダ有効化
		DamageCol.enabled = true;
	}
	//キャラクターコントローラの設定を変える関数
	public void CharaControllerReset(string FlagName)
	{
		switch (FlagName)
		{
			case "Reset":

				//キャラクターコントローラの大きさを戻す
				CharaController.center = CharaConCenter;
				CharaController.height = CharaConHeight;
				CharaController.radius = CharaConRad;
				CharaController.skinWidth = CharaConSkin;

				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = CharaController.radius;
				RayPoint = new Vector3(0, RayRadius * 2f, 0);

				//ダメージ用コライダの大きさを戻す
				DamageCol.center = DamageColCenter;
				DamageCol.size = DamageColSize;

				break;

			case "Down":

				//靴の高さを表すskinWidthを消す
				CharaController.skinWidth = 0.1f;

				//キャラクターコントローラの大きさを小さくする
				CharaController.height = 1f;
				CharaController.radius = 0.5f;
				CharaController.center = new Vector3(0, CharaController.radius + CharaController.skinWidth, 0);

				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = CharaController.radius;
				RayPoint = new Vector3(0, RayRadius * 2f, 0);

				//ダメージ用コライダの大きさを小さくする
				DamageCol.size = new Vector3(1, 0.5f, 1);
				DamageCol.center = new Vector3(0, DamageCol.size.y * 0.5f, 0);

				break;

			case "Rise":

				//靴の高さを表すskinWidthを消す
				CharaController.skinWidth = 0.1f;

				//キャラクターコントローラの大きさを小さくする
				CharaController.height = 0.75f;
				CharaController.radius = 0.1f;
				CharaController.center = new Vector3(0, CharaController.height * 0.5f + CharaController.skinWidth, 0);

				//接地コライダの大きさも合わせないと壁際で接地できなくなる
				RayRadius = CharaController.radius;
				RayPoint = new Vector3(0, RayRadius * 2f, 0);

				//ダメージ用コライダの大きさを小さくする
				DamageCol.size = new Vector3(1, 1, 1);
				DamageCol.center = new Vector3(0, DamageCol.size.y * 0.5f, 0);

				break;

			case "H":

				//キャラクターコントローラの高さをゼロにする
				CharaController.height = 0f;
				CharaController.radius = 0.1f;

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
		if (!StunFlag)
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
		if (StunFlag)
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
		CurrentAnimator.SetFloat("StunBlend", 0f);
	}

	//興奮値セット関数
	public void SetExcite(float i)
	{
		Excite += i;

		if (Excite > 1 || Excite < 0)
		{
			Excite = Mathf.Round(Excite);
		}
	}

	//スケベ攻撃が当たった時に呼ばれる
	public void H_AttackHit(string ang, int men, GameObject Player)
	{
		//状態フラグをリセット
		FlagReset();

		//スケベフラグを立てる
		H_Flag = true;

		//スケベ状況を入れる
		H_Location = ang + men;

		//キャラクターコントローラを設定
		CharaControllerReset("H");

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("H_Hit", true);

		//スケベ攻撃フラグを立てる
		CurrentAnimator.SetBool("H_Attack0" + H_State % 2, true);

		//スケベ攻撃ヒットモーションを切り替える
		OverRideAnimator["H_Hit_Void"] = H_HitAnimList.Where(a => a.name.Contains(H_Location)).ToList()[0];

		//スケベ攻撃ヒットモーションを切り替える
		OverRideAnimator["H_Attack0" + H_State % 2 + "_void"] = H_AttackAnimList.Where(a => a.name.Contains(H_Location)).ToList()[0];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//スケベ攻撃遷移関数
	public void H_Transition(string Act)
	{
		//スケベ攻撃フラグを立てる
		CurrentAnimator.SetBool("H_Attack0" + H_State % 2, true);

		//スケベ攻撃ヒットモーションを切り替える
		OverRideAnimator["H_Attack0" + H_State % 2 + "_void"] = H_AttackAnimList.Where(a => a.name.Contains(H_Location + Act)).ToList()[0];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//元のスケベステートに戻る関数
	public void H_ReturnState()
	{
		//スケベ攻撃フラグを立てる
		CurrentAnimator.SetBool("H_Attack0" + H_State % 2, true);
	}

	//スケベ攻撃が解除された時に呼ばれる
	public void H_Break(string location)
	{
		//アニメーターのスケベ解除フラグを立てる
		CurrentAnimator.SetBool("H_Break", true);

		//使用するモーションに差し替え
		OverRideAnimator["H_Break_Void"] = H_BreakAnimList.Where(a => a.name.Contains(location)).ToList()[0];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//攻撃コライダ移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartAttackCol(int n)
	{
		//攻撃用コライダーのコライダ移動関数呼び出し、インデックスとコライダ移動タイプを渡す
		ExecuteEvents.Execute<EnemyAttackCollInterface>(AttackCol, null, (reciever, eventData) => reciever.ColStart(n, UseArts));
	}

	//スケベ攻撃コライダ移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartH_AttackCol()
	{
		//スケベ攻撃用コライダーのコライダ移動関数呼び出し、インデックスとコライダ移動タイプを渡す
		ExecuteEvents.Execute<EnemyAttackCollInterface>(AttackCol, null, (reciever, eventData) => reciever.H_ColStart());
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
		CurrentAnimator.SetBool("Attack", false);

		CurrentAnimator.SetBool("H_Try", false);
	}

	//足元の衝撃エフェクトを表示する、アニメーションクリップのイベントから呼ばれる
	private void FootImpact(float r)
	{
		//エフェクトのインスタンスを生成
		GameObject TempFootImpactEffect = Instantiate(FootImpactEffect);

		//パーティクルシステムを全て格納するList宣言
		List<ParticleSystem> TempFootImpactList = new List<ParticleSystem>();

		//キャラクターの子にする
		TempFootImpactEffect.transform.parent = gameObject.transform.root.transform;

		//ローカル座標で位置を設定
		TempFootImpactEffect.transform.localPosition *= 0;

		//全てのパーティクルシステムを取得
		TempFootImpactList = TempFootImpactEffect.GetComponentsInChildren<ParticleSystem>().Select(i => i).ToList();

		//全てのパーティクルシステムを回す
		foreach (ParticleSystem i in TempFootImpactList)
		{
			//アクセサを取得
			ParticleSystem.ShapeModule p = i.shape;

			//引数と親の角度を元にエミッタを回転、一番親の奴だけ
			if (i.name.Contains("Clone"))
			{
				p.rotation = new Vector3(-r, gameObject.transform.rotation.eulerAngles.y, 0);
			}

			//エフェクトを再生
			i.Play();
		}
	}

	//歩調に合わせるサインカーブ生成に使う数リセット、アニメーションクリップから呼ばれる
	public void SetSinCount()
	{
		SinCount = 0;
	}

	//アニメーターの攻撃モーションを切り替える、ビヘイビアスクリプトから呼ばれる
	public void SetAttackMotion(string n)
	{
		//使用した攻撃をキャッシュ
		UseArts = AttackClassList.Where(a => a.AttackID.Contains(n)).ToList()[0];

		//ビヘイビアにも渡す
		ExecuteEvents.Execute<EnemyBehaviorInterface>(gameObject, null, (reciever, eventData) => reciever.SetArts(UseArts));

		//使用するモーションに差し替え
		OverRideAnimator["Attack_Void"] = UseArts.Anim;

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
	}

	//ビヘイビアリストを受け取る
	public void SetBehaviorList(List<EnemyBehaviorClass> i)
	{
		BehaviorList = new List<EnemyBehaviorClass>(i);
	}

	//スケベヒットモーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetH_HitAnimList(List<AnimationClip> i)
	{
		//受け取ったListを変数に格納
		H_HitAnimList = new List<AnimationClip>(i);
	}

	//スケベアタックモーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetH_AttackAnimList(List<AnimationClip> i)
	{
		//受け取ったListを変数に格納
		H_AttackAnimList = new List<AnimationClip>(i);
	}

	//スケベブレイクモーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetH_BreakAnimList(List<AnimationClip> i)
	{
		//受け取ったListを変数に格納
		H_BreakAnimList = new List<AnimationClip>(i);
	}

	//ダメージモーションListを受け取る、セッティングスクリプトから呼ばれる
	public void SetDamageAnimList(List<AnimationClip> i)
	{
		//受け取ったListを変数に格納
		DamageAnimList = new List<AnimationClip>(i);

		//オーバーライドコントローラにアニメーションクリップをセット、これをしないとTスタンスが見える
		OverRideAnimator["Damage_Void_0"] = DamageAnimList[0];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
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

		if (b)
		{
			//アニメーション一時停止
			CurrentAnimator.speed = 0;
		}
		else
		{
			//アニメーション再生速度を戻す
			CurrentAnimator.speed = 1;
		}
	}

	//オブジェクト削除コルーチン
	private IEnumerator VanishCoroutine()
	{
		//チョイ待機
		yield return new WaitForSeconds(1);

		//消滅用カウント
		float VanishCount = 0;

		//Enemyレイヤーのレンダラー取得
		List<Renderer> RendList = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>().Where(a => a.gameObject.layer == LayerMask.NameToLayer("Enemy")).ToList());

		//レンダラーを回す
		foreach (var i in RendList)
		{
			//アウトラインを切る為にレイヤーを変更
			//i.gameObject.layer = LayerMask.NameToLayer("InDoor");

			//レンダラーのシャドウを切る
			i.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			//描画順を変更
			i.material.renderQueue = 3000;
		}

		while (VanishCount < 1)
		{
			//マテリアルを回して消滅用数値を入れる
			foreach (var i in RendList)
			{
				foreach (var ii in i.materials)
				{
					ii.SetFloat("_VanishNum", VanishCount);
				}
			}

			//消滅用カウントアップ
			VanishCount += Time.deltaTime;

			//１フレーム待機
			yield return null;
		}

		//マテリアルを回して消滅用数値を入れて完全に消す
		foreach (var i in RendList)
		{
			foreach (var ii in i.materials)
			{
				ii.SetFloat("_VanishNum", 1);
			}
		}

		//自身を削除
		Destroy(gameObject);
	}

	//プレイヤーキャラクターをセットする、キャラ交代した時にMissionManagerから呼ばれる
	public void SetPlayerCharacter(GameObject c)
	{
		//プレイヤーキャラクター更新
		PlayerCharacter = c;

		//ビヘイビアにも渡す
		ExecuteEvents.Execute<EnemyBehaviorInterface>(gameObject, null, (reciever, eventData) => reciever.SetPlayerCharacter(c));
	}

	//戦闘継続処理
	public void BattleNext()
	{
		StartCoroutine(BattleNextCoroutine());
	}
	private IEnumerator BattleNextCoroutine()
	{
		//アニメーター再生
		CurrentAnimator.speed = 1;


		//アングルを測定、プレイヤーに向くまでループ
		while (Vector3.Angle(gameObject.transform.forward, HorizontalVector(PlayerCharacter, gameObject)) > 1)
		{
			//プレイヤーに向けて回転
			EventRotate = HorizontalVector(PlayerCharacter, gameObject);

			//１フレーム待機
			yield return null;
		}

		//たむろモーションが終わるまで待つ
		while (CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
		{
			//１フレーム待機
			yield return null;
		}

		//アイドリングモーションを変更
		CurrentAnimator.SetFloat("IdlingBlend", 0);
	}

	//戦闘開始処理
	public void BattleStart()
	{
		StartCoroutine(BattleStartCoroutine());
	}
	private IEnumerator BattleStartCoroutine()
	{
		//アニメーター再生
		CurrentAnimator.speed = 1;

		//たむろモーションが終わるまで待つ
		while (CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
		{
			//１フレーム待機
			yield return null;
		}

		//アイドリングモーションを変更
		CurrentAnimator.SetFloat("IdlingBlend", 0);

		//ダメージコライダ有効化
		DamageCol.enabled = true;

		//戦闘フラグを立てる
		BattleFlag = true;
	}
}
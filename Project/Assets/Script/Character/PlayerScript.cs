using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using Cinemachine;
using UnityEngine.UI;

/*
+	//角度がきつい坂で滑り落ちる処理
	if (Vector3.Angle(RayHit.normal, Vector3.up) > 45.0f && OnGround)
	{
		MoveVector += Vector3.ProjectOnPlane(transform.forward, RayHit.normal) * PlayerMoveSpeed * Time.deltaTime;
	}

	//地面に沿わせて体を傾ける、ちょっと保留
	//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(MoveVector, RayHit.normal), Vector3.up), TurnSpeed * Time.deltaTime);
	
*/

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface PlayerScriptInterface : IEventSystemHandler
{
	//プレイヤーの攻撃が敵に当たった時の処理
	void HitAttack(GameObject e, int AttackIndex);

	//敵の攻撃が当たった時の処理
	void HitEnemyAttack(EnemyAttackClass arts, GameObject enemy);

	//スケベ攻撃が当たった時の処理
	void H_AttackHit(string ang, int men, GameObject M_Enemy, GameObject S_Enemy);

	//武器をセットする
	void SetWeapon(GameObject w);

	//戦闘フラグをセットする
	void SetFightingFlag(bool b);

	//ポーズ処理
	void Pause(bool b);

	//自身がカメラの画角に入っているか返す
	bool GetOnCameraBool();

	//キャラクターのデータセットする
	void SetCharacterData(CharacterClass CC, List<AnimationClip> DAL, List<AnimationClip> HHL, List<AnimationClip> HDL, List<AnimationClip> HBL, GameObject CRO, GameObject MSA);

	//当たった攻撃が有効か返す
	bool AttackEnable(bool H);
}

public class PlayerScript : GlobalClass, PlayerScriptInterface
{

	//--- オブジェクト　コンポーネント類 ---//

	//キャラクターのID
	private int CharacterID;

	//GameManagerのAllActiveCharacterListに登録されているインデックス
	private int AllActiveCharacterListListIndex;

	//OnCamera判定用スクリプトを持っているオブジェクト
	private GameObject OnCameraObject;

	//プレイヤーが操作するキャラクターコントローラ	
	private CharacterController Controller;

	//キャラクターのアニメーター
	private Animator CurrentAnimator;

	//オーバーライドアニメーターコントローラ、アニメーションクリップ差し替え時に使う
	private AnimatorOverrideController OverRideAnimator;

	//攻撃時にロックした敵オブジェクト
	private GameObject LockEnemy;

	//スケベ攻撃をしてきた敵オブジェクト
	private GameObject H_MainEnemy;

	//スケベ攻撃時に近くにいた敵オブジェクト
	private GameObject H_SubEnemy;

	//コスチュームルートオブジェクト
	private GameObject CostumeRootOBJ;

	//モザイクオブジェクト
	private GameObject MosaicOBJ;

	//足音オブジェクト
	private GameObject FootStepOBJ;

	//攻撃ヒット音オブジェクト
	private GameObject AttackImpactOBJ;



	//--- UI ---//




	//--- 固定パラメータ ---//

	//キャラクターの移動スピード
	private float PlayerMoveSpeed;

	//キャラクターのダッシュスピード
	private float PlayerDashSpeed;

	//キャラクターのローリングスピード
	private float RollingSpeed;

	//キャラクターのジャンプ力
	private float JumpPower;

	//キャラクターの旋回スピード
	private float TurnSpeed;

	//遠近攻撃を切り替えるしきい値
	private float AttackDistance;

	//超必殺技を発動できるバリバリゲージしきい値
	public float SuperGauge;


	//--- 変動パラメータ ---//

	//ライフゲージ
	public float L_Gauge { get; set; }

	//バリバリゲージ
	public float B_Gauge { get; set; }



	//--- 入力フラグ ---//

	//ジャンプ入力フラグ
	private bool JumpInput = false;

	//ローリング入力フラグ
	private bool RollingInput = false;

	//攻撃入力フラグ
	private bool AttackInput = false;

	//特殊攻撃入力フラグ
	private bool SpecialInput = false;

	//超必殺技入力フラグ
	private bool SuperInput = false;

	//オートコンボ入力フラグ
	private bool AutoComboInput = false;



	//--- 状態フラグ ---//

	//接地フラグ
	private bool OnGroundFlag = false;

	//戦闘フラグ
	private bool FightingFlag = false;

	//攻撃ボタン押しっぱなしフラグ
	private bool HoldButtonFlag = false;

	//ホールド攻撃フラグ
	private bool HoldFlag = false;

	//急斜面フラグ
	private bool OnSlopeFlag = false;

	//踏み外しフラグ
	private bool DropFlag = false;

	//敵接触フラグ
	private bool EnemyContactFlag = false;

	//ダメージ状態フラグ
	private bool DamageFlag = false;

	//ダッシュフラグ
	private bool DashFlag = false;

	//空中ローリング許可フラグ
	private bool AirRollingFlag = true;

	//特殊攻撃待機フラグ
	private bool SpecialTryFlag = false;

	//特殊攻撃成功フラグ
	private bool SpecialSuccessFlag = false;

	//特殊攻撃中フラグ
	public bool SpecialAttackFlag { get; set; } = false;

	//超必殺技中フラグ
	public bool SuperFlag { get; set; } = false;

	//スケベフラグ
	public bool H_Flag { get; set; }

	//回転禁止フラグ
	private bool NoRotateFlag = false;

	//クロスバタバタフラグ
	private bool ClothShakeFlag = false;

	//アクションイベントフラグ
	private bool ActionEventFlag = false;

	//ポーズフラグ
	private bool PauseFlag = false;



	//--- 移動値 ---//

	//最終的な移動ベクトル
	private Vector3 MoveVector;

	//水平方向加速度
	private Vector3 HorizonAcceleration;

	//垂直方向の加速度
	private float VerticalAcceleration;

	//重力加速度
	private float GravityAcceleration;

	//ジャンプ開始直後のレバー入力
	private Vector3 JumpHorizonVector;

	//キャラクターのジャンプの回転値
	private Vector3 JumpRotateVector;

	//移動速度補正値
	private float PlayerMoveParam;

	//移動モーションブレンド補正
	private float PlayerMoveBlend;

	//インプットシステムから入力されるキャラクター移動ベクトル
	private Vector2 PlayerMoveInputVecter;

	//キャラクターのローリング移動ベクトル
	private Vector3 RollingMoveVector;

	//キャラクターのローリング回転値
	private Vector3 RollingRotateVector;

	//キャラクターの攻撃移動ベクトル
	private Vector3 AttackMoveVector;

	//キャラクターの攻撃移動のタイプ
	private int AttackMoveType;

	//ダメージ移動ベクトル
	private Vector3 DamageMoveVector;

	//キャラクターのスケベ移動ベクトル
	private Vector3 H_MoveVector;

	//キャラクターのスケベ回転値
	private Vector3 H_RotateVector;

	//イベント回転ベクトル
	private Vector3 EventRotateVector;

	//強制移動ベクトル
	private Vector3 ForceMoveVector;

	//特殊攻撃移動ベクトル
	public Vector3 SpecialMoveVector { get; set; }

	//超必殺技移動ベクトル
	public Vector3 SuperMoveVector { get; set; }





	//--- レイキャスト関連 ---//

	//キャラクターの接地判定をするレイが当たったオブジェクトの情報を格納
	private RaycastHit RayHit;

	//キャラクターの接地判定をするレイの発射位置
	private Vector3 RayPoint;

	//キャラクターの接地判定をするレイの大きさ
	private Vector3 RayRadius;



	//--- カメラ関連 ---//

	//カメラ基準の移動をするためベクトルを取るダミー
	private GameObject PlayerMoveAxis;

	//メインカメラのトランスフォーム
	private Transform MainCameraTransform;



	//--- 制御変数 ---//

	//入力許可フラグを管理する連想配列
	private Dictionary<string, bool> PermitInputBoolDic = new Dictionary<string, bool>();

	//遷移許可フラグを管理する連想配列
	private Dictionary<string, bool> PermitTransitionBoolDic = new Dictionary<string, bool>();

	//アニメーターのレイヤー0に存在する全てのステート名
	private List<string> AllStates = new List<string>();

	//アニメーターのレイヤー0に存在する全てのトランジション名
	private List<string> AllTransitions = new List<string>();

	//無敵状態List
	private List<string> InvincibleList = new List<string>();

	//現在のステート
	private string CurrentState = "";

	//現在の地面属性
	private string GroundSurface = "";

	//キャラクターと地面との距離
	private float GroundDistance;

	//ターゲットしている敵との距離
	private float EnemyDistance;

	//ジャンプボタンが押された時間
	private float JumpTime;

	//チェイン攻撃の入力受付時間
	private float ChainAttackWaitTime;

	//タメ攻撃チャージレベル
	public int ChargeLevel { get; set; }

	//ダッシュ入力受付時間
	private float DashInputTime;

	//超必殺技制御カウント
	private int SuperCount = 0;

	//脱出用レバガチャカウント
	public int BreakCount { get; set; } = 0;

	//脱出レバガチャ許可フラグ
	private bool BreakInputFlag = false;

	//上着はだけフラグ
	private bool TopsOffFlag = false;

	//パンツ下ろしフラグ
	private bool PantsOffFlag = false;








	//装備している武器のオブジェクト
	private GameObject WeaponOBJ;

	//装備している特殊技
	public List<SpecialClass> SpecialArtsList = new List<SpecialClass>();

	//装備している超必殺技
	public SuperClass SuperArts { get; set; } = null;

	//何も技を装備していないフラグ
	private bool NoEquipFlag = false;

	//攻撃用コライダを持っているオブジェクト
	private GameObject AttackColOBJ;

	//強制移動コライダを持っているオブジェクト
	private GameObject ForceMoveColOBJ;

	//目オブジェクト、視線を操作するために使う
	private GameObject EyeOBJ;

	//視線を動かすための目マテリアル
	private Material EyeMaterial;

	//視線を向ける先のポジション
	private Vector3 CharacterLookAtPos;

	//視線変更許可フラグ
	private bool LookAtPosFlag = true;

	//瞬き禁止フラグ
	private bool NoBlinkFlag = false;

	//瞬き処理経過時間
	private float BlinkDelayTime;

	//頬を赤らめるための顔マテリアル
	private Material FaceMaterial;

	//顔テクスチャ
	public Texture2D FaceBaseTex { get; set; }

	//赤頬テクスチャ		
	public Texture2D FaceCheekTex { get; set; }

	//スケベ用カメラワークオブジェクト
	private GameObject H_CameraOBJ;




	//出す技を判定するコンボステート
	private int ComboState;

	//出す表情を判定するコンボステート
	private int FaceState;

	//出すスケベダメージモーションを判定するスケベステート
	private int H_State;

	//スケベモーションループカウント
	private int H_Count;

	//現在のスケベ状況
	private string H_Location;

	//遷移するステートを振り分ける文字列
	private string TransitionAttackState;

	//オーバーライドするステートを振り分ける文字列
	private string OverrideAttackState;

	//攻撃時のボタン入力
	private int AttackButton;

	//攻撃時の状態
	private int AttackLocation;

	//攻撃時のスティック入力
	private int AttackStick;

	//現在使用している技
	private ArtsClass UseArts;





	//技格納マトリクス
	private List<List<List<ArtsClass>>> ArtsMatrix = new List<List<List<ArtsClass>>>();

	//攻撃制御用マトリクス
	private List<List<List<int>>> ArtsStateMatrix = new List<List<List<int>>>();

	//ダメージモーションList
	private List<AnimationClip> DamageAnimList = new List<AnimationClip>();

	//スケベヒットモーションList
	private List<AnimationClip> H_HitAnimList = new List<AnimationClip>();

	//スケベダメージモーションList
	private List<AnimationClip> H_DamageAnimList = new List<AnimationClip>();

	//スケベブレイクモーションList
	private List<AnimationClip> H_BreakAnimList = new List<AnimationClip>();

	//ダメージモーションインデックス
	private int DamageAnimIndex = 0;

	//特殊攻撃入力インデックス
	private int SpecialInputIndex = 100;

	//特殊攻撃入力待ち時間
	private float SpecialSelectTime = 0.1f;






	//攻撃時のTrailエフェクト
	private GameObject AttackTrail;

	//足元の衝撃エフェクト
	private GameObject FootImpactEffect;

	//足元の煙エフェクト
	private GameObject FootSmokeEffect;

	//タメエフェクト
	private GameObject ChargePowerEffect;

	//タメ完了エフェクト
	private GameObject ChargeLevelEffect;

	//特殊攻撃成功エフェクト
	private GameObject SpecialSuccessEffect;
	
	//スケベエフェクト00
	private GameObject H_Effect00;

	//超必殺技用カメラワークオブジェクト
	private GameObject SuperCameraWorkOBJ;



	//モーションにノイズを加えるボーンList
	private List<GameObject> AnimNoiseBone = new List<GameObject>();

	//モーションノイズのランダムシードList
	private List<Vector3> AnimNoiseSeedList = new List<Vector3>();



	void Start()
	{
		//自身をGameManagerのリストに追加、インデックスを受け取る
		AllActiveCharacterListListIndex = GameManagerScript.Instance.AddAllActiveCharacterList(gameObject);

		//OnCamera判定用スクリプトを持っているオブジェクトを検索して取得
		foreach (Transform i in GetComponentsInChildren<Transform>())
		{
			if (i.GetComponent<OnCameraScript>() != null)
			{
				OnCameraObject = i.gameObject;
			}
		}
		
		//CharacterSettingScriptからID取得
		ExecuteEvents.Execute<CharacterSettingScriptInterface>(gameObject, null, (reciever, eventData) => CharacterID = reciever.GetCharacterID());

		//キャラクターコントローラ取得
		Controller = GetComponent<CharacterController>();

		//キャラクターのアニメーターコントローラ取得
		CurrentAnimator = GetComponentInChildren<Animator>();

		//攻撃用コライダを持っているオブジェクト取得
		AttackColOBJ = DeepFind(gameObject, "PlayerAttackColl");

		//目オブジェクト取得、視線を操作するために使う
		EyeOBJ = gameObject.GetComponentsInChildren<Renderer>().Where(i => i.name.Contains("Eye")).ToArray()[0].gameObject;

		//視線を動かすための目マテリアル取得
		EyeMaterial = transform.GetComponentsInChildren<Renderer>().Where(i => i.transform.name.Contains("Eye")).ToArray()[0].material;

		//視線を向ける先のポジション初期化
		CharacterLookAtPos = Vector3.zero;

		//頬を赤らめるための顔マテリアル取得
		FaceMaterial = transform.GetComponentsInChildren<Renderer>().Where(i => i.transform.name.Contains("Face")).ToArray()[0].material;

		//攻撃コライダを非アクティブ化しておく
		AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

		//オーバーライドアニメーターコントローラ初期化
		OverRideAnimator = new AnimatorOverrideController();

		//オーバーライドアニメーターコントローラに元アニメーターコントローラをコピー
		OverRideAnimator.runtimeAnimatorController = CurrentAnimator.runtimeAnimatorController;

		//メインカメラのトランスフォーム取得
		MainCameraTransform = GameObject.Find("MainCamera").transform;

		//スケベ用カメラワークオブジェクト取得
		H_CameraOBJ = DeepFind(gameObject , "H_Camera");

		//ライフゲージ初期化
		L_Gauge = 10f;

		//バリバリゲージ初期化
		B_Gauge = 1f;

		//UIにカメラを追加
		DeepFind(gameObject, "UI").GetComponent<Canvas>().worldCamera = MainCameraTransform.gameObject.GetComponent<Camera>();

		//UIのパネルディタンス設定
		DeepFind(gameObject, "UI").GetComponent<Canvas>().planeDistance = 0.15f;

		//UIのパネルディタンス設定
		DeepFind(gameObject, "MiniMap").GetComponent<RawImage>().texture = GameObject.Find("MiniMapCamera").GetComponent<Camera>().targetTexture;

		//移動ベクトル用ダミー取得
		PlayerMoveAxis = DeepFind(transform.gameObject, "PlayerMoveAxis");

		//インプットシステムから入力されるキャラクター移動値初期化
		PlayerMoveInputVecter = Vector2.zero;

		//水平加速度初期化
		HorizonAcceleration = Vector3.zero;

		//垂直加速度初期化
		VerticalAcceleration = 0;

		//重力加速度初期化
		GravityAcceleration = 0;

		//キャラクターのローリング移動値初期化
		RollingMoveVector = Vector3.zero;

		//キャラクターのローリング回転値初期化
		RollingRotateVector = Vector3.zero;

		//キャラクターのスケベ移動ベクトル初期化
		H_MoveVector = Vector3.zero;

		//キャラクターのスケベ回転値初期化
		H_RotateVector = Vector3.zero;

		//キャラクターのジャンプの回転値初期化
		JumpRotateVector = Vector3.zero;

		//ジャンプボタンが押された時間初期化
		JumpTime = 0;

		//全ての移動値を合算した移動ベクトル初期化
		MoveVector = Vector3.zero;

		//移動速度補正値初期化
		PlayerMoveParam = 1f;

		//キャラクターの接地判定をするレイの発射位置、キャラクターコントローラから算出
		RayPoint = new Vector3(0, Controller.height, Controller.center.z);

		//キャラクターの接地判定をするレイの大きさ、キャラクターコントローラから算出
		RayRadius = new Vector3(Controller.radius, Controller.height * 0.5f, Controller.radius);

		//ディレイ処理の待機時間初期化
		BlinkDelayTime = 0;

		//出す技を判定するコンボステート初期化
		ComboState = 0;

		//出す表情を判定するコンボステート初期化
		FaceState = 0;

		//出すスケベダメージモーションを判定するスケベステート初期化
		H_State = 0;

		//スケベモーションループカウント初期化
		H_Count = 0;

		//現在使用している技初期化
		UseArts = null;

		//攻撃時にロックした敵初期化
		LockEnemy = null;

		//スケベ攻撃をしてきた敵初期化
		H_MainEnemy = null;

		//スケベ攻撃をしてきた敵の近くにいた敵初期化
		H_SubEnemy = null;

		//ターゲットしている敵との距離初期化
		EnemyDistance = 0;

		//攻撃時の移動ベクトルのキャッシュ初期化
		List<Color> AttackMoveVec = new List<Color>();

		//攻撃移動のタイプ初期化
		AttackMoveType = 100;

		//技をマトリクスに装備
		ArtsMatrixSetUp();

		//攻撃制御用マトリクス初期化
		ArtsStateMatrixReset();

		//ダッシュ入力受付時間初期化
		DashInputTime = 0;

		//スケベフラグ初期化
		H_Flag = false;

		//イベント回転ベクトル初期化
		EventRotateVector = Vector3.zero;

		//強制移動ベクトル初期化
		ForceMoveVector = Vector3.zero;

		//ダメージ移動ベクトル初期化
		DamageMoveVector = Vector3.zero;

		//特殊攻撃移動ベクトル初期化
		SpecialMoveVector = Vector3.zero;

		//超必殺技移動ベクトル初期化
		SuperMoveVector = Vector3.zero;

		//攻撃時のTrailエフェクト取得
		AttackTrail = DeepFind(gameObject, "AttackTrail");

		//足元の衝撃エフェクト取得
		FootImpactEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "FootImpact").ToArray()[0];

		//足元の煙エフェクト取得
		FootSmokeEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "FootSmoke").ToArray()[0];

		//タメエフェクト取得
		ChargePowerEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "ChargePower").ToArray()[0];

		//タメ完了エフェクト取得
		ChargeLevelEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "ChargeLevel").ToArray()[0];

		//特殊攻撃成功エフェクト取得
		SpecialSuccessEffect = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "SpecialSuccessEffect").ToArray()[0];

		//スケベエフェクト00取得
		H_Effect00 = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "H_Effect00").ToArray()[0];

		//足音オブジェクト取得
		FootStepOBJ = DeepFind(gameObject, "FootStep");

		//攻撃ヒット音オブジェクト取得
		AttackImpactOBJ = DeepFind(gameObject, "AttackImpact");

		//超必殺技装備
		foreach (var i in GameManagerScript.Instance.AllSuperArtsList.Where(a => a.UseCharacter == CharacterID && a.ArtsIndex == GameManagerScript.Instance.UserData.EquipSuperArts[CharacterID]).ToArray())
		{
			//超必殺技代入
			SuperArts = i;

			//超必殺技用カメラワークオブジェクト生成
			SuperCameraWorkOBJ = Instantiate(SuperArts.Vcam);
			SuperCameraWorkOBJ.transform.parent = transform;
			SuperCameraWorkOBJ.transform.localPosition = Vector3.zero;
			SuperCameraWorkOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//超必殺技のモーションを仕込む
			OverRideAnimator["SuperTry_void"] = GameManagerScript.Instance.AllSuperArtsList.Where(a => a.TryAnimClip.name.Contains(CharacterID + "_SuperTry" + a.ArtsIndex)).ToArray()[0].TryAnimClip;
			OverRideAnimator["SuperArts_void"] = GameManagerScript.Instance.AllSuperArtsList.Where(a => a.ArtsAnimClip.name.Contains(CharacterID + "_SuperArts" + a.ArtsIndex)).ToArray()[0].ArtsAnimClip;

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
		}

		//とりあえずなんでもいいから技を入れる、これが無いと最初の攻撃の時に一瞬Ｔスタンスが見える
		foreach (List<List<ArtsClass>> i in ArtsMatrix)
		{
			foreach (List<ArtsClass> ii in i)
			{
				foreach (ArtsClass iii in ii)
				{
					if (iii != null)
					{
						//次に書き換えるアニメーションを設定、ステートを2で割った余りで01を切り替えて文字列として連結
						OverrideAttackState = "Arts_void_0";

						//オーバーライドコントローラにアニメーションクリップをセット
						OverRideAnimator[OverrideAttackState] = iii.AnimClip;

						//アニメーターを上書きしてアニメーションクリップを切り替える
						CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

						//多重ループを抜ける
						goto ArtsClassLinstLoopBreak;

					}
				}
			}
		}

		//何も技を装備していない場合フラグを立てる
		NoEquipFlag = true;

		//多重ループを抜ける先
		ArtsClassLinstLoopBreak:;

		//無敵状態になるステートListに手動でAdd
		InvincibleList.Add("Rolling");
		InvincibleList.Add("Damage");
		InvincibleList.Add("Down");
		InvincibleList.Add("-> Jump");
		InvincibleList.Add("SpecialAttack");
		InvincibleList.Add("SpecialSuccess");
		InvincibleList.Add("H_Hit");
		InvincibleList.Add("H_Damage00");
		InvincibleList.Add("H_Damage01");
		InvincibleList.Add("H_Break");
		InvincibleList.Add("SuperTry");
		InvincibleList.Add("SuperArts");

		//全てのステート名を手動でAdd、アニメーターのステート名は外部から取れない
		AllStates.Add("AnyState");
		AllStates.Add("Idling");
		AllStates.Add("Run");
		AllStates.Add("Fall");
		AllStates.Add("Jump");
		AllStates.Add("Crouch");
		AllStates.Add("GroundRolling");
		AllStates.Add("AirRolling");
		AllStates.Add("Attack00");
		AllStates.Add("Attack01");
		AllStates.Add("ChainBreak");
		AllStates.Add("Drop");
		AllStates.Add("Stop");
		AllStates.Add("EventAction");
		AllStates.Add("Damage");
		AllStates.Add("Down");
		AllStates.Add("SpecialTry");
		AllStates.Add("SpecialSuccess");		
		AllStates.Add("SpecialAttack");
		AllStates.Add("H_Hit");
		AllStates.Add("H_Damage00");
		AllStates.Add("H_Damage01");
		AllStates.Add("H_Break");
		AllStates.Add("SuperTry");
		AllStates.Add("SuperArts");

		//全てのステートとトランジションをListにAdd
		foreach (string i in AllStates)
		{
			//入力許可フラグ管理用連想配列にステート名をキーにしたboolをAdd、とりあえずfalseを入れとく
			PermitInputBoolDic.Add(i, false);

			//遷移許可フラグ管理用連想配列にステート名をキーにしたboolをAdd、とりあえずfalseを入れとく
			PermitTransitionBoolDic.Add(i, false);

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
					i.name.Contains("Wrist") ||
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
		}
	}

	void LateUpdate()
	{
		//超必殺技中は揺らさない
		if(!SuperFlag)
		{
			//ループカウント
			int count = 0;

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
				i.transform.localRotation *= Quaternion.Euler((NoiseVec1 + NoiseVec2 * 0.25f) * 5f);

				//カウントアップ
				count++;
			}
		}
	}

	void Update()
	{
		if (!PauseFlag)
		{
			//接地判定用のRayを飛ばす関数呼び出し
			GroundRayCast();

			//アニメーションステートを監視する関数呼び出し
			StateMonitor();

			//遷移許可フラグを管理する関数呼び出し
			TransitionManager();

			//アニメーション制御関数呼び出し
			AnimFunc();

			//移動制御関数呼び出し
			MoveFunc();

			//キャラクター移動
			Controller.Move(MoveVector);

			//スケベ処理、遷移中は処理しない
			if (H_Flag)
			{
				H_Func();
			}

			//毎フレーム呼ばなくてもいい処理
			DelayFunc();
		}
	}

	//スケベ処理
	private void H_Func()
	{
		//ブレイクカウントが達した
		if (BreakCount > 10 && BreakInputFlag)
		{
			//レバガチャインプットフラグを下す
			BreakInputFlag = false;

			//アニメーターのフラグを立てる
			CurrentAnimator.SetBool("H_Break", true);

			//頬テクスチャを顔テクスチャに差し替え
			FaceMaterial.SetTexture("_TexFaceBase", FaceBaseTex);

			//オーバーライドコントローラにアニメーションクリップをセット
			OverRideAnimator["H_Break_void"] = H_BreakAnimList.Where(a => a.name.Contains(H_Location)).ToArray()[0];

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

			//敵側のスケベ解除スクリプト呼び出し
			ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.H_Break(H_Location));

			//ブレイクのカメラワーク設定
			H_CameraOBJ.GetComponent<CinemachineCameraScript>().SpecifyIndex = H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(H_Location + "_Break")).ToList()[0]);

			//カメラワーク持続フラグを下ろして次のカメラワークへ
			H_CameraOBJ.GetComponent<CinemachineCameraScript>().KeepCameraFlag = false;
		}
		//敵の興奮値が上がり、何回かループした
		else if(H_MainEnemy.GetComponent<EnemyCharacterScript>().Excite > 0.2f && H_Count > 2 && CurrentState.Contains("H_Damage"))
		{
			//スケベカウントリセット
			H_Count = 0;

			//後ろから
			if (H_Location.Contains("Back"))
			{
				//トップスがはだけていない
				if (!TopsOffFlag)
				{
					//レバガチャインプットフラグを下す
					BreakInputFlag = false;

					//トップスはだけフラグを立てる
					TopsOffFlag = true;

					//オーバーライドコントローラにアニメーションクリップをセット
					OverRideAnimator["H_Damage_" + H_State % 2 + "_void"] = H_DamageAnimList.Where(a => a.name.Contains(H_Location + "_TopsOff")).ToList()[0];

					//アニメーターを上書きしてアニメーションクリップを切り替える
					CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

					//アニメーション遷移フラグを立てる
					CurrentAnimator.SetBool("H_Damage0" + H_State % 2, true);

					//スケベカメラワーク再生
					H_CameraOBJ.GetComponent<CinemachineCameraScript>().PlayCameraWork(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(H_Location + "_TopsOff")).ToList()[0]), false);

					//敵側の処理を呼び出す
					ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.H_Transition("_TopsOff"));
				}
			}
			//前から
			else if (H_Location.Contains("Forward"))
			{
				//パンツが降りていない
				if (!PantsOffFlag)
				{
					//レバガチャインプットフラグを下す
					BreakInputFlag = false;

					//パンツ下ろしフラグを立てる
					PantsOffFlag = true;

					//オーバーライドコントローラにアニメーションクリップをセット
					OverRideAnimator["H_Damage_" + H_State % 2 + "_void"] = H_DamageAnimList.Where(a => a.name.Contains(H_Location + "_PantsOff")).ToList()[0];

					//アニメーターを上書きしてアニメーションクリップを切り替える
					CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

					//アニメーション遷移フラグを立てる
					CurrentAnimator.SetBool("H_Damage0" + H_State % 2, true);

					//敵側の処理を呼び出す
					ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.H_Transition("_PantsOff"));
		   					 				  				  				 			   				
					//スケベカメラワーク再生
					H_CameraOBJ.GetComponent<CinemachineCameraScript>().PlayCameraWork(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(H_Location + "_PantsOff")).ToList()[0]), false);
				}
			}
		}
	}

	//スケベカメラワークを次に進める
	public void NextH_CameraWork(string n)
	{		
		//次のカメラワーク設定
		H_CameraOBJ.GetComponent<CinemachineCameraScript>().SpecifyIndex = H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(n)).ToList()[0]);

		//カメラワークを進める
		H_CameraOBJ.GetComponent<CinemachineCameraScript>().KeepCameraFlag = false;
	}

	//元のスケベステートに戻る関数、アニメーションクリップから呼ばれる
	public void H_ReturnState()
	{
		//遷移中は無効にしないと何回も呼ばれてしまう
		if(!CurrentAnimator.IsInTransition(0))
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("H_Damage0" + H_State % 2, true);

			//敵側の処理を呼び出す
			ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.H_ReturnState());
		}
	}

	//スケベブレイク許可フラグを立てる関数
	public void H_BreakInputFlag()
	{
		//初回ループ時のみ処理
		if(H_Count == 0)
		{
			//レバガチャインプットフラグを立てる
			BreakInputFlag = true;
		}
	}

	//スケベループカウントアップ
	public void H_CountUp()
	{
		H_Count++;
	}

	//移動キーを押した時
	private void OnPlayerMove(InputValue inputValue)
	{
		//入力をキャッシュ
		PlayerMoveInputVecter = inputValue.Get<Vector2>();
	}

	//バリバリゲージによってスケベ表情を切り替える関数
	private void ChangeH_Face()
	{
		//表情レベル
		int Level;
		
		//バリバリゲージによってレベルを変える
		if(B_Gauge > 0.8f)
		{
			Level = 0;
		}
		else if (B_Gauge > 0.6f)
		{
			Level = 1;
		}
		else if (B_Gauge > 0.4f)
		{
			Level = 2;
		}
		else if (B_Gauge > 0.2f)
		{
			Level = 3;
		}
		else
		{
			Level = 4;
		}

		//表情変え関数呼び出し
		ChangeFace("Motion_" + CharacterID + "_H_Face0" + Level + ",0.5");
	}

	//ジャンプキーを押した時
	private void OnPlayerJump()
	{
		//ジャンプ入力許可条件判定
		if (PermitInputBoolDic["Jump"])
		{
			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//フラグ切り替え
			JumpInput = true;
		}

		//スケベ脱出カウントアップ
		if (BreakInputFlag)
		{
			BreakCount++;
		}
	}

	//ローリングキーを押した時
	private void OnPlayerRolling()
	{
		//ローリング入力許可条件判定
		if (PermitInputBoolDic["GroundRolling"] || PermitInputBoolDic["AirRolling"])
		{
			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//ローリング入力フラグ切り替え
			RollingInput = true;

			//地上でレバー入力されていたらそちらに向ける
			if (OnGroundFlag && PlayerMoveInputVecter != Vector2.zero)
			{
				RollingRotateVector = (PlayerMoveAxis.transform.forward * PlayerMoveInputVecter.y) + (PlayerMoveAxis.transform.right * PlayerMoveInputVecter.x);
			}
			//空中もしくはレバー入力されていなかったら正面そのまま
			else
			{
				RollingRotateVector = transform.forward;
			}
		}
	}

	//攻撃ボタンが押された時
	private void OnPlayerAttack00(InputValue i)
	{
		//押した時にture、離した時にfalseが入る
		HoldButtonFlag = i.isPressed;

		if (i.isPressed && !SpecialSuccessFlag)
		{
			AttackInputFunc(0);
		}
		//特殊攻撃成功時に派生を選ぶ
		else if (SpecialSuccessFlag)
		{
			SpecialInputIndex = 0;
		}

		//スケベ脱出カウントアップ
		if(BreakInputFlag)
		{
			BreakCount++;
		}
		
	}
	private void OnPlayerAttack01(InputValue i)
	{
		//押した時にture、離した時にfalseが入る
		HoldButtonFlag = i.isPressed;

		if (i.isPressed && !SpecialSuccessFlag)
		{
			AttackInputFunc(1);
		}
		//特殊攻撃成功時に派生を選ぶ
		else if (SpecialSuccessFlag)
		{
			SpecialInputIndex = 1;
		}

		//スケベ脱出カウントアップ
		if (BreakInputFlag)
		{
			BreakCount++;
		}
	}
	private void OnPlayerAttack02(InputValue i)
	{
		//押した時にture、離した時にfalseが入る
		HoldButtonFlag = i.isPressed;

		if (i.isPressed && !SpecialSuccessFlag)
		{
			AttackInputFunc(2);
		}
		//特殊攻撃成功時に派生を選ぶ
		else if (SpecialSuccessFlag)
		{
			SpecialInputIndex = 2;
		}

		//スケベ脱出カウントアップ
		if (BreakInputFlag)
		{
			BreakCount++;
		}
	}

	//オートコンボボタンを押した時
	private void OnPlayerAutoCombo(InputValue i)
	{
		AutoComboInput = i.isPressed;
	}

	//特殊攻撃を押した時
	private void OnPlayerSpecial(InputValue i)
	{
		//特殊攻撃入力許可条件判定
		if (PermitInputBoolDic["SpecialTry"])
		{
			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//入力フラグを立てる
			SpecialInput = true;
		}
		

		//スケベフラグを戻す処理、とりあえずなので後で消す
		foreach (var ii in CostumeRootOBJ.GetComponentsInChildren<Transform>())
		{
			if (ii.name.Contains("TopsOff"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = false;
			}
			else if (ii.name.Contains("TopsOn"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = true;
			}
		}

		foreach (var ii in CostumeRootOBJ.GetComponentsInChildren<Transform>())
		{
			if (ii.name.Contains("PantsOff"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = false;
			}
			else if (ii.name.Contains("PantsOn"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = true;
			}
		}

		TopsOffFlag = false;

		PantsOffFlag = false;

		//モザイク表示
		MosaicOBJ.SetActive(false);
	}

	//超必殺技ボタンを押した時の処理
	private void OnPlayerSuperArts(InputValue i)
	{
		//超必殺技入力許可条件判定
		if (PermitInputBoolDic["SuperTry"] && B_Gauge >= SuperGauge)
		{
			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//入力フラグを立てる
			SuperInput = true;
		}
	}

	//入力フラグを全て下す関数
	private void InputReset()
	{
		SpecialInput = false;

		SuperInput = false;

		AttackInput = false;

		JumpInput = false;

		RollingInput = false;
	}

	//入力系遷移フラグを全て下す関数
	private void TransitionReset()
	{
		//Damage遷移フラグを下す
		CurrentAnimator.SetBool("Damage", false);

		//Rolling遷移フラグを下す
		CurrentAnimator.SetBool("Rolling", false);

		//Jump遷移フラグを下す
		CurrentAnimator.SetBool("Jump", false);

		//Special遷移フラグを下す
		CurrentAnimator.SetBool("SpecialTry", false);

		//Super遷移フラグを下す
		CurrentAnimator.SetBool("SuperTry", false);

		//Attack遷移フラグを下す
		CurrentAnimator.SetBool("Attack00", false);
		CurrentAnimator.SetBool("Attack01", false);
	}

	//キャラクター移動処理
	private void MoveFunc()
	{
		//移動ベクトル用ダミーの座標をベクトル取得位置に移動
		PlayerMoveAxis.transform.position = new Vector3(MainCameraTransform.position.x, transform.position.y, MainCameraTransform.position.z);

		//ダミーをプレイヤーに向ける
		PlayerMoveAxis.transform.LookAt(transform.position);

		//強制移動ベクトル初期化
		ForceMoveVector *= 0;

		//敵接触フラグ初期化
		EnemyContactFlag = false;

		//空中にいる時は重力加速度を減らし続ける、物理的に正しい加速度だとふんわりしすぎるので2倍
		if (!OnGroundFlag)
		{
			GravityAcceleration += Physics.gravity.y * 2 * Time.deltaTime;
		}

		//急斜面にいる時の処理
		if (OnSlopeFlag)
		{
			//接地面の法線をベクトルにして強制移動値に入れる
			ForceMoveVector += RayHit.normal * PlayerMoveSpeed;

			//Y軸ベクトルにマイナスを入れて落下
			ForceMoveVector.y = -Mathf.Abs(ForceMoveVector.y);
		}

		//敵接触判定処理
		if (!HoldFlag && !SpecialAttackFlag && !H_Flag && !SuperFlag)
		{
			//全てのアクティブな敵を回す
			foreach (GameObject i in GameManagerScript.Instance.AllActiveEnemyList.Where(e => e != null).ToList())
			{
				//近くにいたら処理
				if (HorizontalVector(gameObject, i).sqrMagnitude < 1f && gameObject.transform.position.y - i.transform.position.y < 1f && gameObject.transform.position.y - i.transform.position.y > -0.1f)
				{
					//敵接触フラグを立てる
					EnemyContactFlag = true;

					//敵と自分までのベクトルで強制移動
					ForceMoveVector += Controller.transform.position - new Vector3(i.transform.position.x, transform.position.y, i.transform.position.z);
				}
			}
		}

		//スケベ中の移動制御
		if (CurrentState.Contains("H_"))
		{
			//敵に合わせて移動
			HorizonAcceleration = H_MoveVector * PlayerDashSpeed * 2;

			//敵に合わせて回転
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(H_RotateVector), TurnSpeed * Time.deltaTime);
		}
		//ダメージ中の移動処理
		else if (CurrentState.Contains("Damage"))
		{			
			//ダメージモーションから受け取った移動値を入れる
			HorizonAcceleration = DamageMoveVector;
		}
		//踏み外し中の処理
		else if (CurrentState.Contains("Drop"))
		{
			//移動値を徐々に減速
			HorizonAcceleration *= 0.95f;
		}
		//特殊攻撃中の移動処理
		else if (CurrentState.Contains("Special"))
		{
			//特殊攻撃移動値
			HorizonAcceleration = SpecialMoveVector;

			//ロックしている敵に向ける
			if (LockEnemy != null && !NoRotateFlag)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(HorizontalVector(LockEnemy, gameObject)), TurnSpeed * Time.deltaTime);
			}
		}
		//超必殺技中の移動処理
		else if (CurrentState.Contains("Super"))
		{
			//超必殺技移動値
			HorizonAcceleration = SuperMoveVector;

			//ロックしている敵に向ける
			if (LockEnemy != null && !NoRotateFlag)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(HorizontalVector(LockEnemy, gameObject)), TurnSpeed * Time.deltaTime);
			}
		}
		//攻撃中の移動制御
		else if (CurrentState.Contains("Attack"))
		{
			/*
			AttackMoveType 技の移動タイプ
			0：地上で完結する移動、踏み外しの対象、
			1：空中で完結する移動、
			2：地上から空中に上がる移動
			3：空中から地上に降りる移動
			4：地上突進移動、相手に当たるとその場で止まる、踏み外しの対象、
			5：空中突進移動、相手に当たるとその場で止まる
			*/

			//ロックしている敵に向ける
			if (LockEnemy != null && !NoRotateFlag)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(HorizontalVector(LockEnemy, gameObject)), TurnSpeed * 2 * Time.deltaTime);
			}

			//敵に接触している時は前方に動かさない
			if ((AttackMoveType == 0 || AttackMoveType == 2 || AttackMoveType == 4 || AttackMoveType == 5) && EnemyContactFlag && ((AttackMoveVector.z / transform.forward.z) > 0))
			{
				AttackMoveVector.x = 0;
				AttackMoveVector.z = 0;

				SuperMoveVector *= 0;
			}

			//空中攻撃移動制御
			if (AttackMoveType == 1 || AttackMoveType == 2 || AttackMoveType == 3 || AttackMoveType == 5)
			{
				//垂直方向の加速度で重力を打ち消して攻撃加速度を入れる
				VerticalAcceleration = AttackMoveVector.y - GravityAcceleration;
			}
			else if(GroundDistance < 0.1f)
			{
				//ある程度踏み外しを防ぐために地面に沿ったベクトルを入れる
				VerticalAcceleration = Vector3.ProjectOnPlane(AttackMoveVector, RayHit.normal).y;
			}

			//踏み外し判定
			if ((AttackMoveType == 0 || AttackMoveType == 4) && GroundDistance > 0.5f)
			{
				//踏み外し遷移フラグを立てる
				CurrentAnimator.SetBool("Drop", true);
			}

			//水平方向の攻撃移動値を反映
			HorizonAcceleration = AttackMoveVector;
		}
		//ローリング中の移動制御
		else if (CurrentState.Contains("Rolling"))
		{
			//ローリングの移動方向に向ける
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(RollingRotateVector), TurnSpeed * Time.deltaTime);

			transform.rotation = Quaternion.LookRotation(RollingRotateVector);

			//ローリングの移動値を加える
			HorizonAcceleration = RollingMoveVector * (RollingSpeed + PlayerDashSpeed * PlayerMoveBlend);

			//接触している敵に向かってローリングしていたら移動値をリセット
			if (EnemyContactFlag && Vector3.Angle(ForceMoveVector, RollingMoveVector) > 175)
			{
				HorizonAcceleration *= 0;
			}
		}
		//地上移動制御
		else if (CurrentState.Contains("Run") ||  CurrentState.Contains("Stop"))
		{
			//ダミーのベクトルとプレイヤーからの入力で移動ベクトルを求める
			HorizonAcceleration = (PlayerMoveAxis.transform.forward * PlayerMoveInputVecter.y) + (PlayerMoveAxis.transform.right * PlayerMoveInputVecter.x);

			//入力があったら進行方向に向けて回転
			if (HorizonAcceleration != Vector3.zero)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(HorizonAcceleration, Vector3.up), TurnSpeed * Time.deltaTime);
			}

			//一定時間走っていたらダッシュに移行
			if (Time.time - DashInputTime > 5 && !DashFlag && CurrentState.Contains("Run"))
			{
				//フラグ状態をまっさらに戻す関数呼び出し
				ClearFlag();

				//ダッシュフラグを立てる
				DashFlag = true;

				//ブレンド比率初期値
				PlayerMoveBlend = 0;
			}

			//急停止
			if (CurrentState.Contains("Stop"))
			{
				//移動補正値を減らしていく
				PlayerMoveParam -= 2 * Time.deltaTime;

				//ベクトルを正面に固定して減速
				HorizonAcceleration = transform.forward * (PlayerMoveSpeed + PlayerMoveBlend * PlayerDashSpeed) * Mathf.Clamp01(PlayerMoveParam);
			}
			else
			{
				//ダッシュ速度補正値
				PlayerMoveParam = 1;

				//ユーザー入力による移動ベクトルに移動スピードを加える
				HorizonAcceleration = HorizonAcceleration * (PlayerMoveSpeed + PlayerMoveBlend * PlayerDashSpeed) * Mathf.Clamp01(PlayerMoveParam);
			}				
		}
		//空中移動制御
		else if(CurrentState.Contains("Jump") || CurrentState.Contains("Fall"))
		{
			//入力ベクトルキャッシュ
			Vector3 tempVector = (PlayerMoveAxis.transform.forward * PlayerMoveInputVecter.y) + (PlayerMoveAxis.transform.right * PlayerMoveInputVecter.x);

			//ジャンプ開始直後のベクトルを反映
			HorizonAcceleration = JumpHorizonVector;

			//ジャンプ開始直後のベクトルと入力ベクトルの角度を比較して前じゃなければ動かす
			if ((Vector3.Angle(tempVector , JumpHorizonVector) > 45) || JumpHorizonVector == Vector3.zero)
			{
				//ダミーのベクトルとプレイヤーからの入力で移動ベクトルを求める
				HorizonAcceleration += tempVector * PlayerMoveSpeed * 0.75f;
			}

			//ジャンプ方向に向ける
			if(JumpRotateVector != Vector3.zero && CurrentState.Contains("Jump"))
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(JumpRotateVector), TurnSpeed * 2 * Time.deltaTime);
			}
		}
		//何もしていなければその場に止まる
		else
		{
			HorizonAcceleration *= 0;
		}

		//水平方向の加速度を反映
		MoveVector = HorizonAcceleration * Time.deltaTime;

		//垂直方向の加速度を反映
		MoveVector.y = (GravityAcceleration + VerticalAcceleration) * Time.deltaTime;

		//強制移動ベクトルを加算、敵との接触や崖際とか動く床とか
		if (ForceMoveVector != Vector3.zero)
		{
			MoveVector += ForceMoveVector * PlayerMoveSpeed * Time.deltaTime;
		}
	}

	//毎フレーム呼ばなくてもいい処理
	private void DelayFunc()
	{
		//１秒毎に処理が走る
		if (Time.time - BlinkDelayTime > 1)
		{
			//現在時間をキャッシュ
			BlinkDelayTime = Time.time;

			//瞬きアニメーション
			if (Mathf.FloorToInt(UnityEngine.Random.Range(0f, 5f)) == 0 && !NoBlinkFlag)
			{
				//瞬きコルーチン呼び出し
				StartCoroutine(WinkCoroutine());
			}
		}
	}

	//瞬きコルーチン
	IEnumerator WinkCoroutine()
	{
		//瞬きレイヤーの重み
		float tempnum = 0;

		//瞬きが終わるまでループ
		while (tempnum < 1)
		{
			//カウントアップ
			tempnum += Time.deltaTime * 10;

			//目のアニメーションレイヤーの重みを変更にする
			CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Eye"), tempnum);

			//瞬き禁止フラグが立ったらブレイク
			if (NoBlinkFlag)
			{
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//ちょっと閉じたまま待機
		yield return new WaitForSeconds(0.05f);

		//瞬きが終わるまでループ
		while (tempnum > 0)
		{
			//カウントアップ
			tempnum -= Time.deltaTime * 10;

			//目のアニメーションレイヤーの重みを変更にする
			CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Eye"), tempnum);

			//瞬き禁止フラグが立ったらブレイク
			if (NoBlinkFlag)
			{
				break;
			}

			//1フレーム待機
			yield return null;
		}

		//目のアニメーションレイヤーの重み0にする
		CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Eye"), 0);
	}

	//たまに口を閉じる処理、アニメーションクリップから呼ばれる
	public void MouthClose(float i)
	{
		//引数は開閉頻度になる
		if(UnityEngine.Random.Range(0f, 1f) <= i)
		{
			StartCoroutine(MouthCloseCoroutine());
		}
	}
	IEnumerator MouthCloseCoroutine()
	{
		//現在のブレンド比率を取得
		float TempBlend = CurrentAnimator.GetLayerWeight(CurrentAnimator.GetLayerIndex("Mouth"));

		//開いてたら閉じる
		if (TempBlend < 0.5f)
		{
			while (TempBlend < 1)
			{
				//１フレーム待機
				yield return null;

				//ブレンド比率変更
				TempBlend += Time.deltaTime * 2.5f;

				//口のアニメーションレイヤーの重みを変更
				CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Mouth"), TempBlend);

				//遷移したらブレイク
				if (CurrentAnimator.IsInTransition(0))
				{
					break;
				}
			}
		}
		//閉じてたら開く
		else
		{
			while (TempBlend > 0)
			{
				//１フレーム待機
				yield return null;

				//ブレンド比率変更
				TempBlend -= Time.deltaTime * 10;

				//口のアニメーションレイヤーの重みを変更
				CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Mouth"), TempBlend);

				//遷移したらブレイク
				if (CurrentAnimator.IsInTransition(0))
				{
					break;
				}
			}
		}
	}

	//立ち構え切り替えコルーチン
	IEnumerator IdlingChangeCoroutine()
	{
		//現在のブレンド比率をキャッシュ
		float BlendRatio = CurrentAnimator.GetFloat("Idling_Blend");

		//構えと立ち切り替え、敵がいる時は構え
		if (FightingFlag)
		{
			//ブレンド比率が1になるまで繰り返し
			while (BlendRatio < 1)
			{
				//ブレンド比率をチョイ上げ
				BlendRatio += 0.05f;

				//ブレンド比率を適応
				CurrentAnimator.SetFloat("Idling_Blend", BlendRatio);

				//1フレーム待機
				yield return null;
			}
		}
		//敵がいない時は立ち
		else
		{
			//ブレンド比率が0になるまで繰り返し
			while (BlendRatio > 0)
			{
				//ブレンド比率をチョイ下げ
				BlendRatio -= 0.1f;

				//ブレンド比率を適応
				CurrentAnimator.SetFloat("Idling_Blend", BlendRatio);

				//1フレーム待機
				yield return null;
			}
		}
	}

	//当たった攻撃が有効かを返す関数
	public bool AttackEnable(bool H)
	{
		//現在が無敵ステートか調べる
		bool re = !(InvincibleList.Any(t => CurrentState.Contains(t)) || (Time.time - JumpTime < 0.25f) || SuperInput);

		//スケベ攻撃だった場合は攻撃コライダが有効なら喰らわない
		if (re && H)
		{
			re = !AttackColOBJ.GetComponent<BoxCollider>().enabled;
		}

		//出力
		return re;
	}

	//敵の攻撃が当たった時の処理
	public void HitEnemyAttack(EnemyAttackClass arts, GameObject enemy)
	{
		//当身に当たった
		if (SpecialTryFlag && SpecialArtsList[0].Trigger == 0)
		{
			//特殊攻撃待機フラグを下す
			SpecialTryFlag = false;

			//アニメーターの遷移フラグを立てる
			CurrentAnimator.SetBool("SpecialSuccess", true);

			//特殊攻撃成功コルーチン呼び出し
			StartCoroutine(SpecialArtsSuccess(enemy));
		}
		//普通に喰らった
		else
		{
			//ライフを減らす
			L_Gauge -= arts.Damage;

			if (L_Gauge <= 0)
			{
				//アニメーターの遷移フラグを立てる
				CurrentAnimator.SetBool("Down", true);
			}

			//攻撃用コライダ無効化
			AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

			//接地を判別、ongroundじゃなくアニメーターのフラグを使う
			if (CurrentAnimator.GetBool("Fall"))
			{
				//空中用ダメージモーションに切り替える
				for (int i = 0; i <= DamageAnimList.Count - 1; i++)
				{
					if (DamageAnimList[i].name.Contains("10"))
					{
						//空中ダメージインデックスを反映
						DamageAnimIndex = i;

						//ループを抜ける
						break;
					}
				}
			}
			//接地していたら
			else
			{
				//ダメージモーションインデクスに反映
				DamageAnimIndex = arts.AttackType;
			}

			//オーバーライドコントローラにアニメーションクリップをセット
			OverRideAnimator["Damage_void"] = DamageAnimList[DamageAnimIndex];

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

			//アニメーションフラグを立てる
			CurrentAnimator.SetBool("Damage", true);

			//これ以上イベントを起こさないためにAttackステートを一時停止
			CurrentAnimator.SetFloat("AttackSpeed00", 0.0f);
			CurrentAnimator.SetFloat("AttackSpeed01", 0.0f);

			//攻撃してきた敵の方を向く
			transform.rotation = Quaternion.LookRotation(HorizontalVector(enemy, gameObject));
		}
	}

	//スケベ攻撃が当たった時の処理
	public void H_AttackHit(string ang , int men ,GameObject M_Enemy , GameObject S_Enemy)
	{
		//スケベフラグを立てる
		H_Flag = true;

		//スケベ状況を代入
		H_Location = ang + men;

		//スケベ攻撃をしてきた敵代入
		H_MainEnemy = M_Enemy;

		//スケベ攻撃をしてきた敵の近くにいた敵代入
		H_SubEnemy = S_Enemy;

		//顔テクスチャを頬テクスチャに差し替え
		FaceMaterial.SetTexture("_TexFaceBase", FaceCheekTex);

		//アニメーターのフラグを立てる
		CurrentAnimator.SetBool("H_Hit", true);

		//これ以上イベントを起こさないためにAttackステートを一時停止
		CurrentAnimator.SetFloat("AttackSpeed00", 0.0f);
		CurrentAnimator.SetFloat("AttackSpeed01", 0.0f);

		//攻撃用コライダ無効化
		AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

		//敵のクロス用コライダを自身のクロスオブジェクトに設定
		foreach (var i in gameObject.GetComponentsInChildren<Cloth>())
		{
			//代入用List宣言
			List<CapsuleCollider> tempList = new List<CapsuleCollider>();

			//ListにコライダをAdd
			foreach(var ii in M_Enemy.GetComponentsInChildren<Transform>())
			{
				if(ii.gameObject.name.Contains("Cloth"))
				{
					tempList.Add(ii.gameObject.GetComponent<CapsuleCollider>());
				}
			}

			//複数人ならそいつのコライダもAdd
			if (H_SubEnemy != null)
			{
				foreach (var ii in H_SubEnemy.GetComponentsInChildren<Transform>())
				{
					if (ii.gameObject.name.Contains("Cloth"))
					{
						tempList.Add(ii.gameObject.GetComponent<CapsuleCollider>());
					}
				}
			}

			//クロスのコライダに反映
			i.capsuleColliders = tempList.ToArray();
		}

		//オーバーライドコントローラにアニメーションクリップをセット
		OverRideAnimator["H_Hit_void"] = H_HitAnimList.Where(a => a.name.Contains(H_Location)).ToList()[0];

		//オーバーライドコントローラにアニメーションクリップをセット
		OverRideAnimator["H_Damage_" + H_State % 2 + "_void"] = H_DamageAnimList.Where(a => a.name.Contains(H_Location)).ToList()[0];

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

		//アニメーション遷移フラグを立てる
		CurrentAnimator.SetBool("H_Damage0" + H_State % 2, true);

		//キャラクターのスケベ回転値
		H_RotateVector = ang == "Back" ? H_MainEnemy.transform.forward : -H_MainEnemy.transform.forward;

		//スケベカメラワーク再生
		H_CameraOBJ.GetComponent<CinemachineCameraScript>().PlayCameraWork(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(H_Location + "_Hit")).ToList()[0]) , true);

		//次のカメラワーク設定
		H_CameraOBJ.GetComponent<CinemachineCameraScript>().SpecifyIndex = H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.IndexOf(H_CameraOBJ.GetComponent<CinemachineCameraScript>().CameraWorkList.Where(a => a.name.Contains(H_Location + "_Damage")).ToList()[0]);

		//スケベ攻撃位置合わせコルーチン呼び出し
		StartCoroutine(H_PositionSetting(ang));
	}

	//スケベ攻撃を喰らった時に位置を合わせるコルーチン
	IEnumerator H_PositionSetting(string ang)
	{
		float Dis = 0.25f;

		//ループ制御bool
		bool loopbool = true;

		//時間取得
		float t = Time.time;

		while (loopbool)
		{
			//キャラクターのスケベ移動ベクトル設定
			H_MoveVector = (H_MainEnemy.transform.position + H_MainEnemy.transform.forward * Dis) - gameObject.transform.position;

			//指定の位置まで移動したらフラグを下ろしてループを抜ける、保険として1秒経過で抜ける
			if (H_MoveVector.sqrMagnitude < 0.0001f || (t + 1 < Time.time))
			{
				loopbool = false;
			}

			//1フレーム待機
			yield return null;
		}

		//スケベ移動ベクトル初期化
		H_MoveVector *= 0;
	}

	//特殊攻撃が成功した時の処理
	IEnumerator SpecialArtsSuccess(GameObject enemy)
	{
		//対象の敵をロック
		LockEnemy = enemy;

		//メインカメラにもロック対象を渡す
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

		//特殊攻撃成功フラグを立てる
		SpecialSuccessFlag = true;

		//特殊攻撃インデックスを初期化
		SpecialInputIndex = 100;

		//現在時間をキャッシュ
		float SuccessTime = Time.time;

		//フェードエフェクト呼び出し
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), SpecialSelectTime, (GameObject obj) => { }));

		//ズームエフェクト呼び出し
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.025f, SpecialSelectTime, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));

		//特殊攻撃成功エフェクト生成
		GameObject TempEffect = Instantiate(SpecialSuccessEffect);

		//親を設定
		TempEffect.transform.parent = DeepFind(gameObject, SpecialArtsList[0].EffectPos).transform;

		//ローカルポジションリセット
		TempEffect.transform.localPosition = Vector3.zero;

		//時間をゆっくりにして入力待ち
		GameManagerScript.Instance.TimeScaleChange(0, 0.05f);
		
		//待機時間が経過するか入力があるまで待機
		while (SpecialSuccessFlag)
		{
			//有効な入力があったらgotoでブレーク
			foreach (SpecialClass i in SpecialArtsList)
			{
				if (i.ArtsIndex == SpecialInputIndex)
				{
					goto SpecialLoopBreak;
				}
			}

			//入力受付時間が過ぎたらとりあえず0を出す？どうしよう
			if(CurrentState == "SpecialSuccess" && CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > SpecialSelectTime)
			{
				SpecialInputIndex = 0;

				goto SpecialLoopBreak;
			}

			//1フレーム待機
			yield return null;
		}

		//ループを抜ける先
		SpecialLoopBreak:;

		//タイムスケールを戻す
		GameManagerScript.Instance.TimeScaleChange(0.1f, 1);

		//フラグを下す
		SpecialSuccessFlag = false;

		//オーバーライドコントローラにアニメーションクリップをセット		
		OverRideAnimator["Special_void"] = SpecialArtsList.Where(a => a.ArtsIndex == SpecialInputIndex).ToList()[0].AnimClip;

		//アニメーターを上書きしてアニメーションクリップを切り替える
		CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

		//アニメーターの遷移フラグを立てる
		CurrentAnimator.SetBool("SpecialAttack", true);
	}

	//特殊攻撃待機フラグを下す、アニメーションクリップから呼ばれる
	public void EndSpecialTry()
	{
		SpecialTryFlag = false;
	}

	//特殊攻撃実行、アニメーションクリップから呼ばれる
	public void SpecialArtsAction(int n)
	{
		//特殊攻撃処理を実行
		SpecialArtsList.Where(a => a.ArtsIndex == SpecialInputIndex).ToList()[0].SpecialAtcList[n](gameObject , LockEnemy , SpecialArtsList.Where(a => a.ArtsIndex == SpecialInputIndex).ToList()[0]);
	}

	//超必殺技処理実行、アニメーションクリップから呼ばれる
	public void SuperArtsAction(int n)
	{
		//超必殺技処理を実行
		SuperArts.SuperActList[SuperCount](gameObject, LockEnemy);

		//超必殺技カウントアップ
		SuperCount++;
	}

	//超必殺技カメラワーク再生、アニメーションクリップから呼ばれる
	public void StartSuperArtsCameraWork(int i)
	{
		//カメラワーク再生
		SuperCameraWorkOBJ.GetComponent<CinemachineCameraScript>().PlayCameraWork(0, true);
	}
	//超必殺技カメラワークを次に進める
	public void NextSuperArtsCameraWork()
	{
		//カメラワークを進める
		SuperCameraWorkOBJ.GetComponent<CinemachineCameraScript>().KeepCameraFlag = false;
	}

	//ダメージ時の移動ベクトルを設定する、アニメーションクリップから呼ばれる
	public void StartDamageMove(float s)
	{
		//攻撃移動加速度をリセット
		AttackMoveVector *= 0;

		//ローリング移動加速度をリセット
		RollingMoveVector *= 0;

		//引数が0なら空中ダメージなので上に浮かす
		if (s == 0)
		{
			//垂直方向の加速度をリセット
			VerticalAcceleration = 0f;

			//重力加速度をリセット
			GravityAcceleration = 0;

			//ちょい浮かす
			VerticalAcceleration = 8f - GravityAcceleration;
		}
		else
		{
			//水平方向の加速度を反映
			DamageMoveVector = transform.forward * s;
		}
	}

	//ダメージ時の移動ベクトルを初期化する、アニメーションクリップから呼ばれる
	public void EndDamageMove()
	{
		DamageMoveVector *= 0;
	}

	//ダメージフラグを下す、アニメーションクリップから呼ばれる
	public void EndDamage()
	{
		//ダメージフラグを下す
		DamageFlag = false;

		//表情を戻す
		ChangeFace("Reset");
	}

	//立ちモーションループ
	void StandLoop(float t)
	{
		CurrentAnimator.Play("Idling", 0, t);
	}

	//ローリングのスピードを変える、アニメーションクリップのイベントから呼ばれる
	private void RollingMoveSpeed(float s)
	{
		RollingSpeed = s;
	}

	//攻撃入力受付関数
	private void AttackInputFunc(int b)
	{
		//攻撃入力許可条件判定
		if (PermitInputBoolDic["Attack00"] || PermitInputBoolDic["Attack01"])
		{
			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//攻撃入力フラグを立てる
			AttackInput = true;

			//入力ボタンをキャッシュ
			AttackButton = b;

			//スティック入力をキャッシュ
			if (PlayerMoveInputVecter == Vector2.zero)
			{
				//スティック入力無し
				AttackStick = 0;
			}
			else
			{
				//スティック入力有り
				AttackStick = 1;
			}

			//ロック中の敵がいなければ敵を管理をするマネージャーにロック対象の敵を探させる
			if (LockEnemy == null)
			{
				ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => LockEnemy = reciever.SearchLockEnemy(true, HorizonAcceleration));
			}
		}
	}

	//技選定関数
	private ArtsClass SelectionArts()
	{
		//出力用変数宣言
		ArtsClass ReserveArts = null;

		//ロック対象がいる場合
		if (LockEnemy != null)
		{
			//ロック対象との距離を測定、浮かした時に遠距離判定にならないよう高低差は考慮しない
			EnemyDistance = HorizontalVector(LockEnemy, gameObject).magnitude;
		}
		else
		{
			//いなければ距離はゼロにしとく
			EnemyDistance = 0;
		}

		//ロケーションを判定
		if (!OnGroundFlag)
		{
			//空中にいる場合
			AttackLocation = 2;
		}
		else if (EnemyDistance < AttackDistance)
		{
			//地上にいてロックした敵が近い場合
			AttackLocation = 0;
		}
		else
		{
			//地上にいてロックした敵が遠い場合
			AttackLocation = 1;
		}

		//オートコンボしている
		if (AutoComboInput)
		{
			//チェイン中はとりあえずチェイン続行
			if (CurrentAnimator.GetBool("Chain"))
			{
				ReserveArts = UseArts;

				//入力情報を更新
				AttackLocation = UseArts.UseLocation["Location"];
				AttackStick = UseArts.UseLocation["Stick"];
				AttackButton = UseArts.UseLocation["Button"];
			}
			//地上で入力
			else if (AttackLocation != 2)
			{
				//入力ボタンを初期化してループ
				for (int iii = 0; iii <= 2; iii++)
				{
					//距離を初期化してループ
					for (int i = 0; i <= 1; i++)
					{
						//レバー入れを初期化してループ
						for (int ii = 0; ii <= 1; ii++)
						{
							//使用済み判定
							if (ArtsStateMatrix[i][ii][iii] != 10000 && ArtsMatrix[i][ii][iii] != null)
							{
								//入力情報を更新
								AttackLocation = i;
								AttackStick = ii;
								AttackButton = iii;

								//技確定
								ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];

								//ループを抜ける
								goto ArtsLoopBreak;
							}
						}
					}
				}
			}
			//空中で入力
			else
			{
				//入力ボタンを初期化してループ
				for (int iii = 0; iii <= 2; iii++)
				{
					//レバー入れを初期化してループ
					for (int ii = 0; ii <= 1; ii++)
					{
						//使用済み判定
						if (ArtsStateMatrix[AttackLocation][ii][iii] != 10000 && ArtsMatrix[AttackLocation][ii][iii] != null)
						{
							//入力情報を更新
							AttackStick = ii;
							AttackButton = iii;

							//技確定
							ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];

							//ループを抜ける
							goto ArtsLoopBreak;
						}
					}
				}
			}

			//ループを抜ける先
			ArtsLoopBreak:;
		}
		//入力された技が存在する
		else if (ArtsMatrix[AttackLocation][AttackStick][AttackButton] != null)
		{
			//入力された技をそのまま使用
			ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];
		}
		//入力された技が存在しなければコンボアシスト
		else
		{
			//地上技
			if (AttackLocation != 2)
			{
				//レバー入れを反転して判定
				if (ArtsMatrix[AttackLocation][Mathf.Abs(AttackStick - 1)][AttackButton] != null)
				{
					//レバー入力判定を反転
					AttackStick = Mathf.Abs(AttackStick - 1);

					//使用する技確定
					ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];
				}
				//それも無ければ距離を反転して判定
				else if (ArtsMatrix[Mathf.Abs(AttackLocation - 1)][AttackStick][AttackButton] != null)
				{
					//敵との距離判定を反転
					AttackLocation = Mathf.Abs(AttackLocation - 1);

					//使用する技確定
					ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];
				}
				//それも無ければレバー入れと距離を反転して判定
				else if (ArtsMatrix[Mathf.Abs(AttackLocation - 1)][Mathf.Abs(AttackStick - 1)][AttackButton] != null)
				{
					//レバー入力判定を反転
					AttackStick = Mathf.Abs(AttackStick - 1);

					//敵との距離判定を反転
					AttackLocation = Mathf.Abs(AttackLocation - 1);

					//使用する技確定
					ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];
				}
			}
			//空中技はレバー入力を反転して調べる
			else if (ArtsMatrix[AttackLocation][Mathf.Abs(AttackStick - 1)][AttackButton] != null)
			{
				//レバー入力判定を反転
				AttackStick = Mathf.Abs(AttackStick - 1);

				//使用する技確定
				ReserveArts = ArtsMatrix[AttackLocation][AttackStick][AttackButton];
			}
		}

		//技が見つかったら入力時のロケーションを保存
		if (ReserveArts != null)
		{
			ReserveArts.UseLocation["Location"] = AttackLocation;
			ReserveArts.UseLocation["Stick"] = AttackStick;
			ReserveArts.UseLocation["Button"] = AttackButton;
		}

		//出力
		return ReserveArts;
	}

	//回転禁止フラグ制御、突進攻撃で振り向かせたくない場合など、アニメーションクリップのイベントから呼ばれる
	public void RotateControl(int b)
	{
		//引数でフラグを切り替える
		NoRotateFlag = b == 0 ? false : true;
	}

	//ループ攻撃処理、、アニメーションクリップのイベントから呼ばれる
	private void AttackLoop(float i)
	{
		//ボタン押しっぱなしで先行入力されていない
		if(HoldButtonFlag && !AttackInput)
		{
			//空中技で低空じゃない
			if(!(UseArts.LocationFlag == 2 && GroundDistance < 0.75f))
			{
				//ループ実行
				CurrentAnimator.Play("Attack0" + (ComboState + 1) % 2, 0, i);
			}
		}
	}

	//タメ攻撃処理、アニメーションクリップのイベントから呼ばれる
	private void AttackCharge(int i)
	{
		//ボタンが長押しされていなければ処理をスルー
		if (HoldButtonFlag)
		{
			//コルーチン呼び出し
			StartCoroutine(AttackChargeCoroutine(i));
		}
	}
	IEnumerator AttackChargeCoroutine(int i)
	{
		//イベント発生を止めるためモーション再生時間を止める
		CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0f);

		//タメ開始時間をキャッシュ
		float ChargeTime = Time.time;

		//モーションの再生位置をキャッシュ
		float AnimTime = CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;

		//タメループエフェクトのインスタンスを生成
		GameObject TempChargePowerEffect = Instantiate(ChargePowerEffect);

		//キャラクターの子にする
		TempChargePowerEffect.transform.parent = gameObject.transform.root.transform;

		//ローカル座標で位置を設定
		TempChargePowerEffect.transform.localPosition *= 0;

		//いったん非アクティブにする
		TempChargePowerEffect.SetActive(false);

		//ボタン押しっぱなし、もしくはポーズ中ループ
		while ((HoldButtonFlag && !PauseFlag && !DamageFlag && !H_Flag) || PauseFlag)
		{
			//ポーズ中は経過時間を相殺してタメが進まないようにする
			if (PauseFlag)
			{
				ChargeTime += Time.time - ChargeTime;
			}

			//モーションの再生位置を交互に変えてプルプルさせる
			if (CurrentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < AnimTime)
			{
				CurrentAnimator.Play("Attack0" + (ComboState + 1) % 2, 0, AnimTime);
			}
			else
			{
				CurrentAnimator.Play("Attack0" + (ComboState + 1) % 2, 0, AnimTime - 0.0025f);
			}

			//ジャンプかローリングが押されたらループを抜ける
			if (JumpInput || RollingInput)
			{
				break;
			}

			//チャージ1段階完了処理
			if ((Time.time - ChargeTime > 0.5f) && ChargeLevel == 0)
			{
				//チャージレベルをあげる
				ChargeLevel = 1;

				//揺れ物バタバタ関数呼び出し
				StartClothShake(1);

				//タメループエフェクトをアクティブにする
				TempChargePowerEffect.SetActive(true);

				//タメ完了エフェクトのインスタンスを生成
				GameObject TempChargeLevelEffect = Instantiate(ChargeLevelEffect);

				//2段階目のエフェクトを消す
				DeepFind(TempChargeLevelEffect, "ChargeLevel2").SetActive(false);

				//3段階目のエフェクトを消す
				DeepFind(TempChargeLevelEffect, "ChargeLevel3").SetActive(false);

				//キャラクターの子にする
				TempChargeLevelEffect.transform.parent = gameObject.transform.root.transform;

				//ローカル座標で位置を設定
				TempChargeLevelEffect.transform.localPosition *= 0;

				//足元衝撃エフェクト表示
				FootImpact(90);

				//フェードエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), 0.25f, (GameObject obj) => { }));

				//ズームエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.025f, 0.25f, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));
			}
			//チャージ2段階完了処理
			else if ((Time.time - ChargeTime > 2.0f) && ChargeLevel == 1)
			{
				//チャージレベルをあげる
				ChargeLevel = 2;

				//揺れ物バタバタ関数呼び出し
				StartClothShake(2);

				//タメループエフェクトの段階をあげる
				DeepFind(TempChargePowerEffect, "ChargePower2").GetComponent<ParticleSystem>().Play();

				//タメ完了エフェクトのインスタンスを生成
				GameObject TempChargeLevelEffect = Instantiate(ChargeLevelEffect);

				//3段階目のエフェクトを消す
				DeepFind(TempChargeLevelEffect, "ChargeLevel3").SetActive(false);

				//キャラクターの子にする
				TempChargeLevelEffect.transform.parent = gameObject.transform.root.transform;

				//ローカル座標で位置を設定
				TempChargeLevelEffect.transform.localPosition *= 0;

				//足元衝撃エフェクト表示
				FootImpact(90);

				//フェードエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), 0.25f, (GameObject obj) => { }));

				//ズームエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.025f, 0.25f, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));
			}
			//チャージ3段階完了処理
			else if ((Time.time - ChargeTime > 5.0f) && ChargeLevel == 2)
			{
				//チャージレベルをあげる
				ChargeLevel = 3;

				//揺れ物バタバタ関数呼び出し
				StartClothShake(3);

				//タメループエフェクトの段階をあげる
				DeepFind(TempChargePowerEffect, "ChargePower3").GetComponent<ParticleSystem>().Play();

				//タメ完了エフェクトのインスタンスを生成
				GameObject TempChargeLevelEffect = Instantiate(ChargeLevelEffect);

				//キャラクターの子にする
				TempChargeLevelEffect.transform.parent = gameObject.transform.root.transform;

				//ローカル座標で位置を設定
				TempChargeLevelEffect.transform.localPosition *= 0;

				//足元衝撃エフェクト表示
				FootImpact(90);

				//フェードエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), 0.25f, (GameObject obj) => { }));

				//ズームエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.025f, 0.25f, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));

				//画面揺らし
				ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.CameraShake(10000f));
			}

			//入力時と違うボタンが押されたらタメ解除
			if (UseArts.UseLocation["Button"] != AttackButton)
			{
				HoldButtonFlag = false;
			}

			//ちょっと待機、毎フレームだとプルプルが早すぎるのでこんくらい
			yield return new WaitForSeconds(0.05f);
		}

		//タメループエフェクトを止める
		foreach (ParticleSystem ii in TempChargePowerEffect.GetComponentsInChildren<ParticleSystem>())
		{
			//タメループエフェクトをアクティブにする、これをしないと以下の処理が走らずオブジェクトが消えない
			TempChargePowerEffect.SetActive(true);

			//アクセサを取り出す
			ParticleSystem.MainModule tempMain = ii.main;

			//ループを止めてパーティクルの発生を停止
			tempMain.loop = false;
		}

		//画面揺らし止める
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.CameraShake(0f));

		//揺れ物バタバタを止める
		EndClothShake();

		//ジャンプかローリングが押されたら攻撃中断
		if (JumpInput || RollingInput || DamageFlag)
		{
			//チャージレベル初期化
			ChargeLevel = 0;

			//攻撃ボタン押しっぱなしフラグを下げる
			HoldButtonFlag = false;

			//イベント発生を止めるためモーション再生時間を止める
			CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0f);

			//遷移可能フラグを立てる
			CurrentAnimator.SetBool("Transition", true);
		}
		else
		{
			//モーション再生時間を戻して発射
			CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 1.0f);
		}
	}

	//攻撃終了処理、アニメーションクリップのイベントから呼ばれる
	private void EndAttack()
	{
		//遷移可能フラグを立てる
		CurrentAnimator.SetBool("Transition", true);

		//使用技を破棄
		UseArts = null;
	}	

	//攻撃移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartAttackMove(int n)
	{
		//ごくまれにnullが入るのでエラー回避
		if (UseArts == null)
		{
			print("StartAttackMoveでUseArtsがNull");
		}
		else
		{
			//使用中の技から移動値を求める、引数で使用する移動値リストのインデックスが入ってくる
			AttackMoveVector = UseArts.MoveVec[n].a * ((UseArts.MoveVec[n].r * Controller.transform.right) + (UseArts.MoveVec[n].g * Controller.transform.up) + (UseArts.MoveVec[n].b * Controller.transform.forward));

			//使用中の技から移動タイプを求める、引数で使用する移動値リストのインデックスが入ってくる
			AttackMoveType = UseArts.MoveType[n];
		}
	}

	//攻撃移動終了処理、アニメーションクリップのイベントから呼ばれる
	public void EndAttackMove()
	{
		//移動値をゼロにする
		AttackMoveVector *= 0;

		//移動タイプを初期化する
		AttackMoveType = 100;
	}

	//超必殺技移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartSuperMove(float n)
	{
		SuperMoveVector = transform.forward * n;
	}

	//超必殺技移動終了処理、アニメーションクリップのイベントから呼ばれる
	private void EndSuperMove()
	{
		//移動ベクトル初期化
		SuperMoveVector *= 0;
	}

	//攻撃コライダ移動開始処理、アニメーションクリップのイベントから呼ばれる
	private void StartAttackCol(int n)
	{
		//攻撃用コライダーのコライダ移動関数呼び出し、インデックスとコライダ移動タイプを渡す
		ExecuteEvents.Execute<PlayerAttackCollInterface>(AttackColOBJ, null, (reciever, eventData) => reciever.ColStart(n, UseArts));
	}

	//攻撃コライダ終了処理、アニメーションクリップのイベントから呼ばれる
	private void EndAttackCol()
	{
		//コライダ移動終了処理関数呼び出し
		ExecuteEvents.Execute<PlayerAttackCollInterface>(AttackColOBJ, null, (reciever, eventData) => reciever.ColEnd());
	}

	//超必殺技コライダ出現処理、アニメーションクリップのイベントから呼ばれる
	private void StartSuperCol(string pos)
	{
		//受け取った引数をfloatのListに入れる
		List<float> PosList = new List<float>(pos.Split(',').Select(a => float.Parse(a)));

		//超必殺技コライダ処理関数呼び出し
		ExecuteEvents.Execute<PlayerAttackCollInterface>(AttackColOBJ, null, (reciever, eventData) => reciever.StartSuperCol(new Vector3(PosList[0], PosList[1], PosList[2]), new Vector3(PosList[3], PosList[4], PosList[5])));
	}

	//超必殺技暗転演出、アニメーションクリップのイベントから呼ばれる
	private void StartSuperArtsLightEffect(float t)
	{
		//超必殺技暗転演出呼び出し
		GameManagerScript.Instance.StartSuperArtsLightEffect(t);
	}
	private void EndSuperArtsLightEffect(float t)
	{
		//超必殺技暗転演出呼び出し
		GameManagerScript.Instance.EndSuperArtsLightEffect(t);
	}

	//超必殺技時間停止演出
	private void SuperArtsStopEffect(float t)
	{
		GameManagerScript.Instance.SuperArtsStopEffect(t, LockEnemy);
	}

	//画面揺らし演出、アニメーションクリップのイベントから呼ばれる
	private void ScreenShake(float t)
	{
		//画面揺らし
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.CameraShake(t));
	}

	//攻撃時のエフェクトを再生する、アニメーションクリップのイベントから呼ばれる
	private void AttackEffect(String n)
	{
		//引数で受け取った名前のエフェクトのインスタンスを生成
		GameObject TempAttackEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == n).ToArray()[0]);

		//キャラクターの子にする
		TempAttackEffect.transform.parent = gameObject.transform.root.transform;

		//ローカル座標で位置を設定
		TempAttackEffect.transform.localPosition *= 0;

		//回転値をリセット
		TempAttackEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);

		//タメ攻撃の場合エフェクトをでかくする
		if(ChargeLevel > 0)
		{
			//エフェクトのアクセサを取り出す
			ParticleSystem.MainModule Accesser = TempAttackEffect.GetComponent<ParticleSystem>().main;

			//チャージレベルを掛けて弾をでかくする
			Accesser.startSize = Accesser.startSize.constant * (ChargeLevel * 0.5f + 1);
		}
	}

	//攻撃モーション再生速度変更処理、アニメーションクリップのイベントから呼ばれる
	private void AttackSpeedChange(int n)
	{
		//非同期処理をするためのコルーチン呼び出し
		StartCoroutine(AttackSpeedChangetCoroutine(n));
	}
	IEnumerator AttackSpeedChangetCoroutine(int n)
	{
		//経過時間を初期化
		ChainAttackWaitTime = 0;

		//nullチェック
		if (UseArts == null)
		{
			print("AttackSpeedChangeでUseArtがnull");
		}
		else
		{
			//速度変更タイプ判定：チェイン攻撃
			if (UseArts.TimeType[n] == 0)
			{
				//現在のステートのモーション再生時間を変更、スロー再生
				CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.2f);

				//接地状況でチェインブレイクのモーションを切り替える
				CurrentAnimator.SetFloat("ChainBreakBlend", OnGroundFlag ? 0 : 1);

				//遷移可能フラグを立てる
				CurrentAnimator.SetBool("Transition", true);

				//入力待機状態ループ
				for (; ; )
				{
					//攻撃が入力されていたらUseArtsのと比較
					if (AttackInput && SelectionArts() != null)
					{
						//同じ技ならチェイン攻撃続行
						if (UseArts.NameC == SelectionArts().NameC)
						{
							//攻撃入力フラグを下す
							AttackInput = false;

							//遷移可能フラグを下ろす
							CurrentAnimator.SetBool("Transition", false);

							//次の遷移フラグを下ろす
							CurrentAnimator.SetBool("Attack0" + ComboState % 2, false);

							//ループを抜けてチェイン続行
							break;
						}
						//違う技ならチェインを切って次の技へ
						else
						{
							//これ以上イベントを起こさないために現在のステートを一時停止
							CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.0f);

							//チェインフラグを下ろす
							CurrentAnimator.SetBool("Chain", false);

							//次の遷移フラグを立てる
							CurrentAnimator.SetBool("Attack0" + ComboState % 2, true);

							//攻撃終了処理関数
							EndAttack();

							//コルーチンを抜ける
							yield break;
						}
					}
					//ローリングかジャンプか特殊攻撃かダメージででチェイン中断
					else if ((RollingInput && AirRollingFlag) || (JumpInput && OnGroundFlag) || SpecialInput || SuperInput || DamageFlag || H_Flag)
					{
						//これ以上イベントを起こさないために現在のステートを一時停止
						CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.0f);

						//チェインフラグを下ろす
						CurrentAnimator.SetBool("Chain", false);

						//攻撃入力フラグを下す
						AttackInput = false;

						//コルーチンを抜ける
						yield break;
					}
					//入力受付時間を過ぎたらブレイク
					else if (ChainAttackWaitTime > 0.5f)
					{
						//これ以上イベントを起こさないために現在のステートを一時停止
						CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.0f);

						//チェインフラグを下ろす
						CurrentAnimator.SetBool("Chain", false);

						//チェインブレイクフラグを立てる
						CurrentAnimator.SetBool("ChainBreak", true);

						//攻撃入力フラグを下す
						AttackInput = false;

						//移動値をリセット
						EndAttackMove();

						//コルーチンを抜ける
						yield break;
					}

					//経過時間カウントアップ
					ChainAttackWaitTime += Time.deltaTime;

					//1フレーム待機
					yield return null;
				}
			}
			//速度変更タイプ判定：降下攻撃
			else if (UseArts.TimeType[n] == 1)
			{
				//現在のステートのモーション再生時間を変更、完全停止
				CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.0f);

				//接地するまでループ
				while (!OnGroundFlag)
				{
					//1フレーム待機
					yield return null;
				}
			}

			//モーション再生時間を戻す
			CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 1.0f);
		}
	}

	//プレイヤーの攻撃が敵に当たった時の処理
	public void HitAttack(GameObject e, int AttackIndex)
	{
		//超必殺技が当たった
		if(CurrentState.Contains("SuperTry"))
		{
			//超必殺技遷移フラグを立てる
			CurrentAnimator.SetBool("SuperArts", true);

			//当たった敵をロックする
			LockEnemy = e;

			//攻撃対象ポーズ解除
			if (e != null)
			{
				ExecuteEvents.Execute<EnemyCharacterInterface>(e, null, (reciever, eventData) => reciever.Pause(false));
				ExecuteEvents.Execute<EnemyBehaviorInterface>(e, null, (reciever, eventData) => reciever.Pause(false));
			}

			//攻撃が当たった時の敵側の処理を呼び出す
			ExecuteEvents.Execute<EnemyCharacterInterface>(e.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.SuperArtsHit(CharacterID, SuperArts.Down));

			//敵をプレイヤーキャラクターに向ける
			e.transform.LookAt(new Vector3(gameObject.transform.position.x, e.transform.position.y, gameObject.transform.position.z));

			//メインカメラの敵ロックを外す
			ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(null));

			//メインカメラの超必殺技中フラグを立てる
			ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetSuperArtsFlag(true));
		}
		else
		{
			//バリバリゲージ増加
			SetB_Gauge(0.1f);

			//ヒットSEを鳴らす
			{
				foreach (var i in AttackImpactOBJ.GetComponents<SoundEffectScript>())
				{
					if (i.AudioName.Contains(UseArts.HitSE[AttackIndex]))
					{
						i.PlayRandomList();
					}
				}
			}

			//巻き込み攻撃でなければ当たった敵をロックする
			if (UseArts.ColType[AttackIndex] != 7 && UseArts.ColType[AttackIndex] != 8)
			{
				LockEnemy = e;
			}

			//メインカメラにもロック対象を渡す
			ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

			//攻撃ステート制御リストに使用済み数値を入れる
			ArtsStateMatrix[UseArts.UseLocation["Location"]][UseArts.UseLocation["Stick"]][UseArts.UseLocation["Button"]] = 10000;

			//ヒットストップに値が入っていたら演出
			if (UseArts.HitStop[AttackIndex] != 0)
			{
				//ヒットストップ処理
				ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(UseArts.HitStop[AttackIndex], 0.1f));
			}

			//地上突進技が当たったらその場で停止させる
			if (AttackMoveType == 4)
			{
				AttackMoveVector *= 0;
			}
			//ある程度の高度で空中突進技が当たった
			else if (AttackMoveType == 5 && GroundDistance > 1.25f)
			{
				//これ以上イベントを起こさないために現在のステートを一時停止
				CurrentAnimator.SetFloat("AttackSpeed0" + (ComboState + 1) % 2, 0.0f);

				//攻撃を強制終了
				EndAttack();

				//攻撃が先行入力されていなければ空中ローリング
				if (!AttackInput)
				{
					//ローリングの正面を設定
					RollingRotateVector = transform.forward;

					//ローリングフラグを立てて強制的にローリング実行
					CurrentAnimator.SetBool("Rolling", true);
				}
			}
			//引き起こし攻撃が当たった
			else if (UseArts.AttackType[AttackIndex] == 30)
			{
				//強制移動ベクトル初期化
				ForceMoveVector *= 0;

				//ホールド状態フラグを立てる
				HoldFlag = true;

				//回転制御フラグを立てる
				NoRotateFlag = true;

				//当たった敵の方を向く
				transform.rotation = Quaternion.LookRotation(HorizontalVector(LockEnemy, gameObject));

				//ホールド状態維持コルーチン呼び出し
				StartCoroutine(KeepHold());
			}

			//敵を倒したらロックを外す処理
			if (LockEnemy != null)
			{
				//返り値受け取り用変数
				bool tempbool = false;

				//攻撃を当てた敵から死亡フラグを受け取る
				ExecuteEvents.Execute<EnemyCharacterInterface>(LockEnemy, null, (reciever, eventData) => tempbool = reciever.GetDestroyFlag());

				//死んでたら
				if (tempbool)
				{
					//敵のロックを外す	
					LockEnemy = null;

					//メインカメラのロックも外す
					ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));
				}
			}

			//タメ攻撃がフルチャージ
			if (ChargeLevel == 3)
			{
				//ヒットストップ処理
				ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(0.5f, 0.1f));

				//フェードエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), 0.25f, (GameObject obj) => { }));

				//ズームエフェクト呼び出し
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.05f, 0.25f, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));
			}
		}
	}

	//瞬き禁止、アニメーションクリップのイベントから呼ばれる
	private void NoBlink()
	{
		//瞬き禁止フラグを立てる
		NoBlinkFlag = true;
	}

	//バリバリゲージセット関数
	public void SetB_Gauge(float i)
	{
		//満タンエフェクトフラグ
		bool EffectFlag = false;

		//ゲージが満タンじゃない
		if(B_Gauge < 1)
		{
			EffectFlag = true;
		}

		//ゲージ増減
		B_Gauge += i;

		//0~1の範囲に収める
		if (B_Gauge > 1 || B_Gauge < 0)
		{
			B_Gauge = Mathf.Round(B_Gauge);
		}

		//バリバリゲージが満タンになった
		if (B_Gauge == 1 && EffectFlag) 
		{
			//エフェクト再生
			GameObject TempEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "SuperArtsStopTimeEffect").ToArray()[0]);
			TempEffect.transform.position = gameObject.transform.position;
		}

		//バリバリゲージが下がるほどアソコも緩む
		CurrentAnimator.SetFloat("Vagina_Blend", 1 - B_Gauge);　
	}

	//表情を変える、アニメーションクリップのイベントから呼ばれる
	private void ChangeFace(string n)
	{
		//瞬き禁止フラグを下ろす
		NoBlinkFlag = false;

		//普通の表情に戻す
		if (n == "Reset")
		{
			//クロスフェードで表情切り替え
			CurrentAnimator.CrossFadeInFixedTime("FaceBase", 0.5f, 1);
		}
		//指定された表情に変える
		else
		{
			//使用するアニメーションクリップ名
			string AnimName = n.Split(',').ToList()[0];

			//遷移時間
			float DurationTime = float.Parse(n.Split(',').ToList()[1]);

			//オーバーライドコントローラにアニメーションクリップをセット
			OverRideAnimator["Face_Void_" + FaceState % 2] = GameManagerScript.Instance.AllFaceList.Where(a => a.name == AnimName).ToArray()[0];

			//アニメーターを上書きしてアニメーションクリップを切り替える
			CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

			//クロスフェードで表情切り替え
			CurrentAnimator.CrossFadeInFixedTime("Face_" + FaceState % 2, DurationTime, 1);

			//表情ステート変数カウントアップ
			FaceState++;
		}
	}

	//チェイン攻撃の最後の１発の発生時にチェインフラグを切る、アニメーションクリップのイベントから呼ばれる
	private void ChainEnd()
	{
		//チェイン攻撃フラグを下ろす
		CurrentAnimator.SetBool("Chain", false);
	}

	//攻撃時のTrailを表示する、アニメーションクリップのイベントから呼ばれる
	private void StartAttackTrail(int n)
	{
		//引数で受け取った番号のエフェクトを再生
		DeepFind(AttackTrail, "AttackTrail" + n).GetComponent<ParticleSystem>().Play();
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

	//揺れ物バタバタ関数
	public void StartClothShake(int p)
	{
		//コルーチン呼び出し
		StartCoroutine(ClothShakeCoroutine(p));
	}

	private IEnumerator ClothShakeCoroutine(int p)
	{
		//多重処理を防ぐ為に一度フラグを下す
		ClothShakeFlag = false;

		//フラグ反映の為に１フレーム待機
		yield return null;

		//フラグを立てる
		ClothShakeFlag = true;

		//クロスList
		List<Cloth> tempClothList = new List<Cloth>(GetComponentsInChildren<Cloth>());

		//ダイナミックボーンList
		List<DynamicBone> tempBoneList = new List<DynamicBone>(transform.GetComponentsInChildren<DynamicBone>().Where(a => !a.m_Root.name.Contains("Breast")));

		//クロスにベクトルを与える
		foreach (Cloth i in tempClothList)
		{
			i.randomAcceleration = new Vector3(0, 200, 0);

			i.randomAcceleration *= p;
		}

		//フラグが降りるまで力を与え続ける
		while (ClothShakeFlag)
		{
			//DynamicBoneにベクトルを与える
			foreach (DynamicBone i in tempBoneList)
			{
				i.m_Force.x = UnityEngine.Random.Range(-0.002f, 0.002f);
				i.m_Force.y = UnityEngine.Random.Range(0, 0.01f);
				i.m_Force.z = UnityEngine.Random.Range(-0.002f, 0.002f);

				i.m_Force *= p;
			}

			//１フレーム待機
			yield return null;
		}

		//クロスに力を加えるのをやめる
		foreach (Cloth i in tempClothList)
		{
			i.randomAcceleration = new Vector3(0, 0, 0);
		}

		//DynamicBoneに力を加えるのをやめる
		foreach (DynamicBone i in tempBoneList)
		{
			i.m_Force *= 0;
		}
	}

	//揺れ物バタバタ止め関数
	public void EndClothShake()
	{
		//フラグを下す
		ClothShakeFlag = false;
	}

	//足元の煙エフェクトを表示する、アニメーションクリップのイベントから呼ばれる
	private void FootSmoke(float t)
	{
		//コルーチン呼び出し
		StartCoroutine(FootSmokeCoroutine(t));
	}
	IEnumerator FootSmokeCoroutine(float t)
	{
		//開始時間をキャッシュ
		float SmokeTime = Time.time;

		//エフェクトのインスタンスを生成
		GameObject TempFootSmokeEffect = Instantiate(FootSmokeEffect);

		//キャラクターの子にする
		TempFootSmokeEffect.transform.parent = gameObject.transform.root.transform;

		//ローカル座標で位置を設定
		TempFootSmokeEffect.transform.localPosition *= 0;

		//エフェクト再生
		TempFootSmokeEffect.GetComponent<ParticleSystem>().Play();

		//引数で受け取った秒数だけ待機
		while (Time.time < SmokeTime + t)
		{
			yield return null;

			//ポーズされたら経過時間を足していく
			if (PauseFlag)
			{
				SmokeTime += Time.deltaTime;
			}
		}

		//メインモジュールのアクセサ取得
		ParticleSystem.MainModule TempMainModule = TempFootSmokeEffect.GetComponent<ParticleSystem>().main;

		//ループを切ってエフェクトを止める、こうしないとポーズした時に変な事になる
		TempMainModule.loop = false;
	}

	//ホールド状態を解除する、アニメーションクリップのイベントから呼ばれる。
	public void HoldBreak()
	{
		if (LockEnemy != null)
		{
			//敵のホールド解除インターフェイス呼び出し
			ExecuteEvents.Execute<EnemyCharacterInterface>(LockEnemy, null, (reciever, eventData) => reciever.HoldBreak(0.1f));

			//ホールドブレイクコルーチン呼び出し、フラグを下ろすのを少し待つ
			StartCoroutine(HoldBreakCoroutine());
		}
	}
	//ホールドブレイクコルーチン
	IEnumerator HoldBreakCoroutine()
	{
		//チョイ待機
		yield return new WaitForSeconds(0.1f);

		//ホールドフラグを下ろす
		HoldFlag = false;
	}

	//トップスをはだける、アニメーションクリップから呼ばれる
	public void TopsOff()
	{
		//モデルの表示切り替え
		foreach (var ii in CostumeRootOBJ.GetComponentsInChildren<Transform>())
		{
			if (ii.name.Contains("TopsOff"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = true;
			}
			else if (ii.name.Contains("TopsOn"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = false;
			}
		}

		//スケベエフェクト生成
		GameObject TempEffect = Instantiate(H_Effect00);

		//親を設定
		TempEffect.transform.parent = gameObject.transform;

		//ローカルPRSリセット
		TempEffect.transform.localPosition = Vector3.up;
		TempEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);

		//スローモーション
		GameManagerScript.Instance.TimeScaleChange(0.5f, 0.5f);
	}

	//パンツを下ろす、アニメーションクリップから呼ばれる
	public void PantsOff()
	{
		//モデルの表示切り替え
		foreach (var ii in CostumeRootOBJ.GetComponentsInChildren<Transform>())
		{
			if (ii.name.Contains("PantsOff"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = true;
			}
			else if (ii.name.Contains("PantsOn"))
			{
				ii.GetComponent<SkinnedMeshRenderer>().enabled = false;
			}
		}

		//モザイク表示
		MosaicOBJ.SetActive(true);

		//スケベエフェクト生成
		GameObject TempEffect = Instantiate(H_Effect00);

		//親を設定
		TempEffect.transform.parent = gameObject.transform;

		//ローカルPRSリセット
		TempEffect.transform.localPosition = Vector3.up;
		TempEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);

		//スローモーション
		GameManagerScript.Instance.TimeScaleChange(0.5f, 0.5f);
	}

	//スケベ状態を解除する、アニメーションクリップから呼ばれる
	public void H_Break()
	{
		//後ろ１人
		if(H_Location.Contains("Back"))
		{
			//ヒットエフェクトインスタンス生成
			GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect00").ToList()[0]);

			//プレイヤーの子にする
			HitEffect.transform.parent = gameObject.transform;

			//PRS設定
			HitEffect.transform.localPosition = new Vector3(0, 0.75f, -0.5f);
			HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 0));

			//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
			ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, -7.5f, 0.1f) }, new List<int>() { 0 }, new List<int>() { 1 }), 0));
		}
		//前１人
		else if(H_Location.Contains("Forward"))
		{
			//ヒットエフェクトインスタンス生成
			GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect01").ToList()[0]);

			//プレイヤーの子にする
			HitEffect.transform.parent = gameObject.transform;

			//PRS設定
			HitEffect.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
			HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(45, 0, 0));

			//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
			ExecuteEvents.Execute<EnemyCharacterInterface>(H_MainEnemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 7.5f, 0.1f) }, new List<int>() { 0 }, new List<int>() { 4 }), 0));
		}

		//敵のクロス用コライダを解除する
		foreach (var i in gameObject.GetComponentsInChildren<Cloth>())
		{
			//クロスのコライダに反映
			i.capsuleColliders = new List<CapsuleCollider>().ToArray();
		}

		//スケベカメラ無効化、このままじゃ階段とかで別のヴァーチャルカメラが有効な時に上手くいかないのでとりあえず
		H_CameraOBJ.GetComponent<CinemachineCameraScript>().KeepCameraFlag = false;
	}

	//視線を直接指定する、アニメーションクリップから呼ばれる
	public void DirectEye(string i)
	{
		//視線ダイレクトモード有効化
		ExecuteEvents.Execute<CharacterEyeShaderScriptInterface>(EyeOBJ, null, (reciever, eventData) => reciever.SetDirectMode(true));

		//引数をカンマで分割してFloatにキャスト
		List<float> templist = new List<float>(i.Split(',').ToList().Select(a => float.Parse(a)));

		//スムースダンプベロシティ
		EyeOBJ.GetComponent<CharacterEyeShaderScript>().DirectEyeVelocity = 0;

		//タイリング
		EyeOBJ.GetComponent<CharacterEyeShaderScript>().DirectEyeTiling = new Vector2(templist[0], templist[1]);

		//オフセット
		EyeOBJ.GetComponent<CharacterEyeShaderScript>().DirectEyeOffset = new Vector2(templist[2], templist[3]);
	}
	//視線を自動にする、アニメーションクリップから呼ばれる
	public void AutoEye()
	{
		//視線ダイレクトモード無効化
		ExecuteEvents.Execute<CharacterEyeShaderScriptInterface>(EyeOBJ, null, (reciever, eventData) => reciever.SetDirectMode(false));
	}

	//技格納マトリクス初期化関数
	private void ArtsMatrixSetUp()
	{
		//技格納マトリクス初期化
		List<ArtsClass> ArtButtonList00 = new List<ArtsClass>();
		List<ArtsClass> ArtButtonList01 = new List<ArtsClass>();
		List<ArtsClass> ArtButtonList02 = new List<ArtsClass>();
		List<ArtsClass> ArtButtonList03 = new List<ArtsClass>();
		List<ArtsClass> ArtButtonList04 = new List<ArtsClass>();
		List<ArtsClass> ArtButtonList05 = new List<ArtsClass>();

		List<List<ArtsClass>> ArtStickList00 = new List<List<ArtsClass>>();
		List<List<ArtsClass>> ArtStickList01 = new List<List<ArtsClass>>();
		List<List<ArtsClass>> ArtStickList02 = new List<List<ArtsClass>>();

		ArtButtonList00.Add(null);
		ArtButtonList00.Add(null);
		ArtButtonList00.Add(null);

		ArtButtonList01.Add(null);
		ArtButtonList01.Add(null);
		ArtButtonList01.Add(null);

		ArtButtonList02.Add(null);
		ArtButtonList02.Add(null);
		ArtButtonList02.Add(null);

		ArtButtonList03.Add(null);
		ArtButtonList03.Add(null);
		ArtButtonList03.Add(null);

		ArtButtonList04.Add(null);
		ArtButtonList04.Add(null);
		ArtButtonList04.Add(null);

		ArtButtonList05.Add(null);
		ArtButtonList05.Add(null);
		ArtButtonList05.Add(null);

		ArtStickList00.Add(ArtButtonList00);
		ArtStickList00.Add(ArtButtonList01);
		ArtStickList01.Add(ArtButtonList02);
		ArtStickList01.Add(ArtButtonList03);
		ArtStickList02.Add(ArtButtonList04);
		ArtStickList02.Add(ArtButtonList05);

		ArtsMatrix.Add(ArtStickList00);
		ArtsMatrix.Add(ArtStickList01);
		ArtsMatrix.Add(ArtStickList02);

		//全ての技をnullにする
		for (int i = 0; i <= ArtsMatrix.Count - 1; i++)
		{
			for (int ii = 0; ii <= ArtsMatrix[i].Count - 1; ii++)
			{
				for (int iii = 0; iii <= ArtsMatrix[i][ii].Count - 1; iii++)
				{
					ArtsMatrix[i][ii][iii] = null;
				}
			}
		}

		//UserDataから技を装備
		for (int i = 0; i <= GameManagerScript.Instance.UserData.ArtsMatrix[CharacterID].Count - 1; i++)
		{
			for (int ii = 0; ii <= GameManagerScript.Instance.UserData.ArtsMatrix[CharacterID][i].Count - 1; ii++)
			{
				for (int iii = 0; iii <= GameManagerScript.Instance.UserData.ArtsMatrix[CharacterID][i][ii].Count - 1; iii++)
				{
					if (GameManagerScript.Instance.UserData.ArtsMatrix[CharacterID][i][ii][iii] != "")
					{
						foreach (var iiii in GameManagerScript.Instance.AllArtsList)
						{
							if (iiii.NameC == GameManagerScript.Instance.UserData.ArtsMatrix[CharacterID][i][ii][iii])
							{
								ArtsMatrix[i][ii][iii] = iiii;
							}
						}
					}
				}
			}
		}
	}

	//攻撃制御マトリクス初期化関数
	private void ArtsStateMatrixReset()
	{
		//コンボステート初期化
		ComboState = 0;

		//リストを新しく作って
		ArtsStateMatrix = new List<List<List<int>>>();

		//技一覧を元にAdd
		foreach (int i in Enumerable.Range(0, ArtsMatrix.Count))
		{
			List<List<int>> iList = new List<List<int>>();

			foreach (int ii in Enumerable.Range(0, ArtsMatrix[i].Count))
			{
				List<int> iiList = new List<int>();

				foreach (int iii in Enumerable.Range(0, ArtsMatrix[i][ii].Count))
				{
					iiList.Add(iii);
				}

				iList.Add(iiList);
			}

			ArtsStateMatrix.Add(iList);
		}
	}

	//武器をセットする
	public void SetWeapon(GameObject w)
	{
		WeaponOBJ = w;
	}

	//攻撃時に武器のアタッチ先を変更する
	private void AttackAttachWeapon(string s)
	{
		//アタッチ先を変更
		WeaponOBJ.transform.parent = DeepFind(gameObject, s).transform;

		//ローカルトランスフォームを設定
		WeaponOBJ.transform.localPosition *= 0;
		WeaponOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);
	}

	//武器のミラーに敵の顔を映す
	private void EnemyFaceMirror()
	{
		//一応nullチェック
		if(LockEnemy != null)
		{
			//武器ミラーの処理呼び出し
			WeaponOBJ.GetComponentInChildren<MirrorShaderScript>().EnemyFaceMirror(DeepFind(LockEnemy, "HeadBone"));
		}		
	}
	//武器のミラーを切る
	private void WeaponMirrorOff()
	{
		//武器ミラーの処理呼び出し
		WeaponOBJ.GetComponentInChildren<MirrorShaderScript>().MirrorSwitch(false);
	}

	//接地判定用のRayを飛ばす関数
	private void GroundRayCast()
	{
		//レイを飛ばして接地判定をしてフラグを切り替える
		if (Physics.SphereCast(transform.position + (Vector3.up * (RayRadius.x + 1f)), RayRadius.x, Vector3.down, out RayHit, Mathf.Infinity, LayerMask.GetMask("TransparentFX")))
		{
			//地面との距離に値を入れる
			GroundDistance = RayHit.distance - 1f;

			//急斜面判定
			OnSlopeFlag = Vector3.Angle(RayHit.normal, Vector3.up) > 60 && Vector3.Angle(RayHit.normal, Vector3.up) < 85;

			if (GroundDistance < 0.01f || Controller.isGrounded)
			{
				OnGroundFlag = true;
			}
			else
			{
				OnGroundFlag = false;
			}

			//地面属性を取る
			if (GroundSurface != RayHit.collider.tag)
			{
				GroundSurface = RayHit.collider.tag;
			}
		}
		else
		{
			OnGroundFlag = false;
		}

		//急斜面判定
		OnSlopeFlag = Vector3.Angle(RayHit.normal, Vector3.up) > 60 && Vector3.Angle(RayHit.normal, Vector3.up) < 85 && OnGroundFlag;
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

	//視線を決定するコルーチン
	IEnumerator SetCharacterLookAtPos()
	{
		//ロックしている敵がいたらそいつ、複数の敵が要る場合はランダムな相手、いなければ正面を見る
		if (LockEnemy != null)
		{
			CharacterLookAtPos = LockEnemy.transform.position;
		}
		else if (GameManagerScript.Instance.AllActiveEnemyList.Where(e => e != null).ToList().Count > 0)
		{
			//視線変更許可フラグで制御
			if (LookAtPosFlag)
			{
				//視線変更許可フラグを下ろす
				LookAtPosFlag = false;

				//敵リストからランダムに対象を選定、座標を送る
				CharacterLookAtPos = GameManagerScript.Instance.AllActiveEnemyList.Where(e => e != null).OrderBy(i => Guid.NewGuid()).ToArray()[0].transform.position;

				//ランダムな秒数待機
				yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 5f));

				//視線変更許可フラグを立てる
				LookAtPosFlag = true;
			}
		}
		else
		{
			CharacterLookAtPos = gameObject.transform.position + gameObject.transform.forward;
		}
	}

	//キャラクターアニメーション制御処理
	private void AnimFunc()
	{
		//視線を決定するコルーチン呼び出し
		StartCoroutine(SetCharacterLookAtPos());

		//目シェーダーに視線ベクトルを送る
		ExecuteEvents.Execute<CharacterEyeShaderScriptInterface>(EyeOBJ, null, (reciever, eventData) => reciever.GetLookPos(CharacterLookAtPos));

		//移動入力がしきい値以上ならアニメーターのフラグを立てる
		CurrentAnimator.SetBool("Move", Mathf.Abs(PlayerMoveInputVecter.x) + Mathf.Abs(PlayerMoveInputVecter.y) > 0.0f);
		
		//移動中のモーションブレンド
		if (CurrentAnimator.GetBool("Move") && !CurrentState.Contains("Attack"))
		{
			//ダッシュフラグでブレンド比率を加減する
			PlayerMoveBlend = DashFlag ? Mathf.Clamp01(PlayerMoveBlend += Time.deltaTime) : Mathf.Clamp01(PlayerMoveBlend -= Time.deltaTime);

			//ブレンド比率でモーション再生速度を加減する
			CurrentAnimator.SetFloat("MoveSpeed", (Mathf.Clamp01(CurrentAnimator.GetFloat("Move_Blend") - 1) * 0.5f) + 1);

			//移動値によってモーションブレンド比率を変える
			CurrentAnimator.SetFloat("Move_Blend", PlayerMoveBlend + Mathf.Clamp01(Mathf.Abs(PlayerMoveInputVecter.x) + Mathf.Abs(PlayerMoveInputVecter.y)));
		}
		else if (!CurrentState.Contains("Run") && !CurrentState.Contains("Stop"))
		{
			//移動していなければブレンド比率を初期化
			PlayerMoveBlend = 0;
		}

		//Fall制御：
		CurrentAnimator.SetBool("Fall",
			//攻撃しておらず、ある程度の高さがある、下り坂でFallにならないようにするやつ
			(GroundDistance > 0.5f && !CurrentAnimator.GetBool("Combo")) ||
			//攻撃中に全く接地していない
			(CurrentAnimator.GetBool("Combo") && !OnGroundFlag) ||
			//急斜面にいる
			OnSlopeFlag);

		//Crouch制御：平地に接地している
		CurrentAnimator.SetBool("Crouch", OnGroundFlag && !OnSlopeFlag);

		//Crouch中は水平移動禁止
		if (CurrentAnimator.GetCurrentAnimatorStateInfo(0).IsName("Crouch"))
		{
			MoveVector.x = 0.0f;
			MoveVector.z = 0.0f;
		}

		//Rolling遷移判定
		if (PermitTransitionBoolDic["GroundRolling"] || PermitTransitionBoolDic["AirRolling"])
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("Rolling", true);
		}

		//Jump遷移判定
		if (PermitTransitionBoolDic["Jump"])
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("Jump", true);
		}

		//Special遷移判定
		if (PermitTransitionBoolDic["SpecialTry"])
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("SpecialTry", true);
		}

		//Super遷移判定
		if (PermitTransitionBoolDic["SuperTry"])
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("SuperTry", true);

			//ロック中の敵がいなければ敵を管理をするマネージャーにロック対象の敵を探させる
			if (LockEnemy == null)
			{
				ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => LockEnemy = reciever.SearchLockEnemy(true, HorizonAcceleration));
			}
		}

		//Attack遷移判定
		if (PermitTransitionBoolDic["Attack00"] || PermitTransitionBoolDic["Attack01"])
		{
			//アニメーション遷移フラグを立てる
			CurrentAnimator.SetBool("Attack0" + ComboState % 2, true);
		}

		//ジャンプ中の処理
		if (CurrentState.Contains("Jump"))
		{
			//ダメージを受けてたら入れない、屈伸分のディレイをちょっと待つ
			if (!DamageFlag && Time.time - JumpTime > 0.1f)
			{
				//垂直加速度に値を入れる
				VerticalAcceleration = JumpPower;
			}
		}
		//地上ローリング中の処理
		else if (CurrentState.Contains("GroundRolling"))
		{
			/*
			//ローリング中に攻撃が入力されたら
			if (AttackInput && LockEnemy != null)
			{
				//ロックしている敵方向のベクトルを得る
				RollingRotateVector = HorizontalVector(LockEnemy, gameObject);
			}
			*/

			//ローリング移動ベクトルを入れる
			RollingMoveVector = transform.forward;
		}
		//空中ローリング中の処理
		else if (CurrentState.Contains("AirRolling"))
		{
			//ローリング移動ベクトルを入れる
			RollingMoveVector = transform.forward;
		}
		//スケベダメージ中の処理
		else if (CurrentState.Contains("H_Damage"))
		{
			//アニメーション再生速度にノイズを加える
			CurrentAnimator.SetFloat("H_Speed", Mathf.PerlinNoise(Time.time * 2.5f, -Time.time) + 0.5f);
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

			//フラグを更新
			FlagManager(CurrentState);
		}
	}
	//入力許可フラグを管理する関数、ステートが変化しないと更新されない事に注意
	private void FlagManager(string s)
	{
		//特殊攻撃入力許可条件
		PermitInputBoolDic["SpecialTry"]
		= !PauseFlag &&
		!H_Flag &&
		OnGroundFlag &&
		!ActionEventFlag &&
		(
			s.Contains("Attack")
			||
			s.Contains("Stop")
			||
			s == "Idling"
			||
			s == "Run"
			||
			s == "GroundRolling"
			||
			s.Contains("-> Idling")
			||
			s.Contains("-> Run")
			||
			s.Contains("-> GroundRolling")
		);

		//超必殺技入力許可条件
		PermitInputBoolDic["SuperTry"]
		= !PauseFlag &&
		SuperArts != null &&
		!H_Flag &&
		OnGroundFlag &&
		!ActionEventFlag &&
		(
			s.Contains("Attack")
			||
			s.Contains("Stop")
			||
			s == "Idling"
			||
			s == "Run"
			||
			s == "Jump"
			||
			s == "Fall"
			||
			s == "GroundRolling"
			||
			s == "AirRolling"
			||
			s == "SpecialSuccess"
			||
			s == "SpecialAttack"
			||
			s.Contains("-> Idling")
			||
			s.Contains("-> Run")
			||
			s.Contains("-> Jump")
			||
			s.Contains("-> Fall")
			||
			s.Contains("-> GroundRolling")
			||
			s.Contains("-> AirRolling")
		);

		//ジャンプ入力許可条件
		PermitInputBoolDic["Jump"]
		= !PauseFlag &&
		!H_Flag &&
		!ActionEventFlag &&
		(
			s.Contains("Attack")
			||
			s.Contains("Stop")
			||
			s == "Idling"
			||
			s == "Run"
			||
			s == "SpecialAttack"
			||
			s.Contains("-> Idling")
			||
			s.Contains("-> Run")
		);

		//ローリング入力許可ステート
		PermitInputBoolDic["GroundRolling"]
		= !PauseFlag &&
		!H_Flag &&
		!ActionEventFlag &&
		(
			s.Contains("Attack")
			||
			s.Contains("Stop")
			||
			s == "Idling"
			||
			s == "Run"
			||
			s == "Jump"
			||
			s == "Fall"
			||
			s == "SpecialAttack"
			||
			s.Contains("-> Idling")
			||
			s.Contains("-> Run")
			||
			s.Contains("-> Jump")
			||
			s.Contains("-> Fall")
		);

		//空中ローリング入力許可ステート
		PermitInputBoolDic["AirRolling"] = PermitInputBoolDic["GroundRolling"] && AirRollingFlag;

		//攻撃入力許可ステート
		PermitInputBoolDic["Attack00"]
		= !PauseFlag &&
		!H_Flag &&
		!NoEquipFlag &&
		!ActionEventFlag &&
		(
			s.Contains("Attack")
			||
			s.Contains("Stop")
			||
			s == "Idling"
			||
			s == "Run"
			||
			s == "Jump"
			||
			s == "Fall"
			||
			s == "GroundRolling"
			||
			s == "AirRolling"
			||
			s == "SpecialSuccess"
			||
			s == "SpecialAttack"
			||
			s.Contains("-> Idling")
			||
			s.Contains("-> Run")
			||
			s.Contains("-> Jump")
			||
			s.Contains("-> Fall")
			||
			s.Contains("-> GroundRolling")
			||
			s.Contains("-> AirRolling")
		);

		//攻撃入力許可ステート
		PermitInputBoolDic["Attack01"] = PermitInputBoolDic["Attack00"];

		//Idlingになった瞬間の処理
		if (s.Contains("-> Idling"))
		{
			//フラグ状態をまっさらに戻す関数呼び出し
			ClearFlag();
		}
		//Runになった瞬間の処理
		else if (s.Contains("-> Run"))
		{
			//フラグ状態をまっさらに戻す関数呼び出し
			ClearFlag();

			//入力時間を記録
			DashInputTime = Time.time;
		}

		//Crouchになった瞬間の処理
		else if (s.Contains("-> Crouch"))
		{
			//フラグ状態をまっさらに戻す関数呼び出し
			ClearFlag();
		}
		//Jumpになった瞬間の処理
		else if (s.Contains("-> Jump"))
		{
			//Jump入力フラグを下す
			JumpInput = false;

			//Jump遷移フラグを下す
			CurrentAnimator.SetBool("Jump", false);

			//ジャンプした瞬間に重力加速度をリセットする、接地でリセットすると着地がガクっとしてしまう
			GravityAcceleration = Physics.gravity.y * 2 * Time.deltaTime;

			//垂直加速度をリセット
			VerticalAcceleration = 0;

			//ジャンプ開始時間をキャッシュ
			JumpTime = Time.time;

			//ジャンプ開始直後の移動ベクトルをキャッシュ
			JumpHorizonVector = ((PlayerMoveAxis.transform.forward * PlayerMoveInputVecter.y) + (PlayerMoveAxis.transform.right * PlayerMoveInputVecter.x)) * (PlayerMoveSpeed + PlayerMoveBlend * PlayerDashSpeed) * Mathf.Clamp01(PlayerMoveParam);

			//ジャンプ回転値を入れる
			JumpRotateVector = JumpHorizonVector;
		}
		//Fallになった瞬間の処理
		else if (s.Contains("-> Fall"))
		{
			//Jump入力フラグを下す
			JumpInput = false;

			//Jump遷移フラグを下す
			CurrentAnimator.SetBool("Jump", false);

			//強制繊維フラグを下す
			CurrentAnimator.SetBool("ForceFall", false);
		}
		//Dropになった瞬間の処理
		else if (s.Contains("-> Drop"))
		{
			//踏み外し遷移フラグを下ろす
			CurrentAnimator.SetBool("Drop", false);

			//重力加速度をリセット
			GravityAcceleration = Physics.gravity.y * 2 * Time.deltaTime;

			//垂直加速度をリセット
			VerticalAcceleration = 0;

			//これ以上イベントを起こさないためにAttackステートを一時停止
			CurrentAnimator.SetFloat("AttackSpeed00", 0.0f);
			CurrentAnimator.SetFloat("AttackSpeed01", 0.0f);

			//コライダを非アクティブ化
			AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

			//表情を普通に戻す
			ChangeFace("Reset");

			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();
		}

		//Rollingになった瞬間の処理
		else if (s.Contains("-> GroundRolling") || s.Contains("-> AirRolling"))
		{
			//入力フラグを下す
			RollingInput = false;

			//Rolling遷移フラグを下す
			CurrentAnimator.SetBool("Rolling", false);

			//ホールド状態フラグを下す
			HoldFlag = false;

			//攻撃移動値をリセット
			AttackMoveVector *= 0;

			//攻撃移動タイプを初期化
			AttackMoveType = 100;

			//表情を普通に戻す
			ChangeFace("Reset");

			//空中ローリングの場合
			if (s.Contains("-> AirRolling"))
			{
				//ジャンプ開始時ベクトルをリセット
				JumpHorizonVector *= 0;

				//空中ローリング許可フラグを下ろす
				AirRollingFlag = false;

				//重力加速度をリセット
				GravityAcceleration = Physics.gravity.y * 2 * Time.deltaTime;

				//跳ねる
				VerticalAcceleration = JumpPower * 0.9f;
			}
		}
		//Attackになった瞬間の処理
		else if (s.Contains("-> Attack"))
		{
			//攻撃入力フラグを下す
			AttackInput = false;

			//ホールド状態フラグを下す
			HoldFlag = false;

			//使用技する技を選定する関数呼び出し
			UseArts = SelectionArts();

			//技が無かったら
			if (UseArts == null)
			{
				//オートコンボを切る
				AutoComboInput = false;

				//もう一度技を探す
				UseArts = SelectionArts();
			}

			//アニメーションを書き換えるAttackステートを選定、ステートカウントを2で割った余りで 0<->1 を切り替えて文字列として連結
			OverrideAttackState = "Arts_void_" + ComboState % 2;

			//どうしても技がない場合は技を出さない処理をする
			if (UseArts == null)
			{
				//使った遷移フラグを下す
				CurrentAnimator.SetBool("Attack0" + ComboState % 2, false);

				//敵のロックを外す
				LockEnemy = null;

				//メインカメラのロックも外す
				ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

				//モーションをIdlingとFallにして技を出さない
				if (OnGroundFlag)
				{
					//オーバーライドコントローラにアニメーションクリップをセット
					OverRideAnimator[OverrideAttackState] = CurrentAnimator.runtimeAnimatorController.animationClips.Where(a => a.name.Contains("Idling")).ToList()[0];

					//強制繊維フラグを立てる
					CurrentAnimator.SetBool("ForceIdling", true);
				}
				else
				{
					//オーバーライドコントローラにアニメーションクリップをセット
					OverRideAnimator[OverrideAttackState] = CurrentAnimator.runtimeAnimatorController.animationClips.Where(a => a.name.Contains("Fall")).ToList()[0];

					//強制繊維フラグを立てる
					CurrentAnimator.SetBool("ForceFall", true);
				}

				//アニメーターを上書きしてアニメーションクリップを切り替える
				CurrentAnimator.runtimeAnimatorController = OverRideAnimator;
			}
			//技があったら攻撃処理
			else
			{
				//メインカメラにロック対象を渡す
				ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

				//オーバーライドコントローラにアニメーションクリップをセット
				OverRideAnimator[OverrideAttackState] = UseArts.AnimClip;

				//アニメーターを上書きしてアニメーションクリップを切り替える
				CurrentAnimator.runtimeAnimatorController = OverRideAnimator;

				//地上技を出したら
				if (OnGroundFlag)
				{
					//空中ローリング許可フラグを立てる
					AirRollingFlag = true;
				}

				//使った遷移フラグを下す
				CurrentAnimator.SetBool("Attack0" + ComboState % 2, false);

				//モーション再生時間を初期化
				CurrentAnimator.SetFloat("AttackSpeed0" + ComboState % 2, 1.0f);

				//チェインブレイクフラグを下ろす
				CurrentAnimator.SetBool("ChainBreak", false);

				//コンボフラグを立てる
				CurrentAnimator.SetBool("Combo", true);

				//遷移可能フラグを下ろす
				CurrentAnimator.SetBool("Transition", false);

				//チェイン攻撃フラグを入れる
				CurrentAnimator.SetBool("Chain", UseArts.Chain);

				//コライダを非アクティブ化
				AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

				//回転制御フラグを下す
				NoRotateFlag = false;

				//チャージレベル初期化
				ChargeLevel = 0;

				//ジャンプ開始直後のベクトル初期化
				JumpHorizonVector *= 0;

				//コンボステートカウントアップ
				ComboState++;
			}
		}
		//Damageになった瞬間の処理
		else if (s.Contains("-> Damage"))
		{
			//ダメージ状態フラグを立てる
			DamageFlag = true;

			//入力フラグを全て下す関数呼び出し
			InputReset();
			
			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//ホールドフラグを下す
			HoldFlag = false;

			//ホールド状態を解除
			HoldBreak();

			//ロックを解除
			LockEnemy = null;
		}
		//Damageから抜けた瞬間の処理
		else if (s.Contains("Damage ->"))
		{
			//ダメージフラグを下す
			DamageFlag = false;
		}

		//SpecialTryになった瞬間の処理
		else if (s.Contains("-> SpecialTry"))
		{
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("SpecialTry", false);

			//特殊攻撃入力フラグを下す
			SpecialInput = false;

			//特殊攻撃待機フラグを立てる
			SpecialTryFlag = true;

			//特殊攻撃対象オブジェクト
			GameObject tempOBJ = null;

			//特殊攻撃対象を探す関数呼び出し
			ExecuteEvents.Execute<SpecialArtsScriptInterface>(gameObject, null, (reciever, eventData) => tempOBJ = reciever.SearchSpecialTarget(CharacterID));

			//特殊攻撃対象オブジェクトがいたらロック
			if (tempOBJ != null)
			{
				LockEnemy = tempOBJ;
			}
		}
		//SpecialSuccessになった瞬間の処理
		else if (s.Contains("-> SpecialSuccess"))
		{
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("SpecialSuccess", false);
		}
		//SpecialAttackになった瞬間の処理
		else if (s.Contains("-> SpecialAttack"))
		{		
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("SpecialAttack", false);
		}
		//H_Hitになった瞬間の処理
		else if (s.Contains("-> H_Hit"))
		{
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("H_Hit", false);

			//コライダを非アクティブ化
			AttackColOBJ.GetComponent<BoxCollider>().enabled = false;

			//入力フラグを全て下す関数呼び出し
			InputReset();

			//遷移フラグを全て下す関数呼び出し
			TransitionReset();

			//ホールドフラグを下す
			HoldFlag = false;

			//スケベモーションループカウント初期化
			H_Count = 0;

			//ホールド状態を解除
			HoldBreak();

			//ロックを解除
			LockEnemy = null;
		}
		//H_Damageになった瞬間の処理
		else if (s.Contains("-> H_Damage"))
		{
			//口のアニメーションレイヤーの重みをリセット
			CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Mouth"), 0);

			//スケベステートカウントアップ
			H_State++;

			//スケベモーションループカウント初期化
			H_Count = 0;

			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("H_Damage00", false);
			CurrentAnimator.SetBool("H_Damage01", false);
		}
		//SuperTryになった瞬間の処理
		else if (s.Contains("-> SuperTry"))
		{
			//超必殺技入力フラグを下す
			SuperInput = false;

			//超必殺技制御カウント初期化
			SuperCount = 0;

			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("SuperTry", false);
		}
		//SuperArtsになった瞬間の処理
		else if (s.Contains("-> SuperArts"))
		{
			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("SuperArts", false);

			//超必殺技モーション同期コルーチン呼び出し
			StartCoroutine(SuperArtsSyncCoroutine()); 			
		}

		//スケベブレイクから遷移した瞬間の処理
		if (s.Contains("H_Break ->"))
		{
			//スケベフラグを下ろす
			H_Flag = false;

			//脱出用レバガチャカウントリセット
			BreakCount = 0;

			//スケベ攻撃をしてきた敵初期化
			H_MainEnemy = null;

			//スケベ攻撃をしてきた敵の近くにいた敵初期化
			H_SubEnemy = null;

			//アニメーターのフラグを下ろす
			CurrentAnimator.SetBool("H_Break", false);

			//ゲームマネージャーのスケベフラグを下ろす
			GameManagerScript.Instance.H_Flag = false;

			//視線ダイレクトモード解除
			AutoEye();

			//口のアニメーションレイヤーの重みをリセット
			CurrentAnimator.SetLayerWeight(CurrentAnimator.GetLayerIndex("Mouth"), 0);
		}
	}
	private IEnumerator SuperArtsSyncCoroutine()
	{
		//アニメーターを一時停止
		CurrentAnimator.speed = 0;

		//敵のモーションが超必殺技喰らいになるまで待つ
		while(!LockEnemy.GetComponent<EnemyCharacterScript>().CurrentState.Contains("-> SuperDamage"))
		{
			yield return null;
		}

		//アニメーター再生
		CurrentAnimator.speed = 1;
	}
	//遷移許可フラグを管理する関数
	private void TransitionManager()
	{
		//超必殺技遷移許可条件
		PermitTransitionBoolDic["SuperTry"] =
		OnGroundFlag &&
		SuperInput &&
		PermitInputBoolDic["SuperTry"] &&
		CurrentAnimator.GetBool("Transition")
		;

		//特殊攻撃遷移許可条件
		PermitTransitionBoolDic["SpecialTry"] =
		OnGroundFlag &&
		SpecialInput &&
		PermitInputBoolDic["SpecialTry"] &&
		CurrentAnimator.GetBool("Transition")
		;

		//ジャンプ遷移許可条件
		PermitTransitionBoolDic["Jump"] =
		OnGroundFlag &&
		JumpInput &&
		PermitInputBoolDic["Jump"] &&
		CurrentAnimator.GetBool("Transition")
		;

		//地上ローリング遷移許可条件
		PermitTransitionBoolDic["GroundRolling"] =
		OnGroundFlag &&
		RollingInput &&
		PermitInputBoolDic["GroundRolling"] &&
		CurrentAnimator.GetBool("Transition")
		;

		//空中ローリング遷移許可条件
		PermitTransitionBoolDic["AirRolling"] =
		!OnGroundFlag &&
		RollingInput &&
		PermitInputBoolDic["AirRolling"] &&
		CurrentAnimator.GetBool("Transition")
		;

		//攻撃遷移許可条件
		PermitTransitionBoolDic["Attack00"] =
		AttackInput &&
		PermitInputBoolDic["Attack00"] &&
		//足を踏み外していない
		!DropFlag &&
		//ジャンプしてすぐじゃない
		(Time.time - JumpTime > 0.2f)
		;

		PermitTransitionBoolDic["Attack01"] = PermitTransitionBoolDic["Attack00"];
	}

	//フラグ状態をまっさらに戻す関数
	private void ClearFlag()
	{
		//攻撃入力されていない、これをしないと攻撃入力後にここが呼ばれてロックできない場合がある
		if (!AttackInput)
		{
			//敵のロックを外す
			LockEnemy = null;

			//メインカメラのロックも外す
			ExecuteEvents.Execute<MainCameraScriptInterface>(MainCameraTransform.parent.gameObject, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));
		}
		
		//入力フラグを全て下す関数呼び出し
		InputReset();

		//遷移フラグを全て下す関数呼び出し
		TransitionReset();

		//ダメージ状態フラグを下す
		DamageFlag = false;

		//ダッシュフラグを下す
		DashFlag = false;

		//ホールド中フラグを下す
		HoldFlag = false;

		//空中ローリング許可フラグを立てる
		AirRollingFlag = true;

		//攻撃ボタン押しっぱなしフラグを下す
		HoldButtonFlag = false;

		//回転制御フラグを下す
		NoRotateFlag = false;

		//特殊攻撃フラグを下ろす
		SpecialTryFlag = false;

		//特殊攻撃成功フラグを下ろす
		SpecialSuccessFlag = false;

		//ローリング移動ベクトルリセット
		RollingMoveVector *= 0;

		//攻撃移動ベクトルリセット
		AttackMoveVector *= 0;

		//ダメージ移動ベクトルリセット
		DamageMoveVector *= 0;

		//キャラクターのスケベ移動ベクトルリセット
		H_MoveVector *= 0;

		//ジャンプ開始直後のベクトル初期化
		JumpHorizonVector *= 0;

		//垂直加速度をリセット
		VerticalAcceleration = 0;

		//重力加速度をリセット
		GravityAcceleration = Physics.gravity.y * 2 * Time.deltaTime;

		//コンボフラグを下ろす
		CurrentAnimator.SetBool("Combo", false);

		//チェインフラグを下ろす
		CurrentAnimator.SetBool("Chain", false);

		//踏み外しフラグを下ろす
		CurrentAnimator.SetBool("Drop", false);

		//ダメージフラグを下ろす
		CurrentAnimator.SetBool("Damage", false);

		//強制遷移フラグを下す
		CurrentAnimator.SetBool("ForceIdling", false);
		CurrentAnimator.SetBool("ForceFall", false);

		//遷移可能フラグを立てる
		CurrentAnimator.SetBool("Transition", true);

		//イベントアニメーションフラグを下ろす
		CurrentAnimator.SetBool("ActionEvent", false);

		//表情を普通に戻す
		ChangeFace("Reset");

		//攻撃移動タイプを初期化
		AttackMoveType = 100;

		//コンボステートをリセット
		ArtsStateMatrixReset();
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
		if (Time.time - OnCameraTime > 0.025 * (GameManagerScript.Instance.FrameRate / GameManagerScript.Instance.FPS))
		{
			re = false;
		}

		//出力
		return re;
	}

	//ホールド状態を維持するコルーチン
	IEnumerator KeepHold()
	{
		while (HoldFlag)
		{
			//１フレーム待機
			yield return null;
		}

		//敵接触フラグを下ろす、ホールド解除と同時にTrueになっている時がある
		EnemyContactFlag = false;
	}

	//足音を鳴らす
	public void PlayFootSetp()
	{
		foreach(var i in FootStepOBJ.GetComponents<SoundEffectScript>())
		{
			if(i.AudioName.Contains(GroundSurface))
			{
				i.PlayRandomList();
			}
		}
	}

	//キャラクターのデータをセットする、キャラクターセッティングから呼ばれる
	public void SetCharacterData(CharacterClass CC, List<AnimationClip> DAL, List<AnimationClip> HHL, List<AnimationClip> HDL, List<AnimationClip> HBL, GameObject CRO, GameObject MSA)
	{
		//ダメージモーションList
		DamageAnimList = new List<AnimationClip>(DAL);

		//スケベヒットモーションList
		H_HitAnimList = new List<AnimationClip>(HHL);

		//スケベダメージモーションList
		H_DamageAnimList = new List<AnimationClip>(HDL);

		//スケベブレイクモーションList
		H_BreakAnimList = new List<AnimationClip>(HBL);

		//特殊攻撃処理取得
		foreach(var i in SpecialArtsList)
		{
			//アンロック状況を判定
			if(i.UnLock == 1)
			{
				//代入用変数宣言
				List<Action<GameObject, GameObject, SpecialClass>> TempSpecialAct = null;

				//スクリプトから処理を受け取る
				ExecuteEvents.Execute<SpecialArtsScriptInterface>(gameObject, null, (reciever, eventData) => TempSpecialAct = new List<Action<GameObject, GameObject, SpecialClass>>(reciever.GetSpecialAct(i.UseCharacter, i.ArtsIndex)));

				//処理を代入
				i.SpecialAtcList = new List<Action<GameObject, GameObject, SpecialClass>>(TempSpecialAct);
			}
		}

		//超必殺技処理取得
		if(SuperArts != null)
		{
			//超必殺技処理代入用変数宣言
			List<Action<GameObject, GameObject>> TempSuperAct = null;

			//超必殺技処理のListを受け取る
			ExecuteEvents.Execute<SpecialArtsScriptInterface>(gameObject, null, (reciever, eventData) => TempSuperAct = new List<Action<GameObject, GameObject>>(reciever.GetSuperAct(SuperArts.UseCharacter, SuperArts.ArtsIndex)));

			//超必殺技処理を代入
			SuperArts.SuperActList = new List<Action<GameObject, GameObject>>(TempSuperAct);
		}

		//コスチュームルートオブジェクト
		CostumeRootOBJ = CRO;

		//モザイクオブジェクト
		MosaicOBJ = MSA;

		//移動速度
		PlayerMoveSpeed = CC.PlayerMoveSpeed;

		//ダッシュ速度
		PlayerDashSpeed = CC.PlayerDashSpeed;

		//ローリング速度
		RollingSpeed = CC.RollingSpeed;

		//旋回速度
		TurnSpeed = CC.TurnSpeed;

		//ジャンプ力
		JumpPower = CC.JumpPower;

		//遠近攻撃切り替え距離
		AttackDistance = CC.AttackDistance;
	}

	//戦闘フラグをセットする
	public void SetFightingFlag(bool b)
	{
		//引数で受け取ったフラグをセットする
		FightingFlag = b;

		//構え切り替えコルーチン呼び出し
		StartCoroutine(IdlingChangeCoroutine());
	}

	//ポーズ処理、ゲームマネージャーから呼び出されるインターフェイス
	public void Pause(bool b)
	{
		//ポーズフラグ引数で受け取ったboolをフラグに代入
		PauseFlag = b;

		//アタックコライダにフラグを送る
		ExecuteEvents.Execute<PlayerAttackCollInterface>(AttackColOBJ, null, (reciever, eventData) => reciever.SetPauseFlag(PauseFlag));

		//ポーズオン
		if (PauseFlag)
		{
			//アニメーターを止める
			CurrentAnimator.speed = 0;

			//全ての再生中パーティクルシステムを一時停止
			foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
			{
				if (i.isPlaying)
				{
					i.Pause();
				}
			}
		}
		//ポーズオフ
		else
		{
			//アニメーターを動かす
			CurrentAnimator.speed = 1f;

			//ポーズされていた全てのパーティクルシステムを再開
			foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
			{
				if (i.isPaused)
				{
					i.Play();
				}
			}
		}
	}
}

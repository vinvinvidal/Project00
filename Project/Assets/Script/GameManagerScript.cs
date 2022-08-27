using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface GameManagerScriptInterface : IEventSystemHandler
{
	//AllActiveCharacterListに追加する
	int AddAllActiveCharacterList(GameObject obj);

	//AllActiveCharacterListから削除する
	void RemoveAllActiveCharacterList(int n);

	//AllActiveEnemyListに追加する
	int AddAllActiveEnemyList(GameObject obj);

	//AllActiveEnemyListから削除する
	void RemoveAllActiveEnemyList(int n);

	//プレイアブルキャラクターをセットするインターフェイス
	void SetPlayableCharacterOBJ(GameObject obj);

	//現在のプレイアブルキャラクターを返すインターフェイス
	GameObject GetPlayableCharacterOBJ();

	//カメラルート返すインターフェイス
	GameObject GetCameraOBJ();

	//交代可能な参加メンバーをセットするインターフェイス
	void SetMissionCharacterDic(Dictionary<int, GameObject> MCD);

	//バトルフィールドをセットするインターフェイス
	void SetBattleFieldOBJ(GameObject obj);

	//現在のバトルフィールドを返すインターフェイス
	GameObject GetBattleFieldOBJ();

	//ロック対象の敵を返す
	GameObject SearchLockEnemy(Vector3 Vec);

	//カメラワークの遷移でイージングをするためヴァーチャルカメラを制御する
	void EasingVcamera();

	//タイムスケール変更
	void TimeScaleChange(float t, float s, Action act);

	//スカイボックス取得、シーンの最初に呼び出す
	void SetSkyBox();

	//ミニマップ表示切り替え
	void MiniMapSwitch(bool s);

	//超必殺技暗転演出
	void StartSuperArtsLightEffect(float t);
	void EndSuperArtsLightEffect(float t);

	//超必殺技時間停止演出
	void SuperArtsStopEffect(float t, GameObject e);
}

//シングルトンなので専用オブジェクトにつけてシーン移動後も常に存在させる
public class GameManagerScript : GlobalClass , GameManagerScriptInterface
{
	//開発用スイッチ
	public bool DevSwicth;

	//無音スイッチ
	public bool SoundOffSwicth;

	//性的表現スイッチ
	public bool SexualSwicth;
	
	//ゲームデータ読み込み開始フラグ
	public bool LoadGameDataFlag { get; set; } = false;

	//ユニークインスタンス
	private static GameManagerScript instance;

	//ユニークインスタンスのプロパティ、セッター無し
	public static GameManagerScript Instance
	{
		//ゲッター
		get
		{
			if (null == instance)
			{
				instance = (GameManagerScript)FindObjectOfType(typeof(GameManagerScript));
			}

			return instance;
		}
	}

	//画面比率
	public Vector2Int ScreenAspect;

	//画面解像度
	public int ScreenResolution;

	//フレームレート
	public int FrameRate;

	//FPS測定用変数
	public float FPS;
	private int FrameCount;
	private float PrevTime;
	private float NextTime;

	//ポーズフラグ
	public bool PauseFlag { get; set; } = false;





	//セーブロードするセーブデータ
	public UserDataClass UserData { get; set; } = null;

	//外部ファイルが置いてあるデータパス
	public string DataPath { get; set; }

	//AssetBudleの依存関係データ
	public AssetBundleManifest DependencyManifest { get; set; }

	//読み込み済みアセットバンドルリスト、重複ロードを防止する
	public List<string> LoadedDataList { get; set; }

	//セーブデータ準備完了フラグ
	public bool UserDataReadyFlag { get; set; } = false;






	//選択中のミッションID
	public int SelectedMissionNum { get; set; } = 0;

	//選択中のチャプターID
	public int SelectedMissionChapter { get; set; } = 0;

	//現在のプレイアブルキャラクター
	private GameObject PlayableCharacterOBJ;

	//ダウンしているキャラクター
	public List<GameObject> DownCharacterList { get; set; } = new List<GameObject>();

	//現在のバトルフィールドオブジェクト
	private GameObject BattleFieldOBJ;

	//ミッションに参加していて交代可能なキャラクター
	private Dictionary<int, GameObject> MissionCharacterDic;

	//メインカメラ
	private GameObject MainCamera;

	//バーチャルカメラ
	private CinemachineVirtualCamera VCamera;

	//ミッションUIスクリプト
	public MissionUIScript MissionUI { get; set; }

	//ロケーションによって切り替える野外ライト
	private Light OutDoorLight;

	//スケベフラグ
	public bool H_Flag { get; set; } = false;

	//イベント中フラグ
	public bool EventFlag { get; set; } = false;

	//敵生成中フラグ
	public bool GenerateEnemyFlag { get; set; } = false;

	//戦闘中フラグ
	public bool BattleFlag { get; set; } = false;

	//ロック対象になる敵を入れるList
	private List<GameObject> LockEnemyList;

	//タイムスケール係数
	private float TimeScaleNum = 1;

	//使用中のスカイボックス
	private Material SkyBoxMaterial;



	//存在している全てのキャラクターリスト
	public List<GameObject> AllActiveCharacterList { get; set; } = new List<GameObject>();

	//存在している全てのエネミーリスト
	public List<GameObject> AllActiveEnemyList { get; set; } = new List<GameObject>();

	//存在している全ての敵飛び道具オブジェクトList
	public List<GameObject> AllEnemyWeaponList { get; set; } = new List<GameObject>();

	//存在している全てのプレイヤー飛び道具オブジェクトList
	public List<GameObject> AllPlayerWeaponList { get; set; } = new List<GameObject>();

	//汎用SE
	public SoundEffectScript GenericSE { get; set; }

	//足音SE
	public List<SoundEffectScript> FootStepSEList { get; set; }

	//攻撃スイング音SE
	public SoundEffectScript AttackSwingSE { get; set; }

	//攻撃ヒット音SE
	public List<SoundEffectScript> AttackImpactSEList { get; set; }

	//武器固有SE
	public List<SoundEffectScript> WeaponSEList { get; set; }





	//全てのキャラクター情報を持ったList
	public List<CharacterClass> AllCharacterList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllCharacterListCompleteFlag = false;

	//全ての敵情報を持ったList
	public List<EnemyClass> AllEnemyList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllEnemyListCompleteFlag = false;

	//全ての敵ウェーブ情報を持ったList
	public List<EnemyWaveClass> AllEnemyWaveList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllEnemyWaveListCompleteFlag = false;

	//全ての敵攻撃情報を持ったList
	public List<EnemyAttackClass> AllEnemyAttackList { get; set; }
	//↑の読み込み完了フラグ全ての技のアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllEnemyAttackAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのミッション情報を持ったList
	public List<MissionClass> AllMissionList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllMissionListCompleteFlag = false;

	//全ての技情報を持ったList
	public List<ArtsClass> AllArtsList { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string , bool> AllArtsAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全ての特殊技情報を持ったList
	public List<SpecialClass> AllSpecialArtsList { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllSpecialArtsAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全ての超必殺技情報を持ったList
	public List<SuperClass> AllSuperArtsList { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllSuperArtsAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全ての表情アニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllFaceDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllFaceAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのスケベ表情アニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllH_FaceDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllH_FaceAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのダメージアニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllDamageDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllDamageAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのキャラ交代アニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllChangeDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllChangeAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのスケベヒットアニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllH_HitDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllH_HitAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのスケベダメージアニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllH_DamageDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllH_DamageAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのスケベ愛撫アニメーションを持ったDic
	public Dictionary<int,List<AnimationClip>> AllH_CaressDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllH_CaressAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのスケベブレイクアニメーションを持ったDic
	public Dictionary<int, List<AnimationClip>> AllH_BreakDic { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllH_BreakAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全ての武器情報を持ったList
	public List<WeaponClass> AllWeaponList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllWeaponListCompleteFlag = false;

	//全てのパーティクルエフェクトを持ったリスト
	public List<GameObject> AllParticleEffectList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllParticleEffectListCompleteFlag = false;

	//全てのデータ読み込み完了フラグ
	public bool AllDetaLoadCompleteFlag { get; set; } = false;

	//フラグを追加したら下のAllDetaLoadCompleteCheckに追加すること

	//読み込み完了フラグチェックコルーチン
	IEnumerator AllDetaLoadCompleteCheck()
	{		
		//外部データの読み込みが完了するまで回る
		while
		(!(
			AllCharacterListCompleteFlag &&
			AllEnemyListCompleteFlag &&
			AllEnemyWaveListCompleteFlag &&
			AllEnemyAttackAnimCompleteFlagDic.Any() &&
			AllEnemyAttackAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllSpecialArtsAnimCompleteFlagDic.Any() &&
			AllSpecialArtsAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllSuperArtsAnimCompleteFlagDic.Any() &&
			AllSuperArtsAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllMissionListCompleteFlag && 
			AllParticleEffectListCompleteFlag &&
			AllWeaponListCompleteFlag &&
			AllArtsAnimCompleteFlagDic.Any()&&
			AllArtsAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllFaceAnimCompleteFlagDic.Any() &&
			AllFaceAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllH_FaceAnimCompleteFlagDic.Any() &&
			AllH_FaceAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllDamageAnimCompleteFlagDic.Any() &&
			AllDamageAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllChangeAnimCompleteFlagDic.Any() &&
			AllChangeAnimCompleteFlagDic.All(a => a.Value == true) &&			
			AllH_HitAnimCompleteFlagDic.Any() &&
			AllH_HitAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllH_DamageAnimCompleteFlagDic.Any() &&
			AllH_DamageAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllH_CaressAnimCompleteFlagDic.Any() &&
			AllH_CaressAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllH_BreakAnimCompleteFlagDic.Any() &&
			AllH_BreakAnimCompleteFlagDic.All(a => a.Value == true) &&
			UserDataReadyFlag
		))
		{
			yield return null;
		}		

		//追加要素チェック関数呼び出し
		AddContentCheck();

		//終わったらフラグを立てる
		AllDetaLoadCompleteFlag = true;
	}

	void Awake()
	{
		//マウスカーソル非表示
		Cursor.visible = false;

		//起動時にスクリーンサイズを決定
		Screen.SetResolution(ScreenAspect.x * ScreenResolution, ScreenAspect.y * ScreenResolution, FullScreenMode.FullScreenWindow);

		//フレームレートを設定
		Application.targetFrameRate = FrameRate;

		//シーン遷移してもユニーク性を保つようにする関数呼び出し
		Singleton();

		//データパス取得
		DataPath = Application.dataPath;

		//メインカメラ取得
		MainCamera = DeepFind(gameObject, "CameraRoot");

		//バーチャルカメラ取得
		VCamera = DeepFind(gameObject , "MasterVcam").GetComponent<CinemachineVirtualCamera>();

		//ミッションUIスクリプト取得
		MissionUI = DeepFind(gameObject, "MissionUI").GetComponent<MissionUIScript>();

		//野外ライト取得
		OutDoorLight = DeepFind(gameObject , "OutDoorLight").GetComponent<Light>();

		//攻撃ヒット音オブジェクト取得
		AttackImpactSEList = new List<SoundEffectScript>(DeepFind(gameObject, "AttackImpactSE").GetComponents<SoundEffectScript>());

		//攻撃スイング音オブジェクト取得
		AttackSwingSE = DeepFind(gameObject, "AttackSwingSE").GetComponent<SoundEffectScript>();

		//足音SE取得
		FootStepSEList = new List<SoundEffectScript>(DeepFind(gameObject, "FootStepSE").GetComponents<SoundEffectScript>());

		//汎用SE取得
		GenericSE = DeepFind(gameObject, "GenericSE").GetComponent<SoundEffectScript>();

		//武器SE取得
		WeaponSEList = new List<SoundEffectScript>(DeepFind(gameObject, "WeaponSE").GetComponents<SoundEffectScript>());


		//FPS測定用変数初期化
		FPS = 0;
		FrameCount = 0;
		PrevTime = 0.0f;
		NextTime = 0.0f;


		//ゲームデータ読み込みコルーチン呼び出し
		StartCoroutine(LoadGameData());
	}

	private IEnumerator LoadGameData()
	{
		//UIの準備が整うまでループ
		while (!LoadGameDataFlag)
		{
			//1フレーム待機
			yield return null;
		}

		//開発スイッチ
		if (!DevSwicth)
		{
			//AssetBudleの依存関係データ読み込み
			DependencyManifest = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/StreamingAssets").LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		}

		//読み込み済みデータList更新
		LoadedDataListUpdate();

		//セーブデータ読み込み関数呼び出し
		UserDataLoad();

		//セーブデータ読み込み完了を待つ
		while(!UserDataReadyFlag)
		{
			//1フレーム待機
			yield return null;
		}

		//キャラクターCSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Character/", "csv", (List<object> list) =>
		{
			//全てのキャラクター情報を持ったList初期化
			AllCharacterList = new List<CharacterClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//CharacterClassコンストラクタ代入用変数
				int id = 0;

				string LNC = "";
				string LNH = "";
				string FNC = "";
				string FNH = "";
				string ON = "";

				float pms = 0;
				float pds = 0;
				float rs = 0;
				float jp = 0;
				float ts = 0;
				float ad = 0;

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "CharacterID": id = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "L_NameC": LNC = ii.Split(',').ToList().ElementAt(1); break;
						case "L_NameH": LNH = ii.Split(',').ToList().ElementAt(1); break;
						case "F_NameC": FNC = ii.Split(',').ToList().ElementAt(1); break;
						case "F_NameH": FNH = ii.Split(',').ToList().ElementAt(1); break;
						case "OBJname": ON = ii.Split(',').ToList().ElementAt(1); break;
						case "PlayerMoveSpeed": pms = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "PlayerDashSpeed": pds = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "RollingSpeed": rs = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "JumpPower": jp = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "TurnSpeed": ts = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "AttackDistance": ad = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
					}
				}

				//装備中の髪型、衣装、武器のインデックス読み込む
				int Hid = GameManagerScript.instance.UserData.EquipHairList[id];
				int Cid = GameManagerScript.instance.UserData.EquipCostumeList[id];
				int Wid = GameManagerScript.instance.UserData.EquipWeaponList[id];

				//ListにAdd
				AllCharacterList.Add(new CharacterClass(id, LNC, LNH, FNC, FNH, Hid, Cid, Wid, ON, pms, pds, rs, jp, ts, ad));
			}

			//読み込み完了フラグを立てる
			AllCharacterListCompleteFlag = true;

			//表情アニメーションクリップ読み込み完了判定Dicを作る
			AllFaceAnimCompleteFlagDic = new Dictionary<string, bool>();

			//スケベ表情アニメーションクリップ読み込み完了判定Dicを作る
			AllH_FaceAnimCompleteFlagDic = new Dictionary<string, bool>();

			//ダメージアニメーションクリップ読み込み完了判定Dicを作る
			AllDamageAnimCompleteFlagDic = new Dictionary<string, bool>();

			//キャラ交代アニメーションクリップ読み込み完了判定Dicを作る
			AllChangeAnimCompleteFlagDic = new Dictionary<string, bool>();




			//全ての表情アニメーションクリップを持ったList初期化
			AllFaceDic = new Dictionary<int, List<AnimationClip>>();

			//全てのスケベ表情アニメーションクリップを持ったList初期化
			AllH_FaceDic = new Dictionary<int, List<AnimationClip>>();

			//全てのダメージモーションを持ったList初期化
			AllDamageDic = new Dictionary<int, List<AnimationClip>>();

			//全てのキャラ交代モーションを持ったList初期化
			AllChangeDic = new Dictionary<int, List<AnimationClip>>();

			//全てのスケベヒットモーションを持ったList初期化
			AllH_HitDic = new Dictionary<int, List<AnimationClip>>();

			//全てのスケベダメージモーションを持ったDic初期化
			AllH_DamageDic = new Dictionary<int, List<AnimationClip>>();

			//全てのスケベ愛撫モーションを持ったDic初期化
			AllH_CaressDic = new Dictionary<int, List<AnimationClip>>();

			//全てのスケベブレイクモーションを持ったList初期化
			AllH_BreakDic = new Dictionary<int, List<AnimationClip>>();

			//キャラクターリストを回す
			foreach (CharacterClass i in AllCharacterList)
			{
				//表情アニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllFaceAnimCompleteFlagDic.Add(i.F_NameC, false);

				//スケベ表情アニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllH_FaceAnimCompleteFlagDic.Add(i.F_NameC, false);

				//ダメージアニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllDamageAnimCompleteFlagDic.Add(i.F_NameC, false);

				//キャラ交代アニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllChangeAnimCompleteFlagDic.Add(i.F_NameC, false);

				//表情アニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/Face/", "anim", (List<object> FaceOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllFaceDic.Add(i.CharacterID, FaceOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだ表情アニメーションのDicをtrueにする
					AllFaceAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//スケベ表情アニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/H_Face/", "anim", (List<object> H_FaceOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllH_FaceDic.Add(i.CharacterID, H_FaceOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだ表情アニメーションのDicをtrueにする
					AllH_FaceAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//ダメージアニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/Damage/", "anim", (List<object> DamageOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllDamageDic.Add(i.CharacterID, DamageOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだダメージアニメーションのDicをtrueにする
					AllDamageAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//キャラ交代アニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/Change/", "anim", (List<object> ChangeOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllChangeDic.Add(i.CharacterID, ChangeOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだダメージアニメーションのDicをtrueにする
					AllChangeAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//スケベヒットアニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/H_Hit/", "anim", (List<object> H_HitOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllH_HitDic.Add(i.CharacterID, H_HitOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだスケベヒットアニメーションのDicをtrueにする
					AllH_HitAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//スケベダメージアニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/H_Damage/", "anim", (List<object> H_DamageOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllH_DamageDic.Add(i.CharacterID, H_DamageOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだスケベダメージアニメーションのDicをtrueにする
					AllH_DamageAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//スケベ愛撫アニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/H_Caress/", "anim", (List<object> H_CaressOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllH_CaressDic.Add(i.CharacterID, H_CaressOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだスケベダメージアニメーションのDicをtrueにする
					AllH_CaressAnimCompleteFlagDic[i.F_NameC] = true;
				}));			

				//スケベブレイクアニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/H_Break/", "anim", (List<object> H_BreakOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllH_BreakDic.Add(i.CharacterID, H_BreakOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだスケベブレイクアニメーションのDicをtrueにする
					AllH_BreakAnimCompleteFlagDic[i.F_NameC] = true;
				}));
			}
		}));

		//敵攻撃CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/EnemyAttack/", "csv", (List<object> list) =>
		{
			//全ての敵攻撃情報を持ったList初期化
			AllEnemyAttackList = new List<EnemyAttackClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//EnemyAttackClassコンストラクタ代入用変数
				string id = "";
				string ud = "";
				string an = "";
				string info = "";
				int dm = 0;
				int at = 0;
				int dt = 0;
				string am = "";
				int pdm = 0;
				int pdt = 0;
				Color pkb = new Color(0,0,0,0);

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "ID": id = ii.Split(',').ToList().ElementAt(1); break;
						case "UserID": ud = ii.Split(',').ToList().ElementAt(1); break;
						case "Name": an = ii.Split(',').ToList().ElementAt(1); break;
						case "info": info = ii.Split(',').ToList().ElementAt(1); break;
						case "Damage": dm = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;						
						case "AttackType": at = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "DamageType": dt = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Anim": am = ii.Split(',').ToList().ElementAt(1); break;
						case "PlayerUseDamage": pdm = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "PlyaerUseDamageType": pdt = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "PlyaerUseKnockBackVec":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								pkb = new Color(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2)), float.Parse(iii.Split('*').ElementAt(3)));
							}

							break;
					}
				}

				//ListにAdd
				AllEnemyAttackList.Add(new EnemyAttackClass(LineFeedCodeClear(id), LineFeedCodeClear(ud), LineFeedCodeClear(an), LineFeedCodeClear(info), dm, at, dt, pdm, pdt, pkb, LineFeedCodeClear(am)));
			}

			//アニメーションクリップ読み込み完了判定Dicを作る
			foreach (EnemyAttackClass i in AllEnemyAttackList)
			{
				AllEnemyAttackAnimCompleteFlagDic.Add(i.AnimName, false);
			}

			foreach (EnemyAttackClass i in AllEnemyAttackList)
			{
				//技のアニメーションを読み込む
				StartCoroutine(LoadOBJ("Anim/Enemy/" + i.UserID + "/Attack/", i.AnimName, "anim", (object O) =>
				{
					//ListにAdd
					i.Anim = O as AnimationClip;

					//読み込んだアニメーションのDicをtrueにする
					AllEnemyAttackAnimCompleteFlagDic[i.AnimName] = true;

				}));
			}
		}));

		//敵CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Enemy/", "csv", (List<object> list) =>
		{
			//全ての敵情報を持ったList初期化
			AllEnemyList = new List<EnemyClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//EnemyClassコンストラクタ代入用変数
				string id = "";
				string objname = "";
				string name = "";
				int life = 0;
				float stun = 0;
				float downtime = 0;
				float movespeed = 0;
				float turnspeed = 0;

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "ID": id = ii.Split(',').ToList().ElementAt(1); break;
						case "OBJname": objname = ii.Split(',').ToList().ElementAt(1); break;
						case "Name": name = ii.Split(',').ToList().ElementAt(1); break;
						case "Life": life = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Stun": stun = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "DownTime": downtime = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "MoveSpeed": movespeed = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "TurnSpeed": turnspeed = float.Parse(ii.Split(',').ToList().ElementAt(1)); break;
					}
				}

				//ListにAdd
				AllEnemyList.Add(new EnemyClass(LineFeedCodeClear(id), LineFeedCodeClear(objname), LineFeedCodeClear(name), life, stun, downtime, movespeed, turnspeed));
			}

			//読み込み完了フラグを立てる
			AllEnemyListCompleteFlag = true;

		}));

		//敵ウェーブCSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/EnemyWave/", "csv", (List<object> list) =>
		{
			//全ての敵ウェーブ情報を持ったList初期化
			AllEnemyWaveList = new List<EnemyWaveClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//EnemyWaveClassコンストラクタ代入用変数
				int id = 0;
				string info = "";
				List<int> el = null;

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "ID": id = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Info": info = ii.Split(',').ToList().ElementAt(1); break;
						case "Enemy": el = new List<int>(ii.Split(',').ToList().ElementAt(1).Split('|').ToList().Select(t => int.Parse(t))); break;
					}
				}

				//ListにAdd
				AllEnemyWaveList.Add(new EnemyWaveClass(id, LineFeedCodeClear(info), el));
			}

			//読み込み完了フラグを立てる
			AllEnemyWaveListCompleteFlag = true;

		}));

		//ミッションCSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Mission/", "csv", (List<object> list) =>
		{
			//全てのミッション情報を持ったList初期化
			AllMissionList = new List<MissionClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//MissionClassコンストラクタ代入用変数
				float Num = 0;
				string MissionTitle = "";
				string Introduction = "";
				List<int> MissionCharacter = null;
				List<List<int>> ChapterCharacter = new List<List<int>>();
				List<int> FirstCharacter = null;
				List<string> ChapterStage = null;
				List<Vector3> CharacterPosListt = new List<Vector3>();
				List<Vector3> CameraPosList = new List<Vector3>();
				List<int> LightG = new List<int>();
				List<float> LightP = new List<float>();

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "Nam":
							Num = float.Parse(ii.Split(',').ToList().ElementAt(1));
							break;

						case "Title":
							MissionTitle = ii.Split(',').ToList().ElementAt(1);
							break;

						case "Introduction":
							Introduction = ii.Split(',').ToList().ElementAt(1);
							break;

						case "MissionCharacter":							
							MissionCharacter = new List<int>(ii.Split(',').ToList().ElementAt(1).Split('|').ToList().Select(t => int.Parse(t)));
							break;

						case "ChapterCharacter":
							foreach (string iii in ii.Split(',').ToList().ElementAt(1).Split('|').ToList())
							{
								ChapterCharacter.Add(iii.Split('*').Select(t => int.Parse(t)).ToList());
							}

							break;

						case "FirstCharacter":
							FirstCharacter = new List<int>(ii.Split(',').ToList().ElementAt(1).Split('|').ToList().Select(t => int.Parse(t)));
							break;

						case "ChapterStage":
							ChapterStage = new List<string>(ii.Split(',').ToList().ElementAt(1).Split('|').ToList());
							break;

						case "PlayerPos":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								//*で分割した値をVector3にしてAdd
								CharacterPosListt.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;

						case "CameraPos":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								//*で分割した値をVector3にしてAdd
								CameraPosList.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;

						case "LightG":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								LightG.Add(int.Parse(iii));
							}

							break;

						case "LightP":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								LightP.Add(float.Parse(iii));
							}

							break;
					}
				}

				//ListにAdd
				AllMissionList.Add(new MissionClass(Num, MissionTitle, Introduction, MissionCharacter, ChapterCharacter, FirstCharacter, ChapterStage, CharacterPosListt, CameraPosList, LightG, LightP));
			}

			//読み込み完了フラグを立てる
			AllMissionListCompleteFlag = true;

		}));

		//技CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Arts/", "csv", (List<object> list) =>
		{
			//全ての技情報を持ったList初期化
			AllArtsList = new List<ArtsClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//ArtsClassコンストラクタ代入用変数
				string nc = "";
				string nh = "";
				int uc = 0;
				string an = "";
				List<Color> mv = new List<Color>();
				List<float> dm = new List<float>();
				List<int> st = new List<int>();
				List<int> dl = new List<int>();
				List<Color> cv = new List<Color>();
				List<Color> kb = new List<Color>();
				string intro = "";
				List<int> lk = new List<int>();
				List<int> mt = new List<int>();
				List<int> at = new List<int>();
				List<int> de = new List<int>();
				List<int> ct = new List<int>();
				List<int> tt = new List<int>();
				bool ch = true;
				List<string> he = new List<string>();
				List<Vector3> hp = new List<Vector3>();
				List<Vector3> ha = new List<Vector3>();
				List<float> hs = new List<float>();
				List<int> cg = new List<int>();
				List<Vector3> hl = new List<Vector3>();
				int lf = 0;
				List<string> hse = new List<string>();
				float mct = 0;

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "NameC":
							nc = ii.Split(',').ToList().ElementAt(1);
							break;

						case "NameH":
							nh = ii.Split(',').ToList().ElementAt(1);
							break;

						case "UseCharacter":
							uc = int.Parse(ii.Split(',').ToList().ElementAt(1));
							break;

						case "Anim":
							an = ii.Split(',').ToList().ElementAt(1);
							break;

						case "MoveVec":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								mv.Add(new Color(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2)), float.Parse(iii.Split('*').ElementAt(3))));
							}

							break;

						case "Damage":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								dm.Add(float.Parse(iii));
							}

							break;

						case "Stun":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								st.Add(int.Parse(iii));
							}

							break;

						case "Deadly":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								dl.Add(int.Parse(iii));
							}

							break;

						case "ColVec":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								cv.Add(new Color(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2)), float.Parse(iii.Split('*').ElementAt(3))));
							}

							break;

						case "KnockBackVec":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								kb.Add(new Color(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2)), float.Parse(iii.Split('*').ElementAt(3))));
							}

							break;

						case "Introduction":
							intro = ii.Split(',').ToList().ElementAt(1);
							break;

						case "UnLock":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								lk.Add(int.Parse(iii));
							}

							break;

						case "MoveType":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								mt.Add(int.Parse(iii));
							}

							break;

						case "AttackType":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								at.Add(int.Parse(iii));
							}

							break;

						case "DownEnable":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								de.Add(int.Parse(iii));
							}

							break;

						case "ColType":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								ct.Add(int.Parse(iii));
							}

							break;

						case "TimeType":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								tt.Add(int.Parse(iii));
							}

							break;

						case "Chain":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								if (int.Parse(iii) == 0)
								{
									ch = false;
								}
							}

							break;

						case "HitEffect":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								he.Add(LineFeedCodeClear(iii));
							}

							break;

						case "HitEffectPos":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								hp.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;

						case "HitEffectAngle":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								ha.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;

						case "HitStop":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								hs.Add(float.Parse(iii));
							}

							break;

						case "Charge":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								cg.Add(int.Parse(iii));
							}

							break;

						case "HoldPos":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								hl.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;

						case "LocationFlag":
							lf = int.Parse(ii.Split(',').ToList().ElementAt(1));

							break;

						case "HitSE":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								hse.Add(LineFeedCodeClear(iii));
							}

							break;

						case "CoolDownTime":
							mct = float.Parse(ii.Split(',').ToList().ElementAt(1));
							break;
					}
				}

				//ListにAdd
				AllArtsList.Add(new ArtsClass(nc, nh, uc, an, mv, dm, st, dl, cv, kb, intro, lk, mt, at, de, ct, tt, ch, he, hp, ha, hs, cg, hl, lf, hse, mct));
			}

			//アニメーションクリップ読み込み完了判定Dicを作る
			foreach (ArtsClass i in AllArtsList)
			{
				AllArtsAnimCompleteFlagDic.Add(i.AnimName, false);
			}

			foreach (ArtsClass i in AllArtsList)
			{
				//技のアニメーションを読み込む
				StartCoroutine(LoadOBJ("Anim/Character/" + i.UseCharacter + "/Arts/", i.AnimName, "anim", (object O) =>
				{
					//ListにAdd
					i.AnimClip = O as AnimationClip;

					//読み込んだアニメーションのDicをtrueにする
					AllArtsAnimCompleteFlagDic[i.AnimName] = true;

				}));
			}
		}));

		//特殊技CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Special/", "csv", (List<object> list) =>
		{
			//全ての特殊技情報を持ったList初期化
			AllSpecialArtsList = new List<SpecialClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//EnemyAttackClassコンストラクタ代入用変数
				int cid = 0;
				int aid = 0;
				int ul = 0;
				int tr = 0;
				string nc = "";
				string nh = "";
				string an = "";
				string info = "";
				int di = 0;
				int dm = 0;
				string ep = "";
				List<Action<GameObject, GameObject, GameObject, SpecialClass>> sa = new List<Action<GameObject, GameObject, GameObject, SpecialClass>>();

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "UseCharacter": cid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "ArtsIndex": aid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Trigger": tr = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "NameC": nc = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "NameH": nh = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "AnimName": an = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "Info": info = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "EffectPos": ep = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "DamageIndex": di = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Damage": dm = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "UnLock": ul = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
					}
				}

				//特殊攻撃処理のListを受け取る
				//ExecuteEvents.Execute<SpecialArtsScriptInterface>(gameObject, null, (reciever, eventData) => sa = new List<Action<GameObject, GameObject, SpecialClass>>(reciever.GetSpecialAct(cid, aid)));

				//セーブデータからアンロック状況を読み込む
				foreach(string ii in UserData.SpecialUnLock)
				{
					//リストに名前があればアンロック済み
					if(ii == nc)
					{
						ul = 1;
					}
				}				

				//ListにAdd
				AllSpecialArtsList.Add(new SpecialClass(cid, aid, ul, nc, nh, an, info, tr, di, dm, ep, sa));
			}

			//アニメーションクリップ読み込み完了判定Dicを作る。
			foreach (SpecialClass i in AllSpecialArtsList)
			{
				//フラグ管理DicにAdd
				AllSpecialArtsAnimCompleteFlagDic.Add(i.AnimName, false);
			}

			//特殊攻撃Listを回す
			foreach (SpecialClass i in AllSpecialArtsList)
			{
				//技のアニメーションを読み込む
				StartCoroutine(LoadOBJ("Anim/Character/" + i.UseCharacter + "/Special/", i.AnimName, "anim", (object O) =>
				{
					//ClassのListにAdd
					i.AnimClip = O as AnimationClip;

					//読み込んだアニメーションのDicをtrueにする
					AllSpecialArtsAnimCompleteFlagDic[i.AnimName] = true;

				}));
			}
		}));

		//超必殺技CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Super/", "csv", (List<object> list) =>
		{
			//全ての特殊技情報を持ったList初期化
			AllSuperArtsList = new List<SuperClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//EnemyAttackClassコンストラクタ代入用変数
				int cid = 0;
				int aid = 0;
				int ul = 0;
				int dwn = 0;
				int lc = 0;
				string nc = "";
				string nh = "";
				string tan = "";
				string aan = "";
				string info = "";
				string vcn = "";
				List<Action<GameObject, GameObject>> sa = new List<Action<GameObject, GameObject>>();

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "UseCharacter": cid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "ArtsIndex": aid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "NameC": nc = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "NameH": nh = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "TryAnimName": tan = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "ArtsAnimName": aan = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "Info": info = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "UnLock": ul = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Down": dwn = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Location": lc = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Vcam": vcn = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
					}
				}

				//セーブデータからアンロック状況を読み込む
				foreach (string ii in UserData.SuperUnLock)
				{
					//リストに名前があればアンロック済み
					if (ii == nc)
					{
						ul = 1;
					}
				}

				//ListにAdd
				AllSuperArtsList.Add(new SuperClass(cid, aid, ul, tan, aan, nc, nh, info, dwn, vcn, lc, sa));
			}

			//アニメーションクリップ読み込み完了判定Dicを作る。
			foreach (SuperClass i in AllSuperArtsList)
			{
				//フラグ管理DicにAdd
				AllSuperArtsAnimCompleteFlagDic.Add(i.TryAnimName, false);
				AllSuperArtsAnimCompleteFlagDic.Add(i.ArtsAnimName, false);
				AllSuperArtsAnimCompleteFlagDic.Add(i.VcamName, false);
			}

			//超必殺技Listを回す
			foreach (SuperClass i in AllSuperArtsList)
			{
				//技のアニメーションを読み込む
				StartCoroutine(LoadOBJ("Anim/Character/" + i.UseCharacter + "/Super/", i.TryAnimName, "anim", (object O) =>
				{
					//ClassのListにAdd
					i.TryAnimClip = O as AnimationClip;

					//読み込んだアニメーションのDicをtrueにする
					AllSuperArtsAnimCompleteFlagDic[i.TryAnimName] = true;
				}));

				//技のアニメーションを読み込む
				StartCoroutine(LoadOBJ("Anim/Character/" + i.UseCharacter + "/Super/", i.ArtsAnimName, "anim", (object O) =>
				{
					//ClassのListにAdd
					i.ArtsAnimClip = O as AnimationClip;

					//読み込んだアニメーションのDicをtrueにする
					AllSuperArtsAnimCompleteFlagDic[i.ArtsAnimName] = true;
				}));

				//カメラワークを読み込む
				StartCoroutine(LoadOBJ("Object/VirtualCamera/Super/", i.VcamName, "prefab", (object O) =>
				{
					//ClassのListにAdd
					i.Vcam = O as GameObject;

					//読み込んだアニメーションのDicをtrueにする
					AllSuperArtsAnimCompleteFlagDic[i.VcamName] = true;
				}));
			}
		}));

		//パーティクルエフェクト読み込み
		StartCoroutine(AllFileLoadCoroutine("Object/Effect/", "prefab", (List<object> list) =>
		{
			//全てのパーティクルエフェクトを持ったリスト初期化
			AllParticleEffectList = new List<GameObject>();

			//読み込んだobjectを回す
			foreach (object i in list)
			{
				//ListにAdd
				AllParticleEffectList.Add(i as GameObject);
			}

			//読み込み完了フラグを立てる
			AllParticleEffectListCompleteFlag = true;

		}));

		//武器CSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Weapon/", "csv", (List<object> list) =>
		{
			//全ての武器情報を持ったリスト初期化
			AllWeaponList = new List<WeaponClass>();

			//WeaponClassコンストラクタ代入用変数
			int CharacterID = 0;
			int WeaponID = 0;
			string WeaponName = "";

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "CharacterID": CharacterID = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "WeaponID": WeaponID = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "WeaponName": WeaponName = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
					}
				}

				//ListにAdd
				AllWeaponList.Add(new WeaponClass(CharacterID, WeaponID, WeaponName));
			}

			//読み込み完了フラグを立てる
			AllWeaponListCompleteFlag = true;

		}));

		//読み込み完了フラグチェックコルーチン呼び出し
		StartCoroutine(AllDetaLoadCompleteCheck());
	}

	private void Update()
	{
		//開発用スイッチON
		if (DevSwicth)
		{
		
		}
		
		//FPS計測関数呼び出し
		FPSFunc();

		//処理落ちの具合に合わせてゲーム速度を変える
		Time.timeScale = FPS / FrameRate * TimeScaleNum;
	}

	//スカイボックスをセットする、シーンの最初に呼び出す
	public void SetSkyBox()
	{
		//スカイボックスを取得
		SkyBoxMaterial = new Material(RenderSettings.skybox);

		//スカイボックスを設定、これをしないと元のスカイボックスの設定が変わってしまう
		RenderSettings.skybox = SkyBoxMaterial;
	}

	//タイムスケール係数を入れる
	public void TimeScaleChange(float t, float s, Action act)
	{
		//タイムスケールを変更
		TimeScaleNum = s;

		//タイムスケール持続コルーチン呼び出し
		StartCoroutine(TimeScaleChangeCoroutine(t, act));
	}
	IEnumerator TimeScaleChangeCoroutine(float t, Action act)
	{
		//経過時間宣言
		float StopTime = 0;

		//引数で受け取った持続時間まで待機
		while (t > StopTime)
		{
			//1フレーム待機
			yield return null;

			//経過時間加算
			StopTime += Time.deltaTime / Time.timeScale;
		}

		//持続時間がゼロならタイムスケールは戻さない
		if (t != 0)
		{
			//タイムスケールを元に戻す
			TimeScaleNum = 1;
		}

		//匿名関数実行
		act();
	}

	//カメラワークの遷移でイージングをするためヴァーチャルカメラを制御する
	public void EasingVcamera()
	{
		//バーチャルカメラPRSをメインカメラに合わせる
		VCamera.gameObject.transform.position = DeepFind(MainCamera , "MainCamera").transform.position;
		VCamera.gameObject.transform.rotation = DeepFind(MainCamera, "MainCamera").transform.rotation;

		//バーチャルカメラ有効化
		VCamera.enabled = true;

		//コルーチン呼び出し
		StartCoroutine(VcameraWaitCoroutine());
	}
	IEnumerator VcameraWaitCoroutine()
	{
		//1フレーム待機
		yield return null;

		//バーチャルカメラ無効化
		VCamera.enabled = false;
	}

	//超必殺技時間停止演出
	public void SuperArtsStopEffect(float t, GameObject e)
	{
		//コルーチン呼び出し
		StartCoroutine(SuperArtsStopEffectCoroutine(t, e));
	}
	private IEnumerator SuperArtsStopEffectCoroutine(float t, GameObject e)
	{
		//経過時間
		float StopTime = 0;

		//レンズフレア演出
		ExecuteEvents.Execute<LensFlareEffectScriptInterface>(DeepFind(gameObject, "LensFlareEffect"), null, (reciever, eventData) => reciever.LensFlareEffect(t));

		//エフェクト再生
		GameObject TempEffect = Instantiate(AllParticleEffectList.Where(a => a.name == "SuperArtsStopTimeEffect").ToArray()[0]);

		//親を設定
		TempEffect.transform.position = PlayableCharacterOBJ.transform.position;

		//ローカルローテーションリセット
		TempEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);

		//プレイヤーポーズ処理
		ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.Pause(true));

		//敵ポーズ処理
		foreach (GameObject i in AllActiveEnemyList.Where(a => a != null))
		{
			ExecuteEvents.Execute<EnemyCharacterInterface>(i, null, (reciever, eventData) => reciever.Pause(true));
			ExecuteEvents.Execute<EnemyBehaviorInterface>(i, null, (reciever, eventData) => reciever.Pause(true));			
		}

		//敵飛び道具ポーズ処理
		foreach (var i in AllEnemyWeaponList.Where(a => a != null))
		{
			//飛び道具オブジェクトにポーズフラグを送る
			ExecuteEvents.Execute<ThrowWeaponScriptInterface>(i, null, (reciever, eventData) => reciever.Pause(true));
		}

		//プレイヤー飛び道具ポーズ処理
		foreach (var i in AllPlayerWeaponList.Where(a => a != null))
		{
			//飛び道具オブジェクトにポーズフラグを送る
			ExecuteEvents.Execute<Character2WeaponColInterface>(i, null, (reciever, eventData) => reciever.Pause(true));
		}		

		while (StopTime < t)
		{
			//経過時間加算
			StopTime += Time.deltaTime;

			//１フレーム待機
			yield return null;
		}

		//プレイヤーポーズ解除
		ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.Pause(false));
	}

	//超必殺技暗転演出
	public void StartSuperArtsLightEffect(float t)
	{
		//スカイボックスをフェードで消す
		StartCoroutine(SkyBoxOffCoroutine(t));

		//他のライトを消す
		ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.LightChange(t, 0.1f, () => { }));
		ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "InDoorLight"), null, (reciever, eventData) => reciever.LightChange(t, 0.1f, () => { }));

		//演出用ライト点灯
		DeepFind(gameObject, "SuperArtsLight").GetComponent<Light>().enabled = true;

		//キャラクターの顔シェーダーのライト切り替え
		ExecuteEvents.Execute<CharacterFaceShaderScriptInterface>(PlayableCharacterOBJ.GetComponentInChildren<CharacterFaceShaderScript>().gameObject, null, (reciever, eventData) => reciever.ChangeLight(DeepFind(gameObject, "SuperArtsLight").transform));
	}
	public void EndSuperArtsLightEffect(float t)
	{
		//スカイボックスをフェードで点ける
		StartCoroutine(SkyBoxOnCoroutine(t));

		//演出用ライト消灯
		DeepFind(gameObject, "SuperArtsLight").GetComponent<Light>().enabled = false;

		//キャラクターの顔シェーダーのライト切り替え
		ExecuteEvents.Execute<CharacterFaceShaderScriptInterface>(PlayableCharacterOBJ.GetComponentInChildren<CharacterFaceShaderScript>().gameObject, null, (rec, eve) => rec.ChangeLight(DeepFind(gameObject, "OutDoorLight").transform));

		//他のライトを点ける
		if (MainCamera.GetComponent<MainCameraScript>().Location.Contains("In"))
		{
			//屋内
			ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.LightChange(t, 0.65f, () => { }));
		}
		else
		{
			//野外
			ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.LightChange(t, 1f, () => { }));
		}
		
		ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "InDoorLight"), null, (reciever, eventData) => reciever.LightChange(t, 0.5f, () =>
		{
			//敵ポーズ解除
			foreach (GameObject i in AllActiveEnemyList)
			{
				if (i != null)
				{
					ExecuteEvents.Execute<EnemyCharacterInterface>(i, null, (rec, eve) => rec.Pause(false));
					ExecuteEvents.Execute<EnemyBehaviorInterface>(i, null, (rec, eve) => rec.Pause(false));
				}
			}

			//飛び道具ポーズ解除
			foreach (var i in AllEnemyWeaponList.Where(a => a != null))
			{
				//飛び道具オブジェクトにポーズフラグを送る
				ExecuteEvents.Execute<ThrowWeaponScriptInterface>(i, null, (rec, eve) => rec.Pause(false));
			}

			//プレイヤー飛び道具ポーズ処理
			foreach (var i in AllPlayerWeaponList.Where(a => a != null))
			{
				//飛び道具オブジェクトにポーズフラグを送る
				ExecuteEvents.Execute<Character2WeaponColInterface>(i, null, (rec, eve) => rec.Pause(false));
			}
		}));

		//メインカメラの超必殺技フラグを下す
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.SetSuperArtsFlag(false));
	}

	//スカイボックスを消す
	private IEnumerator SkyBoxOffCoroutine(float t)
	{
		//係数
		float SkyBoxNum = 1;

		//時間
		float SkyBoxTime = t;

		while(SkyBoxNum > 0)
		{
			//係数を算出
			SkyBoxNum -= (SkyBoxTime - Time.deltaTime) / t;

			//スカイボックスに係数を反映
			SkyBoxMaterial.SetFloat("_Exposure", SkyBoxNum);

			//1フレーム待機
			yield return null;
		}

		//正規化
		SkyBoxMaterial.SetFloat("_Exposure", 0);
	}

	//スカイボックスを点ける
	private IEnumerator SkyBoxOnCoroutine(float t)
	{
		//係数
		float SkyBoxNum = 0;

		//時間
		float SkyBoxTime = 0;

		while (SkyBoxNum < 1)
		{
			//係数を算出
			SkyBoxNum += (SkyBoxTime + Time.deltaTime) / t;

			//スカイボックスに係数を反映
			SkyBoxMaterial.SetFloat("_Exposure", SkyBoxNum);

			//1フレーム待機
			yield return null;
		}

		//正規化
		SkyBoxMaterial.SetFloat("_Exposure", 1);
	}

	//プレイヤーの攻撃時にロック対象を検索する関数、boolがfalseならnullを返す。メッセージシステムから呼び出される
	public GameObject SearchLockEnemy(Vector3 Vec)
	{
		//ロック対象の敵オブジェクトを初期化
		GameObject LockEnemy = null;

		//索敵処理
		if (BattleFlag)
		{
			//ロックする対象を選定するリストを初期化
			LockEnemyList = new List<GameObject>();
			
			//カメラの画角に収まってるかbool
			bool InCameraViewBool = false;

			//死んでるかbool
			bool DestroyBool = false;

			//まずカメラに収まっているか判定
			foreach (GameObject e in AllActiveEnemyList.Where(e => e != null))
			{
				//死んでるか判定
				ExecuteEvents.Execute<EnemyCharacterInterface>(e, null, (reciever, eventData) => DestroyBool = reciever.GetDestroyFlag());

				//死んでる奴はロック対象から外す
				if(!DestroyBool)
				{
					//カメラの画角収まってるか関数呼び出し、変数に代入。
					ExecuteEvents.Execute<EnemyCharacterInterface>(e, null, (reciever, eventData) => InCameraViewBool = reciever.GetOnCameraBool());

					//画面内にいたらリストに追加
					if (InCameraViewBool)
					{
						LockEnemyList.Add(e);
					}
				}
			}

			//画面内に敵がいない場合
			if(LockEnemyList.Count == 0)
			{
				//5m以内の敵を昇順でソート
				List<GameObject> TempEnemyList = AllActiveEnemyList.Where(e => e != null).Where(e => (e.transform.position - PlayableCharacterOBJ.transform.position).sqrMagnitude < Mathf.Pow(5, 2)).OrderBy(e => (e.transform.position - PlayableCharacterOBJ.transform.position).sqrMagnitude).ToList();

				//近い敵をロック
				if (TempEnemyList.Count != 0)
				{
					LockEnemy = TempEnemyList[0];
				}
			}
			//画面内に敵が１人しかいない場合はそいつをロック
			else if (LockEnemyList.Count == 1)
			{
				LockEnemy = LockEnemyList[0];
			}
			//画面内に敵が複数いる場合
			else
			{
				//ロックベクトルにキャラクターの正面を入れる
				Vector3 LockVec = PlayableCharacterOBJ.transform.forward;

				//スティック入力がある場合入力ベクトルをキャッシュ
				if (Vec != Vector3.zero)
				{
					LockVec = Vec;
				}

				//索敵角度を宣言
				float angle = 5;

				//見付かるまで回す
				while (LockEnemy == null || angle < 100)
				{
					//索敵角度の敵を距離でソートしてListにAdd
					List<GameObject> TempEnemyList = LockEnemyList.Where(e => Vector3.Angle(LockVec, HorizontalVector(e, PlayableCharacterOBJ)) < angle).OrderBy(e => (PlayableCharacterOBJ.transform.position - e.transform.position).sqrMagnitude).ToList();
					
					if (TempEnemyList.Count > 0)
					{
						//見付かった敵が10m以内、以上でも索敵角度が45以上ならロック
						if ((TempEnemyList[0].transform.position - PlayableCharacterOBJ.transform.position).sqrMagnitude < Mathf.Pow(10, 2) || angle > 45)
						{
							LockEnemy = TempEnemyList[0];
						}
					}	

					//索敵角度を広げる
					angle += 5;
				}
			}
		}

		//メインカメラにもロック対象を渡す
		//ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

		//選定されたロック対象の敵を出力
		return LockEnemy;
	}

	//プレイアブルキャラクターをセットするインターフェイス
	public void SetPlayableCharacterOBJ(GameObject obj)
	{
		//プレイアブルキャラクターを代入
		PlayableCharacterOBJ = obj;
	}

	//現在のプレイアブルキャラクターを返すインターフェイス
	public GameObject GetPlayableCharacterOBJ()
	{
		return PlayableCharacterOBJ;
	}

	//カメラルート返すインターフェイス
	public GameObject GetCameraOBJ()
	{
		return MainCamera;
	}

	//交代可能な参加メンバーをセットするインターフェイス
	public void SetMissionCharacterDic(Dictionary<int, GameObject> MCD)
	{
		MissionCharacterDic = new Dictionary<int, GameObject>(MCD);
	}

	//バトルフィールドをセットするインターフェイス
	public void SetBattleFieldOBJ(GameObject obj)
	{
		//バトルフィールドを代入
		BattleFieldOBJ = obj;
	}

	//現在のバトルフィールドを返すインターフェイス
	public GameObject GetBattleFieldOBJ()
	{
		return BattleFieldOBJ;
	}

	//操作キャラクターを交代する
	public void ChangePlayableCharacter(bool DownFlag, int c, int n, GameObject e, bool g, float t, bool a, bool f, float d)
	{
		//交代するキャラクターのインデックス宣言
		int NextIndex = c + n;

		//交代するキャラクター宣言
		GameObject NextCharacter = null;

		//ループカウント
		int count = 0;

		//キャラの数だけ回す
		while(count != AllActiveCharacterList.Count - 1)
		{
			if(NextIndex < 0)
			{
				NextIndex = AllActiveCharacterList.Count - 1;
			}
			else if(NextIndex > AllActiveCharacterList.Count - 1)
			{
				NextIndex = 0;
			}

			//キャラを探して取得、見つかったらブレーク
			if (AllActiveCharacterList[NextIndex] != null)
			{
				NextCharacter = AllActiveCharacterList[NextIndex];

				break;
			}

			//カウント増減
			NextIndex += n;

			//カウントアップ
			count++;
		}

		if (DownFlag)
		{
			//ダウンでの交代ならバトルフィールドの初期位置に移動
			NextCharacter.transform.position = DeepFind(BattleFieldOBJ, "PlayerPosOBJ").transform.position;
			NextCharacter.transform.rotation = Quaternion.LookRotation(HorizontalVector(PlayableCharacterOBJ, NextCharacter));
		}
		else
		{
			//位置を合わせる
			NextCharacter.transform.position = PlayableCharacterOBJ.transform.position;
			NextCharacter.transform.rotation = PlayableCharacterOBJ.transform.rotation;
		}

		//キャラクターの出現関数呼び出し
		NextCharacter.GetComponent<PlayerScript>().ChangeAppear(0.25f, g);

		//状況を引き継ぐ
		ExecuteEvents.Execute<PlayerScriptInterface>(NextCharacter, null, (reciever, eventData) => reciever.ContinueSituation(e, g, t, a, f, d));

		//メインカメラにもプレイヤーキャラクターを渡す
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.SetPlayerCharacter(NextCharacter));

		//メインカメラにもロック中の敵を引き継ぐ
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.SetLockEnemy(e));

		//敵にプレイヤーキャラクターを渡す
		foreach (var i in AllActiveEnemyList)
		{
			ExecuteEvents.Execute<EnemyCharacterInterface>(i, null, (reciever, eventData) => reciever.SetPlayerCharacter(NextCharacter));
		}

		//バトルフィールドにプレイヤーキャラクターを渡す
		if(BattleFieldOBJ != null)
		{
			ExecuteEvents.Execute<BattleFieldScriptInterface>(BattleFieldOBJ, null, (reciever, eventData) => reciever.SetPlayerCharacter(NextCharacter));
		}

		//UIを切り替える
		MissionUI.CharacterChange(NextIndex);

		//プレイヤーキャラクターを更新
		SetPlayableCharacterOBJ(NextCharacter);
	}

	//ミッション開始時に初期化を行う関数
	public void StartMission()
	{
		//存在している全てのキャラクターリスト初期化
		AllActiveCharacterList = new List<GameObject>();

		//存在している全てのエネミーリスト初期化
		AllActiveEnemyList = new List<GameObject>();

		//存在している全ての敵飛び道具オブジェクトリスト初期化
		AllEnemyWeaponList = new List<GameObject>();

		//存在している全てのプレイヤー飛び道具オブジェクトリスト初期化
		AllPlayerWeaponList = new List<GameObject>();

		//チャプターID初期化
		SelectedMissionChapter = 0;

		//敵生成中フラグを下す
		GenerateEnemyFlag = false;
	}

	//戦闘一時停止処理
	public void StopBattle(bool b)
	{
		//戦闘中フラグを適応
		BattleFlag = b;

		//全ての敵に戦闘中フラグを送る
		foreach (var i in AllActiveEnemyList)
		{
			if(i != null)
			{
				i.GetComponent<EnemyCharacterScript>().BattleFlag = b;
			}
		}
	}

	//追加されたキャラクターをリストに追加
	public int AddAllActiveCharacterList(GameObject i)
	{
		//呼び出してきたキャラクターをリストに追加
		AllActiveCharacterList.Add(i);

		//呼び出してきたキャラクターにリストのインデックスを送る
		return AllActiveCharacterList.Count - 1;
	}

	//退場したキャラクターをリストから削除する関数、メッセージシステムから呼び出される
	public void RemoveAllActiveCharacterList(int i)
	{
		//退場したキャラクターをリストから削除する、Removeを使うとインデックスがズレるのでnullを入れる
		AllActiveCharacterList[i] = null;
	}

	//追加されたエネミーをリストに追加
	public int AddAllActiveEnemyList(GameObject i)
	{
		//呼び出してきたエネミーをリストに追加
		AllActiveEnemyList.Add(i);

		//呼び出してきたエネミーにリストのインデックスを送る
		return AllActiveEnemyList.Count - 1;
	}

	//退場したエネミーをリストから削除する関数、メッセージシステムから呼び出される
	public void RemoveAllActiveEnemyList(int i)
	{
		//退場したエネミーをリストから削除する、Removeを使うとインデックスがズレるのでnullを入れる
		AllActiveEnemyList[i] = null;
	}

	//ミニマップ表示切り替え
	public void MiniMapSwitch(bool s)
	{
		DeepFind(gameObject, "MiniMap").GetComponent<RawImage>().enabled = s;
	}

	//技クールダウンコルーチン
	public void ArtsCoolDown(ArtsClass Arts, List<List<List<ArtsClass>>> ArtsMatrix)
	{
		StartCoroutine(ArtsCoolDownCoroutine(Arts, ArtsMatrix));
	}
	private IEnumerator ArtsCoolDownCoroutine(ArtsClass Arts, List<List<List<ArtsClass>>> ArtsMatrix)
	{
		//マトリクスの場所取得
		int pos0 = int.Parse(Arts.MatrixPos[0].ToString());
		int pos1 = int.Parse(Arts.MatrixPos[1].ToString());
		int pos2 = int.Parse(Arts.MatrixPos[2].ToString());

		//クールダウンタイム設定
		ArtsMatrix[pos0][pos1][pos2].CoolDownTime = Arts.MaxCoolDownTime;

		//重複実行を防ぐ処理
		if (ArtsMatrix[pos0][pos1][pos2].CoolDownFlag)
		{
			yield break;
		}

		//クールダウンフラグを立てる
		ArtsMatrix[pos0][pos1][pos2].CoolDownFlag = true;

		//クールダウン処理
		while (ArtsMatrix[pos0][pos1][pos2].CoolDownTime > 0)
		{
			if (!GameManagerScript.Instance.PauseFlag)
			{
				//クールダウン時間減算
				ArtsMatrix[pos0][pos1][pos2].CoolDownTime -= Time.deltaTime;		
			}

			//1フレーム待機
			yield return null;
		}

		//クールダウン時間をゼロにリセット
		ArtsMatrix[pos0][pos1][pos2].CoolDownTime = 0;

		//クールダウンフラグを下す
		ArtsMatrix[pos0][pos1][pos2].CoolDownFlag = false;
	}

	//ポーズボタンを押した時
	private void OnPause()
	{
		//全てのデータが読み込み終わっている
		if(AllDetaLoadCompleteFlag)
		{
			//フラグを反転
			PauseFlag = !PauseFlag;

			//プレイヤーキャラクターにポーズフラグを送る
			ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.Pause(PauseFlag));

			//存在している敵を回す
			foreach (GameObject i in AllActiveEnemyList.Where(e => e != null))
			{
				//敵にポーズフラグを送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(i, null, (reciever, eventData) => reciever.Pause(PauseFlag));
				ExecuteEvents.Execute<EnemyBehaviorInterface>(i, null, (reciever, eventData) => reciever.Pause(PauseFlag));
			}

			//存在している飛び道具を回す
			foreach(var i in AllEnemyWeaponList.Where(e => e != null))
			{
				//飛び道具オブジェクトにポーズフラグを送る
				ExecuteEvents.Execute<ThrowWeaponScriptInterface>(i, null, (reciever, eventData) => reciever.Pause(PauseFlag));
			}

			//プレイヤー飛び道具ポーズ処理
			foreach (var i in AllPlayerWeaponList.Where(a => a != null))
			{
				//飛び道具オブジェクトにポーズフラグを送る
				ExecuteEvents.Execute<Character2WeaponColInterface>(i, null, (rec, eve) => rec.Pause(PauseFlag));
			}
		}
	}

	//デバッグキーを押した時
	private void OnDebug(InputValue inputValue)
	{
		//開発フラグが立っていたら処理
		if (DevSwicth)
		{
			//敵の攻撃を喰らった事にする
			ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.HitEnemyAttack(new EnemyAttackClass("", "", "", "", 0, 0, 0, 0, 0, new Color(0, 0, 0, 0), ""), gameObject, null));

			PlayableCharacterOBJ.GetComponent<PlayerScript>().H_Reset();
		}
	}

	//引数のフォルダ内のファイルを全て非同期ロードしてobjectで返す関数
	public IEnumerator AllFileLoadCoroutine(string Dir, string Ext, Action<List<object>> Act)
	{
		//引数の改行コードを削除
		Dir = LineFeedCodeClear(Dir);
		Ext = LineFeedCodeClear(Ext);

		//return用変数宣言
		List<object> re = new List<object>();

		//フォルダ内の全てのファイルのファイルパスを格納用するList
		List<string> FullPathList = new List<string>();

		//ファイルパスの改行コードを削除
		Dir = LineFeedCodeClear(Dir);

		//開発用にリソースから読み込む場合、ビルドすると動かないのでエディタ上のみ
		if (DevSwicth)
		{
			//引数で指定されたフォルダ以下のファイル名を取得
			foreach (FileInfo i in new DirectoryInfo(DataPath + "/Resources/" + Dir).GetFiles("*" + Ext))
			{
				//ファイルのパスをListにAdd
				FullPathList.Add(Dir + Path.GetFileNameWithoutExtension(i.FullName));
			}

			//ファイルのフルパスリストを回す
			foreach (string i in FullPathList)
			{
				//ロード処理を待つためのリクエスト
				ResourceRequest RR = new ResourceRequest();

				//リソース非同期読み込み
				RR = Resources.LoadAsync(i);

				//ロードが終わるまで待つ
				while (RR.isDone == false)
				{
					yield return null;
				}

				//ロードが終わったらreturn用変数に追加
				re.Add(RR.asset);
			}
		}
		//本番用にアセットバンドルから読み込む場合
		else
		{
			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadBundleRequest = new AssetBundleCreateRequest();

			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadDependencyRequest = new AssetBundleCreateRequest();

			//読み込み済みデータList更新
			LoadedDataListUpdate();

			//引数のパスで指定されたフォルダにあるファイル一覧を取得
			foreach (FileInfo i in new DirectoryInfo(Application.streamingAssetsPath + GenerateBundlePath(Dir)).GetFiles("*"))
			{
				//余計なファイルを除外してAssetBundle読み込み、ListにAdd
				if (!Regex.IsMatch(i.Name, "meta|manifest"))
				{
					//ロードするデータが参照している全ての依存データを取得する
					foreach (string ii in DependencyManifest.GetAllDependencies(Dir.ToLower() + i.Name))
					{
						//依存データが未読み込みの場合処理を実行
						if (!LoadedDataList.Contains(ii))
						{
							//依存データをロード
							LoadDependencyRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + ii);

							//ロードが終わるまで待つ
							while (LoadDependencyRequest.isDone == false)
							{
								yield return null;
							}

							//読み込み済みデータList更新
							LoadedDataListUpdate();

							//読み込んだデータを明示的開放
							//LoadDependencyRequest.assetBundle.Unload(false);
						}
					}

					//データが未読み込みの場合処理を実行
					if (!LoadedDataList.Contains(Dir.ToLower() + i.Name))
					{
						//依存データをロードしたら本データをロード
						LoadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + Dir + i.Name);

						//ロードが終わるまで待つ
						while (LoadBundleRequest.isDone == false)
						{
							yield return null;
						}

						//ロードが終わったらreturn用変数に追加
						re.Add(LoadBundleRequest.assetBundle.LoadAsset(i.Name));
						
						//読み込み済みデータList更新
						LoadedDataListUpdate();

						//読み込んだデータを明示的開放
						LoadBundleRequest.assetBundle.Unload(false);
					}
				}
			}
		}

		//呼び出し元から受け取った匿名メソッドにロードしたデータを送る
		Act(re);
	}

	//指定されたオブジェクトをロードして返すコルーチン
	public IEnumerator LoadOBJ(string Dir, string File, string Ext , Action<object> Act)
	{
		//return用変数宣言
		object re = new object();

		//引数の改行コードを削除
		Dir = LineFeedCodeClear(Dir);		
		File = LineFeedCodeClear(File);
		Ext = LineFeedCodeClear(Ext);

		//開発用にリソースから読み込む場合、ビルドすると動かないのでエディタ上のみ
		if (DevSwicth)
		{
			//ロード処理を待つためのリクエスト
			ResourceRequest RR = new ResourceRequest();

			//引数で指定されたフォルダ名と拡張子を外したファイル名でリソース非同期読み込み
			RR = Resources.LoadAsync(Dir + Path.GetFileNameWithoutExtension(File + "." +Ext));

			//ロードが終わるまで待つ
			while (RR.isDone == false)
			{
				yield return null;
			}

			//ロードが終わったらreturn用変数に追加
			re = RR.asset;
		}
		//本番用にアセットバンドルから読み込む場合
		else
		{
			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadBundleRequest = new AssetBundleCreateRequest();

			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadDependencyRequest = new AssetBundleCreateRequest();

			//読み込み済みデータList更新
			LoadedDataListUpdate();

			//ロードするデータが参照している全ての依存データを取得する
			foreach (string ii in DependencyManifest.GetAllDependencies((Dir + File).ToLower()))
			{
				//依存データが未読み込みの場合処理を実行
				if (!LoadedDataList.Contains(ii))
				{                       //依存データをロード
					LoadDependencyRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + ii);

					//ロードが終わるまで待つ
					while (LoadDependencyRequest.isDone == false)
					{
						yield return null;
					}

					//読み込み済みデータList更新
					LoadedDataListUpdate();
				}
			}

			//データが未読み込みの場合処理を実行
			if (!LoadedDataList.Contains((Dir + File).ToLower()))
			{
				//依存データをロードしたら本データをロード
				LoadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GenerateBundlePath(Dir + File));
				
				//ロードが終わるまで待つ
				while (LoadBundleRequest.isDone == false)
				{
					yield return null;
				}
				
				//ロードが終わったらreturn用変数に追加
				re = LoadBundleRequest.assetBundle.LoadAsset(File);

				//読み込み済みデータList更新
				LoadedDataListUpdate();

				//読み込んだデータを明示的開放
				LoadBundleRequest.assetBundle.Unload(false);
			}
			else
			{
				foreach (AssetBundle i in AssetBundle.GetAllLoadedAssetBundles())
				{
					if (i.name == (Dir + File).ToLower())
					{
						re = i.LoadAsset(File);
					}
				}
			}
		}

		//呼び出し元から受け取った匿名メソッドにロードしたデータを送る
		Act(re);
	}

	//セーブデータロード関数
	public void UserDataLoad()
	{
		//セーブデータロードコルーチン呼び出し
		StartCoroutine(UserDataLoadCoroutine());
	}
	//セーブデータロードコルーチン
	private IEnumerator UserDataLoadCoroutine()
	{
		//セーブデータ準備完了フラグを下す
		UserDataReadyFlag = false;

		//セーブフォルダチェックフラグ宣言
		bool SaveFolderCheckCompleteFlag = false;

		//セーブフォルダチェック関数呼び出し
		SaveFolderCheckAsync((bool re) =>
		{
			SaveFolderCheckCompleteFlag = true;
		});

		//処理が終わるまで待つ
		while (!SaveFolderCheckCompleteFlag)
		{
			yield return null;
		}

		//セーブファイルの存在チェック、あったら読み込み
		if (File.Exists(DataPath + "/SaveData/Save"))
		{
			//処理完了フラグ
			bool UserDataLoadCompleteFlag = false;

			//セーブデータロード関数呼び出し、ファイルパスと匿名メソッドを渡してロードした値を受け取る
			UserDataLoadAsync(DataPath + "/SaveData/Save", (UserDataClass LoadFile) =>
			{
				//ロードしたデータが引数で入ってくるのでコピー
				UserData = LoadFile;

				//処理が終わったらフラグを立てる
				UserDataLoadCompleteFlag = true;
			});

			//処理が終わるまで待つ
			while (!UserDataLoadCompleteFlag)
			{
				yield return null;
			}

			//セーブデータ準備完了フラグを立てる
			UserDataReadyFlag = true;
		}
		//無ければ新規作成
		else
		{
			//処理完了フラグ
			bool UserDataSaveCompleteFlag = false;

			//セーブファイル新規作成
			UserDataClass SaveData = new UserDataClass();

			//セーブファイル初期値設定関数呼び出し
			SaveData = CreateSaveData();

			//UserDataにコピー
			UserData = SaveData;

			//セーブデータセーブ関数呼び出し
			UserDataSaveAsync(UserData , DataPath + "/SaveData/Save", () =>
			{
				//処理が終わったらフラグを立てる
				UserDataSaveCompleteFlag = true;
			});

			//処理が終わるまで待つ
			while (!UserDataSaveCompleteFlag)
			{
				yield return null;
			}

			//セーブデータ準備完了フラグを立てる
			UserDataReadyFlag = true;
		}
	}

	//セーブデータセーブ実行関数
	public async void UserDataSaveAsync(UserDataClass SaveData, string path, Action Act)
	{
		//非同期処理
		await Task.Run(() =>
		{
			// バイナリ形式でシリアル化
			BinaryFormatter bf = new BinaryFormatter();

			// 指定したパスにファイルを作成
			FileStream file = File.Create(path);

			//例外処理をしてセーブ
			try
			{
				//セーブデータをシリアル化
				bf.Serialize(file, SaveData);
			}
			finally
			{
				//明示的破棄
				if (file != null)
				{
					file.Close();
				}
			}

			//処理が終わったら匿名関数実行
			Act();
		});
	}

	//セーブデータロード実行関数
	public async void UserDataLoadAsync(string path, Action<UserDataClass> Act)
	{
		//retrun用変数宣言
		UserDataClass re = null;

		//非同期処理でセーブデータ読み込み、結果をreturn用変数に格納
		re = await Task.Run(() =>
		{
			//Task内return用変数
			UserDataClass LoadData = new UserDataClass();

			//セーブファイルの存在確認
			if (File.Exists(path))
			{
				// バイナリ形式でデシリアライズ
				BinaryFormatter bf = new BinaryFormatter();

				// 指定したパスのファイルストリームを開く
				FileStream file = File.Open(path, FileMode.Open);

				//例外処理をしてロード
				try
				{
					// 指定したファイルストリームをオブジェクトにデシリアライズ。
					LoadData = (UserDataClass)bf.Deserialize(file);
				}
				finally
				{
					//明示的破棄
					if (file != null)
					{
						file.Close();
					}
				}
			}

			//出力
			return LoadData;
		});

		//呼び出し元から受け取った匿名関数に読み込んだデータを渡す
		Act(re);
	}

	//セーブフォルダチェック関数
	private async void SaveFolderCheckAsync(Action<bool> Act)
	{
		//出力用bool
		bool re = false;

		//セーブフォルダが無ければ作る
		if (!Directory.Exists(DataPath + "/SaveData"))
		{
			//非同期処理でセーブデータ読み込み、結果をreturn用変数に格納
			re = await Task.Run(() =>
			{
				//セーブフォルダ作成
				Directory.CreateDirectory(DataPath + "/SaveData");

				//処理が終わったらboolを返す
				return true;
			});
		}
		//あったら何もしない
		else
		{
			re = true;
		}

		//呼び出し元から受け取った匿名関数にboolを渡す
		Act(re);
	}

	//セーブバックアップ実行関数
	public async void SaveDataBackUp(string path, string dir, Action Act)
	{
		//非同期処理
		await Task.Run(() =>
		{
			//セーブファイルのリスト宣言
			List<FileInfo> fileinfolist = new List<FileInfo>();

			//セーブファイル名を作成
			string FileName = path + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + "BackUp";
			
			//ファイル名カブりフラグ
			bool NGFileName = false;

			//セーブフォルダ内のファイル数を数える
			foreach (FileInfo i in new DirectoryInfo(dir).GetFiles("*"))
			{
				//metaファイルを除外
				if (!Regex.IsMatch(i.Name, "meta"))
				{
					//リストにadd
					fileinfolist.Add(i);
				}
			}

			//バックアップファイルが10以上あったら古い物を削除
			if(fileinfolist.Count >= 10)
			{
				File.Delete(fileinfolist.Last().FullName);
			}

			//これから作ろうとしているバックアックファイルと同じ名前が無いかチェック
			foreach (FileInfo i in fileinfolist)
			{
				if (i.Name == "Save" + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + "BackUp")
				{
					NGFileName = true;
				}
			}

			//ファイル名がカブってたら文字を足す
			if (NGFileName)
			{
				//セーブファイルをリネームしてバックアップ
				File.Move(dir + "/" + path, dir + "/" + FileName + "a");
			}
			else
			{
				//セーブファイルをリネームしてバックアップ
				File.Move(dir + "/" + path, dir + "/" + FileName);
			}

			//処理が終わったら匿名関数実行
			Act();
		});
	}

	//オートセーブ実行関数
	public void AutoSave(Action Act)
	{
		StartCoroutine(AutoSaveCoroutine(Act));
	}
	IEnumerator AutoSaveCoroutine(Action Act)
	{
		//バックアップ完了フラグ
		bool BackUpFlag = false;

		//セーブ完了フラグ
		bool SaveFlag = false;

		//セーブバックアップ実行関数呼び出し
		GameManagerScript.Instance.SaveDataBackUp("Save", Application.dataPath + "/SaveData", () =>
		{
			//バックアップ完了フラグを立てる
			BackUpFlag = true;
		});

		//バックアップが終わるまで待機
		while (!BackUpFlag)
		{
			yield return null;
		}

		//セーブデータセーブ関数呼び出し
		GameManagerScript.Instance.UserDataSaveAsync(GameManagerScript.Instance.UserData, Application.dataPath + "/SaveData/Save", () =>
		{
			//セーブ完了フラグを立てる
			SaveFlag = true;
		});

		//セーブが終わるまで待機
		while (!SaveFlag)
		{
			yield return null;
		}

		//処理が終わったら匿名関数実行
		Act();
	}

	//アセットバンドル用のパスを作る関数
	private string GenerateBundlePath(string i)
	{
		//文頭にスラッシュをつけて小文字にする
		return "/" + i.ToLower();
	}

	//改行コードを削る関数
	public string LineFeedCodeClear(string i)
	{
		return i.Replace("\r", "").Replace("\n", "");
	}

	//すでに読み込まれているAssetBundleデータのListを更新する関数、ロード前は必ず呼ぶこと。
	public void LoadedDataListUpdate()
	{
		//List初期化
		LoadedDataList = new List<string>();

		//読み込まれているデータ名を調べる
		foreach (AssetBundle i in AssetBundle.GetAllLoadedAssetBundles())
		{
			//Listに格納
			LoadedDataList.Add(i.name);
		}
	}

	//シーン遷移してもユニーク性を保つようにする関数
	private void Singleton()
	{
		//シーン開始時に専用オブジェクトが存在しているかチェック
		GameObject[] obj = GameObject.FindGameObjectsWithTag("UserData");

		if (1 < obj.Length)
		{
			// 既に存在しているなら削除
			Destroy(gameObject);
		}
		else
		{
			// シーン遷移で破棄させない
			DontDestroyOnLoad(gameObject);
		}
	}

	//FPS計測関数
	private void FPSFunc()
	{
		FrameCount++;

		NextTime = Time.realtimeSinceStartup - PrevTime;

		if (NextTime >= 0.5f)
		{
			FPS = FrameCount / NextTime;

			FrameCount = 0;

			PrevTime = Time.realtimeSinceStartup;
		}

		if (FPS > FrameRate)
		{
			FPS = FrameRate;
		}
	}

	//追加要素をチェックして初期設定を行う関数
	private void AddContentCheck()
	{
		//全てのキャラクターを回す
		foreach (CharacterClass i in AllCharacterList)
		{
			//技マトリクスが存在するかチェック、無ければ新規作成
			if(UserData.ArtsMatrix[i.CharacterID] == null)
			{
				//とりあえずnullを入れたカラのArtsMatrixを作る
				List<string> tempArtsMatrix00 = new List<string>();
				List<string> tempArtsMatrix01 = new List<string>();
				List<string> tempArtsMatrix02 = new List<string>();
				List<string> tempArtsMatrix03 = new List<string>();
				List<string> tempArtsMatrix04 = new List<string>();
				List<string> tempArtsMatrix05 = new List<string>();

				tempArtsMatrix00.Add(null);
				tempArtsMatrix00.Add(null);
				tempArtsMatrix00.Add(null);

				tempArtsMatrix01.Add(null);
				tempArtsMatrix01.Add(null);
				tempArtsMatrix01.Add(null);

				tempArtsMatrix02.Add(null);
				tempArtsMatrix02.Add(null);
				tempArtsMatrix02.Add(null);

				tempArtsMatrix03.Add(null);
				tempArtsMatrix03.Add(null);
				tempArtsMatrix03.Add(null);

				tempArtsMatrix04.Add(null);
				tempArtsMatrix04.Add(null);
				tempArtsMatrix04.Add(null);

				tempArtsMatrix05.Add(null);
				tempArtsMatrix05.Add(null);
				tempArtsMatrix05.Add(null);

				List<List<string>> tempArtsMatrix06 = new List<List<string>>();
				List<List<string>> tempArtsMatrix07 = new List<List<string>>();
				List<List<string>> tempArtsMatrix08 = new List<List<string>>();

				tempArtsMatrix06.Add(tempArtsMatrix00);
				tempArtsMatrix06.Add(tempArtsMatrix01);

				tempArtsMatrix07.Add(tempArtsMatrix02);
				tempArtsMatrix07.Add(tempArtsMatrix03);

				tempArtsMatrix08.Add(tempArtsMatrix04);
				tempArtsMatrix08.Add(tempArtsMatrix05);

				List<List<List<string>>> tempArtsMatrix = new List<List<List<string>>>();

				tempArtsMatrix.Add(tempArtsMatrix06);
				tempArtsMatrix.Add(tempArtsMatrix07);
				tempArtsMatrix.Add(tempArtsMatrix08);

				//技リストから初期装備技を検出してリストに入れる
				foreach (ArtsClass ii in AllArtsList)
				{
					if(ii.UseCharacter == i.CharacterID)
					{
						//10以下ってことは初期装備技
						if (ii.UnLock[0] < 10)
						{
							//マトリクスに装備
							tempArtsMatrix[ii.UnLock[0]][ii.UnLock[1]][ii.UnLock[2]] = ii.NameC;

							//技アンロックリストに名前を入れる
							UserData.ArtsUnLock.Add(ii.NameC);
						}
					}
				}

				//UserDataに代入
				UserData.ArtsMatrix[i.CharacterID] = tempArtsMatrix;
			}
		}

		//全ての技を回す
		foreach(ArtsClass i in AllArtsList)
		{
			//初期アンロック技を探す
			if (i.UnLock[0] < 10)
			{
				//ユーザーデータに名前が無ければ追加する
				if(UserData.ArtsUnLock.Where(a => a == i.NameC).ToList().Count == 0)
				{
					UserData.ArtsUnLock.Add(i.NameC);
				}
			}
		}
	}

	//セーブデータ新規作成時に初期値を入れる関数
	public UserDataClass CreateSaveData()
	{
		UserDataClass re = new UserDataClass();

		re.FirstPlay = true;

		re.Score = 0;

		re.ClearMission = 0.0f;

		re.MissionUnlockList = new List<float>();

		//最初のミッションをアンロックしておく
		re.MissionUnlockList.Add(1);

		re.MissionResultList = new List<MissionResultClass>();

		re.ArtsUnLock = new List<string>();

		re.SpecialUnLock = new List<string>();

		re.SuperUnLock = new List<string>();

		re.EquipSuperArts = new List<int>();

		re.ArrowKeyInputAttackUnLock = new List<bool>();

		re.ArtsMatrix = new List<List<List<List<string>>>>();

		re.EquipHairList = new List<int>();

		re.EquipCostumeList = new List<int>();

		re.EquipWeaponList = new List<int>();

		//髪と衣装と武器に初期装備を入れる、適当に10人分、多分そんなに使わない
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipHairList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipCostumeList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.EquipWeaponList.Add(0);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.ArtsMatrix.Add(null);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);
		re.EquipSuperArts.Add(0);


		/*
		//キャラクターの数を求める為のfileinfo配列
		FileInfo[] tempfileinfolist;

		//キャラクター数
		int Num = 0;
		//開発用
		if (DevSwicth)
		{
			tempfileinfolist = new DirectoryInfo(DataPath + "/Resources/csv/Character/").GetFiles();
		}
		//本番用
		else
		{
			tempfileinfolist = new DirectoryInfo(Application.streamingAssetsPath + GenerateBundlePath("/csv/Character/")).GetFiles();
		}
		 
		//キャラクターCSVの数を数える
		foreach(var i in tempfileinfolist)
		{
			//余計なファイルを除外
			if (!Regex.IsMatch(i.Name, "meta|manifest"))
			{
				//キャラクター数カウントアップ
				Num++;
			}
		}
		
		//キャラクターの数だけ回す
		for (int i = 0; i <= Num - 1; i++)
		{
			//髪と衣装と武器に初期装備を入れる、適当に10人分、多分そんなに使わない
			re.EquipHairList.Add(0);
			re.EquipCostumeList.Add(0);
			re.EquipWeaponList.Add(0);

			//装備中の技マトリクスに要素追加
			re.ArtsMatrix.Add(null);

			//レバー入れ攻撃アンロック状況を追加
			re.ArrowKeyInputAttackUnLock.Add(false);

			//装備中の超必殺技を追加、最初は何も装備していないので適当な数字を入れておく
			//re.EquipSuperArts.Add(100);

			//超必殺技デバッグ用
			re.EquipSuperArts.Add(0);
		}
		*/
		//出力
		return re;	
	}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

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

	//ロック対象の敵を返す
	GameObject SearchLockEnemy(bool b, Vector3 Vec);

	//タイムスケール変更
	void TimeScaleChange(float t, float s);
}

//シングルトンなので専用オブジェクトにつけてシーン移動後も常に存在させる
public class GameManagerScript : GlobalClass , GameManagerScriptInterface
{
	//開発用スイッチ
	public bool DevSwicth;

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
	private bool PauseFlag = false;


	//セーブロードするユーザーデータ
	public UserDataClass UserData { get; set; } = null;

	//外部ファイルが置いてあるデータパス
	public string DataPath { get; set; }

	//AssetBudleの依存関係データ
	public AssetBundleManifest DependencyManifest { get; set; }

	//読み込み済みアセットバンドルリスト、重複ロードを防止する
	public List<string> LoadedDataList { get; set; }

	//ユーザーデータ準備完了フラグ
	public bool UserDataReadyFlag { get; set; } = false;






	//選択中のミッションナンバー
	public float SelectedMissionNum { get; set; } = 0;

	//選択中のチャプターナンバー
	public int SelectedMissionChapter { get; set; } = 0;

	//現在のプレイアブルキャラクター
	private GameObject PlayableCharacterOBJ;

	//メインカメラ
	private GameObject MainCamera;

	//ロック対象になる敵を入れるList
	private List<GameObject> LockEnemyList;

	//タイムスケール係数
	float TimeScaleNum = 1;





	//存在している全てのキャラクターリスト
	public List<GameObject> AllActiveCharacterList { get;} = new List<GameObject>();

	//存在している全てのエネミーリスト
	public List<GameObject> AllActiveEnemyList { get; } = new List<GameObject>();

	//全てのキャラクター情報を持ったList
	public List<CharacterClass> AllCharacterList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllCharacterListCompleteFlag = false;

	//全ての敵情報を持ったList
	public List<EnemyClass> AllEnemyList { get; set; }
	//↑の読み込み完了フラグ
	private bool AllEnemyListCompleteFlag = false;

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

	//全ての表情アニメーションを持ったList
	public List<AnimationClip> AllFaceList { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllFaceAnimCompleteFlagDic = new Dictionary<string, bool>();

	//全てのダメージアニメーションを持ったList
	public List<List<AnimationClip>> AllDamageList { get; set; }
	//↑の全てのアニメーションクリップ読み込み完了Dic
	private Dictionary<string, bool> AllDamageAnimCompleteFlagDic = new Dictionary<string, bool>();

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
			AllEnemyAttackAnimCompleteFlagDic.Any() &&
			AllEnemyAttackAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllSpecialArtsAnimCompleteFlagDic.Any() &&
			AllSpecialArtsAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllMissionListCompleteFlag && 
			AllParticleEffectListCompleteFlag &&
			AllWeaponListCompleteFlag &&
			AllArtsAnimCompleteFlagDic.Any()&&
			AllArtsAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllFaceAnimCompleteFlagDic.Any() &&
			AllFaceAnimCompleteFlagDic.All(a => a.Value == true) &&
			AllDamageAnimCompleteFlagDic.Any() &&
			AllDamageAnimCompleteFlagDic.All(a => a.Value == true) &&			
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

		//AssetBudleの依存関係データ読み込み
		DependencyManifest = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/StreamingAssets").LoadAsset<AssetBundleManifest>("AssetBundleManifest");

		//読み込み済みデータList更新
		LoadedDataListUpdate();

		//FPS測定用変数初期化
		FPS = 0;
		FrameCount = 0;
		PrevTime = 0.0f;
		NextTime = 0.0f;

		//キャラクターCSV読み込み
		StartCoroutine(AllFileLoadCoroutine("csv/Character/" , "csv" , (List<object> list) =>
		{
			//全てのキャラクター情報を持ったList初期化
			AllCharacterList = new List<CharacterClass>();

			//読み込んだCSVを回す
			foreach (string i in list.Select(t => t as TextAsset).Select(t => t.text))
			{
				//CharacterClassコンストラクタ代入用変数
				int id = 0;
				//ここは将来的にセーブデータから読み込む
				int Hid = 0;
				int Cid = 0;
				int Wid = 0;
				//
				string LNC ="";
				string LNH ="";
				string FNC ="";
				string FNH = "";
				string ON = "";

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch(ii.Split(',').ToList().First())
					{
						case "CharacterID": id = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "L_NameC": LNC = ii.Split(',').ToList().ElementAt(1); break;
						case "L_NameH": LNH = ii.Split(',').ToList().ElementAt(1); break;
						case "F_NameC": FNC = ii.Split(',').ToList().ElementAt(1); break;
						case "F_NameH": FNH = ii.Split(',').ToList().ElementAt(1); break;
						case "OBJname": ON = ii.Split(',').ToList().ElementAt(1); break;
					}
				}

				//ListにAdd
				AllCharacterList.Add(new CharacterClass(id, LNC, LNH, FNC, FNH, Hid, Cid, Wid, ON));
			}

			//読み込み完了フラグを立てる
			AllCharacterListCompleteFlag = true;

			//表情アニメーションクリップ読み込み完了判定Dicを作る
			AllFaceAnimCompleteFlagDic = new Dictionary<string, bool>();

			//ダメージアニメーションクリップ読み込み完了判定Dicを作る
			AllDamageAnimCompleteFlagDic = new Dictionary<string, bool>();

			//全ての表情アニメーションクリップを持ったList初期化
			AllFaceList = new List<AnimationClip>();

			//全てのダメージモーションを持ったList初期化
			AllDamageList = new List<List<AnimationClip>>();

			//キャラクターリストを回す
			foreach (CharacterClass i in AllCharacterList)
			{
				//表情アニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllFaceAnimCompleteFlagDic.Add(i.F_NameC, false);

				//ダメージアニメーションクリップ読み込み完了判定Dicにキャラ名でAdd
				AllDamageAnimCompleteFlagDic.Add(i.F_NameC, false);

				//表情アニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/Face/", "anim", (List<object> FaceOBJList) =>
				{		
					//読み込んだオブジェクトListを回す
					foreach (object ii in FaceOBJList)
					{	
						//ListにAdd
						AllFaceList.Add(ii as AnimationClip);
					}

					//読み込んだ表情アニメーションのDicをtrueにする
					AllFaceAnimCompleteFlagDic[i.F_NameC] = true;
				}));

				//ダメージアニメーションクリップ読み込み
				StartCoroutine(AllFileLoadCoroutine("Anim/Character/" + i.CharacterID + "/Damage/", "anim", (List<object> DamageOBJList) =>
				{
					//読み込んだアニメーションをListにしてAdd
					AllDamageList.Add(DamageOBJList.Select(o => o as AnimationClip).ToList());

					//読み込んだダメージアニメーションのDicをtrueにする
					AllDamageAnimCompleteFlagDic[i.F_NameC] = true;

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
				int tp = 0;
				string am = "";

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
						case "Type": tp = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Anim": am = ii.Split(',').ToList().ElementAt(1); break;
					}
				}

				//ListにAdd
				AllEnemyAttackList.Add(new EnemyAttackClass(LineFeedCodeClear(id), LineFeedCodeClear(ud), LineFeedCodeClear(an), LineFeedCodeClear(info), dm, tp, LineFeedCodeClear(am)));
			}

			//アニメーションクリップ読み込み完了判定Dicを作る
			foreach(EnemyAttackClass i in AllEnemyAttackList)
			{
				AllEnemyAttackAnimCompleteFlagDic.Add(i.AnimName, false);
			}

			foreach(EnemyAttackClass i in AllEnemyAttackList)
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
		StartCoroutine(AllFileLoadCoroutine("csv/Enemy/" , "csv" , (List<object> list) =>
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
				AllEnemyList.Add(new EnemyClass(LineFeedCodeClear(id), LineFeedCodeClear(objname), LineFeedCodeClear(name), life , stun , downtime,movespeed , turnspeed));
			}

			//読み込み完了フラグを立てる
			AllEnemyListCompleteFlag = true;

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
				List<int> PlayableCharacter = null;
				List<string> ChapterStage = null;
				List<Vector3> CharacterPosListt = new List<Vector3>();
				List<Vector3> CameraPosList = new List<Vector3>();

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

						case "PlayableCharacter":
							PlayableCharacter = new List<int>(ii.Split(',').ToList().ElementAt(1).Split('|').ToList().Select(t => int.Parse(t)));
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

							foreach(var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								//*で分割した値をVector3にしてAdd
								CameraPosList.Add(new Vector3(float.Parse(iii.Split('*').ElementAt(0)), float.Parse(iii.Split('*').ElementAt(1)), float.Parse(iii.Split('*').ElementAt(2))));
							}

							break;
					}
				}
				
				//ListにAdd
				AllMissionList.Add(new MissionClass(Num, MissionTitle, Introduction, PlayableCharacter, ChapterStage, CharacterPosListt, CameraPosList));
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
				List<int> dm = new List<int>();
				List<int> st = new List<int>();
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
								dm.Add(int.Parse(iii));
							}
							
							break;

						case "Stun":

							foreach (var iii in ii.Split(',').ToList().ElementAt(1).Split('|'))
							{
								st.Add(int.Parse(iii));
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
					}
				}

				//ListにAdd
				AllArtsList.Add(new ArtsClass(nc, nh, uc, an, mv, dm, st, cv, kb, intro, lk, mt, at, de, ct, tt, ch, he, hp, ha, hs, cg, hl));
			}
			
			//アニメーションクリップ読み込み完了判定Dicを作る
			foreach(ArtsClass i in AllArtsList)
			{
				AllArtsAnimCompleteFlagDic.Add(i.AnimName, false);
			}

			foreach(ArtsClass i in AllArtsList)
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
				int tr = 0;
				string nc = "";
				string nh = "";
				string an = "";
				string info = "";
				int di = 0;
				string ep = "";
				List<Action<GameObject, GameObject, SpecialClass>> sa = new List<Action<GameObject, GameObject, SpecialClass>>();

				//改行で分割して回す
				foreach (string ii in i.Split('\n').ToList())
				{
					//カンマで分割した最初の要素で条件分岐、続く値を変数に代入
					switch (ii.Split(',').ToList().First())
					{
						case "UseCharacter": cid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "ArtsIndex": aid = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "Trigger": tr = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;
						case "NameC": nc = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1));	break;
						case "NameH": nh = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1));	break;
						case "AnimName": an = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "Info": info = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "EffectPos": ep = LineFeedCodeClear(ii.Split(',').ToList().ElementAt(1)); break;
						case "DamageIndex": di = int.Parse(ii.Split(',').ToList().ElementAt(1)); break;					
					}
				}

				//特殊攻撃処理のListを受け取る
				ExecuteEvents.Execute<SpecialArtsScriptInterface>(gameObject, null, (reciever, eventData) => sa = new List<Action<GameObject, GameObject, SpecialClass>>(reciever.GetSpecialAct(cid , aid)));

				//ListにAdd、アンロック状況は将来的にセーブデータから読み込む
				AllSpecialArtsList.Add(new SpecialClass(cid, aid, 1, nc, nh, an, info, tr, di, ep, sa));
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
				AllWeaponList.Add(new WeaponClass(CharacterID,WeaponID,WeaponName));
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

	//タイムスケール係数を入れる
	public void TimeScaleChange(float t , float s)
	{
		//タイムスケールを変更
		TimeScaleNum = s;

		//タイムスケール持続コルーチン呼び出し
		StartCoroutine(TimeScaleChangeCoroutine(t));
	}
	IEnumerator TimeScaleChangeCoroutine(float t)
	{
		//現在の時間をキャッシュ
		float HitTime = Time.time;

		//引数で受け取った持続時間まで待機
		while (Time.time - HitTime < t * TimeScaleNum)
		{
			yield return null;
		}

		//持続時間がゼロならタイムスケールは戻さない
		if(t != 0)
		{
			//タイムスケールを元に戻す
			TimeScaleNum = 1;
		}
	}

	//プレイヤーの攻撃時にロック対象を検索する関数、boolがfalseならnullを返す。メッセージシステムから呼び出される
	public GameObject SearchLockEnemy(bool b, Vector3 Vec)
	{
		//ロック対象の敵オブジェクトを初期化
		GameObject LockEnemy = null;

		//索敵処理
		if (b)
		{
			//ロックする対象を選定するリストを初期化
			LockEnemyList = new List<GameObject>();
			
			//カメラの画角に収まってるかbool
			bool InCameraViewBool = false;

			//まずカメラに収まっているか判定
			foreach (GameObject e in AllActiveEnemyList.Where(e => e != null))
			{				
				//カメラの画角収まってるか関数呼び出し、変数に代入。
				ExecuteEvents.Execute<EnemyCharacterInterface>(e, null, (reciever, eventData) => InCameraViewBool = reciever.GetOnCameraBool());

				//画面内にいたらリストに追加
				if(InCameraViewBool)
				{
					LockEnemyList.Add(e);
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
		ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.SetLockEnemy(LockEnemy));

		//選定されたロック対象の敵を出力
		return LockEnemy;
	}

	//プレイアブルキャラクターをセットするインターフェイス
	public void SetPlayableCharacterOBJ(GameObject obj)
	{
		PlayableCharacterOBJ = obj;
	}

	//現在のプレイアブルキャラクターを返すインターフェイス
	public GameObject GetPlayableCharacterOBJ()
	{
		return PlayableCharacterOBJ;
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

		//プレイアブルキャラクターの戦闘中フラグを立てる
		ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.SetFightingFlag(true));

		//呼び出してきたエネミーにリストのインデックスを送る
		return AllActiveEnemyList.Count - 1;
	}

	//退場したエネミーをリストから削除する関数、メッセージシステムから呼び出される
	public void RemoveAllActiveEnemyList(int i)
	{
		//退場したエネミーをリストから削除する、Removeを使うとインデックスがズレるのでnullを入れる
		AllActiveEnemyList[i] = null;

		//敵がいなくなったら戦闘中フラグを下ろす
		if(AllActiveEnemyList.All(a => a == null))
		{
			//プレイアブルキャラクターの戦闘中フラグを下ろす
			ExecuteEvents.Execute<PlayerScriptInterface>(PlayableCharacterOBJ, null, (reciever, eventData) => reciever.SetFightingFlag(false));
		}
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
			}
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

	//ユーザーデータロード関数
	public void UserDataLoad()
	{
		//ユーザーデータロードコルーチン呼び出し
		StartCoroutine(UserDataLoadCoroutine());
	}
	//ユーザーデータロードコルーチン
	private IEnumerator UserDataLoadCoroutine()
	{
		//ユーザーデータ準備完了フラグを下す
		UserDataReadyFlag = false;

		//セーブフォルダチェック
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

			//ユーザーデータ準備完了フラグを立てる
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

			//ユーザーデータセーブ関数呼び出し
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

			//ユーザーデータ準備完了フラグを立てる
			UserDataReadyFlag = true;
		}
	}

	//ユーザーデータセーブ実行関数
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

	//ユーザーデータロード実行関数
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
				File.Move(path, FileName + "a");
			}
			else
			{
				//セーブファイルをリネームしてバックアップ
				File.Move(path, FileName);
			}

			//処理が終わったら匿名関数実行
			Act();
		});
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
		foreach(var i in AllCharacterList)
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
							tempArtsMatrix[ii.UnLock[0]][ii.UnLock[1]][ii.UnLock[2]] = ii.NameC;
						}
					}
				}

				//UserDataに代入
				UserData.ArtsMatrix[i.CharacterID] = tempArtsMatrix;
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

		re.ArtsMatrix = new List<List<List<List<string>>>>();

		//装備している髪と衣装と武器、とりあえず00にしておくが後でちゃんと読み込み処理を書く
		re.EquipHairList = new List<int>();
		re.EquipHairList.Add(0);

		re.EquipCostumeList = new List<int>();
		re.EquipCostumeList.Add(0);

		re.EquipWeaponList = new List<int>();
		re.EquipWeaponList.Add(0);

		//キャラクター番号とリストのインデックスを一致させるために予め要素を追加しておく
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

		re.ArrowKeyInputAttackUnLock = new List<List<bool>>();

		//キャラクター番号とリストのインデックスを一致させるために予め要素を追加しておく
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());
		re.ArrowKeyInputAttackUnLock.Add(new List<bool>());

		return re;
	}
}

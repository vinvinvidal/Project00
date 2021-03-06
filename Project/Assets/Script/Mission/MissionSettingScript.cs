using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface MissionSettingScriptInterface : IEventSystemHandler
{
	//モデルを読み込んだりしてミッションの準備をするインターフェイス、チャプター移行時に呼ぶ
	void MissionSetting(int MissionChapterNum);

	//プレイヤー読み込みフラグを受け取る、衣装なんかも読み込むで向こうから受け取る必要がある
	void GetCharacterCompleteFlag(int i, bool b);
}

public class MissionSettingScript : GlobalClass, MissionSettingScriptInterface
{
	//使用するMissionClass
	private MissionClass UseMissionClass;

	//ステージ読み込み完了フラグ
	private bool StageCompleteFlag;

	//キャラクター読み込み完了フラグDic
	private Dictionary<int, bool> CharacterCompleteFlagDic = new Dictionary<int, bool>();

	//ミッション参加キャラクター
	private Dictionary<int, GameObject> MissionCharacterDic = new Dictionary<int, GameObject>();

	void Awake()
    {
		//ゲームマネージャーの初期化関数を呼び出す
		GameManagerScript.Instance.StartMission();

		//全てのミッションリストを回す
		foreach (MissionClass i in GameManagerScript.Instance.AllMissionList)
		{
			//選択中のミッションを抽出
			if (i.Num == GameManagerScript.Instance.SelectedMissionNum)
			{
				//変数に代入
				UseMissionClass = i;
			}
		}

		//キャラクター読み込み完了フラグDicを作る
		foreach (int i in UseMissionClass.MissionCharacterList)
		{
			CharacterCompleteFlagDic.Add(i, false);
		}

		//UIスクリプトにミッションクラスを送る
		GameObject.Find("MissionUI").GetComponent<MissionUIScript>().SettingArtsMatrix(UseMissionClass);

		//野外ライトのライトカラーを設定する
		ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.GradientChange(UseMissionClass.LightColorIndexList[0] , UseMissionClass.LightColorPosList[0]));

		//屋内ライトのライトカラーを設定する
		ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "InDoorLight"), null, (reciever, eventData) => reciever.GradientChange(UseMissionClass.LightColorIndexList[1], UseMissionClass.LightColorPosList[1]));

		//ミッションに参加するキャラクターを全て読み込むコルーチン呼び出し
		StartCoroutine(LoadCharacterCoroutine());

		//ミッション開始時のセッティング、以降のチャプターはGameManagerから呼ぶ
		MissionSetting(0);

		//処理が終わるまで時間を止める
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(0,0, () => { }));
	}

	//ミッションに参加するキャラクターを全て読み込むコルーチン
	private IEnumerator LoadCharacterCoroutine()
	{
		//ミッションに参加するキャラクターを全て読み込む
		foreach (int i in UseMissionClass.MissionCharacterList)
		{
			//キャラクターモデル読み込み
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + i + "/", GameManagerScript.Instance.AllCharacterList[i].OBJname, "prefab", (object OBJ) =>
			{
				//読み込んだオブジェクトをListに追加
				MissionCharacterDic.Add(i, Instantiate(OBJ as GameObject));

				//名前から(Clone)を消す
				MissionCharacterDic[i].name = (OBJ as GameObject).name;
			}));

			//読み込みが終わるまで待って1キャラづつ読み込む、これをしないと重複ロードが起きてエラーになる
			while(!CharacterCompleteFlagDic[i])
			{
				yield return null;
			}
		}
	}

	//モデルを読み込んだりしてミッションの準備をするインターフェイス
	public void MissionSetting(int MissionChapterNum)
	{
		//ステージモデル読み込み
		StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Stage/", UseMissionClass.ChapterStageList[MissionChapterNum], "prefab", (object OBJ) =>
		{
			//読み込んだオブジェクトをインスタンス化、名前から(Clone)を消す
			Instantiate(OBJ as GameObject).name = (OBJ as GameObject).name;

			//読み込み完了フラグを立てる
			StageCompleteFlag = true;
		}));

		//読み込み完了チェックコルーチン呼び出し
		StartCoroutine(CompleteCheck(MissionChapterNum));
	}

	//読み込み完了チェックコルーチン
	IEnumerator CompleteCheck(int MissionChapterNum)
	{
		//外部データの読み込みが完了するまで回る
		while (!(StageCompleteFlag && CharacterCompleteFlagDic.Values.All(a => a)))
		{
			yield return null;
		}

		//初期操作キャラクターを有効化
		MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]].SetActive(true);

		//キャラクターを初期位置に移動、キャラクターコントローラを切らないと直接position指定できない
		MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]].GetComponent<CharacterController>().enabled = false;
		MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]].transform.position = UseMissionClass.PlayableCharacterPosList[MissionChapterNum];
		MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]].GetComponent<CharacterController>().enabled = true;

		//GameManagerにプレイアブルキャラクターを送る
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetPlayableCharacterOBJ(MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]]));

		//GameManagerにミッションキャラクターを送る
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetMissionCharacterDic(MissionCharacterDic));

		//GameManagerにスカイボックスを取得させる
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetSkyBox());

		//メインカメラにプレイアブルキャラクターを送る
		ExecuteEvents.Execute<MainCameraScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "CameraRoot"), null, (reciever, eventData) => reciever.SetPlayerCharacter(MissionCharacterDic[UseMissionClass.FirstCharacterList[MissionChapterNum]]));

		//メインカメラの初期設定関数呼び出し
		ExecuteEvents.Execute<MainCameraScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "CameraRoot"), null, (reciever, eventData) => reciever.MissionCameraSetting());

		//ミニマップ表示
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.MiniMapSwitch(true));

		//読み込み完了したら時間を進める
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(0, 1, () => { }));

		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 2, new Color(1, 1, 1, 1), 1, (GameObject g) =>{ g.GetComponent<Renderer>().enabled = false; }));
	}

	//プレイヤー読み込みフラグを受け取る、衣装なんかも読み込むので向こうの処理から受け取る
	public void GetCharacterCompleteFlag(int i, bool b)
	{
		CharacterCompleteFlagDic[i] = b;
	}
}

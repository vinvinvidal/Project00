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
	void GetCharacterCompleteFlag(bool b);
}

public class MissionSettingScript : GlobalClass, MissionSettingScriptInterface
{
	//使用するMissionClass
	private MissionClass UseMissionClass;

	//ステージ読み込み完了フラグ
	private bool StageCompleteFlag;

	//キャラクター読み込み完了フラグ
	private bool CharacterCompleteFlag;

	//操作キャラクター
	private GameObject PlayableCharacter;

	void Start()
    {
		//ミッション開始時のセッティング、以降のチャプターはGameManagerから呼ぶ
		MissionSetting(0);

		//処理が終わるまで時間を止める
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(0,0));
	}

	//モデルを読み込んだりしてミッションの準備をするインターフェイス
	public void MissionSetting(int MissionChapterNum)
	{
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

		//ステージモデル読み込み
		StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Stage/", UseMissionClass.ChapterStageList[MissionChapterNum], "prefab", (object OBJ) =>
		{
			//読み込んだオブジェクトをインスタンス化、名前から(Clone)を消す
			Instantiate(OBJ as GameObject).name = (OBJ as GameObject).name;

			//読み込み完了フラグを立てる
			StageCompleteFlag = true;
		}));

		//キャラクターモデル読み込み
		StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + UseMissionClass.PlayableCharacterList[MissionChapterNum] + "/", GameManagerScript.Instance.AllCharacterList[UseMissionClass.PlayableCharacterList[MissionChapterNum]].OBJname, "prefab", (object OBJ) =>
		{
			//読み込んだオブジェクトをインスタンス化、プレイアブルキャラクターに代入
			PlayableCharacter = Instantiate(OBJ as GameObject);

			//名前から(Clone)を消す
			PlayableCharacter.name = (OBJ as GameObject).name;

			//キャラクターを初期位置に移動、キャラクターコントローラを切らないと直接position指定できない
			PlayableCharacter.GetComponent<CharacterController>().enabled = false;
			PlayableCharacter.transform.position = UseMissionClass.PlayableCharacterPosList[MissionChapterNum];
			PlayableCharacter.GetComponent<CharacterController>().enabled = true;

		}));

		//読み込み完了チェックコルーチン呼び出し
		StartCoroutine(CompleteCheck());
	}

	//読み込み完了チェックコルーチン
	IEnumerator CompleteCheck()
	{
		//外部データの読み込みが完了するまで回る
		while (!(StageCompleteFlag && CharacterCompleteFlag))
		{
			yield return null;
		}

		//GameManagerにプレイアブルキャラクターを送る
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetPlayableCharacterOBJ(PlayableCharacter));

		//メインカメラにプレイアブルキャラクターを送る
		ExecuteEvents.Execute<MainCameraScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "CameraRoot"), null, (reciever, eventData) => reciever.SetPlayerCharacter(PlayableCharacter));

		//メインカメラの初期設定関数呼び出し
		ExecuteEvents.Execute<MainCameraScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "CameraRoot"), null, (reciever, eventData) => reciever.MissionCameraSetting());

		//読み込み完了したら時間を進める
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.TimeScaleChange(0, 1));

		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 2, new Color(1, 1, 1, 1), 1, (GameObject g) =>{ g.GetComponent<Renderer>().enabled = false; }));
	}

	//プレイヤー読み込みフラグを受け取る、衣装なんかも読み込むので向こうの処理から受け取る
	public void GetCharacterCompleteFlag(bool b)
	{
		CharacterCompleteFlag = b;
	}
}

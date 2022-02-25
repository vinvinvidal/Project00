using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ミッションデータをプロパティに持つクラス
[System.Serializable]
public class MissionClass
{
	//ミッション番号
	public float Num;

	//ミッションのタイトル
	public String MissionTitle;

	//ミッションの説明文
	public String Introduction;

	//ミッションに参加するキャラクターList
	public List<int> MissionCharacterList;

	//チャプターに参加するプレイアブルキャラクターList
	public List<List<int>> ChapterCharacterList;

	//チャプター開始時にプレイヤーが操作するキャラクターList、100なら変更なし
	public List<int> FirstCharacterList;

	//ミッションのチャプター毎の初期表示ステージ
	public List<string> ChapterStageList;

	//ミッションのチャプター毎のキャラクター初期位置
	public List<Vector3> PlayableCharacterPosList;

	//ミッションのチャプター毎のカメラ初期位置
	public List<Vector3> CameraPosList;

	//ミッション開始時に使用するライトカラー用グラデーションインデックス
	public Gradient LightColorIndex;

	//コンストラクタ
	public MissionClass
	(
		float n,
		string title,
		string intro,
		List<int> MCL,
		List<List<int>> CCL,
		List<int> FCL,
		List<string> ST,
		List<Vector3> PPOS,
		List<Vector3> CPOS
	)
	{
		Num = n;
		MissionTitle = title;
		Introduction = intro;
		MissionCharacterList = new List<int>(MCL);
		ChapterCharacterList = new List<List<int>>(CCL);
		FirstCharacterList = new List<int>(FCL);
		ChapterStageList = new List<string>(ST);
		PlayableCharacterPosList = new List<Vector3>(PPOS);
		CameraPosList = new List<Vector3>(CPOS);
	}
}

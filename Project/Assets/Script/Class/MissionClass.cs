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

	//ミッションのチャプター開始時にプレイヤーが操作するキャラクターID、100なら変更なし
	public List<int> PlayableCharacterList;

	//ミッションのチャプター毎の初期表示ステージ
	public List<string> ChapterStageList;

	//ミッションのチャプター毎のキャラクター初期位置
	public List<Vector3> PlayableCharacterPosList;

	//ミッションのチャプター毎のカメラ初期位置
	public List<Vector3> CameraPosList;

	//コンストラクタ
	public MissionClass
	(
		float n,
		string title,
		string intro,
		List<int> CL,
		List<string> ST,
		List<Vector3> PPOS,
		List<Vector3> CPOS
	)
	{
		Num = n;
		MissionTitle = title;
		Introduction = intro;
		PlayableCharacterList = new List<int>(CL);
		ChapterStageList = new List<string>(ST);
		PlayableCharacterPosList = new List<Vector3>(PPOS);
		CameraPosList = new List<Vector3>(CPOS);
	}
}

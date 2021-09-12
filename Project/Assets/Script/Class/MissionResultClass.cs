using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MissionResultClass
{
	//ミッションタイトル
	public string MissionTitle;

	//ミッション番号
	public float Num;

	//クリアフラグ
	public bool ClearFlag;

	//ハイスコア
	public int HiScore;

	//クリアタイム
	public float ClearTime;

	//クリアランク
	public int ClearRank;

	//コンストラクタ
	public MissionResultClass(string i , float n)
	{
		MissionTitle = i;

		Num = n;

		ClearFlag = false;

		HiScore = 0;

		ClearTime = 0;

		ClearRank = 0;
	}
}

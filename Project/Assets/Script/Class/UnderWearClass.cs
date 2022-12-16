using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//下着の情報を格納するクラス
public class UnderWearClass
{
	//キャラクターID
	public int CharacterID;

	//衣装ID
	public int UnderWearID;

	//衣装名
	public string UnderWearName;

	//説明
	public string Information;

	//コンストラクタ
	public UnderWearClass
	(
		int CharaID,
		int CosID,
		string N,
		string I

	)
	{
		CharacterID = CharaID;
		UnderWearID = CosID;
		UnderWearName = N;
		Information = I;
	}
}

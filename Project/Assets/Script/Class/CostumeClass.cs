using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//衣装の情報を格納するクラス
public class CostumeClass
{
	//キャラクターID
	public int CharacterID;

	//衣装ID
	public int CostumeID;

	//衣装名
	public string CostumeName;

	//説明
	public string Information;

	//コンストラクタ
	public CostumeClass
	(
		int CharaID,
		int CosID,
		string N,
		string I

	)
	{
		CharacterID = CharaID;
		CostumeID = CosID;
		CostumeName = N;
		Information = I;
	}
}

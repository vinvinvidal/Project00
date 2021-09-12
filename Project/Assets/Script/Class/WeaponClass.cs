using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponClass
{
	//キャラクターID
	public int CharacterID;

	//武器ID
	public int WeaponID;

	//武器の名前
	public string WeaponName;

	//コンストラクタ
	public WeaponClass(int CID, int WID, string WN)
	{
		CharacterID = CID;
		WeaponID = WID;
		WeaponName = WN;
	}
}

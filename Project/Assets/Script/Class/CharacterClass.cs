using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//キャラクターの情報を格納するクラス
public class CharacterClass
{
	//キャラクターID
	public int CharacterID;

	//オブジェクト名
	public string OBJname;

	//キャラクターの苗字・漢字
	public string L_NameC;

	//キャラクターの苗字・ひらがな
	public string L_NameH;

	//キャラクターの名前・漢字
	public string F_NameC;

	//キャラクターの名前・ひらがな
	public string F_NameH;

	//選択されている髪型ID
	public int HairID;

	//選択されている衣装ID
	public int CostumeID;

	//選択されている武器ID
	public int WeaponID;

	//コンストラクタ
	public CharacterClass
	(
		int id,
		string LNC,
		string LNH,
		string FNC,
		string FNH,
		int Hid,
		int Cid,
		int Wid,
		string ON
	)
	{
		CharacterID = id;
		L_NameC = LNC;
		L_NameH = LNH;
		F_NameC = FNC;
		F_NameH = FNH;
		HairID = Hid;
		CostumeID = Cid;
		WeaponID = Wid;
		OBJname = ON;
	}
}

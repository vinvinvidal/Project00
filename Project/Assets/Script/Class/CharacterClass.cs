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

	//選択されている下着ID
	public int UnderWearID;

	//選択されている武器ID
	public int WeaponID;

	//移動速度
	public float PlayerMoveSpeed;

	//ダッシュ速度
	public float PlayerDashSpeed;

	//ローリング速度
	public float RollingSpeed;

	//ジャンプ力
	public float JumpPower;

	//旋回速度
	public float TurnSpeed;

	//遠近攻撃切り替え距離
	public float AttackDistance;

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
		int Uid,
		int Wid,
		string ON,
		float pms,
		float pds,
		float rs,
		float jp,
		float ts,
		float ad
	)
	{
		CharacterID = id;
		L_NameC = LNC;
		L_NameH = LNH;
		F_NameC = FNC;
		F_NameH = FNH;
		HairID = Hid;
		CostumeID = Cid;
		UnderWearID = Uid;
		WeaponID = Wid;
		OBJname = ON;
		PlayerMoveSpeed = pms;
		PlayerDashSpeed = pds;
		RollingSpeed = rs;
		JumpPower = jp;
		TurnSpeed = ts;
		AttackDistance = ad;
	}
}

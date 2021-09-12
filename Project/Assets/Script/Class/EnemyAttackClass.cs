using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敵の攻撃情報を格納するクラス
public class EnemyAttackClass
{
	//技ID
	public string AttackID;

	//この攻撃を使う敵のID
	public string UserID;

	//攻撃名
	public string AttackName;

	//攻撃の説明文
	public string Info;

	//攻撃力
	public int Damage;

	//攻撃タイプ
	public int AttackType;

	//アニメーションクリップ名
	public string AnimName;

	//アニメーションクリップ本体
	public AnimationClip Anim;

	//コンストラクタ
	public EnemyAttackClass
	(
		string id,
		string ud,
		string an,
		string info,
		int dm,
		int at,
		string am
	)
	{
		AttackID = id;
		UserID = ud;
		AttackName = an;
		Info = info;
		Damage = dm;
		AttackType = at;
		AnimName = am;
	}
}

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
	public float Damage;

	//攻撃タイプ
	public int AttackType;

	/*------//
	0	:通常攻撃
	10	:飛び道具
	
	//------*/

	//ダメージモーションタイプ
	public int DamageType;

	//飛び道具の場合、プレイヤーが打ち返した時のダメージ
	public int PlyaerUseDamage;

	//飛び道具の場合、プレイヤーが打ち返した時のノックバックベクトル
	public Color PlyaerUseKnockBackVec;

	//飛び道具の場合、プレイヤーが打ち返した時のダメージタイプ
	public int PlyaerUseDamageType;

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
		float dm,
		int at,
		int dt,
		int pdm,
		int pdt,
		Color pkb,
		string am
	)
	{
		AttackID = id;
		UserID = ud;
		AttackName = an;
		Info = info;
		Damage = dm;
		AttackType = at;
		DamageType = dt;
		AnimName = am;
		PlyaerUseDamage = pdm;
		PlyaerUseKnockBackVec = pkb;
		PlyaerUseDamageType = pdt;
	}
}

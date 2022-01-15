using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//特殊攻撃を格納するClass
public class SpecialClass
{
	//技の名前
	public string NameC;

	//技の名前のふりがな
	public string NameH;

	//技の説明
	public string Introduction;

	//アンロック状況
	public int UnLock;

	//使用できるキャラクターインデックス
	public int UseCharacter;

	//技インデックス
	public int ArtsIndex;

	//敵側のダメージモーションインデックス
	public int DamageIndex;

	//ダメージ
	public int Damage;

	/*
	40：鍔掃よろけ 
	*/

	//アニメーションクリップ名
	public string AnimName;

	//実際に使用するアニメーションクリップ実体
	public AnimationClip AnimClip;

	//成功エフェクトをアタッチする先
	public string EffectPos;

	//発動条件
	public int Trigger;

	/*
	 0:敵の攻撃が当たった
	 1:こちらの攻撃が当たった
	 */

	//技の処理List
	public List<Action<GameObject, GameObject, GameObject, SpecialClass>> SpecialAtcList;

	//コンストラクタ
	public SpecialClass(int cid, int aid, int ul, string nc, string nh, string an, string info, int tr, int di, int dm, string ep, List<Action<GameObject, GameObject, GameObject, SpecialClass>> sa)
	{
		UseCharacter = cid;
		ArtsIndex = aid;
		UnLock = ul;
		NameC = nc;
		NameH = nh;
		AnimName = an;
		Introduction = info;
		Trigger = tr;
		DamageIndex = di;
		Damage = dm;
		EffectPos = ep;
		SpecialAtcList = new List<Action<GameObject, GameObject, GameObject, SpecialClass>>(sa);
	}
}

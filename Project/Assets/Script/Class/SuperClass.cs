using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//超必殺技を格納するClass
public class SuperClass
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

	//アニメーションクリップ名
	public string TryAnimName;

	//実際に使用するアニメーションクリップ実体
	public AnimationClip TryAnimClip;

	//アニメーションクリップ名
	public string ArtsAnimName;

	//実際に使用するアニメーションクリップ実体
	public AnimationClip ArtsAnimClip;

	//技インデックス
	public int ArtsIndex;

	//技後の敵のダウン状態
	public int Down;

	//当たるロケーション
	public int Location;

	//使用するカメラワークを持っているオブジェクトの名前
	public string VcamName;

	//使用するカメラワークを持っているオブジェクト
	public GameObject Vcam;

	//技の処理List
	public List<Action<GameObject, GameObject>> SuperActList;

	//コンストラクタ
	public SuperClass(int cid, int aid, int ul, string tan, string aan, string nc, string nh, string info, int dwn, string vcn, int lc, List<Action<GameObject, GameObject>> sa)
	{
		UseCharacter = cid;
		UnLock = ul;
		NameC = nc;
		NameH = nh;
		Introduction = info;
		ArtsIndex = aid;
		SuperActList = new List<Action<GameObject, GameObject>>(sa);
		TryAnimName = tan;
		ArtsAnimName = aan;
		Down = dwn;
		VcamName = vcn;
		Location = lc;
	}
}
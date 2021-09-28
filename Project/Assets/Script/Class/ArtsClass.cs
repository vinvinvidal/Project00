using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//技の情報を格納するクラス
public class ArtsClass
{

	//技の名前
	public string NameC;

	//技の名前のふりがな
	public string NameH;

	//使用できるキャラクターインデックス
	public int UseCharacter;

	//アニメーションクリップ名
	public string AnimName;

	//実際に使用するアニメーションクリップ実体
	public AnimationClip AnimClip;

	//威力
	public List<int> Damage;

	//スタン値
	public List<int> Stun;

	//技の移動タイプ
	public List<int> MoveType;

	//技の攻撃タイプ
	public List<int> AttackType;

	//ダウンしている相手に当たるか
	public List<int> DownEnable;

	//コライダの移動タイプ
	public List<int> ColType;

	//技の時間変更タイプ
	public List<int> TimeType;

	//移動ベクトル、RGBにXYZのフラグ（前進ならZに1）アルファに速度を入れる
	public List<Color> MoveVec;

	//当たった敵のノックバックベクトル
	public List<Color> KnockBackVec;

	//コライダの移動ベクトル
	public List<Color> ColVec;

	//ヒットエフェクト
	public List<string> HitEffectList;

	//ヒットエフェクトの出現位置
	public List<Vector3> HitEffectPosList;

	//ヒットエフェクトの角度
	public List<Vector3> HitEffectAngleList;

	//ヒットストップ
	public List<float> HitStop;

	//チェイン攻撃フラグ
	public bool Chain;

	//タメ攻撃威力
	public List<int> ChargeDamage;

	//タメ段階
	public int ChargeLevel;

	//使われたロケーション
	public Dictionary<string, int> UseLocation;

	//技の説明
	public string Introduction;

	//初期装備状況
	public List<int> UnLock;

	//ホールド攻撃時に敵を移動させる場所
	public List<Vector3> HoldPosList;

	//空中技フラグ
	public bool AirAttackFlag;

	/*
	MoveType 技の移動タイプ
	0：地上で完結する移動、踏み外しの対象、
	1：空中で完結する移動、
	2：地上から空中に上がる移動
	3：空中から地上に降りる移動
	4：地上突進移動、相手に当たるとその場で止まる、踏み外しの対象、
	5：空中突進移動、相手に当たるとその場で止まる

	AttackType 技の攻撃タイプ、当たった敵がどう動くか
	0：腹、ダウンしない
	1：腹、ダウンする
	2：腹、吹っ飛ぶ
	3：顔、ダウンしない
	4：顔、ダウンする
	5：顔、吹っ飛ぶ
	6：うつ伏せ叩きつけ
	7：仰向け叩きつけ
	8：足払いされてダウン
	9：背後から衝撃、ダウンしない
	10：背後から衝撃、ダウンする
	11：打ち上げ
	12：左香港スピン
	13：右香港スピン


	20：空中通常攻撃

	30：胸倉掴みホールド立ち
	31：胸倉掴みホールド仰向け
	32：胸倉掴みホールドうつ伏せ

	40：鍔掃崩され



	ColType　コライダの移動タイプ
	0：固定位置上段、普通の攻撃、RGBが出現位置、A半径
	1：固定位置下段、普通の攻撃、RGBが出現位置、A半径
	2：直線移動上段、飛び道具、RGBがベクトル、A半径
	3：直線移動下段、飛び道具、RGBがベクトル、A半径
	4：回転移動上段、周囲を回る、RGBが出現位置、Aが回転方向
	5：回転移動下段、周囲を回る、RGBが出現位置、Aが回転方向
	6：ホールド追撃専用
	7：範囲発生
	8：範囲発生、地上の敵にしか当たらない

	TimeType モーション再生時間変更タイプ
	0：チェイン攻撃、入力か時間経過で解除
	1：降下攻撃、接地で解除
	2：
	3：
	4：
	5：
	100：変更なしの通常技

	UnLock
	Listのインデックス0　0・近距離：1・遠距離：2・空中
	Listのインデックス1　0・レバー入れなし：1・レバー入れあり
	Listのインデックス2　0・Xボタン：1・Yボタン：2・Bボタン

	それぞれに入っている値で初期装備の位置を判別する
	100が入っていたら初期アンロック状態
	200が入っていたら初期ロック状態

	*/

	//コンストラクタ
	public ArtsClass
	(
		string nc,

		string nh,

		int uc,

		string an,

		List<Color> mv,

		List<int> dm,

		List<int> st,

		List<Color> cv,

		List<Color> kb,

		string intro,

		List<int> lk,

		List<int> mt,

		List<int> at,

		List<int> de,

		List<int> ct,

		List<int> tt,

		bool ch,

		List<string> he,

		List<Vector3> hp,

		List<Vector3> ha,

		List<float> hs,

		List<int> cg,

		List<Vector3> hl,

		bool af
	)
	{

		NameC = nc;

		NameH = nh;

		UseCharacter = uc;

		AnimName = an;

		UseLocation = new Dictionary<string, int>();
		UseLocation.Add("Location", 100);
		UseLocation.Add("Stick", 100);
		UseLocation.Add("Button", 100);

		MoveVec = new List<Color>(mv);

		Damage = new List<int>(dm);

		Stun = new List<int>(st);

		ColVec = new List<Color>(cv);

		KnockBackVec = new List<Color>(kb);

		Introduction = intro;

		UnLock = new List<int>(lk);

		MoveType = new List<int>(mt);

		AttackType = new List<int>(at);

		DownEnable = new List<int>(de);

		ColType = new List<int>(ct);

		TimeType = tt;

		Chain = ch;

		HitEffectList = new List<string>(he);

		HitEffectPosList = new List<Vector3>(hp);

		HitEffectAngleList = new List<Vector3>(ha);

		HitStop = new List<float>(hs);

		ChargeDamage = new List<int>(cg);

		ChargeLevel = 0;

		HoldPosList = new List<Vector3>(hl);

		AirAttackFlag = af;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//保存されるユーザーデータ
[System.Serializable]
public class UserDataClass
{
	//初回起動フラグ
	public bool FirstPlay;

	//スコア
	public int Score;

	//直前にクリアしたミッション番号
	public float ClearMission;

	//ミッションのアンロック状況を格納するList
	public List<float> MissionUnlockList;

	//ミッション結果を格納するList
	public List<MissionResultClass> MissionResultList;

	//技のアンロック状況
	public List<string> ArtsUnLock;

	//装備している技のマトリクス
	public List<List<List<List<string>>>> ArtsMatrix;

	//レバー入れ攻撃アンロック状況
	public List<List<bool>> ArrowKeyInputAttackUnLock;

	//装備している髪型リスト
	public List<int> EquipHairList;

	//装備している衣装リスト
	public List<int> EquipCostumeList;

	//装備している武器リスト
	public List<int> EquipWeaponList;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//保存されるセーブデータ
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

	//キャラクターのアンロック状況
	public List<int> CharacterUnLock;

	//技のアンロック状況
	public List<string> ArtsUnLock;

	//特殊技のアンロック状況
	public List<string> SpecialUnLock;

	//超必殺技のアンロック状況
	public List<string> SuperUnLock;

	//装備している超必殺技
	public List<int> EquipSuperArts;

	//装備している技のマトリクス
	public List<List<List<List<string>>>> ArtsMatrix;

	//レバー入れ攻撃アンロック状況
	public List<bool> ArrowKeyInputAttackUnLock;

	//装備している髪型リスト
	public List<int> EquipHairList;

	//装備している衣装リスト
	public List<int> EquipCostumeList;

	//装備している下着リスト
	public List<int> EquipUnderWearList;

	//装備している武器リスト
	public List<int> EquipWeaponList;
}

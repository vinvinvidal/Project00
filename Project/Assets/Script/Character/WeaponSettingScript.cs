using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class WeaponSettingScript : GlobalClass
{
	//武器をアタッチするオブジェクト
	public GameObject WeaponAttachOBJ;

	//武器をアタッチするオブジェクトをアタッチするオブジェクトの名前
	public string WeaponAttachOBJName;

	//武器をアタッチするオブジェクトの相対位置
	public Vector3 WeaponAttachOBJPos;

	//武器をアタッチするオブジェクトの相対回転
	public Vector3 WeaponAttachOBJRotate;

	//攻撃時に武器をアタッチするオブジェクトList
	public List<GameObject> WeaponAttachAttackOBJList;

	//攻撃時に武器をアタッチするオブジェクトをアタッチするオブジェクトの名前List
	public List<string> WeaponAttachAttackNameList;

	//攻撃時に武器をアタッチするオブジェクトの相対位置List
	public List<Vector3> WeaponAttachAttackPosList;

	//攻撃時に武器をアタッチするオブジェクトの相対回転List
	public List<Vector3> WeaponAttachAttackRotateList;

	//武器のクロス用コリジョンオブジェクト
	public GameObject WeaponClothCol;

	//武器のクロス用コリジョンオブジェクトをアタッチするオブジェクトの名前
	public string WeaponClothColName;

	//武器のクロス用コリジョンオブジェクトの相対位置
	public Vector3 WeaponClothColPos;

	//武器のクロス用コリジョンオブジェクトの相対回転
	public Vector3 WeaponClothColRotate;

	void Start()
    {
		//武器をアタッチするオブジェクトをBodyのBoneの子にする
		WeaponAttachOBJ.transform.parent = DeepFind(gameObject.transform.root.gameObject, WeaponAttachOBJName).transform;

		//ローカルTransformを設定
		WeaponAttachOBJ.transform.localPosition = WeaponAttachOBJPos;
		WeaponAttachOBJ.transform.localRotation = Quaternion.Euler(WeaponAttachOBJRotate);

		//武器本体をアタッチするオブジェクトにアタッチ
		transform.parent = WeaponAttachOBJ.transform;

		//ローカルTransformを設定
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.Euler(Vector3.zero);

		//ループインデックス用int
		int count = 0;

		//攻撃時のアタッチオブジェクトを各ボーンにアタッチ
		foreach(GameObject i in WeaponAttachAttackOBJList)
		{
			//攻撃時用アタッチオブジェクトをBoneの子にする
			i.transform.parent = DeepFind(gameObject.transform.root.gameObject, WeaponAttachAttackNameList[count]).transform;
			
			//ローカルTransformを設定
			i.transform.localPosition = WeaponAttachAttackPosList[count];
			i.transform.localRotation = Quaternion.Euler(WeaponAttachAttackRotateList[count]);

			//インデックス用intカウントアップ
			count++;
		}

		//武器のクロス用コリジョン取得、代入用に配列に入れる
		CapsuleCollider[] ColArray = { DeepFind(transform.root.gameObject, "WeaponClothCol").GetComponent<CapsuleCollider>() };

		//武器のクロス用コリジョンを設定する
		gameObject.GetComponentInChildren<Cloth>().capsuleColliders = ColArray;

		//武器のクロス用コリジョンをBodyのBoneの子にする
		WeaponClothCol.transform.parent = DeepFind(gameObject.transform.root.gameObject, WeaponClothColName).transform;

		//ローカルTransformを設定
		WeaponClothCol.transform.localPosition = WeaponClothColPos;
		WeaponClothCol.transform.localRotation = Quaternion.Euler(WeaponClothColRotate);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class WeaponSettingScript : GlobalClass
{
	//武器をアタッチするオブジェクトList
	public List<GameObject> WeaponAttachOBJList;

	//↑をアタッチするオブジェクトの名前List
	public List<string> WeaponAttachOBJNameList;

	//武器をアタッチするオブジェクトの相対位置
	public List<Vector3> WeaponAttachOBJPosList;

	//武器オブジェクトの移動相対位置
	public List<Vector3> WeaponAttachOBJMoveList;

	//武器をアタッチするオブジェクトの相対回転
	public List<Vector3> WeaponAttachOBJRotateList;

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
		//ループカウント
		int count = 0;

		//武器をアタッチするオブジェクトListを回す
		foreach(GameObject i in WeaponAttachOBJList)
		{
			//武器をアタッチするオブジェクトをBodyのBoneの子にする
			i.transform.parent = DeepFind(gameObject.transform.root.gameObject, WeaponAttachOBJNameList[count]).transform;

			//ローカルTransformを設定
			i.transform.localPosition = WeaponAttachOBJPosList[count];
			i.transform.localRotation = Quaternion.Euler(WeaponAttachOBJRotateList[count]);
			i.transform.localScale = Vector3.one;

			//カウントアップ
			count++;
		}

		//武器本体を初期アタッチするオブジェクトにアタッチ
		transform.parent = WeaponAttachOBJList[0].transform;

		//ローカルTransformを設定
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		transform.localScale = Vector3.one;

		//クロスがあればコリジョンを処理
		if (WeaponClothCol != null)
		{
			//武器のクロス用コリジョン取得、代入用に配列に入れる
			CapsuleCollider[] ColArray = { WeaponClothCol.GetComponent<CapsuleCollider>() , null ,null};

			//武器のクロス用コリジョンを設定する
			gameObject.GetComponentInChildren<Cloth>().capsuleColliders = ColArray;

			//武器のクロス用コリジョンをBodyのBoneの子にする
			WeaponClothCol.transform.parent = DeepFind(gameObject.transform.root.gameObject, WeaponClothColName).transform;

			//ローカルTransformを設定
			WeaponClothCol.transform.localPosition = WeaponClothColPos;
			WeaponClothCol.transform.localRotation = Quaternion.Euler(WeaponClothColRotate);
		}
	}
}

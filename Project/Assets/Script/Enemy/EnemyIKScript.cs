using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyArmIKScript : GlobalClass
{
	//リグビルダー
	private RigBuilder ArmRigBuilder;

	//IKコンポーネントデータ
	private TwoBoneIKConstraintData ConstData;

	//IK有効化スイッチ
	private bool EnableSwitch = false;

	//座標を反映するオブジェクトList
	public List<GameObject> AttachOBJList;

    void Start()
    {
		//リグビルダー取得
		ArmRigBuilder = GetComponent<RigBuilder>();

		//IKコンポーネントデータ取得
		ConstData = GetComponentInChildren<TwoBoneIKConstraint>().data;
	}

	private void LateUpdate()
	{
		if(EnableSwitch)
		{
			AttachOBJList[0].transform.position = ConstData.root.transform.position;
			AttachOBJList[0].transform.rotation = ConstData.root.transform.rotation;

			//AttachOBJList[1].transform.rotation = Quaternion.Slerp(AttachOBJList[0].transform.rotation, AttachOBJList[2].transform.rotation, 0.5f);
			AttachOBJList[1].transform.LookAt(AttachOBJList[2].transform, AttachOBJList[0].transform.up);

			AttachOBJList[2].transform.position = ConstData.mid.transform.position;
			AttachOBJList[2].transform.rotation = ConstData.mid.transform.rotation;
		}
	}

	//もしかしたら２回目はうまく行かないか？
	public void EnableIK(GameObject Target)
	{
		StartCoroutine(EnableIKCoroutine(Target));
	}
	private IEnumerator EnableIKCoroutine(GameObject Target)
	{
		ArmRigBuilder.enabled = false;

		yield return null;

		GetComponentInChildren<TwoBoneIKConstraint>().data.target = Target.transform;

		yield return null;

		ArmRigBuilder.enabled = true;
		yield return null;
		EnableSwitch = true;
	}
}

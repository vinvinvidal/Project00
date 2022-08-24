using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyArmIKScript : GlobalClass
{
	//リグビルダー
	private RigBuilder ArmRigBuilder;

	//IKコンストレイントデータ
	private TwoBoneIKConstraint ConstData;

	//IK有効化スイッチ
	private bool EnableSwitch = false;

	//座標を反映するオブジェクトList
	public List<GameObject> AttachOBJList;

	//補間値
	private float LeapNum = 0;

    void Start()
    {
		//リグビルダー取得
		ArmRigBuilder = GetComponent<RigBuilder>();

		//IKコンストレイントデータ取得
		ConstData = GetComponentInChildren<TwoBoneIKConstraint>();
	}

	private void LateUpdate()
	{
		if(EnableSwitch)
		{
			if(LeapNum == 1)
			{
				AttachOBJList[0].transform.position = ConstData.data.root.transform.position;
				AttachOBJList[0].transform.rotation = ConstData.data.root.transform.rotation;

				AttachOBJList[2].transform.position = ConstData.data.mid.transform.position;
				AttachOBJList[2].transform.rotation = ConstData.data.mid.transform.rotation;
			}
			else
			{
				AttachOBJList[0].transform.position = Vector3.Lerp(AttachOBJList[0].transform.position, ConstData.data.root.transform.position, LeapNum);
				AttachOBJList[0].transform.rotation = Quaternion.Lerp(AttachOBJList[0].transform.rotation, ConstData.data.root.transform.rotation, LeapNum);

				//AttachOBJList[1].transform.LookAt(AttachOBJList[2].transform, AttachOBJList[0].transform.up);

				AttachOBJList[2].transform.position = Vector3.Lerp(AttachOBJList[2].transform.position, ConstData.data.mid.transform.position, LeapNum);
				AttachOBJList[2].transform.rotation = Quaternion.Lerp(AttachOBJList[2].transform.rotation, ConstData.data.mid.transform.rotation, LeapNum);
			}
		}
	}

	//もしかしたら２回目はうまく行かないか？
	public void EnableIK(GameObject Target, Vector3 Offset)
	{
		//コルーチン呼び出し
		StartCoroutine(EnableIKCoroutine(Target, Offset));
	}
	private IEnumerator EnableIKCoroutine(GameObject Target, Vector3 Offset)
	{
		//リグビルダー無効化
		ArmRigBuilder.enabled = false;

		//補間値をゼロにしとく
		LeapNum = 0;

		//ターゲットのローカルポジションを設定
		Target.transform.localPosition = Offset;

		//ターゲット設定
		ConstData.data.target = Target.transform;

		//1フレーム待機、これをしないとなんかターゲットが入らない
		yield return null;

		//各コンストレイントの位置を合わせる
		ConstData.data.root.transform.position = AttachOBJList[0].transform.position;
		ConstData.data.root.transform.rotation = AttachOBJList[0].transform.rotation;

		ConstData.data.mid.transform.position = AttachOBJList[2].transform.position;
		ConstData.data.mid.transform.rotation = AttachOBJList[2].transform.rotation;

		//リグビルダー有効化
		ArmRigBuilder.enabled = true;

		//IK有効化スイッチを入れる
		EnableSwitch = true;

		//補間値が１になるまでループ
		while (LeapNum < 1)
		{
			//補間値を加算
			LeapNum += Time.deltaTime * 5;

			//1フレーム待機
			yield return null;
		}

		//補間値を１に
		LeapNum = 1;
	}
}

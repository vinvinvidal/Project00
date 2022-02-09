using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotTouchObjectColScript : GlobalClass
{
	//カメラの移動ターゲット
	GameObject CameraTarget;

	private void Start()
	{
		//カメラの移動ターゲット取得
		CameraTarget = GameObject.Find("MainCameraTarget");
	}

	//触れたオブジェクトを押し返す
	private void OnTriggerStay(Collider other)
	{
		//カメラならターゲットの座標を直接変える
		if (LayerMask.LayerToName(other.gameObject.layer) == "MainCamera")
		{
			CameraTarget.transform.position += HorizontalVector(other.gameObject, gameObject).normalized * 5 * Time.deltaTime;
		}
		//敵とかプレイヤーならキャラクターコントローラに移動ベクトルを与える
		else
		{
			other.gameObject.GetComponent<CharacterController>().Move(HorizontalVector(other.gameObject, gameObject).normalized * 5 * Time.deltaTime);
		}
	}
}

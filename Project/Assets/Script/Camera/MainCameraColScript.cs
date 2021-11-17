using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface MainCameraColScriptInterface : IEventSystemHandler
{
	//ヒット情報を返すインターフェイス
	ControllerColliderHit GetColHit();
}

public class MainCameraColScript : GlobalClass, MainCameraColScriptInterface
{
	//ヒット情報を代入する変数
	ControllerColliderHit CameraColHit = null;

	//コライダにヒットした時刻を記録する変数
	float HitTime = 0;

	private void Update()
	{
		//コライダにヒットしてから一定時間経ったら離れたっぽいのでnullにする
		if (Time.time - HitTime > 0.1f && CameraColHit != null)
		{
			CameraColHit = null;
		}
	}

	//コライダが接触したら呼ばれるコールバック
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//接触した時刻を記録
		HitTime = Time.time;

		//ヒット情報を格納
		CameraColHit = hit;
	}

	//ヒット情報を返すインターフェイス
	public ControllerColliderHit GetColHit()
	{
		return CameraColHit;
	}
}

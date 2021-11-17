using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface OnCameraScriptInterface : IEventSystemHandler
{
	//カメラに表示されていた時刻を返すインターフェイス
	float GetOnCameraTime();
}

//レンダラーがあるオブジェクトにつける事
public class OnCameraScript : GlobalClass, OnCameraScriptInterface
{
	//カメラに表示されていた時刻
	public float OnCameraTime { get; set; }

	//カメラにいる時に呼び出されるコールバック
	void OnWillRenderObject()
	{
		//メインカメラにいる間は時間を記録し続ける
		if(Camera.current.name == "MainCamera")
		{
			OnCameraTime = Time.time;
		}
	}

	//カメラに表示されていた時刻を返すインターフェイス
	public float GetOnCameraTime()
	{
		return OnCameraTime;
	}
}

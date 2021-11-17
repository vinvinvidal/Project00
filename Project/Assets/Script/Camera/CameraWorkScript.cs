using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWorkScript : GlobalClass
{
	//カメラモード
	[Header("カメラモード"), Tooltip("　0　フィックス　\n　1　パストラッキング、片道　\n　2　パストラッキング、往復")]
	public int CameraMode;
		
	//持続条件
	[Header("持続条件"), Tooltip("　0　外部からフラグ変更されるまで持続　\n　1　持続時間が経過するまで持続　\n　2　トラッキングが終了するまで持続")]
	public int KeepMode;

	//遷移モード
	[Header("遷移モード"), Tooltip("　0　カット　\n　1　イージング")]
	public int TransrationMode;

	//次のカメラワークモード
	[Header("次のカメラワークモード"), Tooltip("　0　次のインデックス　\n　1　最初に戻る　\n　2　ランダム　\n　3　インデックス指定　\n　10　ここで終わり")]
	public int NextCameraWorkMode;

	//持っているVcamの中からランダムで再生する
	[Header("持っているVcamの中からランダムで再生するフラグ")]
	public bool RandomFlag;
	[Space(15)]

	//イージング時間
	public float EasingTime;

	//持続時間
	public float KeepTime;

	//移動速度
	public float MoveSpeed;

	//注視点にするオブジェクト名
	public string LookAtOBJName;	
}

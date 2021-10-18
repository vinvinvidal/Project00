using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWorkScript : MonoBehaviour
{
	//カメラモード
	public int CameraMode;
	/*
		0:フィックス
		1:パストラッキング、片道
		2:パストラッキング、往復
	*/

	//持続条件
	public int KeepMode;
	/*
		0:外部からフラグ変更されるまで持続
		1:持続時間が経過するまで持続
		2:トラッキングが終了するまで持続
	*/

	//遷移モード
	public int TransrationMode;
	/*
	 	0:カット
		1:イージング		 
	*/

	//イージング時間
	public float EasingTime;
	
	//次のカメラワークモード
	public int NextCameraWorkMode;
	/*
		0:次のインデックス
		1:最初に戻る
		2:ランダム
		3:インデックス指定
		10:ここで終わり
	*/

	//持続時間
	public float KeepTime;

	//移動速度
	public float MoveSpeed;

	//注視点にするオブジェクト名
	public string LookAtOBJName;
}

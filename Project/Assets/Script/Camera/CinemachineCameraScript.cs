﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CinemachineCameraScript : GlobalClass
{
	//メインカメラ
	private GameObject MainCamera;

	//メインカメラのターゲットオブジェクト
	private GameObject CameraTarget;

	//バーチャルカメラ
	private List<CinemachineVirtualCamera> Vcam;

	//使用するカメラワークオブジェクト
	public List<GameObject> CameraWorkList { get; set; }

	//トラッキングポジション
	private CinemachineTrackedDolly PathPos;

	//カメラワーク持続フラグ
	public bool KeepCameraFlag { get; set; } = true;

	//インデックス指定用
	public int SpecifyIndex { get; set; }

	//デフォルト優先度
	public int DefaultPriority;

	//優先度カウント
	private int PriorityCount = 0;

	//開始時と終了時に使用するVcam
	private CinemachineVirtualCamera MasterVcam;

	void Start()
	{
		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//メインカメラのターゲットオブジェクト取得
		CameraTarget = GameObject.Find("MainCameraTarget");

		//開始時と終了時に使用するVcam
		MasterVcam = DeepFind(GameManagerScript.Instance.gameObject , "MasterVcam").GetComponent<CinemachineVirtualCamera>();

		//カメラワークオブジェクトを取得
		CameraWorkList = new List<GameObject>(gameObject.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("CameraWorkOBJ")).ToList().Select(b => b.gameObject).ToList());
	}

	//カメラワーク再生関数
	public void PlayCameraWork(int Index, bool First)
	{
		//カメラワーク開始時
		if (First)
		{
			//イージング用Vcamera有効化関数呼び出し
			GameManagerScript.Instance.EasingVcamera();

			//優先度カウントリセット
			PriorityCount = 0;

			//全てのカメラワークを回す
			foreach (var i in CameraWorkList)
			{
				foreach(var ii in i.GetComponentsInChildren<CinemachineVirtualCamera>())
				{
					//ヴァーチャルカメラ有効化
					ii.enabled = true;

					//優先度をリセット
					ii.Priority = DefaultPriority;
				}
			}
		}

		//メインカメラのシネマシン有効化
		MainCamera.GetComponent<CinemachineBrain>().enabled = true;

		//カメラワーク持続フラグ初期化
		KeepCameraFlag = true;

		//ヴァーチャルカメラ取得
		Vcam = new List<CinemachineVirtualCamera>(CameraWorkList[Index].GetComponentsInChildren<CinemachineVirtualCamera>());

		//優先度カウントアップ
		PriorityCount++;

		//再生するカメラワークインデックス
		int VcamIndex = 0;

		//ランダムでカメラワークを決める場合
		if (CameraWorkList[Index].GetComponent<CameraWorkScript>().RandomFlag)
		{
			VcamIndex = Random.Range(0, Vcam.Count);
		}	

		//カメラ優先度を上げて切り替える
		Vcam[VcamIndex].Priority += PriorityCount; 

		//注視点オブジェクトが指定されていたら設定
		if(CameraWorkList[Index].GetComponent<CameraWorkScript>().LookAtOBJName != "")
		{
			Vcam[VcamIndex].LookAt = GameObject.Find(CameraWorkList[Index].GetComponent<CameraWorkScript>().LookAtOBJName).transform;
		}

		//遷移モードを設定
		switch (CameraWorkList[Index].GetComponent<CameraWorkScript>().TransrationMode)
		{
			//カット
			case 0:

				//メインカメラの遷移をカットに設定
				MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;

				break;

			//イージング
			case 1:

				//メインカメラの遷移をイージングに設定
				MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;

				//メインカメラのイージングタイムを設定
				MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = CameraWorkList[Index].GetComponent<CameraWorkScript>().EasingTime;

				break;
		}

		//ヴァーチャルカメラのパストラッキング取得
		PathPos = Vcam[VcamIndex].GetCinemachineComponent<CinemachineTrackedDolly>();

		//パストラッキングが存在したら
		if (PathPos != null)
		{
			//トラッキングポジションを初期位置に移動
			PathPos.m_PathPosition = 0;

			//トラッキング制御コルーチン呼び出し
			StartCoroutine(TrackingMoveCoroutine(CameraWorkList[Index].GetComponent<CameraWorkScript>().CameraMode, Index));
		}

		//持続条件によって呼び出すコルーチンを切り替える
		switch (CameraWorkList[Index].GetComponent<CameraWorkScript>().KeepMode)
		{
			//外部からフラグ変更されるまで持続
			case 0:
				StartCoroutine(FlagModeCoroutine(Index));
				break;

			//持続時間が経過するまで持続
			case 1:
				StartCoroutine(TimeModeCoroutine(Index));
				break;

			//トラッキングが終了するまで持続
			case 2:
				StartCoroutine(TrackingEndModeCoroutine(Index));
				break;
		}
	}

	//トラッキング制御コルーチン呼び出し
	IEnumerator TrackingMoveCoroutine(int i , int Index)
	{
		//片道
		if (i == 1)
		{
			//フラグが降りるまで持続
			while (KeepCameraFlag)
			{
				//トラッキング値加算
				PathPos.m_PathPosition += CameraWorkList[Index].GetComponent<CameraWorkScript>().MoveSpeed * Time.deltaTime;

				yield return null;
			}
		}
		//往復
		if (i == 2)
		{
			//サインカープに使うカウント初期化
			float SinCount = -1;

			//フラグが降りるまで持続
			while (KeepCameraFlag)
			{
				//サインカーブで移動
				PathPos.m_PathPosition = (Mathf.Sin(CameraWorkList[Index].GetComponent<CameraWorkScript>().MoveSpeed * SinCount) + 1) * 0.5f;

				//サインカーブ生成に使う数カウントアップ
				SinCount += Time.deltaTime;

				yield return null;
			}
		}
	}

	//トラッキングが終了するまで持続コルーチン
	IEnumerator TrackingEndModeCoroutine(int Index)
	{
		//優先度カウントキャッシュ
		int TempCount = PriorityCount;

		//トラッキングが終わるまで待機
		while (PathPos.m_PathPosition <= 1)
		{
			yield return null;			
		}

		//カメラワーク切り替え関数呼び出し
		StartCoroutine(NextCameraWork(Index));
	}

	//持続時間が経過するまで持続コルーチン
	IEnumerator TimeModeCoroutine(int Index)
	{
		//終了時刻をキャッシュ
		float t = Time.time + CameraWorkList[Index].GetComponent<CameraWorkScript>().KeepTime;

		//終了時間まで待機
		while (t > Time.time)
		{
			yield return null;
		}

		//カメラワーク切り替え関数呼び出し
		StartCoroutine(NextCameraWork(Index));
	}

	//外部からフラグ変更されるまで持続コルーチン
	IEnumerator FlagModeCoroutine(int Index)
	{
		//フラグが降りるまで持続
		while (KeepCameraFlag)
		{
			yield return null;
		}

		//カメラワーク切り替え関数呼び出し
		StartCoroutine(NextCameraWork(Index));
	}

	//カメラワーク切り替え関数
	IEnumerator NextCameraWork(int Index)
	{
		//カメラワーク持続フラグを下ろす
		KeepCameraFlag = false;

		//インデックスを取得
		int NextIndex = Index;
		
		//最後のカメラワークの場合
		if (CameraWorkList[Index].GetComponent<CameraWorkScript>().NextCameraWorkMode == 10)
		{
			//カメラワーク終了関数呼び出し
			EndCameraWork();
		}
		else
		{
			//次のカメラワークを設定
			switch (CameraWorkList[Index].GetComponent<CameraWorkScript>().NextCameraWorkMode)
			{
				//次のインデックス
				case 0:
					NextIndex++;
					break;

				//最初に戻る
				case 1:
					NextIndex = 0;
					break;

				//ランダム
				case 2:
					NextIndex = Random.Range(0, CameraWorkList.Count);
					break;

				//インデックス指定
				case 3:
					NextIndex = SpecifyIndex;
					break;
			}

			//フラグを反映するために１フレーム待機
			yield return null;

			//次のカメラワーク再生
			PlayCameraWork(NextIndex, false);
		}
	}

	//カメラワーク終了関数
	public void EndCameraWork()
	{
		//メインカメラの遷移をイージングに設定
		MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;

		//メインカメラのイージングタイムを設定
		MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 1f;

		//終了用Vcamを通常の注視点に向ける、ポジションは開始時のEasingVcamera()で入る
		MasterVcam.transform.LookAt(GameManagerScript.Instance.GetPlayableCharacterOBJ().transform.position + MainCamera.transform.parent.GetComponent<MainCameraScript>().LookAtOffset);

		//終了用Vcam有効化
		MasterVcam.enabled = true;

		//全てのカメラワークを回す
		foreach (var i in CameraWorkList)
		{
			foreach (var ii in i.GetComponentsInChildren<CinemachineVirtualCamera>())
			{
				//ヴァーチャルカメラ無効化
				ii.enabled = false;

				//優先度をリセット
				ii.Priority = DefaultPriority;
			}
		}

		//コルーチン呼び出し
		StartCoroutine(EndCameraWorkCoroutine());
	}
	//この処理をやらないとコリジョンの外にVcamがあった場合カメラがハマる
	private IEnumerator EndCameraWorkCoroutine()
	{
		//イージング時間待機
		yield return new WaitForSeconds(1);		

		//メインカメラターゲットをキャッシュしてあった位置に移動
		CameraTarget.transform.position = MasterVcam.transform.position;

		//メインカメラのシネマシン無効
		MainCamera.GetComponent<CinemachineBrain>().enabled = false;

		//終了用Vcam無効化
		MasterVcam.enabled = false;
	}
}

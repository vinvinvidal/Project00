using Cinemachine;
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
	private CinemachineVirtualCamera Vcam;

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

	void Start()
	{
		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//メインカメラのターゲットオブジェクト取得
		CameraTarget = GameObject.Find("MainCameraTarget");

		//カメラワークオブジェクトを取得
		CameraWorkList = new List<GameObject>(gameObject.transform.root.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("CameraWorkOBJ")).ToList().Select(b => b.gameObject).ToList());
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
				//優先度をリセット
				i.GetComponentInChildren<CinemachineVirtualCamera>().Priority = DefaultPriority;

				//ヴァーチャルカメラ有効化
				i.GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
			}
		}

		//メインカメラのシネマシン有効無効切り替え
		MainCamera.GetComponent<CinemachineBrain>().enabled = true;

		//カメラワーク持続フラグ初期化
		KeepCameraFlag = true;

		//ヴァーチャルカメラ取得
		Vcam = CameraWorkList[Index].GetComponentInChildren<CinemachineVirtualCamera>();

		//優先度カウントアップ
		PriorityCount++;

		//カメラ優先度を上げて切り替える
		Vcam.Priority += PriorityCount; 

		//注視点オブジェクトが指定されていたら設定
		if(CameraWorkList[Index].GetComponent<CameraWorkScript>().LookAtOBJName != "")
		{
			Vcam.LookAt = GameObject.Find(CameraWorkList[Index].GetComponent<CameraWorkScript>().LookAtOBJName).transform;
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

				MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = CameraWorkList[Index].GetComponent<CameraWorkScript>().EasingTime;

				break;
		}

		//ヴァーチャルカメラのパストラッキング取得
		PathPos = Vcam.GetCinemachineComponent<CinemachineTrackedDolly>();

		//トラッキング制御コルーチン呼び出し
		StartCoroutine(TrackingMoveCoroutine(CameraWorkList[Index].GetComponent<CameraWorkScript>().CameraMode, Index));

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
		//トラッキングポジションを初期位置に移動
		PathPos.m_PathPosition = 0;

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
			//メインカメラのシネマシン無効
			MainCamera.GetComponent<CinemachineBrain>().enabled = false;

			//全てのカメラワークを回す
			foreach (var i in CameraWorkList)
			{
				//ヴァーチャルカメラ無効化
				i.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;
			}

			CameraTarget.transform.position = MainCamera.transform.position;
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
}

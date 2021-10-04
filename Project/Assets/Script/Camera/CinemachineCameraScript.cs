using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CinemachineCameraScript : GlobalClass
{
	//メインカメラ
	private GameObject MainCamera;

	//バーチャルカメラ
	private CinemachineVirtualCamera Vcam;

	//使用するカメラワークオブジェクト
	private List<GameObject> CameraWorkList;

	//カメラワークインデックス
	private int Index = 0;

	//トラッキングポジション
	private CinemachineTrackedDolly PathPos;

	//カメラワーク持続フラグ
	public bool KeepCameraFlag { get; set; } = true;

	void Start()
	{
		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//メインカメラのシネマシン有効無効切り替え
		MainCamera.GetComponent<CinemachineBrain>().enabled = true;

		//カメラワークオブジェクトを取得
		CameraWorkList = new List<GameObject>(gameObject.transform.root.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("CameraWorkOBJ")).ToList().Select(b => b.gameObject).ToList());

		//カメラワーク再生関数呼び出し
		PlayCameraWork();
	}

	//カメラワーク再生関数
	private void PlayCameraWork()
	{
		//カメラワーク持続フラグ初期化
		KeepCameraFlag = true;

		//ヴァーチャルカメラ取得
		Vcam = CameraWorkList[Index].GetComponentInChildren<CinemachineVirtualCamera>();

		//ヴァーチャルカメラ有効化
		Vcam.enabled = true;

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

				break;
		}

		//ヴァーチャルカメラのパストラッキング取得
		PathPos = Vcam.GetCinemachineComponent<CinemachineTrackedDolly>();

		//トラッキング制御コルーチン呼び出し
		StartCoroutine(TrackingMoveCoroutine(CameraWorkList[Index].GetComponent<CameraWorkScript>().CameraMode));

		//持続条件によって呼び出すコルーチンを切り替える
		switch (CameraWorkList[Index].GetComponent<CameraWorkScript>().KeepMode)
		{
			//外部からフラグ変更されるまで持続
			case 0:
				StartCoroutine(FlagModeCoroutine());
				break;

			//持続時間が経過するまで持続
			case 1:
				StartCoroutine(TimeModeCoroutine());
				break;

			//トラッキングが終了するまで持続
			case 2:
				StartCoroutine(TrackingEndModeCoroutine());
				break;
		}
	}

	//トラッキング制御コルーチン呼び出し
	IEnumerator TrackingMoveCoroutine(int i)
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
	IEnumerator TrackingEndModeCoroutine()
	{
		//トラッキングポジションを初期位置に移動
		PathPos.m_PathPosition = 0;

		//トラッキングが終わるまで待機
		while (PathPos.m_PathPosition <= 1)
		{
			yield return null;
		}

		//カメラワーク終了関数呼び出し
		StartCoroutine(EndCameraWork());
	}

	//持続時間が経過するまで持続コルーチン
	IEnumerator TimeModeCoroutine()
	{
		//終了時刻をキャッシュ
		float t = Time.time + CameraWorkList[Index].GetComponent<CameraWorkScript>().KeepTime;

		//終了時間まで待機
		while (t > Time.time)
		{
			yield return null;
		}

		//カメラワーク終了関数呼び出し
		StartCoroutine(EndCameraWork());
	}

	//外部からフラグ変更されるまで持続コルーチン
	IEnumerator FlagModeCoroutine()
	{
		//フラグが降りるまで持続
		while (KeepCameraFlag)
		{
			yield return null;
		}

		//カメラワーク終了関数呼び出し
		StartCoroutine(EndCameraWork());
	}

	//カメラワーク終了関数
	IEnumerator EndCameraWork()
	{
		//ここで終わりなら処理しない
		if(CameraWorkList[Index].GetComponent<CameraWorkScript>().NextCameraWorkMode != 10)
		{
			//カメラワーク持続フラグを下す
			KeepCameraFlag = false;

			//次のカメラワークを設定
			switch (CameraWorkList[Index].GetComponent<CameraWorkScript>().NextCameraWorkMode)
			{
				//次のインデックス
				case 0:
					Index++;
					break;

				//最初に戻る
				case 1:
					Index = 0;
					break;

				//ランダム
				case 2:
					Index = Random.Range(0, CameraWorkList.Count);
					break;
			}

			//フラグを反映するために１フレーム待機
			yield return null;

			//ヴァーチャルカメラ無効化
			Vcam.enabled = false;

			//次のカメラワーク再生
			PlayCameraWork();
		}
	}
}

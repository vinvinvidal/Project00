using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class BattleFieldScript : GlobalClass
{
	//出現する敵ウェーブリスト
	public List<int> EnemyWaveList;

	//出現位置List
	private List<GameObject> SpawnPosList;

	//ウェーブカウント
	private int WaveCount = 0;

	//出現させた敵List
	private List<GameObject> EnemyList;

	//全滅チェックフラグ
	private bool EnemyCheckFlag = false;

	//壁生成スクリプトリスト
	private List<GenerateWallScript> WallGanerateScriptList;

	//戦闘開始用バーチャルカメラスクリプト
	private CinemachineCameraScript BattleStartVcam;

	//次のウェーブ移行用バーチャルカメラスクリプト
	private CinemachineCameraScript BattleNextVcam;

	//プレイヤーキャラクター
	private GameObject PlayerCharacter = null;

	//バーチャルカメラで映す敵、ウェーブリストの最初のヤツ
	private GameObject LookAtEnemy = null;	

	private void Start()
	{
		//出現位置List初期化
		SpawnPosList = new List<GameObject>(gameObject.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("SpawnPos")).Select(a => a.gameObject).ToList());

		//出現させた敵List初期化
		EnemyList = new List<GameObject>();

		//壁生成完了スクリプト取得
		WallGanerateScriptList = new List<GenerateWallScript>(gameObject.GetComponentsInChildren<GenerateWallScript>());

		//戦闘開始用バーチャルカメラスクリプト取得
		BattleStartVcam = DeepFind(gameObject, "BattleFieldCamera_Start").GetComponent<CinemachineCameraScript>();

		//次のウェーブ移行用バーチャルカメラスクリプト
		BattleNextVcam = DeepFind(gameObject, "BattleFieldCamera_Next").GetComponent<CinemachineCameraScript>();

		//敵出現位置List取得
		foreach(Transform i in DeepFind(gameObject, "SpawnPosOBJ").GetComponentsInChildren<Transform>())
		{
			if(i.gameObject != DeepFind(gameObject, "SpawnPosOBJ"))
			{
				SpawnPosList.Add(i.gameObject);
			}			
		}

		//敵初期配置コルーチン呼び出し
		StartCoroutine(StandByCoroutine());
	}

	private void Update()
	{
		//敵全滅チェック
		if (EnemyCheckFlag)
		{
			//敵を全て倒したら処理
			if (EnemyList.All(a => a == null))
			{
				//敵全滅チェック停止
				EnemyCheckFlag = false;

				//最終ウェーブだったらフィールド解除処理
				if (EnemyWaveList.Count == WaveCount)
				{
					//フィールド解除コルーチン呼び出し
					StartCoroutine(ReleaseBattleFieldCoroutine());
				}
				//次のウェーブ出現処理
				else
				{
					//次のウェーブ移行関数呼び出し
					NextWaveCoroutine();
				}
			}
		}
	}

	//次のウェーブ移行コルーチン
	private void NextWaveCoroutine()
	{
		//イベント中フラグを立てる
		GameManagerScript.Instance.EventFlag = true;

		//プレイヤーの次のウェーブ移行関数呼び出し
		ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleEventNext(gameObject, DeepFind(gameObject, "PlayerPosOBJ")));

		//最初のカメラワークの注視点にプレイヤーキャラクターを入れる
		BattleNextVcam.CameraWorkList[0].GetComponentInChildren<CinemachineVirtualCamera>().LookAt = DeepFind(PlayerCharacter, "HeadBone").transform;

		//バーチャルカメラ再生
		BattleNextVcam.PlayCameraWork(0, true);

		//敵配置コルーチン呼び出し
		StartCoroutine(EnemySpawnCoroutine(()=> 
		{
			//敵に戦闘開始フラグを送る
			foreach (var i in EnemyList.Where(a => a != null).ToList())
			{
				i.GetComponent<EnemyCharacterScript>().BattleNext();
			}

			//カメラ演出待ちコルーチン呼び出し
			StartCoroutine(WaitCameraCoroutine(BattleNextVcam.CameraWorkList[0], () =>
			{
				//イベント中フラグを下ろす
				GameManagerScript.Instance.EventFlag = false;

				//プレイヤーキャラクターの戦闘演出終了処理呼び出し
				ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleEventEnd());

				//敵の戦闘フラグを立てる
				foreach (var i in EnemyList.Where(a => a != null).ToList())
				{
					i.GetComponent<EnemyCharacterScript>().BattleFlag = true;
				}

				//全滅チェックフラグを立てる
				EnemyCheckFlag = true;
			}));
		}));
	}

	//敵配置コルーチン
	private IEnumerator EnemySpawnCoroutine(Action act)
	{
		//敵出現カウント
		int EnemyCount = 0;

		//読み込んだ敵List
		List<GameObject> TempEnemyOBJList = new List<GameObject>();

		//出現する敵Listを回す
		foreach (var i in GameManagerScript.Instance.AllEnemyWaveList[EnemyWaveList[WaveCount]].EnemyList)
		{
			//インスタンス生成
			GameObject TempEnemy = null;

			//全ての敵リストからIDで検索
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == i).ToList()[0];

			//敵オブジェクトを読み込む
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
			{
				//読み込んだ敵をインスタンス化
				TempEnemy = O as GameObject;
			}));

			//読み込み完了を待つ
			while (TempEnemy == null)
			{
				yield return null;
			}

			//ListにAdd
			TempEnemyOBJList.Add(TempEnemy);
		}

		//読み込んだ敵オブジェクトをインスタンス化
		foreach (GameObject i in TempEnemyOBJList)
		{
			//インスタンス生成
			GameObject TempEnemy = Instantiate(i);

			//座標を直で変更するためキャラクターコントローラを無効化
			TempEnemy.GetComponent<CharacterController>().enabled = false;

			//ポジション設定、ある程度ランダムに配置
			TempEnemy.transform.position = gameObject.transform.position + SpawnPosList[Random.Range(0, SpawnPosList.Count)].transform.localPosition + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

			//キャラクターコントローラを有効化
			TempEnemy.GetComponent<CharacterController>().enabled = true;

			//出現させた敵Listに追加
			EnemyList.Add(TempEnemy);

			//敵出現カウントアップ
			EnemyCount++;

			//同じ敵を同時に出現させるとSettingで読み込み重複が起こるので終わるまで待機
			while (!TempEnemy.GetComponent<EnemySettingScript>().AllReadyFlag)
			{
				//１フレーム待機
				yield return null;
			}
		}

		//全ての敵が読み込み終わるまで待機
		while(!EnemyList.Where(a => a != null).All(b => b.GetComponent<EnemySettingScript>().AllReadyFlag))
		{
			//１フレーム待機
			yield return null;
		}

		//ウェーブカウントアップ
		WaveCount++;

		//匿名関数実行
		act();
	}

	//敵初期配置コルーチン
	private IEnumerator StandByCoroutine()
	{
		//プレイヤーキャラクターが入るまで待つ
		while (PlayerCharacter == null)
		{
			//プレイヤーキャラクター取得
			PlayerCharacter = GameManagerScript.Instance.GetPlayableCharacterOBJ();

			//１フレーム待機
			yield return null;
		}

		//敵出現カウント
		int EnemyCount = 0;

		//配置用変数
		float PlacementPoint = 0;

		//読み込んだ敵List
		List<GameObject> TempEnemyOBJList = new List<GameObject>();

		//出現する敵Listを回す
		foreach (var i in GameManagerScript.Instance.AllEnemyWaveList[EnemyWaveList[WaveCount]].EnemyList)
		{
			//インスタンス生成
			GameObject TempEnemy = null;

			//全ての敵リストからIDで検索
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == i).ToList()[0];

			//敵オブジェクトを読み込む
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
			{
				//読み込んだ敵をインスタンス化
				TempEnemy = O as GameObject;
			}));

			//読み込み完了を待つ
			while (TempEnemy == null)
			{
				yield return null;
			}

			//ListにAdd
			TempEnemyOBJList.Add(TempEnemy);
		}

		//読み込んだ敵オブジェクトをインスタンス化
		foreach (GameObject i in TempEnemyOBJList)
		{
			//インスタンス生成
			GameObject TempEnemy = Instantiate(i);

			//親をバトルフィールドにする
			TempEnemy.transform.parent = gameObject.transform;

			//座標を直で変更するためキャラクターコントローラを無効化
			TempEnemy.GetComponent<CharacterController>().enabled = false;

			//配置用変数算出
			PlacementPoint = (float)EnemyCount / GameManagerScript.Instance.AllEnemyWaveList[EnemyWaveList[WaveCount]].EnemyList.Count * (2.0f * Mathf.PI);

			//ポジション設定
			TempEnemy.transform.localPosition = new Vector3(Mathf.Cos(PlacementPoint) * 1.5f, 0, Mathf.Sin(PlacementPoint) * 1.5f);

			//ローテンション設定
			TempEnemy.transform.LookAt(gameObject.transform.position - TempEnemy.transform.localPosition);

			//親を解除
			TempEnemy.transform.parent = null;

			//キャラクターコントローラを有効化
			TempEnemy.GetComponent<CharacterController>().enabled = true;

			//出現させた敵Listに追加
			EnemyList.Add(TempEnemy);

			//カメラに映す敵がnullなら入れる
			if(LookAtEnemy == null)
			{
				LookAtEnemy = TempEnemy;
			}

			//敵出現カウントアップ
			EnemyCount++;

			//同じ敵を同時に出現させるとSettingで読み込み重複が起こるので終わるまで待機
			while (!TempEnemy.GetComponent<EnemySettingScript>().AllReadyFlag)
			{
				//１フレーム待機
				yield return null;
			}
		}

		//ウェーブカウントアップ
		WaveCount++;
	}

	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		if(ColHit.gameObject.layer == LayerMask.NameToLayer("Player"))
		{		
			//コライダを無効化
			gameObject.GetComponent<SphereCollider>().enabled = false;

			//バトルフィールドコライダ有効化
			gameObject.GetComponentInChildren<MeshCollider>().enabled = true;

			//イベント中フラグを立てる
			GameManagerScript.Instance.EventFlag = true;

			//ガベージを撒いて壁を作るスクリプトの関数呼び出し
			WallGanerateScriptList.Select(a => StartCoroutine(a.GenerateWallCoroutine())).ToArray();

			//壁生成完了待ちコルーチン呼び出し
			StartCoroutine(WaitWallGenerateCoroutine());

			//最初のカメラワークの注視点にプレイヤーキャラクターを入れる
			BattleStartVcam.CameraWorkList[0].GetComponentInChildren<CinemachineVirtualCamera>().LookAt = DeepFind(PlayerCharacter, "HeadBone").transform;

			//次のカメラワークの注視点に敵キャラクターを入れる
			BattleStartVcam.CameraWorkList[1].GetComponentInChildren<CinemachineVirtualCamera>().LookAt = DeepFind(LookAtEnemy, "HeadBone").transform;

			//次のカメラワークの注視点に敵キャラクターを入れる
			BattleStartVcam.CameraWorkList[2].GetComponentInChildren<CinemachineVirtualCamera>().LookAt = DeepFind(LookAtEnemy, "HeadBone").transform;

			//バーチャルカメラ再生
			BattleStartVcam.PlayCameraWork(0, true);

			//プレイアブルキャラクターの戦闘演出開始処理関数呼び出し
			ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleEventStart(gameObject, DeepFind(gameObject, "PlayerPosOBJ")));

			//カメラ演出待ちコルーチン呼び出し
			StartCoroutine(WaitCameraCoroutine(BattleStartVcam.CameraWorkList[0], () =>
			{
				//敵に戦闘開始フラグを送る
				foreach (var i in EnemyList.Where(a => a != null).ToList())
				{
					i.GetComponent<EnemyCharacterScript>().BattleStart();
				}

				//カメラ演出待ちコルーチン呼び出し
				StartCoroutine(WaitCameraCoroutine(BattleStartVcam.CameraWorkList[1], () =>
				{
					//カメラ演出待ちコルーチン呼び出し
					StartCoroutine(WaitCameraCoroutine(BattleStartVcam.CameraWorkList[2], () =>
					{
						//イベント中フラグを下ろす
						GameManagerScript.Instance.EventFlag = false;

						//プレイヤーキャラクターの戦闘演出終了処理呼び出し
						ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleEventEnd());

						//敵の戦闘フラグを立てる
						foreach (var i in EnemyList.Where(a => a != null).ToList())
						{
							i.GetComponent<EnemyCharacterScript>().BattleFlag = true;
						}

						//全滅チェックフラグを立てる
						EnemyCheckFlag = true;

					}));
				}));
			}));
		}
	}

	//カメラ演出待ちコルーチン
	private IEnumerator WaitCameraCoroutine(GameObject Vcam, Action Act)
	{
		//カメラワーク終了値宣言
		float EndNum = 0;

		//フィックス
		if (Vcam.GetComponent<CameraWorkScript>().CameraMode == 0)
		{
			//終了が経過時間
			if(Vcam.GetComponent<CameraWorkScript>().KeepMode == 1)
			{
				//経過時間が過ぎるまでループ
				while (EndNum < Vcam.GetComponent<CameraWorkScript>().KeepTime)
				{
					//経過時間カウントアップ
					EndNum += Time.deltaTime;

					//1フレーム待機
					yield return null;
				}
			}
		}
		//パストラッキング片道
		else if(Vcam.GetComponent<CameraWorkScript>().CameraMode == 1)
		{				
			//終了がパスユニット
			if(Vcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_PositionUnits == CinemachinePathBase.PositionUnits.PathUnits)
			{
				//終了値をパス数に設定
				EndNum = Vcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_Path.MaxPos;
			}

			//ポジションが終了値に達するまでループ
			while (EndNum > Vcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition)
			{
				//1フレーム待機
				yield return null;
			}
		}

		//匿名関数実行
		Act();
	}

	//壁生成完了待ちコルーチン
	private IEnumerator WaitWallGenerateCoroutine()
	{
		//完了フラグが全て立つまで待機
		while (!WallGanerateScriptList.All(a => a.CompleteFlag))
		{
			//１フレーム待機
			yield return null;
		}

		//壁が落ち着くまでちょっと待つ
		yield return new WaitForSeconds(2.5f);

		//物理挙動を止める
		foreach (var i in gameObject.GetComponentsInChildren<Rigidbody>())
		{
			i.isKinematic = true;
		}
	}

	//フィールド解放コルーチン
	private IEnumerator ReleaseBattleFieldCoroutine()
	{
		//コライダ無効化
		DeepFind(gameObject, "FieldCol").GetComponent<MeshCollider>().enabled = false;

		//壁オブジェクト取得
		List<GameObject> OBJList = new List<GameObject>(DeepFind(gameObject, "WallOBJ").GetComponentsInChildren<Transform>().Where(a => a.gameObject.layer == LayerMask.NameToLayer("PhysicOBJ")).Select(b => b.gameObject).ToList());

		//壁オブジェクトのRigitBodyを回す
		foreach (Rigidbody i in OBJList.Select(a => a.GetComponent<Rigidbody>()))
		{
			//オブジェクトに壁消失用スクリプト追加
			i.gameObject.AddComponent<WallVanishScript>();

			//物理挙動有効化
			i.isKinematic = false;

			//外側に力を与えて壁を崩す
			i.AddForce((i.transform.position - gameObject.transform.position + Vector3.up) * 2.5f, ForceMode.Impulse);
		}

		//壁オブジェクトが全部消えるまでループ
		while (!OBJList.All(a => a == null))
		{
			yield return null;
		}

		//フィールド削除
		Destroy(gameObject);
	}
}

using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface BattleFieldScriptInterface : IEventSystemHandler
{
	void SetPlayerCharacter(GameObject p);
}

public class BattleFieldScript : GlobalClass, BattleFieldScriptInterface
{
	//出現する敵ウェーブリスト
	public List<int> EnemyWaveList;

	//このバトルフィールドに出現する全ての敵オブジェクトList
	private List<List<GameObject>> AllEnemyList;

	//出現する全ての敵オブジェクト準備完了フラグ
	private bool GenerateEnemyFlag = false;

	//出現させた敵List
	private List<GameObject> EnemyList;

	//最後の１人
	private GameObject LastEnemy = null;

	//全滅チェックフラグ
	private bool EnemyCheckFlag = false;

	//ウェーブカウント
	private int WaveCount = 0;

	//最後のウェーブフラグ
	private bool LastWaveFlag = false;

	//出現位置List
	private List<GameObject> SpawnPosList;

	//壁生成オブジェクト
	private GameObject WallOBJ;

	//コライダオブジェクト
	private GameObject ColOBJ;

	//壁素材オブジェクト
	private List<GameObject> WallMaterial;

	//プレイヤーキャラクター
	private GameObject PlayerCharacter = null;

	//プレイヤーキャラクターポジションオブジェクト
	private GameObject PlayerPosOBJ;

	//コライダ　
	private SphereCollider BattleFieldCol;

	//戦闘継続演出用ヴァーチャルカメラ
	private CinemachineCameraScript BattleNextVcam;

	//戦闘勝利演出用ヴァーチャルカメラ
	private CinemachineCameraScript BattleVictoryVcam;

	private void Start()
	{
		//敵出現位置List初期化
		SpawnPosList = new List<GameObject>(gameObject.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("SpawnPos")).Select(a => a.gameObject).ToList());

		//このバトルフィールドに出現する全ての敵オブジェクトList初期化
		AllEnemyList = new List<List<GameObject>>();

		//コライダオブジェクト取得
		ColOBJ = DeepFind(gameObject, "ColOBJ");

		//壁生成オブジェクト取得
		WallOBJ = DeepFind(gameObject, "WallOBJ");

		//壁素材オブジェクト取得
		WallMaterial = new List<GameObject>(WallOBJ.GetComponentsInChildren<Rigidbody>().Select(a => a.gameObject).ToList());

		//演戦闘継続演出用ヴァーチャルカメラ取得
		BattleNextVcam = DeepFind(gameObject, "BattleFieldCamera_Next").GetComponent<CinemachineCameraScript>();

		//演戦闘勝利演出用ヴァーチャルカメラ取得
		BattleVictoryVcam = DeepFind(gameObject, "BattleFieldCamera_Victory").GetComponent<CinemachineCameraScript>();

		//コライダ取得
		BattleFieldCol = GetComponent<SphereCollider>();

		//取得したらすぐに非活性化しておく
		foreach (var i in WallMaterial)
		{
			i.SetActive(false);
		}

		//プレイヤーキャラクターポジションオブジェクト取得
		PlayerPosOBJ = DeepFind(gameObject, "PlayerPosOBJ");

		//プレイヤーキャラクター取得コルーチン呼び出し
		StartCoroutine(GetPlayerCharacterCoroutine());

		//敵オブジェクト読み込みコルーチン呼び出し
		StartCoroutine(GenerateEnemyCoroutine());

		//敵配置コルーチン呼び出し
		StartCoroutine(StandByCoroutine(() => { }));
	}

	private void Update()
	{
		//敵全滅チェック
		if(EnemyCheckFlag)
		{
			//最後の１人を取得
			if (LastWaveFlag && LastEnemy == null && GameManagerScript.Instance.AllActiveEnemyList.Where(a => a !=null).ToList().Count == 1)
			{
				LastEnemy = GameManagerScript.Instance.AllActiveEnemyList.Where(a => a != null).ToList()[0];
			}

			if(GameManagerScript.Instance.AllActiveEnemyList.All(a => a == null))
			{
				//敵全滅チェック停止
				EnemyCheckFlag = false;

				//最終ウェーブ
				if (LastWaveFlag)
				{
					//勝利処理
					Victory();
				}
				else
				{
					//次のウェーブ処理
					NextWave();
				}
			}
		}
	}

	//出現する全ての敵を生成するコルーチン
	private IEnumerator GenerateEnemyCoroutine()
	{
		//全てのウェーブを回す
		foreach (var i in EnemyWaveList)
		{
			//Add用敵リスト初期化
			List<GameObject> TempEnemyList = new List<GameObject>();

			//全ての敵を回す
			foreach (var ii in GameManagerScript.Instance.AllEnemyWaveList.Where(a => a.WaveID == i).ToArray()[0].EnemyList)
			{
				//全ての敵リストからIDでクラスを取得
				EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == ii).ToList()[0];

				//インスタンス用オブジェクト宣言
				GameObject TempEnemy = null;

				//敵オブジェクトを読み込む
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
				{
					//読み込んだ敵を代入
					TempEnemy = O as GameObject;
				}));

				//読み込み完了を待つ
				while (TempEnemy == null)
				{
					yield return null;
				}

				//トランスフォームリセット
				ResetTransform(TempEnemy);

				//ListにAdd
				TempEnemyList.Add(Instantiate(TempEnemy));
			}

			//ListにAdd
			AllEnemyList.Add(TempEnemyList);
		}

		//全ての敵の準備が終わるまで待つ
		while(!AllEnemyList.All(a => a.All(b => b.GetComponent<EnemySettingScript>().AllReadyFlag)))
		{
			yield return null;
		}

		//フラグを立てる
		GenerateEnemyFlag = true;
	}

	//プレイヤーキャラクターをセットするインターフェイス
	public void SetPlayerCharacter(GameObject p)
	{
		PlayerCharacter = p;
	}

	//勝利処理関数
	private void Victory()
	{
		//勝利処理コルーチン呼び出し
		StartCoroutine(VictoryCoroutine());
	}
	//勝利処理コルーチン
	private IEnumerator VictoryCoroutine()
	{
		//ゲームマネージャーの戦闘フラグを下す
		GameManagerScript.Instance.BattleFlag = false;

		//ゲームマネージャーのバトルフィールドをnullにする
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetBattleFieldOBJ(null));

		//壁オブジェクト取得
		List<GameObject> OBJList = new List<GameObject>(DeepFind(gameObject, "WallOBJ").GetComponentsInChildren<Transform>().Where(a => a.gameObject.layer == LayerMask.NameToLayer("PhysicOBJ")).Select(b => b.gameObject).ToList());

		//壁コライダ無効化
		ColOBJ.GetComponent<MeshCollider>().enabled = false;

		//プレイヤーキャラクターの戦闘終了処理実行
		PlayerCharacter.GetComponent<PlayerScript>().BattleEnd();

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

		//拉致で終わったら演出カット
		if(!LastEnemy.GetComponent<EnemyCharacterScript>().AbductionSuccess_Flag)
		{
			//演出カメラ位置基準オブジェクトを取得
			GameObject PosOBJ = LastEnemy != null ? LastEnemy : PlayerCharacter;

			//相手が下にいる
			if (PlayerCharacter.transform.position.y - PosOBJ.transform.position.y > 1.5f)
			{
				//演出カメラを演出位置に移動
				BattleVictoryVcam.gameObject.transform.position = DeepFind(PlayerCharacter, "PelvisBone").transform.position + ((DeepFind(PlayerCharacter, "PelvisBone").transform.position - PosOBJ.transform.position).normalized * 2.5f);

				//演出カメラを注視点に向ける
				BattleVictoryVcam.gameObject.transform.LookAt(DeepFind(PosOBJ, "PelvisBone").transform.position + PlayerCharacter.transform.right);

				//演出カメラを演出位置に移動
				BattleVictoryVcam.gameObject.transform.position += (BattleVictoryVcam.gameObject.transform.right * -1.25f);

				//演出カメラを注視点に向ける
				BattleVictoryVcam.gameObject.transform.LookAt(DeepFind(PosOBJ, "PelvisBone").transform.position + PlayerCharacter.transform.right);
			}
			else
			{
				//演出カメラを演出位置に移動
				BattleVictoryVcam.gameObject.transform.position = PosOBJ.transform.position + (PlayerCharacter.transform.forward * 1.5f) + (PlayerCharacter.transform.right * -0.75f) + new Vector3(0, 0.25f, 0);

				//注視点をプレイヤーキャラクターにする
				PosOBJ = PlayerCharacter;

				//演出カメラを注視点に向ける
				BattleVictoryVcam.gameObject.transform.LookAt(DeepFind(PosOBJ, "NeckBone").transform.position);

				//Z軸に傾ける
				BattleVictoryVcam.gameObject.transform.localRotation *= Quaternion.Euler(0, 0, 40);
			}

			//バーチャルカメラ再生
			BattleVictoryVcam.PlayCameraWork(0, true);

			//フェードエフェクト呼び出し
			ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 0, new Color(0, 0, 0, 1), 0.25f, (GameObject obj) => { }));

			//ズームエフェクト呼び出し
			ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Zoom(false, 0.05f, 0.25f, (GameObject obj) => { obj.GetComponent<Renderer>().enabled = false; }));

			//スローモーション演出
			GameManagerScript.Instance.TimeScaleChange(2.5f, 0.1f, () =>
			{
				//カメラワーク終了
				BattleVictoryVcam.KeepCameraFlag = false;
			});
		}

		//壁オブジェクトが全部消えるまでループ
		while (!OBJList.All(a => a == null))
		{
			yield return null;
		}

		//メモリ開放関数呼び出し
		AssetsUnload();

		//自身を削除
		Destroy(gameObject);
	}

	//次のウェーブ移行処理関数
	private void NextWave()
	{
		//コルーチン呼び出し
		StartCoroutine(NextWaveCoroutine());
	}
	private IEnumerator NextWaveCoroutine()
	{
		//敵が消滅するまで待機
		while(!EnemyList.All(a => a == null))
		{
			yield return null;
		}

		//イベント中フラグを立てる
		GameManagerScript.Instance.EventFlag = true;

		//プレイヤーキャラクターの戦闘継続処理実行
		ExecuteEvents.Execute<PlayerScriptInterface>(PlayerCharacter, null, (reciever, eventData) => reciever.BattleNext(PlayerPosOBJ, gameObject, () =>
		{
			//演出カメラの注視オブジェクト設定
			BattleNextVcam.CameraWorkList[0].GetComponent<CameraWorkScript>().LookAtOBJ = DeepFind(PlayerCharacter, "NeckBone");

			//トランスフォームをキャラクターと同期させる
			BattleNextVcam.transform.position = PlayerCharacter.transform.position;

			//バーチャルカメラ再生
			BattleNextVcam.PlayCameraWork(0, true);

			//敵配置コルーチン呼び出し
			StartCoroutine(StandByCoroutine(() =>
			{
				//カメラワーク終了
				BattleNextVcam.KeepCameraFlag = false;

				//イベント中フラグを下す
				GameManagerScript.Instance.EventFlag = false;

				//プレイヤーキャラクターの戦闘継続処理実行
				PlayerCharacter.GetComponent<PlayerScript>().BattleContinue();

			}));
		}));
	}

	//敵配置コルーチン
	private IEnumerator StandByCoroutine(Action Act)
	{
		//プレイヤーキャラクター存在チェック
		while (PlayerCharacter == null)
		{
			//１フレーム待機
			yield return null;
		}

		//敵生成完了まで待つ
		while(!GenerateEnemyFlag)
		{
			//１フレーム待機
			yield return null;
		}

		//敵リスト初期化
		EnemyList = new List<GameObject>();

		//敵カウント
		int EnemyCount = 0;

		//配置用変数
		float PlacementPoint = 0;

		//敵オブジェクトを配置する
		foreach (GameObject i in AllEnemyList[WaveCount])
		{
			//セッティングスクリプト取得
			EnemySettingScript TempSettingScript = i.GetComponent<EnemySettingScript>();

			//エネミースクリプト取得
			EnemyCharacterScript TempEnemyScript = i.GetComponent<EnemyCharacterScript>();

			//トランスフォームリセット
			ResetTransform(i);
			/*
			//同じ敵を同時に出現させるとSettingで読み込み重複が起こり、原点から外れるとメッシュ結合が上手くいなないので終わるまで待機
			while (!TempSettingScript.AllReadyFlag)
			{
				//１フレーム待機
				yield return null;
			}
			*/
			//親をバトルフィールドにする
			i.transform.parent = gameObject.transform;

			//配置用変数算出
			PlacementPoint = (float)EnemyCount / GameManagerScript.Instance.AllEnemyWaveList.Where(a => a.WaveID == EnemyWaveList[WaveCount]).ToArray()[0].EnemyList.Count * (2.0f * Mathf.PI);

			//ポジション設定
			i.transform.localPosition = new Vector3(Mathf.Cos(PlacementPoint) * 1.5f, 0, Mathf.Sin(PlacementPoint) * 1.5f);

			//ローテンション設定
			i.transform.LookAt(gameObject.transform.position - i.transform.localPosition);

			//親を解除
			i.transform.parent = null;

			//オブジェクト有効化
			i.SetActive(true);

			//敵出現カウントアップ
			EnemyCount++;

			//敵リストにAdd
			EnemyList.Add(i);
		}

		//敵の配置が終わってから行う処理
		if (WaveCount == 0)
		{
			//コライダ有効化をする
			BattleFieldCol.enabled = true;
		}
		else
		{
			//パス終了値を取得
			float EndNum = BattleNextVcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_Path.MaxPos;

			//カメラワークが終わるまで待機
			while (EndNum > BattleNextVcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition)
			{
				//1フレーム待機
				yield return null;
			}

			//敵の戦闘開始処理実行
			foreach (var i in EnemyList.Where(a => a != null).ToList())
			{
				i.GetComponent<EnemyCharacterScript>().BattleStart();
			}

			//イベント中フラグを下す
			GameManagerScript.Instance.EventFlag = false;
		}

		//ウェーブカウントアップ
		WaveCount++;

		//全滅チェック開始
		EnemyCheckFlag = true;

		//最後のウェーブか判別
		if (EnemyWaveList.Count == WaveCount)
		{
			LastWaveFlag = true;
		}

		//匿名関数実行
		Act();


		/*
		//敵リスト初期化
		EnemyList = new List<GameObject>();

		//敵カウント
		int EnemyCount = 0;

		//配置用変数
		float PlacementPoint = 0;

		//読み込んだ敵List宣言
		List<GameObject> TempEnemyOBJList = new List<GameObject>();

		//登録されている敵ウェーブリストから出現する敵のリストを回す
		foreach (var i in GameManagerScript.Instance.AllEnemyWaveList.Where(a => a.WaveID == EnemyWaveList[WaveCount]).ToArray()[0].EnemyList)
		{
			//全ての敵リストからIDでクラスを取得
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == i).ToList()[0];

			//インスタンス用オブジェクト宣言
			GameObject TempEnemy = null;

			//敵オブジェクトを読み込む
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
			{
				//読み込んだ敵を代入
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

		//読み込んだ敵オブジェクトをインスタンス化して配置する
		foreach (GameObject i in TempEnemyOBJList)
		{
			//インスタンス生成
			GameObject TempEnemy = Instantiate(i);

			//セッティングスクリプト取得
			EnemySettingScript TempSettingScript = TempEnemy.GetComponent<EnemySettingScript>();

			//エネミースクリプト取得
			EnemyCharacterScript TempEnemyScript = TempEnemy.GetComponent<EnemyCharacterScript>();

			//トランスフォームリセット
			ResetTransform(TempEnemy);

			//同じ敵を同時に出現させるとSettingで読み込み重複が起こり、原点から外れるとメッシュ結合が上手くいなないので終わるまで待機
			while (!TempSettingScript.AllReadyFlag)
			{
				//１フレーム待機
				yield return null;
			}

			//親をバトルフィールドにする
			TempEnemy.transform.parent = gameObject.transform;

			//配置用変数算出
			PlacementPoint = (float)EnemyCount / GameManagerScript.Instance.AllEnemyWaveList.Where(a => a.WaveID == EnemyWaveList[WaveCount]).ToArray()[0].EnemyList.Count * (2.0f * Mathf.PI);

			//ポジション設定
			TempEnemy.transform.localPosition = new Vector3(Mathf.Cos(PlacementPoint) * 1.5f, 0, Mathf.Sin(PlacementPoint) * 1.5f);

			//ローテンション設定
			TempEnemy.transform.LookAt(gameObject.transform.position - TempEnemy.transform.localPosition);

			//親を解除
			TempEnemy.transform.parent = null;

			//キャラクターコントローラを有効化
			TempEnemy.GetComponent<CharacterController>().enabled = true;

			//敵出現カウントアップ
			EnemyCount++;

			//敵リストにAdd
			EnemyList.Add(TempEnemy);
		}

		//敵の配置が終わってから行う処理
		if (WaveCount == 0)
		{
			//コライダ有効化をする
			BattleFieldCol.enabled = true;
		}
		else
		{
			//パス終了値を取得
			float EndNum = BattleNextVcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_Path.MaxPos;

			//カメラワークが終わるまで待機
			while (EndNum > BattleNextVcam.GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition)
			{
				//1フレーム待機
				yield return null;
			}

			//敵の戦闘開始処理実行
			foreach (var i in EnemyList.Where(a => a != null).ToList())
			{
				i.GetComponent<EnemyCharacterScript>().BattleStart();
			}

			//イベント中フラグを下す
			GameManagerScript.Instance.EventFlag = false;
		}

		//ウェーブカウントアップ
		WaveCount++;		

		//全滅チェック開始
		EnemyCheckFlag = true;

		//最後のウェーブか判別
		if (EnemyWaveList.Count == WaveCount)
		{
			LastWaveFlag = true;
		}

		//匿名関数実行
		Act();
		*/
	}

	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		if (ColHit.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			//侵入してきたプレイアブルキャラクター取得
			SetPlayerCharacter(ColHit.gameObject);

			//ゲームマネージャーの戦闘フラグを立てる
			GameManagerScript.Instance.BattleFlag = true;

			//ダウンしているキャラクター初期化
			GameManagerScript.Instance.DownCharacterList = new List<GameObject>();

			//ゲームマネージャーに自身を送る
			ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => reciever.SetBattleFieldOBJ(gameObject));

			//メインカメラの戦闘開始処理を呼び出す
			ExecuteEvents.Execute<MainCameraScript>(DeepFind(GameManagerScript.Instance.gameObject, "CameraRoot").gameObject, null, (reciever, eventData) => reciever.BattleStart());

			//進入コライダを無効化
			gameObject.GetComponent<SphereCollider>().enabled = false;

			//壁コライダ有効化
			ColOBJ.GetComponent<MeshCollider>().enabled = true;

			//プレイヤーキャラクターの戦闘開始処理実行
			ColHit.gameObject.GetComponent<PlayerScript>().BattleStart();

			//敵に戦闘開始フラグを送る
			foreach (var i in EnemyList.Where(a => a != null).ToList())
			{
				i.GetComponent<EnemyCharacterScript>().BattleStart();
			}

			//壁生成コルーチン呼び出し
			StartCoroutine(GeneraeteWallCoroutine());
		}
	}

	//壁生成コルーチン
	private IEnumerator GeneraeteWallCoroutine()
	{
		//画面揺らし
		ExecuteEvents.Execute<MainCameraScriptInterface>(GameManagerScript.Instance.GetCameraOBJ(), null, (reciever, eventData) => reciever.CameraShake(1f));

		//ガベージ発生頂点座標List宣言
		List<Vector3> VertexList = new List<Vector3>();

		//壁メッシュ頂点位置を抽出
		foreach (var i in WallOBJ.GetComponent<MeshFilter>().mesh.vertices)
		{
			//リストにAdd
			VertexList.Add(i);			
		}

		//リストをシャッフル
		VertexList = VertexList.OrderBy(a => Guid.NewGuid()).ToList();

		//出現位置オフセット
		float Offset = 0.5f;

		//煙エフェクト取得
		GameObject TempSmoke = GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "GenerateWall").ToArray()[0];

		//ループカウント
		int count = 0;

		//頂点位置から壁素材を発生させる
		for (int i = 0; i < 10; i++)
		{
			//頂点位置を重複を省いてループ、
			foreach (var ii in VertexList.Distinct())
			{
				//壁素材をインスタンス化
				GameObject TempWallMat = Instantiate(WallMaterial[Random.Range(0, WallMaterial.Count)]);

				//物理を切る
				TempWallMat.GetComponent<Rigidbody>().isKinematic = true;

				//親を設定
				TempWallMat.transform.parent = DeepFind(gameObject, "WallOBJ").transform;

				//発生位置に移動
				TempWallMat.transform.position = ii;

				//ランダムに回転
				TempWallMat.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));

				//活性化
				TempWallMat.SetActive(true);

				//壁生成スクリプトを付けて壁生成関数を呼び出す
				TempWallMat.AddComponent<GenerateWallScript>().GenerateWall(ii, ii + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.25f, 0.25f) + Offset, Random.Range(-0.5f, 0.5f)));

				//カウントアップ
				count++;

				//エフェクトを出す、半分だけでいいかな
				if(count % 2 == 0)
				{
					//煙エフェクトインスタンスを生成
					GameObject tempeffect = Instantiate(TempSmoke);

					//壁オブジェクトの子にする
					tempeffect.transform.parent = TempWallMat.transform;

					//トランスフォームリセット
					ResetTransform(tempeffect);
				}
			}

			//オフセット位置を上げる
			Offset += 0.2f;

			//1フレーム待機
			yield return null;
		}

		//チョイ待つ
		yield return new WaitForSeconds(0.5f);
	}

	//プレイヤーキャラクター取得コルーチン
	private IEnumerator GetPlayerCharacterCoroutine()
	{
		//プレイヤーキャラクターが入るまで待つ
		while (PlayerCharacter == null)
		{
			//プレイヤーキャラクター取得
			PlayerCharacter = GameManagerScript.Instance.GetPlayableCharacterOBJ();

			//１フレーム待機
			yield return null;
		}
	}
}

/*

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
*/
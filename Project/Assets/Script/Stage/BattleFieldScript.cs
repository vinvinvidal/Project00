using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleFieldScript : GlobalClass
{
	//出現する敵ウェーブリスト
	public List<int> EnemyWaveList;

	//出現位置List
	private List<GameObject> SpawnPosList;

	//敵オブジェクトリスト
	private List<GameObject> EnemyOBJList;

	//敵読み込み完了フラグ
	private bool LoadCompleteFlag = false;

	//ウェーブカウント
	private int WaveCount = 0;

	//出現させた敵List
	private List<GameObject> EnemyList;

	//全滅チェックフラグ
	private bool EnemyCheckFlag = false;

	//壁生成スクリプトリスト
	private List<GenerateWallScript> WallGanerateScriptList;

	//バーチャルカメラ
	private CinemachineCameraScript Vcam;

	private void Start()
	{
		//出現位置List初期化
		SpawnPosList = new List<GameObject>(gameObject.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("SpawnPos")).Select(a => a.gameObject).ToList());

		//出現させた敵List初期化
		EnemyList = new List<GameObject>();

		//壁生成完了スクリプト取得
		WallGanerateScriptList = new List<GenerateWallScript>(gameObject.GetComponentsInChildren<GenerateWallScript>());

		//バーチャルカメラ取得
		Vcam = DeepFind(gameObject, "BattleFieldCamera").GetComponent<CinemachineCameraScript>();
		
		//敵オブジェクトリスト初期化
		EnemyOBJList = new List<GameObject>();

		//使用する敵オブジェクト読み込み
		StartCoroutine(LoadEnemyCoroutine());

		//初期配置コルーチン呼び出し
		StartCoroutine(StandByCoroutine());
	}

	//初期配置コルーチン
	private IEnumerator StandByCoroutine()
	{
		//読み込み完了まで待機
		while (!LoadCompleteFlag)
		{
			//待機
			yield return null;
		}

		//敵出現カウント
		int EnemyCount = 0;

		//配置用変数
		float PlacementPoint = 0;

		//出現する敵Listを回す
		foreach (var i in GameManagerScript.Instance.AllEnemyWaveList[WaveCount].EnemyList)
		{
			//インスタンス生成
			GameObject TempEnemy = Instantiate(EnemyOBJList.Where(a => int.Parse(a.GetComponent<EnemySettingScript>().ID) == i).ToArray()[0]);

			//親をバトルフィールドにする
			TempEnemy.transform.parent = gameObject.transform;

			//座標を直で変更するためキャラクターコントローラを無効化
			TempEnemy.GetComponent<CharacterController>().enabled = false;

			//配置用変数さんしゅつ
			PlacementPoint = (float)EnemyCount / GameManagerScript.Instance.AllEnemyWaveList[0].EnemyList.Count * (2.0f * Mathf.PI);

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

			//敵出現カウントアップ
			EnemyCount ++;
		}

		//ウェーブカウントアップ
		WaveCount++;
	}

	//敵読み込みコルーチン
	private IEnumerator LoadEnemyCoroutine()
	{
		//敵IDList宣言
		List<int> EnemyIDList = new List<int>();

		//読み込み完了Dic宣言
		Dictionary<int, bool> LoadDic = new Dictionary<int, bool>();

		//ウェーブリストを回す
		foreach (var i in EnemyWaveList)
		{
			//出現する敵リストを回す
			foreach(var ii in GameManagerScript.Instance.AllEnemyWaveList[i].EnemyList)
			{
				//被った要素が無ければリストに追加
				if(!EnemyIDList.Any(a => a == ii))
				{
					EnemyIDList.Add(ii);

					LoadDic.Add(ii, false);
				}				
			}
		}

		//敵IDListを回す
		foreach (var i in EnemyIDList)
		{
			//全ての敵リストからIDで検索
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == i).ToList()[0];

			//敵オブジェクトを読み込む
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
			{
				//リストにAdd
				EnemyOBJList.Add(O as GameObject);

				//読み込み完了フラグを立てる
				LoadDic[i] = true;

			}));
		}

		//読み込み完了まで待機
		while(!LoadDic.Values.All(a => a))
		{
			//待機
			yield return null;
		}

		//読み込み完了フラグを立てる
		LoadCompleteFlag = true;
	}

	private void Update()
	{
		//敵全滅チェック
		if(EnemyCheckFlag)
		{
			//敵を全て倒したら処理
			if(EnemyList.All(a => a == null))
			{
				//最終ウェーブだったらフィールド解除処理
				if(EnemyWaveList.Count == WaveCount)
				{
					//全滅チェックフラグを下す
					EnemyCheckFlag = false;

					//フィールド解除コルーチン呼び出し
					StartCoroutine(ReleaseBattleFieldCoroutine());
				}
				//次のウェーブ出現処理
				else
				{
					//EnemySpawn();
				}				
			}
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
		foreach(Rigidbody i in OBJList.Select(a => a.GetComponent<Rigidbody>()))
		{
			//オブジェクトに壁消失用スクリプト追加
			i.gameObject.AddComponent<WallVanishScript>();

			//物理挙動有効化
			i.isKinematic = false;

			//外側に力を与えて壁を崩す
			i.AddForce((i.transform.position - gameObject.transform.position + Vector3.up) * 2.5f, ForceMode.Impulse);
		}

		//壁オブジェクトが全部消えるまでループ
		while(!OBJList.All(a => a == null))
		{
			yield return null;
		}

		//フィールド削除
		Destroy(gameObject);
	}

	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		//ヒットコライダを無効化
		gameObject.GetComponent<SphereCollider>().enabled = false;

		//フィールドコライダ有効化
		gameObject.GetComponentInChildren<MeshCollider>().enabled = true;

		//イベント中フラグを立てる
		GameManagerScript.Instance.EventFlag = true;

		//バーチャルカメラをキャラクターの子にする
		Vcam.gameObject.transform.parent = GameManagerScript.Instance.GetPlayableCharacterOBJ().transform;

		//位置合わせ
		Vcam.gameObject.transform.localPosition *= 0;
		Vcam.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);

		//バーチャルカメラ再生
		Vcam.PlayCameraWork(0, true);

		//ガベージを撒いて壁を作るスクリプトの関数呼び出し
		WallGanerateScriptList.Select(a => StartCoroutine(a.GenerateWallCoroutine())).ToArray();

		//壁生成完了待ちコルーチン呼び出し
		StartCoroutine(WaitWallGenerateCoroutine());

		//敵に戦闘開始フラグを送る
		foreach(var i in EnemyList.Where(a => a != null).ToList())
		{
			i.GetComponent<EnemyCharacterScript>().BattleStart();
		}

		//全滅チェックフラグを立てる
		EnemyCheckFlag = true;

		//プレイアブルキャラクターの戦闘戦闘開始処理関数呼び出し
		ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleStart(gameObject));
	}

	//壁生成完了待ちコルーチン
	private IEnumerator WaitWallGenerateCoroutine()
	{
		//敵出現関数呼び出し
		//EnemySpawn();

		//完了フラグが全て立つまで待機
		while (!WallGanerateScriptList.All(a => a.CompleteFlag))
		{
			//１フレーム待機
			yield return null;
		}

		//壁が落ち着くまでちょっと待つ
		yield return new WaitForSeconds(2.5f);

		//物理挙動を止める
		foreach(var i in gameObject.GetComponentsInChildren<Rigidbody>())
		{			
			i.isKinematic = true;
		}

		//バーチャルカメラ停止
		Vcam.KeepCameraFlag = false;

		//カメラが移動するまでちょっと待つ
		yield return new WaitForSeconds(1);

		//イベント中フラグを下す
		GameManagerScript.Instance.EventFlag = false;

		//プレイヤーキャラクターのイベントフラグを下す
		GameManagerScript.Instance.GetPlayableCharacterOBJ().GetComponent<PlayerScript>().ActionEventFlag = false;
	}

	//敵出現
	public void EnemySpawn()
	{
		//コルーチン呼び出し
		StartCoroutine(EnemySpawnCoroutine());
	}
	private IEnumerator EnemySpawnCoroutine()
	{
		//敵全滅チェック停止
		EnemyCheckFlag = false;

		//出現する敵ウェーブリストを回す
		foreach (var i in GameManagerScript.Instance.AllEnemyWaveList[EnemyWaveList[WaveCount]].EnemyList)
		{
			//全ての敵リストからIDで検索
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => int.Parse(e.EnemyID) == i).ToList()[0];

			//敵オブジェクトを読み込む
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + tempclass.EnemyID + "/", tempclass.OBJname, "prefab", (object O) =>
			{
				//インスタンス生成
				GameObject tempenemy = Instantiate(O as GameObject);

				//座標を直で変更するためキャラクターコントローラを無効化
				tempenemy.GetComponent<CharacterController>().enabled = false;

				//座標をランダムにする
				tempenemy.transform.position = gameObject.transform.position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0.1f, UnityEngine.Random.Range(-5.0f, 5.0f));

				//キャラクターコントローラを有効化
				tempenemy.GetComponent<CharacterController>().enabled = true;

				//リストにAdd
				EnemyList.Add(tempenemy);

			}));

			//待機
			yield return new WaitForSeconds(1);
		}

		//ウェーブカウントアップ
		WaveCount ++;
		
		//敵全滅チェック開始
		EnemyCheckFlag = true;
	}
}

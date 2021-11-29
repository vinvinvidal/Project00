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
					EnemySpawn();
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

		//プレイアブルキャラクターの戦闘戦闘開始処理関数呼び出し
		ExecuteEvents.Execute<PlayerScriptInterface>(GameManagerScript.Instance.GetPlayableCharacterOBJ(), null, (reciever, eventData) => reciever.BattleStart(gameObject));
	}

	//壁生成完了待ちコルーチン
	private IEnumerator WaitWallGenerateCoroutine()
	{
		//敵出現関数呼び出し
		EnemySpawn();

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

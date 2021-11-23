using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	private void Start()
	{
		//出現位置List初期化
		SpawnPosList = new List<GameObject>(gameObject.GetComponentsInChildren<Transform>().Where(a => a.name.Contains("SpawnPos")).Select(a => a.gameObject).ToList());

		//出現させた敵List初期化
		EnemyList = new List<GameObject>();
	}

	private void Update()
	{
		//敵全滅チェック
		if(EnemyCheckFlag)
		{
			if(EnemyList.All(a => a == null))
			{
				if(EnemyWaveList.Count == WaveCount)
				{
					Destroy(gameObject);
				}
				else
				{
					EnemySpawn();
				}				
			}
		}
	}

	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		//ヒットコライダを無効化
		gameObject.GetComponent<SphereCollider>().enabled = false;

		//フィールドコライダ有効化
		gameObject.GetComponentInChildren<MeshCollider>().enabled = true;

		//ガベージを撒いて壁を作る
		foreach(var i in GetComponentsInChildren<GarbageSplashScript>())
		{
			i.enabled = true;
		}

		//敵出現関数呼び出し
		EnemySpawn();
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

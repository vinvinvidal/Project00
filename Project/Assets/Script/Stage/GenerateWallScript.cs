using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWallScript : GlobalClass
{
	//壁素材List
	public List<GameObject> GarbageList;

	//壁生成時間
	public float GanerateTime;

	//終了時間
	private float EndTime = 0;

	//完了フラグ
	public bool CompleteFlag { get; set; } = false;

	void Start()
    {
		
		//StartCoroutine(GenerateWallCoroutine());
    }

	//壁生成コルーチン
	public IEnumerator GenerateWallCoroutine()
	{
		//ドーリーで移動開始
		gameObject.GetComponent<CinemachineDollyCart>().enabled = true;

		while (EndTime < GanerateTime)
		{
			//生成時間カウントアップ
			EndTime += Time.deltaTime;

			//オブジェクトリストからランダムでオブジェクトのインスタンス生成
			GameObject TempGarbage = Instantiate(GarbageList[Random.Range(0, GarbageList.Count)]);

			//レンダラーのあるオブジェクトのレイヤーを自身のレイヤーと同じにする
			TempGarbage.GetComponentInChildren<Renderer>().transform.gameObject.layer = gameObject.layer;
			
			//自身と同じ位置にする
			TempGarbage.transform.position = transform.position;

			//移動を制限
			//TempGarbage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

			//壁オブジェクトをまとめるオブジェクトの子にする
			TempGarbage.transform.parent = DeepFind(gameObject.transform.parent.gameObject, "WallOBJ").transform;

			//下方向に力を加える
			TempGarbage.GetComponent<Rigidbody>().AddForce(Vector3.down * 10, ForceMode.Impulse);

			//ちょっとランダム性を持たせて待機
			yield return new WaitForSeconds(Random.Range(Time.deltaTime, 0.05f));
		}

		//ドーリーで移動停止
		gameObject.GetComponent<CinemachineDollyCart>().enabled = false;

		//完了フラグを立てる
		CompleteFlag = true;
	}
}

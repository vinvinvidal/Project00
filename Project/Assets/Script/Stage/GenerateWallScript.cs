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

    void Start()
    {
		//壁生成コルーチン呼び出し
		StartCoroutine(GenerateWallCoroutine());
    }

	private IEnumerator GenerateWallCoroutine()
	{
		while(EndTime < GanerateTime)
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
			TempGarbage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

			//ルートオブジェクトの子にする
			TempGarbage.transform.parent = gameObject.transform.parent;


			//１フレーム待機
			yield return null;
		}		
	}
}

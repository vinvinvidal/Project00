using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GarbageSplashScript : GlobalClass
{
	//発生させるガベージオブジェクトList
    public List<GameObject> GarbageList;

	//発生比率
	public List<int> GarbageRetioList;

	//発生させるガベージ数
	public int GarbageCount;

	//発生時に加える力
	public float SplashPower;

	//ガベージのレイヤー
	public string GarbageLayerName;

    void Start()
    {
		//ガベージ発生コルーチン呼び出し
		StartCoroutine(GarbageCreateCoroutine());
    }

	//ガベージ発生コルーチン
	IEnumerator GarbageCreateCoroutine()
	{
		//発生数だけ回す
		for (int i = 0; i < GarbageCount; i++)
		{
			//発生比率を元にランダムな値を求める
			int TempRatio = Random.Range(1, GarbageRetioList.Sum() + 1);

			//比率から発生させるガベージを選定するループ
			for (int ii = 0; ii < GarbageRetioList.Count; ii++)
			{
				//比率を合計していき、乱数がどの範囲にあるか求める
				if(GarbageRetioList.Take(ii + 1).Sum() >= TempRatio)
				{
					//インスタンス生成、変数に代入
					GameObject TempGarbage = Instantiate(GarbageList[ii]);

					//レンダラーのあるオブジェクトのレイヤーを変更
					TempGarbage.GetComponentInChildren<Renderer>().transform.gameObject.layer = LayerMask.NameToLayer(GarbageLayerName);

					//子にする
					TempGarbage.transform.parent = gameObject.transform;

					//ローカル座標で移動、親と同じ位置にする
					TempGarbage.transform.localPosition = Vector3.zero;

					//RigidBodyがある場合
					if(TempGarbage.GetComponent<Rigidbody>() != null)
					{
						//ランダムな回転値を与える
						TempGarbage.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), ForceMode.Impulse);

						//ランダムな方向に力を加える
						TempGarbage.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-SplashPower, SplashPower), Random.Range(-SplashPower, SplashPower), Random.Range(-SplashPower, SplashPower)), ForceMode.Impulse);
					}

					//ループを抜ける
					break;
				}				
			}

			//1フレーム待機
			yield return null;
		}
	}
}

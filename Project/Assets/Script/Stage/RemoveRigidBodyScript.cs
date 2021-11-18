using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveRigidBodyScript : MonoBehaviour
{
	//RigidBody
	private Rigidbody Rbody;

	//生まれた時間
	private float CreateTime;

    void Start()
    {
		//RigidBody取得
		Rbody = GetComponent<Rigidbody>();

		//生成時間をキャッシュ
		CreateTime = Time.time;
	}

    void Update()
    {
		//速度を測定し、停止したらコンポーネント停止処理
		if (Rbody.velocity.magnitude == 0 && Time.time - CreateTime > 1.0f)
		{
			//Rigidbody削除
			Destroy(Rbody);
			
			//コライダ削除
			Destroy(GetComponent<MeshCollider>());

			//このスクリプト削除
			Destroy(GetComponent<RemoveRigidBodyScript>());
		}
    }
}

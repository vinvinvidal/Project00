using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CigaretteSmokeScript : GlobalClass
{
	//煙を出しているパーティクルシステム
	private ParticleSystem ParSys;

	//煙を出しているパーティクルシステムのアクセサ
	private ParticleSystem.MainModule ParSysAccess;

	//位置のキャッシュ
	private Vector3 TempPos;

	//停止時間
	private float StopTime = 0;

	void Start()
    {
		//煙を出しているパーティクルシステム取得
		ParSys = GetComponent<ParticleSystem>();

		//煙を出しているパーティクルシステムのアクセサを取得
		ParSysAccess = ParSys.main;
	}

    void Update()
    {
		//ある程度移動してたらパーティクルの発生を止める
		if((gameObject.transform.position - TempPos).sqrMagnitude > 0.0025f)
		{
			StopTime -= Time.deltaTime;
		}
		else
		{
			StopTime += Time.deltaTime;			
		}		

		if(StopTime > 0.5f)
		{
			ParSys.Play();

			StopTime = 0;
		}
		else if(StopTime < -0.1f)
		{
			ParSys.Stop();

			StopTime = 0;
		}

		//位置をキャッシュ
		TempPos = gameObject.transform.position;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敵の情報を格納するクラス
public class EnemyClass
{
	//キャラクターID
	public string EnemyID;

	//オブジェクト名
	public string OBJname;

	//キャラクターの名前
	public string Name;

	//ライフ
	public int Life;

	//スタン値
	public float Stun;

	//ダウンタイム
	public float DownTime;

	//移動速度
	public float MoveSpeed;

	//旋回速度
	public float TurnSpeed;

	//コンストラクタ
	public EnemyClass
	(
		string id,
		string on,
		string nm,
		int lf,
		float st,
		float dt,
		float ms,
		float ts
	)
	{
		EnemyID = id;
		OBJname = on;
		Name = nm;
		Life = lf;
		Stun = st;
		DownTime = dt;
		MoveSpeed = ms;
		TurnSpeed = ts;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviorClass
{
	//この行動の名前
	public string Name;

	//この行動の優先度
	public int Priority;

	//この行動の実行条件
	public Func<bool> BehaviorConditions;

	//この行動の処理
	public Action BehaviorAction;

	//コンストラクタ
	public EnemyBehaviorClass
	(
		string nm,
		int pr,		
		Action ba,
		Func<bool> bc
	)
	{
		Name = nm;
		Priority = pr;
		BehaviorConditions = new Func<bool>(bc);
		BehaviorAction = ba;
	}
}

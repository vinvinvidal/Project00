using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveClass
{
	public int WaveID;

	public string Info;

	public List<int> EnemyList;

	//コンストラクタ
	public EnemyWaveClass
	(
		int id,
		string info,
		List<int> el
	)
	{
		WaveID = id;
		Info = info;
		EnemyList = new List<int>(el);
	}
}

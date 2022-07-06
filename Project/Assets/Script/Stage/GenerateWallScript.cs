using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWallScript : GlobalClass
{
	public void GenerateWall(Vector3 from, Vector3 to)
	{
		//壁生成コルーチン呼び出し
		StartCoroutine(GenerateWallCoroutine(from, to));
	}

	private IEnumerator GenerateWallCoroutine(Vector3 from, Vector3 to)
	{
		float StartTime = Time.time;

		//停止するまでループ
		while (Time.time - StartTime < 0.5f)
		{
			transform.position += (to - from) * Time.deltaTime * 2;

			transform.rotation *= Quaternion.Euler(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)));

			yield return null;			
		}

		//このスクリプトを無効化
		GetComponent<GenerateWallScript>().enabled = false;
	}
}

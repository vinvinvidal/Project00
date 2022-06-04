using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface WireShaderScriptInterface : IEventSystemHandler
{
	//ワイヤーを動かす
	void WireWave(float t);
}
public class WireShaderScript : GlobalClass, WireShaderScriptInterface
{
	//マテリアル
	Material WireMaterial;

	//オフセット値
	Vector2 OffsetVec = Vector2.zero;

    void Start()
    {
		//マテリアル取得
		WireMaterial = GetComponent<Renderer>().material;
	}

	void Update()
    {
		//テクスチャのオフセット値にノイズを加える
		OffsetVec.x += Time.deltaTime * Mathf.PerlinNoise(Time.time , 0) * 5;

		//テクスチャオフセットを反映
		WireMaterial.SetTextureOffset("_MainTexture", -OffsetVec);
	}

	//ワイヤー波打ち制御関数
	public void WireWave(float t)
	{
		StartCoroutine(WireWaveCoroutine(t));
	}
	private IEnumerator WireWaveCoroutine(float t)
	{
		//引数の正負で目標値を決める
		int n = t > 0 ? 1 : 0;

		//初期値、nが0なら1、1なら0になる
		float i = 1 - n;

		while (n * 10 != Mathf.Round(i * 10))
		{
			//波打ち制御変数に代入
			WireMaterial.SetFloat("WaveNum", i);

			//加算
			i += t;
			
			//1フレーム待機
			yield return null;
		}

		//波打ち制御変数に代入
		WireMaterial.SetFloat("WaveNum", n);
	}
}

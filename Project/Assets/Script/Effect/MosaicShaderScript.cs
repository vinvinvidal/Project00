using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MosaicShaderScript : GlobalClass
{
	//モザイクマテリアル
	Material Mat;

	//メインカメラオブジェクト
	GameObject MainCamera;

	//カメラとの距離
	float Dist;

    void Start()
    {
		//マテリアル取得
		foreach(var i in GetComponent<Renderer>().materials)
		{
			if(i.name.Contains("Mosaic"))
			{
				Mat = i;
			}
		}		

		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//パーティクルのアクセサを取り出す
		ParticleSystem.ShapeModule Accesser = GetComponent<ParticleSystem>().shape;

		//親のSkinnedMeshRendererをShapeに反映
		Accesser.skinnedMeshRenderer = transform.parent.GetComponent<SkinnedMeshRenderer>();

		//最初は消しとくコルーチン呼び出し
		StartCoroutine(FirstCoroutine());
	}

	void Update()
	{
		//カメラとの距離を測定
		Dist = Mathf.Ceil((MainCamera.transform.position - transform.position).sqrMagnitude);		

		//カメラとの距離によってモザイクの粗さを変える
		Mat.SetFloat("_BlockSize", Dist * 2.5f);
	}

	//最初は消しとくコルーチン
	private IEnumerator FirstCoroutine()
	{
		//1秒待つ、これをしないと使う時に位置が変になってたりする、危険。
		yield return new WaitForSeconds(1);

		//無効化
		gameObject.SetActive(false);
	}
}

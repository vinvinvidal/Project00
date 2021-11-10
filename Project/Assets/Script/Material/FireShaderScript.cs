using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShaderScript : MonoBehaviour
{
	//マテリアル
	private Material FireMaterial;

	//カメラのトランスフォーム
	private Transform CameraTransform;

	//サインカーブに使うカウント
	private float SinCount = 0;

	//オフセット移動値
	private float TexTureOffset = 0;

	void Start()
    {
		//マテリアル取得
		FireMaterial = transform.GetComponent<Renderer>().material;

		//ディレクショナルライトのトランスフォーム取得
		CameraTransform = GameObject.Find("MainCamera").transform;
	}

    void Update()
    {
		//カメラの行列をシェーダーに渡す
		FireMaterial.SetMatrix("_CameraMatrix", CameraTransform.worldToLocalMatrix);

		//サインカーブカウントアップ
		SinCount = Mathf.PerlinNoise(Time.time * 3f, -Time.time * 0.3f);

		TexTureOffset -= Mathf.PerlinNoise(-Time.time , Time.time) + 0.5f;

		//火を動かす
		//FireMaterial.SetTextureScale("_FireNormalTex", new Vector2(0,0));
		FireMaterial.SetTextureOffset("_FireNormalTex", new Vector2(Mathf.Sin(2 * Mathf.PI * 0.001f * SinCount), TexTureOffset * 0.01f));
	}
}

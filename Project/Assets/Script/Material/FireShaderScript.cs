using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShaderScript : MonoBehaviour
{
	//マテリアル
	private Material FireMaterial;

	//サインカーブに使うカウント
	private float SinCount = 0;

	//オフセット移動値
	private float TexTureOffset = 0;

	void Start()
    {
		//マテリアル取得
		FireMaterial = gameObject.GetComponent<Renderer>().material;		
	}

    void Update()
    {
		//サインカーブカウントアップ
		SinCount = Mathf.PerlinNoise(Time.time * 3f, -Time.time * 0.3f);

		//オフセットカウントアップ
		TexTureOffset -= Mathf.PerlinNoise(-Time.time , Time.time) + 0.5f;

		//火を動かす
		FireMaterial.SetTextureOffset("_FireNormalTex", new Vector2(Mathf.Sin(2 * Mathf.PI * 0.001f * SinCount), TexTureOffset * 0.01f));
	}
}

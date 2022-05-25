using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireShaderScript : GlobalClass
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
		OffsetVec.x += Time.deltaTime * Mathf.PerlinNoise(Time.time , 0) * 5;

		WireMaterial.SetTextureOffset("_MainTexture", -OffsetVec);
	}
}

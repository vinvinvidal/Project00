using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireShaderScript : GlobalClass
{
	//マテリアル
	Material WireMaterial;

    void Start()
    {
		//マテリアル取得
		WireMaterial = GetComponent<Renderer>().material;
	}

	void Update()
    {
		WireMaterial.SetTextureOffset("_MainTexture", new Vector2(Time.time * 2, 0));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapShaderScript : GlobalClass
{
	//ミニマップマテリアル
	Material MiniMapMaterial;

	//カメラルート
	GameObject CameraOBJ;

    void Start()
    {
		//ミニマップマテリアル取得
		MiniMapMaterial = gameObject.GetComponent<Renderer>().material;

		//カメラルート取得
		CameraOBJ = GameObject.Find("CameraRoot");
	}

	private void Update()
	{
		//シェーダーに位置を渡す
		MiniMapMaterial.SetFloat("_PlayerCharacterPos", CameraOBJ.transform.position.y);
	}
}

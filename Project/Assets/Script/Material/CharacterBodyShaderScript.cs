using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyShaderScript : GlobalClass
{
	//マテリアル
	private Material BodyMaterial;

	//ベーステクスチャ
	public Texture2D _TexBase;

	//ノーマルマップ
	public Texture2D _TexNormal;

	//ハイライトのテクスチャ
	public Texture2D _TexHiLight;

	//ハイライトのmatcap
	public Texture2D _HiLightMatCap;

	//線画テクスチャ
	public Texture2D _TexLine;

	//ディレクショナルライトのトランスフォーム
	private Transform LightTransform;

	void Start()
    {
		//マテリアル取得
		BodyMaterial = transform.GetComponent<Renderer>().material;

		//ディレクショナルライトのトランスフォーム取得
		LightTransform = GameObject.Find("OutDoorLight").transform;

		//マテリアルにテクスチャを渡す
		BodyMaterial.SetTexture("_TexBase", _TexBase);
		BodyMaterial.SetTexture("_TexNormal", _TexNormal);
		BodyMaterial.SetTexture("_TexHiLight", _TexHiLight);
		BodyMaterial.SetTexture("_HiLightMatCap", _HiLightMatCap);
		BodyMaterial.SetTexture("_TexLine", _TexLine);
	}

    void Update()
    {
		//ディレクショナルライトの行列をシェーダーに渡す
		BodyMaterial.SetMatrix("_LightMatrix", LightTransform.worldToLocalMatrix);
	}

	//はだけテクスチャに変える
	public void SetOffTexture()
	{
		/*
		//マテリアルにテクスチャを渡す
		BodyMaterial.SetTexture("_TexBase", _TexBaseOff);
		BodyMaterial.SetTexture("_TexNormal", _TexNormalOff);
		BodyMaterial.SetTexture("_TexHiLight", _TexHiLight);
		BodyMaterial.SetTexture("_HiLightMatCap", _HiLightMatCap);
		BodyMaterial.SetTexture("_TexLine", _TexLineOff);
		*/
	}

	//はだけテクスチャを元に戻す
	public void SetOnTexture()
	{
		//マテリアルにテクスチャを渡す
		BodyMaterial.SetTexture("_TexBase", _TexBase);
		BodyMaterial.SetTexture("_TexNormal", _TexNormal);
		BodyMaterial.SetTexture("_TexHiLight", _TexHiLight);
		BodyMaterial.SetTexture("_HiLightMatCap", _HiLightMatCap);
		BodyMaterial.SetTexture("_TexLine", _TexLine);
	}
}

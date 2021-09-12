using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*
ポストエフェクトについて
このスクリプトはメインカメラにアタッチする
このスクリプトでサブカメラのレンダリング結果とメインカメラのレンダリング結果を合成する
実際のエフェクトはサブカメラのシェーダーで加工する
*/

public class MixTexScript : GlobalClass
{
	//テクスチャ合成シェーダー用マテリアル
	public Material MixTexMaterial;

	//ポストエフェクトに使用するテクスチャをレンダリングするサブカメラ
	Camera PostEffectCamera;

	void Start()
	{
		//ポストエフェクトに使用するテクスチャをレンダリングするサブカメラを取得
		PostEffectCamera = GameObject.Find("PostEffectCamera").GetComponent<Camera>();
	}

	//レンダリングが完了すると呼ばれる
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		//テクスチャ合成シェーダーにポストエフェクト用レンダーテクスチャを渡す
		MixTexMaterial.SetTexture("_EffectTex", PostEffectCamera.targetTexture);

		//シェーダー呼び出しテクスチャを合成して画面に描画
		Graphics.Blit(src, dest, MixTexMaterial);
	}
}
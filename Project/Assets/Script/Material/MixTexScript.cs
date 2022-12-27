using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;

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

	//自身のカメラ
	private Camera MainCamera;

	//最終出力するルートカメラ
	private Camera RootCamera;

	//ポストエフェクトに使用するテクスチャをレンダリングするサブカメラ
	private Camera PostEffectCamera;

	//レンダーテクスチャ
	private RenderTexture RendTex;

	void Start()
	{
		//自身のカメラ取得
		MainCamera = GetComponent<Camera>();

		//ディスプレイに描画するルートカメラ取得
		RootCamera = transform.parent.GetComponent<Camera>();

		//レンダーテクスチャ更新
		TextureRefresh();

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

	//テクスチャフォーマット更新関数
	public void TextureRefresh()
	{
		//コルーチン呼び出し
		StartCoroutine(TextureRefreshCoroutine());
	}
	private IEnumerator TextureRefreshCoroutine()
	{
		//１フレーム待機
		yield return null;

		//画面サイズからテクスチャを生成
		RendTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

		//レンダーテクスチャ設定
		MainCamera.targetTexture = RendTex;

		//コマンドバッファ宣言
		CommandBuffer CmdBuffer = new CommandBuffer();

		//メッシュのレンダリング結果とUIカメラのレンダリング結果をブレンド
		CmdBuffer.Blit((RenderTargetIdentifier)RendTex, BuiltinRenderTextureType.CameraTarget);

		//最終出力カメラにコマンドバッファをAdd
		RootCamera.AddCommandBuffer(CameraEvent.AfterEverything, CmdBuffer);
	}
}
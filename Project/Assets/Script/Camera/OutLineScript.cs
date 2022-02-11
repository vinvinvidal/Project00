using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutLineScript : GlobalClass
{
	//エフェクトをかけるシェーダーを持っているマテリアル
	public Material OutLineMaterial;

	//アウトラインを描画するシェーダー
	public Shader OutLineShader;

	//アウトラインのマスキングを描画するシェーダー
	public Shader MaskingShader;

	//メインカメラ
	private Camera MainCamera;

	//ポストエフェクトカメラ
	private Camera PostEffectCamera;

	//最も離れているキャラクターとの距離
	private float Distance;
	private float TempDistance;

	//カメラに設定されているレイヤーマスク
	private int Mask;

	//アウトラインをレンダリングするテクスチャ
	private RenderTexture OutLineTexture;

	//マスキングをレンダリングするテクスチャ
	private RenderTexture MaskingTexture;

	//アウトラインをレンダリングするフラグ
	private bool OutLineFlag;

	private void Start()
	{
		//アウトラインをレンダリングするテクスチャ作成
		//OutLineTexture = new RenderTexture(Mathf.RoundToInt(Screen.width * 0.75f), Mathf.RoundToInt(Screen.height * 0.75f), 24, RenderTextureFormat.ARGB32);
		OutLineTexture = new RenderTexture(Mathf.RoundToInt(GameManagerScript.Instance.ScreenResolution * GameManagerScript.Instance.ScreenAspect.x * 0.75f), Mathf.RoundToInt(GameManagerScript.Instance.ScreenResolution * GameManagerScript.Instance.ScreenAspect.y * 0.75f), 24, RenderTextureFormat.ARGB32);

		//マスキングをレンダリングするテクスチャ作成
		MaskingTexture = new RenderTexture(Mathf.RoundToInt(GameManagerScript.Instance.ScreenResolution * GameManagerScript.Instance.ScreenAspect.x * 0.1f) , Mathf.RoundToInt(GameManagerScript.Instance.ScreenResolution * GameManagerScript.Instance.ScreenAspect.y * 0.1f), 24, RenderTextureFormat.ARGB32);

		//メインカメラ取得
		MainCamera = transform.parent.GetComponent<Camera>();

		//ポストエフェクトカメラ取得
		PostEffectCamera = GetComponent<Camera>();

		//最も離れているキャラクターとの距離初期化
		Distance = 0.0f;

		//カメラに設定されているレイヤーマスク取得
		Mask = PostEffectCamera.cullingMask;

		//バックグランドカラー設定
		PostEffectCamera.backgroundColor = new Color(0, 0, 0, 0);
	}

	private void Update()
	{
		//最も離れているキャラクターとの距離初期化
		Distance = 0;
		TempDistance = 0;

		//メインカメラとFOVを同期
		PostEffectCamera.fieldOfView = MainCamera.fieldOfView;

		//スケベ中はプレイヤーキャラクターに合わせる
		if(GameManagerScript.Instance.H_Flag)
		{
			Distance = (transform.position - GameManagerScript.Instance.GetPlayableCharacterOBJ().transform.position).sqrMagnitude;
		}
		else
		{
			//まずキャラクターの位置を測る
			foreach (GameObject i in GameManagerScript.Instance.AllActiveCharacterList.Where(a => a == GameManagerScript.Instance.GetPlayableCharacterOBJ()).ToList())
			{
				//カメラとの距離を計る
				TempDistance = (transform.position - i.transform.position).sqrMagnitude;

				//距離を比較
				if (Distance < TempDistance)
				{
					//遠かったら値を更新
					Distance = TempDistance;
				}
			}

			//敵の位置を測る
			foreach (GameObject i in GameManagerScript.Instance.AllActiveEnemyList.Where(a => a!= null).ToList())
			{
				//カメラに入ってたら位置を比較
				if(i.GetComponent<EnemyCharacterScript>().GetOnCameraBool())
				{
					//カメラとの距離を計る
					TempDistance = (transform.position - i.transform.position).sqrMagnitude;

					//距離を比較
					if (Distance < TempDistance && TempDistance < 900)
					{
						//遠かったら値を更新
						Distance = TempDistance;
					}
				}
			}
		}

		//最も遠いキャラクター位置に合わせてFar設定をリアルタイムで更新する
		PostEffectCamera.farClipPlane = Mathf.Sqrt(Distance) + 1.0f;

		//レイヤーマスク切り替え、アウトラインがエフェクトに被らないようにするためエフェクト部分をマスキングする
		PostEffectCamera.cullingMask = 1 << LayerMask.NameToLayer("Effect");

		//レンダリングテクスチャをセット
		PostEffectCamera.targetTexture = MaskingTexture;

		//カメラのレンダリングモードを変更
		PostEffectCamera.depthTextureMode |= DepthTextureMode.None;

		//クリアフラグ設定
		PostEffectCamera.clearFlags = CameraClearFlags.SolidColor;

		//フラグ切り替え
		OutLineFlag = false;

		//レンダリング
		PostEffectCamera.Render();

		//レイヤーマスク切り替え
		PostEffectCamera.cullingMask = Mask;

		//レンダリングテクスチャをセット
		PostEffectCamera.targetTexture = OutLineTexture;

		//カメラをデプスバッファと法線バッファをレンダリングするモードにする
		PostEffectCamera.depthTextureMode |= DepthTextureMode.DepthNormals;

		//クリアフラグ設定
		PostEffectCamera.clearFlags = CameraClearFlags.Depth;

		//フラグ切り替え
		OutLineFlag = true;

		//レンダリング
		PostEffectCamera.Render();
	}

	//レンダリングが完了すると呼ばれる
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		//フラグでシェーダー切り替え
		if(OutLineFlag)
		{
			//シェーダー切り替え
			OutLineMaterial.shader = OutLineShader;

			//シェーダーにマスキング用テクスチャを渡す
			OutLineMaterial.SetTexture("_MaskTex", MaskingTexture);

			//シェーダー呼び出しレンダーテクスチャに保存
			Graphics.Blit(src, dest, OutLineMaterial);
		}
		else
		{
			//シェーダー切り替え
			OutLineMaterial.shader = MaskingShader;

			//シェーダー呼び出しレンダーテクスチャに保存
			Graphics.Blit(src, dest, OutLineMaterial);
		}
	}
}

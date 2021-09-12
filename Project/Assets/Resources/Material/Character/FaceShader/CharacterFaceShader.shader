Shader "Custom/CharacterFaceShader"
{
	Properties
	{
		//肌色
		_SkinColor("_SkinColor", Color) = (1, 1, 1, 1)						

		//顔ベーステクスチャ
		_TexFaceBase("_TexFaceBase", 2D) = "white" {}

		//顔ハイライトのテクスチャ
		_TexFaceHiLight("_TexFaceHiLight", 2D) = "white" {}

		//ハイライトのmatcapテクスチャ
		_TexFaceHiLightMatCap("_TexFaceHiLightMatCap", 2D) = "black" {}

		_SunnyH("_SunnyH", Range(0.0, 1.0)) = 0.0						//日向の色相
		_SunnyS("_SunnyS", Range(-1.0, 1.0)) = 0.0						//日向の彩度
		_SunnyV("_SunnyV", Range(-1.0, 1.0)) = 0.0						//日向の輝度

		_SunnyVol("_SunnyVol", Range(0, 1.0)) = 0						//日向のかかり具合
		_SunnyGradation("_SunnyGradation", Range(0.0, 1.0)) = 0.0		//日向のグラデーション具合

		//前方光源からの影
		_TexFaceShadowFront("_TexFaceShadowFront", 2D) = "white" {}
		_TexFaceShadowFrontTop("_TexFaceShadowFrontTop", 2D) = "white" {}
		_TexFaceShadowFrontLeft("_TexFaceShadowFrontLeft", 2D) = "white" {}
		_TexFaceShadowFrontRight("_TexFaceShadowFrontRight", 2D) = "white" {}
		_TexFaceShadowFrontBottom("_TexFaceShadowFrontBottom", 2D) = "white" {}

		//後方光源からの影
		_TexFaceShadowBack("_TexFaceShadowBack", 2D) = "white" {}
		_TexFaceShadowBackTop("_TexFaceShadowBackTop", 2D) = "white" {}
		_TexFaceShadowBackLeft("_TexFaceShadowBackLeft", 2D) = "white" {}
		_TexFaceShadowBackRight("_TexFaceShadowBackRight", 2D) = "white" {}
		_TexFaceShadowBackBottom("_TexFaceShadowBackBottom", 2D) = "white" {}

		//左方光源からの影
		_TexFaceShadowLeft("_TexFaceShadowLeft", 2D) = "white" {}
		_TexFaceShadowLeftTop("_TexFaceShadowLeftTop", 2D) = "white" {}
		_TexFaceShadowLeftBottom("_TexFaceShadowLeftBottom", 2D) = "white" {}

		//右方光源からの影
		_TexFaceShadowRight("_TexFaceShadowRight", 2D) = "white" {}
		_TexFaceShadowRightTop("_TexFaceShadowRightTop", 2D) = "white" {}
		_TexFaceShadowRightBottom("_TexFaceShadowRightBottom", 2D) = "white" {}

		//上方光源からの影
		_TexFaceShadowTop("_TexFaceShadowTop", 2D) = "white" {}

		//下方光源からの影
		_TexFaceShadowBottom("_TexFaceShadowBottom", 2D) = "white" {}

		//背景からの影色
		_DropShadowColor("_DropShadowColor", Color) = (1, 1, 1, 1)				

	}

	SubShader
	{
		Tags 
		{
			"Queue" = "AlphaTest" 
			"RenderType" = "TransparentCutout" 
			"IgnoreProjector" = "True" 
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			//両面表示
			Cull off

			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0						//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert						//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag					//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdbase			//マルチコンパイル、TRANSFER_SHADOW用

			//変数宣言

			fixed4 _SkinColor;					//肌色

			sampler2D _TexFaceBase;				//顔テクスチャ
			sampler2D _TexFaceHiLight;			//顔ハイライトのテクスチャ
			sampler2D _TexFaceHiLightMatCap;	//ハイライトのmatcap

			float4x4 _LightMatrix;				//スクリプトから受け取るディレクショナルライトのマトリクス、ハイライトのmatcapに使用

			sampler2D texXX;					//X軸直交テクスチャ
			sampler2D texYY;					//Y軸直交テクスチャ
			sampler2D texZZ;					//Z軸直交テクスチャ
			sampler2D texXY;					//X軸とY軸の中間テクスチャ
			sampler2D texXZ;					//X軸とZ軸の中間テクスチャ
			sampler2D texYZ;					//Y軸とZ軸の中間テクスチャ

			float TextureBlendXX;				//X軸直交テクスチャ用ブレンド比率
			float TextureBlendYY;				//Y軸直交テクスチャ用ブレンド比率
			float TextureBlendZZ;				//Z軸直交テクスチャ用ブレンド比率
			float TextureBlendXY;				//X軸とY軸の中間テクスチャ用ブレンド比率
			float TextureBlendXZ;				//X軸とZ軸の中間テクスチャ用ブレンド比率
			float TextureBlendYZ;				//Y軸とZ軸の中間テクスチャ用ブレンド比率

			fixed4 colorXX;						//X軸直交テクスチャカラー
			fixed4 colorYY;						//Y軸直交テクスチャカラー
			fixed4 colorZZ;						//Z軸直交テクスチャカラー
			fixed4 colorXY;						//X軸とY軸の中間テクスチャカラー
			fixed4 colorXZ;						//X軸とZ軸の中間テクスチャカラー
			fixed4 colorYZ;						//Y軸とZ軸の中間テクスチャカラー

			fixed4 _SunnyColor;				//日向
			float _SunnyH;					//日向色相
			float _SunnyS;					//日向彩度
			float _SunnyV;					//日向明度
			float _SunnyVol;				//日向のレベル
			float _SunnyGradation;			//日向のグラデーション具合
			fixed4 SunnyArea;				//日向の範囲

			fixed4 _LightColor0;				//ライトカラー

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				// テクスチャ座標を取得
				float2 uv : TEXCOORD0;
			};

			//頂点シェーダーからフラグメントシェーダーに情報を渡す構造体を宣言
			struct vertex_output
			{
				// 頂点座標
				float4 pos : SV_POSITION;

				// 法線情報
				half3 normal: NORMAL;

				// テクスチャ座標
				float2 uv : TEXCOORD0;

				// ハイライト用テクスチャ座標
				float2 hilightuv : TEXCOORD1;

				//ドロップシャドウ
				SHADOW_COORDS(2)
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = v.uv;

				//頂点座標をクリップ座標系に変換
				re.pos = UnityObjectToClipPos(v.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//スクリプトから受け取ったライトのマトリクスでmatcap用uvを求める
				re.hilightuv = mul(_LightMatrix, re.normal).xy * 0.5 + 0.5;

				//ドロップシャドウ
				TRANSFER_SHADOW(re);
				
				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//return用変数を宣言
				fixed4 re = 0;				

				//肌色を塗る
				re = _SkinColor;

				//各軸テクスチャから描画するカラーを取得
				colorXX = tex2D(texXX, i.uv);
				colorXY = tex2D(texXY, i.uv);
				colorXZ = tex2D(texXZ, i.uv);
				colorYY = tex2D(texYY, i.uv);
				colorYZ = tex2D(texYZ, i.uv);
				colorZZ = tex2D(texZZ, i.uv);
			
				//X軸直交テクスチャをブレンド
				re = lerp(re, colorXX, saturate(TextureBlendXX * colorXX.a));

				//Y軸直交テクスチャをブレンド
				re = lerp(re, colorYY, saturate(TextureBlendYY * colorYY.a));

				//Z軸直交テクスチャをブレンド
				re = lerp(re, colorZZ, saturate(TextureBlendZZ * colorZZ.a));

				//X軸とZ軸の中間テクスチャをブレンド
				re = lerp(re, colorXZ, saturate(TextureBlendXZ * colorXZ.a));

				//X軸とY軸の中間テクスチャをブレンド
				re = lerp(re, colorXY, saturate(TextureBlendXY * colorXY.a));

				//Y軸とZ軸の中間テクスチャをブレンド
				re = lerp(re, colorYZ, saturate(TextureBlendYZ * colorYZ.a));

				//肌色から日向の色を作る
				_SunnyColor = float4(HSV2RGB(float3(RGB2HSV(re.rgb).r + _SunnyH, RGB2HSV(re.rgb).g + _SunnyS, RGB2HSV(re.rgb).b + _SunnyV)), re.a);

				//日向を適応
				re = lerp(re, _SunnyColor, SHADOW_ATTENUATION(i));

				//顔テクスチャを乗算
				re *= tex2D(_TexFaceBase, i.uv);

				//顔のハイライトを加算
				re.rgb += lerp(0, tex2D(_TexFaceHiLight, i.uv), tex2D(_TexFaceHiLightMatCap, i.hilightuv)) * saturate(dot(i.normal, _WorldSpaceLightPos0));

				//ライトカラーをブレンド
				re *= lerp(1, _LightColor0, _LightColor0.a);		

				//re = tex2D(_TexFaceBase, i.uv);



				//出力
				return re;

			}

			//プログラム終了
			ENDCG

		}

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
	}
}
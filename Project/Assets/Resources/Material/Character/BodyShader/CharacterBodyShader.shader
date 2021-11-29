Shader "Custom/CharacterBodyShader"
{
	Properties
	{
		//--体とか服とか髪とか--//
		_Shadow1H("_Shadow1H", Range(0.0, 1.0)) = 0.0					//1影の色相
		_Shadow1S("_Shadow1S", Range(-1.0, 1.0)) = 0.0					//1影の彩度
		_Shadow1V("_Shadow1V", Range(-1.0, 1.0)) = 0.0					//1影の輝度

		_Shadow1Vol("_Shadow1Vol", Range(-0.01, 1.0)) = 0.5				//1影のかかり具合
		_Shadow1Gradation("_Shadow1Gradation", Range(0.0, 1.0)) = 0.0	//1影のグラデーション具合

		_Shadow2H("_Shadow2H", Range(0.0, 1.0)) = 0.0					//2影の色相
		_Shadow2S("_Shadow2S", Range(-1.0, 1.0)) = 0.0					//2影の彩度
		_Shadow2V("_Shadow2V", Range(-1.0, 1.0)) = 0.0					//2影の輝度

		_Shadow2Vol("_Shadow2Vol", Range(-0.01 , 1.0)) = 0.1			//2影のかかり具合
		_Shadow2Gradation("_Shadow2Gradation", Range(0.0, 1.0)) = 0.0	//2影のグラデーション具合

		_SunnyH("_SunnyH", Range(0.0, 1.0)) = 0.0						//日向の色相
		_SunnyS("_SunnyS", Range(-1.0, 1.0)) = 0.0						//日向の彩度
		_SunnyV("_SunnyV", Range(-1.0, 1.0)) = 0.0						//日向の輝度

		_SunnyVol("_SunnyVol", Range(0, 1.0)) = 0						//日向のかかり具合
		_SunnyGradation("_SunnyGradation", Range(0.0, 1.0)) = 0.0		//日向のグラデーション具合

		_TexBase("_TexBase", 2D) = "white" {}							//ベーステクスチャ
		_TexLine("_TexLine", 2D) = "white" {}							//線画テクスチャ
		_TexNormal("_TexNormal", 2D) = "bump" {}						//ノーマルマップ
		_TexHiLight("_TexHiLight", 2D) = "white" {}						//ハイライトのテクスチャ
		_HiLightMatCap("_HiLightMatCap", 2D) = "black" {}				//ハイライトのmatcap

		_VanishNum("_VanishNum",float) = 0								//消滅用係数	
	}

	SubShader
	{
		Tags 
		{
			"Queue" = "AlphaTest" 
			"RenderType" = "TransparentCutout" 
			//"IgnoreProjector" = "True" 
		}

		//アルファブレンド
		//Blend SrcAlpha OneMinusSrcAlpha

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

			//--体とか服とか髪とか--//
			sampler2D _TexBase;				//ベーステクスチャ
			sampler2D _TexLine;				//線画テクスチャ
			sampler2D _TexNormal;			//ノーマルマップ
			sampler2D _TexHiLight;			//ハイライトのテクスチャ
			sampler2D _HiLightMatCap;		//ハイライトのmatcap

			fixed4 _Shadow1Color;			//1影
			float _Shadow1H;				//1影色相
			float _Shadow1S;				//1影彩度
			float _Shadow1V;				//1影明度
			float _Shadow1Vol;				//影1のレベル
			float _Shadow1Gradation;		//影1のグラデーション具合
			fixed4 Shadow1Area;				//影1の範囲

			fixed4 _Shadow2Color;			//2影
			float _Shadow2H;				//2影色相
			float _Shadow2S;				//2影彩度
			float _Shadow2V;				//2影明度
			float _Shadow2Vol;				//影2のレベル
			float _Shadow2Gradation;		//影2のグラデーション具合
			fixed4 Shadow2Area;				//影2の範囲

			fixed4 _SunnyColor;				//日向
			float _SunnyH;					//日向色相
			float _SunnyS;					//日向彩度
			float _SunnyV;					//日向明度
			float _SunnyVol;				//日向のレベル
			float _SunnyGradation;			//日向のグラデーション具合
			fixed4 SunnyArea;				//日向の範囲

			float _DotLightNormal;			//光源と法線内積

			fixed4 _LightColor0;			//ライトカラー	

			float4x4 _LightMatrix;			//スクリプトから受け取るディレクショナルライトのマトリクス、ハイライトのmatcapに使用

			float _VanishNum;				//消滅用係数

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				// テクスチャ座標を取得
				float2 uv : TEXCOORD0;

				//頂点カラー
				//float4 vertColor : COLOR;
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

				// ハイライト用テクスチャ座標を取得
				float2 hilightuv : TEXCOORD1;

				//ドロップシャドウ
				SHADOW_COORDS(3)
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
				//return用変数を宣言、ベースtextureを貼る
				fixed4 re = tex2D(_TexBase, i.uv);
				
				//ベーステクスチャから色をサンプリングして日向の色を作る
				_SunnyColor = float4(HSV2RGB(float3(RGB2HSV(re.rgb).r + _SunnyH, RGB2HSV(re.rgb).g + _SunnyS, RGB2HSV(re.rgb).b + _SunnyV)), re.a);
				
				//日向を適応
				re = lerp(re, _SunnyColor, SHADOW_ATTENUATION(i));
				
				//ベーステクスチャから色をサンプリングして1影の色を作る
				_Shadow1Color = float4(HSV2RGB(float3(RGB2HSV(re.rgb).r + _Shadow1H, RGB2HSV(re.rgb).g + _Shadow1S, RGB2HSV(re.rgb).b + _Shadow1V)), re.a);
			
				//ベーステクスチャから色をサンプリングして2影の色を作る
				_Shadow2Color = float4(HSV2RGB(float3(RGB2HSV(re.rgb).r + _Shadow2H, RGB2HSV(re.rgb).g + _Shadow2S, RGB2HSV(re.rgb).b + _Shadow2V)), re.a);
				
				//光源と法線の内積を求めてハーフランパートを求める、ノーマルマップもここで展開する
				_DotLightNormal = (dot(i.normal + UnpackNormal(tex2D(_TexNormal, i.uv)), _WorldSpaceLightPos0) + 1) * 0.5;

				//1影の範囲を求める
				Shadow1Area = lerp(re, _Shadow1Color, ReverseSaturate(InverseLerp(_Shadow1Vol - _Shadow1Gradation, _Shadow1Vol + _Shadow1Gradation, _DotLightNormal)));

				//1影を適応
				re = Shadow1Area;

				//2影の範囲を求める
				Shadow2Area = lerp(re, _Shadow2Color, ReverseSaturate(InverseLerp(_Shadow2Vol - _Shadow2Gradation, _Shadow2Vol + _Shadow2Gradation, _DotLightNormal)));

				//2影を適応
				re = Shadow2Area;

				//線画テクスチャを乗算
				re *= tex2D(_TexLine, i.uv);

				//ハイライトを加算
				re.rgb += lerp(0, tex2D(_TexHiLight, i.uv), tex2D(_HiLightMatCap, i.hilightuv)) * saturate(dot(i.normal, _WorldSpaceLightPos0));

				//ライトカラーをブレンド
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//透明部分をクリップ、消滅用の乱数精製
				clip(re.a - 0.01 - ((Random(i.uv * _VanishNum, round(_VanishNum)) + 0.05) * _VanishNum));

				//出力
				return re;
			}

			//プログラム終了
			ENDCG

		}
		/*
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0						//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert						//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag					//各ピクセル毎に実行されるフラグメントシェーダ

			fixed4 _LightColor0;					//ライトカラー	

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
				
				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{			
				//出力
				return _LightColor0;			
			}

			//プログラム終了
			ENDCG
		}*/

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
	}
}
Shader "Custom/FootImpactShader"
{
	Properties
	{
		_TexParticle("_TexParticle", 2D) = "white" {}

		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("_ZTest", Float) = 0

	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent" 
				"RenderType"="Transparent"			
			}

			//アルファブレンド
			Blend SrcAlpha OneMinusSrcAlpha

			//Zテスト
			ZTest[_ZTest]

			Pass
			{
				Tags
				{
					"LightMode" = "ForwardBase"
				}

				//プログラム開始
				CGPROGRAM

				//インクルードファイル
				#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
				#include "AutoLight.cginc"									//ライティングやシャドウ機能
				#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

				#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
				#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
				#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
				#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _TexParticle;

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

				//頂点カラー
				float4 vertColor : COLOR;
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

				//頂点カラー
				float4 vertColor : COLOR;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input i)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = i.uv;

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(i.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(i.normal);

				//頂点カラー
				re.vertColor = i.vertColor;

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{

				//出力用変数を宣言、パーティクルシステムで設定した頂点カラーを適応
				fixed4 re = i.vertColor * (_LightColor0 + 0.25);

			//加算の場合
			//出力用変数を宣言、テクスチャカラーを乗算
			re *= tex2D(_TexParticle, i.uv);

			/*
			//アルファブレンドの場合
			//テクスチャの透明度を適応
			re.a *= tex2D(_TexParticle, i.uv).a;
			*/

			//出力
			return re * i.vertColor.a;
		}

				//プログラム終了
				ENDCG

			}
		}
}
/*
{
    Properties
    {
        _TexParticle("_TexParticle", 2D) = "white" {}

		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("_ZTest", Float) = 0

    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent" 
			"RenderType"="Transparent"			
		}

		//アルファブレンド
		Blend SrcAlpha OneMinusSrcAlpha

		//両面表示
		//Cull off

		//Zテスト
		ZTest[_ZTest]

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _TexParticle;				//ノーマルマップ

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

				//頂点カラー
				float4 Color : COLOR;
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

				//頂点カラー
				float4 Color : COLOR;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = v.uv;

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(v.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//頂点カラー
				re.Color = v.Color;

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//頂点カラーとライトカラー
				fixed4 re = i.Color * _LightColor0;

				//光源と法線の内積を乗算
				re.rgb *= (dot(i.normal , _WorldSpaceLightPos0) + 1) * 0.5;

				//テクスチャのアルファ
				re.a *= tex2D(_TexParticle, i.uv).a;

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
*/
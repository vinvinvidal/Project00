Shader "Custom/FireShader"
{
    Properties
    {
		_FireMainTex ("_FireMainTex", 2D) = "white" {}		//火のメインテクスチャ
		_FireNormalTex("_FireNormalTex", 2D) = "bump" {}	//火の法線テクスチャ

		_FadeCount("_FadeCount", Range(0.0, 1.0)) = 1.0		//フェードに使う数値
    }

    SubShader
    {
		Tags
		{
			"Queue" = "Transparent" 
			"RenderType"="Transparent"
		}

		//加算ブレンド
		Blend One One

		//ライティングしない
		Lighting Off

		//両面表示
		//Cull off

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

			//変数宣言
			sampler2D _FireMainTex;				//火のメインテクスチャ
			sampler2D _FireNormalTex;			//火の法線テクスチャ
			float4 _FireMainTex_ST;				//火のメインテクスチャのタイリングとオフセット
			float4 _FireNormalTex_ST;			//火の法線テクスチャのタイリングとオフセット

			float _FadeCount;					//フェードに使う数値

			fixed4 _LightColor0;				//ライトカラー

			//float4x4 _CameraMatrix;			//スクリプトから受け取るカメラのマトリクス、Matcapに使用
			
			

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				//正接
				half4 tangent : TANGENT;

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

				//正接
				half3 tangent : TEXCOORD1;

				//従法線
				half3 binormal : TEXCOORD2;

				//頂点カラー
				float4 vertColor : COLOR;

				// Matcap用テクスチャ座標
				//float2 MatCapuv : TEXCOORD3;

				//カメラからのベクトル
				//float3 ViewVec   : TEXCOORD4;
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

				//正接
				re.tangent = mul((float3x3)UNITY_MATRIX_MV, i.tangent.xyz);

				//従法線
				re.binormal = normalize(cross(i.normal.xyz, i.tangent.xyz) * i.tangent.w * unity_WorldTransformParams.w);

				//ビューベクトル
				//re.ViewVec = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, i.pos).xyz);

				//スクリプトから受け取ったライトのマトリクスでmatcap用uvを求める
				//re.MatCapuv = mul(_CameraMatrix, re.normal).xy * 0.5 + 0.5;

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				// ノーマルマップから法線情報を取得する
				half3 NormalMap = UnpackNormal(tex2D(_FireNormalTex, i.uv * _FireNormalTex_ST.xy + _FireNormalTex_ST.zw));

				// ビュー空間に変換
				NormalMap = (i.tangent * NormalMap.x) + (i.binormal * NormalMap.y) + (i.normal * NormalMap.z);

				//UVに適応
				float2 MatCapUV = NormalMap.xy * 0.5 + 0.5;

				//Return用変数宣言、MatCapでメインテクスチャを貼る
				fixed4 re = tex2D(_FireMainTex, MatCapUV * _FireMainTex_ST.xy + _FireMainTex_ST.zw);
				
				//頂点カラーを乗算
				re *= i.vertColor;
				//re *= lerp(1, _LightColor0, _LightColor0.a);

				//出力
				return re * _FadeCount;

				

				/*
				return dot(NormalMap, i.ViewVec);
				//Return用変数宣言、メインテクスチャを貼る
				fixed4 re = 1;//tex2D(_FireMainTex, i.uv);				
				
				//ライトカラーを乗算
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//MatCap用のUVにカメラ方向からの法線マップ内積を加算
				i.MatCapuv += (dot(i.normal + UnpackNormal(tex2D(_FireNormalTex, i.uv * _FireNormalTex_ST.xy + _FireNormalTex_ST.zw)) , _WorldSpaceCameraPos) + 1) * 0.5;

				re *= tex2D(_FireSubTex, i.MatCapuv);

				//Matcapを乗算
				re *= tex2D(_FireMatCapTex, i.MatCapuv);				

				//fixed4 re =(dot(UnpackNormal(tex2D(_FireNormalTex, i.uv)) , i.ViewVec) + 1) * 0.5;
				*/

				//出力
				//return re;
			}

            ENDCG
        }
    }
}

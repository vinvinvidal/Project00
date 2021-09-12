//共用サブライト
//いろんなシェーダーに継承させるやつ

Shader "Unlit/PublicSubLight"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		Pass
		{
			//シェーダーに継承させるために名前を定義する
			Name "PublicSubLight"

			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			//加算
			Blend OneMinusDstColor One

			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdadd		//マルチコンパイル、ドロップシャドウを受けたい場合など

			fixed4 _LightColor0;				//ライトカラー

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 vertex : POSITION;

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

				//頂点ワールド座標
				float3 worldPos : TEXCOORD1;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = v.uv;

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(v.vertex);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//頂点をワールド座標系に変換
				re.worldPos = mul(unity_ObjectToWorld, v.vertex);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//ポイントライトとかのサブライトの処理
				UNITY_LIGHT_ATTENUATION(Attenuation, i, i.worldPos);

				//出力
				return _LightColor0 * dot(i.normal, UnityWorldSpaceLightDir(i.worldPos)) * Attenuation;
			}

			//プログラム終了
			ENDCG
		}
	}
}

Shader "Custom/WireShader"
{
	Properties
	{
		_MainTexture("_MainTexture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest" 

			//"Queue" = "Transparent" 
			//"RenderType"="Transparent"			
		}

		//アルファブレンド
		//Blend SrcAlpha OneMinusSrcAlpha

		//Zテスト
		//ZTest[_ZTest]

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
			//#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _MainTexture;

			float4 _MainTexture_ST;	

			fixed4 _LightColor0;				//ライトカラー

			vector VartexVector;

			float WaveNum = 0;

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				//頂点ID
				uint vid : SV_VertexID;

				//頂点カラー
				float4 vertColor : COLOR;

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

				//頂点をワールド座標に変換
				v.pos = mul(unity_ObjectToWorld, v.pos);

				//波打ちアニメーション
				v.pos.y += (sin(round(v.vertColor.r * 10) + (-_Time.z * 10)) * 0.9) * v.vertColor.g * WaveNum;

				//頂点をオブジェクト座標に戻す
				v.pos = mul(unity_WorldToObject, v.pos);

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(v.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				fixed4 re = tex2D(_MainTexture, i.uv * _MainTexture_ST.xy + _MainTexture_ST.zw) * _LightColor0;

				//出力
				return re;
			}

			//プログラム終了
			ENDCG

		}
	}
}
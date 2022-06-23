Shader "Custom/BombEffectShader"
{
	Properties
	{
		_TexParticle("_TexParticle", 2D) = "white" {}
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
			sampler2D _TexParticle;

			float4 _TexParticle_ST;	

			fixed4 _LightColor0;				//ライトカラー

			float VertNum;

			vector OBJPos;

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				// テクスチャ座標を取得
				float2 uv : TEXCOORD0;

				//頂点ID
				uint vid : SV_VertexID;

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
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = v.uv;

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//頂点をワールド座標に変換
				v.pos = mul(unity_ObjectToWorld, v.pos);

				//爆発
				//v.pos.xyz += re.normal * (saturate(v.vid % 50) -1) * -1 * VertNum;

				v.pos.xyz += normalize(v.pos.xyz - OBJPos.xyz) * (saturate(v.vid % 40) -1) * -1 * VertNum;				

				//頂点をオブジェクト座標に戻す
				v.pos = mul(unity_WorldToObject, v.pos);

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(v.pos);



				//頂点カラー
				re.vertColor = v.vertColor;

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//頂点カラーとライトカラー
				//fixed4 re = (tex2D(_TexParticle, i.uv) + i.vertColor) * _LightColor0;

				fixed4 re = i.vertColor;

				//テクスチャのアルファ
				re.a *= tex2D(_TexParticle, i.uv * _TexParticle_ST.xy + _TexParticle_ST.zw).a - i.vertColor.a * 0.5f;

				//光源と法線の内積を乗算
				re.rgb *= (dot(i.normal, _WorldSpaceLightPos0) + 1) * 0.5;

				//透明部分をクリップ、消滅用の乱数精製
				clip(re.a);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG

		}

		//共用シャドウキャスター
		//UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"

	}
}
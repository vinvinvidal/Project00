Shader "Custom/StageShader_Metalic"
{
	Properties
	{
		//表面テクスチャ
		_TexSurface("_TexSurface", 2D) = "white" {}

		//ペイントテクスチャ
		_TexPaint("_TexPaint", 2D) = "white" {}

		//金属感
		_MetarlicVol("_MetarlicVol", Range(0, 1.0)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
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

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _TexSurface;				//表面テクスチャ
			float4 _TexSurface_ST;				//表面テクスチャのタイリングとオフセット

			sampler2D _TexPaint;				//ペイントテクスチャ

			fixed4 _LightColor0;				//ライトカラー

			float _MetarlicVol;					//金属感

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

				//ドロップシャドウ
				SHADOW_COORDS(2)

				//モデルのワールド座標
                float3 worldPos : TEXCOORD3;
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

				//ドロップシャドウ
				TRANSFER_SHADOW(re);

				//モデルをワールドに変換
				re.worldPos = mul(unity_ObjectToWorld, v.pos);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{

				//出力用変数宣言、表面テクスチャを貼る
				fixed4 re = tex2D(_TexSurface, i.uv * _TexSurface_ST.xy + _TexSurface_ST.zw);
				
				//ペイントテクスチャ合成
				re = lerp(re, tex2D(_TexPaint, i.uv), tex2D(_TexPaint, i.uv).a);

				//リフレクションをブレンド
				re *= lerp(1 , UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflect(-normalize(_WorldSpaceCameraPos - i.worldPos), i.normal), 0) , _MetarlicVol);

				//ハーフランパート乗算
				re *= (dot(i.normal, _WorldSpaceLightPos0) + 1) * 0.9;

				//ハイライトを適当に加算
				re += round(saturate(dot(i.normal, _WorldSpaceLightPos0) - 0.8));

				//オブジェクトからのドロップシャドウ乗算
				re *= saturate(SHADOW_ATTENUATION(i) + 0.5);

				//ライトカラーをブレンド
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}

		//共用サブライト
		UsePass "Unlit/PublicSubLight/PublicSubLight"

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
	}
}
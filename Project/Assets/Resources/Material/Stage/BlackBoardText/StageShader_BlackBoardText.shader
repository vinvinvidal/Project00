Shader "Custom/StageShader_BlackBoardText"
{
	Properties
	{
		//表面テクスチャ
		_BlackBoardTextTex("_BlackBoardTextTex", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent" 
			"RenderType"="Transparent"	
		}

		//両面表示
		//Cull off

		//アルファブレンド
		Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _BlackBoardTextTex;		//黒板テキストテクスチャ

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

				//ドロップシャドウ
				SHADOW_COORDS(1)
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

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力用変数宣言、表面テクスチャを貼る
				fixed4 re = tex2D(_BlackBoardTextTex, i.uv);

				//オブジェクトからのドロップシャドウ乗算
				re *= saturate(SHADOW_ATTENUATION(i) + 0.5);

				//ライトカラーをブレンド
				re.rgb *= lerp(1, _LightColor0, _LightColor0.a);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}
	}
}

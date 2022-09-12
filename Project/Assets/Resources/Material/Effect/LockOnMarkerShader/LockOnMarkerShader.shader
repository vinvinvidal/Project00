Shader "Custom/LockOnMarkerShader"
{
	Properties
	{
		_TexMain("_TexMain", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest"
			"RenderType" = "TransparentCutout"
		}

		//アルファブレンド
		//Blend SrcAlpha OneMinusSrcAlpha

		//Zテスト
		ZTest[Always]

		//両面表示
		Cull off

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
			sampler2D _TexMain;

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// テクスチャ座標を取得
				float2 uv : TEXCOORD0;
			};

			//頂点シェーダーからフラグメントシェーダーに情報を渡す構造体を宣言
			struct vertex_output
			{
				// 頂点座標
				float4 pos : SV_POSITION;

				// テクスチャ座標
				float2 uv : TEXCOORD0;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用構造体宣言
				vertex_output re;

				//頂点座標
				re.pos = UnityObjectToClipPos(v.pos);

				//UV
				re.uv = v.uv;

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//return用変数を宣言、ベースtextureを貼る
				fixed4 re = tex2D(_TexMain, i.uv);

				//透明部分をクリップ
				clip(re.a - 0.01);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}
	}
}

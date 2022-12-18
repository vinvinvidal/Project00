Shader "Custom/FakeMirrorShader"
{
	Properties
	{	
		//消滅用係数
		_VanishNum("_VanishNum",float) = 0

		_VanishTex("_VanishTex", 2D) = "white" {}						//消滅用テクスチャ
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

			//プログラム開始
			CGPROGRAM

			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ

			//変数宣言

			sampler2D _MainTex;
			fixed4 _LightColor0;			//ライトカラー
			int MirrorON;
			float _VanishNum;				//消滅用係数

			sampler2D _VanishTex;			//消滅用テクスチャ

			float4 _VanishTex_ST;			//消滅用テクスチャスケールタイリング
			

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

				// Grab用テクスチャ座標
				half4 GrabPos : TEXCOORD1;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input i)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = i.uv;

				//UVの左右を反転
				re.uv.x = abs(re.uv.x - 1);

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(i.pos);

				// Grab用テクスチャ座標
				re.GrabPos = ComputeScreenPos(re.pos);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//Return用変数宣言
				fixed4 re;

				if (MirrorON == 1)
				{
					re = tex2D(_MainTex, i.uv) * _LightColor0;
				}
				else
				{
					re = _LightColor0;
				}

				//消失用テクスチャのタイリング設定
				i.GrabPos.xy *= _VanishTex_ST.xy;

				//テクスチャと変数から透明度を算出
				re.a -= tex2Dproj(_VanishTex, i.GrabPos).a;

				//透明部分をクリップ、消滅用の乱数精製
				clip(re.a - 0.01);

				//出力
				return re;
			}

			ENDCG
		}
	}
}

Shader "Unlit/EyeShader"
{
    Properties
    {
        _EyeTex ("_EyeTex", 2D) = "white" {}						//目のテクスチャ
		_EyeHiLight("_EyeHiLight", 2D) = "white" {}					//目のハイライトのテクスチャ
		_EyeShadow("_EyeShadow", 2D) = "white" {}					//目の影のテクスチャ
		_EyeShadowColor("_EyeShadowColor", Color) = (0, 0, 0, 0)	//ドロップシャドウのカラー
    }

    SubShader
    {
		Tags
		{
			"Queue" = "Geometry"
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

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _EyeTex;					//目のテクスチャ
			sampler2D _EyeHiLight;				//目のハイライトのテクスチャ
			sampler2D _EyeShadow;				//目の影のテクスチャ
			fixed4 _EyeShadowColor;				//ドロップシャドウのカラー
			float4 _EyeTex_ST;					//目のテクスチャのタイリングとオフセット

			fixed4 _LightColor0;				//ライトカラー

			float Eye_HiLightRotationSin;		//スクリプトから受け取るハイライト回転用Sin
			float Eye_HiLightRotationCos;		//スクリプトから受け取るハイライト回転用Cos
			float2x2 Eye_HiLightRotation;		//ハイライト回転行列、光源位置によって回転させる

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

				// ハイライト用テクスチャ座標
				float2 EyeHilightuv : TEXCOORD2;

				//ドロップシャドウ
				//SHADOW_COORDS(1)

			};

			//頂点シェーダ
			vertex_output vert(vertex_input i)
			{

				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = i.uv;

				//ハイライト用UV格納
				re.EyeHilightuv = i.uv;

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(i.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(i.normal);

				//ドロップシャドウ
				//TRANSFER_SHADOW(re);

				//出力　
				return re;

			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{

				//Return用変数宣言してベーステクスチャを貼る、オフセットを足して眼球を移動
				fixed4 re = tex2D(_EyeTex, i.uv * _EyeTex_ST.xy + (_EyeTex_ST.zw - ((_EyeTex_ST.xy - 1) * 0.5f)));
				
				//まぶたから落ちる目の影を乗算合成
				re *= lerp(1,tex2D(_EyeShadow, i.uv), tex2D(_EyeShadow, i.uv).a);

				//ドロップシャドウを乗算
				//re *= saturate(_EyeShadowColor + round(SHADOW_ATTENUATION(i) + 0.75));

				//スクリプトから受け取った値でハイライト用回転行列を作成
				Eye_HiLightRotation = float2x2(Eye_HiLightRotationCos, -Eye_HiLightRotationSin, Eye_HiLightRotationSin, Eye_HiLightRotationCos);
				
				//ハイライトの基点を中心ズラす
				i.EyeHilightuv -= float2(0.5, 0.5);

				//ハイライトのUV回転
				i.EyeHilightuv = mul(Eye_HiLightRotation, i.EyeHilightuv);

				//ズラしたハイライトの基点を戻す
				i.EyeHilightuv += float2(0.5, 0.5);
			
				//ハイライトを加算合成
				re += lerp(0, tex2D(_EyeHiLight, i.EyeHilightuv), tex2D(_EyeHiLight, i.EyeHilightuv).a);

				//ライトカラーを乗算
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//出力
				return re;
			}

            ENDCG
        }

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
    }
}

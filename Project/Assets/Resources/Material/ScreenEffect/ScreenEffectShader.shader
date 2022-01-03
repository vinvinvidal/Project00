Shader "Custom/ScreenEffectShader"
{
	Properties
	{
		_TexParticle("_TexParticle", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			//常に最前面に出すため数値を挙げる
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		//アルファブレンド
		Blend SrcAlpha OneMinusSrcAlpha

		//ライティングしない
		Lighting Off

		//常にZテストをパス
		ZTest Always

		// GrabPassをテクスチャ名を指定して定義
		GrabPass { "_GrabScreenTex" }

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
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile Add Mul Nor	//Ifdefで合成処理を分けるためのマルチコンパイル

			//変数宣言
			sampler2D _TexParticle;
			sampler2D _GrabScreenTex;
			fixed4 _AddColor;
			fixed4 _Color;
			float4 _TexParticle_ST;					

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

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(i.pos);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(i.normal);

				// Grab用テクスチャ座標
				re.GrabPos = ComputeGrabScreenPos(re.pos);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力用変数を宣言
				fixed4 re;
				
				//_GrabをコピーしてSTをいじれる様にする
				_TexParticle = _GrabScreenTex;
				
				//プロジェクションで_Grabを貼る
				re = tex2Dproj(_TexParticle, i.GrabPos + _TexParticle_ST);

				//スクリプトから受け取ったカラーを加算合成
				#ifdef Add

                    re.rgb += _Color.rgb;

				//スクリプトから受け取ったカラーを乗算合成
                #elif Mul

                    re.rgb *= _Color.rgb;

				//スクリプトから受け取ったカラーをそのまま出力
				#elif Nor

					re.rgb = _Color.rgb;	
                
				#endif

				//スクリプトから受け取った透明度を適応
				re.a = _Color.a;

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}
	}
}
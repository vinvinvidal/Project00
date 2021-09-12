Shader "Unlit/OutLineShader"
{
	Properties
	{	
		_MainTex("_MainTex", 2D) = ""{}											//カメラのレンダリング結果
		_LineConcentration("_LineConcentration", Range(0.0, 1.0)) = 0.3			//アウトラインの濃さ

		_DepthLineWidth("_DepthLineWidth", Range(0.0, 5.0)) = 1.0				//アウトラインの太さ		
		_DepthLineBorder("_DepthLineBorder", Range(0.001, 0.01)) = 0.0			//アウトラインを描画するしきい値

		_NormalLineWidth("_NormalLineWidth", Range(0.0, 5.0)) = 1.0				//インラインの太さ	
		_NormalLineBorder("_NormalLineBorder", Range(0.001, 1)) = 0.0			//インラインを描画するしきい値
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			//関数宣言
			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される			
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ

			//変数宣言
			sampler2D _CameraDepthTexture;					//デプステクスチャ
			float4	_CameraDepthTexture_TexelSize;			//デプステクスチャの正規化されたピクセルサイズ、これがあることで正確に1ピクセル分のUV移動ができる

			sampler2D _CameraDepthNormalsTexture;			//法線テクスチャ	
			float4	_CameraDepthNormalsTexture_TexelSize;	//法線テクスチャの正規化されたピクセルサイズ、これがあることで正確に1ピクセル分のUV移動ができる

			sampler2D _MainTex;								//カメラのレンダリング結果
			sampler2D _MaskTex;								//マスキングのレンダリング結果

			float SampleDepth_C;							//現在のピクセルのデプスバッファ、以下その周囲８マス
			float SampleDepth_R;						
			float SampleDepth_L;						
			float SampleDepth_U;						
			float SampleDepth_B;						
			float SampleDepth_UR;						
			float SampleDepth_BR;						
			float SampleDepth_UL;						
			float SampleDepth_BL;		
			
			float3 SampleNormal_C;							//現在のピクセルの法線バッファ、以下その周囲８マス
			float3 SampleNormal_R;
			float3 SampleNormal_L;
			float3 SampleNormal_U;
			float3 SampleNormal_B;
			float3 SampleNormal_UR;
			float3 SampleNormal_BR;
			float3 SampleNormal_UL;
			float3 SampleNormal_BL;

			float CompareDepth;								//デプスバッファ最大差ピクセル抽出用
			float CompareNormal;							//法線バッファ最大差ピクセル抽出用

			float SampleUV_X;								//UVをずらすXの値
			float SampleUV_Y;								//UVをずらすYの値

			float _LineConcentration;						//アウトラインの濃さ

			float _DepthLineWidth;							//アウトラインの太さ
			float _DepthLineBorder;							//アウトラインを描画するしきい値

			float _NormalLineWidth;							//インラインの太さ
			float _NormalLineBorder;						//インラインを描画するしきい値

			float3 LineColorHSV;							//カラーバッファをHSVに変換して一時保存する


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
			vertex_output vert(vertex_input i)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = i.uv;
				
				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(i.pos);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//Return用変数宣言
				fixed4 re = float4(1,1,1,0);
				
				//--------深度バッファからアウトラインを作る----------//
				//深度バッファから色を抽出する場所をずらすための値			
				SampleUV_X = _CameraDepthTexture_TexelSize.x * _DepthLineWidth;
				SampleUV_Y = _CameraDepthTexture_TexelSize.y * _DepthLineWidth;
			
				//現在レンダリングされているピクセルの色を抽出
				SampleDepth_C = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
				
				//現在レンダリングされているピクセルの色を抽出
				SampleNormal_C = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv));

				//何もない所は処理しない
				if(SampleDepth_C + SampleNormal_C.x + SampleNormal_C.y + SampleNormal_C.z != 0)
				{
					//現在レンダリングされているピクセルの周囲のピクセルの色を抽出
					/*
					SampleDepth_R = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(SampleUV_X, 0)));
					SampleDepth_L = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(-SampleUV_X, 0)));
					SampleDepth_U = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(0, SampleUV_Y)));
					SampleDepth_B = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(0, -SampleUV_Y)));
					*/
					SampleDepth_UR = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(SampleUV_X, SampleUV_Y)));
					SampleDepth_BR = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(SampleUV_X, -SampleUV_Y)));
					SampleDepth_UL = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(-SampleUV_X, SampleUV_Y)));
					SampleDepth_BL = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + half2(-SampleUV_X, -SampleUV_Y)));
				
					//サンプルされた周囲の色から一番差が大きいピクセルを抜き出す
					/*
					CompareDepth = max((SampleDepth_C - SampleDepth_R), (SampleDepth_C - SampleDepth_L));
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_U));
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_B));
					*/
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_UL));
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_UR));
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_BR));
					CompareDepth = max(CompareDepth, (SampleDepth_C - SampleDepth_BL));
				
					//残った最大差のピクセルとしきい値を比較
					CompareDepth = max(CompareDepth, _DepthLineBorder);

					//そこからしきい値を引くと不合格ピクセルはゼロになる
					CompareDepth -= _DepthLineBorder;

					//出た値を透明度に取る
					re.a += lerp(0, 1, InverseLerp(0, _DepthLineBorder * 10 , CompareDepth));				

					//--------法線バッファからアウトラインを作る----------//
					//法線バッファから抽出する場所をずらすための値
					SampleUV_X = _CameraDepthTexture_TexelSize.x * _NormalLineWidth;
					SampleUV_Y = _CameraDepthTexture_TexelSize.y * _NormalLineWidth;
					/*
					SampleNormal_R = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(SampleUV_X, 0)));
					SampleNormal_L = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(-SampleUV_X, 0)));
					SampleNormal_U = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(0, SampleUV_Y)));
					SampleNormal_B = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(0, -SampleUV_Y)));
					*/
					SampleNormal_UR = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(SampleUV_X, SampleUV_Y)));
					SampleNormal_UL = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(-SampleUV_X, SampleUV_Y)));
					SampleNormal_BR = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(SampleUV_X, -SampleUV_Y)));
					SampleNormal_BL = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + half2(-SampleUV_X, -SampleUV_Y)));
					
					//サンプルされた周囲の色から一番差が大きいピクセルを抜き出す
					/*
					CompareNormal = max(distance(SampleNormal_C, SampleNormal_R), distance(SampleNormal_C, SampleNormal_L));
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_U));
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_B));
					*/
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_UR));
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_UL));
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_BR));
					CompareNormal = max(CompareNormal, distance(SampleNormal_C, SampleNormal_BL));					

					//残った最大差のピクセルとしきい値を比較
					CompareNormal = max(CompareNormal, _NormalLineBorder);

					//そこからしきい値を引くと不合格ピクセルはゼロになる
					CompareNormal -= _NormalLineBorder;

					//出た値を透明度に取る
					re.a += lerp(0, 1, InverseLerp(0, _NormalLineBorder * 10, CompareNormal));
				}

				//--------カラーバッファからアウトラインの色を作る----------//
				//カラーバッファをHSVに変換
				LineColorHSV = RGB2HSV(tex2D(_MainTex, i.uv).rgb);

				//彩度を上げ明度を落とし濃くする
				LineColorHSV.g += _LineConcentration;
				LineColorHSV.b -= _LineConcentration;

				//HSVをRGBに戻して線の色として使う
				re.rgb = HSV2RGB(LineColorHSV);

				//アルファを正規化
				re.a = saturate(re.a);

				//エフェクト部分をマスキングする
				re.a *= -1 * (tex2D(_MaskTex, i.uv).a -1);

				//出力
				return re;

			}

			ENDCG
		}
	}
}
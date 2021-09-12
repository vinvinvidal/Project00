Shader "Custom/MixTexShader"
{	
	Properties
	{		
		_MainTex("_MainTex", 2D) = ""{}													//メインカメラのレンダリング結果
		
		_EffectTex("_EffectTex", 2D) = ""{}												//ポストエフェクトカメラのレンダリング結果

		[Enum(Mix,0,Add,1,Mul,2,SubOnly,3,MainOnly,10)]_MixMode("MixMode", int) = 0		//合成法の選択
	}

	SubShader
	{
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
			}
			
			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数

			//変数宣言
			
			sampler2D _MainTex;					//メインカメラのレンダリング結果

			sampler2D _EffectTex;				//ポストエフェクトカメラのレンダリング結果

			int _MixMode;						//合成法の選択

			//関数宣言
			#pragma target 5.0
			#pragma vertex vert_img
			#pragma fragment frag

			//フラグメントシェーダ
			fixed4 frag(v2f_img i) : COLOR
			{
				//Return用変数を宣言しメインカメラのレンダリング結果を入れる
				fixed4 re = tex2D(_MainTex, i.uv);
				
				/*
				//ポストエフェクトカメラのレンダリング結果を合成
				if (_MixMode == 0)
				{
					re = lerp(re, tex2D(_EffectTex, i.uv), tex2D(_EffectTex, i.uv).a);
				}
				else if (_MixMode == 1)
				{
					re += lerp(0, tex2D(_EffectTex, i.uv), tex2D(_EffectTex, i.uv).a);
				}
				else if (_MixMode == 2)
				{
					re *= lerp(1, tex2D(_EffectTex, i.uv), tex2D(_EffectTex, i.uv).a);
				}
				else if (_MixMode == 3)
				{
					re = lerp(0, tex2D(_EffectTex, i.uv), tex2D(_EffectTex, i.uv).a);
				}
				*/
				
				//ポストエフェクトカメラのレンダリング結果を合成
				re *= lerp(1, tex2D(_EffectTex, i.uv), tex2D(_EffectTex, i.uv).a);

				//出力
				return re;

			}

			//プログラム終了
			ENDCG

		}
	}
}

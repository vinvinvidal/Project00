//共用シャドウキャスター
//いろんなシェーダーに継承させるやつ

Shader "Unlit/PublicShadowCaster"
{
    SubShader
    {
		Tags
		{
			"RenderType" = "Opaque"
		}

		Pass
		{
			//シェーダーに継承させるために名前を定義する
			Name "PublicShadowCaster"

			//Pass用タグ　ライティングなど各Pass毎の設定
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			//デプスバッファに書き込み
			ZWrite On

			//デプステスト
			ZTest LEqual

			//両面表示
			//Cull off
			
			//プログラム開始
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct vertex_output
			{
				V2F_SHADOW_CASTER;
			};

			vertex_output vert(appdata_base v)
			{
				vertex_output re;

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(re)

				return re;
			}

			float4 frag(vertex_output i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i);
			}

			ENDCG

		}
    }
}

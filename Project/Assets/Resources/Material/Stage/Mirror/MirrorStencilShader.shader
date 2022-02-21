Shader "Custom/MirrorStencilShader"
{
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

        ZWrite Off

        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
            // ステンシルバッファの設定
            Stencil
			{
				// ステンシルの番号
				Ref 2

				// ステンシルは常に成功  
				Comp Always  
				
				// ステンシルに成功したら2に置き換える  
				Pass Replace  
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

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 vertex : POSITION;
			};

			//頂点シェーダーからフラグメントシェーダーに情報を渡す構造体を宣言
			struct vertex_output
			{
				// 頂点座標
				float4 vertex : SV_POSITION;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;	

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.vertex = UnityObjectToClipPos(v.vertex);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力
				return fixed4 (0, 0, 0, 0);
			}

			//プログラム終了
			ENDCG
		}
	}
}
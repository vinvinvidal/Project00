Shader "Custom/MosaicShader"
{
    Properties
    {
		_BlockSize("_BlockSize", Float) = 0

		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("_ZTest", Float) = 4
    }

    SubShader
    {
		Tags
		{
			//"Queue" = "Geometry"
			"Queue" = "Transparent"
			"RenderType"="Transparent"			
		}

		//アルファブレンド
		Blend SrcAlpha OneMinusSrcAlpha 

		//ライティングしない
		Lighting Off

		//常にZテスト
		ZTest[_ZTest]

		// GrabPassをテクスチャ名を指定して定義
		GrabPass {"_MosaicGrabTex"}

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

			//変数宣言		

			sampler2D _MosaicGrabTex;

			float _Width;
			float _Height;
			float _BlockSize;

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 pos : POSITION;

				// 法線情報を取得
				half3 normal : NORMAL;

				// テクスチャ座標を取得
				float2 uv : TEXCOORD0;

				//頂点カラー
				float4 vertColor : COLOR;
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

				//頂点カラー
				float4 vertColor : COLOR;
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

				// Grab用テクスチャ座標
				re.GrabPos = ComputeGrabScreenPos(re.pos);

				//頂点カラー
				re.vertColor = v.vertColor;
				
				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力用変数を宣言
				fixed4 re;

				i.GrabPos.x = floor((i.GrabPos.x * _ScreenParams.x) / _BlockSize) * _BlockSize / _ScreenParams.x;
				i.GrabPos.y = floor((i.GrabPos.y * _ScreenParams.y) / _BlockSize) * _BlockSize / _ScreenParams.y;

				//プロジェクションで_Grabを貼り、Trail用のテクスチャのアルファを元に座標をずらす
				re = tex2Dproj(_MosaicGrabTex, i.GrabPos);

				//パーティクルシステムで設定した頂点カラーの透明度を適応
				re.a = i.vertColor.a;

				//出力
				return re;
			}

			//プログラム終了
			ENDCG

		}
    }
}

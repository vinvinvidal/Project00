// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MiniMapShader"
{
	Properties
	{
		//ミニマップの色
		_MiniMapColor("_MiniMapColor", Color) = (0, 0, 0, 1)

		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("_ZTest", Float) = 8
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType"="Transparent"
		}

		//アルファブレンド
		Blend SrcAlpha OneMinusSrcAlpha 

		//ライティングしない
		Lighting Off

		//Zテスト
		ZTest[_ZTest]

		//両面表示
		//Cull off

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

			//変数宣言
			fixed4 _MiniMapColor;				//ミニマップの色

			float _PlayerCharacterPos;			//プレイヤーキャラクターのY位置


			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標を取得
				float4 vertex : POSITION;

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

				//ワールド座標
				float3 worldPos : TEXCOORD1;
			};

			//頂点シェーダ
			vertex_output vert(vertex_input v)
			{
				//return用output構造体宣言
				vertex_output re;

				//UVを格納
				re.uv = v.uv;

				//頂点座標を格納 UnityObjectToClipPos()に頂点座標を渡すと画面上ピクセル座標を返してくる
				re.pos = UnityObjectToClipPos(v.vertex);

				//法線をワールド座標系に変換
				re.normal = UnityObjectToWorldNormal(v.normal);

				//描画ピクセルのワールド座標
                re.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力用変数宣言、色を反映
				fixed4 re = _MiniMapColor;

				//プレイヤーキャラクターより高い位置にあるピクセルを透明化
				re.a = normalize((_PlayerCharacterPos - i.worldPos.y) + 1);

				//プレイヤーキャラクターより低い位置にあるピクセルを透明化
				re.a *= -((_PlayerCharacterPos - 10 - i.worldPos.y) * 0.1f);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}
	}
}

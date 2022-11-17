Shader "Custom/StageShader_Desk&Chair"
{
	Properties
	{
		//表面テクスチャ
		_TexSurface("_TexSurface", 2D) = "white" {}

		//法線テクスチャ
		_TexNormal("_TexNormal", 2D) = "white" {}

		//消滅用係数
		_VanishNum("_VanishNum",float) = 0

		//消滅用テクスチャ
		_VanishTex("_VanishTex", 2D) = "white" {}						
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest" 
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			//両面表示
			//Cull off

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
			sampler2D _TexSurface;				//表面テクスチャ
			sampler2D _TexNormal;				//法線テクスチャ

			float4 _TexSurface_ST;				//表面テクスチャのタイリングとオフセット
			float4 _TexNormal_ST;				//法線テクスチャのタイリングとオフセット

			fixed4 _LightColor0;				//ライトカラー

			float _VanishNum;					//消滅用係数

			sampler2D _VanishTex;				//消滅用テクスチャ

			float4 _VanishTex_ST;				//消滅用テクスチャスケールタイリング

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

			/*
            //ジオメトリシェーダーからフラグメントシェーダーに渡すデータ
			struct g2f
			{
				float4 pos : SV_POSITION;

				float2 uv : TEXCOORD0;
			};
			*/

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

				//ドロップシャドウ
				SHADOW_COORDS(2)
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
				re.GrabPos = ComputeScreenPos(re.pos);

				//ドロップシャドウ
				TRANSFER_SHADOW(re);

				//出力　
				return re;
			}

			//フラグメントシェーダ
			fixed4 frag(vertex_output i) : SV_Target
			{
				//出力用変数宣言、表面テクスチャを貼る、タイリング適応
				fixed4 re = tex2D(_TexSurface, i.uv * _TexSurface_ST.xy);

				//光源と法線ハーフランパート乗算
				re *= (dot(i.normal + UnpackNormal(tex2D(_TexNormal, i.uv * _TexNormal_ST.xy)), _WorldSpaceLightPos0) + 1) * 0.5;

				//オブジェクトからのドロップシャドウ乗算
				re *= saturate(SHADOW_ATTENUATION(i) + 0.5);

				//ライトカラーをブレンド
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//消失用テクスチャのタイリング設定
				i.GrabPos.xy *= _VanishTex_ST.xy;

				//テクスチャと変数から透明度を算出
				re.a -= (tex2Dproj(_VanishTex, i.GrabPos).a * _VanishNum * 10);

				//透明部分をクリップ
				clip(re.a - 0.01);

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}

		//共用サブライト
		UsePass "Unlit/PublicSubLight/PublicSubLight"

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
	}
}

/*
{
	Properties
	{
		//表面テクスチャ
		_TexSurface("_TexSurface", 2D) = "white" {}

		//法線テクスチャ
		_TexNormal("_TexNormal", 2D) = "white" {}

		//消滅用係数
		_VanishNum("_VanishNum",float) = 0									
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest" 
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			//両面表示
			Cull off

			//プログラム開始
			CGPROGRAM

			//インクルードファイル
			#include "UnityCG.cginc"									//一般的に使用されるヘルパー関数
			#include "AutoLight.cginc"									//ライティングやシャドウ機能
			#include "Assets/Global/ShaderFunction.cginc"				//自前で作った関数

			#pragma target 5.0					//Shaderモデル指定、テッセレーションなどを使いたい場合は5.0
			#pragma vertex vert					//各頂点について実行される頂点シェーダ、フラグメントシェーダに出力される
			#pragma geometry geom
			#pragma fragment frag				//各ピクセル毎に実行されるフラグメントシェーダ
			#pragma multi_compile_fwdbase		//マルチコンパイル、ドロップシャドウを受けたい場合など

			//変数宣言
			sampler2D _TexSurface;				//表面テクスチャ
			sampler2D _TexNormal;				//法線テクスチャ

			float4 _TexSurface_ST;				//表面テクスチャのタイリングとオフセット
			float4 _TexNormal_ST;				//法線テクスチャのタイリングとオフセット

			fixed4 _LightColor0;				//ライトカラー

			float _VanishNum;					//消滅用係数

			//オブジェクトから頂点シェーダーに情報を渡す構造体を宣言
			struct vertex_input
			{
				// 頂点座標
				float4 pos : POSITION;

				// 法線情報
				half3 normal : NORMAL;

				// テクスチャ座標
				float2 uv : TEXCOORD0;
			};

            //ジオメトリシェーダーからフラグメントシェーダーに渡すデータ
			struct g2f
			{
				float4 pos : SV_POSITION;

				half3 normal: NORMAL;

				float2 uv : TEXCOORD0;

				//ドロップシャドウ
				SHADOW_COORDS(1)
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

				//ドロップシャドウ
				SHADOW_COORDS(2)
			};

			//頂点シェーダ
			vertex_input vert(vertex_input v)
			{
				//出力　
				return v;
			}

			// ジオメトリシェーダー
			[maxvertexcount(3)]
			void geom (triangle vertex_input input[3], inout TriangleStream<g2f> stream)
			{
				// 法線ベクトルを計算

				float3 vec1 = input[1].pos - input[0].pos;

				float3 vec2 = input[2].pos - input[0].pos;

				float3 normal = normalize(cross(vec1, vec2));

				float3 center = (input[0].pos + input[1].pos + input[2].pos) / 3;
				
				float Randnum = Random(center.xy, 1);				

				[unroll]
				for(int i = 0; i < 3; i++)
				{				
					g2f o;

					vertex_input v = input[i];

					//v.pos.xyz = rotate(v.pos.xyz - center, Randnum * _VanishNum);

					v.pos.xyz += normal * Randnum * _VanishNum;

					o.pos = UnityObjectToClipPos(v.pos);

					o.normal = v.normal;

					o.uv = v.uv;

					//ドロップシャドウ
					TRANSFER_SHADOW(o);

					stream.Append(o);
				}

				stream.RestartStrip();
			}

			//フラグメントシェーダ
			fixed4 frag(g2f i) : SV_Target
			{
				//出力用変数宣言、表面テクスチャを貼る、タイリング適応
				fixed4 re = tex2D(_TexSurface, i.uv * _TexSurface_ST.xy);

				//光源と法線ハーフランパート乗算
				re *= (dot(i.normal + UnpackNormal(tex2D(_TexNormal, i.uv * _TexNormal_ST.xy)), _WorldSpaceLightPos0) + 1) * 0.5;

				//オブジェクトからのドロップシャドウ乗算
				re *= saturate(SHADOW_ATTENUATION(i) + 0.5);

				//ライトカラーをブレンド
				re *= lerp(1, _LightColor0, _LightColor0.a);

				//透明部分をクリップ、消滅用の乱数精製
				clip(re.a - 0.01 - ((Random(i.uv * _VanishNum, round(_VanishNum)) + 0.05) * _VanishNum));

				//出力
				return re;
			}

			//プログラム終了
			ENDCG
		}

		//共用サブライト
		UsePass "Unlit/PublicSubLight/PublicSubLight"

		//共用シャドウキャスター
		UsePass "Unlit/PublicShadowCaster/PublicShadowCaster"
	}
}
*/
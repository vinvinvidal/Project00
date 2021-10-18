using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface CharacterEyeShaderScriptInterface : IEventSystemHandler
{
	//視線を向ける先の座標を受け取る関数
	void GetLookPos(Vector3 vec);

	//ダイレクトモードを設定する関数
	void SetDirectMode(bool b);
}

public class CharacterEyeShaderScript : GlobalClass , CharacterEyeShaderScriptInterface
{
	//変数宣言

	//目テクスチャ
	public Texture2D _EyeTex;

	//目ハイライトテクスチャ
	public Texture2D _EyeHiLight;

	//目影テクスチャ
	public Texture2D _EyeShadow;

	//目の影色
	public Color _EyeShadowColor;

	//頭の向き
	Transform HeadAngle;

	//ディレクショナルライト
	GameObject DrcLight;

	//眼マテリアル
	Material EyeMaterial;

	//目線を向ける場所へのベクトル
	Vector3 EyeVec;

	//目マテリアルに渡すオフセット値
	Vector2 EyeOffset;

	//頭と光源の内積各要素
	float HeadDotLigntX;             
	float HeadDotLigntY;             
	float HeadDotLigntZ;

	//頭と目線先の内積各要素
	float HeadDotEyePosX;
	float HeadDotEyePosY;
	float HeadDotEyePosZ;

	//ダイレクトモードフラグ
	public bool DirectFlag { get; set; } = false;

	//ダイレクトモードに使うタイリング値
	public Vector2 DirectEyeTiling { get; set; } = new Vector2(1, 1);

	//ダイレクトモードに使うオフセット値
	public Vector2 DirectEyeOffset { get; set; } = new Vector2(0, 0);

	//視線を滑らかに変更するベロシティ
	public float DirectEyeVelocity { get; set; } = 0;

	void Start()
    {
		//マテリアル取得、マルチの場合1番目のマテリアルを取得するので注意
		EyeMaterial = GetComponent<Renderer>().material;

		//マテリアルにテクスチャとカラーをセット
		EyeMaterial.SetTexture("_EyeTex", _EyeTex);
		EyeMaterial.SetTexture("_EyeHiLight", _EyeHiLight);
		EyeMaterial.SetTexture("_EyeShadow", _EyeShadow);
		EyeMaterial.SetColor("_EyeShadowColor", _EyeShadowColor);

		//顔のトランスフォーム取得
		HeadAngle = DeepFind(transform.root.gameObject, "HeadAngle").transform;

		//ディレクショナルライト取得
		DrcLight = GameObject.Find("OutDoorLight");

		//目線を向ける場所初期化
		EyeVec = new Vector3();

		//目マテリアルに渡すオフセット値初期化
		EyeOffset = new Vector2();
	}

	void Update()
    {
		//オブジェクトとライト各軸の内積を取る、Clampをしないと後のAcosでNaNが発生する
		HeadDotLigntX = Mathf.Clamp(Vector3.Dot(HeadAngle.right, DrcLight.transform.forward), -1.0f, 1.0f);
		HeadDotLigntY = Mathf.Clamp(Vector3.Dot(HeadAngle.up, DrcLight.transform.forward), -1.0f, 1.0f);
		HeadDotLigntZ = Mathf.Clamp(Vector3.Dot(HeadAngle.forward, DrcLight.transform.forward), -1.0f, 1.0f);

		//スケベ中はハイライトを小刻みに動かす
		if(GameManagerScript.Instance.H_Flag)
		{
			HeadDotLigntZ += ((Mathf.PerlinNoise(Time.time * 50, -Time.time) - 0.5f) * 2) * 0.25f;
		}

		//ハイライト回転用の値をシェーダーに渡す
		EyeMaterial.SetFloat("Eye_HiLightRotationSin", HeadDotLigntY + HeadDotLigntZ * 0.75f);
		EyeMaterial.SetFloat("Eye_HiLightRotationCos", HeadDotLigntX + HeadDotLigntZ * 0.25f);

		//ダイレクトモード
		if (DirectFlag)
		{
			//視線を動かす
			EyeMaterial.SetTextureScale("_EyeTex", new Vector2(Mathf.Lerp(EyeMaterial.GetTextureScale("_EyeTex").x, DirectEyeTiling.x, DirectEyeVelocity) , Mathf.Lerp(EyeMaterial.GetTextureScale("_EyeTex").y, DirectEyeTiling.y, DirectEyeVelocity)));
			EyeMaterial.SetTextureOffset("_EyeTex", new Vector2(Mathf.Lerp(EyeMaterial.GetTextureOffset("_EyeTex").x, DirectEyeOffset.x, DirectEyeVelocity), Mathf.Lerp(EyeMaterial.GetTextureOffset("_EyeTex").y, DirectEyeOffset.y, DirectEyeVelocity)));

			//ベロシティカウントアップ
			DirectEyeVelocity += Time.deltaTime * 0.75f;

			//1になったら止める
			if (DirectEyeVelocity >= 1)
			{
				DirectEyeVelocity = 1;
			}
		}
		else
		{
			//顔の方向と注視点からのベクトルの内積を求める
			EyeOffset.x = Vector3.Dot(HeadAngle.right, EyeVec.normalized) * -0.15f;
			EyeOffset.y = Vector3.Dot(HeadAngle.up, EyeVec.normalized) * 0.12f;

			//視線を動かす
			EyeMaterial.SetTextureOffset("_EyeTex", EyeOffset);
		}
	}

	//視線を向ける先の座標を受け取る関数、メッセージシステムから呼ばれる
	public void GetLookPos(Vector3 vec)
	{
		//受け取ったポジションから頭までのベクトルを変数に代入
		EyeVec = HeadAngle.transform.position - vec;		
	}

	//ダイレクトモードを設定する関数
	public void SetDirectMode(bool b)
	{
		//フラグを反映
		DirectFlag = b;

		//ダイレクトモード解除なら目の大きさをリセットする
		if(!DirectFlag)
		{
			EyeMaterial.SetTextureScale("_EyeTex", new Vector2(1, 1));
		}
	}
}

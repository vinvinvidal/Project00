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
	void SetLookPos(Vector3 vec);

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

	//目の高さになるオブジェクト
	GameObject EyePosOBJ;

	//ディレクショナルライト
	GameObject DrcLight;

	//メインカメラ
	GameObject MainCamera;

	//眼マテリアル
	Material EyeMaterial;

	//目線を向ける場所へのベクトル
	Vector3 EyeVec;

	//目マテリアルに渡すオフセット値
	Vector2 EyeOffset;

	//頭と光源の内積各要素
	float HeadDotLigntX;             
	float HeadDotLigntY;

	//頭と目線先の内積各要素
	float HeadDotEyePosX;
	float HeadDotEyePosY;
	float HeadDotEyePosZ;

	//ハイライトの回転角度
	float HilightAngle;

	//ダイレクトモードフラグ
	public bool DirectFlag { get; set; } = false;

	//カメラ目線フラグ
	public bool CameraEyeFlag { get; set; } = false;

	//ダイレクトモードに使うタイリング値
	public Vector2 DirectEyeTiling { get; set; } = new Vector2(1, 1);

	//ダイレクトモードに使うオフセット値
	public Vector2 DirectEyeOffset { get; set; } = new Vector2(0, 0);

	//視線を滑らかに変更するベロシティ
	public float DirectEyeVelocity { get; set; } = 0;

	//敵が背面にいる時に視線が左右に細かく切り替わるのを防ぐ変数
	private float BackEnemyEyeTime = 0f;

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

		//目の位置になるトランスフォーム取得
		EyePosOBJ = DeepFind(transform.root.gameObject, "R_EyeCorner");

		//ディレクショナルライト取得
		DrcLight = GameObject.Find("OutDoorLight");

		//メインカメラ取得
		MainCamera = GameManagerScript.Instance.GetMainCameraOBJ();

		//目線を向ける場所初期化
		EyeVec = new Vector3();

		//目マテリアルに渡すオフセット値初期化
		EyeOffset = new Vector2();
	}

	void Update()
    {
		//頭とディレクショナルライトの内積を取る
		HeadDotLigntX = Vector3.Dot(HeadAngle.right, DrcLight.transform.forward);
		HeadDotLigntY = Vector3.Dot(HeadAngle.up, DrcLight.transform.forward);

		//内積を２次元座標として角度を取る、オイラーをラジアンにして上下の正負を反映
		HilightAngle = Vector2.Angle(Vector2.right , new Vector2(HeadDotLigntX, HeadDotLigntY)) * Mathf.Deg2Rad * Mathf.Sign(HeadDotLigntY);
		
		//ハイライト回転用の値をシェーダーに渡す
		EyeMaterial.SetFloat("Eye_HiLightRotationSin", Mathf.Sin(HilightAngle));
		EyeMaterial.SetFloat("Eye_HiLightRotationCos", Mathf.Cos(HilightAngle));

		//カメラ目線
		if (CameraEyeFlag)
		{
			//目の位置からカメラ位置までのベクトルを直接代入
			EyeVec = new Vector3(HeadAngle.transform.position.x, EyePosOBJ.transform.position.y, HeadAngle.transform.position.z) - MainCamera.transform.position;
		}

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
			//正面
			if(Vector3.Dot(HeadAngle.forward, EyeVec.normalized) < 0)
			{
				//顔の方向と注視点からのベクトルの内積を求める
				EyeOffset.x = Vector3.Dot(HeadAngle.right, EyeVec.normalized) * -0.2f;				
			}
			//背面、視線を変えてから一定時間経過している
			else if(Time.time > BackEnemyEyeTime + 1)
			{
				//敵が背面に居る時は左右どちらかに振る
				EyeOffset.x = Mathf.Sign(Vector3.Dot(HeadAngle.right, EyeVec.normalized)) * -0.2f;

				//視線を変えた時間を記録
				BackEnemyEyeTime = Time.time;
			}

			//上下移動
			EyeOffset.y = Vector3.Dot(HeadAngle.up, EyeVec.normalized) * 0.125f;

			//視線を動かす
			EyeMaterial.SetTextureOffset("_EyeTex", EyeOffset);
		}
	}

	//視線を向ける先の座標を受け取る関数、メッセージシステムから呼ばれる
	public void SetLookPos(Vector3 vec)
	{
		if(!CameraEyeFlag && !DirectFlag)
		{
			//受け取ったポジションから頭までのベクトルを変数に代入
			EyeVec = HeadAngle.transform.position - vec;
		}
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

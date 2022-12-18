using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface CharacterFaceShaderScriptInterface : IEventSystemHandler
{
	//ディレクショナルライトを切り替える、演出時に使う
	void ChangeLight(Transform t);
}

public class CharacterFaceShaderScript : GlobalClass, CharacterFaceShaderScriptInterface
{
	//顔マテリアル
	private Material FaceMaterial;

	//肌色
	public Color _SkinColor;

	//顔ベーステクスチャ
	public Texture2D _TexFaceBase;

	//赤面テクスチャ
	public Texture2D _TexFaceBlush;

	//顔ハイライトのテクスチャ
	public Texture2D _TexFaceHiLight;

	//ハイライトのmatcapテクスチャ
	public Texture2D _TexFaceHiLightMatCap;

	//前方光源からの影テクスチャ
	public Texture2D _TexFaceShadowFront;
	public Texture2D _TexFaceShadowFrontTop;
	public Texture2D _TexFaceShadowFrontLeft;
	public Texture2D _TexFaceShadowFrontRight;
	public Texture2D _TexFaceShadowFrontBottom;

	//後方光源からの影テクスチャ
	public Texture2D _TexFaceShadowBack;
	public Texture2D _TexFaceShadowBackTop;
	public Texture2D _TexFaceShadowBackLeft;
	public Texture2D _TexFaceShadowBackRight;
	public Texture2D _TexFaceShadowBackBottom;

	//左方光源からの影テクスチャ
	public Texture2D _TexFaceShadowLeft;
	public Texture2D _TexFaceShadowLeftTop;
	public Texture2D _TexFaceShadowLeftBottom;

	//右方光源からの影テクスチャ
	public Texture2D _TexFaceShadowRight;
	public Texture2D _TexFaceShadowRightTop;
	public Texture2D _TexFaceShadowRightBottom;

	//上方光源からの影テクスチャ
	public Texture2D _TexFaceShadowTop;

	//下方光源からの影テクスチャ
	public Texture2D _TexFaceShadowBottom;

	//顔アングルのトランスフォーム
	private Transform FaceTransform;

	//ディレクショナルライトのトランスフォーム
	private Transform LightTransform;

	//使用テクスチャ決定用連想配列
	Dictionary<Vector3, Action> TextureSetDic = new Dictionary<Vector3, Action>();

	private float ObjDotLigntX;             //オブジェクトとライトのX軸内積
	private float ObjDotLigntY;             //オブジェクトとライトのY軸内積
	private float ObjDotLigntZ;             //オブジェクトとライトのZ軸内積

	private Vector3 LightAngle;             //ライトがある方角
	private Vector3 LightAngleCache;        //ライトがある方角のキャッシュ

	private float MaxBlendRatio;            //各軸の内積の合計、ブレンド比率の最大値

	private Vector4 EachBlendRatio;         //xyzに各軸のブレンド比率を格納する

	private float TextureBlendXX;           //X軸方向の中間テクスチャ用ブレンド比率
	private float TextureBlendYY;           //Y軸方向の中間テクスチャ用ブレンド比率
	private float TextureBlendZZ;           //Z軸方向の中間テクスチャ用ブレンド比率

	private float TextureBlendXZ;           //X軸方向の中間テクスチャ用ブレンド比率
	private float TextureBlendXY;           //X軸方向の中間テクスチャ用ブレンド比率
	private float TextureBlendYZ;           //Y軸方向の中間テクスチャ用ブレンド比率

	void Start()
    {
		//マテリアル取得
		FaceMaterial = transform.GetComponent<Renderer>().material;

		//肌色をセット
		FaceMaterial.SetColor("_SkinColor", _SkinColor);

		//統合用テクスチャ
		Texture2D _TexFaceAtlas = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

		//使用するテクスチャを配列に入れる
		Texture2D[] Textures = { _TexFaceBase, _TexFaceBlush, _TexFaceHiLight, _TexFaceHiLightMatCap };

		//テクスチャ統合、Rectを受け取る
		Rect[] TexRect = _TexFaceAtlas.PackTextures(Textures, 0, 1024, true);

		//マテリアルに統合テクスチャを渡す
		FaceMaterial.SetTexture("_TexFaceAtlas", _TexFaceAtlas);

		//ベーステクスチャのRectを渡す
		FaceMaterial.SetVector("_TexFaceBaseRectPos", TexRect[0].position);
		FaceMaterial.SetVector("_TexFaceBaseRectSize", TexRect[0].size);

		//赤面テクスチャのRectを渡す
		FaceMaterial.SetVector("_TexFaceBlushRectPos", TexRect[1].position);
		FaceMaterial.SetVector("_TexFaceBlushRectSize", TexRect[1].size);

		//ハイライトテクスチャのRectを渡す
		FaceMaterial.SetVector("_TexFaceHiLightRectPos", TexRect[2].position);
		FaceMaterial.SetVector("_TexFaceHiLightRectSize", TexRect[2].size);

		//マットキャップテクスチャのRectを渡す
		FaceMaterial.SetVector("_TexFaceMatCapRectPos", TexRect[3].position);
		FaceMaterial.SetVector("_TexFaceMatCapRectSize", TexRect[3].size);

		/*
		//各テクスチャをシェーダーにセット
		FaceMaterial.SetTexture("_TexFaceBase", _TexFaceBase);
		FaceMaterial.SetTexture("_TexFaceBlush", _TexFaceBlush);
		FaceMaterial.SetTexture("_TexFaceHiLight", _TexFaceHiLight);
		FaceMaterial.SetTexture("_TexFaceHiLightMatCap", _TexFaceHiLightMatCap);
		*/

		//顔アングルのトランスフォーム取得
		FaceTransform = DeepFind(transform.root.gameObject, "HeadAngle").transform;

		//ディレクショナルライトのトランスフォーム
		LightTransform = GameObject.Find("OutDoorLight").transform;

		//テクスチャ決定用連想配列に要素追加、光源の位置によって呼び出す関数を切り替える
		TextureSetDic.Add(new Vector3(1, -1, -1), TextureSetLeftFrontTop);
		TextureSetDic.Add(new Vector3(-1, -1, -1), TextureSetRightFrontTop);
		TextureSetDic.Add(new Vector3(1, -1, 1), TextureSetLeftFrontBottom);
		TextureSetDic.Add(new Vector3(-1, -1, 1), TextureSetRightFrontBottom);
		TextureSetDic.Add(new Vector3(1, 1, 1), TextureSetLeftBackBottom);
		TextureSetDic.Add(new Vector3(-1, 1, 1), TextureSetRightBackBottom);
		TextureSetDic.Add(new Vector3(1, 1, -1), TextureSetLeftBackTop);
		TextureSetDic.Add(new Vector3(-1, 1, -1), TextureSetRightBackTop);

		//消失用テクスチャを渡す
		FaceMaterial.SetTexture("_VanishTex", GameManagerScript.Instance.VanishTextureList[0]);		

		//スクリーンサイズから消失用テクスチャのスケーリングを設定
		FaceMaterial.SetTextureScale("_VanishTex", new Vector2(Screen.width / FaceMaterial.GetTexture("_VanishTex").width, Screen.height / FaceMaterial.GetTexture("_VanishTex").height) * GameManagerScript.Instance.ScreenResolutionScale);

	}

	void Update()
    {
		//オブジェクトとライト各軸の内積を取る、Clampをしないと後のAcosでNaNが発生する
		ObjDotLigntX = Mathf.Clamp(Vector3.Dot(FaceTransform.right, LightTransform.forward), -1.0f, 1.0f);
		ObjDotLigntY = Mathf.Clamp(Vector3.Dot(FaceTransform.up, LightTransform.forward), -1.0f, 1.0f);
		ObjDotLigntZ = Mathf.Clamp(Vector3.Dot(FaceTransform.forward, LightTransform.forward), -1.0f, 1.0f);

		//位置関係判定のため内積の正負を求める
		LightAngle.x = Mathf.Sign(ObjDotLigntX);
		LightAngle.z = Mathf.Sign(ObjDotLigntY);
		LightAngle.y = Mathf.Sign(ObjDotLigntZ);

		//各軸の内積を合計してブレンド比率の最大値を求める
		MaxBlendRatio = Mathf.Abs(ObjDotLigntX) + Mathf.Abs(ObjDotLigntY) + Mathf.Abs(ObjDotLigntZ);

		//各軸のブレンド比を求める
		EachBlendRatio.x = Mathf.Round(Mathf.InverseLerp(0, MaxBlendRatio, Mathf.Abs(ObjDotLigntX)) * 100) * 0.01f;
		EachBlendRatio.y = Mathf.Round(Mathf.InverseLerp(0, MaxBlendRatio, Mathf.Abs(ObjDotLigntY)) * 100) * 0.01f;
		EachBlendRatio.z = Mathf.Round(Mathf.InverseLerp(0, MaxBlendRatio, Mathf.Abs(ObjDotLigntZ)) * 100) * 0.01f;
		EachBlendRatio.w = MaxBlendRatio;

		//軸直交テクスチャブレンド比率を求める
		TextureBlendXX = Mathf.InverseLerp(0.5f, 1.0f, EachBlendRatio.x);
		TextureBlendYY = Mathf.InverseLerp(0.5f, 1.0f, EachBlendRatio.y);
		TextureBlendZZ = Mathf.InverseLerp(0.5f, 1.0f, EachBlendRatio.z);

		//中間テクスチャ用ブレンド比率を求める
		TextureBlendXZ = Mathf.InverseLerp(0.0f, 0.25f, (EachBlendRatio.x * EachBlendRatio.z));
		TextureBlendYZ = Mathf.InverseLerp(0.0f, 0.25f, (EachBlendRatio.y * EachBlendRatio.z));
		TextureBlendXY = Mathf.InverseLerp(0.0f, 0.25f, (EachBlendRatio.x * EachBlendRatio.y));

		//各テクスチャのブレンド比率をシェーダーに渡す
		FaceMaterial.SetFloat("TextureBlendXX", TextureBlendXX);
		FaceMaterial.SetFloat("TextureBlendYY", TextureBlendYY);
		FaceMaterial.SetFloat("TextureBlendZZ", TextureBlendZZ);
		FaceMaterial.SetFloat("TextureBlendXZ", TextureBlendXZ);
		FaceMaterial.SetFloat("TextureBlendYZ", TextureBlendYZ);
		FaceMaterial.SetFloat("TextureBlendXY", TextureBlendXY);

		//キャッシュと比較してアングルが変わっていたら処理
		if (LightAngle != LightAngleCache)
		{
			//テクスチャ決定用連想配列に光源位置を入れて関数呼び出し、使用テクスチャをシェーダーに渡す
			TextureSetDic[LightAngle]();

			//アングルをキャッシュ
			LightAngleCache = LightAngle;
		}
	}

	//テクスチャ決定用連想配列から呼び出される関数、光源の位置によって使用テクスチャをシェーダーに渡す
	void TextureSetLeftFrontTop()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowLeft);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowTop);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowFront);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowLeftTop);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowFrontLeft);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowFrontTop);
	}

	void TextureSetRightFrontTop()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowRight);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowTop);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowFront);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowRightTop);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowFrontRight);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowFrontTop);
	}

	void TextureSetLeftFrontBottom()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowLeft);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowBottom);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowFront);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowLeftBottom);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowFrontLeft);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowFrontBottom);
	}

	void TextureSetRightFrontBottom()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowRight);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowBottom);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowFront);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowRightBottom);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowFrontRight);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowFrontBottom);
	}

	void TextureSetLeftBackBottom()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowLeft);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowBottom);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowBack);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowLeftBottom);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowBackLeft);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowBackBottom);
	}

	void TextureSetRightBackBottom()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowRight);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowBottom);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowBack);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowRightBottom);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowBackRight);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowBackBottom);
	}

	void TextureSetLeftBackTop()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowLeft);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowTop);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowBack);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowLeftTop);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowBackLeft);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowBackTop);
	}

	void TextureSetRightBackTop()
	{
		FaceMaterial.SetTexture("texXX", _TexFaceShadowRight);

		FaceMaterial.SetTexture("texYY", _TexFaceShadowTop);

		FaceMaterial.SetTexture("texZZ", _TexFaceShadowBack);

		FaceMaterial.SetTexture("texXY", _TexFaceShadowRightTop);

		FaceMaterial.SetTexture("texXZ", _TexFaceShadowBackRight);

		FaceMaterial.SetTexture("texYZ", _TexFaceShadowBackTop);
	}

	//ディレクショナルライトを切り替える、演出時に使う
	public void ChangeLight(Transform t)
	{
		LightTransform = t;
	}
}

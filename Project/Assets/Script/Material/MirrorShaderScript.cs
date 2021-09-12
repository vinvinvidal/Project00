using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public interface MirrorShaderScriptInterface : IEventSystemHandler
{
	void MirrorSwitch(bool b);
}

public class MirrorShaderScript : GlobalClass , MirrorShaderScriptInterface
{
	//変数宣言

	//メインカメラ
	private GameObject MainCamera;

	//ミラーカメラ
	private Camera MirrorCamera;

	//メインカメラから鏡までのベクトル
	private Vector3 LookAtVec;

	//鏡からの反射ベクトル
	private Vector3 ReflectVec;

	//鏡とカメラの距離
	private float MirrorDistance;

	//ミラーを持っているオブジェクト、インスペクタ
	public GameObject MirrorOBJ;

	//ミラーの正面を向いているオブジェクト
	private GameObject MirrorForwardOBJ;

	//ミラーを持っているオブジェクトのサイズ
	private float MirrorSize;

	//ミラー用レンダーテクスチャ
	private RenderTexture MirrorTexture;

	//ミラー有効bool、インスペクタ
	public bool OnMirror;

	void Start()
    {
		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//ミラーカメラ取得
		MirrorCamera = DeepFind(transform.gameObject, "MirrorCamera").GetComponent<Camera>();
		
		//ミラーの正面を向いているオブジェクト取得
		MirrorForwardOBJ = DeepFind(transform.gameObject, "MirrorForward");

		//ミラー用レンダーテクスチャを初期化
		MirrorTexture = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);

		//カメラのターゲットテクスチャにレンダーテクスチャをセット
		MirrorCamera.targetTexture = MirrorTexture;

		//ミラーの大きさを取得
		MirrorSize = MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.x;

		//一番大きい値を探すための配列
		float[] tempsizeArray = new float[] { MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.x, MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.y, MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.z };

		//Linqで最大値を抽出
		MirrorSize = tempsizeArray.Max();

		//インスペクタのスイッチで初期動作
		MirrorSwitch(OnMirror);
	}

    void Update()
    {
		//ミラーカメラオン
		if (OnMirror)
		{
			//レンダーテクスチャをシェーダーに送る
			MirrorOBJ.GetComponent<Renderer>().material.SetTexture("_MainTex", MirrorTexture);

			//カメラから鏡までのベクトル取得
			LookAtVec = MirrorForwardOBJ.transform.position - MainCamera.transform.position;

			//鏡の反射ベクトル取得
			ReflectVec = LookAtVec + 2 * Vector3.Dot(-LookAtVec, MirrorForwardOBJ.transform.forward) * MirrorForwardOBJ.transform.forward;

			//カメラを撮影ポジションに移動
			MirrorCamera.transform.position = MirrorForwardOBJ.transform.position - ReflectVec;

			//カメラを撮影方向に向ける
			MirrorCamera.transform.LookAt(MirrorForwardOBJ.transform.position);

			//カメラと鏡の距離を測る
			MirrorDistance = Vector3.Distance(MirrorCamera.transform.position, MirrorForwardOBJ.transform.position);

			//nearを設定
			MirrorCamera.nearClipPlane = MirrorDistance * 1.1f;

			//画角を調整
			MirrorCamera.fieldOfView = 2 * Mathf.Atan(MirrorSize / (2 * MirrorDistance)) * Mathf.Rad2Deg;
		}
    }

	//インターフェイス、外部からフラグを切り替える
	public void MirrorSwitch(bool b)
	{
		//受け取ったフラグを反映
		OnMirror = b;

		//オンになった場合
		if(b)
		{
			//シェーダーにフラグを送って処理実行
			MirrorOBJ.GetComponent<Renderer>().material.SetInt("MirrorON", 1);

			//カメラを点ける
			MirrorCamera.enabled = true;
		}
		else
		{
			//シェーダーにフラグを送って処理を止める
			MirrorOBJ.GetComponent<Renderer>().material.SetInt("MirrorON", 0);

			//カメラを切る
			MirrorCamera.enabled = false;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public interface MirrorShaderScriptInterface : IEventSystemHandler
{
	//ミラーの有効無効を切り替える
	void MirrorSwitch(bool b);

	//狙ったオブジェクトを映すようにムリヤリカメラ位置を移動させる
	void EnemyFaceMirror(GameObject obj);
}

public class MirrorShaderScript : GlobalClass, MirrorShaderScriptInterface
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

	//敵顔ミラーフラグ
	private bool EnemyFaceMirrorFlag = false;

	//敵顔ミラーオブジェクト
	private GameObject EnemyFaceMirrorOBJ;

	//ミラーマテリアル
	private Material MirrorMaterial;

	void Start()
	{
		//メインカメラ取得
		MainCamera = GameManagerScript.Instance.GetMainCameraOBJ();

		//ミラーカメラ取得
		MirrorCamera = DeepFind(transform.gameObject, "MirrorCamera").GetComponent<Camera>();

		//ミラーの正面を向いているオブジェクト取得
		MirrorForwardOBJ = DeepFind(transform.gameObject, "MirrorForward");

		//ミラー用レンダーテクスチャを初期化
		MirrorTexture = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGB32);

		//カメラのターゲットテクスチャにレンダーテクスチャをセット
		MirrorCamera.targetTexture = MirrorTexture;

		//ミラーの大きさを取得
		MirrorSize = MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.x;

		//マテリアル取得
		MirrorMaterial = MirrorOBJ.GetComponent<Renderer>().material;

		//一番大きい値を探すための配列
		float[] tempsizeArray = new float[] { MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.x, MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.y, MirrorOBJ.GetComponent<MeshFilter>().mesh.bounds.size.z };

		//Linqで最大値を抽出
		MirrorSize = tempsizeArray.Max();

		//インスペクタのスイッチで初期動作
		MirrorSwitch(OnMirror);

		//消失用テクスチャがあるか判別
		if(MirrorMaterial.GetTexturePropertyNames().Any(a => a == "_VanishTex"))
		{
			//テクスチャをセット
			MirrorMaterial.SetTexture("_VanishTex", GameManagerScript.Instance.VanishTextureList[0]);

			//スクリーンサイズから消失用テクスチャのスケーリングを設定
			MirrorMaterial.SetTextureScale("_VanishTex", new Vector2(Screen.width / MirrorMaterial.GetTexture("_VanishTex").width, Screen.height / MirrorMaterial.GetTexture("_VanishTex").height) * GameManagerScript.Instance.ScreenResolutionScale);
		}

		//レンダーテクスチャをシェーダーに送る
		MirrorMaterial.SetTexture("_MainTex", MirrorTexture);
	}

	void Update()
	{
		//カメラと鏡が向き合っていたら鏡を有効化
		if(MirrorCamera.enabled != Vector3.Dot(MainCamera.transform.forward, MirrorForwardOBJ.transform.forward) < 0)
		{
			MirrorCamera.enabled = Vector3.Dot(MainCamera.transform.forward, MirrorForwardOBJ.transform.forward) < 0;
		}

		//ミラーカメラオン
		if (OnMirror && !EnemyFaceMirrorFlag && MirrorCamera.enabled)
		{
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

			//オブジェクトをカメラに向ける
			MirrorOBJ.transform.LookAt(MainCamera.transform);		
		}
		//敵顔ミラーオン
		else if(OnMirror && EnemyFaceMirrorFlag)
		{
			//カメラを敵の顔の前に移動
			MirrorCamera.transform.position = EnemyFaceMirrorOBJ.transform.position + (EnemyFaceMirrorOBJ.transform.up * 0.2f);

			//カメラを注視点に向ける
			MirrorCamera.transform.LookAt(EnemyFaceMirrorOBJ.transform.position);
		}
	}

	//インターフェイス、敵の顔を映す位置にカメラを移動させる
	public void EnemyFaceMirror(GameObject obj)
	{
		//ミラー有効化
		MirrorSwitch(true);

		//敵顔ミラーフラグを立てる
		EnemyFaceMirrorFlag = true;

		//敵顔ミラーオブジェクト代入
		EnemyFaceMirrorOBJ = obj;

		//適当にnearを設定
		MirrorCamera.nearClipPlane = 0.01f;

		//適当に画角を設定
		MirrorCamera.fieldOfView = 60;

		//持続コルーチン呼び出し
		StartCoroutine(ForceShowCoroutine());
	}
	private IEnumerator ForceShowCoroutine()
	{
		//カメラを切るまでループ
		while(OnMirror)
		{
			yield return null;
		}

		//敵顔ミラーフラグを下ろす
		EnemyFaceMirrorFlag = false;
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
			MirrorMaterial.SetInt("MirrorON", 1);

			//カメラを点ける
			MirrorCamera.enabled = true;
		}
		else
		{
			//シェーダーにフラグを送って処理を止める
			MirrorMaterial.SetInt("MirrorON", 0);

			//カメラを切る
			MirrorCamera.enabled = false;
		}
	}
}

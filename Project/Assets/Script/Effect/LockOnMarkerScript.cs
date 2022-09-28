using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LockOnMarkerScript : GlobalClass
{
	//パーティクルシステム
	private ParticleSystem ParticleSys;

	//レンダラー
	private MeshRenderer Rend;

	//メインカメラ
	private GameObject MainCamera;

	//ターゲット
	private GameObject TargetOBJ = null;

	//ロックオン継続フラグ
	private bool LockOnFlag = false;

	private void Start()
	{
		//パーティクルシステム取得
		ParticleSys = GetComponent<ParticleSystem>();

		//レンダラー取得
		Rend = GetComponent<MeshRenderer>();

		//メインカメラ取得
		MainCamera = DeepFind(GameManagerScript.Instance.gameObject, "MainCamera");
	}

	//ロックオンマーカー有効関数
	public void LockOn(GameObject Target)
	{
		//引数で受け取ったオブジェクトをすでにロックしているか判別
		if(TargetOBJ != Target)
		{
			//コルーチン呼び出し
			StartCoroutine(LockOnCoroutine(Target));
		}
	}
	//ターゲットを指定するコルーチン
	private IEnumerator LockOnCoroutine(GameObject Target)
	{
		//ターゲット代入
		TargetOBJ = Target;

		//レンダラー有効化
		Rend.enabled = true;

		//パーティクル停止
		ParticleSys.Stop();

		//フラグを下す
		LockOnFlag = false;

		//１フレーム待機して多重再生を防ぐ
		yield return null;

		//フラグを立てる
		LockOnFlag = true;

		//パーティクル再生
		ParticleSys.Play();

		//フラグが降りるまで待機
		while (LockOnFlag && Target != null)
		{
			//メインカメラに向ける
			gameObject.transform.LookAt(MainCamera.transform);

			//ターゲットの位置に移動
			gameObject.transform.position = Target.transform.position + (Vector3.up * 0.5f);

			//１フレーム待機
			yield return null;
		}
	}

	//ロックオンマーカー無効関数
	public void LockOff()
	{
		//レンダラー無効化
		Rend.enabled = false;

		//フラグを下す
		LockOnFlag = false;

		//ターゲットをnullにする
		TargetOBJ = null;
	}


		/*メッシュ生成とかのサンプルとして取っておく
		//パーティクルシステム
		private ParticleSystem ParticleSys;

		//Shapeモジュールのアクセサ
		private ParticleSystem.ShapeModule ParticleShape;

		//Emissionモジュールのアクセサ
		private ParticleSystem.EmissionModule ParticleEmission;
		private ParticleSystem.Burst ParticleBurst;

		//パーティクルを発生させるメッシュ、リアルタイムで作成する
		private Mesh LockOnMesh;

		//頂点位置List
		private List<Vector3> VerticeList = new List<Vector3>();

		//ロックオン継続フラグ
		private bool LockOnFlag = false;

		private void Start()
		{
			//パーティクルシステム取得
			ParticleSys = GetComponent<ParticleSystem>();

			//パーティクルシステムのアクセサ取得
			ParticleShape = ParticleSys.shape;
			ParticleEmission = ParticleSys.emission;
			ParticleBurst = ParticleEmission.GetBurst(0);

			//メッシュ初期化
			LockOnMesh = new Mesh();

			//とりあえず入れておく
			VerticeList.Add(Vector3.zero);

			//メッシュに頂点追加
			LockOnMesh.SetVertices(VerticeList);

			//発生メッシュを仕込む
			ParticleShape.mesh = LockOnMesh;
		}

		//ロックオンマーカー無効関数
		public void LockOff()
		{
			//パーティクルを停止
			ParticleSys.Stop();

			//ロックオンフラグを下ろす
			LockOnFlag = false;
		}

		//ロックオンマーカー有効関数
		public void LockOn(GameObject Player, GameObject Target)
		{
			//コルーチン呼び出し
			StartCoroutine(LockOnCoroutine(Player, Target));		
		}
		//ターゲットを指定するコルーチン
		private IEnumerator LockOnCoroutine(GameObject Player, GameObject Target)
		{
			//一旦終了処理
			LockOff();

			//1フレーム待機して多重に回るのを防ぐ
			yield return null;

			//パーティクルを再生
			ParticleSys.Play();

			//相手との距離
			float Dist;

			//頂点位置と距離の差
			float Diff;

			//頂点を打つ間隔
			float VirtPos = 0.25f;

			//ロックオンフラグを立てる
			LockOnFlag = true;

			//フラグが降りるまで待機
			while (LockOnFlag && Target != null)
			{
				transform.position = Player.transform.position;

				//ターゲットの方に向ける
				transform.LookAt(Target.transform);

				//距離測定
				Dist = Vector3.Distance(Target.transform.position, Player.transform.position);

				//距離と要素数の差を求める
				Diff = VerticeList.Count - (Dist / VirtPos);

				//距離が離れたら要素追加
				if (Diff < -VirtPos) 
				{
					VerticeList.Add(Vector3.zero);
				}
				//距離が近づいたら要素削除
				else if(Diff > VirtPos && VerticeList.Count > 1)
				{
					VerticeList.RemoveAt(VerticeList.Count - 1);
				}

				//距離に応じて頂点を打つ
				for (int i = 0; i < VerticeList.Count; i++)
				{
					VerticeList[i] = new Vector3(0, 0, i * VirtPos);
				}

				//メッシュ更新
				LockOnMesh.SetVertices(VerticeList);

				ParticleBurst.count = VerticeList.Count + 1;

				ParticleEmission.SetBurst(0, ParticleBurst);

				//1フレーム待機
				yield return null;
			}

			//終了処理
			LockOff();
		}
		*/
	}

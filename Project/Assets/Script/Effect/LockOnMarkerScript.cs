using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LockOnMarkerScript : GlobalClass
{
	//パーティクルシステム
	private ParticleSystem ParticleSys;

	//Shapeモジュールのアクセサ
	private ParticleSystem.ShapeModule ParticleShape;

	//パーティクルを発生させるメッシュ、リアルタイムで作成する
	private Mesh LockOnMesh;

	//頂点位置List
	private List<Vector3> VerticeList = new List<Vector3>();

	//ロックオン継続フラグ
	private bool LockOnFlag = false;

	//ルックアットコンストレイント
	///private LookAtConstraint LookAtConst;

	private void Start()
	{
		//パーティクルシステム取得
		ParticleSys = GetComponent<ParticleSystem>();

		//パーティクルシステムのアクセサ取得
		ParticleShape = ParticleSys.shape;

		//メッシュ初期化
		LockOnMesh = new Mesh();

		//とりあえず原点だけ入れておく
		//VerticeList.Add(Vector3.zero);

		//メッシュに頂点追加
		LockOnMesh.SetVertices(VerticeList);

		//発生メッシュを仕込む
		ParticleShape.mesh = LockOnMesh;
	}

	//ロックオンマーカー解除関数
	public void LockOff()
	{
		//パーティクルを停止
		ParticleSys.Stop();

		//ロックオンフラグを下ろす
		LockOnFlag = false;
	}

	//ターゲットを指定する関数
	public void SetTarget(GameObject Target)
	{
		//コルーチン呼び出し
		StartCoroutine(SetTargetCoroutine(Target));		
	}
	//ターゲットを指定するコルーチン
	private IEnumerator SetTargetCoroutine(GameObject Target)
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
			//ターゲットの方に向ける
			transform.LookAt(Target.transform);

			//距離測定
			Dist = Vector3.Distance(Target.transform.position, gameObject.transform.position);

			//距離と要素数の差を求める
			Diff = VerticeList.Count - (Dist / VirtPos);

			//距離が離れたら要素追加
			if (Diff < -VirtPos) 
			{
				VerticeList.Add(Vector3.zero);
			}
			//距離が近づいたら要素削除
			else if(Diff > VirtPos && VerticeList.Count > 0)
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

			//1フレーム待機
			yield return null;
		}

		//終了処理
		LockOff();
	}
}

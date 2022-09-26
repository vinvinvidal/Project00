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
		//ルックアットコンストレイント取得
		//LookAtConst = gameObject.GetComponent<LookAtConstraint>();

		//パーティクルシステム取得
		ParticleSys = GetComponent<ParticleSystem>();

		//アクセサ取得
		ParticleShape = ParticleSys.shape;

		//メッシュ初期化
		LockOnMesh = new Mesh();

		//とりあえず原点だけ入れておく
		VerticeList.Add(Vector3.zero);

		//メッシュに頂点追加
		LockOnMesh.SetVertices(VerticeList);

		//発生メッシュを仕込む
		ParticleShape.mesh = LockOnMesh;


		/*
		ConstraintSource Source = new ConstraintSource();

		Source.sourceTransform = GameObject.Find("MainCamera").transform;

		Source.weight = 1;
		*/
		//gameObject.GetComponent<LookAtConstraint>().SetSource(0, Source);
	}

	//ターゲットを指定する
	public void SetTarget(GameObject Target)
	{/*
		ConstraintSource Source = new ConstraintSource();

		Source.sourceTransform = Target.transform;

		LookAtConst.SetSource(0, Source);

		LookAtConst.enabled = true;
		*/
		LockOnFlag = true;

		ParticleSys.Play();

		StartCoroutine(SetTargetCoroutine(Target));		
	}
	
	private IEnumerator SetTargetCoroutine(GameObject Target)
	{
		int Dist;

		int Count;

		Vector3 Vec = new Vector3();

		while (LockOnFlag)
		{
			Dist = (int)Mathf.Floor(Vector3.Distance(Target.transform.position, gameObject.transform.position));

			Count = VerticeList.Count - Dist;

			Vec = Target.transform.position - gameObject.transform.position;

			transform.LookAt(Target.transform);

			if (Count < 0)
			{
				VerticeList.Add(Vector3.zero);
			}
			else if(Count > 0 && VerticeList.Count > 1)
			{
				VerticeList.RemoveAt(VerticeList.Count - 1);
			}

			for (int i = 0; i < VerticeList.Count; i++)
			{
				VerticeList[i] = new Vector3(0, 0, i);
			}

			//メッシュ更新
			LockOnMesh.SetVertices(VerticeList);

			//LockOnMesh.vertices = VerticeList.ToArray();

			yield return null;
		}		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperDecalScript : GlobalClass
{
	//色を抽出するグラデーション
	public Gradient Grad;

	//マテリアル
	public Material LightDecalMaterial;

	//サインカーブ用カウント
	private float SinCount;

	void Start()
    {
		//プロジェクターにマテリアルをインスタンスで設定、同一マテリアルのプロパティ変更に対応する
		gameObject.GetComponent<Projector>().material = Instantiate(LightDecalMaterial);

		//グラデーションから色を抽出
		gameObject.GetComponent<Projector>().material.SetColor("_Color", Grad.Evaluate(Random.Range(0f, 1f)));

		//初期値をランダムにする
		SinCount = Random.Range(0f, 100f);
	}

    void Update()
    {
		//サインカーブカウントアップ
		SinCount += Time.deltaTime;

		//回転
		transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Sin(SinCount * 3) * 45, 0, Mathf.Sin(SinCount) * 45)) * Quaternion.Euler(90, 0, 0);
	}
}

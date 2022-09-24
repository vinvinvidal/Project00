using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnomatopeSettingScript : GlobalClass
{
	//セットするテクスチャList
	public List<Texture2D> TexList;

	//Ztest
	public float ZTest;

	//マテリアル
	private Material Mat;

	void Start()
	{
		//カメラに向ける
		gameObject.transform.LookAt(DeepFind(GameManagerScript.Instance.GetCameraOBJ(), "MainCamera").transform);

		//マテリアル取得
		Mat = GetComponent<Renderer>().material;

		//マテリアルにテクスチャをランダムでセット
		Mat.SetTexture("_TexParticle", TexList[Random.Range(0, TexList.Count)]);

		//マテリアルにZTextをセット
		Mat.SetFloat("_ZTest", ZTest);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalScript : GlobalClass
{
	//デカールリスト
	public List<Texture2D> DecalList;

	//使用するマテリアル
	public Material DecalMaterial;

    void Start()
    {
		//プロジェクターにマテリアルをインスタンスで設定、同一マテリアルのテクスチャ変更に対応する
		gameObject.GetComponent<Projector>().material =  Instantiate(DecalMaterial);

		//リストからテクスチャをランダムで決定してマテリアルに渡す
		gameObject.GetComponent<Projector>().material.SetTexture("_ShadowTex", DecalList[Random.Range(0,DecalList.Count)]);

		//チョイZ軸回転
		gameObject.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, Random.Range(-20, 20)));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSettingScript : GlobalClass
{
	//セットするテクスチャ
	public Texture2D Tex;

	//セットするテクスチャのST
	public Vector2 Tiling;
	public Vector2 OffSet;

	//Ztest
	public float ZTest;

	//マテリアル
	private List<Material> MatList;

	void Start()
    {
		//トレイルとかやるとマテリアルが複数の場合があるので、とりあえず全部に入れる
		MatList = new List<Material>(GetComponent<Renderer>().materials);

		//回す
		foreach (Material i in MatList)
		{
			//マテリアルにテクスチャをセット
			i.SetTexture("_TexParticle", Tex);

			//テクスチャのSTをセット
			i.SetTextureScale("_TexParticle", Tiling == Vector2.zero ? new Vector2(1,1) : Tiling);
			i.SetTextureOffset("_TexParticle", OffSet == Vector2.zero ? Vector2.zero : OffSet);

			//マテリアルにZTextをセット
			i.SetFloat("_ZTest", ZTest);
		}
	}
}

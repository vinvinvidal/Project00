using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShaderScript : MonoBehaviour
{
	//マテリアル
	private Material FireMaterial;

	//YXオフセット移動値
	private float TexTureX_Offset = 0;

	//Yオフセット移動値
	private float TexTureY_Offset = 0;

	//Y移動速度
	public float Y_Speed;

	//X移動速度
	public float X_Speed;

	void Start()
    {
		//マテリアル取得
		FireMaterial = gameObject.GetComponent<Renderer>().material;		
	}

    void Update()
    {
		//オフセットカウントアップ
		TexTureX_Offset += Time.deltaTime;
		TexTureY_Offset -= Mathf.PerlinNoise(-Time.time , Time.time) + 0.5f;

		//火を動かす
		FireMaterial.SetTextureOffset("_FireNormalTex", new Vector2(0, TexTureY_Offset * Y_Speed));
		FireMaterial.SetTextureOffset("_FireMainTex", new Vector2(TexTureX_Offset * X_Speed, 0));
	}
}

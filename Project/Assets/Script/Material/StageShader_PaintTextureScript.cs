using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageShader_PaintTextureScript : GlobalClass
{
	//ペイントテクスチャ配列
	public Texture2D[] TexPaint;

	void Start()
    {
		//ループカウント
		int count = 0;

		//マテリアルを回す
        foreach(Material i in GetComponent<Renderer>().materials)
		{
			//ペイントテクスチャをセット
			i.SetTexture("_TexPaint", TexPaint[count]);

			//カウントアップ
			count++;
		}
    }
}

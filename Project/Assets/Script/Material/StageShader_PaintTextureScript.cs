using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageShader_PaintTextureScript : GlobalClass
{
	//ペイントテクスチャ配列
	public Texture2D[] TexPaint;

	void Start()
    {
		int count = 0;

        foreach(Material i in GetComponent<Renderer>().materials)
		{
			i.SetTexture("_TexPaint", TexPaint[count]);

			count++;
		}
    }
}

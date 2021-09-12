using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBoardTextScript : GlobalClass
{
	//ペイントテクスチャ配列
	public Texture2D BlackBoardTextTex;

	void Start()
	{
		SetTexture();
	}

	public void SetTexture()
	{
		foreach (Material i in GetComponent<Renderer>().materials)
		{
			if (i.name.Contains("BlackBoard"))
			{
				i.SetTexture("_BlackBoardTextTex", BlackBoardTextTex);
			}
		}
	}
}

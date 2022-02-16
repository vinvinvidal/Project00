using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class MiniMapCameraScript : GlobalClass
{
    void Start()
    {
		//ミニマップのレンダーテクスチャを設定
		gameObject.GetComponent<Camera>().targetTexture = new RenderTexture(192, 192, 0, RenderTextureFormat.ARGBHalf);

		//UIのミニマップにテクスチャを設定
		DeepFind(gameObject.transform.root.gameObject, "MiniMap").GetComponent<RawImage>().texture = gameObject.GetComponent<Camera>().targetTexture;

		//メインカメラと水平回転を同期するコンストレイントを有効化、ここでやらないとなんかうまくいかない
		gameObject.GetComponent<RotationConstraint>().constraintActive = true;	
	}
}

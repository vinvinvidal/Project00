using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MiniMapCameraScript : GlobalClass
{
    void Start()
    {
		//ミニマップのレンダーテクスチャを設定
		gameObject.GetComponent<Camera>().targetTexture = new RenderTexture(192, 192, 0, RenderTextureFormat.ARGBHalf);

		//メインカメラと水平回転を同期するコンストレイントを有効化、ここでやらないとなんかうまくいかない
		gameObject.GetComponent<RotationConstraint>().constraintActive = true;
	}
}

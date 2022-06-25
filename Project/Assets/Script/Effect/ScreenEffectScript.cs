using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//メッセージシステムでイベントを受け取るためのインターフェイス
public interface ScreenEffectScriptInterface : IEventSystemHandler
{
	//フェード
	void Fade(bool f, int i, Color c, float t, Action<GameObject> a);

	//ズーム
	void Zoom(bool z, float p, float t, Action<GameObject> a);
}

public class ScreenEffectScript : GlobalClass, ScreenEffectScriptInterface
{
	//マテリアル
	Material mat;

	//ズーム係数、テクスチャのScaleとOffsetに入れる
	float EffectZoom;

	//シェーダーに送る色
	Color EffectColor;

	private void Start()
	{
		//マテリアル取得
		mat = gameObject.GetComponent<Renderer>().sharedMaterial;
	}

	//ズーム
	public void Zoom(bool z, float p, float t, Action<GameObject> a)
	{
		StartCoroutine(ZoomCoroutine(z, p, t, a));
	}
	IEnumerator ZoomCoroutine(bool z, float p, float t, Action<GameObject> a)
	{
		//有効化
		gameObject.GetComponent<Renderer>().enabled = true;

		//補完値宣言初期化
		float tm = t;

		//Zoom倍率宣言
		float Zoom = p;

		//引数で受け取ったフラグでインアウトを切り替える
		if (z)
		{
			//補完値が0になるまでループ
			while (Zoom > 0)
			{
				//ズーム係数を引数で受け取った秒数で補完する
				Zoom -= p / t * Time.deltaTime;

				//シェーダーにズーム倍率を送る
				mat.SetTextureScale("_TexParticle", new Vector2(Zoom * 0.5f, Zoom * 0.5f));
				mat.SetTextureOffset("_TexParticle", new Vector2(0, Zoom));

				//1フレーム待機
				yield return null;
			}
		}
		else
		{
			//補完値を初期化
			Zoom = 0;

			//補完値がしきい値以上になるまでループ
			while (Zoom < p)
			{
				//ズーム係数を引数で受け取った秒数で補完する
				Zoom += p / t * Time.deltaTime;

				//シェーダーにズーム倍率を送る
				mat.SetTextureScale("_TexParticle", new Vector2(Zoom * 0.5f, Zoom * 0.5f));
				mat.SetTextureOffset("_TexParticle", new Vector2(0, Zoom));

				//1フレーム待機
				yield return null;
			}
		}

		//ズーム倍率をリセット
		mat.SetTextureScale("_TexParticle", new Vector2(0, 0));
		mat.SetTextureOffset("_TexParticle", new Vector2(0, 0));

		//引数で受け取った関数実行
		a(transform.gameObject);
	}

	//フェード
	public void Fade(bool f, int i, Color c, float t, Action<GameObject> a)
	{
		StartCoroutine(FadeCoroutine(f,i,c,t,a));		
	}
	IEnumerator FadeCoroutine(bool f, int i, Color c, float t, Action<GameObject> a)
	{
		//有効化
		gameObject.GetComponent<Renderer>().enabled = true;

		//補完値宣言初期化
		float tm = t;

		//開始値宣言
		float Begin = t;

		//終了値宣言
		float End = t;

		//引数のboolでフェードインアウトを分ける
		if(f)
		{
			Begin = 0;
		}
		else
		{
			End = 0;
		}

		//シェーダーの合成法フラグを立てる
		switch (i)
		{
			case 0: mat.EnableKeyword("Add"); break;
			case 1: mat.EnableKeyword("Mul"); break;
			case 2: mat.EnableKeyword("Nor"); break;
			default:break;
		}

		//補完値が0になるまでループ
		while (tm > 0)
		{
			//引数で受け取った秒数で補完する
			tm -= Time.deltaTime;
			
			//シェーダーに引数で受け取ったカラーを送る
			mat.SetColor("_Color", new Color(c.r, c.g, c.b, Mathf.InverseLerp(Begin, End, tm)));

			//1フレーム待機
			yield return null;
		}

		//引数で受け取った関数実行
		a(transform.gameObject);
	}	
}

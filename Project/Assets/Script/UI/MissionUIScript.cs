using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissionUIScript : GlobalClass
{
	//装備中の技マトリクスオブジェクト
	private GameObject ArtsMatrixOBJ;

	//技マトリクスのルートオブジェクト
	private GameObject ArtsMatrixRoot;

	//アニメーター
	private Animator AnimCon;

	//スクリーンイメージ
	private Image ScreenIMG;

	//読み込みロゴイメージ
	private Image WaitLogoIMG00;
	private Image WaitLogoIMG01;

	//ウェイトバーイメージ
	private Image WaitBarIMG;

	//ミッションに参加しているプレイヤーキャラクターの技マトリクス連想配列
	private Dictionary<int, GameObject> ArtsMatrixDic = new Dictionary<int, GameObject>();

	//オノマトペオブジェクト
	private GameObject OnomatopeOBJ;

	//弱攻撃が当たった時テクスチャList
	public List<Texture2D> LightAttackHitOnomatopeTextureList;

	//中攻撃が当たった時テクスチャList
	public List<Texture2D> MiddleAttackHitOnomatopeTextureList;

	//強攻撃が当たった時テクスチャList
	public List<Texture2D> HeavyAttackHitOnomatopeTextureList;

	//オノマトペテクスチャList
	private List<List<Texture2D>> OnomatopeTextureList = new List<List<Texture2D>>();

	void Start()
    {
		//装備中の技マトリクスオブジェクト取得
		ArtsMatrixOBJ = DeepFind(gameObject, "ArtsMatrix");

		//これ自体はコピー元なので消しとく
		ArtsMatrixOBJ.SetActive(false);	

		//技マトリクスのルートオブジェクト取得
		ArtsMatrixRoot = DeepFind(gameObject, "ArtsMatrixRoot");

		//アニメーター取得
		AnimCon = GetComponent<Animator>();

		//スクリーンイメージ取得
		ScreenIMG = DeepFind(gameObject, "Screen").GetComponent<Image>();

		//読み込みロゴイメージ取得
		WaitLogoIMG00 = DeepFind(gameObject, "WaitLogo00").GetComponent<Image>();
		WaitLogoIMG01 = DeepFind(gameObject, "WaitLogo01").GetComponent<Image>();

		//ウェイトバーイメージ取得
		WaitBarIMG = DeepFind(gameObject, "WaitBar").GetComponent<Image>();

		//オノマトペオブジェクト取得
		OnomatopeOBJ = DeepFind(gameObject, "Onomatope");

		//オノマトペテクスチャListをAdd
		OnomatopeTextureList.Add(LightAttackHitOnomatopeTextureList);
		OnomatopeTextureList.Add(MiddleAttackHitOnomatopeTextureList);
		OnomatopeTextureList.Add(HeavyAttackHitOnomatopeTextureList);
	}

	//攻撃ヒットオノマトペ表示
	public void ShowAttackHitOnomatope(OnomatopeTextureEnum Enum, GameObject Target)
	{
		//オノマトペのインスタンス生成
		GameObject TempOnomatope = Instantiate(OnomatopeOBJ);

		//テクスチャ宣言
		Texture2D TempTexture = null;

		//テクスチャ取得
		TempTexture = OnomatopeTextureList[(int)Enum][Random.Range(0, OnomatopeTextureList[(int)Enum].Count)];

		//親を設定、サイズ変更前に処理しないとスクリーンサイズで大きさが変わってしまう
		TempOnomatope.GetComponent<RectTransform>().SetParent(gameObject.transform);

		//テクスチャ割り当て
		TempOnomatope.GetComponent<Image>().sprite = Sprite.Create(TempTexture, new Rect(0, 0, TempTexture.width, TempTexture.height), Vector2.zero);

		//サイズをテクスチャと合わせる
		TempOnomatope.GetComponent<RectTransform>().sizeDelta = new Vector2(TempTexture.width, TempTexture.height);

		//スケールを１にする
		TempOnomatope.GetComponent<Image>().rectTransform.localScale = Vector3.one;

		//表示用関数呼び出し
		TempOnomatope.GetComponent<OnomatopeScript>().ShowAttackHitOnomatope(Target.transform.position);
	}

	//読み込み待ちロゴスタート
	public void StartWaitLogo()
	{
		//有効化
		WaitLogoIMG00.enabled = true;
		WaitLogoIMG01.enabled = true;

		//アニメーターのフラグを立てる
		AnimCon.SetBool("Wait_Start", true);
		AnimCon.SetBool("Wait_End", false);
	}

	//読み込み待ちロゴエンド
	public void EndWaitLogo()
	{
		//アニメーターのフラグを立てる
		AnimCon.SetBool("Wait_End", true);
	}

	//読み込み待ちロゴ完了
	public void FinishWaitLogo()
	{
		//無効化
		WaitLogoIMG00.enabled = false;
		WaitLogoIMG01.enabled = false;

		//アニメーターのフラグを下す
		AnimCon.SetBool("Wait_Start", false);
		AnimCon.SetBool("Wait_End", false);
	}

	//読み込み状況に合わせてウェイトバーを延ばす
	public void SetWaitbar(float n)
	{
		//ゼロが入ってきたらスケールをゼロにする
		if (n == 0)
		{
			WaitBarIMG.rectTransform.localScale = new Vector3(0, 1, 1);

			WaitBarIMG.enabled = true;
		}
		else if (n == 1)
		{

		}
		else
		{
			WaitBarIMG.rectTransform.localScale = new Vector3(n, 1, 1);
		}
	}

	//スクリーンをフェードさせる
	public void FadeScreen(bool b, float t)
	{
		//コルーチン呼び出し
		StartCoroutine(FadeScreenCoroutine(b,t));
	}
	private IEnumerator FadeScreenCoroutine(bool b, float t)
	{
		//加算用Color宣言
		Color TempColor = new Color(0, 0, 0, t);

		//フェードイン
		if (b)
		{
			//透明度をゼロに
			ScreenIMG.color = new Color(ScreenIMG.color.r, ScreenIMG.color.g, ScreenIMG.color.b, 0);

			//有効化
			ScreenIMG.enabled = true;

			//透明度が１になるまでループ
			while (ScreenIMG.color.a < 1)
			{
				//透明度加算
				ScreenIMG.color += TempColor;

				//1フレーム待機
				yield return null;
			}

			//透明度を１にする
			ScreenIMG.color = new Color(ScreenIMG.color.r, ScreenIMG.color.g, ScreenIMG.color.b, 1);
		}
		//フェードアウト
		else
		{
			//有効化
			ScreenIMG.enabled = true;

			//透明度が0になるまでループ
			while (ScreenIMG.color.a > 0)
			{
				//透明度加算
				ScreenIMG.color -= TempColor;

				//1フレーム待機
				yield return null;
			}

			//透明度を0にする
			ScreenIMG.color = new Color(ScreenIMG.color.r, ScreenIMG.color.g, ScreenIMG.color.b, 0);

			//無効化
			ScreenIMG.enabled = false;
		}		
	}

	//プレイヤーキャラクター分の技マトリクスを用意する、ミッションセッティングから呼ばれる
	public void SettingArtsMatrix(MissionClass mission)
	{
		//チャプターに参加するキャラクター分だけマトリクスのインスタンスを複製
		foreach(var i in mission.ChapterCharacterList[GameManagerScript.Instance.SelectedMissionChapter])
		{
			//インスタンス生成
			GameObject TempMatrix = Instantiate(ArtsMatrixOBJ);

			//親を設定
			TempMatrix.GetComponent<RectTransform>().SetParent(ArtsMatrixRoot.GetComponent<RectTransform>());

			//位置合わせ
			TempMatrix.GetComponent<RectTransform>().position = ArtsMatrixOBJ.GetComponent<RectTransform>().position;

			//何か変なスケールが入るのでリセット
			TempMatrix.GetComponent<RectTransform>().localScale = ArtsMatrixOBJ.GetComponent<RectTransform>().localScale;

			//連想配列にAdd
			ArtsMatrixDic.Add(i, TempMatrix);
		}

		//連想配列を回して処理
		foreach(var n in ArtsMatrixDic.Keys)
		{
			//有効化
			ArtsMatrixDic[n].SetActive(true);

			//ヒエラルキーを上げて描画順を変更、とりあえずインデックスが若い順になる
			ArtsMatrixDic[n].GetComponent<Transform>().SetAsFirstSibling();
		}

		//初期操作キャラクターのマトリクスを前面にする
		ChangeArtsMatrixHierarchy(mission.FirstCharacterList[GameManagerScript.Instance.SelectedMissionChapter]);
	}

	//技マトリクスに技をとスクリプトをセットする、プレイヤースクリプトの初期処理から呼ばれる
	public void SetArtsClass(int c, ArtsClass Arts)
	{
		DeepFind(ArtsMatrixDic[c], "Arts" + Arts.MatrixPos).AddComponent<MissionUIArtsCoolDownScript>().Arts = Arts;
	}

	//キャラクターチェンジ処理
	public void CharacterChange(int n)
	{
		//技マトリクスのヒエラルキーを変更
		ChangeArtsMatrixHierarchy(n);
	}

	//技マトリクスのヒエラルキーを変更する、将来的にはアニメーションさせる
	public void ChangeArtsMatrixHierarchy(int n)
	{
		//ループカウント
		int count = 0;

		//指定されたキャラのマトリクスが一番下に来るまで回す
		while (ArtsMatrixDic[n].GetComponent<Transform>().GetSiblingIndex() != ArtsMatrixDic.Count - 1)
		{
			//一番上の奴を下に回す
			ArtsMatrixRoot.GetComponentsInChildren<Transform>().Where(a => !a.name.Contains("Root")).ToList()[0].SetAsLastSibling();

			//ループカウントアップ
			count++;

			//無限ループ防止用Break
			if(count > 10)
			{
				break;
			}
		}
	}
}

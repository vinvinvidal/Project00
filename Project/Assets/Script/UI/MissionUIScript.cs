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

	//ミッションに参加しているプレイヤーキャラクターの技マトリクス連想配列
	private Dictionary<int, GameObject> ArtsMatrixDic = new Dictionary<int, GameObject>();

	void Start()
    {
		//装備中の技マトリクスオブジェクト取得
		ArtsMatrixOBJ = DeepFind(gameObject, "ArtsMatrix");

		//これ自体はコピー元なので消しとく
		ArtsMatrixOBJ.SetActive(false);	

		//技マトリクスのルートオブジェクト取得
		ArtsMatrixRoot = DeepFind(gameObject, "ArtsMatrixRoot");
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

			//UserDataから装備中の技を取得
			for (int i = 0; i <= GameManagerScript.Instance.UserData.ArtsMatrix[n].Count - 1; i++)
			{
				for (int ii = 0; ii <= GameManagerScript.Instance.UserData.ArtsMatrix[n][i].Count - 1; ii++)
				{
					for (int iii = 0; iii <= GameManagerScript.Instance.UserData.ArtsMatrix[n][i][ii].Count - 1; iii++)
					{
						if(GameManagerScript.Instance.UserData.ArtsMatrix[n][i][ii][iii] != null)
						{
							DeepFind(ArtsMatrixDic[n],"Arts" + i + ii + iii).GetComponentInChildren<Text>().text = GameManagerScript.Instance.UserData.ArtsMatrix[n][i][ii][iii];
						}					
					}
				}
			}
		}

		//初期操作キャラクターのマトリクスを前面にする
		ChangeArtsMatrixHierarchy(mission.FirstCharacterList[GameManagerScript.Instance.SelectedMissionChapter]);
	}

	//キャラクターチェンジ処理
	public void CharacterChange(int n)
	{
		//技マトリクス変更
		ChangeArtsMatrixHierarchy(n);
	}

	//技マトリクスのヒエラルキーを変更する、Whileを使うので一応コルーチンにしておく
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

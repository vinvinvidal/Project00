using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scene01_MainMenuScript : GlobalClass
{
	//UIのイベントを受け取るイベントシステム
	private EventSystem EventSystemUI;

	//UIのアニメーター
	private Animator UIAnim;

	//メインメニューのオブジェクトルート
	private GameObject MainMenuOBJ;

	//カスタマイズのオブジェクトルート
	private GameObject CustomizeOBJ;

	//技装備のオブジェクトルート
	private GameObject ArtsEquipOBJ;

	//技選択用ボタン
	private GameObject ArtsSelectButtonOBJ;

	//技選択リストコンテンツルートオブジェクト
	private GameObject ArtsSelectButtonContentOBJ;

	//生成した技選択ボタンを格納するList
	List<GameObject> ArtsSelectButtonList = new List<GameObject>();

	//入力許可フラグ
	private bool InputReadyFlag = false;

	void Start()
    {
		//UIのイベントを受け取るイベントシステム取得
		EventSystemUI = GameObject.Find("EventSystem").GetComponent<EventSystem>();

		//メインメニューのオブジェクトルート取得
		MainMenuOBJ = GameObject.Find("MainMenu");

		//カスタマイズのオブジェクトルート取得
		CustomizeOBJ = GameObject.Find("Customize");

		//技装備のオブジェクトルート取得
		ArtsEquipOBJ = GameObject.Find("ArtsEquip");

		//技選択用ボタン取得
		ArtsSelectButtonOBJ = GameObject.Find("SelectArtsButton");

		//技選択リストコンテンツルートオブジェクト取得
		ArtsSelectButtonContentOBJ = GameObject.Find("ArtsListContent");

		//キャンバスにカメラを設定
		GetComponent<Canvas>().worldCamera = GameObject.Find("MainCamera").GetComponent<Camera>();

		//キャンバスの位置を設定
		GetComponent<Canvas>().planeDistance = 0.25f;

		//アニメーター取得
		UIAnim = GetComponent<Animator>();

		//カメラの位置を移動
		Transform CameraTransform = GameObject.Find("CameraRoot").GetComponent<Transform>();
		CameraTransform.position = new Vector3(-12f, 6.5f, 1.4f);
		CameraTransform.rotation = Quaternion.Euler(new Vector3(30, 140, 0));

		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 2, new Color(1, 1, 1, 1), 1, (GameObject g) => { g.GetComponent<Renderer>().enabled = false; }));
	}

    void Update()
    {

    }

	//スタートボタンがSubmitされた時の処理
	public void StartSubmit()
	{
		if(InputReadyFlag)
		{
			//スクリーンエフェクトで白フェード
			ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(false, 2, new Color(1, 1, 1, 1), 1, (GameObject g) =>
			{
				//フェードが終わったらゲームマネージャーのシーン遷移関数呼び出し
				GameManagerScript.Instance.NextScene("Scene10_Mission");
			}));
		}
	}

	//カスタマイズがSubmitされた時の処理
	public void CustomizeSubmit()
	{
		if (InputReadyFlag)
		{
			//アニメーターのフラグを立てる
			UIAnim.SetBool("MainMenu_Show", false);
			UIAnim.SetBool("MainMenu_Vanish", true);
			UIAnim.SetBool("Customize_Show", true);
			UIAnim.SetBool("Customize_Vanish", false);

			//入力許可フラグを下ろす
			InputReadyFlag = false;
		}	
	}

	//カスタマイズでCancelされた時の処理
	public void CustomizeCancel()
	{
		if (InputReadyFlag)
		{
			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", true);
			UIAnim.SetBool("Customize_Show", false);
			UIAnim.SetBool("MainMenu_Show", true);
			UIAnim.SetBool("MainMenu_Vanish", false);

			//入力許可フラグを下ろす
			InputReadyFlag = false;
		}
	}

	//技装備がSubmitされた時の処理
	public void ArtsEquipSubmit()
	{
		if (InputReadyFlag)
		{
			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", true);
			UIAnim.SetBool("Customize_Show", false);
			UIAnim.SetBool("ArtsEquip_Show", true);
			UIAnim.SetBool("ArtsEquip_Vanish", false);

			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//装備技List生成、とりあえず御命
			ArtsEquipListReset(0);
		}
	}

	//装備技Listを更新する関数
	private void ArtsEquipListReset(int n)
	{
		//装備中の技表示用変数宣言
		int Location = 0;
		int Stick = 0;
		int Button = 0;

		//装備中の技を表示
		foreach (var i in GameManagerScript.Instance.UserData.ArtsMatrix[n])
		{
			Stick = 0;

			foreach (var ii in i)
			{
				Button = 0;

				foreach (var iii in ii)
				{
					if (iii != null)
					{
						GameObject.Find("EquipArtsButton" + Location + Stick + Button).GetComponentInChildren<Text>().text = iii;
					}

					Button++;
				}

				Stick++;

			}

			Location++;
		}

		//汎用ボタンのRect取得
		RectTransform Temprect = ArtsSelectButtonOBJ.GetComponent<RectTransform>();

		//ループカウント宣言
		int count = 0;

		//全ての技を回す
		foreach (ArtsClass i in GameManagerScript.Instance.AllArtsList)
		{
			//引数で仕様キャラクターを判別、アンロックされているかを判別
			if (i.UseCharacter == n && GameManagerScript.Instance.UserData.ArtsUnLock.Any(a => a == i.NameC))
			{
				//ボタンのインスタンス生成
				GameObject TempButton = Instantiate(ArtsSelectButtonOBJ);

				//親を設定
				TempButton.GetComponent<RectTransform>().SetParent(ArtsSelectButtonContentOBJ.transform, false);

				//位置を設定
				TempButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(Temprect.anchoredPosition.x, Temprect.anchoredPosition.y - ((Temprect.rect.height + 5) * count));

				//テキストに技名を入れる
				TempButton.GetComponentInChildren<Text>().text = i.NameC;

				//ボタンの名前を連番にする
				TempButton.name = ArtsSelectButtonOBJ.name + count;

				//ListにAdd
				ArtsSelectButtonList.Add(TempButton);

				//ループカウントアップ
				count++;
			}
		}

		//ループカウントリセット
		count = 0;

		//生成したボタンを回す
		foreach (GameObject o in ArtsSelectButtonList)
		{
			//ナビゲーションのアクセサ取得
			Navigation tempnavi = o.GetComponent<Button>().navigation;

			//上を押した時の処理
			if (count == 0)
			{
				tempnavi.selectOnUp = DeepFind(ArtsSelectButtonContentOBJ, ArtsSelectButtonOBJ.name + (ArtsSelectButtonList.Count - 1)).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnUp = DeepFind(ArtsSelectButtonContentOBJ, ArtsSelectButtonOBJ.name + (count - 1)).GetComponent<Button>();
			}

			//下を押した時の選択先設定
			if (count == ArtsSelectButtonList.Count - 1)
			{
				tempnavi.selectOnDown = DeepFind(ArtsSelectButtonContentOBJ, ArtsSelectButtonOBJ.name + 0).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnDown = DeepFind(ArtsSelectButtonContentOBJ, ArtsSelectButtonOBJ.name + (count+1)).GetComponent<Button>();				
			}

			//アクセサを反映
			o.GetComponent<Button>().navigation = tempnavi;

			//ループカウントアップ
			count++;
		}
	}

	//技装備がCancelされた時の処理
	public void ArtsEquipCancel()
	{
		if (InputReadyFlag)
		{
			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", false);
			UIAnim.SetBool("Customize_Show", true);
			UIAnim.SetBool("ArtsEquip_Show", false);
			UIAnim.SetBool("ArtsEquip_Vanish", true);

			//入力許可フラグを下ろす
			InputReadyFlag = false;
		}
	}

	//メインメニュー有効、アニメーションクリップから呼ばれる
	public void MainMenuActive()
	{
		//選択状態のボタンを切り替え
		EventSystemUI.SetSelectedGameObject(GameObject.Find("StartButton"));
	}

	//カスタマイズ有効、アニメーションクリップから呼ばれる
	public void CustomizeActive()
	{
		//選択状態のボタンを切り替え
		EventSystemUI.SetSelectedGameObject(GameObject.Find("ArtsEquipButton"));

		//技装備ボタンオブジェクト破棄
		foreach (GameObject i in ArtsSelectButtonList)
		{
			Destroy(i);
		}

		//技装備ボタンList初期化
		ArtsSelectButtonList = new List<GameObject>();
	}

	//技装備有効、アニメーションクリップから呼ばれる
	public void ArtsEquipActive()
	{
		//選択状態のボタンを切り替え
		EventSystemUI.SetSelectedGameObject(GameObject.Find("SelectArtsButton0"));
	}

	//入力許可フラグを立てる、アニメーションクリップから呼ばれる
	public void InputReady()
	{
		InputReadyFlag = true;
	}
}

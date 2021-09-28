﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

	//技装備マトリクスオブジェクトList
	private List<GameObject> EquipArtsList = new List<GameObject>();

	//技装備のオブジェクトルート
	private GameObject ArtsEquipOBJ;

	//技選択用ボタン
	private GameObject ArtsSelectButtonOBJ;

	//技選択リストコンテンツルートオブジェクト
	private GameObject ArtsSelectButtonContentOBJ;

	//技選択リストスクロールバーオブジェクト
	private GameObject ArtsSelectScrollBarOBJ;

	//選択中のオブジェクト
	private GameObject SelectedArtsOBJ;

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

		//技装備マトリクスオブジェクトList取得
		foreach(Button i in GameObject.Find("EquipArtsButton").GetComponentsInChildren<Button>())
		{
			EquipArtsList.Add(i.gameObject);
		}

		//技装備のオブジェクトルート取得
		ArtsEquipOBJ = GameObject.Find("ArtsEquip");

		//技選択用ボタン取得
		ArtsSelectButtonOBJ = GameObject.Find("SelectArtsButton");

		//技選択リストコンテンツルートオブジェクト取得
		ArtsSelectButtonContentOBJ = GameObject.Find("ArtsListContent");

		//技選択リストスクロールバーオブジェクト取得
		ArtsSelectScrollBarOBJ = DeepFind(ArtsEquipOBJ, "ArtsList");

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

	//汎用ボタンが押された時の処理
	public void OnGeneral()
	{
		if (InputReadyFlag)
		{
			//装備技マトリクスが選択されている
			if (EquipArtsList.Any(a => a == EventSystemUI.currentSelectedGameObject))
			{
				//装備解除
				EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text = "";

				//選択技解除
				SelectedArtsOBJ = null;
			}
		}
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

	//技選択でSubmitされた時の処理
	public void SelectedArtsSubmit()
	{
		//選択中の技オブジェクトなし
		if (SelectedArtsOBJ == null)
		{
			//技オブジェクトを取得
			SelectedArtsOBJ = EventSystemUI.currentSelectedGameObject;
		}
		//選択中の技オブジェクトがある
		else
		{
			//選択側テキストキャッシュ
			string SelectedName = SelectedArtsOBJ.GetComponentInChildren<Text>().text;

			//ターゲット側テキストキャッシュ
			string TargetName = EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text;

			//装備可能フラグ
			bool EquipFlag = false;

			//マトリクス側が選択されている
			if (EquipArtsList.Any(a => a == SelectedArtsOBJ))
			{
				//マトリクス側がターゲットされている
				if (EquipArtsList.Any(a => a == EventSystemUI.currentSelectedGameObject))
				{
					//選択側が空欄
					if(SelectedName == "")
					{
						EquipFlag = AirArtsCheck(EventSystemUI.currentSelectedGameObject, SelectedArtsOBJ);
					}
					else
					{
						EquipFlag = AirArtsCheck(SelectedArtsOBJ, EventSystemUI.currentSelectedGameObject);
					}

					//装備
					if(EquipFlag)
					{
						EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text = SelectedName;
						SelectedArtsOBJ.GetComponentInChildren<Text>().text = TargetName;
					}
				}
				//リスト側がターゲットされている
				else if(ArtsSelectButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
				{
					if (AirArtsCheck(EventSystemUI.currentSelectedGameObject , SelectedArtsOBJ))
					{
						//すでに装備していたら外す
						foreach (GameObject i in EquipArtsList)
						{
							if (i.GetComponentInChildren<Text>().text == TargetName)
							{
								i.GetComponentInChildren<Text>().text = SelectedName;

								break;
							}
						}

						SelectedArtsOBJ.GetComponentInChildren<Text>().text = TargetName;
					}
				}

				//選択技解除
				SelectedArtsOBJ = null;
			}
			//リスト側が選択されている
			else
			{
				//マトリクス側がターゲットされている
				if (EquipArtsList.Any(a => a == EventSystemUI.currentSelectedGameObject))
				{
					EquipFlag = AirArtsCheck(SelectedArtsOBJ, EventSystemUI.currentSelectedGameObject);					

					//装備
					if (EquipFlag)
					{
						//すでに装備していたら外す
						foreach (GameObject i in EquipArtsList)
						{
							if (i.GetComponentInChildren<Text>().text == SelectedName)
							{
								i.GetComponentInChildren<Text>().text = TargetName;

								break;
							}
						}

						EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text = SelectedName;
					}

					//選択技解除
					SelectedArtsOBJ = null;
				}
				else
				{
					SelectedArtsOBJ = EventSystemUI.currentSelectedGameObject;
				}
			}
		}
	}

	//空中技チェックして技装備する関数
	private bool AirArtsCheck(GameObject arts,GameObject target)
	{
		//技名取得
		string artsname = arts.GetComponentInChildren<Text>().text;

		//ターゲットされている場所の名前からロケーション部分を抽出
		char TargetArtsNum = target.name.Skip(15).Take(1).ToList()[0];

		//空中装備可能場所か判別
		bool re = TargetArtsNum == '2';

		//全ての技を回す
		foreach (ArtsClass i in GameManagerScript.Instance.AllArtsList)
		{
			//名前で検索
			if(i.NameC == artsname)
			{
				//フラグを比較してboolを返す
				if(i.AirAttackFlag == re)
				{
					re = true;
				}
				else
				{
					re = false;
				}

				break;
			}
		}

		return re;
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
				TempButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(Temprect.anchoredPosition.x, Temprect.anchoredPosition.y - (Temprect.rect.height * count));

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

		//装備技のボタンにナビゲータを仕込む
		foreach(var i in EquipArtsList)
		{
			if(i.name.Last() == '0')
			{
				//ナビゲーションのアクセサ取得
				Navigation tempnavi = i.GetComponent<Button>().navigation;

				//生成したボタンの一番上を選択させる、本当は位置を合わせたい
				tempnavi.selectOnLeft = ArtsSelectButtonList[0].GetComponent<Button>();

				//アクセサを反映
				i.GetComponent<Button>().navigation = tempnavi;
			}
		}
	}

	//技装備で選択変更された時の処理
	public void ArtsEquipMove()
	{
		//処理実行フラグ
		bool SelectedListOBJFlag = false;

		//選択されているオブジェクトのインデックスに使うループカウント
		int count = ArtsSelectButtonList.Count - 1;

		//選択されているオブジェクトを判別、ついでにインデックスも取得
		foreach (GameObject i in ArtsSelectButtonList)
		{
			//リスト内のオブジェクトが選択されていたらフラグを立てる
			if(EventSystemUI.currentSelectedGameObject == i)
			{
				SelectedListOBJFlag = true;

				break;
			}

			//カウントダウン
			count--;
		}

		//フラグで処理実行
		if(InputReadyFlag && SelectedListOBJFlag)
		{
			//現在のスクロールバーの位置
			float BarPos = ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition;

			//見えない部分全体のサイズ
			float ArtsSelectButtonContentSize = ArtsSelectButtonContentOBJ.GetComponent<RectTransform>().sizeDelta.y;

			//見えている部分のサイズ
			float ArtsSelectScrollSize = ArtsSelectScrollBarOBJ.GetComponent<RectTransform>().sizeDelta.y;

			//現在の下座標
			float WindowBottomPos = ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition * (ArtsSelectButtonContentSize - ArtsSelectScrollSize);

			//現在の上座標
			float WindowTopPos = WindowBottomPos + ArtsSelectScrollSize;

			//選択オブジェクトの下座標
			float ButtonBottomPos = (count * EventSystemUI.currentSelectedGameObject.GetComponent<RectTransform>().rect.height) + (count * ArtsSelectButtonContentOBJ.GetComponent<VerticalLayoutGroup>().spacing);

			//選択オブジェクトの上座標
			float ButtonTopPos = ButtonBottomPos + EventSystemUI.currentSelectedGameObject.GetComponent<RectTransform>().rect.height;

			//上にはみ出し
			if (WindowTopPos < ButtonTopPos)
			{
				ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = (ButtonTopPos - ArtsSelectScrollSize) / (ArtsSelectButtonContentSize - ArtsSelectScrollSize);
			}
			//下にはみ出し
			else if (ButtonBottomPos < WindowBottomPos)
			{
				ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = ButtonBottomPos / (ArtsSelectButtonContentSize - ArtsSelectScrollSize);
			}
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

using System.Collections;
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

	//カメラルートオブジェクト
	private GameObject CameraRootOBJ;

	//カメラワークオブジェクト
	private GameObject CameraWorkOBJ;

	//メインメニューのオブジェクトルート
	private GameObject MainMenuOBJ;

	//カスタマイズのオブジェクトルート
	private GameObject CustomizeOBJ;

	//技装備マトリクスオブジェクトList
	private List<GameObject> ArtsMatrixButtonList = new List<GameObject>();

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

	//衣装選択用ボタン
	private GameObject CostumeSelectButtonOBJ;

	//衣装選択リストコンテンツルートオブジェクト
	private GameObject CostumeSelectButtonContentOBJ;

	//衣装選択リストスクロールバーオブジェクト
	private GameObject CostumeSelectScrollBarOBJ;

	//生成した衣装選択ボタンを格納するList
	List<GameObject> CostumeSelectButtonList = new List<GameObject>();

	//下着選択用ボタン
	private GameObject UnderWearSelectButtonOBJ;

	//下着選択リストコンテンツルートオブジェクト
	private GameObject UnderWearSelectButtonContentOBJ;

	//下着選択リストスクロールバーオブジェクト
	private GameObject UnderWearSelectScrollBarOBJ;

	//生成した下着選択ボタンを格納するList
	List<GameObject> UnderWearSelectButtonList = new List<GameObject>();

	//武器選択用ボタン
	private GameObject WeaponSelectButtonOBJ;

	//武器選択リストコンテンツルートオブジェクト
	private GameObject WeaponSelectButtonContentOBJ;

	//武器選択リストスクロールバーオブジェクト
	private GameObject WeaponSelectScrollBarOBJ;

	//武器した衣装選択ボタンを格納するList
	List<GameObject> WeaponSelectButtonList = new List<GameObject>();



	//入力許可フラグ
	private bool InputReadyFlag = false;

	//編集中のキャラクターID
	private int CharacterID = 0;

	//編集可能なキャラクターオブジェクトリスト
	private List<GameObject> CharacterList = new List<GameObject>();

	//編集可能な衣装リスト
	private List<List<GameObject>> CostumeList = new List<List<GameObject>>();

	//編集可能な下着リスト
	private List<List<GameObject>> UnderWearList = new List<List<GameObject>>();

	//編集可能な武器リスト
	private List<List<GameObject>> WeaponList = new List<List<GameObject>>();

	//現在のメニューモード
	private CurrentModeEnum CurrentMode;

	//現在のメニューモードEnum
	private enum CurrentModeEnum
	{
		MainMenu,           //メインメニュー
		Customize,          //カスタマイズメニュー
		Option,		       //オプションメニュー
		Arts,				//技装備
		Costume,			//衣装装備
	}

	void Start()
    {
		//カメラルートオブジェクト取得
		CameraRootOBJ = GameObject.Find("CameraRoot");

		//カメラワークオブジェクト取得
		CameraWorkOBJ = GameObject.Find("CameraWork");

		//現在のメニューモードをメインメニューにする
		CurrentMode = CurrentModeEnum.MainMenu;

		//UIのイベントを受け取るイベントシステム取得
		EventSystemUI = GameObject.Find("EventSystem").GetComponent<EventSystem>();

		//メインメニューのオブジェクトルート取得
		MainMenuOBJ = GameObject.Find("MainMenu");

		//カスタマイズのオブジェクトルート取得
		CustomizeOBJ = GameObject.Find("Customize");

		//技装備マトリクスオブジェクトList取得
		foreach(Button i in GameObject.Find("EquipArtsButton").GetComponentsInChildren<Button>())
		{
			ArtsMatrixButtonList.Add(i.gameObject);
		}

		//技装備のオブジェクトルート取得
		ArtsEquipOBJ = GameObject.Find("ArtsEquip");

		//技選択用ボタン取得
		ArtsSelectButtonOBJ = GameObject.Find("SelectArtsButton");

		//衣装選択用ボタン取得
		CostumeSelectButtonOBJ = GameObject.Find("SelectCostumeButton");

		//下着選択用ボタン取得
		UnderWearSelectButtonOBJ = GameObject.Find("SelectUnderWearButton");

		//武器選択用ボタン取得
		WeaponSelectButtonOBJ = GameObject.Find("SelectWeaponButton");



		//技選択リストコンテンツルートオブジェクト取得
		ArtsSelectButtonContentOBJ = GameObject.Find("ArtsListContent");

		//衣装選択リストコンテンツルートオブジェクト取得
		CostumeSelectButtonContentOBJ = GameObject.Find("CostumeListContent");

		//下着選択リストコンテンツルートオブジェクト取得
		UnderWearSelectButtonContentOBJ = GameObject.Find("UnderWearListContent");

		//武器選択リストコンテンツルートオブジェクト取得
		WeaponSelectButtonContentOBJ = GameObject.Find("WeaponListContent");

		

		//技選択リストスクロールバーオブジェクト取得
		ArtsSelectScrollBarOBJ = DeepFind(ArtsEquipOBJ, "ArtsList");

		//衣装選択リストスクロールバーオブジェクト取得
		CostumeSelectScrollBarOBJ = DeepFind(GameObject.Find("CostumeEquip"), "CostumeList");

		//下着選択リストスクロールバーオブジェクト取得
		UnderWearSelectScrollBarOBJ = DeepFind(GameObject.Find("CostumeEquip"), "UnderWearList");

		//武器選択リストスクロールバーオブジェクト取得
		WeaponSelectScrollBarOBJ = DeepFind(GameObject.Find("CostumeEquip"), "WeaponList");



		//キャンバスにカメラを設定
		GetComponent<Canvas>().worldCamera = GameManagerScript.Instance.GetMainCameraOBJ().GetComponent<Camera>();

		//キャンバスの位置を設定
		GetComponent<Canvas>().planeDistance = 0.25f;

		//アニメーター取得
		UIAnim = GetComponent<Animator>();

		//カメラのメニューモード初期設定呼び出し
		ExecuteEvents.Execute<MainCameraScriptInterface>(CameraRootOBJ, null, (reciever, eventData) => reciever.MenuCameraSetting());

		//コルーチン呼び出し
		StartCoroutine(MainMenuStartCoroutine());
	}

	private IEnumerator MainMenuStartCoroutine()
	{
		//キャラクターリスト初期化
		CharacterList = new List<GameObject>();

		//とりあえず要素数を確保
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);
		CharacterList.Add(null);

		//衣装リスト初期化
		CostumeList = new List<List<GameObject>>();

		//とりあえず要素数を確保
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());
		CostumeList.Add(new List<GameObject>());

		//下着リスト初期化
		UnderWearList = new List<List<GameObject>>();

		//とりあえず要素数を確保
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());
		UnderWearList.Add(new List<GameObject>());

		//武器リスト初期化
		WeaponList = new List<List<GameObject>>();

		//とりあえず要素数を確保
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());
		WeaponList.Add(new List<GameObject>());

		foreach (var CID in GameManagerScript.Instance.UserData.CharacterUnLock)
		{
			//キャラクター読み込み完了フラグ宣言
			bool CharacterLoadFlag = false;

			//髪型読み込み完了フラグ宣言
			bool HairLoadFlag = false;

			//衣装読み込み完了フラグ宣言
			bool CostumeLoadFlag = false;

			//下着読み込み完了フラグ宣言
			bool UnderWearLoadFlag = false;

			//武器読み込み完了フラグ宣言
			bool WeaponLoadFlag = false;

			//キャラクターモデル読み込み
			StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + CID + "/", GameManagerScript.Instance.AllCharacterList[CID].OBJname, "prefab", (object OBJ) =>
			{
				//インスタンス化してListに追加
				CharacterList[CID] = Instantiate(OBJ as GameObject);

				//入力を無効化
				CharacterList[CID].GetComponent<PlayerInput>().enabled = false;

				//キャラクターセッティングを無効化
				CharacterList[CID].GetComponent<CharacterSettingScript>().enabled = false;

				//レンダラー有効化
				foreach (var ii in CharacterList[CID].GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					ii.enabled = true;
				}

				//目線をカメラ目線にする
				CharacterList[CID].GetComponentInChildren<CharacterEyeShaderScript>().CameraEyeFlag = true;

				//アニメーター有効
				CharacterList[CID].GetComponent<Animator>().enabled = true;

				//編集中キャラクターをアクティブキャラクターに設定
				if (CharacterID == CID)
				{
					GameManagerScript.Instance.SetPlayableCharacterOBJ(CharacterList[CID]);
				}
				//編集中ではないキャラクターを非表示
				else
				{
					CharacterList[CID].SetActive(false);
				}

				//全ての髪オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Character/" + CID + "/Hair/", "prefab", (List<object> list) =>
				{
					//読み込んだオブジェクトを回す
					foreach (var ii in list)
					{
						//読み込んだオブジェクトをインスタンス化
						GameObject TempHairOBJ = Instantiate(ii as GameObject);

						//頭ボーンの子にする
						TempHairOBJ.transform.parent = DeepFind(CharacterList[CID], "HeadBone").transform;

						//ローカルtransformをゼロに
						ResetTransform(TempHairOBJ);

						//髪のダイナミックボーンに使うコライダを全て取得
						foreach (DynamicBoneCollider iii in TempHairOBJ.GetComponentsInChildren<DynamicBoneCollider>())
						{
							//名前で判別してキャラクターのボーンの子にする
							if (iii.name.Contains("Neck"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "NeckBone").transform;
							}
							else if (iii.name.Contains("Spine02"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "SpineBone.002").transform;
							}
							else if (iii.name.Contains("L_") && iii.name.Contains("Shoulder"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "L_ShoulderBone").transform;
							}
							else if (iii.name.Contains("R_") && iii.name.Contains("Shoulder"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "R_ShoulderBone").transform;
							}

							//相対位置と回転をゼロにする
							ResetTransform(iii.gameObject);
						}

						//髪のクロスに使うSphereColliderを全て取得
						foreach (SphereCollider iii in TempHairOBJ.GetComponentsInChildren<SphereCollider>())
						{
							//名前で判別してキャラクターのボーンの子にする
							if (iii.name.Contains("Spine02"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "SpineBone.002").transform;
							}
							else if (iii.name.Contains("Neck"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "NeckBone").transform;
							}
							else if (iii.name.Contains("L_") && iii.name.Contains("Shoulder"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "L_ShoulderBone").transform;
							}
							else if (iii.name.Contains("R_") && iii.name.Contains("Shoulder"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "R_ShoulderBone").transform;
							}
							else if (iii.name.Contains("L_") && iii.name.Contains("Breast"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "L_BreastBone").transform;
							}
							else if (iii.name.Contains("R_") && iii.name.Contains("Breast"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "R_BreastBone").transform;
							}
							else if (iii.name.Contains("L_") && iii.name.Contains("Nipple"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "L_NippleBone").transform;
							}
							else if (iii.name.Contains("R_") && iii.name.Contains("Nipple"))
							{
								iii.transform.parent = DeepFind(CharacterList[CID], "R_NippleBone").transform;
							}

							//相対位置と回転をゼロにする
							ResetTransform(iii.gameObject);
						}
					}

					//髪型読み込み完了フラグを立てる
					HairLoadFlag = true;

				}));

				//全ての衣装オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Character/" + CID + "/Costume/", "prefab", (List<object> list) =>
				{
					//読み込んだオブジェクトを回す
					foreach (var O in list)
					{
						//要素数を確保
						CostumeList[CID].Add(null);

						//読み込んだ衣装オブジェクトをインスタンス化
						GameObject TempCostumeOBJ = Instantiate(O as GameObject);

						//名前からCloneを消す
						TempCostumeOBJ.name = TempCostumeOBJ.name.Replace("(Clone)", "");

						//読み込んだ衣装オブジェクトをListに入れる
						//CostumeList[CID][int.Parse(TempCostumeOBJ.name.Replace("Costume_" + CID + "_", ""))] = TempCostumeOBJ;
						CostumeList[CID][CostumeList[CID].Count - 1] = TempCostumeOBJ;

						//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
						SkinnedMeshRenderer CostumeRenderer = DeepFind(CharacterList[CID], "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

						//ローカルトランスフォームをリセット
						ResetTransform(TempCostumeOBJ);

						//衣装を子にする
						TempCostumeOBJ.transform.parent = CharacterList[CID].transform;

						//ローカルトランスフォームをリセット
						ResetTransform(TempCostumeOBJ);

						//衣装プレハブ内のスキニングメッシュレンダラーを全て取得
						foreach (SkinnedMeshRenderer ii in TempCostumeOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
						{
							//ボーン構成をコピーしてキャラクターのボーンと紐付ける
							ii.bones = CostumeRenderer.bones;

							//脱がされていない状態のモデルを表示
							if (!ii.gameObject.name.Contains("Off_"))
							{
								ii.enabled = true;
							}
						}

						//衣装用ボーンセッティング関数呼び出し
						SetCostumeCol(CharacterList[CID], TempCostumeOBJ);

						//一旦無効化しておく
						TempCostumeOBJ.SetActive(false);
					}

					//装備中の衣装を有効化
					CostumeList[CID][GameManagerScript.Instance.AllCharacterList[CID].CostumeID].SetActive(true);

					//衣装読み込み完了フラグを立てる
					CostumeLoadFlag = true;

				}));

				//全ての下着オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Character/" + CID + "/UnderWear/", "prefab", (List<object> list) =>
				{
					//読み込んだオブジェクトを回す
					foreach (var O in list)
					{
						//要素数を確保
						UnderWearList[CID].Add(null);

						//読み込んだ下着オブジェクトをインスタンス化
						GameObject TempUnderWearOBJ = Instantiate(O as GameObject);

						//名前からCloneを消す
						TempUnderWearOBJ.name = TempUnderWearOBJ.name.Replace("(Clone)", "");

						//読み込んだ下着オブジェクトをListに入れる
						//UnderWearList[CID][int.Parse(TempUnderWearOBJ.name.Replace("UnderWear_" + CID + "_", ""))] = TempUnderWearOBJ;
						UnderWearList[CID][UnderWearList[CID].Count - 1] = TempUnderWearOBJ;

						//Bodyに仕込んであるUnderWearのSkinnedMeshRendererを取得する
						SkinnedMeshRenderer UnderWearRenderer = DeepFind(CharacterList[CID], "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

						//ローカルトランスフォームをリセット
						ResetTransform(TempUnderWearOBJ);

						//衣装を子にする
						TempUnderWearOBJ.transform.parent = CharacterList[CID].transform;

						//ローカルトランスフォームをリセット
						ResetTransform(TempUnderWearOBJ);

						//衣装プレハブ内のスキニングメッシュレンダラーを全て取得
						foreach (SkinnedMeshRenderer ii in TempUnderWearOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
						{
							//ボーン構成をコピーしてキャラクターのボーンと紐付ける
							ii.bones = UnderWearRenderer.bones;

							//脱がされていない状態のモデルを表示
							if (!ii.gameObject.name.Contains("Off_"))
							{
								ii.enabled = true;
							}
						}

						//一旦無効化しておく
						TempUnderWearOBJ.SetActive(false);
					}

					//装備中の衣装を有効化
					UnderWearList[CID][GameManagerScript.Instance.AllCharacterList[CID].UnderWearID].SetActive(true);

					//衣装読み込み完了フラグを立てる
					UnderWearLoadFlag = true;

				}));


				//全ての武器オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Character/" + CID + "/Weapon/", "prefab", (List<object> list) =>
				{
					//読み込んだオブジェクトを回す
					foreach (var O in list)
					{
						//要素数を確保
						WeaponList[CID].Add(null);

						//読み込んだ武器オブジェクトをインスタンス化
						GameObject TempWeaponOBJ = Instantiate(O as GameObject);

						//読み込んだ武器オブジェクトをListに入れる
						WeaponList[CID][WeaponList[CID].Count - 1] = TempWeaponOBJ;

						//キャラクターオブジェクトの子にする
						TempWeaponOBJ.transform.parent = CharacterList[CID].transform;

						//ローカルトランスフォームをリセット
						ResetTransform(TempWeaponOBJ);

						//一旦無効化しておく
						TempWeaponOBJ.SetActive(false);
					}

					//装備中の武器を有効化
					WeaponList[CID][GameManagerScript.Instance.AllCharacterList[CID].WeaponID].SetActive(true);

					//武器読み込み完了フラグを立てる
					WeaponLoadFlag = true;

				}));

				//キャラクター読み込み完了フラグを立てる
				CharacterLoadFlag = true;

			}));

			//読み込みが終わるまで待つ
			while (!CharacterLoadFlag && !HairLoadFlag && !CostumeLoadFlag && !UnderWearLoadFlag && !WeaponLoadFlag)
			{
				yield return null;
			}

			//準備完了フラグを入れる
			CharacterList[CID].GetComponent<PlayerScript>().AllReadyFlag = true;
		}

		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 2, new Color(1, 1, 1, 1), 1, (GameObject g) => 
		{
			//自身を描画を切る
			g.GetComponent<Renderer>().enabled = false;

			//メニューモードをメインメニューに
			CurrentMode = CurrentModeEnum.MainMenu;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("MainMenu_Show", true);

			//カメラワーク再生
			CameraWorkOBJ.GetComponent<CinemachineCameraScript>().PlayCameraWork(0, true);

		}));
	}


	void Update()
    {

    }

	//スタートボタンがSubmitされた時の処理
	public void MissionStartSubmit()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下す
			InputReadyFlag = false;

			//オートセーブ関数呼び出し
			GameManagerScript.Instance.AutoSave(() => 
			{
				//処理が終わったらスクリーンエフェクトで白フェード
				ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(false, 2, new Color(1, 1, 1, 1), 1, (GameObject g) =>
				{
					//フェードが終わったらシーン遷移関数呼び出し
					NextScene("Scene10_Mission");
				}));
			});
		}
	}

	//オプションがSubmitされた時の処理
	public void OptionSubmit()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをオプションに
			CurrentMode = CurrentModeEnum.Option;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("MainMenu_Show", false);
			UIAnim.SetBool("MainMenu_Vanish", true);
			UIAnim.SetBool("Option_Show", true);
			UIAnim.SetBool("Option_Vanish", false);

			//カメラワーク再生
			CameraWorkOBJ.GetComponent<CinemachineCameraScript>().ForceCameraWorkChange(3);
		}
	}

	//フルスクリーンモードがSubmitされた時の処理
	public void FullScreenSubmit()
	{
		ChangeResolution(true, GameManagerScript.Instance.UserData.Reso);
	}

	//ウィンドウモードがSubmitされた時の処理
	public void WindowSubmit()
	{
		ChangeResolution(false, GameManagerScript.Instance.UserData.Reso);
	}

	//各解像度がSubmitされた時の処理
	public void HiResSubmit()
	{
		ChangeResolution(GameManagerScript.Instance.UserData.FullScreen, 0);
	}
	public void MidResSubmit()
	{
		ChangeResolution(GameManagerScript.Instance.UserData.FullScreen, 1);
	}
	public void LowResSubmit()
	{
		ChangeResolution(GameManagerScript.Instance.UserData.FullScreen, 2);
	}

	//オプションでCancelされた時の処理
	public void OptionCancel()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをメインメニューに
			CurrentMode = CurrentModeEnum.MainMenu;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Option_Vanish", true);
			UIAnim.SetBool("Option_Show", false);
			UIAnim.SetBool("MainMenu_Show", true);
			UIAnim.SetBool("MainMenu_Vanish", false);
		}
	}

	//カスタマイズがSubmitされた時の処理
	public void CustomizeSubmit()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをカスタマイズに
			CurrentMode = CurrentModeEnum.Customize;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("MainMenu_Show", false);
			UIAnim.SetBool("MainMenu_Vanish", true);
			UIAnim.SetBool("Customize_Show", true);
			UIAnim.SetBool("Customize_Vanish", false);

			//カメラワーク再生
			CameraWorkOBJ.GetComponent<CinemachineCameraScript>().ForceCameraWorkChange(3);
		}	
	}

	//カスタマイズでCancelされた時の処理
	public void CustomizeCancel()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをメインメニューに
			CurrentMode = CurrentModeEnum.MainMenu;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", true);
			UIAnim.SetBool("Customize_Show", false);
			UIAnim.SetBool("MainMenu_Show", true);
			UIAnim.SetBool("MainMenu_Vanish", false);

			//カメラワーク再生
			CameraWorkOBJ.GetComponent<CinemachineCameraScript>().KeepCameraFlag = false;
		}
	}

	//技装備がSubmitされた時の処理
	public void ArtsEquipSubmit()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードを技装備に
			CurrentMode = CurrentModeEnum.Arts;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", true);
			UIAnim.SetBool("Customize_Show", false);
			UIAnim.SetBool("ArtsEquip_Show", true);
			UIAnim.SetBool("ArtsEquip_Vanish", false);

			//装備技List生成
			ArtsEquipListReset(CharacterID);
		}
	}

	//技装備でCancelされた時の処理
	public void ArtsEquipCancel()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをカスタマイズに
			CurrentMode = CurrentModeEnum.Customize;

			//技装備更新
			ArtsMatrixUpdate(CharacterID);		

			//選択技解除
			SelectedArtsOBJ = null;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", false);
			UIAnim.SetBool("Customize_Show", true);
			UIAnim.SetBool("ArtsEquip_Show", false);
			UIAnim.SetBool("ArtsEquip_Vanish", true);
		}
	}

	//衣装装備がSubmitされた時の処理
	public void CostumeSubmit()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードを衣装装備に
			CurrentMode = CurrentModeEnum.Costume;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", true);
			UIAnim.SetBool("Customize_Show", false);
			UIAnim.SetBool("Costume_Show", true);
			UIAnim.SetBool("Costume_Vanish", false);

			//衣装装備List生成
			CostumeListReset(CharacterID);
		}
	}

	//衣装装備でCancelされた時の処理
	public void CostumeCancel()
	{
		if (InputReadyFlag)
		{
			//入力許可フラグを下ろす
			InputReadyFlag = false;

			//メニューモードをカスタマイズに
			CurrentMode = CurrentModeEnum.Customize;

			//アニメーターのフラグを立てる
			UIAnim.SetBool("Customize_Vanish", false);
			UIAnim.SetBool("Customize_Show", true);
			UIAnim.SetBool("Costume_Show", false);
			UIAnim.SetBool("Costume_Vanish", true);

			//装備中の衣装表示
			CostumeList[CharacterID][GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].SetActive(true);

			//装備中の下着表示
			UnderWearList[CharacterID][GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].SetActive(true);
		}
	}

	//衣装装備でSelectされた時の処理
	public void CostumeSelect()
	{
		if (InputReadyFlag)
		{
			//装備中の衣装非表示
			CostumeList[CharacterID][GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].SetActive(false);

			//選択された衣装をユーザーデータに反映
			GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID] = CostumeSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);
			GameManagerScript.Instance.AllCharacterList[CharacterID].CostumeID = CostumeSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);

			//選択された衣装を表示
			CostumeList[CharacterID][CostumeSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject)].SetActive(true);

			//下着リストのボタンにナビゲータを仕込む
			foreach (GameObject o in UnderWearSelectButtonList)
			{
				//ナビゲーションのアクセサ取得
				Navigation tempnavi = o.GetComponent<Button>().navigation;

				//左を押した時の処理
				tempnavi.selectOnLeft = CostumeSelectButtonList[GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].GetComponent<Button>();

				//アクセサを反映
				o.GetComponent<Button>().navigation = tempnavi;
			}
		}
	}

	//下着装備でSelectされた時の処理
	public void UnderWearSelect()
	{
		if (InputReadyFlag)
		{
			//装備中の衣装非表示
			CostumeList[CharacterID][GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].SetActive(false);

			//装備中の下着非表示
			UnderWearList[CharacterID][GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].SetActive(false);

			//選択された下着をユーザーデータに反映
			GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID] = UnderWearSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);
			GameManagerScript.Instance.AllCharacterList[CharacterID].UnderWearID = UnderWearSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);

			//選択された下着を表示
			UnderWearList[CharacterID][UnderWearSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject)].SetActive(true);

			//衣装リストのボタンにナビゲータを仕込む
			foreach (GameObject o in CostumeSelectButtonList)
			{
				//ナビゲーションのアクセサ取得
				Navigation tempnavi = o.GetComponent<Button>().navigation;

				//右を押した時の処理
				tempnavi.selectOnRight = UnderWearSelectButtonList[GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].GetComponent<Button>();

				//アクセサを反映
				o.GetComponent<Button>().navigation = tempnavi;
			}

			//武器リストのボタンにナビゲータを仕込む
			foreach (GameObject o in WeaponSelectButtonList)
			{
				//ナビゲーションのアクセサ取得
				Navigation tempnavi = o.GetComponent<Button>().navigation;

				//左を押した時の処理
				tempnavi.selectOnLeft = UnderWearSelectButtonList[GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].GetComponent<Button>();

				//アクセサを反映
				o.GetComponent<Button>().navigation = tempnavi;
			}
		}
	}

	//武器装備でSelectされた時の処理
	public void WeaponSelect()
	{
		if (InputReadyFlag)
		{
			//装備中の武器非表示
			WeaponList[CharacterID][GameManagerScript.Instance.UserData.EquipWeaponList[CharacterID]].SetActive(false);

			//選択された武器をユーザーデータに反映
			GameManagerScript.Instance.UserData.EquipWeaponList[CharacterID] = WeaponSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);
			GameManagerScript.Instance.AllCharacterList[CharacterID].WeaponID = WeaponSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject);

			//選択された武器を表示
			WeaponList[CharacterID][WeaponSelectButtonList.IndexOf(EventSystemUI.currentSelectedGameObject)].SetActive(true);

			//選択されている衣装を表示
			CostumeList[CharacterID][GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].SetActive(true);

			//下着リストのボタンにナビゲータを仕込む
			foreach (GameObject o in UnderWearSelectButtonList)
			{
				//ナビゲーションのアクセサ取得
				Navigation tempnavi = o.GetComponent<Button>().navigation;

				//右を押した時の処理
				tempnavi.selectOnRight = WeaponSelectButtonList[GameManagerScript.Instance.UserData.EquipWeaponList[CharacterID]].GetComponent<Button>();

				//アクセサを反映
				o.GetComponent<Button>().navigation = tempnavi;
			}
		}
	}


	//汎用ボタンが押された時の処理
	public void OnGeneral()
	{
		if (InputReadyFlag)
		{
			//装備技マトリクスが選択されている
			if (ArtsMatrixButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
			{
				//装備解除
				EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text = "";

				//選択技解除
				SelectedArtsOBJ = null;
			}
		}
	}

	//スティックが入力された時の処理、イベントシステムやナビゲーションでフォローできない処理を書く
	public void OnNavigate(InputValue input)
	{
		//左が押された
		if (input.Get<Vector2>().x < -0.75f)
		{
			//技マトリクスボタンを選択しているか判別
			if (ArtsMatrixButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
			{
				//技マトリクスの左端の列で左が押されたら技リストに戻る
				if (EventSystemUI.currentSelectedGameObject.GetComponent<Button>().navigation.selectOnLeft == null)
				{
					//リストに戻る処理
					ArtsListBack();
				}
			}
		}
	}

	//アクティブ切り替えボタンが押された時の処理
	public void OnChangeActive(InputValue input)
	{
		if (InputReadyFlag && 
			(
				CurrentMode == CurrentModeEnum.Customize || 
				CurrentMode == CurrentModeEnum.Arts || 
				CurrentMode == CurrentModeEnum.Costume
			))
		{

			if (CurrentMode == CurrentModeEnum.Arts)
			{
				//切り替え前のキャラクターの技装備更新
				ArtsMatrixUpdate(CharacterID);
			}

			//装備中の衣装表示
			CostumeList[CharacterID][GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].SetActive(true);

			//装備中の下着表示
			UnderWearList[CharacterID][GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].SetActive(true);

			//装備中の武器表示
			WeaponList[CharacterID][GameManagerScript.Instance.UserData.EquipWeaponList[CharacterID]].SetActive(true);

			//表示キャラクター非表示
			CharacterList[CharacterID].SetActive(false);

			//キャラクターID切り替え
			CharacterID += Mathf.RoundToInt(input.Get<float>());

			if (CharacterID == -1)
			{
				CharacterID = GameManagerScript.Instance.AllCharacterList.Count - 1;
			}
			else if (CharacterID == GameManagerScript.Instance.AllCharacterList.Count)
			{
				CharacterID = 0;
			}

			//表示キャラクター表示
			CharacterList[CharacterID].SetActive(true);

			if(CurrentMode == CurrentModeEnum.Arts)
			{
				//技リストと技マトリクスを更新する関数呼び出し
				ArtsEquipListReset(CharacterID);
			}
			else if(CurrentMode == CurrentModeEnum.Costume)
			{
				CostumeListReset(CharacterID);
			}
		}
	}

	//技リスト、技マトリクスでSubmitされた時の処理
	public void SelectedArtsSubmit()
	{
		//選択中の技オブジェクトなし
		if (SelectedArtsOBJ == null)
		{
			//技オブジェクトを取得
			SelectedArtsOBJ = EventSystemUI.currentSelectedGameObject;

			//技リストボタンを選択しているか判別
			if (ArtsSelectButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
			{
				//選択した技のロケーション属性を取得
				int n = GameManagerScript.Instance.AllArtsList.Where(a => a.NameC == EventSystemUI.currentSelectedGameObject.GetComponentInChildren<Text>().text).ToList()[0].LocationFlag;

				//選択した技のロケーションによって技マトリクスの選択先を切り替え
				EventSystemUI.SetSelectedGameObject(GameObject.Find("EquipArtsButton" + n + "00"));
			}
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
			if (ArtsMatrixButtonList.Any(a => a == SelectedArtsOBJ))
			{
				//マトリクス側がターゲットされている
				if (ArtsMatrixButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
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
						foreach (GameObject i in ArtsMatrixButtonList)
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
				if (ArtsMatrixButtonList.Any(a => a == EventSystemUI.currentSelectedGameObject))
				{
					EquipFlag = AirArtsCheck(SelectedArtsOBJ, EventSystemUI.currentSelectedGameObject);					

					//装備
					if (EquipFlag)
					{
						//すでに装備していたら外す
						foreach (GameObject i in ArtsMatrixButtonList)
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

	//技マトリクスから技リストに戻る時の処理
	public void ArtsListBack()
	{
		//選択中の技をリセット
		SelectedArtsOBJ = null;

		//現在のスクロールバーの位置
		float BarPos = ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition;

		//見えない部分全体のサイズ
		float ArtsSelectButtonContentSize = ArtsSelectButtonContentOBJ.GetComponent<RectTransform>().sizeDelta.y;

		//見えている部分のサイズ
		float ArtsSelectScrollSize = ArtsSelectScrollBarOBJ.GetComponent<RectTransform>().sizeDelta.y;

		//現在の下座標
		float WindowBottomPos = ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition * (ArtsSelectButtonContentSize - ArtsSelectScrollSize);

		//ボタンのサイズ
		float ButtonSize = ArtsSelectButtonOBJ.GetComponent<RectTransform>().rect.height + ArtsSelectButtonContentOBJ.GetComponent<VerticalLayoutGroup>().spacing;

		//見えているボタンの数
		int ShowButtonCount = (int) Mathf.Round(ArtsSelectScrollSize / ButtonSize);

		//下に隠れているボタンの数
		int HiddenButtonCount = (int)Mathf.Round(WindowBottomPos / ButtonSize);

		//ボタンの数とかでインデックスを選定して選択ボタンを切り替え
		EventSystemUI.SetSelectedGameObject(ArtsSelectButtonList[ArtsSelectButtonList.Count - (HiddenButtonCount + ShowButtonCount)]);

		//技リストで選択変更された時の処理実行
		ArtsEquipMove();
	}

	//装備可能かチェックする関数
	private bool AirArtsCheck(GameObject arts,GameObject target)
	{
		//技名取得
		string artsname = arts.GetComponentInChildren<Text>().text;

		//ターゲットされている場所の名前からロケーション部分を抽出
		int TargetArtsNum = int.Parse(target.name.Skip(15).Take(1).ToList()[0].ToString());

		//出力用変数宣言
		bool re = false;

		//全ての技を回す
		foreach (ArtsClass i in GameManagerScript.Instance.AllArtsList)
		{
			//名前で検索、フラグを比較してboolを返す
			if (i.NameC == artsname)
			{
				//ロケーションが一致しているなら装備可能
				if(i.LocationFlag == TargetArtsNum)
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

	//衣装リストを更新する関数
	private void CostumeListReset(int ID)
	{
		//衣装リストを初期化
		foreach (var i in CostumeSelectButtonList)
		{
			Destroy(i);
		}

		//下着リストを初期化
		foreach (var i in UnderWearSelectButtonList)
		{
			Destroy(i);
		}

		//武器リストを初期化
		foreach (var i in WeaponSelectButtonList)
		{
			Destroy(i);
		}

		//衣装ボタンリスト初期化
		CostumeSelectButtonList = new List<GameObject>();

		//下着ボタンリスト初期化
		UnderWearSelectButtonList = new List<GameObject>();

		//武器ボタンリスト初期化
		WeaponSelectButtonList = new List<GameObject>();

		//ひな型ボタンのRect取得
		RectTransform Temprect = CostumeSelectButtonOBJ.GetComponent<RectTransform>();

		//ループカウント宣言
		int count = 0;

		//全ての衣装を回す
		foreach (CostumeClass i in GameManagerScript.Instance.AllCostumeList)
		{
			//引数で仕様キャラクターを判別、アンロックされているかを判別
			if (i.CharacterID == ID)
			{
				//ボタンのインスタンス生成
				GameObject TempButton = Instantiate(CostumeSelectButtonOBJ);

				//親を設定
				TempButton.GetComponent<RectTransform>().SetParent(CostumeSelectButtonContentOBJ.transform, false);

				//位置を設定
				TempButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(Temprect.anchoredPosition.x, Temprect.anchoredPosition.y - (Temprect.rect.height * count));

				//テキストに技名を入れる
				TempButton.GetComponentInChildren<Text>().text = i.CostumeName;

				//ボタンの名前を連番にする
				TempButton.name = ID + CostumeSelectButtonOBJ.name + count;

				//ListにAdd
				CostumeSelectButtonList.Add(TempButton);

				//ループカウントアップ
				count++;
			}
		}

		//ループカウントリセット
		count = 0;

		//ひな型ボタンのRect取得
		Temprect = UnderWearSelectButtonOBJ.GetComponent<RectTransform>();

		//全ての下着を回す
		foreach (UnderWearClass i in GameManagerScript.Instance.AllUnderWearList)
		{
			//引数で仕様キャラクターを判別、アンロックされているかを判別
			if (i.CharacterID == ID)
			{
				//ボタンのインスタンス生成
				GameObject TempButton = Instantiate(UnderWearSelectButtonOBJ);

				//親を設定
				TempButton.GetComponent<RectTransform>().SetParent(UnderWearSelectButtonContentOBJ.transform, false);

				//位置を設定
				TempButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(Temprect.anchoredPosition.x, Temprect.anchoredPosition.y - (Temprect.rect.height * count));

				//テキストに技名を入れる
				TempButton.GetComponentInChildren<Text>().text = i.UnderWearName;

				//ボタンの名前を連番にする
				TempButton.name = ID + UnderWearSelectButtonOBJ.name + count;

				//ListにAdd
				UnderWearSelectButtonList.Add(TempButton);

				//ループカウントアップ
				count++;
			}
		}

		//ループカウントリセット
		count = 0;

		//ひな型ボタンのRect取得
		Temprect = WeaponSelectButtonOBJ.GetComponent<RectTransform>();

		//全ての武器を回す
		foreach (WeaponClass i in GameManagerScript.Instance.AllWeaponList)
		{
			//引数で仕様キャラクターを判別、アンロックされているかを判別
			if (i.CharacterID == ID)
			{
				//ボタンのインスタンス生成
				GameObject TempButton = Instantiate(WeaponSelectButtonOBJ);

				//親を設定
				TempButton.GetComponent<RectTransform>().SetParent(WeaponSelectButtonContentOBJ.transform, false);

				//位置を設定
				TempButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(Temprect.anchoredPosition.x, Temprect.anchoredPosition.y - (Temprect.rect.height * count));

				//テキストに技名を入れる
				TempButton.GetComponentInChildren<Text>().text = i.WeaponName;

				//ボタンの名前を連番にする
				TempButton.name = ID + WeaponSelectButtonOBJ.name + count;

				//ListにAdd
				WeaponSelectButtonList.Add(TempButton);

				//ループカウントアップ
				count++;
			}
		}

		//ループカウントリセット
		count = 0;

		//衣装リストのボタンにナビゲータを仕込む
		foreach (GameObject o in CostumeSelectButtonList)
		{
			//ナビゲーションのアクセサ取得
			Navigation tempnavi = o.GetComponent<Button>().navigation;

			//上を押した時の処理
			if (count == 0)
			{
				tempnavi.selectOnUp = DeepFind(CostumeSelectButtonContentOBJ, ID + CostumeSelectButtonOBJ.name + (CostumeSelectButtonList.Count - 1)).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnUp = DeepFind(CostumeSelectButtonContentOBJ, ID + CostumeSelectButtonOBJ.name + (count - 1)).GetComponent<Button>();
			}

			//下を押した時の選択先設定
			if (count == CostumeSelectButtonList.Count - 1)
			{
				tempnavi.selectOnDown = DeepFind(CostumeSelectButtonContentOBJ, ID + CostumeSelectButtonOBJ.name + 0).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnDown = DeepFind(CostumeSelectButtonContentOBJ, ID + CostumeSelectButtonOBJ.name + (count + 1)).GetComponent<Button>();
			}

			//右を押した時の処理
			tempnavi.selectOnRight = UnderWearSelectButtonList[GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].GetComponent<Button>();

			//アクセサを反映
			o.GetComponent<Button>().navigation = tempnavi;

			//ループカウントアップ
			count++;
		}

		//ループカウントリセット
		count = 0;

		//下着リストのボタンにナビゲータを仕込む
		foreach (GameObject o in UnderWearSelectButtonList)
		{
			//ナビゲーションのアクセサ取得
			Navigation tempnavi = o.GetComponent<Button>().navigation;
			
			//上を押した時の処理
			if (count == 0)
			{
				tempnavi.selectOnUp = DeepFind(UnderWearSelectButtonContentOBJ, ID + UnderWearSelectButtonOBJ.name + (UnderWearSelectButtonList.Count - 1)).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnUp = DeepFind(UnderWearSelectButtonContentOBJ, ID + UnderWearSelectButtonOBJ.name + (count - 1)).GetComponent<Button>();
			}

			//下を押した時の選択先設定
			if (count == UnderWearSelectButtonList.Count - 1)
			{
				tempnavi.selectOnDown = DeepFind(UnderWearSelectButtonContentOBJ, ID + UnderWearSelectButtonOBJ.name + 0).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnDown = DeepFind(UnderWearSelectButtonContentOBJ, ID + UnderWearSelectButtonOBJ.name + (count + 1)).GetComponent<Button>();
			}

			//左を押した時の処理
			tempnavi.selectOnLeft = CostumeSelectButtonList[GameManagerScript.Instance.UserData.EquipCostumeList[CharacterID]].GetComponent<Button>();

			//右を押した時の処理
			tempnavi.selectOnRight = WeaponSelectButtonList[GameManagerScript.Instance.UserData.EquipWeaponList[CharacterID]].GetComponent<Button>();


			//アクセサを反映
			o.GetComponent<Button>().navigation = tempnavi;

			//ループカウントアップ
			count++;
		}

		//ループカウントリセット
		count = 0;

		//武器リストのボタンにナビゲータを仕込む
		foreach (GameObject o in WeaponSelectButtonList)
		{
			//ナビゲーションのアクセサ取得
			Navigation tempnavi = o.GetComponent<Button>().navigation;

			//上を押した時の処理
			if (count == 0)
			{
				tempnavi.selectOnUp = DeepFind(WeaponSelectButtonContentOBJ, ID + WeaponSelectButtonOBJ.name + (WeaponSelectButtonList.Count - 1)).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnUp = DeepFind(WeaponSelectButtonContentOBJ, ID + WeaponSelectButtonOBJ.name + (count - 1)).GetComponent<Button>();
			}

			//下を押した時の選択先設定
			if (count == WeaponSelectButtonList.Count - 1)
			{
				tempnavi.selectOnDown = DeepFind(WeaponSelectButtonContentOBJ, ID + WeaponSelectButtonOBJ.name + 0).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnDown = DeepFind(WeaponSelectButtonContentOBJ, ID + WeaponSelectButtonOBJ.name + (count + 1)).GetComponent<Button>();
			}

			//左を押した時の処理
			tempnavi.selectOnLeft = UnderWearSelectButtonList[GameManagerScript.Instance.UserData.EquipUnderWearList[CharacterID]].GetComponent<Button>();

			//アクセサを反映
			o.GetComponent<Button>().navigation = tempnavi;

			//ループカウントアップ
			count++;
		}

		//選択ボタンを切り替え
		EventSystemUI.SetSelectedGameObject(CostumeSelectButtonList[GameManagerScript.Instance.AllCharacterList[CharacterID].CostumeID]);

		//スクロールの位置調整
		CostumeSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
		UnderWearSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
		WeaponSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
	}

	//技リストと技マトリクスを更新する関数
	private void ArtsEquipListReset(int ID)
	{
		//装備中の技表示用変数宣言
		int Location = 0;
		int Stick = 0;
		int Button = 0;

		//一度全部空白にする
		foreach(GameObject i in ArtsMatrixButtonList)
		{
			i.GetComponentInChildren<Text>().text = "";
		}

		//装備中の技をマトリクスに表示
		foreach (var i in GameManagerScript.Instance.UserData.ArtsMatrix[ID])
		{
			Stick = 0;

			foreach (var ii in i)
			{
				Button = 0;

				foreach (var iii in ii)
				{
					if (iii != null)
					{
						ArtsMatrixButtonList.Where(a => a.name == "EquipArtsButton" + Location + Stick + Button).ToList()[0].GetComponentInChildren<Text>().text = iii;
					}

					Button++;
				}

				Stick++;

			}

			Location++;
		}

		//ひな型ボタンのRect取得
		RectTransform Temprect = ArtsSelectButtonOBJ.GetComponent<RectTransform>();

		//リストを初期化
		foreach(var i in ArtsSelectButtonList)
		{
			Destroy(i);
		}

		ArtsSelectButtonList = new List<GameObject>();

		//ループカウント宣言
		int count = 0;

		//全ての技を回す
		foreach (ArtsClass i in GameManagerScript.Instance.AllArtsList)
		{
			//引数で仕様キャラクターを判別、アンロックされているかを判別
			if (i.UseCharacter == ID && GameManagerScript.Instance.UserData.ArtsUnLock.Any(a => a == i.NameC))
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
				TempButton.name = ID + ArtsSelectButtonOBJ.name + count;

				//ListにAdd
				ArtsSelectButtonList.Add(TempButton);

				//ループカウントアップ
				count++;
			}
		}

		//ループカウントリセット
		count = 0;

		//技リストのボタンにナビゲータを仕込む
		foreach (GameObject o in ArtsSelectButtonList)
		{
			//ナビゲーションのアクセサ取得
			Navigation tempnavi = o.GetComponent<Button>().navigation;

			//上を押した時の処理
			if (count == 0)
			{
				tempnavi.selectOnUp = DeepFind(ArtsSelectButtonContentOBJ, ID + ArtsSelectButtonOBJ.name + (ArtsSelectButtonList.Count - 1)).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnUp = DeepFind(ArtsSelectButtonContentOBJ, ID + ArtsSelectButtonOBJ.name + (count - 1)).GetComponent<Button>();
			}

			//下を押した時の選択先設定
			if (count == ArtsSelectButtonList.Count - 1)
			{
				tempnavi.selectOnDown = DeepFind(ArtsSelectButtonContentOBJ, ID + ArtsSelectButtonOBJ.name + 0).GetComponent<Button>();
			}
			else
			{
				tempnavi.selectOnDown = DeepFind(ArtsSelectButtonContentOBJ, ID + ArtsSelectButtonOBJ.name + (count+1)).GetComponent<Button>();				
			}

			//アクセサを反映
			o.GetComponent<Button>().navigation = tempnavi;

			//ループカウントアップ
			count++;
		}

		//選択ボタンを切り替え
		EventSystemUI.SetSelectedGameObject(ArtsSelectButtonList[0]);

		//スクロールの位置調整
		ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
	}

	//技リストで選択変更された時の処理
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

			//上にはみ出してる時の処理
			if (WindowTopPos < ButtonTopPos)
			{
				ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = (ButtonTopPos - ArtsSelectScrollSize) / (ArtsSelectButtonContentSize - ArtsSelectScrollSize);
			}
			//下にはみ出してる時の処理
			else if (ButtonBottomPos < WindowBottomPos)
			{
				ArtsSelectScrollBarOBJ.GetComponent<ScrollRect>().verticalNormalizedPosition = ButtonBottomPos / (ArtsSelectButtonContentSize - ArtsSelectScrollSize);
			}
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

		//衣装装備ボタンオブジェクト破棄
		foreach (GameObject i in CostumeSelectButtonList)
		{
			Destroy(i);
		}

		//下着リストを初期化
		foreach (var i in UnderWearSelectButtonList)
		{
			Destroy(i);
		}

		//武器リストを初期化
		foreach (var i in WeaponSelectButtonList)
		{
			Destroy(i);
		}

		//技装備ボタンList初期化
		ArtsSelectButtonList = new List<GameObject>();

		//衣装ボタンリスト初期化
		CostumeSelectButtonList = new List<GameObject>();

		//下着ボタンリスト初期化
		UnderWearSelectButtonList = new List<GameObject>();

		//武器ボタンリスト初期化
		WeaponSelectButtonList = new List<GameObject>();
	}

	//オプション有効、アニメーションクリップから呼ばれる
	public void OptionActive()
	{
		//選択状態のボタンを切り替え
		EventSystemUI.SetSelectedGameObject(GameObject.Find("FullScreenButton"));
	}

	//入力許可フラグを立てる、アニメーションクリップから呼ばれる
	public void InputReady()
	{
		InputReadyFlag = true;
	}

	//技装備更新
	private void ArtsMatrixUpdate(int c)
	{
		//装備技をリセット
		foreach (int i in Enumerable.Range(0, GameManagerScript.Instance.UserData.ArtsMatrix[c].Count))
		{
			foreach (int ii in Enumerable.Range(0, GameManagerScript.Instance.UserData.ArtsMatrix[c][i].Count))
			{
				foreach (int iii in Enumerable.Range(0, GameManagerScript.Instance.UserData.ArtsMatrix[c][i][ii].Count))
				{
					//対応キャラクターが合ってるか確認
					if(GameManagerScript.Instance.AllArtsList.Where(a => a.UseCharacter == c).Any(a => a.NameC == ArtsMatrixButtonList.Where(b => b.name == "EquipArtsButton" + i + ii + iii).ToList()[0].GetComponentInChildren<Text>().text))
					{
						GameManagerScript.Instance.UserData.ArtsMatrix[c][i][ii][iii] = "";

						GameManagerScript.Instance.UserData.ArtsMatrix[c][i][ii][iii] = ArtsMatrixButtonList.Where(a => a.name == "EquipArtsButton" + i + ii + iii).ToList()[0].GetComponentInChildren<Text>().text;
					}				
				}
			}
		}
	}
}

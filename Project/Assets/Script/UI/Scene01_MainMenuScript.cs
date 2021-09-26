using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scene01_MainMenuScript : GlobalClass
{
	//メニューに配置されているボタンList
	private List<GameObject> ButtonList = new List<GameObject>();

	//UIのイベントを受け取るイベントシステム
	EventSystem EventSystemUI;

	void Start()
    {
		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(true, 2, new Color(1, 1, 1, 1), 1, (GameObject g) => { g.GetComponent<Renderer>().enabled = false; }));

		//全てのボタンを取得
		foreach(RectTransform i in gameObject.GetComponentsInChildren<RectTransform>())
		{
			if(i.name.Contains("Button"))
			{
				ButtonList.Add(i.gameObject);
			}
		}

		//初期選択
		foreach(GameObject i in ButtonList)
		{
			if(i.name == "StartButton")
			{
				i.GetComponent<Button>().Select();
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
		print(EventSystemUI.currentSelectedGameObject.name);   
    }

	public void GameStart()
	{
		//スクリーンエフェクトで白フェード
		ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "ScreenEffect"), null, (reciever, eventData) => reciever.Fade(false, 2, new Color(1, 1, 1, 1), 1, (GameObject g) =>
		{
			//フェードが終わったらゲームマネージャーのシーン遷移関数呼び出し
			GameManagerScript.Instance.NextScene("Scene10_Mission");
		}));
	}

	private void OnSubmit()
	{

	}

	private void OnCancel()
	{

	}

	private void OnGeneral()
	{

	}

	private void OnNavigate()
	{

	}

	private void OnChangeActive()
	{

	}

	private void OnChangeMenu()
	{

	}
}

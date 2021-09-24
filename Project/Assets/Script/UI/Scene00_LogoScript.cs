using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Scene00_LogoScript : GlobalClass
{
	//アニメーターコントローラー
	private Animator AnimCon;

	//ロゴ回転速度
	private float RotateSpeed = 0;

	//スクリーンエフェクトオブジェクト
	private GameObject ScreenEffectOBJ;

	//次のシーンを読み込む処理を一度だけにするためのフラグ
	private bool NextSceneFlag = true;

	void Start()
    {
		//アニメーターコントローラー取得
		AnimCon = transform.GetComponent<Animator>();

		//スクリーンエフェクトオブジェクト取得
		ScreenEffectOBJ = GameObject.Find("MainCameraScreenEffect");
	}

	void Update()
    {
		//アニメーションが回転になり、回転速度が10以下
		if(AnimCon.GetCurrentAnimatorStateInfo(0).IsName("Logo01") && (AnimCon.GetFloat("RotateSpeed") < 10))
		{
			//回転速度を徐々に増やす
			RotateSpeed += Time.deltaTime;

			//アニメーターに回転速度を渡す
			AnimCon.SetFloat("RotateSpeed", RotateSpeed);
		}
		
		//ロゴの回転数が一定数以上になったらゲームデータの読み込みフラグを立てる
		if (AnimCon.GetFloat("RotateSpeed") > 1 && !GameManagerScript.Instance.LoadGameDataFlag)
		{
			GameManagerScript.Instance.LoadGameDataFlag = true;
		}

		//外部データの読み込み完了して回転速度が一定以上になった
		if (GameManagerScript.Instance.AllDetaLoadCompleteFlag && (AnimCon.GetFloat("RotateSpeed") > 1) && NextSceneFlag)
		{
			//フラグを下ろして処理を一度にする
			NextSceneFlag = false;

			//スクリーンエフェクトで白フェード
			ExecuteEvents.Execute<ScreenEffectScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject , "ScreenEffect") , null, (reciever, eventData) => reciever.Fade(false , 2 , new Color(1,1,1,1) , 1 , (GameObject g) => 
			{
				//フェードが終わったらゲームマネージャーのシーン遷移関数呼び出し
				GameManagerScript.Instance.NextScene("Scene10_Mission");			
			}));
		}
    }
}

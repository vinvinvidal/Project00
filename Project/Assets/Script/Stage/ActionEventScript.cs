using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionEventScript : GlobalClass
{
	//オブジェクト自身を動かすアニメーション
	Animation AcitionAnim;

	//プレイヤーキャラクター
	GameObject PlayerCharacter;

	//メインカメラ
	GameObject MainCamera;

	//野外用ライト
	GameObject OutDoorLight;

	//野外用ライト
	GameObject InDoorLight;

	//トレースカメラ用ヴァーチャルカメラ
	CinemachineVirtualCamera VCamera;

	//イベント実行タイミング
	string PlayTiming;

	//カメラかキャラか
	string HitOBJ;

	//プレイヤーイベントList
	public List<string> ActionEventList;
	public List<GameObject> ObjList;
	public List<float> FloatList;
	public List<AnimationClip> AnimList;
	public List<String> StringList;
	public List<String> PlayTimingList;
	public List<Color> ColorList;

	private void Start()
	{
		//アニメーション取得
		AcitionAnim = transform.parent.GetComponentInChildren<Animation>();

		//メインカメラ取得
		MainCamera = GameObject.Find("MainCamera");

		//野外用ライト取得
		OutDoorLight = GameObject.Find("OutDoorLight");

		//室内用ライト取得
		InDoorLight = GameObject.Find("InDoorLight");

		//トレースカメラ用ヴァーチャルカメラ取得
		if(DeepFind(gameObject , "vcam") != null)
		{
			VCamera = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
		}		
	}
	
	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		//プレイヤーキャラクター取得
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => PlayerCharacter = reciever.GetPlayableCharacterOBJ());

		//イベント実行タイミングに代入
		PlayTiming = "IN";

		//当たったオブジェクトを記録
		HitOBJ = LayerMask.LayerToName(ColHit.gameObject.layer);

		//イベント実行
		EventRun();
	}
	//プレイヤーがエリアから離脱したら呼ばれる関数
	private void OnTriggerExit(Collider ColHit)
	{
		//プレイヤーキャラクター取得
		ExecuteEvents.Execute<GameManagerScriptInterface>(GameManagerScript.Instance.gameObject, null, (reciever, eventData) => PlayerCharacter = reciever.GetPlayableCharacterOBJ());

		//イベント実行タイミングに代入
		PlayTiming = "OUT";

		//当たったオブジェクトを記録
		HitOBJ = LayerMask.LayerToName(ColHit.gameObject.layer);

		//イベント実行
		EventRun();
	}

	//アクション実行
	public void EventRun()
	{
		//PlayerEventListの要素数だけ回す
		for (int Count = 0; Count < ActionEventList.Count; Count++)
		{
			//イベント実行タイミング判定
			if(PlayTiming == PlayTimingList[Count])
			{
				if (HitOBJ == "Player")
				{
					//オブジェクトを表示する
					if (ActionEventList[Count] == "ActiveOBJ")
					{
						//リストに登録されているオブジェクトを有効化
						ObjList[Count].SetActive(true);
					}
					//オブジェクトを非表示にする
					else if (ActionEventList[Count] == "HiddenOBJ")
					{
						//リストに登録されているオブジェクトを無効化
						ObjList[Count].SetActive(false);
					}
					//オブジェクトをロードする
					else if (ActionEventList[Count] == "LoadOBJ")
					{
						//読み込むオブジェクトのパス、カンマでパスとファイル名を分ける
						string dir = StringList[Count].Split(',')[0];

						//読み込むオブジェクトのファイル名、カンマでパスとファイル名を分ける
						string file = StringList[Count].Split(',')[1];

						//すでに存在しているかチェック
						if (!GameObject.Find(file + "(Clone)"))
						{
							//オブジェクトロードコルーチン呼び出し
							StartCoroutine(GameManagerScript.Instance.LoadOBJ(dir, file, "prefab", (object OBJ) =>
							{
								//読み込んだオブジェクトをインスタンス化、名前から(Clone)を消す
								//Instantiate(OBJ as GameObject).name = (OBJ as GameObject).name;

								Instantiate(OBJ as GameObject);

							}));
						}
					}
					//オブジェクトを削除する
					else if (ActionEventList[Count] == "DestroyOBJ")
					{
						//すでに存在しているかチェック
						if (GameObject.Find(StringList[Count] + "(Clone)"))
						{
							//オブジェクト削除
							Destroy(GameObject.Find(StringList[Count] + "(Clone)"));
						}
					}
					//敵を出現させる
					else if (ActionEventList[Count] == "EnemySpawn")
					{
						//全ての敵リストからIDで検索
						EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => e.EnemyID == StringList[Count]).ToList()[0];

						//敵オブジェクトを読み込む
						StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Enemy/" + StringList[Count] + "/", tempclass.OBJname, "prefab", (object O) =>
						{
							//インスタンス生成
							GameObject tempenemy = Instantiate(O as GameObject);

							//座標を直で変更するためキャラクターコントローラを無効化
							tempenemy.GetComponent<CharacterController>().enabled = false;

							//座標をランダムにする
							tempenemy.transform.position = gameObject.transform.position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0.1f, UnityEngine.Random.Range(-5.0f, 5.0f));

							//キャラクターコントローラを有効化
							tempenemy.GetComponent<CharacterController>().enabled = true;
						}));
					}
					else if (ActionEventList[Count] == "TraceCamera")
					{
						//引数用のbool宣言
						bool b = StringList[Count] == "ON";

						//ヴァーチャルカメラにプレイヤーキャラクターを仕込む
						VCamera.Follow = PlayerCharacter.GetComponent<Transform>();
						VCamera.LookAt = PlayerCharacter.GetComponent<Transform>();

						//ヴァーチャルカメラ有効無効切り替え
						VCamera.GetComponent<CinemachineVirtualCamera>().enabled = b;

						//メインカメラのシネマシン有効無効切り替え
						MainCamera.GetComponent<CinemachineBrain>().enabled = b;

						//メインカメラの遷移をイージングにする
						MainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
					
						//遷移元のヴァーチャルカメラ有効化
						GameManagerScript.Instance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;

						//メインカメラのトランスフォームを遷移元のヴァーチャルカメラにコピー
						GameManagerScript.Instance.GetComponentInChildren<CinemachineVirtualCamera>().transform.position = MainCamera.transform.position;
						GameManagerScript.Instance.GetComponentInChildren<CinemachineVirtualCamera>().transform.rotation = MainCamera.transform.rotation;
						
						//ヴァーチャルカメラコルーチン呼び出し
						StartCoroutine(VcameraCoroutine());
					}
				}
				else if (HitOBJ == "MainCamera")
				{
					//ライトカラーを変更する
					if (ActionEventList[Count] == "LightColorChange" && OutDoorLight.GetComponent<Light>().color != ColorList[Count])
					{
						StartCoroutine(ChangeLightCoroutine(ColorList[Count]));
					}
				}
			}
		}
	}

	//ヴァーチャルカメラパス移動コルーチン
	private IEnumerator VcameraCoroutine()
	{
		//パストラッキング取得
		CinemachineTrackedDolly PathPos = VCamera.GetCinemachineComponent<CinemachineTrackedDolly>();

		//ウェイポイント取得
		CinemachinePath.Waypoint[] WayPoint = gameObject.GetComponentInChildren<CinemachinePath>().m_Waypoints;

		//遷移元のヴァーチャルカメラを１フレームだけ有効化して遷移させる
		yield return null;

		//遷移元のヴァーチャルカメラ無効化
		GameManagerScript.Instance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;

		//一番近いウェイポイントを選出するための距離
		float Dis = Mathf.Infinity;

		//ループカウント
		int count = 0;

		//ループカウントをキャッシュしてウェイポイントのインデックスにする
		int nearcount = 0;

		//パストラッキングのポジション
		float PathPoscount = PathPos.m_PathPosition;

		//パストラッキング移動速度
		float movespeed = 0;

		//ウェイポイントの座標
		Vector3 Waypos = Vector3.zero;

		//初っ端の直近ウェイポイントを探す
		foreach (var i in WayPoint)
		{
			//距離を測定、前回より近かったら処理
			if (Dis > (i.position - (PlayerCharacter.transform.position + Vector3.up - PlayerCharacter.transform.forward)).sqrMagnitude)
			{
				//距離更新
				Dis = (i.position - (PlayerCharacter.transform.position + Vector3.up - PlayerCharacter.transform.forward)).sqrMagnitude;

				//ループカウントキャッシュ
				nearcount = count;
			}

			//カウントアップ
			count++;
		}

		//みつかった直近ウェイポイントをトラッキングポジションにする
		PathPos.m_PathPosition = nearcount;

		//ヴァーチャルカメラが無効になるまでループ
		while (VCamera.enabled)
		{
			//距離初期化
			Dis = Mathf.Infinity;

			//ループカウント初期化
			count = 0;

			//トラッキングポジションをキャッシュ
			PathPoscount = PathPos.m_PathPosition;

			//直近ウェイポイントを探す
			foreach (var i in WayPoint)
			{
				//距離を測定、前回より近かったら処理
				if (Dis > (i.position - (PlayerCharacter.transform.position + Vector3.up)).sqrMagnitude)
				{
					//距離更新
					Dis = (i.position - (PlayerCharacter.transform.position + Vector3.up)).sqrMagnitude;

					//ループカウントキャッシュ
					nearcount = count;

					//ウェイポイントの座標キャッシュ
					Waypos = i.position;
				}

				//カウントアップ
				count++;
			}

			//移動速度算出、カメラとウェイポイントが離れているほど早くする
			movespeed = (Waypos - MainCamera.transform.position).sqrMagnitude * 0.05f;

			//直近のウェイポイントと現在のトラッキングポジションを比較して増減させる
			if (PathPos.m_PathPosition > nearcount)
			{
				PathPoscount -= movespeed * Time.deltaTime;
			}
			else
			{
				PathPoscount += movespeed * Time.deltaTime;
			}			

			//トラッキングポジション移動
			PathPos.m_PathPosition = PathPoscount;

			//１フレーム待機
			yield return null;
		}

		//ループが終わったらメインカメラのターゲットダミーの位置をメインカメラと同じにする
		GameObject.Find("MainCameraTarget").transform.position = MainCamera.transform.position;
	}

	//ライト変更コルーチン
	private IEnumerator ChangeLightCoroutine(Color c)
	{
		float time = Time.time;

		Color tempcolor = OutDoorLight.GetComponent<Light>().color;

		while (Time.time - time < 1)
		{
			tempcolor.r = Mathf.Lerp(tempcolor.r, c.r, Time.time - time);
			tempcolor.g = Mathf.Lerp(tempcolor.g, c.g, Time.time - time);
			tempcolor.b = Mathf.Lerp(tempcolor.b, c.b, Time.time - time);
			tempcolor.a = Mathf.Lerp(tempcolor.a, c.a, Time.time - time);

			OutDoorLight.GetComponent<Light>().color = tempcolor;
			//InDoorLight.GetComponent<Light>().color = tempcolor;
			yield return null;
		}		
	}

	//アニメーション再生、↑で呼んだプレイヤーのイベントアクション関数から呼ばれる
	public void EventAnim(AnimationClip Anim)
	{
		//引数で受け取ったAnimationClipをセット
		AcitionAnim.AddClip(Anim , Anim.name);

		//アニメーション再生
		AcitionAnim.Play(Anim.name);
	}
}

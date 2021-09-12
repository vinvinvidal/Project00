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
		MainCamera = GameObject.Find("CameraRoot");

		//野外用ライト取得
		OutDoorLight = GameObject.Find("OutDoorLight");

		//室内用ライト取得
		InDoorLight = GameObject.Find("InDoorLight");
	}

	//プレイヤーがエリアに進入したら呼ばれる関数
	private void OnTriggerEnter(Collider ColHit)
	{
		//プレイヤーキャラクター取得
		PlayerCharacter = ColHit.gameObject;

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
		PlayerCharacter = ColHit.gameObject;

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
				}
				else if (HitOBJ == "MainCamera")
				{
					//ライトカラーを変更する
					if (ActionEventList[Count] == "LightColorChange" && OutDoorLight.GetComponent<Light>().color != ColorList[Count])
					{
						StartCoroutine(ChangeLightCoroutine(ColorList[Count]));
					}
					else if(ActionEventList[Count] == "TraceCamera")
					{
						//引数用のbool宣言
						bool b = StringList[Count] == "ON";

						//カメラにフラグを送る
						ExecuteEvents.Execute<MainCameraScriptInterface>(MainCamera, null, (reciever, eventData) => reciever.TraceCameraMode(b));
					}
				}
			}
		}
	}

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

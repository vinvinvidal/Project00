using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//当たっていたらRayと障害物の反射ベクトルを取る
//MainCameraTargetPos = CameraRay.direction + 2 * (Vector3.Dot(-CameraRay.direction, CameraRayHit.normal)) * CameraRayHit.normal;

public interface MainCameraScriptInterface : IEventSystemHandler
{
	//引数でプレイヤーキャラクターを受け取る
	void SetPlayerCharacter(GameObject c);

	//受け取った座標が画角にあるか返す関数
	bool InCameraView(Vector3 pos);

	//引数でプレイヤーがロックしている敵を受け取る
	void SetLockEnemy(GameObject e);

	//引数で超必殺技演出フラグをを受け取る
	void SetSuperArtsFlag(bool b);

	//画面を揺らす
	void CameraShake(float t);

	//ミッション開始時にカメラの初期設定する
	void MissionCameraSetting();

	//メニューモードカメラの初期設定をする
	void MenuCameraSetting();

	//トレースカメラモード
	void TraceCameraMode(bool b);
}

public class MainCameraScript : GlobalClass, MainCameraScriptInterface
{

	//プレイヤーが操作するキャラクター
	private GameObject PlayerCharacter;

	//ミッションの管理をするマネージャー
	GameObject MissionManager;

	//メインカメラ本体
	private GameObject MainCamera;

	//メインカメラ本体のキャラクターコントローラ	
	private CharacterController CameraController;

	//メインカメラの移動目標になるターゲットダミーオブジェクト
	private GameObject MainCameraTargetOBJ;

	//メインカメラの移動目標になるターゲットダミーの位置
	private Vector3 MainCameraTargetPos;

	//メインカメラの移動目標になるターゲットダミーとプレイヤーとの距離
	public float MainCameraTargetDistance;

	//カメラ位置近さの限界
	public float CameraNearLimit;

	//カメラ位置遠さの限界
	public float CameraFarLimit;

	//カメラ位置低さの限界
	public float CameraDownLimit;

	//カメラ位置高さの限界、位置ではなく角度
	public float CameraUpLimit;

	//インプットシステムから入力されるカメラ移動値
	private Vector2 CameraMoveinputValue;

	//インプットシステムから入力されるカメラズーム値
	private float CameraZoominputValue;

	//カメラ移動速度
	public float CameraMoveSpeed;

	//トレースカメラ移動速度
	private float TraceCameraMoveSpeed = 1;

	//カメラ回転速度
	public float CameraRotateSpeed;

	//カメラの注視点座標に加えるオフセット値
	public Vector3 LookAtOffset;

	//カメラを向ける注視点座標
	private Vector3 LookAtPos;

	//カメラとキャラクターの間にある障害物をチェックするRay
	private Ray CameraRay;

	//カメラとキャラクターの間にある障害物をチェックするRayに当たったコライダ情報を格納
	private RaycastHit CameraRayHit;

	//視線が遮られた時の経過時間
	private float RayHitTimeCount = 0;

	//地面の属性をチェックするRayに当たったコライダ情報を格納
	private RaycastHit LocationRayHit;

	//ステージだけにRayを当てるためのレイヤーマスク
	public LayerMask RayMask;

	//屋外などのロケーション情報
	public string Location { get; set; }

	//コライダヒット情報
	private ControllerColliderHit ColHit = null;

	//プレイヤーが通過してきた場所を記録するList
	private List<Vector3> PlayerTraceList;

	//プレイヤーがロックしている敵オブジェクト
	private GameObject LockEnemy;

	//カメラ視線障害物フラグ
	private bool RayHitFlag;

	//カメラリセットフラグ
	private bool CameraResetFlag;

	//画面揺らしフラグ
	private bool CameraShakeFlag;

	//トレースカメラフラグ
	private bool TraceCameraFlag;

	//超必殺技フラグ
	private bool SuperArtsFlag;

	//クローズアップカメラフラグ
	private bool CloseUpCameraFlag;

	//カメラモード切り替えスイッチ
	private int CameraModeSwitch;
	/*
		0:メニューモード
		1:ミッションモード	
	*/
	void Start()
	{
		//ミッションの管理をするマネージャー取得
		MissionManager = transform.root.gameObject;

		//メインカメラ本体を取得
		MainCamera = DeepFind(transform.root.gameObject, "MainCamera");

		//メインカメラ本体のキャラクターコントローラ取得
		CameraController = MainCamera.GetComponent<CharacterController>();

		//メインカメラの移動目標になるターゲットダミーオブジェクトを取得
		MainCameraTargetOBJ = DeepFind(transform.root.gameObject, "MainCameraTarget");

		//カメラを向ける座標初期化
		LookAtPos = new Vector3();

		//プレイヤーがロックしている敵オブジェクト初期化
		LockEnemy = null;

		//インプットシステムから入力されるカメラ移動値初期化
		CameraMoveinputValue = Vector2.zero;

		//インプットシステムから入力されるカメラズーム値初期化
		CameraZoominputValue = 0.0f;

		//プレイヤーが通過してきた場所を記録するList初期化
		PlayerTraceList = new List<Vector3>();

		//カメラ視線障害物フラグ初期化
		RayHitFlag = false;

		//カメラリセットフラグ初期化
		CameraResetFlag = false;

		//トレースカメラフラグ初期化
		TraceCameraFlag = false;

		//クローズアップカメラフラグ初期化
		CloseUpCameraFlag = false;

		//超必殺技フラグ初期化
		SuperArtsFlag = false;

		//画面揺らしフラグ初期化
		CameraShakeFlag = false;

		//カメラモード切り替えスイッチ初期化
		CameraModeSwitch = 0;
	}

	private void LateUpdate()
	{
		//超必殺技中は処理しない
		if(!SuperArtsFlag)
		{
			//カメラモード切り替えスイッチ
			switch (CameraModeSwitch)
			{
				//メニューモード
				case 0:
					break;

				//ミッションモード
				case 1:

					//コライダ関係処理コルーチン呼び出し
					StartCoroutine(CameraColProcess());

					//注視点設定関数呼び出し
					LookAtPosSetting();

					//キャラクター関連の処理関数呼び出し
					CharacterProcess();

					//ターゲットダミー移動処理関数呼び出し
					TargetMove();

					//カメラ本体処理関数呼び出し
					CameraMove();

					//障害物回避処理
					if (RayHitFlag)
					{
						//カメラの速度を変更、トレースポイントが溜まっているほど早くする
						TraceCameraMoveSpeed = 0.1f * PlayerTraceList.Count;

						//経過時間カウントアップ
						RayHitTimeCount += Time.deltaTime;

						//経過時間が過ぎたらカメラがハマってるのでリセット
						if(RayHitTimeCount > 2)
						{
							//経過時間リセット
							RayHitTimeCount = 0;

							//カメラ位置リセット関数呼び出し
							OnCameraReset();
						}

						//プレイヤーの位置をトレースする関数呼び出し
						TracePlayer();
					}
					else
					{
						//視線が通った瞬間の処理
						if (PlayerTraceList.Count > 0)
						{
							//経過時間リセット
							RayHitTimeCount = 0;

							//プレイヤーの位置をトレースするList初期化
							PlayerTraceList = new List<Vector3>();

							//カメラの速度を戻す
							TraceCameraMoveSpeed = 1f;

							//現在のカメラ距離を測定して反映する
							MainCameraTargetDistance = Vector3.Distance(MainCamera.transform.position, PlayerCharacter.transform.position);
						}
					}

					break;
			}
		}
	}
	
	//注視点設定関数
	private void LookAtPosSetting()
	{
		//ロックしている敵がいる
		if (LockEnemy != null)
		{
			//注視点をキャラクターとロック対象の中間点に設定
			LookAtPos = ((LockEnemy.transform.position + PlayerCharacter.transform.position) * 0.5f) + LookAtOffset;
		}
		//通常処理
		else
		{
			//注視点をプレイヤーキャラクターに設定
			LookAtPos = PlayerCharacter.transform.position + LookAtOffset;
		}

		//画面揺らし処理、注視点をランダムに動かす
		if (CameraShakeFlag)
		{
			LookAtPos += new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
		}
	}

	//カメラ本体処理関数
	private void CameraMove()
	{
		//カメラリセット処理
		if (CameraResetFlag)
		{
			//カメラをプレイヤーの近くに移動
			MainCamera.transform.position = gameObject.transform.position + Vector3.up - (PlayerCharacter.transform.forward * 0.5f);

			//カメラを注視点に向ける
			MainCamera.transform.LookAt(LookAtPos);

			//距離を初期化
			MainCameraTargetDistance = 5;
		}
		//クローズアップカメラモード
		else if(CloseUpCameraFlag)
		{
			//何もしない
		}
		//通常処理
		else
		{
			//カメラ本体をターゲットダミー位置まで滑らかに移動させる
			CameraController.Move((MainCameraTargetOBJ.transform.position - MainCamera.transform.position) * Time.deltaTime * CameraMoveSpeed * TraceCameraMoveSpeed);

			//カメラ本体を注視点に滑らかに向ける
			MainCamera.transform.rotation = Quaternion.Slerp(MainCamera.transform.rotation, Quaternion.LookRotation(LookAtPos - MainCamera.transform.position), CameraRotateSpeed * Time.deltaTime);
		}
	}

	//ターゲットダミー移動処理関数
	private void TargetMove()
	{
		//ターゲットダミーを注視点に向ける
		MainCameraTargetOBJ.transform.LookAt(LookAtPos);

		//カメラリセット処理
		if (CameraResetFlag)
		{
			//ターゲットダミーをプレイヤー後方に移動
			MainCameraTargetOBJ.transform.position = PlayerCharacter.transform.position - (PlayerCharacter.transform.forward * CameraNearLimit) + new Vector3(0, 1.5f, 0);
		}
		//視線が通ってない
		else if (RayHitFlag)
		{
			//トレースポイントが２以上ある場合処理、カメラ操作で隠れた場合にガクガクさせないため
			if (PlayerTraceList.Count > 1)
			{
				//カメラターゲットダミーの位置を直接移動させる、トレースポイントの1から0までのベクトルを倍に延ばした位置
				MainCameraTargetOBJ.transform.position = (PlayerTraceList[0] + Vector3.up + ((PlayerTraceList[0] - PlayerTraceList[1]) * 2));
	
				//カメラがトレースポイントに接近したら消す
				if (Vector3.Distance(MainCamera.transform.position, PlayerTraceList[0] + Vector3.up + ((PlayerTraceList[0] - PlayerTraceList[1]) * 2)) < 1f)
				{
					PlayerTraceList.RemoveAt(0);
				}
			}
		}
		//通常カメラ処理
		else
		{
			//プレイヤーインプットから受け取った値をターゲットダミー移動座標に加える
			MainCameraTargetPos = (MainCameraTargetOBJ.transform.right * -CameraMoveinputValue.x) + (MainCameraTargetOBJ.transform.up * CameraMoveinputValue.y);

			//プレイヤーインプットから受け取ったズーム値を距離制限に入れる
			MainCameraTargetDistance -= CameraZoominputValue * CameraMoveSpeed * Time.deltaTime;
			
			//コライダがステージに触れている
			if (ColHit != null && LayerMask.LayerToName(ColHit.gameObject.layer) == "TransparentFX")
			{
				//カメラの向きと接触面の反射ベクトルにカメラを移動させる
				MainCameraTargetPos += -MainCamera.transform.forward + 2 * Vector3.Dot(MainCamera.transform.forward, ColHit.normal) * ColHit.normal;
			}
			//敵をロックしている
			else if (LockEnemy != null)
			{
				//画角に収まってるかbool
				bool OnCameraBool = true;

				//判定用ポジションList
				List<Vector3> PosList = new List<Vector3>();

				//判定用のポジションをAdd、キャラクターの周囲８点とエネミーの上下２点
				PosList.Add(PlayerCharacter.transform.position + new Vector3(0.25f, 0.25f, 0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(0.25f, 0.25f, -0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(-0.25f, 0.25f, 0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(-0.25f, 0.25f, -0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(0.25f,1.5f, 0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(0.25f,1.5f, -0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(-0.25f, 1.5f, 0.25f));
				PosList.Add(PlayerCharacter.transform.position + new Vector3(-0.25f, 1.5f, -0.25f));
				PosList.Add(LockEnemy.transform.position + new Vector3(0, 0.25f, 0));
				PosList.Add(LockEnemy.transform.position + new Vector3(0, LockEnemy.GetComponent<CharacterController>().height, 0));

				//全てのポジションを判定
				foreach (Vector3 i in PosList)
				{
					//１点でも出ていたらboolをfalseにしてループを抜ける
					if(!InCameraView(i))
					{
						OnCameraBool = false;

						goto LoopBreak;
					}
				}

				//ループを抜ける先
				LoopBreak:;

				//プレイヤーもしくはロックしている敵が画面外に出ている
				if (!OnCameraBool)
				{
					//プレイヤーの後方にターゲットダミー移動、高低差も取って上げ下げする
					MainCameraTargetPos += new Vector3(0, PlayerCharacter.transform.position.y - LockEnemy.transform.position.y, 0).normalized;

					//画角を取るためカメラを引く
					MainCameraTargetDistance += CameraMoveSpeed * 0.5f * Time.deltaTime;
				}
			}

			//距離制限
			if (MainCameraTargetDistance > CameraFarLimit)
			{
				MainCameraTargetDistance = CameraFarLimit;
			}
			else if (MainCameraTargetDistance < CameraNearLimit)
			{
				MainCameraTargetDistance = CameraNearLimit;
			}

			if ((transform.position - MainCameraTargetOBJ.transform.position).sqrMagnitude > Mathf.Pow(MainCameraTargetDistance + 0.01f, 2))
			{
				MainCameraTargetPos += MainCameraTargetOBJ.transform.forward;
			}
			else if ((transform.position - MainCameraTargetOBJ.transform.position).sqrMagnitude < Mathf.Pow(MainCameraTargetDistance - 0.01f, 2))
			{
				MainCameraTargetPos += -MainCameraTargetOBJ.transform.forward;
			}

			//角度制限
			if (Vector3.Angle(MainCameraTargetOBJ.transform.forward, -PlayerCharacter.transform.up) > CameraDownLimit)
			{
				MainCameraTargetPos += MainCameraTargetOBJ.transform.up;
			}
			else if (Vector3.Angle(MainCameraTargetOBJ.transform.forward, -PlayerCharacter.transform.up) < CameraUpLimit)
			{
				MainCameraTargetPos += -MainCameraTargetOBJ.transform.up;
			}

			//ターゲットダミー移動
			MainCameraTargetOBJ.transform.position += MainCameraTargetPos * CameraMoveSpeed * 2 * Time.deltaTime;
		}
	}

	//プレイヤーインプットから呼ばれるカメラ位置リセット
	private void OnCameraReset()
	{
		StartCoroutine(OnCameraResetCoroutine());
	}
	IEnumerator OnCameraResetCoroutine()
	{
		//カメラリセットフラグを立てる
		CameraResetFlag = true;

		//1フレームだとたまにシケるのでちょっと待機
		yield return new WaitForSeconds(0.1f);

		//カメラリセットフラグを下ろす
		CameraResetFlag = false;
	}

	//トレースカメラモード
	public void TraceCameraMode(bool b)
	{
		TraceCameraFlag = b;
	}

	//クローズアップ演出モード
	public void CloseUpCameraMode(Vector3 pos, Vector3 repos, GameObject look, float time, float move, Action act)
	{
		//コルーチン呼び出し
		StartCoroutine(CloseUpCameraCoroutine(pos , repos , look, time, move, act));		
	}
	IEnumerator CloseUpCameraCoroutine(Vector3 pos, Vector3 repos, GameObject look, float time, float move, Action act)
	{
		//フラグを立てる
		CloseUpCameraFlag = true;

		//演出開始時間をキャッシュ
		float t = Time.time;

		//メインカメラのキャラクターコントローラを切る
		MainCamera.GetComponent<CharacterController>().enabled = false;

		//メインカメラを指定された座標に移動
		MainCamera.transform.position = PlayerCharacter.transform.position + look.transform.forward;

		//メインカメラを指定されたオブジェクトに向ける
		MainCamera.transform.LookAt(look.transform.position);

		//ズーム係数で位置を調整
		//MainCamera.transform.position += MainCamera.transform.forward * zoom;
		
		//移動開始地点
		//Vector3 StartPos = MainCamera.transform.right * 0.5f;

		//移動終了地点
		//Vector3 EndPos = -MainCamera.transform.right * 0.5f;

		//カメラのレイヤーマスクをキャッシュ
		//LayerMask templayer = MainCamera.GetComponent<Camera>().cullingMask;

		//プレイヤーだけ映す
		//MainCamera.GetComponent<Camera>().cullingMask = LayerMask.GetMask("Player" , "Enemy" , "Effect");

		//背景を黒にする
		//MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

		//クローズアップ演出エフェクトインスタンス生成
		GameObject CloseUpEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "CloseUpEffect").ToArray()[0]);

		//エフェクトをカメラの子にする
		CloseUpEffect.transform.parent = MainCamera.transform;

		//ローカル座標回転設定
		//CloseUpEffect.transform.localPosition *= 0;
		//CloseUpEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);
		ResetTransform(CloseUpEffect);

		//エフェクト再生
		CloseUpEffect.GetComponent<ParticleSystem>().Play();

		//指定された時間待機
		while (Time.time - t < time)
		{
			//メインカメラを指定されたオブジェクトに向ける
			MainCamera.transform.LookAt(look.transform.position);

			//カメラ移動
			MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, MainCamera.transform.position - MainCamera.transform.right * move,(Time.time - t) / time);

			//1フレーム待機
			yield return null;
		}

		//エフェクトを消す
		Destroy(CloseUpEffect);

		//レイヤーマスクを元に戻す
		//MainCamera.GetComponent<Camera>().cullingMask = templayer;

		//背景をスカイボックスに戻す
		//MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;

		//メインカメラを元の座標に移動
		MainCamera.transform.position = repos;

		//メインカメラを指定された座標に向ける
		MainCamera.transform.LookAt(LookAtPos);

		//メインカメラのキャラクターコントローラを入れる
		MainCamera.GetComponent<CharacterController>().enabled = true;

		//フラグを下す
		CloseUpCameraFlag = false;

		//匿名関数実行
		act();
	}

	//コライダ関係処理関数
	IEnumerator CameraColProcess()
	{
		//コリジョンが何かに触れているっぽい
		if
		(
			(gameObject.transform.position - MainCamera.transform.position).sqrMagnitude < Mathf.Pow(CameraNearLimit, 2) ||
			(gameObject.transform.position - MainCamera.transform.position).sqrMagnitude > Mathf.Pow(CameraFarLimit, 2)
		)
		{
			//コライダからHit情報を受け取る
			ExecuteEvents.Execute<MainCameraColScriptInterface>(MainCamera, null, (reciever, eventData) => ColHit = reciever.GetColHit());
		}
		//触れてないっぽい
		else if (ColHit != null)
		{
			//ヒット情報をnullにしとく
			ColHit = null;
		}

		//レイキャスト関数呼び出し、地面の属性を調べる
		if (Physics.Raycast(MainCamera.transform.position, Vector3.down, out LocationRayHit, Mathf.Infinity, LayerMask.GetMask("TransparentFX")))
		{
			//ロケーションが変わったら処理
			if(Location != LocationRayHit.collider.name)
			{
				//ロケーション更新
				Location = LocationRayHit.collider.name;

				//ライトカラー変更
				if(Location.Contains("InDoor"))
				{
					ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.LightChange(0.5f, 0.65f, () => { }));
				}
				else if (Location.Contains("OutDoor"))
				{
					ExecuteEvents.Execute<LightColorChangeScriptInterface>(DeepFind(GameManagerScript.Instance.gameObject, "OutDoorLight"), null, (reciever, eventData) => reciever.LightChange(0.5f, 1, () => { }));
				}
			}
		}

		//レイキャスト関数呼び出し、キャラクターからカメラに向かって撃つ
		if (CameraRaycast(PlayerCharacter.transform.position + new Vector3(0, 0.8f, 0) , MainCamera.transform.position - (PlayerCharacter.transform.position + new Vector3(0, 0.8f, 0)) , Vector3.Distance(PlayerCharacter.transform.position + new Vector3(0, 0.8f, 0), MainCamera.transform.position)))
		{
			//Rayのヒットフラグを立てる
			RayHitFlag = true;
		}
		else
		{
			//Rayのヒットフラグを降ろす
			RayHitFlag = false;
		}	

		//毎フレーム呼ばなくてもよいかな
		yield return null;
	}

	//レイキャスト関数
	private bool CameraRaycast(Vector3 ori , Vector3 vec ,float dis)
	{
		//発射地点
		CameraRay.origin = ori;

		//発射ベクトル
		CameraRay.direction = vec;

		//レイ可視化
		Debug.DrawRay(ori, vec, Color.green , 0.1f);

		//発射
		return Physics.Raycast(CameraRay, out CameraRayHit , dis, RayMask);
	}

	//プレイヤーの位置をトレースする関数
	private void TracePlayer()
	{
		//トレースリストに値が入っていない場合
		if (PlayerTraceList.Count == 0)
		{
			//キャラクターが移動していない
			if (PlayerCharacter.GetComponent<CharacterController>().velocity.sqrMagnitude == 0 && CameraMoveinputValue.sqrMagnitude != 0)
			{
				//カメラ移動入力値を逆算した位置をAdd
				PlayerTraceList.Add(PlayerCharacter.transform.position + (MainCamera.transform.right * CameraMoveinputValue.x) + new Vector3(0, -CameraMoveinputValue.y, 0));
			}
			else
			{
				//プレイヤーの背後の位置をAdd
				PlayerTraceList.Add(PlayerCharacter.transform.position - (PlayerCharacter.transform.forward * 0.5f));
			}
			
			//プレイヤーの現在位置をAdd
			PlayerTraceList.Add(PlayerCharacter.transform.position);
		}
		//すでに値が入ってたらある程度離れてから追加する
		else if ((PlayerTraceList[PlayerTraceList.Count - 1] - PlayerCharacter.transform.position).sqrMagnitude > Mathf.Pow(0.25f, 2) && PlayerTraceList.Count < 50) 
		{
			PlayerTraceList.Add(PlayerCharacter.transform.position);
		}
	}

	//メニューモードカメラの初期設定をする
	public void MenuCameraSetting()
	{
		//シネマカメラを有効
		MainCamera.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;

		//キャラクターコントローラを切る
		CameraController.enabled = false;

		//カメラをメニューモードに切り替える
		CameraModeSwitch = 0;
	}

	//ミッションモードカメラの初期設定する
	public void MissionCameraSetting()
	{
		//カメラとキャラクターの間にある障害物をチェックするRay初期化
		CameraRay = new Ray(PlayerCharacter.transform.position, MainCamera.transform.position - PlayerCharacter.transform.position);

		//ターゲットダミーを初期位置に移動、キャラクターの後方
		MainCameraTargetOBJ.transform.position = -PlayerCharacter.transform.forward * 5 + Vector3.up;

		//最初はターゲットダミーと同じ位置にカメラ本体を移動させておく
		MainCamera.transform.position = MainCameraTargetOBJ.transform.position;

		//最初はキャラクターと同じ位置にカメラルートダミーを移動させておく
		transform.position = PlayerCharacter.transform.position;

		//最初はカメラをキャラクターに向けておく
		MainCamera.transform.LookAt(PlayerCharacter.transform.position + LookAtOffset);

		//シネマカメラを切る
		MainCamera.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;

		//キャラクターコントローラを入れる
		CameraController.enabled = true;

		//カメラをミッションモードに切り替える
		CameraModeSwitch = 1;
	}

	//キャラクター関連の処理関数
	private void CharacterProcess()
	{
		//ルートオブジェクトをプレイヤーキャラクターと同じ位置に移動
		transform.position = PlayerCharacter.transform.position;
	}

	//プレイヤーインプットから呼ばれるカメラ移動値
	private void OnCameraMove(InputValue inputValue)
	{
		CameraMoveinputValue = inputValue.Get<Vector2>();
	}
	//プレイヤーインプットから呼ばれるカメラズーム値
	private void OnCameraZoom(InputValue inputValue)
	{
		CameraZoominputValue = inputValue.Get<float>();
	}

	//プレイヤーキャラクターをセットする、キャラ交代した時にMissionManagerから呼ばれるインターフェイス
	public void SetPlayerCharacter(GameObject c)
	{
		PlayerCharacter = c;
	}

	//引数で超必殺技演出フラグをを受け取る
	public void SetSuperArtsFlag(bool b)
	{
		SuperArtsFlag = b;
	}

	//引数でプレイヤーがロックしている敵を受け取る、インターフェイス
	public void SetLockEnemy(GameObject e)
	{
		LockEnemy = e;
	}

	//画面を揺らす、インターフェイス
	public void CameraShake(float t)
	{
		//コルーチン呼び出し
		StartCoroutine(CameraShakeCoroutine(t));
	}
	IEnumerator CameraShakeCoroutine(float t)
	{
		//フラグを立てる
		CameraShakeFlag = true;

		//引数で指定された時間が経過するまで待機
		yield return new WaitForSeconds(t);

		//フラグを下す
		CameraShakeFlag = false;
	}

	//受け取った座標が画角にあるか返す関数、インターフェイス
	public bool InCameraView(Vector3 pos)
	{
		//出力用変数宣言
		bool re = false;

		//カメラの向きと敵の位置の角度を求める、これがないと真後ろの敵もロックしてしまう
		if(Vector3.Angle(MainCamera.transform.forward, pos - MainCamera.transform.position) < 90)
		{
			//画角に入っていたらロック
			re = new Rect(0, 0, 1, 1).Contains(MainCamera.GetComponent<Camera>().WorldToViewportPoint(pos));
		}

		//出力
		return re;
	}
}
 
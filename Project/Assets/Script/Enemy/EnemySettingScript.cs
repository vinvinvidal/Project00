using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class EnemySettingScript : GlobalClass
{
	//このキャラクターのID
	public string ID;

	//敵キャラクタースクリプト
	private EnemyCharacterScript TempEnemyScript;

	//結合するメッシュの元になるスキンメッシュレンダラー
	private SkinnedMeshRenderer CostumeRenderer;

	//統合するスキンメッシュレンダラーを持っているゲームオブジェクト
	private List<GameObject> CombineRendererOBJList = new List<GameObject>();


	//セッティング完了フラグ
	public bool AllReadyFlag { get; set; }

	//ダメージモーション読み込み完了フラグ
	private bool DamageAnimLoadCompleteFlag = false;

	//たむろモーション読み込み完了フラグ
	private bool ReadyAnimLoadCompleteFlag = false;

	//スケベヒットモーション読み込み完了フラグ
	private bool H_HitAnimLoadCompleteFlag = false;

	//スケベアタックモーション読み込み完了フラグ
	private bool H_AttackAnimLoadCompleteFlag = false;

	//スケベブレイクモーション読み込み完了フラグ
	private bool H_BreakAnimLoadCompleteFlag = false;

	//髪オブジェクト読み込み完了フラグ
	private bool HairLoadCompleteFlag = false;

	//下着オブジェクト読み込み完了フラグ
	private bool UnderWearLoadCompleteFlag = false;

	//インナーオブジェクト読み込み完了フラグ
	private bool InnerLoadCompleteFlag = false;

	//靴下オブジェクト読み込み完了フラグ
	private bool SocksLoadCompleteFlag = false;

	//ボトムスオブジェクト読み込み完了フラグ
	private bool BottomsLoadCompleteFlag = false;

	//靴オブジェクト読み込み完了フラグ
	private bool ShoesLoadCompleteFlag = false;

	//ダメージモーション読み込み用Dictionary
	private Dictionary<string,AnimationClip> DamageAnimDic = new Dictionary<string, AnimationClip>();

	void Start()
    {
		//セッティング完了フラグを下ろす
		AllReadyFlag = false;

		//準備完了待機コルーチン呼び出し
		StartCoroutine(AllReadyCoroutine());

		//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
		CostumeRenderer = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

		//スキンメッシュレンダラーを回す
		foreach (SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			//ボディ部分のメッシュを統合Listに入れる
			if (i.name.Contains("Body") || i.name.Contains("Face") || i.name.Contains("Others"))
			{
				//トランスフォームリセット
				ResetTransform(i.gameObject);

				//メッシュ統合ListにAdd
				CombineRendererOBJList.Add(i.gameObject);
			}

			//レンダラーを切って非表示にする
			i.enabled = false;
		}

		//下着オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/UnderWear/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject UnderWearOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			UnderWearOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(UnderWearOBJ);

			//メッシュ統合ListにAdd
			CombineRendererOBJList.Add(UnderWearOBJ);

			//読み込み完了フラグを立てる
			UnderWearLoadCompleteFlag = true;

		}));

		//靴下オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Socks/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject SocksOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			SocksOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(SocksOBJ);

			//メッシュ統合ListにAdd
			CombineRendererOBJList.Add(SocksOBJ);

			//読み込み完了フラグを立てる
			SocksLoadCompleteFlag = true;

		}));

		//インナーオブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Inner/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject InnerOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			InnerOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(InnerOBJ);

			//メッシュ統合ListにAdd
			CombineRendererOBJList.Add(InnerOBJ);

			//読み込み完了フラグを立てる
			InnerLoadCompleteFlag = true;

		}));

		//ボトムスオブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Bottoms/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject BottomsOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			BottomsOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(BottomsOBJ);

			//メッシュ統合ListにAdd
			CombineRendererOBJList.Add(BottomsOBJ);

			//読み込み完了フラグを立てる
			BottomsLoadCompleteFlag = true;

		}));

		//靴オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Shoes/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject ShoesOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			ShoesOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(ShoesOBJ);

			//メッシュ統合ListにAdd
			CombineRendererOBJList.Add(ShoesOBJ);

			//読み込み完了フラグを立てる
			ShoesLoadCompleteFlag = true;

		}));

		//メッシュ結合コルーチン呼び出し
		StartCoroutine(SkinMeshIntegrationCoroutine());
	}

	//メッシュ結合コルーチン
	private IEnumerator SkinMeshIntegrationCoroutine()
	{
		//読み込み完了するまで回る
		while(!(
			UnderWearLoadCompleteFlag &&
			InnerLoadCompleteFlag &&
			SocksLoadCompleteFlag &&
			ShoesLoadCompleteFlag &&
			BottomsLoadCompleteFlag
		))
		{
			yield return null;
		}

		//スキンメッシュを統合する関数呼び出し
		SkinMeshIntegration(CombineRendererOBJList, CostumeRenderer, (GameObject OBJ) =>
		{
			//敵キャラクタースクリプト取得
			TempEnemyScript = gameObject.GetComponent<EnemyCharacterScript>();

			//スクリプト有効化
			TempEnemyScript.enabled = true;

			//親を設定
			OBJ.transform.parent = gameObject.transform;

			//名前を設定
			OBJ.name = "EnemyCombineMeshOBJ";

			//レイヤーをEnemyにする
			OBJ.layer = LayerMask.NameToLayer("Enemy");

			//カメラに収まってるか調べるスクリプトを付ける
			OBJ.AddComponent<OnCameraScript>();

			//スクリプトのOnCameraObjectに代入
			TempEnemyScript.OnCameraObject = OBJ;

			//EnemyClass読み込み
			EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => e.EnemyID == ID).ToList()[0];

			//ライフ読み込み
			TempEnemyScript.Life = tempclass.Life;

			//スタン値読み込み
			TempEnemyScript.Stun = tempclass.Stun;

			//移動速度読み込み
			TempEnemyScript.MoveSpeed = tempclass.MoveSpeed;

			//旋回速度読み込み
			TempEnemyScript.TurnSpeed = tempclass.TurnSpeed;

			//ダウン時間読み込み
			TempEnemyScript.DownTime = tempclass.DownTime;

			//敵攻撃情報Listを敵スクリプトに送る
			ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetAttackClassList(new List<EnemyAttackClass>(GameManagerScript.Instance.AllEnemyAttackList.Where(i => i.UserID == ID).ToList())));

			//ダメージモーション読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/Damage/", "anim", (List<object> OBJList) =>
			{
				//モーション番号判別ループ
				for (int count1 = 0; count1 < 10; count1++)
				{
					for (int count2 = 0; count2 < 10; count2++)
					{
						//連想配列に番号をキーにした要素追加
						DamageAnimDic.Add(count1.ToString() + count2, null);

						//読み込んだアニメーションListを回す
						foreach (object i in OBJList)
						{
							//名前で検索、ヒットしたらDictionaryにAdd
							if ((i as AnimationClip).name.Contains("Damage" + count1.ToString() + count2))
							{
								DamageAnimDic[count1.ToString() + count2] = i as AnimationClip;
							}
						}
					}
				}

				//Listを敵スクリプトに送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetDamageAnimList(new List<AnimationClip>(DamageAnimDic.Values)));

				//読み込み完了フラグを立てる
				DamageAnimLoadCompleteFlag = true;

			}));

			//たむろモーション読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/Ready/", "anim", (List<object> OBJList) =>
			{
				//代入用変数宣言
				List<AnimationClip> templist = new List<AnimationClip>();

				//読み込んだアニメーションをListにAdd
				foreach (var i in OBJList)
				{
					templist.Add(i as AnimationClip);
				}

				//Listを敵スクリプトに送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetReadyAnimList(templist));

				//読み込み完了フラグを立てる
				ReadyAnimLoadCompleteFlag = true;

			}));
					   
			//スケベヒットモーション読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/H_Hit/", "anim", (List<object> OBJList) =>
			{
				//代入用変数宣言
				List<AnimationClip> templist = new List<AnimationClip>();

				//読み込んだアニメーションをListにAdd
				foreach (var i in OBJList)
				{
					templist.Add(i as AnimationClip);
				}

				//Listを敵スクリプトに送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetH_HitAnimList(templist));

				//読み込み完了フラグを立てる
				H_HitAnimLoadCompleteFlag = true;

			}));

			//スケベアタックモーション読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/H_Attack/", "anim", (List<object> OBJList) =>
			{
				//代入用変数宣言
				List<AnimationClip> templist = new List<AnimationClip>();

				//読み込んだアニメーションをListにAdd
				foreach (var i in OBJList)
				{
					templist.Add(i as AnimationClip);
				}

				//Listを敵スクリプトに送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetH_AttackAnimList(templist));

				//読み込み完了フラグを立てる
				H_AttackAnimLoadCompleteFlag = true;

			}));

			//スケベブレイクモーション読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/H_Break/", "anim", (List<object> OBJList) =>
			{
				//代入用変数宣言
				List<AnimationClip> templist = new List<AnimationClip>();

				//読み込んだアニメーションをListにAdd
				foreach (var i in OBJList)
				{
					templist.Add(i as AnimationClip);
				}

				//Listを敵スクリプトに送る
				ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetH_BreakAnimList(templist));

				//読み込み完了フラグを立てる
				H_BreakAnimLoadCompleteFlag = true;

			}));

			//足のボーンにコンストレイント追加
			DeepFind(gameObject, "R_FootBone").AddComponent<PositionConstraint>().constraintActive = true;
			DeepFind(gameObject, "L_FootBone").AddComponent<PositionConstraint>().constraintActive = true;

			//足首を繋げるConstraintSource
			ConstraintSource R_FootConstraint = new ConstraintSource();
			ConstraintSource L_FootConstraint = new ConstraintSource();

			//コンストレイントの重みを設定
			R_FootConstraint.weight = 1;
			L_FootConstraint.weight = 1;

			//コンストレイントのsourceを設定
			R_FootConstraint.sourceTransform = DeepFind(gameObject, "R_LowerLegBone_end").transform;
			L_FootConstraint.sourceTransform = DeepFind(gameObject, "L_LowerLegBone_end").transform;

			//足首を繋げる
			DeepFind(gameObject, "R_FootBone").GetComponent<PositionConstraint>().AddSource(R_FootConstraint);
			DeepFind(gameObject, "L_FootBone").GetComponent<PositionConstraint>().AddSource(L_FootConstraint);

			//クロス用コライダを腕に仕込む
			foreach (CapsuleCollider i in gameObject.GetComponentsInChildren<CapsuleCollider>())
			{
				//右腕
				if (i.gameObject.name.Contains("_RArm"))
				{
					//ボーンを親にする
					i.gameObject.transform.parent = DeepFind(gameObject, "R_LowerArmBone").transform;

					//トランスフォームリセット
					ResetTransform(i.gameObject);
				}
				//右手
				else if (i.gameObject.name.Contains("_RHand"))
				{
					//ボーンを親にする
					i.gameObject.transform.parent = DeepFind(gameObject, "R_HandBone").transform;

					//トランスフォームリセット
					ResetTransform(i.gameObject);
				}
				//左腕
				else if (i.gameObject.name.Contains("_LArm"))
				{
					//ボーンを親にする
					i.gameObject.transform.parent = DeepFind(gameObject, "L_LowerArmBone").transform;

					//トランスフォームリセット
					ResetTransform(i.gameObject);
				}
				//左手
				else if (i.gameObject.name.Contains("_LHand"))
				{
					//ボーンを親にする
					i.gameObject.transform.parent = DeepFind(gameObject, "L_HandBone").transform;

					//トランスフォームリセット
					ResetTransform(i.gameObject);
				}
				else if (i.gameObject.name.Contains("_Head"))
				{
					//ボーンを親にする
					i.gameObject.transform.parent = DeepFind(gameObject, "HeadBone").transform;

					//トランスフォームリセット
					ResetTransform(i.gameObject);
				}
			}

			//性器オブジェクトにモザイクエフェクトを仕込む
			foreach (Transform ii in gameObject.GetComponentsInChildren<Transform>())
			{
				//名前で検索
				if (ii.name.Contains("Genital"))
				{
					//モザイクエフェクトインスタンス化
					GameObject mosaicOBJ = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "Mosaic").ToArray()[0]);

					//性器オブジェクトの子にする
					mosaicOBJ.transform.parent = ii.gameObject.transform;

					//トランスフォームリセット
					ResetTransform(mosaicOBJ);

					//最初は消しとく
					mosaicOBJ.SetActive(false);
					ii.gameObject.SetActive(false);

				}
			}

			//髪オブジェクト読み込み
			StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Hair/", "prefab", (List<object> OBJList) =>
			{
				//読み込んだオブジェクトからランダムで髪型を選びインスタンス化
				GameObject HairOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

				//レンダラーを切って非表示にする
				foreach (SkinnedMeshRenderer i in HairOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					i.enabled = false;
				}

				//頭ボーンの子にする
				HairOBJ.transform.parent = DeepFind(gameObject, "HeadBone").transform;

				//トランスフォームリセット
				ResetTransform(HairOBJ);

				//読み込み完了フラグを立てる
				HairLoadCompleteFlag = true;

			}));
		});
	}

	//準備完了待機コルーチン
	IEnumerator AllReadyCoroutine()
	{
		//読み込み完了するまで回る
		while (!(
			DamageAnimLoadCompleteFlag &&
			ReadyAnimLoadCompleteFlag &&
			H_HitAnimLoadCompleteFlag &&
			H_AttackAnimLoadCompleteFlag &&
			H_BreakAnimLoadCompleteFlag &&
			HairLoadCompleteFlag && 
			UnderWearLoadCompleteFlag && 
			InnerLoadCompleteFlag && 
			SocksLoadCompleteFlag &&
			ShoesLoadCompleteFlag &&
			BottomsLoadCompleteFlag))
		{
			yield return null;
		}

		//キャラクターコントローラ有効化
		gameObject.GetComponent<CharacterController>().enabled = true;

		//アニメーター有効化
		gameObject.GetComponent<Animator>().enabled = true;

		//読み込み完了したら全てのレンダラーを有効化して表示する
		foreach (SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			i.enabled = true;
		}

		//セッティング完了フラグを立てる
		AllReadyFlag = true;

		//自身を無効化
		gameObject.SetActive(false);
	}
}

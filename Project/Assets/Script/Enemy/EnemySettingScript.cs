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

	//セッティング完了フラグ
	public bool AllReadyFlag { get; set; }

	//ダメージモーション読み込み完了フラグ
	private bool DamageAnimLoadCompleteFlag = false;

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

		//レンダラーを切って非表示にする
		foreach(SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			i.enabled = false;
		}

		//EnemyClass読み込み
		EnemyClass tempclass = GameManagerScript.Instance.AllEnemyList.Where(e => e.EnemyID == ID).ToList()[0];

		//本体のスクリプト取得
		EnemyCharacterScript tempscript = gameObject.GetComponent<EnemyCharacterScript>();

		//ライフ読み込み
		tempscript.Life = tempclass.Life;

		//スタン値読み込み
		tempscript.Stun = tempclass.Stun;

		//移動速度読み込み
		tempscript.MoveSpeed = tempclass.MoveSpeed;

		//旋回速度読み込み
		tempscript.TurnSpeed = tempclass.TurnSpeed;

		//ダウン時間読み込み
		tempscript.DownTime = tempclass.DownTime;

		//敵攻撃情報Listを敵スクリプトに送る
		ExecuteEvents.Execute<EnemyCharacterInterface>(gameObject, null, (reciever, eventData) => reciever.SetAttackClassList(new List<EnemyAttackClass>(GameManagerScript.Instance.AllEnemyAttackList.Where(i => i.UserID == ID).ToList())));

		//ダメージモーション読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/Damage/", "anim", (List<object> OBJList) =>
		{
			//モーション番号判別ループ
			for (int count1 = 0; count1 < 6; count1++)
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

				//ローカルTransform設定
				mosaicOBJ.transform.localPosition *= 0;
				mosaicOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

				//最初は消しとく
				mosaicOBJ.SetActive(false);
				ii.gameObject.SetActive(false);

			}
		}

		//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
		SkinnedMeshRenderer CostumeRenderer = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

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

			//ローカルtransformをゼロに
			HairOBJ.transform.localPosition *= 0;
			HairOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//読み込み完了フラグを立てる
			HairLoadCompleteFlag = true;
		
		}));

		//下着オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/UnderWear/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject UnderWearOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in UnderWearOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			UnderWearOBJ.transform.parent = gameObject.transform;

			//ローカルtransformをゼロに
			UnderWearOBJ.transform.localPosition *= 0;
			UnderWearOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in UnderWearOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;
			}			

			//読み込み完了フラグを立てる
			UnderWearLoadCompleteFlag = true;

		}));

		//インナーオブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Inner/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject InnerOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in InnerOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			InnerOBJ.transform.parent = gameObject.transform;

			//ローカルtransformをゼロに
			InnerOBJ.transform.localPosition *= 0;
			InnerOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in InnerOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;
			}

			//読み込み完了フラグを立てる
			InnerLoadCompleteFlag = true;

		}));

		//靴下オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Socks/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject SocksOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in SocksOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			SocksOBJ.transform.parent = gameObject.transform;

			//ローカルtransformをゼロに
			SocksOBJ.transform.localPosition *= 0;
			SocksOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in SocksOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;
			}

			//読み込み完了フラグを立てる
			SocksLoadCompleteFlag = true;

		}));

		//ボトムスオブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Bottoms/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject BottomsOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in BottomsOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			BottomsOBJ.transform.parent = gameObject.transform;

			//ローカルtransformをゼロに
			BottomsOBJ.transform.localPosition *= 0;
			BottomsOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in BottomsOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;
			}

			//読み込み完了フラグを立てる
			BottomsLoadCompleteFlag = true;

		}));

		//靴オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/Shoes/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject ShoesOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in ShoesOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			ShoesOBJ.transform.parent = gameObject.transform;

			//ローカルtransformをゼロに
			ShoesOBJ.transform.localPosition *= 0;
			ShoesOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in ShoesOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;
			}

			//読み込み完了フラグを立てる
			ShoesLoadCompleteFlag = true;

		}));

	}

	//準備完了待機コルーチン
	IEnumerator AllReadyCoroutine()
	{
		//読み込み完了するまで回る
		while (!(
			DamageAnimLoadCompleteFlag &&
			HairLoadCompleteFlag && 
			UnderWearLoadCompleteFlag && 
			InnerLoadCompleteFlag && 
			SocksLoadCompleteFlag &&
			ShoesLoadCompleteFlag &&
			BottomsLoadCompleteFlag))
		{
			yield return null;
		}

		//セッティング完了フラグを立てる
		AllReadyFlag = true;

		//ダメージ用コライダを有効化
		DeepFind(gameObject, "EnemyDamageCol").GetComponent<BoxCollider>().enabled = true;

		//読み込み完了したら全てのレンダラーを有効化して表示する
		foreach (SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			i.enabled = true;
		}
	}
}

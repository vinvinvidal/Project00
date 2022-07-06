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

	//メッシュ結合で使うCharacterBodyMaterial
	private Material BodyMaterial;

	//結合するメッシュの元になるスキンメッシュレンダラー
	private SkinnedMeshRenderer CostumeRenderer;

	//統合するスキンメッシュレンダラー
	private List<SkinnedMeshRenderer> CombineRendererList = new List<SkinnedMeshRenderer>();

	//統合するベーステクスチャ
	private List<Texture2D> PackBaseTextureList = new List<Texture2D>();

	//統合する法線テクスチャ
	private List<Texture2D> PackNormalTextureList = new List<Texture2D>();

	//統合するハイライトテクスチャ
	private List<Texture2D> PackHiLightTextureList = new List<Texture2D>();

	//統合する線画テクスチャ
	private List<Texture2D> PackLineTextureList = new List<Texture2D>();

	//統合するマットキャップテクスチャ
	private List<Texture2D> PackMatCapTextureList = new List<Texture2D>();

	//結合した後に消すオブジェクトList
	private List<GameObject> DestroyOBJList = new List<GameObject>();
	
	//セッティング完了フラグ
	public bool AllReadyFlag { get; set; }

	//ダメージモーション読み込み完了フラグ
	private bool DamageAnimLoadCompleteFlag = false;

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

		//敵キャラクタースクリプト取得
		TempEnemyScript = gameObject.GetComponent<EnemyCharacterScript>();

		//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
		CostumeRenderer = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

		//Bodyに仕込んであるCostumeのボディマテリアル取得
		BodyMaterial = CostumeRenderer.material;

		//スキンメッシュレンダラーを回す
		foreach (SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			//ボディ部分のメッシュを統合Listに入れる
			if (i.name.Contains("Body") || i.name.Contains("Face") || i.name.Contains("Others"))
			{
				//トランスフォームリセット
				ResetTransform(i.gameObject); 

				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				i.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(i);

				//オブジェクトを削除Listに入れる
				DestroyOBJList.Add(i.gameObject);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = i.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}			

			//レンダラーを切って非表示にする
			i.enabled = false;
		}

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
		
		//スケベヒットモーション読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Anim/Enemy/" + ID + "/H_Hit/", "anim", (List<object> OBJList) =>
		{
			//代入用変数宣言
			List<AnimationClip> templist = new List<AnimationClip>();

			//読み込んだアニメーションをListにAdd
			foreach(var i in OBJList)
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
		foreach(CapsuleCollider i in gameObject.GetComponentsInChildren<CapsuleCollider>())
		{
			//右腕
			if(i.gameObject.name.Contains("_RArm"))
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

		//下着オブジェクト読み込み
		StartCoroutine(GameManagerScript.Instance.AllFileLoadCoroutine("Object/Enemy/" + ID + "/Costume/UnderWear/", "prefab", (List<object> OBJList) =>
		{
			//読み込んだオブジェクトからランダムで選びインスタンス化
			GameObject UnderWearOBJ = Instantiate(OBJList[Random.Range(0, OBJList.Count)] as GameObject);

			//自身の子にする
			UnderWearOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(UnderWearOBJ);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in UnderWearOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(ii);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = ii.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}

			//オブジェクトを削除Listに入れる
			DestroyOBJList.Add(UnderWearOBJ);

			//読み込み完了フラグを立てる
			UnderWearLoadCompleteFlag = true;

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

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in InnerOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(ii);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = ii.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}

			//オブジェクトを削除Listに入れる
			DestroyOBJList.Add(InnerOBJ);

			//読み込み完了フラグを立てる
			InnerLoadCompleteFlag = true;

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

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in SocksOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(ii);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = ii.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}

			//オブジェクトを削除Listに入れる
			DestroyOBJList.Add(SocksOBJ);

			/*
			//レンダラーを切って非表示にする
			foreach (SkinnedMeshRenderer i in SocksOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				i.enabled = false;
			}

			//自身の子にする
			SocksOBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(SocksOBJ);

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in SocksOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//結合用メッシュ宣言
				CombineInstance tempmesh = new CombineInstance();

				//メッシュを適応
				tempmesh.mesh = ii.sharedMesh;

				//トランスフォームを適応
				tempmesh.transform = ii.gameObject.transform.localToWorldMatrix;

				//ListにAdd
				CombineRendererList.Add(ii);
			}
			*/
			//読み込み完了フラグを立てる
			SocksLoadCompleteFlag = true;

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

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in BottomsOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(ii);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = ii.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}

			//オブジェクトを削除Listに入れる
			DestroyOBJList.Add(BottomsOBJ);

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

			//プレハブ内のスキニングメッシュレンダラーを全て取得
			foreach (SkinnedMeshRenderer ii in ShoesOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ボーン構成をコピーしてキャラクターのボーンと紐付ける
				ii.bones = CostumeRenderer.bones;

				//ListにAdd
				CombineRendererList.Add(ii);

				//シェーダースクリプト取得
				CharacterBodyShaderScript tempscript = ii.gameObject.GetComponent<CharacterBodyShaderScript>();

				//ベーステクチャを取得
				PackBaseTextureList.Add(tempscript._TexBase);

				//法線テクスチャを取得
				PackNormalTextureList.Add(tempscript._TexNormal);

				//ハイライトテクスチャを取得
				PackHiLightTextureList.Add(tempscript._TexHiLight);

				//線画テクスチャを取得
				PackLineTextureList.Add(tempscript._TexLine);

				//マットキャップテクスチャを取得
				PackMatCapTextureList.Add(tempscript._HiLightMatCap);
			}

			//オブジェクトを削除Listに入れる
			DestroyOBJList.Add(ShoesOBJ);

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
			H_HitAnimLoadCompleteFlag&&
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

		//統合用オブジェクト宣言
		GameObject CombineMeshOBJ = new GameObject();

		//親を設定
		CombineMeshOBJ.transform.parent = gameObject.transform;

		//名前を設定
		CombineMeshOBJ.name = "EnemyCombineMeshOBJ";

		//レイヤーをEnemyにする
		CombineMeshOBJ.layer = LayerMask.NameToLayer("Enemy");

		//カメラに収まってるか調べるスクリプトを付ける
		CombineMeshOBJ.AddComponent<OnCameraScript>();

		//スクリプトのOnCameraObjectに代入
		TempEnemyScript.OnCameraObject = CombineMeshOBJ;

		//結合するCombineInstanceList
		List<CombineInstance> CombineInstanceList = new List<CombineInstance>();

		//結合するUV
		List<Vector2[]> CombineUVList = new List<Vector2[]>();

		//ボーンList
		List<Transform> BoneList = new List<Transform>();

		//ウェイトList
		List<BoneWeight> BoneWeightList = new List<BoneWeight>();

		//バインドボーズList
		List<Matrix4x4> BindPoseList = new List<Matrix4x4>();

		//名前とインデックスのハッシュテーブル
		Hashtable BoneHash = new Hashtable();

		//インデックスに使うループカウント
		int count = 0;

		//サンプルからボーン情報を取る
		foreach (Transform bone in CostumeRenderer.bones)
		{
			//ボーンを取得
			BoneList.Add(bone);

			//名前とインデクスをハッシュテーブルに入れる
			BoneHash.Add(bone.name, count);

			//カウントアップ
			count ++;
		}

		//ループカウント初期化
		count = 0;

		//ボーンのバインドポーズを取得
		foreach(var i in BoneList)
		{
			BindPoseList.Add(BoneList[count].worldToLocalMatrix * transform.worldToLocalMatrix);

			//カウントアップ
			count++;
		}

		//統合するメッシュレンダラーを回す
		foreach (var i in CombineRendererList)
		{
			//ウェイトを回す
			foreach (BoneWeight ii in i.sharedMesh.boneWeights)
			{
				//リマップ用ウェイト
				BoneWeight TempWeight = ii;

				//ハッシュテーブルを元にボーンをリマップ
				TempWeight.boneIndex0 = (int)BoneHash[i.bones[ii.boneIndex0].name];
				TempWeight.boneIndex1 = (int)BoneHash[i.bones[ii.boneIndex1].name];
				TempWeight.boneIndex2 = (int)BoneHash[i.bones[ii.boneIndex2].name];
				TempWeight.boneIndex3 = (int)BoneHash[i.bones[ii.boneIndex3].name];

				//ListにAdd
				BoneWeightList.Add(TempWeight);
			}

			//統合するUVを格納
			CombineUVList.Add(i.sharedMesh.uv);

			//メッシュ統合用CombineInstance
			CombineInstance TempCombineInstance = new CombineInstance();

			//引数のメッシュレンダラーからメッシュを取得
			TempCombineInstance.mesh = i.sharedMesh;

			//引数のメッシュレンダラーからトランスフォームを取得
			TempCombineInstance.transform = i.transform.localToWorldMatrix;

			//ListにAdd
			CombineInstanceList.Add(TempCombineInstance);
		}

		//メッシュレンダラーを付ける
		SkinnedMeshRenderer CombineMeshRenderer = CombineMeshOBJ.AddComponent<SkinnedMeshRenderer>();

		//マテリアル設定
		CombineMeshRenderer.material = BodyMaterial;

		//空メッシュを入れる
		CombineMeshRenderer.sharedMesh = new Mesh();

		//メッシュを結合
		CombineMeshRenderer.sharedMesh.CombineMeshes(CombineInstanceList.ToArray());

		//ボーン設定
		CombineMeshRenderer.bones = BoneList.ToArray();

		//ボーンウェイト設定
		CombineMeshRenderer.sharedMesh.boneWeights = BoneWeightList.ToArray();

		//バインドポーズ設定
		CombineMeshRenderer.sharedMesh.bindposes = BindPoseList.ToArray();

		//バウンディングボックスを設定
		CombineMeshRenderer.localBounds = new Bounds(new Vector3(0, 1, 0), new Vector3(2, 2, 2));

		//統合用ベーステクスチャ
		Texture2D PackBaseTexture =new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//統合用法線テクスチャ
		Texture2D PackNormalTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//統合用ハイライトテクスチャ
		Texture2D PackHiLightTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//統合用線画テクスチャ
		Texture2D PackLineTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//統合用マットキャップテクスチャ
		Texture2D PackMatCapTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//不足分のテクスチャを入れてムリヤリ16枚にする
		while (PackBaseTextureList.Count < 16)
		{
			PackBaseTextureList.Add(new Texture2D(128, 128, TextureFormat.RGBA32, false));
		}
		while (PackNormalTextureList.Count < 16)
		{
			PackNormalTextureList.Add(new Texture2D(128, 128, TextureFormat.RGBA32, false));
		}
		while (PackHiLightTextureList.Count < 16)
		{
			PackHiLightTextureList.Add(new Texture2D(128, 128, TextureFormat.RGBA32, false));
		}
		while (PackLineTextureList.Count < 16)
		{
			PackLineTextureList.Add(new Texture2D(128, 128, TextureFormat.RGBA32, false));
		}
		while (PackMatCapTextureList.Count < 16)
		{
			PackMatCapTextureList.Add(new Texture2D(128, 128, TextureFormat.RGBA32, false));
		}

		//テクスチャをパックしてRectを受け取る
		Rect[] TexBaseRect = PackBaseTexture.PackTextures(PackBaseTextureList.ToArray(), 0, 512, false);

		PackNormalTexture.PackTextures(PackNormalTextureList.ToArray(), 0, 512, false);
		PackHiLightTexture.PackTextures(PackHiLightTextureList.ToArray(), 0, 512, false);
		PackLineTexture.PackTextures(PackLineTextureList.ToArray(), 0, 512, false);
		PackMatCapTexture.PackTextures(PackMatCapTextureList.ToArray(), 0, 512, false);

		//統合用UV宣言
		List<Vector2> CombineUV = new List<Vector2>();

		//ループカウント初期化
		count = 0;

		//UVListを回す
		foreach (var i in CombineUVList)
		{
			//格納用UVList宣言
			List<Vector2> tempUV = new List<Vector2>();

			//パックしたテクスチャのRectを元にUVを16マスに配置する
			foreach (var ii in i)
			{
				tempUV.Add(new Vector2((ii.x * 0.25f) + TexBaseRect[count].position.x, (ii.y * 0.25f) + TexBaseRect[count].position.y));
			}

			//UVを追加
			CombineUV.AddRange(tempUV);

			//カウントアップ
			count++;
		}

		//UVを設定
		CombineMeshRenderer.sharedMesh.uv = CombineUV.ToArray();

		//キャラクターボディシェーダースクリプトを付ける
		CombineMeshOBJ.AddComponent<CharacterBodyShaderScript>();

		//ベーステクスチャを設定
		CombineMeshOBJ.GetComponent<CharacterBodyShaderScript>()._TexBase = PackBaseTexture;

		//法線テクスチャを設定
		CombineMeshOBJ.GetComponent<CharacterBodyShaderScript>()._TexNormal = PackNormalTexture;

		//ハイライトテクスチャを設定
		CombineMeshOBJ.GetComponent<CharacterBodyShaderScript>()._TexHiLight = PackHiLightTexture;

		//線画テクスチャを設定
		CombineMeshOBJ.GetComponent<CharacterBodyShaderScript>()._TexLine = PackLineTexture;

		//マットキャップテクスチャを設定
		CombineMeshOBJ.GetComponent<CharacterBodyShaderScript>()._HiLightMatCap = PackMatCapTexture;

		//不要になった統合前のオブジェクトを消す
		foreach (var i in DestroyOBJList)
		{
			Destroy(i);
		}

		//アニメーター有効化
		gameObject.GetComponent<Animator>().enabled = true;

		//セッティング完了フラグを立てる
		AllReadyFlag = true;

		//読み込み完了したら全てのレンダラーを有効化して表示する
		foreach (SkinnedMeshRenderer i in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			i.enabled = true;
		}
	}

	Texture2D duplicateTexture(Texture2D source)
	{
		RenderTexture renderTex = RenderTexture.GetTemporary(
					source.width,
					source.height,
					0,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear);

		Graphics.Blit(source, renderTex);
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = renderTex;
		Texture2D readableText = new Texture2D(source.width, source.height);
		readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		readableText.Apply();
		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary(renderTex);
		return readableText;
	}
}

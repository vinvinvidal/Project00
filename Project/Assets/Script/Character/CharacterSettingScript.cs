using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface CharacterSettingScriptInterface : IEventSystemHandler
{
	//キャラクターIDを返すインターフェイス
	int GetCharacterID();
}

public class CharacterSettingScript : GlobalClass, CharacterSettingScriptInterface
{
	//このキャラクターのID
	public int ID;

	//ミニマップアイコンの色
	public Color MiniMapColor;

	//ミニマップオブジェクト読み込み完了フラグ
	private bool MiniMapOBJLoadCompleteFlag = false;

	//髪オブジェクト読み込み完了フラグ
	private bool HairLoadCompleteFlag = false;

	//衣装オブジェクト読み込み完了フラグ
	private bool CostumeLoadCompleteFlag = false;

	//武器オブジェクト読み込み完了フラグ
	private bool WeaponLoadCompleteFlag = false;

	void Start()
    {
		//準備完了待機コルーチン呼び出し
		StartCoroutine(AllReadyCoroutine());

		//全てのキャラクターリストを回す
		foreach (CharacterClass i in GameManagerScript.Instance.AllCharacterList)
		{
			//自身のCharacterClassを抽出して処理
			if (i.CharacterID == ID)
			{
				//ミニマップ用オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/MiniMap/", "MiniMapPlayerArrow", "prefab", (object O) =>
				{
					//読み込んだオブジェクトをインスタンス化
					GameObject MinimapOBJ = Instantiate(O as GameObject);

					//キャラクターオブジェクトの子にする
					MinimapOBJ.transform.parent = gameObject.transform;

					//(Clone)を消す
					MinimapOBJ.name = "MiniMapPlayerArrow";

					//トランスフォームリセット
					MinimapOBJ.transform.localPosition = new Vector3(0,0.1f,0);
					MinimapOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

					//ミニマップアイコンの色を設定
					MinimapOBJ.GetComponentInChildren<Renderer>().material.SetColor("_OBJColor", MiniMapColor);

					//読み込み完了フラグを立てる
					MiniMapOBJLoadCompleteFlag = true;
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

				//特殊技を仕込む
				foreach(SpecialClass ii in GameManagerScript.Instance.AllSpecialArtsList)
				{
					//技を検索
					if(ii.UseCharacter == ID && ii.UnLock == 1)
					{
						//スクリプトに特殊技を渡す
						gameObject.GetComponent<PlayerScript>().SpecialArtsList.Add(ii);
					}
				}

				//武器オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + ID + "/Weapon/", "Weapon" + i.WeaponID, "prefab", (object O) =>
				{
					//読み込んだオブジェクトをインスタンス化
					GameObject WeaponOBJ = Instantiate(O as GameObject);

					//キャラクターオブジェクトの子にする
					WeaponOBJ.transform.parent = gameObject.transform;

					//キャラクターの武器を登録するインターフェイス呼び出し
					ExecuteEvents.Execute<PlayerScriptInterface>(gameObject, null, (reciever, eventData) => reciever.SetWeapon(WeaponOBJ));					

					//泉の武器を登録する
					if(ID == 2)
					{
						//タバコオブジェクト
						gameObject.GetComponent<Character2WeaponMoveScript>().CigaretteOBJ = DeepFind(WeaponOBJ, "2_Weapon" + i.WeaponID + "_0");

						//ワイヤーオブジェクト
						gameObject.GetComponent<Character2WeaponMoveScript>().WireOBJ = DeepFind(WeaponOBJ, "2_Weapon" + i.WeaponID + "_1_Mesh");

						//ワイヤーのボーン
						for (int count = 0; count <= 5; count++)
						{
							gameObject.GetComponent<Character2WeaponMoveScript>().BoneList.Add(DeepFind(WeaponOBJ, "2_Weapon" + i.WeaponID + "_1_Bone0" + count));
						}

						//燐糞オブジェクト
						gameObject.GetComponent<Character2WeaponMoveScript>().BombOBJ = DeepFind(WeaponOBJ, "2_Weapon" + i.WeaponID + "_2");
					}

					//読み込み完了フラグを立てる
					WeaponLoadCompleteFlag = true;
				}));

				//髪オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + ID + "/Hair/", "Hair_" + ID + "_" + i.HairID, "prefab", (object O) =>
				{
					//読み込んだオブジェクトをインスタンス化
					GameObject HairOBJ = Instantiate(O as GameObject);

					//頭ボーンの子にする
					HairOBJ.transform.parent = DeepFind(gameObject, "HeadBone").transform;

					//ローカルtransformをゼロに
					//HairOBJ.transform.localPosition *= 0;
					//HairOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);
					ResetTransform(HairOBJ);


					//髪のダイナミックボーンに使うコライダを全て取得
					foreach(DynamicBoneCollider ii in HairOBJ.GetComponentsInChildren<DynamicBoneCollider>())
					{
						//名前で判別してキャラクターのボーンの子にする
						if (ii.name.Contains("Neck"))
						{
							ii.transform.parent = DeepFind(gameObject, "NeckBone").transform;
						}
						else if (ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}

						//相対位置と回転をゼロにする
						//ii.transform.localPosition = new Vector3(0, 0, 0);
						//ii.transform.localRotation = Quaternion.Euler(0, 0, 0);
						ResetTransform(ii.gameObject);
					}

					//髪のクロスに使うSphereColliderを全て取得
					foreach (SphereCollider ii in HairOBJ.GetComponentsInChildren<SphereCollider>())
					{
						//名前で判別してキャラクターのボーンの子にする
						if (ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
						}
						else if (ii.name.Contains("Neck"))
						{
							ii.transform.parent = DeepFind(gameObject, "NeckBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_BreastBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_BreastBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_NippleBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_NippleBone").transform;
						}

						//相対位置と回転をゼロにする
						//ii.transform.localPosition = new Vector3(0, 0, 0);
						//ii.transform.localRotation = Quaternion.Euler(0, 0, 0);
						ResetTransform(ii.gameObject);
					}

					//読み込み完了フラグを立てる
					HairLoadCompleteFlag = true;

				}));

				//衣装オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + ID + "/Costume/", "Costume_" + ID + "_" + i.CostumeID, "prefab", (object O) =>
				{
					//読み込んだオブジェクトをインスタンス化
					GameObject CostumeOBJ = Instantiate(O as GameObject);

					//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
					SkinnedMeshRenderer CostumeRenderer = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

					//ローカルトランスフォームをリセット
					ResetTransform(CostumeOBJ);

					//衣装を子にする
					CostumeOBJ.transform.parent = gameObject.transform;

					//ローカルトランスフォームをリセット
					ResetTransform(CostumeOBJ);

					//下ろされパンツ宣言
					GameObject PantsOffOBJ = null;

					//衣装プレハブ内のスキニングメッシュレンダラーを全て取得
					foreach (SkinnedMeshRenderer ii in CostumeOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
					{
						//ボーン構成をコピーしてキャラクターのボーンと紐付ける
						ii.bones = CostumeRenderer.bones;

						//下ろされパンツオブジェクト取得
						if (ii.name.Contains("P_Off"))
						{
							PantsOffOBJ = ii.gameObject;
						}
					}
					//衣装のダイナミックボーンに使うコライダを全て取得して回す
					foreach (DynamicBoneCollider ii in CostumeOBJ.GetComponentsInChildren<DynamicBoneCollider>())
					{
						//名前で判別してキャラクターのボーンの子にする
						if (ii.name.Contains("L_") && ii.name.Contains("Hip"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_HipBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Hip"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_HipBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Knee"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_KneeBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Knee"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_KneeBone").transform;
						}
						else if (ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
						}
						else if (ii.name.Contains("Spine01"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.001").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("UpperLeg"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_UpperLegBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("UpperLeg"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_UpperLegBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("UpperArm"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_UpperArmBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("UpperArm"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_UpperArmBone").transform;
						}
						else if (ii.name.Contains("Pelvis"))
						{
							ii.transform.parent = DeepFind(gameObject, "PelvisBone").transform;
						}
						else if (ii.name.Contains("Neck"))
						{
							ii.transform.parent = DeepFind(gameObject, "NeckBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_BreastBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_BreastBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_NippleBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_NippleBone").transform;
						}

						//トランスフォームリセット
						ResetTransform(ii.gameObject);
					}

					//衣装のクロスに使うSphereColliderを全て取得
					foreach (SphereCollider ii in CostumeOBJ.GetComponentsInChildren<SphereCollider>())
					{
						//名前で判別してキャラクターのボーンの子にする
						if (ii.name.Contains("L_") && ii.name.Contains("Hip"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_HipBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Hip"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_HipBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Knee"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_KneeBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Knee"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_KneeBone").transform;
						}
						else if (ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
						}
						else if (ii.name.Contains("Spine01"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.001").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("UpperLeg"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_UpperLegBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("UpperLeg"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_UpperLegBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("UpperArm"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_UpperArmBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("UpperArm"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_UpperArmBone").transform;
						}
						else if (ii.name.Contains("Pelvis"))
						{
							ii.transform.parent = DeepFind(gameObject, "PelvisBone").transform;
						}
						else if (ii.name.Contains("Neck"))
						{
							ii.transform.parent = DeepFind(gameObject, "NeckBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_ShoulderBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_ShoulderBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_BreastBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Breast"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_BreastBone").transform;
						}
						else if (ii.name.Contains("L_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "L_NippleBone").transform;
						}
						else if (ii.name.Contains("R_") && ii.name.Contains("Nipple"))
						{
							ii.transform.parent = DeepFind(gameObject, "R_NippleBone").transform;
						}

						//トランスフォームリセット
						ResetTransform(ii.gameObject);
					}
					
					//読み込み完了フラグを立てる
					CostumeLoadCompleteFlag = true;

					//モザイクエフェクト宣言
					GameObject MosaicOBJ = null;

					//性器オブジェクトにモザイクエフェクトを仕込む
					foreach (Transform ii in gameObject.GetComponentsInChildren<Transform>())
					{
						//名前で検索
						if (ii.name.Contains("Genital"))
						{
							//モザイクエフェクトインスタンス化
							MosaicOBJ = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "Mosaic").ToArray()[0]);

							//性器オブジェクトの子にする
							MosaicOBJ.transform.parent = ii.gameObject.transform;

							//ローカルTransform設定
							ResetTransform(MosaicOBJ);
						}
					}

					//スクリプトにデータを渡す
					ExecuteEvents.Execute<PlayerScriptInterface>(gameObject, null, (reciever, eventData) => reciever.SetCharacterData(i, GameManagerScript.Instance.AllFaceDic[ID], GameManagerScript.Instance.AllDamageDic[ID], GameManagerScript.Instance.AllChangeDic[ID], GameManagerScript.Instance.AllH_HitDic[ID], GameManagerScript.Instance.AllH_DamageDic[ID], GameManagerScript.Instance.AllH_BreakDic[ID], CostumeOBJ, MosaicOBJ, PantsOffOBJ));

				}));
			}
		}
    }

	//キャラクターIDを返すインターフェイス
	public int GetCharacterID()
	{
		return ID;
	}

	//準備完了待機コルーチン
	IEnumerator AllReadyCoroutine()
	{
		//読み込み完了するまで回る
		while (!(HairLoadCompleteFlag && CostumeLoadCompleteFlag && WeaponLoadCompleteFlag && MiniMapOBJLoadCompleteFlag))
		{
			yield return null;
		}

		//メッシュ結合用オブジェクトList
		List<GameObject> CombineBaseOBJList = new List<GameObject>();
		List<GameObject> CombineTopsOffOBJList = new List<GameObject>();
		List<GameObject> CombineBraOffOBJList = new List<GameObject>();
		List<GameObject> CombinePantsOffOBJList = new List<GameObject>();

		//メッシュ結合完了フラグ
		bool CombineBaseFinishFlag = false;
		bool CombineTopsOffFinishFlag = false;
		bool CombineBraOffFinishFlag = false;
		bool CombinePantsOffFinishFlag = false;

		//ボーンコピー用SkinnedMeshRenderer
		SkinnedMeshRenderer BoneSample = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

		//スキンメッシュを回してBodyシェーダーが使われてる奴を抽出
		foreach (SkinnedMeshRenderer i in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Where(a => a.GetComponent<CharacterBodyShaderScript>() != null))
		{
			/*
			if(
				i.name.Contains("Body") || 
				i.name.Contains("Others") ||
				i.name.Contains("Glasses") ||
				i.name.Contains("Socks") || 
				i.name.Contains("Shoes") ||
				i.name.Contains("B_On") ||
				i.name.Contains("P_On")
				)
			{*/

			//メッシュ結合フラグが立っているオブジェクトを抽出
			if(i.GetComponent<CharacterBodyShaderScript>().CombineBaseFlag)
			{
				//ListにAdd
				CombineBaseOBJList.Add(i.gameObject);
			}

			if (i.GetComponent<CharacterBodyShaderScript>().CombineTopsOffFlag)
			{
				//ListにAdd
				CombineTopsOffOBJList.Add(i.gameObject);
			}

			if (i.GetComponent<CharacterBodyShaderScript>().CombineBraOffFlag)
			{
				//ListにAdd
				CombineBraOffOBJList.Add(i.gameObject);
			}

			if (i.GetComponent<CharacterBodyShaderScript>().CombinePantsOffFlag)
			{
				//ListにAdd
				CombinePantsOffOBJList.Add(i.gameObject);
			}
		}

		//メッシュ統合
		SkinMeshIntegration(CombineBaseOBJList, BoneSample, (GameObject OBJ) => 
		{
			//親を設定
			OBJ.transform.parent = gameObject.transform;
			
			//トランスフォームリセット
			ResetTransform(OBJ);

			//名前を設定
			OBJ.name = "PlayerCombine_Base_MeshOBJ";

			//レイヤーを設定
			OBJ.layer = LayerMask.NameToLayer("Player");

			//完了フラグを立てる
			CombineBaseFinishFlag = true;
		});

		yield return null;

		//メッシュ統合
		SkinMeshIntegration(CombineTopsOffOBJList, BoneSample, (GameObject OBJ) =>
		{
			//親を設定
			OBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(OBJ);

			//名前を設定
			OBJ.name = "PlayerCombine_TopsOff_MeshOBJ";

			//レイヤーを設定
			OBJ.layer = LayerMask.NameToLayer("Player");

			//非表示にしとく
			OBJ.GetComponent<SkinnedMeshRenderer>().enabled = false;

			//完了フラグを立てる
			CombineTopsOffFinishFlag = true;
		});

		yield return null;

		//メッシュ統合
		SkinMeshIntegration(CombineBraOffOBJList, BoneSample, (GameObject OBJ) =>
		{
			//親を設定
			OBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(OBJ);

			//名前を設定
			OBJ.name = "PlayerCombine_BraOff_MeshOBJ";

			//レイヤーを設定
			OBJ.layer = LayerMask.NameToLayer("Player");

			//非表示にしとく
			OBJ.GetComponent<SkinnedMeshRenderer>().enabled = false;

			//完了フラグを立てる
			CombineBraOffFinishFlag = true;
		});

		yield return null;

		//メッシュ統合
		SkinMeshIntegration(CombinePantsOffOBJList, BoneSample, (GameObject OBJ) =>
		{
			//親を設定
			OBJ.transform.parent = gameObject.transform;

			//トランスフォームリセット
			ResetTransform(OBJ);

			//名前を設定
			OBJ.name = "PlayerCombine_PantsOff_MeshOBJ";

			//レイヤーを設定
			OBJ.layer = LayerMask.NameToLayer("Player");

			//非表示にしとく
			OBJ.GetComponent<SkinnedMeshRenderer>().enabled = false;

			//完了フラグを立てる
			CombinePantsOffFinishFlag = true;
		});

		//メッシュ結合が終わるまで待つ
		while (!(CombineBaseFinishFlag && CombineTopsOffFinishFlag && CombineBraOffFinishFlag && CombinePantsOffFinishFlag))
		{
			yield return null;
		}

		//アニメーター有効化
		gameObject.GetComponent<Animator>().enabled = true;

		//骨揺らしフラグを入れる
		gameObject.GetComponent<PlayerScript>().BoneMoveSwitch = true;

		//自身を消しておく
		gameObject.SetActive(false);

		//読み込み完了したらMissionSettingにフラグを送る
		ExecuteEvents.Execute<MissionSettingScriptInterface>(GameObject.Find("MIssionSetting"), null, (reciever, eventData) => reciever.GetCharacterCompleteFlag(ID, true));
	}
}

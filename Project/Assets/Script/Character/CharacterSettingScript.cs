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

	//髪オブジェクト読み込み完了フラグ
	private bool HairLoadCompleteFlag = false;

	//衣装オブジェクト読み込み完了フラグ
	private bool CostumeLoadCompleteFlag = false;

	//武器オブジェクト読み込み完了フラグ
	private bool WeaponLoadCompleteFlag = false;

	//顔テクスチャ読み込み完了フラグ
	private bool BaseTexLoadCompleteFlag = false;

	//頬テクスチャ読み込み完了フラグ
	private bool CheekTexLoadCompleteFlag = false;

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
				//顔テクスチャ読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Texture/Character/" + ID + "/Face/", "_TexFace0" + ID + "Base", "tga", (object O) =>
				{
					//読み込んだオブジェクトを変換
					gameObject.GetComponent<PlayerScript>().FaceBaseTex = O as Texture2D;

					//読み込み完了フラグを立てる
					BaseTexLoadCompleteFlag = true;
				}));

				//頬テクスチャ読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Texture/Character/" + ID + "/Face/", "_TexFace0" + ID + "Cheek", "tga", (object O) =>
				{
					//読み込んだオブジェクトを変換
					gameObject.GetComponent<PlayerScript>().FaceCheekTex = O as Texture2D;

					//読み込み完了フラグを立てる
					CheekTexLoadCompleteFlag = true;
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
					HairOBJ.transform.localPosition *= 0;
					HairOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

					//髪のクロスに使うSphereColliderを全て取得
					foreach (SphereCollider ii in HairOBJ.GetComponentsInChildren<SphereCollider>())
					{
						//名前で判別してキャラクターのボーンの子にする
						if (ii.name.Contains("Spine") && ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
						}
						else if (ii.name.Contains("Neck"))
						{
							ii.transform.parent = DeepFind(gameObject, "NeckBone").transform;
						}

						//相対位置と回転をゼロにする
						ii.transform.localPosition = new Vector3(0, 0, 0);
						ii.transform.localRotation = Quaternion.Euler(0, 0, 0);
					}

					//読み込み完了フラグを立てる
					HairLoadCompleteFlag = true;

				}));

				//衣装オブジェクト読み込み
				StartCoroutine(GameManagerScript.Instance.LoadOBJ("Object/Character/" + ID + "/Costume/", "Costume_" + ID + "_" + i.CostumeID, "prefab", (object O) =>
				{
					//読み込んだオブジェクトをインスタンス化
					GameObject CostumeOBJ = Instantiate(O as GameObject);

					//衣装を子にする
					CostumeOBJ.transform.parent = gameObject.transform;

					//ローカルトランスフォームをリセット
					CostumeOBJ.transform.localPosition *= 0;
					CostumeOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);

					//Bodyに仕込んであるCostumeのSkinnedMeshRendererを取得する
					SkinnedMeshRenderer CostumeRenderer = DeepFind(gameObject, "CostumeSample_Mesh").GetComponent<SkinnedMeshRenderer>();

					//衣装プレハブ内のスキニングメッシュレンダラーを全て取得
					foreach (SkinnedMeshRenderer ii in CostumeOBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
					{
						//ボーン構成をコピーしてキャラクターのボーンと紐付ける
						ii.bones = CostumeRenderer.bones;
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
						else if (ii.name.Contains("Spine") && ii.name.Contains("Spine02"))
						{
							ii.transform.parent = DeepFind(gameObject, "SpineBone.002").transform;
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


						
						//相対位置と回転をゼロにする
						ii.transform.localPosition = new Vector3(0, 0, 0);
						ii.transform.localRotation = Quaternion.Euler(0, 0, 0);
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
							MosaicOBJ.transform.localPosition *= 0;
							MosaicOBJ.transform.localRotation = Quaternion.Euler(Vector3.zero);
						}
					}

					//スクリプトにデータを渡す
					ExecuteEvents.Execute<PlayerScriptInterface>(gameObject, null, (reciever, eventData) => reciever.SetCharacterData(i, GameManagerScript.Instance.AllFaceDic[ID], GameManagerScript.Instance.AllDamageDic[ID], GameManagerScript.Instance.AllH_HitDic[ID], GameManagerScript.Instance.AllH_DamageDic[ID], GameManagerScript.Instance.AllH_BreakDic[ID], CostumeOBJ, MosaicOBJ));

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
		while (!(HairLoadCompleteFlag && CostumeLoadCompleteFlag && WeaponLoadCompleteFlag && BaseTexLoadCompleteFlag && CheekTexLoadCompleteFlag))
		{
			yield return null;
		}

		//自身を消しておく
		gameObject.SetActive(false);

		//読み込み完了したらMissionSettingにフラグを送る
		ExecuteEvents.Execute<MissionSettingScriptInterface>(GameObject.Find("MIssionSetting"), null, (reciever, eventData) => reciever.GetCharacterCompleteFlag(ID, true));
	}
}

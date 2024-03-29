﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//他のスクリプトに継承させて共通の関数を使用できるようにするClass、MonoBehaviourはこいつから継承させる
public class GlobalClass : MonoBehaviour
{
	//オノマトペ種類用Enum
	public enum OnomatopeTextureEnum
	{
		LightAttackHit,     //弱攻撃が当たった時
		MiddleAttackHit,    //中攻撃が当たった時
		HeavyAttackHit,     //強攻撃が当たった時
	}

	//SE種類用Enum
	public enum SoundEffectEnum
	{
		Generic,			//汎用
		Step,				//足音
		AttackSwing,        //攻撃風切り音
		AttackImpact,		//攻撃が当たった時
		Weapon,				//武器用
	}


	/*
	//セーブデータロード実行関数
	public async void UserDataLoadAsync(string path, Action<UserDataClass> Act)
	{
		//retrun用変数宣言
		UserDataClass re = null;

		//非同期処理でセーブデータ読み込み、結果をreturn用変数に格納
		re = await Task.Run(() =>
		{
			//Task内return用変数
			UserDataClass LoadData = new UserDataClass();

			//セーブファイルの存在確認
			if (File.Exists(path))
			{
				// バイナリ形式でデシリアライズ
				BinaryFormatter bf = new BinaryFormatter();

				// 指定したパスのファイルストリームを開く
				FileStream file = File.Open(path, FileMode.Open);

				//例外処理をしてロード
				try
				{
					// 指定したファイルストリームをオブジェクトにデシリアライズ。
					LoadData = (UserDataClass)bf.Deserialize(file);
				}
				finally
				{
					//明示的破棄
					if (file != null)
					{
						file.Close();
					}
				}
			}

			//出力
			return LoadData;
		});
		*/

	//画面解像度変更
	public void ChangeResolution(bool full, int Reso)
	{
		StartCoroutine(ChangeResolutionCoroutin(full,Reso));		
	}
	private IEnumerator ChangeResolutionCoroutin(bool full, int Reso)
	{
		//スクリーンモードによってマウスカーソルの表示切り替え
		Cursor.visible = !full;

		//一旦ウィンドウモードにする
		Screen.SetResolution(0, 0, false);

		//1フレーム待機して反映を待つ
		yield return null;

		//モニター解像度取得
		int TempWidth = Screen.currentResolution.width;

		//ループカウント
		int count = 0;

		//一番大きい解像度候補を選出
		foreach(var i in GameManagerScript.Instance.ScreenResolutionList)
		{
			if(TempWidth >= i.x)
			{
				//見付かったらブレーク
				break;
			}

			//カウントアップ
			count++;
		}

		//指定された解像度があるか確認
		if (GameManagerScript.Instance.ScreenResolutionList.Count > Reso + count)
		{
			//解像度変更
			Screen.SetResolution((int)GameManagerScript.Instance.ScreenResolutionList[Reso + count].x, (int)GameManagerScript.Instance.ScreenResolutionList[Reso + count].y, full);

			//Reso += count;
		}
		//無ければ最低解像度
		else
		{
			Screen.SetResolution((int)GameManagerScript.Instance.ScreenResolutionList[GameManagerScript.Instance.ScreenResolutionList.Count - 1].x, (int)GameManagerScript.Instance.ScreenResolutionList[GameManagerScript.Instance.ScreenResolutionList.Count - 1].y, full);

			//Reso = GameManagerScript.Instance.ScreenResolutionList.Count - 1;
		}

		//セーブデータのフルスクリーンモード変更
		GameManagerScript.Instance.UserData.FullScreen = full;

		//セーブデータの解像度値変更
		GameManagerScript.Instance.UserData.Reso = Reso;

		//アウトライン用テクスチャ更新
		GameManagerScript.Instance.GetMainCameraOBJ().GetComponentInChildren<OutLineScript>().TextureRefresh();

		//最終出力用テクスチャ更新
		GameManagerScript.Instance.GetMainCameraOBJ().GetComponent<MixTexScript>().TextureRefresh();
	}


	//消失用関数
	public void ObjectVanish(GameObject OBJ, float T, int V, Action<List<Renderer>> BA, Action<List<Renderer>> AA)
	{
		//受け取ったオブジェクトの下にある消失テクスチャを持っているレンダラーを全て取得
		List<Renderer> R = new List<Renderer>(OBJ.GetComponentsInChildren<Renderer>().Where(a => a.materials.Any(b => b.GetTexturePropertyNames().Any(c => c == "_VanishTexture"))).ToList());

		//スクリーンサイズから消失用テクスチャのスケーリングを設定
		foreach (var i in R)
		{
			i.material.SetTextureScale("_VanishTexture", new Vector2(Screen.width / GameManagerScript.Instance.VanishTextureList[0].width, Screen.height / GameManagerScript.Instance.VanishTextureList[0].height) * GameManagerScript.Instance.ScreenResolutionScale);
		}

		//事前処理用の匿名関数実行
		BA(R);

		//コルーチン呼び出し
		StartCoroutine(ObjectVanishCoroutine(R, T, V, AA));
	}
	private IEnumerator ObjectVanishCoroutine(List<Renderer> R, float T, int V, Action<List<Renderer>> AA)
	{
		//経過時間宣言
		float VanishTime = 0;

		//開始番号
		int StartNum = 0;

		//終了番号
		int EndNum = 0;

		//消失
		if (V == 0)
		{
			EndNum = GameManagerScript.Instance.VanishTextureList.Count - 1;			
		}
		//出現
		else if (V == 1)
		{
			StartNum = GameManagerScript.Instance.VanishTextureList.Count - 1;
		}

		//経過時間まで回す
		while (VanishTime < T)
		{
			//レンダラーを回して消失用テクスチャを入れる
			foreach (Renderer i in R)
			{
				i.material.SetTexture("_VanishTexture", GameManagerScript.Instance.VanishTextureList[(int)Mathf.Ceil(Mathf.Lerp(StartNum, EndNum, VanishTime / T))]);
			}

			//消滅用カウントアップ
			VanishTime += Time.deltaTime;		

			//１フレーム待機
			yield return null;
		}

		//マテリアルを回して最終的なテクスチャを入れる
		foreach (Renderer i in R)
		{
			i.material.SetTexture("_VanishTexture", GameManagerScript.Instance.VanishTextureList[EndNum]);
		}

		//事後処理用の匿名関数実行
		AA(R);
	}

	//衣装用コライダをボーンに仕込む関数
	public void SetCostumeCol(GameObject Character, GameObject Costume)
	{
		//受け取った衣装が持っているDynamicBoneColliderを回す
		foreach (DynamicBoneCollider ii in Costume.GetComponentsInChildren<DynamicBoneCollider>())
		{
			//名前で判別してキャラクターのボーンの子にする
			if (ii.name.Contains("L_") && ii.name.Contains("Hip"))
			{
				ii.transform.parent = DeepFind(Character, "L_HipBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Hip"))
			{
				ii.transform.parent = DeepFind(Character, "R_HipBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Knee"))
			{
				ii.transform.parent = DeepFind(Character, "L_KneeBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Knee"))
			{
				ii.transform.parent = DeepFind(Character, "R_KneeBone").transform;
			}
			else if (ii.name.Contains("Spine02"))
			{
				ii.transform.parent = DeepFind(Character, "SpineBone.002").transform;
			}
			else if (ii.name.Contains("Spine01"))
			{
				ii.transform.parent = DeepFind(Character, "SpineBone.001").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("UpperLeg"))
			{
				ii.transform.parent = DeepFind(Character, "R_UpperLegBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("UpperLeg"))
			{
				ii.transform.parent = DeepFind(Character, "L_UpperLegBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "R_ShoulderBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "L_ShoulderBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("UpperArm"))
			{
				ii.transform.parent = DeepFind(Character, "R_UpperArmBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("UpperArm"))
			{
				ii.transform.parent = DeepFind(Character, "L_UpperArmBone").transform;
			}
			else if (ii.name.Contains("Pelvis"))
			{
				ii.transform.parent = DeepFind(Character, "PelvisBone").transform;
			}
			else if (ii.name.Contains("Neck"))
			{
				ii.transform.parent = DeepFind(Character, "NeckBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "L_ShoulderBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "R_ShoulderBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Breast"))
			{
				ii.transform.parent = DeepFind(Character, "L_BreastBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Breast"))
			{
				ii.transform.parent = DeepFind(Character, "R_BreastBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Nipple"))
			{
				ii.transform.parent = DeepFind(Character, "L_NippleBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Nipple"))
			{
				ii.transform.parent = DeepFind(Character, "R_NippleBone").transform;
			}

			//トランスフォームリセット
			ResetTransform(ii.gameObject);
		}

		//受け取った衣装が持っているSphereColliderを回す
		foreach (SphereCollider ii in Costume.GetComponentsInChildren<SphereCollider>())
		{
			//名前で判別してキャラクターのボーンの子にする
			if (ii.name.Contains("L_") && ii.name.Contains("Hip"))
			{
				ii.transform.parent = DeepFind(Character, "L_HipBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Hip"))
			{
				ii.transform.parent = DeepFind(Character, "R_HipBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Knee"))
			{
				ii.transform.parent = DeepFind(Character, "L_KneeBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Knee"))
			{
				ii.transform.parent = DeepFind(Character, "R_KneeBone").transform;
			}
			else if (ii.name.Contains("Spine02"))
			{
				ii.transform.parent = DeepFind(Character, "SpineBone.002").transform;
			}
			else if (ii.name.Contains("Spine01"))
			{
				ii.transform.parent = DeepFind(Character, "SpineBone.001").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("UpperLeg"))
			{
				ii.transform.parent = DeepFind(Character, "R_UpperLegBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("UpperLeg"))
			{
				ii.transform.parent = DeepFind(Character, "L_UpperLegBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "R_ShoulderBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "L_ShoulderBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("UpperArm"))
			{
				ii.transform.parent = DeepFind(Character, "R_UpperArmBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("UpperArm"))
			{
				ii.transform.parent = DeepFind(Character, "L_UpperArmBone").transform;
			}
			else if (ii.name.Contains("Pelvis"))
			{
				ii.transform.parent = DeepFind(Character, "PelvisBone").transform;
			}
			else if (ii.name.Contains("Neck"))
			{
				ii.transform.parent = DeepFind(Character, "NeckBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "L_ShoulderBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Shoulder"))
			{
				ii.transform.parent = DeepFind(Character, "R_ShoulderBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Breast"))
			{
				ii.transform.parent = DeepFind(Character, "L_BreastBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Breast"))
			{
				ii.transform.parent = DeepFind(Character, "R_BreastBone").transform;
			}
			else if (ii.name.Contains("L_") && ii.name.Contains("Nipple"))
			{
				ii.transform.parent = DeepFind(Character, "L_NippleBone").transform;
			}
			else if (ii.name.Contains("R_") && ii.name.Contains("Nipple"))
			{
				ii.transform.parent = DeepFind(Character, "R_NippleBone").transform;
			}

			//トランスフォームリセット
			ResetTransform(ii.gameObject);
		}
	}

	//３D空間のポジションをUIの座標に変換する関数
	public Vector3 UIPosition(CanvasScaler Scaler, RectTransform Rect, Vector3 Pos)
	{
		//UI座標に3D用レンダーテクスチャとUIキャンバスサイズ比率を掛けて出力
		//return GameManagerScript.Instance.GetMainCameraOBJ().GetComponent<Camera>().WorldToScreenPoint(Pos) * (Scaler.referenceResolution.x * Rect.localScale.x) / (Screen.width * GameManagerScript.Instance.ScreenResolutionScale);
		return GameManagerScript.Instance.GetMainCameraOBJ().GetComponent<Camera>().WorldToScreenPoint(Pos);
	}

	//受け取ったボディシェーダー使用のスキンメッシュを統合する関数、これをやる時は元のキャラクターがワールド原点にいて、モーション再生されていないTスタンス状態で処理すること
	public void SkinMeshIntegration(List<GameObject> OBJList, SkinnedMeshRenderer BoneSample, Action<GameObject> Act)
	{
		//コルーチン呼び出し
		StartCoroutine(SkinMeshIntegrationCoroutine(OBJList, BoneSample, Act));
	}
	
	private IEnumerator SkinMeshIntegrationCoroutine(List<GameObject> OBJList, SkinnedMeshRenderer BoneSample, Action<GameObject> Act)
	{
		//スキンメッシュレンダラーList宣言
		List<SkinnedMeshRenderer> MeshList = new List<SkinnedMeshRenderer>();

		//オブジェクトListのスキニングメッシュレンダラーを全て取得
		foreach (GameObject i in OBJList)
		{
			foreach (SkinnedMeshRenderer ii in i.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				//ListにAdd
				MeshList.Add(ii);
			}

			//終わったら無効化しておく
			i.SetActive(false);
		}

		//統合用オブジェクト宣言
		GameObject CombineMeshOBJ = new GameObject();

		//メッシュレンダラーを付けて取得しておく
		SkinnedMeshRenderer CombineMeshRenderer = CombineMeshOBJ.AddComponent<SkinnedMeshRenderer>();

		//マテリアル設定、とりあえずキャラクターのボディシェーダー限定でやる
		CombineMeshRenderer.material = MeshList[0].material;

		//空メッシュを入れる
		CombineMeshRenderer.sharedMesh = new Mesh();

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

		//統合するベーステクスチャ
		List<Texture2D> PackBaseTextureList = new List<Texture2D>();

		//統合する法線テクスチャ
		List<Texture2D> PackNormalTextureList = new List<Texture2D>();

		//統合するハイライトテクスチャ
		List<Texture2D> PackHiLightTextureList = new List<Texture2D>();

		//統合する線画テクスチャ
		List<Texture2D> PackLineTextureList = new List<Texture2D>();

		//最終的に統合するベーステクスチャ
		List<Texture2D> FinalPackBaseTextureList = new List<Texture2D>();

		//最終的に統合する法線テクスチャ
		List<Texture2D> FinalPackNormalTextureList = new List<Texture2D>();

		//最終的に統合するハイライトテクスチャ
		List<Texture2D> FinalPackHiLightTextureList = new List<Texture2D>();

		//最終的に統合する線画テクスチャ
		List<Texture2D> FinalPackLineTextureList = new List<Texture2D>();

		//1フレーム待機
		yield return null;

		//統合用ベーステクスチャ
		Texture2D PackBaseTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//1フレーム待機
		yield return null;

		//統合用法線テクスチャ
		Texture2D PackNormalTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//1フレーム待機
		yield return null;

		//統合用ハイライトテクスチャ
		Texture2D PackHiLightTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//1フレーム待機
		yield return null;

		//統合用線画テクスチャ
		Texture2D PackLineTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

		//1フレーム待機
		yield return null;

		//インデックスに使うループカウント
		int count = 0;

		//サンプルからボーン情報を取る
		foreach (Transform bone in BoneSample.bones)
		{
			//ボーンを取得
			BoneList.Add(bone);

			//名前とインデクスをハッシュテーブルに入れる
			BoneHash.Add(bone.name, count);

			//カウントアップ
			count++;
		}

		//ループカウント初期化
		count = 0;

		//ボーンのバインドポーズを取得
		foreach (Transform i in BoneList)
		{
			BindPoseList.Add(BoneList[count].worldToLocalMatrix * transform.worldToLocalMatrix);

			//カウントアップ
			count++;
		}

		//統合するメッシュレンダラーを回す
		foreach (SkinnedMeshRenderer i in MeshList)
		{
			//ボーン構成をコピーしてキャラクターのボーンと紐付ける
			i.bones = BoneSample.bones;

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
		}

		//1フレーム待機
		yield return null;

		//メッシュを結合
		CombineMeshRenderer.sharedMesh.CombineMeshes(CombineInstanceList.ToArray());

		//1フレーム待機
		yield return null;

		//ボーン設定
		CombineMeshRenderer.bones = BoneList.ToArray();

		//ボーンウェイト設定
		CombineMeshRenderer.sharedMesh.boneWeights = BoneWeightList.ToArray();

		//バインドポーズ設定
		CombineMeshRenderer.sharedMesh.bindposes = BindPoseList.ToArray();

		//バウンディングボックスを設定
		CombineMeshRenderer.localBounds = new Bounds(new Vector3(0, 1, 0), new Vector3(2, 2, 2));

		//各テクスチャ最大サイズ取得
		int BaseTextureSize = PackBaseTextureList.Max(a => a.width);
		int NormalTextureSize = PackNormalTextureList.Max(a => a.width);
		int HiLightTextureSize = PackHiLightTextureList.Max(a => a.width);
		int LineTextureSize = PackLineTextureList.Max(a => a.width);

		//ベーステクスチャListを回す
		foreach (Texture2D i in PackBaseTextureList)
		{
			//1フレーム待機
			yield return null;

			//最大サイズに合わせて小さいテクスチャをリサイズ
			if (i.width < BaseTextureSize)
			{
				//リサイズしたテクスチャをListにAdd
				FinalPackBaseTextureList.Add(TextureResize(i, BaseTextureSize));
			}
			else
			{
				//ListにAdd
				FinalPackBaseTextureList.Add(i);
			}
		}

		//法線テクスチャListを回す
		foreach (Texture2D i in PackNormalTextureList)
		{
			//1フレーム待機
			yield return null;

			//最大サイズに合わせて小さいテクスチャをリサイズ
			if (i.width < NormalTextureSize)
			{
				//リサイズしたテクスチャをListにAdd
				FinalPackNormalTextureList.Add(TextureResize(i, NormalTextureSize));
			}
			else
			{
				//ListにAdd
				FinalPackNormalTextureList.Add(i);
			}
		}

		//線画テクスチャListを回す
		foreach (Texture2D i in PackLineTextureList)
		{
			//1フレーム待機
			yield return null;

			//最大サイズに合わせて小さいテクスチャをリサイズ
			if (i.width < LineTextureSize)
			{
				//リサイズしたテクスチャをListにAdd
				FinalPackLineTextureList.Add(TextureResize(i, LineTextureSize));
			}
			else
			{
				//ListにAdd
				FinalPackLineTextureList.Add(i);
			}
		}

		//ハイライトテクスチャListを回す
		foreach (Texture2D i in PackHiLightTextureList)
		{
			//1フレーム待機
			yield return null;

			//最大サイズに合わせて小さいテクスチャをリサイズ
			if (i.width < HiLightTextureSize)
			{
				//リサイズしたテクスチャをListにAdd
				FinalPackHiLightTextureList.Add(TextureResize(i, HiLightTextureSize));
			}
			else
			{
				//ListにAdd
				FinalPackHiLightTextureList.Add(i);
			}
		}

		//各テクスチャリストをムリヤリ16枚にする
		while (FinalPackBaseTextureList.Count < 16)
		{
			//1フレーム待機
			yield return null;

			FinalPackBaseTextureList.Add(new Texture2D(BaseTextureSize, BaseTextureSize, TextureFormat.R8, false));
		}
		while (FinalPackNormalTextureList.Count < 16)
		{
			//1フレーム待機
			yield return null;

			FinalPackNormalTextureList.Add(new Texture2D(NormalTextureSize, NormalTextureSize, TextureFormat.R8, false));
		}
		while (FinalPackHiLightTextureList.Count < 16)
		{
			//1フレーム待機
			yield return null;

			FinalPackHiLightTextureList.Add(new Texture2D(HiLightTextureSize, HiLightTextureSize, TextureFormat.R8, false));
		}
		while (FinalPackLineTextureList.Count < 16)
		{
			//1フレーム待機
			yield return null;

			FinalPackLineTextureList.Add(new Texture2D(LineTextureSize, LineTextureSize, TextureFormat.R8, false));
		}

		//1フレーム待機
		yield return null;

		//ベーステクスチャを統合してRectを受け取る、
		Rect[] TexBaseRect = PackBaseTexture.PackTextures(FinalPackBaseTextureList.ToArray(), 0, BaseTextureSize * 4, false);
		
		//1フレーム待機
		yield return null;
		
		//法線テクスチャ統合
		PackNormalTexture.PackTextures(FinalPackNormalTextureList.ToArray(), 0, NormalTextureSize * 4, false);

		//1フレーム待機
		yield return null;

		//ハイライトテクスチャ統合
		PackHiLightTexture.PackTextures(FinalPackHiLightTextureList.ToArray(), 0, HiLightTextureSize * 4, false);

		//1フレーム待機
		yield return null;

		//線画テクスチャ統合
		PackLineTexture.PackTextures(FinalPackLineTextureList.ToArray(), 0, LineTextureSize * 4, false);

		//1フレーム待機
		yield return null;

		//統合用UV宣言
		List<Vector2> CombineUV = new List<Vector2>();

		//ループカウント初期化
		count = 0;

		//UVListを回す
		foreach (Vector2[] i in CombineUVList)
		{
			//格納用UVList宣言
			List<Vector2> tempUV = new List<Vector2>();

			//パックしたテクスチャのRectを元にUVを16マスに配置する
			foreach (Vector2 ii in i)
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

		//匿名関数実行
		Act(CombineMeshOBJ);
	}

	//テクスチャをリサイズする関数
	public Texture2D TextureResize(Texture2D Texture, int Size)
	{
		//リサイズ後のテクスチャ宣言
		Texture2D TempTexture = new Texture2D(Size, Size);

		//レンダーテクスチャ宣言
		RenderTexture TempRenderTexture = RenderTexture.GetTemporary(Size, Size);

		//小さいテクスチャをレンダーテクスチャにレンダリング
		Graphics.Blit(Texture, TempRenderTexture);

		//アクティブにする
		RenderTexture.active = TempRenderTexture;

		//テクスチャにアクティブなレンダーテクスチャを読み込む
		TempTexture.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);

		//反映
		TempTexture.Apply();

		//レンダーテクスチャを開放
		RenderTexture.ReleaseTemporary(TempRenderTexture);

		//出力
		return TempTexture;
	}

	//シーン遷移関数
	public void NextScene(string scene)
	{
		//アセット開放
		AssetsUnload();

		//引数で受け取った名前のシーンを読み込む
		SceneManager.LoadScene(scene);
	}

	//引数で受け取ったオブジェクトのトランスフォームをリセットする関数
	public void ResetTransform(GameObject o)
	{
		o.transform.localPosition *= 0;
		o.transform.localRotation = Quaternion.Euler(Vector3.zero);
		o.transform.localScale = Vector3.one;
	}

	//引数のオブジェクトの全ての子オブジェクトを名前で検索して返す関数
	public GameObject DeepFind(GameObject root, string name)
	{
		//引数以下の子オブジェクトのトランスフォームを回す
		foreach (Transform i in root.GetComponentsInChildren<Transform>())
		{
			//名前を比較
			if (i.gameObject.name == name)
			{
				//ヒットしたら返す
				return i.gameObject;
			}
		}

		//無ければnull
		return null;
	}

	//高低差を無視した水平面のベクトルを返す関数
	public Vector3 HorizontalVector(GameObject TargetOBJ, GameObject FromOBJ)
	{
		return new Vector3(TargetOBJ.transform.position.x, FromOBJ.transform.position.y, TargetOBJ.transform.position.z) - FromOBJ.transform.position;
	}
	//目標がポジションのオーバーロード
	public Vector3 HorizontalVector(Vector3 TargetPos, GameObject FromOBJ)
	{
		return new Vector3(TargetPos.x, FromOBJ.transform.position.y, TargetPos.z) - FromOBJ.transform.position;
	}

	//適当なArtsClassを作って返す関数
	public ArtsClass MakeInstantArts(List<Color> KBV, List<float> DML, List<int> DLY, List<int> ATI, List<int> DEN, List<int> CTP)
	{
		//架空の技Classを作る
		ArtsClass temparts = new ArtsClass
		(
			"",
			"",
			0,
			"",
			new List<Color>(),
			DML,//ダメージ
			new List<int>() { 0 },
			DLY,//トドメをさすことができるか
			new List<Color>(),
			KBV,//ノックバックベクトル
			"",
			new List<int>(),
			new List<int>(),
			ATI,//AttackType
			DEN,//ダウンしている相手に当たるか
			CTP,//コライダタイプ
			new List<int>(),
			false,
			new List<string>(),
			new List<Vector3>(),
			new List<Vector3>(),
			new List<float>(),
			new List<int>() { 0 },
			new List<Vector3>(),
			0,
			new List<string>(),
			0
		);

		return temparts;
	}

	//使用していないアセットを開放する、ちょいちょい呼ぶといいらしい
	public void AssetsUnload()
	{
		StartCoroutine(AssetsUnloadCoroutine());
	}
	private IEnumerator AssetsUnloadCoroutine()
	{
		//呼び出し元が消えるまで待機
		yield return new WaitForSeconds(0.1f);

		//メモリ開放
		Resources.UnloadUnusedAssets();
	}	

	//オブジェクトが削除された時にインスタンス化したマテリアルやメッシュを削除する、これをしないとメモリリークする
	private void OnDestroy()
	{
		foreach (Renderer i in GetComponents<Renderer>())
		{
			for (int ii = 0; ii < i.materials.Length; ii++)
			{
				if(i.materials[ii] != null)
				{
					Destroy(i.materials[ii]);

					i.materials[ii] = null;
				}
			}
		}

		foreach (MeshFilter i in GetComponents<MeshFilter>())
		{
			if (i.mesh != null)
			{
				Destroy(i.mesh);

				i.mesh = null;
			}	
		}

		foreach (ParticleSystemRenderer i in GetComponents<ParticleSystemRenderer>())
		{
			for (int ii = 0; ii < i.materials.Length; ii++)
			{
				if (i.materials[ii] != null)
				{
					Destroy(i.materials[ii]);

					i.materials[ii] = null;
				}
			}
		}

		foreach (Renderer i in GetComponentsInChildren<Renderer>())
		{
			for (int ii = 0; ii < i.materials.Length; ii++)
			{
				if (i.materials[ii] != null)
				{
					Destroy(i.materials[ii]);

					i.materials[ii] = null;
				}
			}
		}

		foreach (MeshFilter i in GetComponentsInChildren<MeshFilter>())
		{
			if (i.mesh != null)
			{
				Destroy(i.mesh);

				i.mesh = null;
			}
		}

		foreach (ParticleSystemRenderer i in GetComponentsInChildren<ParticleSystemRenderer>())
		{
			for (int ii = 0; ii < i.materials.Length; ii++)
			{
				if (i.materials[ii] != null)
				{
					Destroy(i.materials[ii]);
	
					i.materials[ii] = null;
				}
			}
		}
	}
}

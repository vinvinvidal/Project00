using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//他のスクリプトに継承させて共通の関数を使用できるようにするClass、MonoBehaviourはこいつから継承させる
public class GlobalClass : MonoBehaviour
{
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
			new List<string>()
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
		foreach (var i in GetComponents<Renderer>())
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

		foreach (var i in GetComponents<MeshFilter>())
		{
			if (i.mesh != null)
			{
				Destroy(i.mesh);

				i.mesh = null;
			}	
		}

		foreach (var i in GetComponents<ParticleSystemRenderer>())
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

		foreach (var i in GetComponentsInChildren<Renderer>())
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

		foreach (var i in GetComponentsInChildren<MeshFilter>())
		{
			if (i.mesh != null)
			{
				Destroy(i.mesh);

				i.mesh = null;
			}
		}

		foreach (var i in GetComponentsInChildren<ParticleSystemRenderer>())
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

	/*
	//引数のフォルダ内のサブフォルダ全てのパスを返す関数
	public List<string> GetSubFolders(string p)
	{
		//return用List宣言
		List<string> re = new List<string>();

		//フォルダパスの改行コードを削除
		p = LineFeedCodeClear(p);

		//開発用にリソースから読み込む場合
		if (UserDataClass.Instance.DevSwicth)
		{
			foreach (var i in new DirectoryInfo(Application.dataPath + "/Resources/" + p).GetDirectories())
			{	
				re.Add(p + i.Name);
			}
		}
		//本番用にアセットバンドルから読み込む場合
		else
		{
			foreach (var i in new DirectoryInfo(Application.dataPath + "/StreamingAssets/" + p).GetDirectories())
			{
				re.Add(p + i.Name);
			}
		}

		return re;
	}

	//引数で指定したファイルを非同期ロードしてobjectで返す関数
	public IEnumerator FileLoadCoroutine(string Dir, string File , string Ext, Action<object> Act)
	{
		//return用変数宣言
		object re = new object();

		//ファイルパスの改行コードを削除
		Dir = LineFeedCodeClear(Dir);

		//開発用にリソースから読み込む場合、ビルドすると動かないのでエディタ上のみ
		if (UserDataClass.Instance.DevSwicth)
		{
			//ロード処理を待つためのリクエスト
			ResourceRequest RR = new ResourceRequest();

			//引数で指定されたフォルダ名と拡張子を外したファイル名でリソース非同期読み込み
			RR = Resources.LoadAsync(Dir + Path.GetFileNameWithoutExtension(File + Ext));

			//ロードが終わるまで待つ
			while (RR.isDone == false)
			{
				yield return null;
			}

			//ロードが終わったらreturn用変数に追加
			re = RR.asset;
		}
		//本番用にアセットバンドルから読み込む場合
		else
		{
			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadBundleRequest = new AssetBundleCreateRequest();

			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadDependencyRequest = new AssetBundleCreateRequest();

			//ロードするデータが参照している全ての依存データを取得する
			foreach (string ii in UserDataClass.Instance.DependencyManifest.GetAllDependencies(Application.streamingAssetsPath + GenerateBundlePath(Dir + File)))
			{
				//依存データが未読み込みの場合処理を実行
				if (!UserDataClass.Instance.LoadedDataList.Contains(ii))
				{
					//依存データをロード
					LoadDependencyRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + ii);

					//ロードが終わるまで待つ
					while (LoadDependencyRequest.isDone == false)
					{
						yield return null;
					}

					//読み込み済みデータList更新
					UserDataClass.Instance.LoadedDataListUpdate();
				}
			}

			//データが未読み込みの場合処理を実行
			if (!UserDataClass.Instance.LoadedDataList.Contains(Dir.ToLower() + File))
			{
				//依存データをロードしたら本データをロード
				LoadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GenerateBundlePath(Dir + File));

				//ロードが終わるまで待つ
				while (LoadBundleRequest.isDone == false)
				{
					yield return null;
				}

				//ロードが終わったらreturn用変数に追加
				re = LoadBundleRequest.assetBundle.LoadAsset(File);

				//読み込み済みデータList更新
				UserDataClass.Instance.LoadedDataListUpdate();

				//読み込んだデータを明示的開放
				LoadBundleRequest.assetBundle.Unload(false);
			}	
		}

		//呼び出し元から受け取った匿名メソッドにロードしたデータを送る
		Act(re);
	}

	//引数のフォルダ内のファイルを全て非同期ロードしてobjectで返す関数
	public IEnumerator AllFileLoadCoroutine(string Dir, string Ext , Action<List<object>> Act)
	{
		//return用変数宣言
		List<object> re = new List<object>();

		//フォルダ内の全てのファイルのファイルパスを格納用するList
		List<string> FullPathList = new List<string>();

		//ファイルパスの改行コードを削除
		Dir = LineFeedCodeClear(Dir);

		//開発用にリソースから読み込む場合、ビルドすると動かないのでエディタ上のみ
		if (UserDataClass.Instance.DevSwicth)
		{
			//引数で指定されたフォルダ以下のファイル名を取得
			foreach (FileInfo i in new DirectoryInfo(Application.dataPath + "/Resources/" + Dir).GetFiles("*" + Ext))
			{
				//ファイルのフルパスをListにAdd
				FullPathList.Add(i.FullName);
			}

			//ファイルのフルパスリストを回す
			foreach (string i in FullPathList)
			{
				//ロード処理を待つためのリクエスト
				ResourceRequest RR = new ResourceRequest();

				//引数で指定されたフォルダ名と拡張子を外したファイル名でリソース非同期読み込み
				RR = Resources.LoadAsync(Dir + Path.GetFileNameWithoutExtension(i));

				//ロードが終わるまで待つ
				while (RR.isDone == false)
				{
					yield return null;
				}

				//ロードが終わったらreturn用変数に追加
				re.Add(RR.asset);
			}
		}
		//本番用にアセットバンドルから読み込む場合
		else
		{
			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadBundleRequest = new AssetBundleCreateRequest();

			//ロード処理を待つためのリクエスト
			AssetBundleCreateRequest LoadDependencyRequest = new AssetBundleCreateRequest();

			//引数のパスで指定されたフォルダにあるファイル一覧を取得
			foreach (FileInfo i in new DirectoryInfo(Application.streamingAssetsPath + GenerateBundlePath(Dir)).GetFiles("*"))
			{
				//余計なファイルを除外してAssetBundle読み込み、ListにAdd
				if (!Regex.IsMatch(i.Name, "meta|manifest"))
				{
					//ロードするデータが参照している全ての依存データを取得する
					foreach (string ii in UserDataClass.Instance.DependencyManifest.GetAllDependencies(Dir.ToLower() + i.Name))
					{
						//依存データが未読み込みの場合処理を実行
						if (!UserDataClass.Instance.LoadedDataList.Contains(ii))
						{
							//依存データをロード
							LoadDependencyRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + ii);

							//ロードが終わるまで待つ
							while (LoadDependencyRequest.isDone == false)
							{
								yield return null;
							}

							//読み込み済みデータList更新
							UserDataClass.Instance.LoadedDataListUpdate();

							//読み込んだデータを明示的開放
							//LoadDependencyRequest.assetBundle.Unload(false);
						}
					}					
										
					//データが未読み込みの場合処理を実行
					if (!UserDataClass.Instance.LoadedDataList.Contains(Dir.ToLower() + i.Name))
					{
						//依存データをロードしたら本データをロード
						LoadBundleRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + Dir + i.Name);

						//ロードが終わるまで待つ
						while (LoadBundleRequest.isDone == false)
						{
							yield return null;
						}						

						//ロードが終わったらreturn用変数に追加
						re.Add(LoadBundleRequest.assetBundle.LoadAsset(i.Name));

						//読み込み済みデータList更新
						UserDataClass.Instance.LoadedDataListUpdate();

						//読み込んだデータを明示的開放
						LoadBundleRequest.assetBundle.Unload(false);
					}				
				}
			}
		}

		//呼び出し元から受け取った匿名メソッドにロードしたデータを送る
		Act(re);
	}

	//ゲームオブジェクトのプレハブを生成する関数
	public IEnumerator CreateInstance(string Path, string FileName , Action<GameObject, bool> Act)
	{		
		//出力用変数宣言
		GameObject re = null;

		//ロードしたデータを受け取るLsit初期化
		List<object> LoadOBJList = null;

		//データ読み込み
		StartCoroutine(AllFileLoadCoroutine(Path, ".prefab", (List<object> objs) =>
		{
			//ロードしたデータが引数で入ってくるのでコピー
			LoadOBJList = new List<object>(objs);

			//ロードしたデータをListにAdd
			foreach (object ii in LoadOBJList)
			{
				//GameObjectに変換
				GameObject tempOBJ = ii as GameObject;

				//受け取った名前と一致したら実体化させて変数に代入
				if (tempOBJ.name == FileName)
				{
					//インスタンス代入
					re = Instantiate(tempOBJ);

					//(Clone)を消す
					re.name = tempOBJ.name;

					//インスタンスを非活性化
					re.SetActive(false);
				}
			}
		}));

		//ロードが終わるまで待つ
		while (LoadOBJList == null)
		{
			yield return null;
		}

		//処理が終わったらboolを返す
		Act(re , false);
	}



	//バンドル用のパスを作るメソッド
	private string GenerateBundlePath(string i)
	{
		//文頭にスラッシュをつけて小文字にする
		return "/" + i.ToLower();
	}
	//改行コードを削るメソッド
	public string LineFeedCodeClear(string i)
	{
		return i.Replace("\r", "").Replace("\n", "");
	}

	//すでに読み込まれているAssetBundleデータのListを更新する関数
	public void LoadedDataListUpdate()
	{
		//List初期化
		UserDataClass.Instance.LoadedDataList = new List<string>();

		//読み込まれているデータ名を調べる
		foreach (AssetBundle i in AssetBundle.GetAllLoadedAssetBundles())
		{
			//Listに格納
			UserDataClass.Instance.LoadedDataList.Add(i.name);
		}
	}

	//現在のシーンで使用するステージやキャラクターのオブジェクトを生成して連想配列に格納するコルーチン
	public IEnumerator CreateSceneObject(Action Act)
	{
		//読み込み待ちをするためのbool宣言
		bool DoneFlag;

		//現在のシーンで使用するStageのプレハブList初期化
		UserDataClass.Instance.SceneStageList = new List<GameObject>();

		//現在のシーンで使用するCharacterのプレハブDictionary初期化
		UserDataClass.Instance.SceneCharacterRootObject = new Dictionary<int, GameObject>();

		//現在のシーンで使用するBodyのプレハブDictionary初期化
		UserDataClass.Instance.SceneBodyObject = new Dictionary<int, GameObject>();

		//現在のシーンで使用するHairのプレハブDictionary初期化
		UserDataClass.Instance.SceneHairObject = new Dictionary<int, GameObject>();

		//現在のシーンで使用するCostumeのプレハブDictionary初期化
		UserDataClass.Instance.SceneCostumeObject = new Dictionary<int, GameObject>();

		//現在のシーンで使用するWeaponのプレハブDictionary初期化
		UserDataClass.Instance.SceneWeaponObject = new Dictionary<int, GameObject>();

		//現在のシーンに登場するオブジェクトのMeshを全て格納するList初期化
		UserDataClass.Instance.SceneMeshObject = new List<Mesh>();

		//読み込むステージデータ名を回す
		foreach (string i in UserDataClass.Instance.SelectedMission.FileName)
		{
			//読み込み待ち用bool初期化
			DoneFlag = true;

			//ステージプレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.SelectedMission.StageDirPath, i, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneStageList.Add(OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;
			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}
		}

		//読み込むキャラクターで回す
		foreach (int i in UserDataClass.Instance.SceneCharacter)
		{
			//読み込み待ち用bool初期化
			DoneFlag = true;

			//キャラクタールートプレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.AllCharacterDic[i].RootDirPath, UserDataClass.Instance.AllCharacterDic[i].RootFileName, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneCharacterRootObject.Add(i, OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}

			//読み込み待ち用bool初期化
			DoneFlag = true;

			//体プレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.AllCharacterDic[i].BodyDirPath, UserDataClass.Instance.AllCharacterDic[i].BodyFileName, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneBodyObject.Add(i, OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}

			//読み込み待ち用bool初期化
			DoneFlag = true;

			//髪プレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.AllHairDic[i][UserDataClass.Instance.UserData.EquipHairList[i]].DirPath, UserDataClass.Instance.AllHairDic[i][UserDataClass.Instance.UserData.EquipHairList[i]].FileName, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneHairObject.Add(i, OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}

			//読み込み待ち用bool初期化
			DoneFlag = true;

			//コスチュームプレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.AllCostumeDic[i][UserDataClass.Instance.UserData.EquipCostumeList[i]].DirPath, UserDataClass.Instance.AllCostumeDic[i][UserDataClass.Instance.UserData.EquipCostumeList[i]].FileName, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneCostumeObject.Add(i, OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}

			//読み込み待ち用bool初期化
			DoneFlag = true;

			//武器プレハブ読み込み
			StartCoroutine(CreateInstance(UserDataClass.Instance.AllWeaponDic[i][UserDataClass.Instance.UserData.EquipWeaponList[i]].DirPath, UserDataClass.Instance.AllWeaponDic[i][UserDataClass.Instance.UserData.EquipWeaponList[i]].FileName, (GameObject OBJ, bool done) =>
			{
				//読み込んだオブジェクトをListにAdd
				UserDataClass.Instance.SceneWeaponObject.Add(i, OBJ);

				//読み込み完了フラグを立てる
				DoneFlag = done;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;
			}

			//読み込み待ち用bool初期化
			DoneFlag = true;

			//メッシュデータ読み込み
			StartCoroutine(AllFileLoadCoroutine("Model/Character/" + i + "/Mesh/", ".mesh", (List<object> objs) =>
			{
				//読み込んだオブジェクトをListにAdd
				foreach(object ii in objs)
				{
					UserDataClass.Instance.SceneMeshObject.Add(ii as Mesh);
				}

				//読み込み完了フラグを立てる
				DoneFlag = false;

			}));

			//処理が終わるまで待つ
			while (DoneFlag)
			{
				yield return null;

				yield return new WaitForSeconds(1);
			}

			//引数で受け取ったIDのキャラクターのパーツを組み合わせてキャラクターを作る関数呼び出し
			CharacterSetting(i);		
		}

		//処理完了したら受け取った匿名関数を実行
		Act();
	}	

	//引数で受け取ったIDのキャラクターのパーツを組み合わせてキャラクターを作る関数
	public void CharacterSetting(int CID)
	{
		//体をキャラの子要素にする
		UserDataClass.Instance.SceneBodyObject[CID].transform.parent = UserDataClass.Instance.SceneCharacterRootObject[CID].transform;

		//体をアクティブにする
		UserDataClass.Instance.SceneBodyObject[CID].SetActive(true);

		//頭角度をとるオブジェクトと頭のボーンを紐付けるConstraintSource
		ConstraintSource HeadConstraint = new ConstraintSource();

		//コンストレイントの重みを設定
		HeadConstraint.weight = 1;

		//コンストレイントのsourceを設定
		HeadConstraint.sourceTransform = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "HeadBone").transform;

		//頭角度をとるオブジェクトと頭のボーンを紐付ける
		DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "HeadAngle").GetComponent<RotationConstraint>().SetSource(0, HeadConstraint);

		//足のボーンにコンストレイント追加
		DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "R_FootBone").AddComponent<PositionConstraint>().constraintActive = true;
		DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "L_FootBone").AddComponent<PositionConstraint>().constraintActive = true;

		//足首を繋げるConstraintSource
		ConstraintSource R_FootConstraint = new ConstraintSource();
		ConstraintSource L_FootConstraint = new ConstraintSource();

		//コンストレイントの重みを設定
		R_FootConstraint.weight = 1;
		L_FootConstraint.weight = 1;

		//コンストレイントのsourceを設定
		R_FootConstraint.sourceTransform = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "R_LowerLegBone_end").transform;
		L_FootConstraint.sourceTransform = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "L_LowerLegBone_end").transform;
		
		//足首を繋げる
		DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "R_FootBone").GetComponent<PositionConstraint>().AddSource(R_FootConstraint);
		DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "L_FootBone").GetComponent<PositionConstraint>().AddSource(L_FootConstraint);
		
		//髪をキャラの子要素にする
		UserDataClass.Instance.SceneHairObject[CID].transform.parent = UserDataClass.Instance.SceneCharacterRootObject[CID].transform;

		//髪をアクティブにする
		UserDataClass.Instance.SceneHairObject[CID].SetActive(true);

		//髪のボーンと頭のボーンを紐付けるConstraintSource
		ConstraintSource HairConstraint = new ConstraintSource();

		//コンストレイントの重みを設定
		HairConstraint.weight = 1;

		//コンストレイントのsourceを設定
		HairConstraint.sourceTransform = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], UserDataClass.Instance.AllHairDic[CID][UserDataClass.Instance.UserData.EquipHairList[CID]].Attach).transform;
		
		//髪のボーンと頭のボーンを紐付ける
		UserDataClass.Instance.SceneHairObject[CID].GetComponentInChildren<ParentConstraint>().SetSource(0, HairConstraint);

		//衣装をキャラの子要素にする
		UserDataClass.Instance.SceneCostumeObject[CID].transform.parent = UserDataClass.Instance.SceneCharacterRootObject[CID].transform;

		//衣装をアクティブにする
		UserDataClass.Instance.SceneCostumeObject[CID].SetActive(true);

		//衣装用スキニングメッシュレンダラー宣言
		SkinnedMeshRenderer CostumeRenderer = new SkinnedMeshRenderer();

		//キャラの子要素からCostumeのSkinnedMeshRendererを取得する
		foreach (SkinnedMeshRenderer i in UserDataClass.Instance.SceneCharacterRootObject[CID].GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			if (i.transform.name.Contains("Costume"))
			{
				CostumeRenderer = i;
			}
		}

		//衣装プレハブ内のスキニングメッシュレンダラーを全て取得
		foreach (SkinnedMeshRenderer i in UserDataClass.Instance.SceneCostumeObject[CID].GetComponentsInChildren<SkinnedMeshRenderer>())
		{			
			//ボーン構成をコピーしてキャラクターのボーンと紐付ける
			i.bones = CostumeRenderer.bones;
		}

		//衣装のクロスに使うSphereColliderを全て取得
		foreach (SphereCollider i in UserDataClass.Instance.SceneCostumeObject[CID].GetComponentsInChildren<SphereCollider>())
		{
			//名前で判別してキャラクターのボーンの子にする
			if (i.name.Contains("L_") && i.name.Contains("Hip"))
			{
				i.transform.parent = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "L_HipBone").transform;
			}
			else if (i.name.Contains("R_") && i.name.Contains("Hip"))
			{
				i.transform.parent = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "R_HipBone").transform;
			}
			else if (i.name.Contains("L_") && i.name.Contains("Knee"))
			{
				i.transform.parent = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "L_KneeBone").transform;
			}
			else if (i.name.Contains("R_") && i.name.Contains("Knee"))
			{
				i.transform.parent = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "R_KneeBone").transform;
			}
			else if(i.name.Contains("Spine") && i.name.Contains("2"))
			{
				i.transform.parent = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], "SpineBone.002").transform;
			}

			//相対位置と回転をゼロにする
			i.transform.localPosition = new Vector3(0, 0, 0);
			i.transform.localRotation = Quaternion.Euler(0, 0, 0);
		}	

		//武器を持っていないキャラクターの場合は処理をしない
		if (UserDataClass.Instance.SceneWeaponObject.ContainsKey(CID))
		{
			//武器をキャラの子要素にする
			UserDataClass.Instance.SceneWeaponObject[CID].transform.parent = UserDataClass.Instance.SceneCharacterRootObject[CID].transform;

			//武器をアクティブにする
			UserDataClass.Instance.SceneWeaponObject[CID].SetActive(true);
			
			//武器のボーンとキャラのボーンを紐付けるConstraintSource
			ConstraintSource WeaponConstraint = new ConstraintSource();

			//コンストレイントの重みを設定
			WeaponConstraint.weight = 1;

			//コンストレイントのsourceを設定
			WeaponConstraint.sourceTransform = DeepFind(UserDataClass.Instance.SceneCharacterRootObject[CID], UserDataClass.Instance.AllWeaponDic[CID][UserDataClass.Instance.UserData.EquipWeaponList[CID]].Attach).transform;

			//武器のボーンとキャラのボーンを紐付ける
			UserDataClass.Instance.SceneWeaponObject[CID].GetComponent<ParentConstraint>().SetSource(0, WeaponConstraint);
		}
	}

	//引数で受け取ったオブジェクトのメッシュとマテリアルを更新する関数
	public GameObject MeshSetting(GameObject OBJ)
	{
		//全てのRendererを回す
		foreach (SkinnedMeshRenderer i in OBJ.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			//全てのメッシュデータを回す
			foreach (Mesh ii in UserDataClass.Instance.SceneMeshObject)
			{
				//名前を比較して同じメッシュを入れる
				if (ii.name == i.sharedMesh.name)
				{
					i.sharedMesh = ii;
				}
			}

			//全てのCharacterShaderMaterialをUserDataClassが持っているsharedMaterialにする
			if (i.sharedMaterial.name == "CharacterShaderMaterial")
			{
				i.sharedMaterial = UserDataClass.Instance.CharacterMaterial;
			}
		}

		//処理をしたオブジェクトを返す
		return OBJ;
	}

	//AutoSaveを実行するコルーチン
	public IEnumerator StartAutoSave(Action Act)
	{
		//IOManagerClass生成
		IOManagerClass SaveManager = new IOManagerClass();

		//セーブ処理待ち用bool宣言
		bool SaveDone = false;

		//バックアップ処理待ち用bool宣言
		bool BackUpDone = false;

		//整理処理待ち用bool宣言
		bool OrganizeDone = false;

		//バックアップファイルの名前重複を防ぐために1秒待つ
		yield return new WaitForSeconds(1);

		//セーブデータをバックアップする
		SaveManager.SaveBackUp(UserDataClass.Instance.DPath + "/SaveData/Save", UserDataClass.Instance.DPath + "/SaveData/", () =>
		{
			//処理が終わったらフラグ切り替え
			BackUpDone = true;
		});

		//バックアップが終わるまで待つ
		while (BackUpDone == false)
		{
			yield return null;
		}

		//セーブデータを整理する
		SaveManager.SaveOrganize(UserDataClass.Instance.DPath + "/SaveData/", () =>
		{
			//処理が終わったらフラグ切り替え
			OrganizeDone = true;
		});

		//整理が終わるまで待つ
		while (OrganizeDone == false)
		{
			yield return null;
		}

		//セーブ実行
		SaveManager.ExecuteSave(UserDataClass.Instance.UserData , UserDataClass.Instance.DPath + "/SaveData/Save" , () =>
		{
			//処理が終わったらフラグ切り替え
			SaveDone = true;
		});

		//セーブが終わるまで待つ
		while (SaveDone == false)
		{
			yield return null;
		}

		//処理が終わったら受け取った匿名関数を実行
		Act();
	}


	*/
}

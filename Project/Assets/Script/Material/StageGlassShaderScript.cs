using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageGlassShaderScript : GlobalClass
{
	//ステージ用ガラスマテリアルセッティング、ルートオブジェクトに付ける
    void Start()
    {
		List<Material> MatList = new List<Material>(gameObject.GetComponentsInChildren<Renderer>().Select(a => a.material));

		foreach(var i in MatList)
		{
			i.SetTextureScale("_TexMain", new Vector2(Screen.width / i.GetTexture("_TexMain").width, Screen.height / i.GetTexture("_TexMain").height) * GameManagerScript.Instance.ScreenResolutionScale);
		}
    }
}

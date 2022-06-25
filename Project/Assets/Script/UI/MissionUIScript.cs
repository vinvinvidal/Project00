using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionUIScript : GlobalClass
{
	//装備中の技
	private GameObject ArtsMatrixOBJ;

    void Start()
    {
		ArtsMatrixOBJ = DeepFind(gameObject, "ArtsMatrix");
		GameObject TempMatrix = Instantiate(ArtsMatrixOBJ);

		TempMatrix.transform.parent = gameObject.transform;

		TempMatrix.GetComponent<RectTransform>().position = ArtsMatrixOBJ.GetComponent<RectTransform>().position + new Vector3(0,-100,0);
		TempMatrix.GetComponent<RectTransform>().localScale = ArtsMatrixOBJ.GetComponent<RectTransform>().localScale;

		//ArtsMatrixOBJ.SetActive(false);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}

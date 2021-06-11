using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderSystem : MonoBehaviour
{

    public GameObject spritePrefab;

    int nextObjectCounter;
    public void ResetObjects()
    {
        nextObjectCounter = 0;
    }

    public GameObject NextObject()
    {
        GameObject go = null;
        if (nextObjectCounter < gameObject.transform.childCount)
        {
            go = gameObject.transform.GetChild(nextObjectCounter).gameObject;
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(spritePrefab);
            go.transform.SetParent(gameObject.transform);
        }
        nextObjectCounter++;
        return go;
    }

    public void CleanObjects()
    {
        GameObject go;
        int childCount = gameObject.transform.childCount;
        for (int i = nextObjectCounter; i < childCount; i++)
        {
            go = gameObject.transform.GetChild(i).gameObject;
            go.SetActive(false);
        }
    }

}

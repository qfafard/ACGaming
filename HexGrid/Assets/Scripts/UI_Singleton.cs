using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Singleton : MonoBehaviour
{
    public static UI_Singleton instance;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    
    
    public void SpawnPopup(Vector2Int pos, float time)
    {
    }

    
}

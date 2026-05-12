using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TuanZiVisual : MonoBehaviour
{
    [SerializeField]private GameObject tuanZiPrefab;

    private void Awake()
    {
     

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayerCharacter(GameObject characterGameObject)
    {
        //记录老团子的位置 然后生成新团子
        Vector3 pos = this.transform.position;
        Quaternion rot = this.transform.rotation;
        Transform parent = this.transform;
        if (tuanZiPrefab != null)
        {
            Destroy(tuanZiPrefab);
            tuanZiPrefab = null;
        }
        
        GameObject tuanZiInstance = Instantiate(characterGameObject, pos, rot, parent);
        tuanZiPrefab = tuanZiInstance;
    }
}

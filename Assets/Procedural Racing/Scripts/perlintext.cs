using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perlintext : MonoBehaviour
{

    public LineRenderer _lineRenderer; 
    private float _a=0.06f;//º‰œ∂£¨∆Ωª¨∆§
    public bool UsePerLin;
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        Vector3[] posArr = new Vector3[100];
        for (int i = 0; i < posArr.Length; i++) {
            if (UsePerLin)
            {
                posArr[i] = new Vector3(i * 0.1f, Mathf.PerlinNoise(i * _a, i * _a), 0);
            }
            else
            {
                posArr[i] = new Vector3(i * 8.1f, Random.value, 0);
            }
        
        }
        _lineRenderer.SetPositions(posArr);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

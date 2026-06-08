using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnTurnPage : MonoBehaviour
{
    public Image btnimg;
    void Start()
    {
        btnimg.alphaHitTestMinimumThreshold = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

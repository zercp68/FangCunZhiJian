using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{


    //所有卡牌数据
    [SerializeField] private List<CardData> allCardDatas;
     public List<CardData> deckData;
    // Start is called before the first frame update
    void Start()
    {
        //显示提示面板
        UIManager.Instance.ShowPanel<StartPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

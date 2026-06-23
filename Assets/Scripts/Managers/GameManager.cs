using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerDataManager.Instance.setup();
        CardManager.Instance.setup();
        StoreDataManager.Instance.setup();
    }


}

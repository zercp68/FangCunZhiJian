using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mana;
    public void UpdateManaTex(int currentMana)
    {
        Debug.Log("¿ªÊ¼¸Ä");
        mana.text = currentMana.ToString();
    }
}

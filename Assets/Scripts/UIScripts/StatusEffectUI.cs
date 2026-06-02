using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image Image;
    [SerializeField] private TMP_Text stackCountText;
    public void Set(Sprite sprite, int stackCount)
    {
        this.Image.sprite = sprite;
        stackCountText.text = stackCount.ToString();
    }
}

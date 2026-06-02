using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// EnemyView和HeroView显示UI脚本基础
/// </summary>
public class CombatantView : MonoBehaviour
{
    [SerializeField]private TMP_Text healthText;
    [SerializeField] private Image Image;
    public int MaxHealth {  get; private set; }
    public int CurrentHealth {  get; private set; }
    private void Awake()
    {
        if(Image == null)
        {
            Image = GetComponent<Image>();
        }
    }

    protected void SetupBase(int health,Sprite sprite)
    {
        MaxHealth=CurrentHealth=health;
        if (sprite != null) 
        {
            Image.sprite = sprite;
        }
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        healthText.text = "HP:" + CurrentHealth;
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }

        //受伤挨打放动画放抖动动画
        

        UpdateHealthText();
    }

}

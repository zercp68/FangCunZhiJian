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
    [SerializeField] private BloodUI bloodUI;
    [SerializeField] private Image imgView;
    public int MAX_HP {  get; private set; }
    public int currentHp {  get; private set; }
    private void Awake()
    {
        if(imgView == null)
        {
            imgView = GetComponent<Image>();
        }
    }

    protected void SetupBase(int health,Sprite sprite)
    {
        MAX_HP=currentHp=health;
        if (sprite != null) 
        {
            imgView.sprite = sprite;
        }
        UpdateHealth(currentHp,MAX_HP);
    }

    private void UpdateHealth(int currentHP,int MAX_HP)
    {
        bloodUI.UpdateBlood(currentHp,MAX_HP);
    }

    public virtual void Damage(int damageAmount)
    {
        currentHp -= damageAmount;
        if (currentHp < 0)
        {
            currentHp = 0;
        }

        UpdateHealth(currentHp, MAX_HP);
    }
    public virtual void Heal(int amount)
    {
        currentHp = Mathf.Min(MAX_HP, currentHp + amount);
        UpdateHealth(currentHp, MAX_HP);
    }
}

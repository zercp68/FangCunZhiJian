using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="NewHeroData",menuName ="Data/Hero")]
public class HeroData : ScriptableObject
{
    [field:SerializeField]public Sprite sprite {  get; private set; }
    [field:SerializeField]public int Health {  get; private set; }

}

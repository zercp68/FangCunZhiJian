using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//只会保存要执行的效果
public class PerformEffectGA : GameAction
{
    public Effect Effect {  get; private set; }
    public PerformEffectGA(Effect effect)
    {
        Effect = effect;
    }
}

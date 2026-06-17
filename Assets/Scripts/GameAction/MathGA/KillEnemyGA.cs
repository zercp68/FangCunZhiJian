using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//只需要一个目标，可以直接杀敌,无视生命值
public class KillEnemyGA : GameAction
{
    public EnemyView EnemyView {  get; private set; }
    public KillEnemyGA(EnemyView enemyView)
    {
        EnemyView = enemyView;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private EnemyView enemyPrefab;
    public EnemyView CreateEnemyView(EnemyData enemyData,Vector3 postion,Quaternion rotation)
    {
        EnemyView enemyView = Instantiate(enemyPrefab, postion, rotation);
        enemyView.Setup(enemyData);
        return enemyView;
    }
}

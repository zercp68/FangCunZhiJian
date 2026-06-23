using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Level
{
    public int LevelID;
    public Image imgBK;
    public List<EnemyData> enemyDatas;
    public HeroData heroData;
}

public class LevelManager : Singleton<LevelManager>
{
    [Header("=== 所有关卡列表 ===")]
    public List<Level> levels = new List<Level>();

    private int currentLevel = 0;

    public Level GetCurrentLevel()
    {
        if (currentLevel >= 0 && currentLevel < levels.Count)
            return levels[currentLevel];
        Debug.LogError($"关卡索引 {currentLevel} 无效，共有 {levels.Count} 关");
        return null;
    }
    public void NextLevel()
    {
        if (currentLevel < levels.Count - 1)
        {
            currentLevel++;
        }
    }

}

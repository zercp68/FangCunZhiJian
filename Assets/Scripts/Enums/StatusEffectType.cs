using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    // 土系
    ARMOR,          // 护甲：抵消伤害，每层 +2 护甲值
    THORNS,         // 荆棘：反弹伤害，每层反弹 1 点

    // 火系
    BURN,           // 灼烧：每回合造成真实伤害，每层 +2 伤害
    EXPLOSION,      // 爆炸：死亡时造成范围伤害，层数越高伤害越高

    // 金系
    CRIT_RATE,      // 暴击率：每层 +5% 暴击率
    CRIT_DAMAGE,    // 暴击伤害：每层 +10% 暴击伤害

    // 木系
    REGENERATION,   // 再生：每回合回复生命，每层 +2 生命
    LIFESTEAL,      // 吸血：造成伤害时回复生命，每层 +5% 吸血比例

    // 水系
    ALL_STAT_BOOST, // 全属性提升：攻击、防御、暴击、命中，每层 +3%
    FREEZE,         // 冰冻：敌人跳过一回合，层数决定持续时间

    // 通用
    ATTACK_BOOST,   // 攻击力提升：每层 +5% 伤害
    DEFENSE_BOOST,  // 防御力提升：每层 +2 防御
    HIT_RATE,       // 命中率提升：每层 +5% 命中
}
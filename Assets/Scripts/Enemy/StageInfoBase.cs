using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfoBase
{
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int act;
    [JsonProperty]
    public readonly int stage;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly DifficultyType difficulty;
    [JsonProperty]
    public readonly int monsterLevel;
    [JsonProperty]
    public readonly int baseExperience;
    [JsonProperty]
    public readonly List<WeightedDropItem> equipmentDropList;
    [JsonProperty]
    public readonly List<WeightedDropItem> archetypeDropList;
    [JsonProperty]
    public readonly List<string> stageProperties;
    [JsonProperty]
    public readonly float expMultiplier;
    [JsonProperty]
    public readonly int equipmentDropCountMin;
    [JsonProperty]
    public readonly int equipmentDropCountMax;
    [JsonProperty]
    public readonly int consumableDropCountMin;
    [JsonProperty]
    public readonly int consumableDropCountMax;
    [JsonProperty]
    public readonly List<EnemyWaveItem> enemyWaves;
    [JsonProperty]
    public readonly int sceneAct;
    [JsonProperty]
    public readonly int sceneStage;
    [JsonProperty]
    public readonly string requiredToUnlock;

}

public class EnemyWaveItem
{
    [JsonProperty]
    public readonly string enemyName;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly RarityType enemyRarity;
    [JsonProperty]
    public readonly List<string> bonusProperties;
    [JsonProperty]
    public readonly BattlePosition position;

}

public class WeightedDropItem
{
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int weight;
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventChoiceData
{
    [Header("Choice")]
    [SerializeField] private string choiceText;

    [Header("Result")]
    [TextArea(2, 5)]
    [SerializeField] private string resultDescription;

    [Header("Conditions")]
    [SerializeField] private List<EventConditionData> conditions = new List<EventConditionData>();

    [Header("Effects")]
    [SerializeField] private List<EventEffectData> effects = new List<EventEffectData>();

    public string ChoiceText => choiceText;
    public string ResultDescription => resultDescription;
    public IReadOnlyList<EventConditionData> Conditions => conditions;
    public IReadOnlyList<EventEffectData> Effects => effects;
}

[Serializable]
public class EventConditionData
{
    public EventConditionType conditionType;

    [Min(0)]
    public int requiredValue;
}

[Serializable]
public class EventEffectData
{
    public EventEffectType effectType;

    [Min(0)]
    public int amount;
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Event")]
public class EventData : ScriptableObject
{
    [Header("Event Information")]
    [SerializeField] private string eventId;
    [SerializeField] private string eventName;

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite background;

    [Header("Choices")]
    [SerializeField] private List<EventChoiceData> choices = new List<EventChoiceData>();

    public string EventId => eventId;
    public string EventName => eventName;
    public string Description => description;
    public Sprite Background => background;
    public IReadOnlyList<EventChoiceData> Choices => choices;
}
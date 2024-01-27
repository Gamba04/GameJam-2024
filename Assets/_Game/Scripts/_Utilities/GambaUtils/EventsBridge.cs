using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsBridge : MonoBehaviour
{
    [SerializeField]
    private List<UnityEvent> events = new List<UnityEvent>();

    public void CallEvent(int index)
    {
        if (index < events.Count) events[index]?.Invoke();
        else Debug.LogError("Event index out of bounds");
    }
}
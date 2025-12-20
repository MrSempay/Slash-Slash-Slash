using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioCreator : MonoBehaviour
{
    public List<BaseEvent> events = new List<BaseEvent>();

    private void Start()
    {
        //Debug.Log(events.Count);
        //Debug.Log(events[0].ClassName);
    }
}

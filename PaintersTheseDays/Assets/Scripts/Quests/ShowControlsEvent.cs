using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControlsEvent : QuestStartEvent
{
    public override void Activate()
    {
        FindObjectOfType<UIManager>().ShowControls();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUnlockEvent : QuestEvent
{
    public override void Activate()
    {
        GameObject.Find("StartExit").gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocksQuestEvent : QuestEvent
{
    public override void Activate()
    {
        GameObject.Find("ByeRocks").gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpotQuestTest : MonoBehaviour
{
    void Start()
    {
        SpotQuest quest = new SpotQuest();
        quest.paints = new Paint[] { new Paint(Color.blue), new Paint(Color.green), new Paint(Color.red) };
        quest.hasIncompletePainting = false;
        quest.position = new Vector3(10f, 1f, 20f);
        quest.yRotation = -120f;
        Object q = Resources.Load<Object>("Prefab/QuestPoint");
        QuestPoint qp = Instantiate(q, quest.position, Quaternion.Euler(0f, quest.yRotation, 0f)).GetComponent<QuestPoint>();
        qp.SetQuest(quest);
    }
}

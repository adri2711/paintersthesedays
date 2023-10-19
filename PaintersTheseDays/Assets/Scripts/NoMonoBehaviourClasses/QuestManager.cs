using Managers;
using NoMonoBehaviourClasses;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance => _instance;
    private const string QUEST_JSON_FILE = "/Quests/";
    public SpotQuest activeQuest;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateQuest(string name)
    {
        if (activeQuest != null) return;
        SpotQuest quest = LoadQuestFromJSON(name);
        SpawnSpotQuest(quest);
        activeQuest = quest;
    }

    public void FinishQuest()
    {
        activeQuest = null;
    }

    private void SpawnSpotQuest(SpotQuest quest)
    {
        if (quest == null) return;
        Object q = Resources.Load<Object>("Prefab/QuestPoint");
        QuestPoint qp = Instantiate(q, quest.position, Quaternion.Euler(0f, quest.yRotation, 0f)).GetComponent<QuestPoint>();
        qp.SetQuest(quest);
    }

    private SpotQuest LoadQuestFromJSON(string questName)
    {
        string jsonPath = Application.streamingAssetsPath + QUEST_JSON_FILE + questName + ".json";

        if (!File.Exists(jsonPath))
        {
            return null;
        }

        string json = File.ReadAllText(jsonPath);

        return JsonUtility.FromJson<SpotQuest>(json);
    }
}

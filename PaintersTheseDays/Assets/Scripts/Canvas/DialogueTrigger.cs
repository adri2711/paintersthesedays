using System.IO;
using Managers;
using NoMonoBehaviourClasses;
using UnityEngine;
public class DialogueTrigger : MonoBehaviour
{
    private const int PLAYER_LAYER = 6;

    [SerializeField] private Dialogue _dialogue;

    [SerializeField] private string _speakerName;
    public bool active = false;
    public bool finished = false;
    private bool ranEvent = false;

    private DialogueManager _dialogueManager;

    private void Start()
    {
        _dialogueManager = DialogueManager.Instance;
        LoadDialogueFromJSON();

    }

    private void LoadDialogueFromJSON()
    {
        string jsonPath = Application.streamingAssetsPath + _dialogueManager.GetDialogueJSONPath() + _speakerName + ".json";

        if (!File.Exists(jsonPath))
        {
            return;
        }

        string json = File.ReadAllText(jsonPath);

        _dialogue = JsonUtility.FromJson<Dialogue>(json);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer != PLAYER_LAYER)
        {
            return;
        }

        if (finished) return;
        if (!active)
        {
            if (QuestManager.Instance.questPoint != null)
            {
                return;
            }
            else
            {
                active = true;
            }
        }

        _dialogueManager.GetCanvas().gameObject.SetActive(true);

        _dialogueManager.StartDialogue(this, _dialogue, _speakerName);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer != PLAYER_LAYER)
        {
            return;
        }
        if (active)
        {
            if (QuestManager.Instance.questPoint == null)
            {
                active = false;
            }
            else if (!ranEvent)
            {
                ranEvent = true;
                QuestStartEvent e = GetComponent<QuestStartEvent>();
                if (e != null) e.Activate();
            }
        }

        _dialogueManager.GetCanvas().gameObject.SetActive(false);

        _dialogueManager.EndDialogue();
    }
}

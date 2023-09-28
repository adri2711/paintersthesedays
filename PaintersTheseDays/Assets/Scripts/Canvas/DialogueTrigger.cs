using System.IO;
using Managers;
using NoMonoBehaviourClasses;
using UnityEngine;

namespace Canvas
{
    public class DialogueTrigger : MonoBehaviour
    {
        private const int PLAYER_LAYER = 6;
        
        [SerializeField] private Dialogue _dialogue;

        [SerializeField] private string _speakerName;

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
            
            _dialogueManager.GetCanvas().gameObject.SetActive(true);
            
            _dialogueManager.StartDialogue(_dialogue, _speakerName);
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != PLAYER_LAYER)
            {
                return;
            }

            _dialogueManager.GetCanvas().gameObject.SetActive(false);

            _dialogueManager.EndDialogue();
        }
    }
}

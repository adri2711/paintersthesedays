using System;
using System.Collections.Generic;
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

        [SerializeField] private string _speecherName;

        private DialogueManager _dialogueManager;

        private void Start()
        {
            _dialogueManager = DialogueManager.Instance;
            LoadDialogueFromJSON();

        }

        private void LoadDialogueFromJSON() 
        {
            string jsonPath = Application.streamingAssetsPath + _dialogueManager.GetDialogueJSONPath();

            if (!File.Exists(jsonPath))
            {
                return;
            }

            string json = File.ReadAllText(jsonPath);

            Dialogues dialogueJson = JsonUtility.FromJson<Dialogues>(json);

            foreach (Dialogue dialogue in dialogueJson.dialogues)
            {
                if (dialogue.speecherName == _speecherName)
                {
                    _dialogue = dialogue;    
                }
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != PLAYER_LAYER)
            {
                return;
            }
            
            _dialogueManager.GetCanvas().gameObject.SetActive(true);
            
            _dialogueManager.StartDialogue(_dialogue);
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

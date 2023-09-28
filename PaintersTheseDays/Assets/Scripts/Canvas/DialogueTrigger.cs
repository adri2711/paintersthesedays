using System;
using Managers;
using UnityEngine;

namespace Canvas
{
    public class DialogueTrigger : MonoBehaviour
    {
        private const int PLAYER_LAYER = 6;
        
        [SerializeField] private Dialogue _dialogue;

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != PLAYER_LAYER)
            {
                return;
            }

            DialogueManager dialogueManager = DialogueManager.Instance;
            
            dialogueManager.GetCanvas().gameObject.SetActive(true);
            
            dialogueManager.StartDialogue(_dialogue);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace NoMonoBehaviourClasses
{
    [System.Serializable]
    public class Dialogue
    {
        public string speecherName;
    
        public DialogueSentences[] dialogueSentences;

        public DialogueOptions[] dialogueOptions;

        public string chosenOptions;
    }
}
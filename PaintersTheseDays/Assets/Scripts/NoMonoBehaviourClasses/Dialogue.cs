using System.Collections.Generic;
using UnityEngine;

namespace NoMonoBehaviourClasses
{
    [System.Serializable]
    public class Dialogue
    {
        public DialogueContent[] dialogueContent;
        public string waitingSentence;
        public string successfulSentence;
        public string failedSentence;

        public string chosenOptions;
    }
}
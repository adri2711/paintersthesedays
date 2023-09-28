namespace NoMonoBehaviourClasses 
{
    [System.Serializable]
    public class DialogueContent
    {
        public string chosenOptionsSequenceNeeded;
        public string[] sentences;
        public DialoguePosOption[] sentencesPosOptions;

        public DialogueOption[] dialogueOptions;
    }
}
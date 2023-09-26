using UnityEngine;

namespace Canvas
{
    [System.Serializable]
    public class Dialogue
    {
        public string speecherName;
    
        [TextArea(2, 10)]
        public string[] sentences;

    }
}

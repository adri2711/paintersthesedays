using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using NoMonoBehaviourClasses;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;

        private const string DIALOGUE_JSON_FILE = "/Dialogues.json";

        [SerializeField] private UnityEngine.Canvas _canvas;
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        
        private Queue<string> _sentences;

        private bool _currentSentenceFinished = true;
        private bool _finish;

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
        
        public static DialogueManager Instance => _instance;

        public static string DIALOGUE_JSON_FILE1 => DIALOGUE_JSON_FILE;

        void Start()
        {
            _sentences = new Queue<string>();
        }

        private void Update()
        {
            if (_canvas.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (_currentSentenceFinished)
                    {
                        DisplayNextSentence();
                        return;
                    }

                    _finish = true;
                }
            }
        }

        public void StartDialogue(Dialogue dialogue)
        {
            _nameText.text = dialogue.speecherName;
            
            _sentences.Clear();

            foreach (string sentence in dialogue.dialogueSentences[0].sentences)
            {
                _sentences.Enqueue(sentence);
            }

            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (_sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            string sentence = _sentences.Dequeue();

            StartCoroutine(TypeSentence(sentence, 0.03f));
        }

        public void EndDialogue()
        {
            _canvas.gameObject.SetActive(false);
        }

        public UnityEngine.Canvas GetCanvas()
        {
            return _canvas;
        }

        private IEnumerator TypeSentence(String sentence, float delayBetweenLetters)
        {
            _dialogueText.text = "";

            _currentSentenceFinished = false;
            
            foreach (char character in sentence)
            {
                if (_finish)
                {
                    _dialogueText.text = sentence;
                    break;
                }
                _dialogueText.text += character;
                yield return new WaitForSeconds(delayBetweenLetters);
            }

            _currentSentenceFinished = true;
            _finish = false;
        }

        public string GetDialogueJSONPath() 
        {
            return DIALOGUE_JSON_FILE;
        }
    }
}

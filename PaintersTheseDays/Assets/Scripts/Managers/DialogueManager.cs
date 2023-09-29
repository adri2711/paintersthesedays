using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NoMonoBehaviourClasses;
using TMPro;
using UnityEngine;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Input = UnityEngine.Input;

namespace Managers
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;

        private const string DIALOGUE_JSON_FILE = "/Dialogues/";

        [SerializeField] private UnityEngine.Canvas _canvas;
        
        [SerializeField] private TextMeshProUGUI[] _dialogueOptions;
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        
        private Queue<string> _sentences;
        
        private DialogueOption[] _options;

        private Dialogue _dialogue;

        private DialogueContent _dialogueContent;

        private string _speakerName;

        private int _optionSelected;

        private bool _currentSentenceFinished = true;
        private bool _finishSentence;
        private bool _choosingOption;

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

            string directoryPath = Application.streamingAssetsPath + "/Dialogues/";

            if (!Directory.Exists(directoryPath))
            {
                return;
            }
            
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".json")
                {
                    File.Copy(directoryPath +"Backups/Backup " + Path.GetFileName(file), file, true);
                }
            }
        }
        
        public static DialogueManager Instance => _instance;

        void Start()
        {
            _sentences = new Queue<string>();
        }

        private void Update()
        {
            if (!_canvas.gameObject.activeSelf)
            {
                return;
            }

            if (_choosingOption)
            {
                ChooseAnOption();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                if (_currentSentenceFinished)
                {
                    DisplayNextSentence();
                    return;
                }

                _finishSentence = true;
            }
        }

        private void ChooseAnOption()
        {
            IterateBetweenOptions();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                _dialogue.chosenOptions += _options[_optionSelected].optionNumber;
                _choosingOption = false;

                foreach (TextMeshProUGUI text in _dialogueOptions)
                {
                    text.gameObject.SetActive(false);
                }

                foreach (DialoguePosOption posOption in _dialogueContent.sentencesPosOptions)
                {
                    if (_dialogue.chosenOptions[^1].ToString() == posOption.selectedOptionNumber)
                    {
                        LoadSentences(posOption.sentences);
                        break;
                    }
                }
            }
        }

        private void IterateBetweenOptions()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                _optionSelected++;
                
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                _optionSelected--;
            }

            _optionSelected = Convert.ToInt32(Mathf.Repeat(_optionSelected, _options.Length));
        }

        public void StartDialogue(Dialogue dialogue, string speakerName)
        {
            _dialogue = dialogue;

            _speakerName = speakerName;
            
            _nameText.text = _speakerName;

            LoadDialogueContent();

            _sentences.Clear();

            LoadSentences(_dialogueContent.sentences);

            DisplayNextSentence();
        }

        private void LoadDialogueContent()
        {
            foreach (DialogueContent content in _dialogue.dialogueContent)
            {
                if (_dialogue.chosenOptions == content.chosenOptionsSequenceNeeded)
                {
                    _dialogueContent = content;
                    break;
                }
                return;
            }
        }

        private void LoadSentences(string[] sentences)
        {
            foreach (string sentence in sentences)
            {
                _sentences.Enqueue(sentence);
            }
        }

        private void DisplayNextSentence()
        {
            if (_sentences.Count == 0)
            {
                _options = new DialogueOption[_dialogueContent.dialogueOptions.Length];

                for (int i = 0; i < _dialogueContent.dialogueOptions.Length; i++)
                {
                    _options[i] = _dialogueContent.dialogueOptions[i];
                }

                if (_options.Length != 0)
                {
                    DisplayOptions();
                    return;
                }

                EndDialogue();
                return;
            }

            string sentence = _sentences.Dequeue();

            StartCoroutine(TypeSentence(sentence, 0.03f));
        }

        private void DisplayOptions()
        {
            _choosingOption = true;
            
            for (int i = 0; i < _options.Length; i++)
            {
                _dialogueOptions[i].text = _options[i].option;
                _dialogueOptions[i].gameObject.SetActive(true);           
            }

            _optionSelected = 0;
        }

        public void EndDialogue()
        {
            _canvas.gameObject.SetActive(false);
            for (int i = 0; i < _options.Length; i++)
            {
                _dialogueOptions[i].gameObject.SetActive(false);           
            }
            
            SaveDialogueToJSON();
        }

        private void SaveDialogueToJSON(bool async = false)
        {

            string json = JsonUtility.ToJson(_dialogue);
            string path = Application.streamingAssetsPath + DIALOGUE_JSON_FILE + _speakerName + ".json";

            if (async)
            {
                File.WriteAllTextAsync(path, json);
            }
            else
            {
                File.WriteAllText(path, json);
            }
        }

        private IEnumerator TypeSentence(String sentence, float delayBetweenLetters)
        {
            _dialogueText.text = "";

            _currentSentenceFinished = false;
            
            foreach (char character in sentence)
            {
                if (_finishSentence)
                {
                    _dialogueText.text = sentence;
                    break;
                }
                _dialogueText.text += character;
                yield return new WaitForSeconds(delayBetweenLetters);
            }

            _currentSentenceFinished = true;
            _finishSentence = false;
        }

        public UnityEngine.Canvas GetCanvas()
        {
            return _canvas;
        }

        public string GetDialogueJSONPath() 
        {
            return DIALOGUE_JSON_FILE;
        }
    }
}

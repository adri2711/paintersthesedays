using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NoMonoBehaviourClasses;
using TMPro;
using UnityEngine;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Image = UnityEngine.UI.Image;
using Input = UnityEngine.Input;

namespace Managers
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;

        private const string DIALOGUE_JSON_FILE = "/Dialogues/";

        [SerializeField] private UnityEngine.Canvas _canvas;
        
        [SerializeField] private TextMeshProUGUI[] _dialogueOptions;
        [SerializeField] private Image[] _arrows;
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        
        private Queue<string> _sentences;
        
        private Queue<DialogueOption> _options;

        private Dialogue _dialogue;

        private DialogueContent _dialogueContent;

        private DialogueTrigger _currentTrigger;

        private string _speakerName;

        private int _optionSelected;

        private bool _currentSentenceFinished = true;
        private bool _finishSentence;
        private bool _choosingOption;
        private bool _lastSentence;
        private bool _waitForQuest = false;
        private bool _questSentence = false;

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
            _options = new Queue<DialogueOption>();
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
                _dialogue.chosenOptions += _optionSelected;
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

                foreach (Image arrow in _arrows)
                {
                    arrow.gameObject.SetActive(false);
                }

                QuestManager.Instance.ActivateQuest(_dialogueContent.dialogueOptions[_optionSelected].questToGive);

                _optionSelected = 0;

                _lastSentence = false;
                
                _options.Clear();
                
                DisplayNextSentence();
            }
        }

        private void IterateBetweenOptions()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                _arrows[_optionSelected].gameObject.SetActive(false);
                _optionSelected++;
                
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                _arrows[_optionSelected].gameObject.SetActive(false);
                _optionSelected--;
            }

            _optionSelected = Convert.ToInt32(Mathf.Repeat(_optionSelected, _options.Count));
            
            _arrows[_optionSelected].gameObject.SetActive(true);
        }

        public void StartDialogue(DialogueTrigger trigger, Dialogue dialogue, string speakerName)
        {
            _currentTrigger = trigger;

            _dialogue = dialogue;

            _speakerName = speakerName;
            
            _nameText.text = _speakerName;

            LoadDialogueContent();

            _sentences.Clear();

            LoadSentences(_dialogueContent.sentences);

            _lastSentence = false;

            if (QuestManager.Instance.activeQuest != null)
            {
                _waitForQuest = true;
                _questSentence = true;
            }

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
            string sentence;
            if (_waitForQuest)
            {
                if (!_questSentence)
                {
                    EndDialogue();
                    return;
                }
                if (QuestManager.Instance.activeQuest.valid)
                {
                    sentence = _dialogue.successfulSentence;
                    _waitForQuest = false;
                    QuestEvent e = _currentTrigger.GetComponent<QuestEvent>();
                    if (e != null)
                    {
                        e.Activate();
                    }
                    QuestManager.Instance.FinishQuest();
                    _sentences.Clear();
                }
                else
                {
                    sentence = _dialogue.failedSentence;
                }
                _questSentence = false;
            }
            else
            {

                if (_sentences.Count == 0)
                {
                    EndDialogue();
                    return;
                }

                sentence = _sentences.Dequeue();

                if (_sentences.Count == 0)
                {
                    _lastSentence = true;
                }
            }

            StartCoroutine(TypeSentence(sentence, 0.03f));
        }

        private void DisplayOptions()
        {
            LoadOptions();

            if (_options.Count == 0)
            {
                return;
            }
            
            _choosingOption = true;

            int counter = 0;
            
            foreach (DialogueOption option in _options)
            {
                _dialogueOptions[counter].text = option.option;
                _dialogueOptions[counter].gameObject.SetActive(true);
                counter++;
            }

            _optionSelected = 0;
        }

        private void LoadOptions()
        {
            int possibleOptions = 0;

            foreach (DialogueOption option in _dialogueContent.dialogueOptions)
            {
                if (option.optionNumber.Length == _dialogue.chosenOptions.Length + 1)
                {
                    _options.Enqueue(option);
                }
            }
        }

        public void EndDialogue()
        {
            _canvas.gameObject.SetActive(false);
            for (int i = 0; i < _options.Count; i++)
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

            if (_lastSentence)
            {
                DisplayOptions();
            }
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

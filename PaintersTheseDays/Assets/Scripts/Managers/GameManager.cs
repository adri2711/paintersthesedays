using System;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        [SerializeField] private FirstPersonController _firstPersonController;

        [SerializeField] private GameObject _crosshairCanvas;

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

        public static GameManager Instance => _instance;


        private void Update()
        {
            _crosshairCanvas.SetActive(_firstPersonController.canMove);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    static public UIManager Instance;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _infoText;
   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }   
    }

    public void UpdateTimer(float tempoRestante)
    {
        int minutos = Mathf.FloorToInt(tempoRestante / 60);
        int segundos = Mathf.FloorToInt(tempoRestante % 60);
        _timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

}

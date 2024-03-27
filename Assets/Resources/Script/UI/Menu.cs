using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Menu : MonoBehaviour
{
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button ESCButton;

    private void Start()
    {
        ContinueButton.onClick.AddListener(ClickCloseButton);
        SaveButton.onClick.AddListener(ClickSaveButton);
        LoadButton.onClick.AddListener(ClickLoadButton);
        ESCButton.onClick.AddListener(ClickESCButton);
    }

    private void ClickCloseButton()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    private void ClickSaveButton()
    {

    }
    private void ClickLoadButton()
    {

    }
    private void ClickESCButton()
    {
        Application.Quit();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    private static PopupManager instance;

    public static PopupManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    public delegate void Callback();

    private Callback callbackOK;
    private Callback callbackYes;
    private Callback callbackNo;

    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject yesNo;

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI message;

    [SerializeField] private Button OK;
    [SerializeField] private Button yes;
    [SerializeField] private Button no;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
            Destroy(gameObject);

        OK.onClick.AddListener(() =>
        {
            callbackOK?.Invoke();
            callbackOK = null;
            popup.gameObject.SetActive(false);
        });
        yes.onClick.AddListener(() =>
        {
            callbackYes?.Invoke();
            callbackYes = null;
            popup.gameObject.SetActive(false);
        });
        no.onClick.AddListener(() =>
        {
            callbackNo?.Invoke();
            callbackNo = null;
            popup.gameObject.SetActive(false);
        });
    }
    
    public void Show(string title, string message)
    {
        popup.gameObject.SetActive(true);
        OK.gameObject.SetActive(true);
        yesNo.gameObject.SetActive(false);

        this.title.text = title;
        this.message.text = message;
        callbackOK = null;
    }

    public void Show(string title, string message, Callback callbackOK)
    {
        popup.gameObject.SetActive(true);
        OK.gameObject.SetActive(true);
        yesNo.gameObject.SetActive(false);

        this.title.text = title;
        this.message.text = message;
        this.callbackOK = callbackOK;
    }

    public void Show(string title, string message, Callback callbackYes, Callback callbackNo)
    {
        popup.gameObject.SetActive(true);
        OK.gameObject.SetActive(false);
        yesNo.gameObject.SetActive(true);

        this.title.text = title;
        this.message.text = message;
        this.callbackYes = callbackYes;
        this.callbackNo = callbackNo;
    }
}

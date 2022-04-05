using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Popup : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI content;
    public Button confirm;

    void Initiate()
    {
        title.text = String.Empty;
        content.text = String.Empty;
        confirm.onClick.RemoveAllListeners();
        Confirm();
    }

    public void Confirm()
    {
        confirm.gameObject.SetActive(true);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Close()
    {
        gameObject.SetActive(false);
        Initiate();
    }
    
}

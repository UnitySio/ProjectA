using System;
using UnityEngine;
using UnityEngine.UI;

public class popup_window : MonoBehaviour
{
    public Text text_message;
    public Button btn_retry;
    public Button btn_confirm;
    public Button btn_cancel;

    void init()
    {
        text_message.text = String.Empty;
        btn_retry.onClick.RemoveAllListeners();
        btn_confirm.onClick.RemoveAllListeners();
        btn_cancel.onClick.RemoveAllListeners();
        setConfirm();
    }

    public void setConfirm()
    {
        btn_confirm.gameObject.SetActive(true);
        btn_retry.gameObject.SetActive(false);
        btn_cancel.gameObject.SetActive(false);
    }

    public void setRetry()
    {
        btn_confirm.gameObject.SetActive(false);
        btn_retry.gameObject.SetActive(true);
        btn_cancel.gameObject.SetActive(true);
    }

    public void showwindow()
    {
        gameObject.SetActive(true);
    }
    
    public void closewindow()
    {
        gameObject.SetActive(false);
        init();
    }
    
}

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SetUserData : MonoBehaviour
{
    private Popup popup;

    private void Awake()
    {
        popup = ServerManager.Instance.Popup;
    }

    public async Task UpdateNickname()
    {
    }
}
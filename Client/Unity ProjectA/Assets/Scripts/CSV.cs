using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class CSV : MonoBehaviour
{
    public Popup popup { get; private set; }

    private void Awake()
    {
        popup = ServerManager.Instance.Popup;
    }

    private void Start()
    {
        StartCoroutine(LoadCSVFile());
    }

    private IEnumerator LoadCSVFile()
    {
        var loadHandle = Addressables.LoadAssetAsync<TextAsset>("TestCSV");
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log(loadHandle.Result.text);
        }
        else if (loadHandle.Status == AsyncOperationStatus.Failed)
        {
            popup.confirm.onClick.RemoveAllListeners();
            popup.title.text = $"에러";
            popup.content.text = $"데이터를 불러오는 도중에 문제가 발생했습니다.";
            popup.confirm.onClick.AddListener(async () =>
            {
                popup.Close();

                await Task.Delay(500);
                SceneManager.LoadScene("LoginScene");
            });
                            
            popup.Show();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;

public partial class LoginManager : MonoBehaviour
{
    [Header("LoginManager.Asset")]
    public List<string> assetKeys = new List<string>();
    public GameObject progressBarGroup;
    public TextMeshProUGUI progressContent;
    public Image progressBar;

    private void InitaiteAddressable()
    {
        Addressables.InitializeAsync().Completed += (op) => Addressables.Release(op);
    }

    private IEnumerator CheckAsset()
    {
        loginState = LoginState.AssetCheck;

        long totalSize = 0;

        for (int i = 0; i < assetKeys.Count; i++)
        {
            var assetKey = assetKeys[i];

            var sizeHandle = Addressables.GetDownloadSizeAsync(assetKey);
            yield return sizeHandle;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                totalSize += sizeHandle.Result;

                Addressables.Release(sizeHandle);
            }
        }

        if (totalSize > 0)
        {
            popup.title.text = $"데이터 다운로드";
            popup.content.text = $"추가 데이터 {FomulaBytes(totalSize)}를 발견했습니다.\nWIFI 환경에서 다운로드 진행을 권장합니다.";
            popup.confirm.onClick.AddListener(() =>
            {
                popup.Close();

                StartCoroutine(DownloadAsset());
            });

            popup.Show();
        }
        else
            ConfirmJWT();
    }

    private IEnumerator DownloadAsset()
    {
        loginState = LoginState.AssetDownload;

        for (int i = 0; i < assetKeys.Count; i++)
        {
            var assetKey = assetKeys[i];

            var sizeHandle = Addressables.GetDownloadSizeAsync(assetKey);
            yield return sizeHandle;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if (sizeHandle.Result > 0)
                {
                    if (i == 0)
                        progressBarGroup.SetActive(true);

                    var downloadHandle = Addressables.DownloadDependenciesAsync(assetKey, true);
                    downloadHandle.Completed += (opDownload) =>
                    {
                        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                            Addressables.Release(opDownload);
                    };

                    while (!downloadHandle.IsDone)
                    {
                        progressContent.text = $"추가 데이터 다운로드 중 {(int)(downloadHandle.GetDownloadStatus().Percent * 100)}% {FomulaBytes((long)Mathf.Lerp(0, sizeHandle.Result, downloadHandle.GetDownloadStatus().Percent))}/{FomulaBytes(sizeHandle.Result)} ({i + 1}/{assetKeys.Count})";
                        progressBar.fillAmount = downloadHandle.GetDownloadStatus().Percent;
                        yield return null;
                    }
                }

                Addressables.Release(sizeHandle);
            }
        }

        if (progressBarGroup.activeSelf)
            progressBarGroup.SetActive(false);

        ConfirmJWT();
    }

    private string FomulaBytes(long bytes)
    {
        int size = 1024;
        string[] units = new string[] { "GB", "MB", "KB", "B" };
        long max = (long)Mathf.Pow(size, units.Length - 1);

        foreach (var unit in units)
        {
            if (bytes > max)
                return string.Format("{0:##.##}{1}", decimal.Divide(bytes, max), unit);

            max /= size;
        }

        return "0B";
    }
}

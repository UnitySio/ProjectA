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
                    {
                        progressBarGroup.SetActive(true);
                        yield return new WaitForSeconds(1f);
                    }

                    var downloadHandle = Addressables.DownloadDependenciesAsync(assetKey, true);
                    downloadHandle.Completed += (opDownload) =>
                    {
                        progressContent.text = $"추가 데이터 다운로드 중 100% {FomulaBytes(sizeHandle.Result)}/{FomulaBytes(sizeHandle.Result)} ({assetKeys.Count}/{assetKeys.Count})";
                        progressBar.fillAmount = 1;
                    };

                    while (!downloadHandle.IsDone)
                    {
                        progressContent.text = $"추가 데이터 다운로드 중 {(int)(downloadHandle.PercentComplete * 100)}% {FomulaBytes((long)Mathf.Lerp(0, sizeHandle.Result, downloadHandle.PercentComplete))}/{FomulaBytes(sizeHandle.Result)} ({i + 1}/{assetKeys.Count})";
                        progressBar.fillAmount = downloadHandle.PercentComplete;
                        yield return null;
                    }
                }
            }
        }

        if (progressBarGroup.activeSelf)
        {
            yield return new WaitForSeconds(1f);
            progressBarGroup.SetActive(false);
        }

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

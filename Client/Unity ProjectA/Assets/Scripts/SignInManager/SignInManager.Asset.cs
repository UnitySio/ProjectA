using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;

public partial class SignInManager : MonoBehaviour
{
    [Header("SignInManager.Asset")]
    public List<string> assetKeys = new List<string>();
    public GameObject progressBarGroup;
    public TextMeshProUGUI progressContent;
    public Image progressBar;

    private IEnumerator CheckAsset()
    {
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
            else if (sizeHandle.Status == AsyncOperationStatus.Failed)
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"�߰� ������ ������ �뷮�� Ȯ���ϴ� ���� ������ �߻��߽��ϴ�.";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(500);
                    Application.Quit();
                });
            }
        }

        if (totalSize > 0)
        {
            popup.title.text = $"������ �ٿ�ε�";
            popup.content.text = $"�߰� ������ {FomulaBytes(totalSize)}�� �߰��߽��ϴ�.\nWIFI ȯ�濡�� �ٿ�ε� ������ �����մϴ�.";
            popup.confirm.onClick.AddListener(() =>
            {
                popup.Close();

                StartCoroutine(DownloadAsset());
            });

            popup.Show();
        }
        else
            CheckJWT();
    }

    private IEnumerator DownloadAsset()
    {
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
                        else if (downloadHandle.Status == AsyncOperationStatus.Failed)
                        {
                            popup.confirm.onClick.RemoveAllListeners();
                            popup.title.text = $"����";
                            popup.content.text = $"�߰� ������ ������ �ٿ�ε��ϴ� ���� ������ �߻��߽��ϴ�.";
                            popup.confirm.onClick.AddListener(async () =>
                            {
                                popup.Close();

                                await Task.Delay(500);
                                Application.Quit();
                            });
                        }
                    };

                    while (!downloadHandle.IsDone)
                    {
                        progressContent.text = $"�߰� ������ �ٿ�ε� �� {(int)(downloadHandle.GetDownloadStatus().Percent * 100)}% {FomulaBytes((long)Mathf.Lerp(0, sizeHandle.Result, downloadHandle.GetDownloadStatus().Percent))}/{FomulaBytes(sizeHandle.Result)} ({i + 1}/{assetKeys.Count})";
                        progressBar.fillAmount = downloadHandle.GetDownloadStatus().Percent;
                        yield return null;
                    }
                }

                Addressables.Release(sizeHandle);
            }
            else if (sizeHandle.Status == AsyncOperationStatus.Failed)
            {
                popup.confirm.onClick.RemoveAllListeners();
                popup.title.text = $"����";
                popup.content.text = $"�߰� ������ ������ �뷮�� Ȯ���ϴ� ���� ������ �߻��߽��ϴ�.";
                popup.confirm.onClick.AddListener(async () =>
                {
                    popup.Close();

                    await Task.Delay(500);
                    Application.Quit();
                });
            }
        }

        if (progressBarGroup.activeSelf)
            progressBarGroup.SetActive(false);

        CheckJWT();
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

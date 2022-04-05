using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class LoginManager : MonoBehaviour
{
    public List<string> assetKeys = new List<string>();

    private void InitaiteAddressable()
    {
        Addressables.InitializeAsync().Completed += (op) => Addressables.Release(op);
    }

    private void CheckAsset()
    {
        loginState = LoginState.AssetCheck;

        for (int i = 0; i < assetKeys.Count; i++)
        {
            var assetKey = assetKeys[i];
            Addressables.GetDownloadSizeAsync(assetKey).Completed += (opSize) =>
            {
                if (opSize.Status == AsyncOperationStatus.Succeeded)
                {
                    if (opSize.Result > 0)
                    {
                        Addressables.DownloadDependenciesAsync(assetKey).Completed += (opDownload) =>
                        {

                        };
                    }
                }
            };
        }
    }
}

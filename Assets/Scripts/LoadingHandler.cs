using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] Image img_Loading = null;
    [SerializeField] GameObject languageSelection = null;
    [SerializeField] TextMeshProUGUI txtVersion;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsynchronously());
    }

    private IEnumerator LoadAsynchronously()
    {
        int gameSceneIndex = 1;
        float progress = 0f;

        yield return new WaitForSeconds(0.5f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneIndex);
        while (!operation.isDone)
        {
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            img_Loading.fillAmount = progress;
            yield return Yielders.EndOfFrame;
        }
    }

#if UNITY_EDITOR
    [ContextMenu ("Reset PlayerPrefs")]
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
#endif
}

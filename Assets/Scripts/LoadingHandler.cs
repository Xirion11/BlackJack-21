using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] Image img_Loading = null;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsynchronously());
    }

    private IEnumerator LoadAsynchronously()
    {
        int gameSceneIndex = 1;
        float progress = 0f;

        const float HalfSecond = 0.5f;
        const float LoaderIncrements = 0.9f;

        yield return new WaitForSeconds(HalfSecond);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneIndex);
        while (!operation.isDone)
        {
            
            progress = Mathf.Clamp01(operation.progress / LoaderIncrements);
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

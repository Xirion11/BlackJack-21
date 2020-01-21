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
        //Start loading the next scene
        StartCoroutine(LoadAsynchronously());
    }

    private IEnumerator LoadAsynchronously()
    {
        int gameSceneIndex = 1;
        float progress = 0f;

        const float HalfSecond = 0.5f;
        const float LoaderIncrements = 0.9f;

        //Wait half a second so the loading screen is visible for a little

        yield return new WaitForSeconds(HalfSecond);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneIndex);
        while (!operation.isDone)
        {
            //Update progress bar
            progress = Mathf.Clamp01(operation.progress / LoaderIncrements);
            img_Loading.fillAmount = progress;
            yield return Yielders.EndOfFrame;
        }
    }

#if UNITY_EDITOR
    //Cheats to get a clean save
    [ContextMenu ("Reset PlayerPrefs")]
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
#endif
}

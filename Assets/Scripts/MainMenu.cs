using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public AudioSource clickSound;
    
    public Button playButton;
    public Button exitButton;

    public string registrationSceneName = "RegistrationScene";

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClick);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClick);
        }
    }

    private void OnPlayClick()
    {
        StartCoroutine(PlaySoundAndLoadScene(registrationSceneName));
    }

    private void OnExitClick()
    {
        StartCoroutine(PlaySoundAndQuit());
    }

    private System.Collections.IEnumerator PlaySoundAndLoadScene(string sceneName)
    {
        if (clickSound != null)
            clickSound.Play();

        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene(sceneName);
    }

    private System.Collections.IEnumerator PlaySoundAndQuit()
    {
        if (clickSound != null)
            clickSound.Play();

        yield return new WaitForSeconds(0.7f);
        Application.Quit();
        Debug.Log("Game exited.");
    }
}

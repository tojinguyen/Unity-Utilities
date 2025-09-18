using UnityEngine;

public class AudioManagerComponent : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            AudioManager.PauseAllAudio();
        }
        else
        {
            AudioManager.ResumeAllAudio();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            AudioManager.PauseAllAudio();
        }
        else
        {
            AudioManager.ResumeAllAudio();
        }
    }

    private void OnDestroy()
    {
        AudioManager.Cleanup();
    }
}
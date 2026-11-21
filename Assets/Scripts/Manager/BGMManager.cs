
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Scene BGM")]
    [SerializeField] private SceneBGM[] sceneBGMs;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private float targetVolume = 1f;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        targetVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        audioSource.volume = 0f;
    }

    private void Start()
    {
        // 게임을 처음 실행했을 때 현재 씬의 BGM 재생
        PlaySceneBGM(SceneManager.GetActiveScene().name);
    }

    // 현재 BGM을 페이드 아웃
    public Coroutine FadeOut()
    {
        StopFadeCoroutine();

        fadeCoroutine = StartCoroutine(FadeOutRoutine());

        return fadeCoroutine;
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return FadeVolumeRoutine(0f, false);

        fadeCoroutine = null;
    }

    // 지정한 씬의 BGM 재생
    public Coroutine PlaySceneBGM(string sceneName)
    {
        AudioClip newClip = GetSceneClip(sceneName);

        StopFadeCoroutine();

        fadeCoroutine = StartCoroutine(ChangeBGM(newClip));

        return fadeCoroutine;
    }

    private IEnumerator ChangeBGM(AudioClip newClip)
    {
        // 해당 씬에 등록된 BGM이 없는 경우
        if (newClip == null)
        {
            yield return FadeVolumeRoutine(0f, false);

            audioSource.Stop();
            audioSource.clip = null;

            fadeCoroutine = null;
            yield break;
        }

        // 기존 BGM과 동일하다면 음악을 다시 시작하지 않음
        if (audioSource.clip == newClip && audioSource.isPlaying)
        {
            yield return FadeVolumeRoutine(targetVolume, true);

            fadeCoroutine = null;
            yield break;
        }

        // 기존 음악을 페이드 아웃
        yield return FadeVolumeRoutine(0f, false);

        // 새로운 음악 재생
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        // 새로운 음악을 페이드 인
        yield return FadeVolumeRoutine(targetVolume, true);

        fadeCoroutine = null;
    }

    // 볼륨 페이드 처리
    private IEnumerator FadeVolumeRoutine(float destination, bool followTargetVolume)
    {
        float duration = followTargetVolume
            ? fadeInDuration
            : fadeDuration;

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float currentDestination = followTargetVolume
                ? targetVolume
                : destination;

            audioSource.volume = Mathf.Lerp(
                startVolume,
                currentDestination,
                t
            );

            yield return null;
        }

        audioSource.volume = followTargetVolume
            ? targetVolume
            : destination;
    }

    private AudioClip GetSceneClip(string sceneName)
    {
        foreach (SceneBGM sceneBGM in sceneBGMs)
        {
            if (sceneBGM.sceneName == sceneName)
            {
                return sceneBGM.bgmClip;
            }
        }

        Debug.LogWarning(
            "[BGMManager] 해당 씬의 BGM이 설정되지 않았습니다: " + sceneName
        );

        return null;
    }

    // 설정창의 BGM 볼륨 슬라이더에서 호출
    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("BGMVolume", targetVolume);

        if (fadeCoroutine == null)
        {
            audioSource.volume = targetVolume;
        }
    }

    public float GetVolume()
    {
        return targetVolume;
    }

    private void StopFadeCoroutine()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }
}

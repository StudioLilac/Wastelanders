using Cinemachine;
using System.Collections;
using Systems.Persistence;
using UnityEngine;

#nullable enable
public record ShakeScreen(float Intensity) : IEvent;
public record GetActiveCamera(): IQuery<CinemachineVirtualCamera?>;
public class ScreenShakeHandler : MonoBehaviour
{
    public static bool IsScreenShakeEnabled
    {
        get => SaveLoadSystem.Instance.GetUserPreferences().screenShakePreference.IsScreenShakeEnabled;
        set => SaveLoadSystem.Instance.GetUserPreferences().screenShakePreference.IsScreenShakeEnabled = value;
    }

    private void Awake()
    {
        this.Subscribe<ShakeScreen>(evt => AttackCameraEffect(evt.Intensity));
    }

    private void AttackCameraEffect(float percentageMax)
    {
        if (!IsScreenShakeEnabled) return;

        var activeCamera = new GetActiveCamera().Query();
        if (activeCamera == null)
        {
            Debug.LogError("No active Cinemachine virtual camera provided.");
            return;
        }

        if (percentageMax < 0.25f)
        {
            StartCoroutine(ShakeCamera(percentageMax, activeCamera));
            StartCoroutine(ZoomEffect(-percentageMax, activeCamera));
        }
        else if (percentageMax < 0.75f)
        {
            StartCoroutine(ShakeCamera(percentageMax, activeCamera));
            StartCoroutine(ZoomEffect(-percentageMax, activeCamera));
        }
        else
        {
            StartCoroutine(ShakeCamera(percentageMax, activeCamera));
            StartCoroutine(ZoomEffect(-percentageMax, activeCamera));
            StartCoroutine(TiltEffect(percentageMax, activeCamera));
        }
    }

    //(@param percentageMax) is a float [0, 1]
    private IEnumerator ShakeCamera(float percentageMax, CinemachineVirtualCamera activeCamera)
    {
        CinemachineBasicMultiChannelPerlin? virtualCameraNoise = activeCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        if (virtualCameraNoise == null)
        {
            Debug.LogError("The active camera does not have a CinemachineBasicMultiChannelPerlin component.");
            yield break;
        }

        float time = Mathf.Min(0.3f, percentageMax);
        float shakeAmplitude = 1f + percentageMax / 2f;
        float shakeFrequency = 1f + percentageMax * 3f / 4f;
        virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
        virtualCameraNoise.m_FrequencyGain = shakeFrequency;
        yield return new WaitForSeconds(time + percentageMax / 20f);
        virtualCameraNoise.m_AmplitudeGain = 0f;
        virtualCameraNoise.m_FrequencyGain = 0f;
    }

    //(@param percentageMax) is a float [0, 1]
    private IEnumerator ZoomEffect(float percentageMax, CinemachineVirtualCamera activeCamera)
    {
        float startFOV = activeCamera.m_Lens.OrthographicSize;
        float endFOV = startFOV - percentageMax;
        yield return StartCoroutine(ZoomCamera(startFOV, endFOV, activeCamera));
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(ZoomCamera(endFOV, startFOV, activeCamera));
    }

    private IEnumerator ZoomCamera(float startFOV, float endFOV, CinemachineVirtualCamera activeCamera)
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            activeCamera.m_Lens.OrthographicSize = Mathf.Lerp(startFOV, endFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeCamera.m_Lens.OrthographicSize = endFOV;
    }

    private IEnumerator TiltEffect(float tilt, CinemachineVirtualCamera activeCamera)
    {
        yield break;
    }
}

[System.Serializable]
public class ScreenShakePreference : ISaveable
{
    public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    [field: SerializeField] public bool IsScreenShakeEnabled { get; set; } = true;
    
}
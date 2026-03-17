using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterHelper : MonoBehaviour
{
    [Header("Settings")]
    public float charsPerSecond = 50f;
    public float eraseCharsPerSecond = 80f;

    private TextMeshProUGUI _txtView;
    private Coroutine _currentRoutine;

    private void Awake()
    {
        _txtView = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        _txtView.maxVisibleCharacters = 0;
    }


    public void Play(string textToType = null)
    {
        if (!string.IsNullOrEmpty(textToType))
        {
            _txtView.text = textToType;
        }

        StopCurrentAnimation();
        _currentRoutine = StartCoroutine(TypewriteText());
    }

    public void Skip()
    {
        StopCurrentAnimation();
        _txtView.maxVisibleCharacters = _txtView.textInfo.characterCount;
    }


    public void Erase()
    {
        StopCurrentAnimation();
        _currentRoutine = StartCoroutine(EraseText());
    }

    public void SkipErase()
    {
        StopCurrentAnimation();
        _txtView.maxVisibleCharacters = 0;
        _txtView.text = string.Empty;
    }


    private void StopCurrentAnimation()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
            _currentRoutine = null;
        }
    }

    private IEnumerator TypewriteText()
    {
        _txtView.ForceMeshUpdate();
        int totalVisibleCharacters = _txtView.textInfo.characterCount;

        _txtView.maxVisibleCharacters = 0;
        float elapsed = 0f;

        while (_txtView.maxVisibleCharacters < totalVisibleCharacters)
        {
            _txtView.maxVisibleCharacters = Mathf.FloorToInt(elapsed * charsPerSecond);
            elapsed += Time.deltaTime;

            yield return null;
        }

        // Ensure we hit the exact max at the end
        _txtView.maxVisibleCharacters = totalVisibleCharacters;
        _currentRoutine = null;
    }

    private IEnumerator EraseText()
    {
        _txtView.ForceMeshUpdate();

        int startingVisible = _txtView.maxVisibleCharacters;

        // Failsafe in case maxVisibleCharacters is out of bounds
        if (startingVisible > _txtView.textInfo.characterCount || (startingVisible == 0 && _txtView.text.Length > 0))
        {
            startingVisible = _txtView.textInfo.characterCount;
        }

        float elapsed = 0f;

        while (_txtView.maxVisibleCharacters > 0)
        {
            elapsed += Time.deltaTime;

            int charsToRemove = Mathf.FloorToInt(elapsed * eraseCharsPerSecond);
            _txtView.maxVisibleCharacters = Mathf.Max(0, startingVisible - charsToRemove);

            yield return null;
        }

        _txtView.text = string.Empty;
        _currentRoutine = null;
    }
}
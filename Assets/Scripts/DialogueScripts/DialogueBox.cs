using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//This class handles Rolling Text behavior, Use this in place of TextMeshProUGUI components to set text
public class DialogueBox : MonoBehaviour
{
    public const string RENDERING_LAYER = "DialogueLayer";
    private const float DEFAULTROLLSPEED = 50f;

    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Image displayingImage;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;

    public float rollingSpeed;

    private bool lineIsFinished;

    public delegate void DialogueBoxDelegate();
    public static event DialogueBoxDelegate DialogueBoxEvent; //New event to help you coordinate cool things during dialogue

    public void SetLine(DialogueText line)
    {
        if (line.DisplayingImage == null)
        {
            displayingImage.enabled = false;
        } else
        {
            aspectRatioFitter.aspectRatio = line.DisplayingImage.bounds.size.x / line.DisplayingImage.bounds.size.y;
            displayingImage.enabled = true;
            displayingImage.sprite = line.DisplayingImage;
        }

        SetFontStyles(line);

        bodyText.text = line.BodyText;
        if (line.SpeakerName != "") {
            nameText.text = line.SpeakerName;
        } else {
            nameText.text = "Narrator";
        }
        bodyText.maxVisibleCharacters = 0;
        bodyText.text = line.BodyText;

        if (line.broadcastAnEvent) DialogueBoxEvent?.Invoke();
        StartDialogue();
    }

    private void Awake()
    {
        if (rollingSpeed == 0)
        {
            rollingSpeed = DEFAULTROLLSPEED;
        }
    }

    public static void ClearDialogueEvents()
    {
        DialogueBoxEvent = null;
    }

    void SetFontStyles(DialogueText line)
    {

        if (line.Bold)
        {
            bodyText.fontStyle |= FontStyles.Bold;
        }
        else
        {
            bodyText.fontStyle &= ~FontStyles.Bold;
        }

        if (line.Italics)
        {
            bodyText.fontStyle |= FontStyles.Italic;
        }
        else
        {
            bodyText.fontStyle &= ~FontStyles.Italic;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StopScrollingText()
    {
        if (bodyText.maxVisibleCharacters < bodyText.text.Length)
        {
            StopAllCoroutines();
            bodyText.maxVisibleCharacters = bodyText.text.Length;
            lineIsFinished = true;
        }
    }


    void StartDialogue()
    {
        StartCoroutine(TypeLine());
    }

    //Effects: Adds the rolling dialogue effect to the text
    IEnumerator TypeLine()
    {

        lineIsFinished = false;

        for (int i = 1; i <= bodyText.text.Length; i++)
        {
            bodyText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(1f / rollingSpeed);
        }

        lineIsFinished = true;

    }

    public bool FinishedLine()
    {
        return lineIsFinished;
    }

}

using UnityEngine;

public class EpilogueSceneManager : MonoBehaviour
{
    [SerializeField] private EpilogueButton epilogueButtonPrefab;
    [SerializeField] private Transform layoutManager;
    [SerializeField] private EpilogueThumbnails epilogueThumbnails;

    void Start()
    {
        foreach (EpilogueSceneData s in EpilogueSceneData.Values)
        {
            EpilogueButton b = Instantiate(epilogueButtonPrefab, layoutManager);
            b.Bind(s, epilogueThumbnails);
        }
    }
}

using UnityEngine;
using System;
using Systems.Persistence;
using System.Collections.Generic;
using System.Linq;
using BountySystem;
using LevelSelectInformation;
using System.Collections;


public record ClearBounty(): IEvent;
public record GetBountyProgress() : IQuery<int?>;

#nullable enable
// A class that persists the current bounty information during level selecting
public class BountyManager : PersistentSingleton<BountyManager>, IBind<BountyStateData>
{
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();

    private BountyStateData? data;

    public BountyStateData ContractStateData
    {
        get
        {
            // This can be null when this manager is created after SaveLoadManager is created. For example, when you start the game in certain scenes.
            if (data == null) SaveLoadSystem.Instance.LoadBountyStateInformation();

            return data!;
        }
        set
        {
            data = value;
        }
    }

    // All ActiveBounty should be contained within BountyInformation's Bounty Collection
    public IBounties? ActiveBounty { get; set; } = null;
    public BountyInformation? SelectedBountyInformation { get; private set; } = null;


    protected override void Awake()
    {
        base.Awake();
        if (invalid) return;

        this.Answer<GetBountyProgress, int?>(_ => ContractStateData.GetNumCompletedBounties());
        this.Subscribe<BountyInformationEvent>(e => SelectedBountyInformation = e.BountyType);
        this.Subscribe<ClearBounty>(_ => 
        {
            ActiveBounty = null;
            SelectedBountyInformation = null;
        });
    }


    public bool IsBountyCompleted(IBounties? bounty)
    {
        if (bounty == null) return false;

        return ContractStateData?.IsBountyCompleted(bounty) ?? false;
    }

    public void NotifyWin()
    {
        if (ActiveBounty != null) ContractStateData?.SetChallengeComplete(ActiveBounty);
    }

    public void GoToEpilogueScene(EpilogueSceneData scene)
    {
        IEnumerator FadeLevelIn(string levelName)
        {
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(0.6f));
            GameStateManager.Instance.LoadScene(levelName);
        }

        StartCoroutine(FadeLevelIn(scene.SceneData.SceneName));
    }

    void IBind<BountyStateData>.Bind(BountyStateData data)
    {
        ContractStateData = data;
        Id = data.Id;
    }
}

// The serialized data for bounties that gets stored in the JSON
[System.Serializable]
public class BountyStateData : ISaveable
{

    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    [field: SerializeField] private List<ChallengeCompletionState> BountyCompletionData { get; set; } = new();

    public void SetChallengeComplete(IBounties bounty)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        if (challengeCompletionState == null) BountyCompletionData.Add(new(bounty.BountyName, true));
        else challengeCompletionState.Completed = true;
    }

    public bool IsBountyCompleted(IBounties bounty)
    {
        ChallengeCompletionState? challengeCompletionState = BountyCompletionData.Find(data => data.BountyName == bounty.BountyName);

        return challengeCompletionState?.Completed ?? false;
    }

    public int GetNumCompletedBounties()
    {
        return BountyCompletionData.Count(data => data.Completed);
    }

    public BountyStateData()
    {
        Initialize();
    }

    private void Initialize()
    {
        BountyCompletionData.Clear();
        IBounties.MapOnValues(bounty => BountyCompletionData.Add(new ChallengeCompletionState(bounty.BountyName, false)));
    }

    [Serializable]
    public class ChallengeCompletionState
    {
        [field: SerializeField] public string BountyName { get; set; }
        [field: SerializeField] public bool Completed { get; set; }

        public ChallengeCompletionState(string bountyName, bool completed)
        {
            Completed = completed;
            BountyName = bountyName;
        }
    }
}

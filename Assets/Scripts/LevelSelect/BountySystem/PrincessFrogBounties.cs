using BountySystem;
using System.Collections.Generic;
using UnityEngine;
using static BountySystem.IBounties;

// PrincessFrogBounties are a collection of challenges. These are to be used in game for the player to select a group of challenges at the same time;
public class PrincessFrogBounties : IBounties
{
    public static readonly PrincessFrogBounties PRINCESS_FROG_CHALLENGE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> 
        { 
            PrincessFrogContracts.ADDITIONAL_ATTACK, 
            PrincessFrogContracts.AGGRESIVE_AI,
            PrincessFrogContracts.EXTRA_RESONANCE,
            PrincessFrogContracts.EXTRA_HEALTH
        },
        challengeName: "Buffed Princess",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "The Princess is back, and she doesn’t intend to lose this time.",
        rewards: "Monster Cards no longer have resonance costs.",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.PrincessFrogChallengeAsset
        );

    public static readonly PrincessFrogBounties QUEEN_CHALLENGE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> { EnemySpawningContracts.QUEEN_BEETLE_SPAWN },
        challengeName: "Queen and Princess",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "Get ready for a royal bloodbath.",
        rewards: "Queen Beetle Cards",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.QueenChallengeAsset
        );

    public static readonly PrincessFrogBounties SLIME_CHALLENGE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> { EnemySpawningContracts.SLIME_SPAWN },
        challengeName: "SlimeFest",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "Test your skills in this slimy situation!",
        rewards: "Slime Cards",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.SlimeChallengeAsset
        );

    public static readonly PrincessFrogBounties FROG_CHALLENGE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> { EnemySpawningContracts.FROG_SPAWN },
        challengeName: "FrogFest",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "These frogs have gone wild, so hop to it!",
        rewards: "Frog Cards",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.FrogChallengeAsset
        );

    public static readonly PrincessFrogBounties SOLO_JACKIE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> { PlayerContracts.SOLO_JACKIE },
        challengeName: "Solo Mission",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "Take on the Princess Beetle alone.",
        rewards: "Storyboard: The Rose and the Gardener.",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.ExhaustedChallengeAsset
        );

    public static readonly PrincessFrogBounties DECREASED_HAND_SIZE = new PrincessFrogBounties(
        contractSet: new HashSet<IContracts> { PlayerContracts.DECREASED_HAND_SIZE },
        challengeName: "Exhausting Mission",
        subtext: "Challenge for Level 5. Extras",
        flavourText: "Fight the Princess Beetle with a restricting hand size.",
        rewards: "Beetle Cards",
        bountyAssetDelegate: (database) => database.PrincessFrogAssets.SoloChallengeAsset
        );

    // Update this every time you add a new bounty please, I don't want to implement reflection.
    public static IEnumerable<PrincessFrogBounties> Values
    {
        get
        {
            yield return QUEEN_CHALLENGE;
            yield return PRINCESS_FROG_CHALLENGE;
            yield return DECREASED_HAND_SIZE;
            yield return FROG_CHALLENGE;
            yield return SLIME_CHALLENGE;
            yield return SOLO_JACKIE;
        }
    }
    public HashSet<IContracts> ContractSet { get; }
    public string BountyName { get; }
    public string FlavourText { get; }
    public string SubText { get; }
    public string Rewards { get; }
    public GetBountyAssetsDelegate GetBountyAssets { get; }
    public string SceneName => SceneData.Get<SceneData.PrincessFrogBounty>().SceneName;

    public PrincessFrogBounties(HashSet<IContracts> contractSet, string challengeName, string subtext, string flavourText, string rewards, GetBountyAssetsDelegate bountyAssetDelegate)
    {
        ContractSet = contractSet;
        FlavourText = flavourText;
        BountyName = challengeName;
        Rewards = rewards;
        GetBountyAssets = bountyAssetDelegate;
        SubText = subtext;
    }
}

[System.Serializable]
public class PrincessFrogAssets
{
    public BountyAssets PrincessFrogChallengeAsset;
    public BountyAssets QueenChallengeAsset;
    public BountyAssets SlimeChallengeAsset;
    public BountyAssets FrogChallengeAsset;
    public BountyAssets SoloChallengeAsset;
    public BountyAssets ExhaustedChallengeAsset;
}
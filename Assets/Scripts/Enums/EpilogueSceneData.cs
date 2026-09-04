using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EpilogueSceneData : Enum<EpilogueSceneData>
{
    public abstract string EpilogueTitle { get; }
    public abstract int BountyRequirement { get; }
    public abstract SceneData SceneData { get; }
    public abstract Sprite GetThumbnail(EpilogueThumbnails thumbnail);

    public new static IEnumerable<EpilogueSceneData> Values => Enum<EpilogueSceneData>.Values;

# nullable enable
    public static EpilogueSceneData? LatestCompleted(int completed) => Values
        .OrderByDescending(e => e.BountyRequirement)
        .FirstOrDefault(e => e.BountyRequirement <= completed);

    public class Epilogue_4 : EpilogueSceneData
    {
        public override string EpilogueTitle => "Ideal Self";
        public override int BountyRequirement => 1;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_4>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue4;
    }

    public class Epilogue_5 : EpilogueSceneData
    {
        public override string EpilogueTitle => "The Lab";
        public override int BountyRequirement => 2;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_5>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue5;
    }

    public class Epilogue_6 : EpilogueSceneData
    {
        public override string EpilogueTitle => "The Call";
        public override int BountyRequirement => 3;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_6>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue6;
    }

    public class Epilogue_7 : EpilogueSceneData
    {
        public override string EpilogueTitle => "The Cave";
        public override int BountyRequirement => 4;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_7>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue7;
    }

    public class Epilogue_8 : EpilogueSceneData
    {
        public override string EpilogueTitle => "The Wastelanders";
        public override int BountyRequirement => 5;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_8>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue8;
    }

    public class Epilogue_9 : EpilogueSceneData
    {
        public override string EpilogueTitle => "The Posessed";
        public override int BountyRequirement => 6;
        public override SceneData SceneData => SceneData.Get<SceneData.Epilogue_9>();
        public override Sprite GetThumbnail(EpilogueThumbnails thumbnail) => thumbnail.epilogue9;
    }

}
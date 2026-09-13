#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scans a grayscale chunk sheet for its distinct grey levels and bakes a
/// 256x1 palette ramp for Impact/WasteParticlePixelRamp.
///
/// Nearest-shade fill by default, so the ramp contains exactly as many colours
/// as your art has shades. No interpolation inventing in-between values that
/// aren't in your palette.
///
///     Tools > Impact > Palette Ramp Baker
/// </summary>
public class PaletteRampBaker : EditorWindow
{
    class Shade
    {
        public byte value;
        public int count;
        public Color color;
    }

    Texture2D _sheet;
    float _cutoff = 0.5f;
    bool _interpolate = false;

    List<Shade> _shades;
    string _warning = "";
    Vector2 _scroll;

    // dark indigo -> violet -> bright purple -> pale lilac
    static readonly Color[] kDefaultRamp =
    {
        new Color32(0x2A, 0x0A, 0x4A, 255),
        new Color32(0x6B, 0x24, 0xB8, 255),
        new Color32(0xB4, 0x5B, 0xFF, 255),
        new Color32(0xF2, 0xD9, 0xFF, 255),
    };

    [MenuItem("Tools/Impact/Palette Ramp Baker")]
    static void Open()
    {
        var w = GetWindow<PaletteRampBaker>("Palette Ramp");
        w.minSize = new Vector2(420, 380);
        if (Selection.activeObject is Texture2D t) w._sheet = t;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
        var newSheet = (Texture2D)EditorGUILayout.ObjectField(
            "Chunk Sheet", _sheet, typeof(Texture2D), false);
        if (newSheet != _sheet) { _sheet = newSheet; _shades = null; }

        _cutoff = EditorGUILayout.Slider("Alpha Cutoff", _cutoff, 0f, 1f);

        using (new EditorGUI.DisabledScope(_sheet == null))
        {
            if (GUILayout.Button("Scan shades", GUILayout.Height(24)))
                Scan();
        }

        CheckSheetSrgb();

        if (!string.IsNullOrEmpty(_warning))
            EditorGUILayout.HelpBox(_warning, MessageType.Warning);

        if (_shades == null || _shades.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Scan a grayscale sheet to list its shades, then assign a colour " +
                "to each one.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Shades ({_shades.Count})", EditorStyles.boldLabel);

        if (GUILayout.Button("Re-apply default purple ramp"))
            ApplyDefaultRamp();

        EditorGUILayout.Space();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        int total = _shades.Sum(s => s.count);
        foreach (var s in _shades)
        {
            EditorGUILayout.BeginHorizontal();

            var swatch = GUILayoutUtility.GetRect(28, 18, GUILayout.Width(28));
            float g = s.value / 255f;
            EditorGUI.DrawRect(swatch, new Color(g, g, g));

            EditorGUILayout.LabelField($"{s.value}", GUILayout.Width(34));
            EditorGUILayout.LabelField($"{100f * s.count / total:0.0}%", GUILayout.Width(46));

            s.color = EditorGUILayout.ColorField(GUIContent.none, s.color, true, false, false);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // live preview strip
        EditorGUILayout.Space();
        var strip = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, 22);
        DrawPreview(strip);

        EditorGUILayout.Space();
        _interpolate = EditorGUILayout.ToggleLeft(
            "Interpolate between shades (off = nearest, pixel-art correct)",
            _interpolate);

        if (GUILayout.Button("Bake 256x1 ramp", GUILayout.Height(28)))
            Bake();
    }

    // -------------------------------------------------------------------------

    void Scan()
    {
        _warning = "";
        var px = ReadPixels(_sheet);
        byte cut = (byte)Mathf.RoundToInt(_cutoff * 255f);

        var counts = new Dictionary<byte, int>();
        bool nonGray = false;

        foreach (var c in px)
        {
            if (c.a < cut) continue;
            if (c.r != c.g || c.g != c.b) nonGray = true;
            counts.TryGetValue(c.r, out int n);
            counts[c.r] = n + 1;
        }

        if (nonGray)
        {
            _warning = "Some opaque pixels aren't grayscale (R, G and B differ). " +
                       "The ramp indexes on the red channel, so those pixels will " +
                       "map by their red value only.";
        }

        _shades = counts
            .OrderBy(kv => kv.Key)
            .Select(kv => new Shade { value = kv.Key, count = kv.Value })
            .ToList();

        if (_shades.Count > 32)
        {
            _warning += (_warning.Length > 0 ? "\n\n" : "") +
                        $"{_shades.Count} distinct shades found. That's a lot for " +
                        "pixel art -- the source may be antialiased or resampled.";
        }

        ApplyDefaultRamp();
    }

    void ApplyDefaultRamp()
    {
        if (_shades == null) return;
        foreach (var s in _shades)
            s.color = SampleDefault(s.value / 255f);
    }

    static Color SampleDefault(float t)
    {
        t = Mathf.Clamp01(t) * (kDefaultRamp.Length - 1);
        int i = Mathf.FloorToInt(t);
        int j = Mathf.Min(i + 1, kDefaultRamp.Length - 1);
        return Color.Lerp(kDefaultRamp[i], kDefaultRamp[j], t - i);
    }

    void DrawPreview(Rect r)
    {
        if (_shades == null || _shades.Count == 0) return;
        var ramp = BuildRamp();
        float step = r.width / 256f;
        for (int i = 0; i < 256; i++)
            EditorGUI.DrawRect(new Rect(r.x + i * step, r.y, step + 1f, r.height), ramp[i]);
    }

    Color[] BuildRamp()
    {
        var ramp = new Color[256];

        for (int i = 0; i < 256; i++)
        {
            float v = i;

            if (!_interpolate)
            {
                // nearest authored shade -- exactly N colours out for N shades in
                Shade best = _shades[0];
                float bestD = Mathf.Abs(v - best.value);
                foreach (var s in _shades)
                {
                    float d = Mathf.Abs(v - s.value);
                    if (d < bestD) { bestD = d; best = s; }
                }
                ramp[i] = best.color;
            }
            else
            {
                Shade lo = _shades[0], hi = _shades[_shades.Count - 1];
                foreach (var s in _shades)
                {
                    if (s.value <= v && s.value >= lo.value) lo = s;
                    if (s.value >= v && s.value <= hi.value) hi = s;
                }
                float span = hi.value - lo.value;
                float t = span <= 0f ? 0f : (v - lo.value) / span;
                ramp[i] = Color.Lerp(lo.color, hi.color, t);
            }

            ramp[i].a = 1f;
        }

        return ramp;
    }

    void Bake()
    {
        string dir = _sheet != null
            ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(_sheet))
            : "Assets";

        string path = EditorUtility.SaveFilePanel(
            "Save palette ramp", dir, "WasteChunks_Ramp", "png");

        if (string.IsNullOrEmpty(path)) return;

        var ramp = BuildRamp();
        var tex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        tex.SetPixels(ramp);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        if (!path.StartsWith(Application.dataPath))
        {
            Debug.LogWarning("[PaletteRampBaker] Saved outside Assets/ -- " +
                             "import settings were not applied.");
            return;
        }

        string rel = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceUpdate);

        if (AssetImporter.GetAtPath(rel) is TextureImporter imp)
        {
            imp.textureType = TextureImporterType.Default;
            imp.sRGBTexture = true;              // the ramp IS colour
            imp.filterMode = FilterMode.Point;   // hard steps between slots
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.npotScale = TextureImporterNPOTScale.None;
            imp.alphaIsTransparency = false;
            imp.maxTextureSize = Mathf.Max(imp.maxTextureSize, 256);
            imp.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(rel);
        Debug.Log($"[PaletteRampBaker] Baked ramp from {_shades.Count} shades -> {rel}");
    }

    /// <summary>
    /// The index sheet must NOT be sRGB. In a Linear project an sRGB-flagged
    /// texture gets gamma-decoded on sample, so grey 128 would arrive as 0.216
    /// and look up ramp texel 55 instead of 128.
    /// </summary>
    void CheckSheetSrgb()
    {
        if (_sheet == null) return;

        var path = AssetDatabase.GetAssetPath(_sheet);
        if (!(AssetImporter.GetAtPath(path) is TextureImporter imp)) return;
        if (!imp.sRGBTexture) return;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This sheet has sRGB enabled. For ramp indexing it must be OFF -- " +
            "the grey is an index, not a colour. Leaving it on shifts every " +
            "lookup in a Linear colour space project.", MessageType.Error);

        if (GUILayout.Button("Turn off sRGB on the sheet"))
        {
            imp.sRGBTexture = false;
            imp.SaveAndReimport();
            _shades = null;
        }
    }

    static Color32[] ReadPixels(Texture2D src)
    {
        var prevFilter = src.filterMode;
        src.filterMode = FilterMode.Point;

        var rt = RenderTexture.GetTemporary(src.width, src.height, 0,
            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        var prevActive = RenderTexture.active;
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;

        var tmp = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, true);
        tmp.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        tmp.Apply();

        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);
        src.filterMode = prevFilter;

        var px = tmp.GetPixels32();
        Object.DestroyImmediate(tmp);
        return px;
    }
}
#endif

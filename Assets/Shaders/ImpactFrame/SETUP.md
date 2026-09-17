# Impact Frame — setup

Written in CG/`UnityCG.cginc` so it compiles unchanged on Built-in and URP
(including the 2D Renderer). No renderer features, no camera stacking, no layer
juggling. Ask if you want native HLSL/URP versions for SRP batching.

## Files

```
Shaders/ImpactMask.shader        stamps flat silhouettes into a 1-channel RT
Shaders/ImpactFrame.shader       composites mask -> black on white, fullscreen
Scripts/ImpactSilhouette.cs      per-object opt-in tag
Scripts/ImpactFrameController.cs mask render + fullscreen quad, on the main cam
Scripts/ImpactSequencer.cs       choreography and time manipulation
```

## Wiring

1. Drop everything under `Assets/`.
2. Add **Impact Frame Controller** to the main camera. Shader fields auto-resolve
   via `Shader.Find`, but assign them in the inspector so they survive a build —
   nothing else references these shaders, so they will get stripped otherwise.
   (Alternatively add both to Project Settings > Graphics > Always Included.)
3. Add **Impact Silhouette** to the player, the enemy, the crown, and each
   ParticleSystem you want in the mask. `Reset` grabs child renderers
   automatically; override the list if you want to exclude a shadow or a
   backdrop sprite.
4. Add **Impact Sequencer** somewhere in the scene and wire the references.
   Call `Play()` from your hit detection.

## Verifying before you hook up the sequence

Drag the controller's `intensity` slider to 1 in play mode. You should see the
frame immediately. If you get a white screen with no silhouettes:

- The objects are missing `ImpactSilhouette`, or
- `alphaCutoff` is above the sprite's alpha, or
- a particle material has no texture (see gotchas).

## Tuning

| Field | Effect |
|---|---|
| `dilatePixels` | Fattens silhouettes. Thin details like crown prongs vanish under a hard threshold otherwise. 1.5–3 is the useful range. |
| `maskDownsample` | 2 gives a chunkier, more stylised edge and costs less. Pairs well with a low-res 2D game. |
| `threshold` | Where the hard cut lands. Raise it if soft particle edges are smearing. |
| `alphaCutoff` | Applied in the mask pass. Raise it if particles come out as blobs rather than shapes. |
| `silhouetteColor` / `backgroundColor` | Swap them for a white-on-black variant, or tint the background a bruised purple to tie into the particles. |

## Gotchas

**Particle materials need a real texture.** The mask alpha-clips against
`_SilhouetteTex`, resolved from the material's main texture. A procedural
particle shader with no texture makes every particle a solid square. Assign a
chunk/blob texture, or set `textureOverride` on the `ImpactSilhouette`.

**Screen Space – Overlay canvases cannot be covered.** They render after every
camera and after post-processing. The sequencer fades `hud` out instead. If you
genuinely need UI inside the silhouette, switch that canvas to Screen Space –
Camera and add `ImpactSilhouette` to its graphics.

**The frame sits inside post-processing.** The quad renders in the transparent
pass, so bloom will bite the white background. Usually a plus — if you want it
untouched, move the composite to a renderer feature at
`AfterRenderingPostProcessing`.

**Don't use `freezeTimeScale = 0`.** At exactly 0 the crown stops dead. 0.02–0.05
lets it drift back over the hold, which is the "time slows" read you described.
`SetTimeScale` scales `fixedDeltaTime` alongside so physics doesn't stutter.

**Sprite masks and 2D lights don't apply to the mask pass.** The mask ignores
them by design — the silhouette is the sprite's own alpha.

## Next

Waste particle shader (chunky posterised purple, dissolve-out edges rather than
alpha fade), the cone/radial ParticleSystem configs, and the enemy dissolve.

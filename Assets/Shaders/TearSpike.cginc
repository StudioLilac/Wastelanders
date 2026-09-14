#ifndef WASTELANDERS_TEAR_SPIKE_INCLUDED
#define WASTELANDERS_TEAR_SPIKE_INCLUDED

// Written globally by TearFilm.cs. Shared by every affected shader, which is what
// guarantees all sources flare on the same axis. Vary the angle per source and the
// effect reads as glitter instead of vision.
float _TearStrength;
float _TearAxis;

float2 TearRotate(float2 p, float angle)
{
    float s, c;
    sincos(angle, s, c);
    return float2(p.y * c - p.x * s, -p.x * c - p.y * s);
}

// One flare line running along the local x axis.
//
// bias makes it lopsided: at 0 the prong is even on both sides, at 0.5 the
// positive side reaches half again as far as the negative. Symmetric prongs look
// synthetic, so a nonzero bias is the default rather than the exception.
float TearProng(float2 q, float reach, float thickness, float falloff, float bias)
{
    float side = q.x >= 0.0 ? 1.0 : -1.0;
    float r = max(reach * (1.0 + bias * side), 1e-4);

    float along  = pow(saturate(1.0 - abs(q.x) / r), falloff);
    float across = pow(saturate(1.0 - abs(q.y) / max(thickness, 1e-4)), 1.5);

    return along * across;
}

// Two prongs: a dominant one on the film axis and a fainter one rotated off it.
float TearFlare(
    float2 p,
    float  majorReach,
    float  minorReach,
    float  thickness,
    float  falloff,
    float  minorRatio,
    float  majorBias,
    float  minorBias,
    float  minorAngle)
{
    float2 qa = TearRotate(p, _TearAxis);
    float major = TearProng(qa, majorReach, thickness, falloff, majorBias);

    float2 qb = TearRotate(p, _TearAxis + minorAngle);
    float minor = TearProng(qb, minorReach, thickness, falloff, minorBias) * minorRatio;

    return major + minor;
}

#endif

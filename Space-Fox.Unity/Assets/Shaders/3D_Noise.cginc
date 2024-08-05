float RandomValue(float3 xyz)
{
    return frac(sin(dot(xyz, float3(12.9898, 78.233, 628.611))) * 43758.5453);
}

float RandomValueLerped(float3 i, float t, float3 add0, float3 add1)
{
    float3 r0 = RandomValue(i + add0);
    float3 r1 = RandomValue(i + add1);
    return lerp(r0, r1, t);
}

float NoiseValue(float3 xyz)
{
    float3 i = floor(xyz);
    float3 f = frac(xyz);
    f = f * f * (3.0 - 2.0 * f);

    float bottomBack = RandomValueLerped(i, f.x,
        float3(0.0, 0.0, 0.0),
        float3(1.0, 0.0, 0.0));

    float topBack = RandomValueLerped(i, f.x,
        float3(0.0, 1.0, 0.0),
        float3(1.0, 1.0, 0.0));

    float bottomFront = RandomValueLerped(i, f.x,
        float3(0.0, 0.0, 1.0),
        float3(1.0, 0.0, 1.0));

    float topFront = RandomValueLerped(i, f.x,
        float3(0.0, 1.0, 1.0),
        float3(1.0, 1.0, 1.0));

    float back = lerp(bottomBack, topBack, f.y);
    float front = lerp(bottomFront, topFront, f.y);
	
    float t = lerp(back, front, f.z);
	
    return t;
}

float NoiseValueByMode(float3 xyz, float scale, float mode)
{
    float freq = pow(2.0, mode);
    float amp = pow(0.5, 3 - mode);
    return NoiseValue(xyz * scale / freq) * amp;
}

void Noise_3D_float(float3 XYZ, float Scale, out float Out)
{
    float t = 0.0;

    t += NoiseValueByMode(XYZ, Scale, 0);
    t += NoiseValueByMode(XYZ, Scale, 1);
    t += NoiseValueByMode(XYZ, Scale, 2);

    Out = t;
}

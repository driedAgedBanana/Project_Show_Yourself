using System;
using UnityEngine;

public static class WeaponNoiseManager
{
    // The position of the noise and the radius around it covers
    public static Action<Vector3, float> OnNoiseMade;

    public static void MakeNoise(Vector3 position, float radius)
    {
        OnNoiseMade?.Invoke(position, radius);
    }
}

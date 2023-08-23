using UnityEngine;

public class Phosphorescent : Crystal
{
    private void Update()
    {
        Crystal[] lineOfSightCrystals = InLineOfSight();

        float maxBrightness = 0f;
        foreach (var crystal in lineOfSightCrystals)
        {
            maxBrightness = Mathf.Max(maxBrightness, crystal.LightLevel);
        }

        if (maxBrightness > LightLevel)
        {
            LightLevel += 0.02f;
        }
        else if (maxBrightness < LightLevel)
        {
            LightLevel -= 0.02f;
        }
    }
}

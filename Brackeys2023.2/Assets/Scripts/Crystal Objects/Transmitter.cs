using UnityEngine;

public class Transmitter : Crystal
{
    private void Update()
    {
        Crystal[] lineOfSightCrystals = InLineOfSight();

        foreach (var crystal in lineOfSightCrystals)
        {
            if (crystal is Collector || crystal is Transmitter)
            {
                if (crystal.LightLevel == 100f)
                {
                    LightLevel = 100f;
                    break;
                }
            }
        }
    }
}

using UnityEngine;

public class Collector : Crystal
{
    private void Update()
    {
        Crystal[] lineOfSightCrystals = InLineOfSight();

        bool sunInLineOfSight = false;
        foreach (var crystal in lineOfSightCrystals)
        {
            if (crystal.CompareTag("Sun"))
            {
                sunInLineOfSight = true;
                break;
            }
        }

        LightLevel = sunInLineOfSight ? 100f : 0f;
    }
}

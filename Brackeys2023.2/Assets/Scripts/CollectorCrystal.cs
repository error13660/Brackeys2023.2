using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorCrystal : Crystal
{
    protected override float GetBrightness(Crystal upstream)
    {
        if (CheckForSky())
        {
            return 1;
        }
        return 0;
    }

    private bool CheckForSky()
    {
        return true;
    }
}

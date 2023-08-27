using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorCrystal : Crystal
{
    private void Awake()
    {
        emmitLight = false;
    }

    protected override float GetBrightness(Crystal upstream)
    {
        if (CheckForSky())
        {
            return 1;
        }
        return 0;
    }

    protected override void Update()
    {
        base.Update();

        brightness = GetBrightness(null);
        if (brightness == 0)
        {
            OnUpdate.Invoke();
        }
    }

    /// <summary>
    /// Returns true if the sky is visible
    /// </summary>
    /// <returns></returns>
    private bool CheckForSky()
    {
        int layerMask = ~(1 << 9);
        Ray ray = new Ray(transform.position+Vector3.up*.3f, Vector3.up);
        if (Physics.Raycast(ray, 10, layerMask))
        {
            return false;
        }
        return true;
    }
}

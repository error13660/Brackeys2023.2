using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance;
    private List<Crystal> activeCrystals;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        activeCrystals = new List<Crystal>();
    }

    public void RegisterCrystal(Crystal crystal)
    {
        activeCrystals.Add(crystal);
    }

    public Crystal GetUpstreamFrom(Crystal crystal)
    {
        Crystal[] inLos = GetInLineOfSightFrom(crystal);
        Crystal brightest = crystal;

        for (int i = 0; i < inLos.Length; i++)
        {

            if (brightest.brightness < inLos[i].brightness)
            {
                brightest = inLos[i];
            }
        }

        if (brightest == crystal) return null;
        return brightest;
    }

    private Crystal[] GetInLineOfSightFrom(Crystal crystal)
    {
        List<Crystal> los = new List<Crystal>();

        for (int i = 0; i < activeCrystals.Count; i++)
        {
            if (crystal.IsLineOfSight(activeCrystals[i])
                && activeCrystals[i] != crystal
                && !activeCrystals[i].IsHeld
                && !crystal.IsHeld)
                los.Add(activeCrystals[i]);
        }
        return los.ToArray();
    }
}

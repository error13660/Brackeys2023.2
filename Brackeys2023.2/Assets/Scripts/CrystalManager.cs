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
        Crystal brightest = null;

        for (int i = 0; i < inLos.Length; i++)
        {
            if (brightest == null)
            {
                brightest = inLos[i];
                continue;
            }

            if (brightest.brightness < inLos[i].brightness)
            {
                brightest = inLos[i];
            }
        }
        return brightest;
    }

    private Crystal[] GetInLineOfSightFrom(Crystal crystal)
    {
        List<Crystal> los = new List<Crystal>();
        int layerMask = 1 << 9;

        for (int i = 0; i < activeCrystals.Count; i++)
        {
            Vector3 delta = activeCrystals[i].transform.position - crystal.transform.position;
            Ray ray = new Ray(crystal.transform.position,
                delta.normalized);
            bool isLos = !Physics.Raycast(ray, delta.magnitude, layerMask); //is there something between?

            if (isLos) los.Add(activeCrystals[i]);
        }
        return los.ToArray();
    }
}

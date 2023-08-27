using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance;
    private List<Crystal> activeCrystals;
    [SerializeField] private Transform[] skyAnchors;

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

    public Crystal[] GetInLineOfSightFrom(Crystal crystal)
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

    public Crystal GetClosest(Vector3 position)
    {
        Crystal crystal = null;

        for (int i = 0; i < activeCrystals.Count; i++)
        {
            if (crystal == null)
            {
                crystal = activeCrystals[i];
                continue;
            }

            if ((position - activeCrystals[i].transform.position).magnitude < //is closer
                (position - crystal.transform.position).magnitude)
            {
                crystal = activeCrystals[i];
            }
        }
        return crystal;
    }

    public Vector3 GetClosestSkyAnchor(Vector3 position)
    {

        Vector3 closest = skyAnchors[0].position;

        for (int i = 1; i < skyAnchors.Length; i++)
        {
            if ((position - skyAnchors[i].position).magnitude < //is closer
                (position - closest).magnitude)
            {
                closest = skyAnchors[i].position;
            }
        }
        return closest;
    }
}

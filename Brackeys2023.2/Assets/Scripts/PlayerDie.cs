using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerDie : MonoBehaviour
{
    [SerializeField] private float chargeDistance;
    [SerializeField] private float skyAnchorChargeDistance;
    [SerializeField] private float charge = 1;
    [SerializeField] private float chargeSpeed = 0.3f;
    [SerializeField] private float dischargeSpeed = 0.05f;

    [SerializeField] private PostProcessVolume volume;

    private void Update()
    {
        if (KinematicController.isPaused) return;

        Crystal crystal = CrystalManager.Instance.GetClosest(transform.position);
        Vector3 skyAnchor = CrystalManager.Instance.GetClosestSkyAnchor(transform.position);

        if ((crystal != null
                && (transform.position - crystal.transform.position).magnitude <= chargeDistance
                && crystal.brightness > 0.01f) //a light source is in range
            || (skyAnchor - transform.position).magnitude <= skyAnchorChargeDistance)
        {
            charge = Mathf.Clamp01(charge + chargeSpeed * Time.deltaTime);
        }
        else
        {
            charge -= dischargeSpeed * Time.deltaTime;
        }

        volume.weight = 1 - charge;

        if (charge <= 0) //DEAD
        {
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();

            UIManager.Instance.DisplayDead();

            //paralise player
            for (int i = 0; i < components.Length; i++)
            {
                Destroy(components[i]);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public abstract class Crystal : MonoBehaviour
{
    [SerializeField]
    private float lightLevel = 0f;

    public float LightLevel
    {
        get { return lightLevel; }
        set { lightLevel = value; }
    }

    public abstract Crystal[] InLineOfSight();
}

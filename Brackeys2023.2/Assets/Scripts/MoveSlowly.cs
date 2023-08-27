using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSlowly : MonoBehaviour
{
    [SerializeField] private Transform destination;
    float startTime;
    [SerializeField] private float moveTime = 8 * 60;
    Vector3 endPosition;
    Vector3 startPosition;

    private void Awake()
    {
        startTime = Time.time;
        endPosition = destination.position;
        startPosition = transform.position;
    }

    void Update()
    {
        float t = (Time.time - startTime) / moveTime;

        if (t > 1) return;

        transform.position = Vector3.Lerp(startPosition, endPosition, t);
    }
}

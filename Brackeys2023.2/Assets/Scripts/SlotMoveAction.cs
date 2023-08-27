using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMoveAction : SlotAction
{
    [SerializeField] private Transform moveEnd;
    [SerializeField] private float moveTime = 2f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnAction()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        float startTime = Time.time;
        Vector3 moveEnd = this.moveEnd.position;
        Vector3 moveStart = transform.position;
        float t = 0;

        while (t <= 1)
        {
            t = (Time.time - startTime) / moveTime;
            transform.position = Vector3.Lerp(moveStart, moveEnd, t);
            yield return null;
        }
        transform.position = moveEnd;
    }

}

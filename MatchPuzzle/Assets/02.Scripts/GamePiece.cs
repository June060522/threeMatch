using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] int xIndex;
    [SerializeField] int yIndex;

    private bool isMoving;

    [SerializeField] InterpType interpolation = InterpType.EaseIn;
    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)Mathf.Round(transform.position.x) + 1, (int)transform.position.y, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)Mathf.Round(transform.position.x) - 1, (int)transform.position.y, 0.5f);
        }
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    private void Move(int dextX, int dextY, float timeToMove)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(dextX, dextY), timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;// 경과시간
        isMoving = true;
        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f) // 목적지에 도착하면
            {
                reachedDestination = true;
                transform.position = destination;
                SetCoord((int)destination.x, (int)destination.y);
            }


            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove,0,1);

            switch(interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseIn:
                    t = 1- Mathf.Cos(t * Mathf.PI * 0.5f);// ease in(천천 - 보통)
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);// ease out(보통-천천)
                    break;
                case InterpType.SmoothStep:
                    t = t * t * (3f - 2f * t);// smoothstep
                    break;
                case InterpType.SmootherStep:
                    t = t * t * t * (t * (6f * t - 15f) + 10f);//smootherStep
                    break;
            }
            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null;
        }
        isMoving = false;
    }
}

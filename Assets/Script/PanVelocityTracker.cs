using UnityEngine;

public class PanVelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }

    private Vector3 lastPos;

    void Update()
    {
        Velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }
}


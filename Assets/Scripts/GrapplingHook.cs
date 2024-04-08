using UnityEngine;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class GrapplingHook : MonoBehaviour
{
    public Transform follow;
    public Rigidbody connectedBody;
    public float hookSpeed = 10;
    public float velocityChange = 5;

    private HookState State 
    { 
        get
        {
            return _state;
        }
        set
        {
            if (_state != value)
            {
                _newState = true;
            }
            _state = value;
        }
    }
    private HookState _state = HookState.Following;
    private bool NewState 
    {
        get 
        {
            if (_newState)
            {
                _newState = false;
                return true;
            }
            return false;
        }
    }
    private bool _newState = true;
    private new Collider collider;
    private Rigidbody rb;
    private LineRenderer line;
    private Vector3[] linePoints = new Vector3[2];

    private void Start()
    {
        transform.SetParent(null, true);

        collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        line.SetPositions(linePoints);
        line.enabled = false;
    }

    private void Update()
    {
        switch (State)
        {
            case HookState.Following:
                // Make hook follow the follow transform when following
                if (NewState)
                {
                    rb.isKinematic = true;
                    collider.enabled = false;
                    line.enabled = false;
                }

                transform.position = follow.position;

                if (Input.GetKey(KeyCode.E))
                {
                    State = HookState.Shooting;
                }
                break;

            case HookState.Shooting:
                // Make hook not follow anymore and make it face the same way
                // as the follow
                if (NewState)
                {
                    transform.forward = follow.forward;

                    rb.isKinematic = false;
                    collider.enabled = true;
                    line.enabled = true;
                }

                // Move hook
                transform.position += hookSpeed * Time.deltaTime * transform.forward;

                // Draw line
                linePoints[0] = transform.position;
                linePoints[1] = connectedBody.position;
                line.SetPositions(linePoints);
                break;
            case HookState.Attached:
                if (NewState)
                {
                    rb.isKinematic = true;
                    collider.enabled = false;
                    line.enabled = true;
                }

                // Draw lines
                linePoints[0] = transform.position;
                linePoints[1] = connectedBody.position;
                line.SetPositions(linePoints);
                break;
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            State = HookState.Following;
        }

    }

    private void FixedUpdate()
    {
        // Pull player towards
        if (State == HookState.Attached)
        {
            Vector3 direction = transform.position - connectedBody.position;
            connectedBody.AddForce(velocityChange * direction.normalized, ForceMode.VelocityChange);
            connectedBody.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State == HookState.Shooting)
        {
            State = HookState.Attached;
        }
    }

    private enum HookState
    {
        Following,
        Shooting,
        Attached
    }
}
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Rigidbody carRb;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] public Transform accelarationPoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness; //max force our spring exerts when fully compressed
    [SerializeField] private float restLength; //the length of our spring
    [SerializeField] private float springTravel; //max travel distance of our spring
    [SerializeField] private float wheelRadius;
    [SerializeField] private float damperStiffness; //density of the damper fluid

    private int[] wheelIsGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Player Input")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float accelaration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float retardation = 50f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private float dragCoeff = 1f;
    [SerializeField] private float downforce = 100f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float oversteerFactor = 1f;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    [Header("Wheel Visuals")]
    [SerializeField] public Transform frontLeftWheel;
    [SerializeField] public Transform frontRightWheel;
    [SerializeField] public Transform rearLeftWheel;
    [SerializeField] public Transform rearRightWheel;

    #region Unity Methods

    private void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = new Vector3(0f, -0.5f, 0f);
        accelarationPoint.position = carRb.transform.TransformPoint(carRb.centerOfMass);

    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Downforce();
        UpdateTireVisuals();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    #endregion

    #region Suspension Functions
    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
            {
                wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(carRb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springCompression * springStiffness;

                float netForce = springForce - dampForce;

                carRb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelIsGrounded[i] = 0;

     

                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
            }
        }
    }

    #endregion

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i< wheelIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRb.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Input Handling

    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Movement

    private void Movement()
    {
        if (isGrounded)
        {
            if (moveInput > 0f)
            {
                if (currentCarLocalVelocity.z < maxSpeed)
                Accelaration();
            }
            else if (moveInput < 0f)
            {
                
                if (currentCarLocalVelocity.z > -maxSpeed)
                Retardation();
            }
            Turn();
            SidewaysDrag();
        }
    }

    private void Accelaration()
    {
        carRb.AddForceAtPosition(accelaration * moveInput * transform.forward, accelarationPoint.position, ForceMode.Acceleration);
    }

    private void Retardation()
    {
        carRb.AddForceAtPosition(retardation * -transform.forward, accelarationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        float steeringTorque = steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio);
        carRb.AddRelativeTorque(steeringTorque * transform.up, ForceMode.Acceleration);

        Vector3 frontAxlePos = (frontLeftWheel.position + frontRightWheel.position) * 0.5f;

        Vector3 lateralForce = steerInput * steerStrength * oversteerFactor * transform.right;
        carRb.AddForceAtPosition(lateralForce, frontAxlePos, ForceMode.Force);
    }

    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * dragCoeff;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRb.AddForceAtPosition(dragForce, carRb.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void Downforce()
    {
        float speed = carRb.linearVelocity.magnitude;
        carRb.AddForce(-transform.up * downforce * speed);
    }

    #endregion

    #region Visuals

    private void UpdateTireVisuals()
    {
        float rotationAmount = currentCarLocalVelocity.z * Time.deltaTime * 360f;
        float steerAngle = steerInput * 30f;

        //rear wheels 
        rearLeftWheel.Rotate(Vector3.right, rotationAmount, Space.Self);
        rearRightWheel.Rotate(Vector3.right, rotationAmount, Space.Self);

        //front wheels
        frontLeftWheel.Rotate(Vector3.right, rotationAmount, Space.Self);
        frontRightWheel.Rotate(Vector3.right, rotationAmount, Space.Self);

        //front steer 
        Vector3 leftEuler = frontLeftWheel.localEulerAngles;
        Vector3 rightEuler = frontRightWheel.localEulerAngles;

        frontLeftWheel.localRotation = Quaternion.Euler(leftEuler.x, steerAngle, leftEuler.z);
        frontRightWheel.localRotation = Quaternion.Euler(rightEuler.x, steerAngle, rightEuler.z);
    }

    #endregion
}

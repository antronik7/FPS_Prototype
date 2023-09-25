using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform hand;
    [SerializeField]
    private Transform gun;
    [SerializeField]
    private GameObject crosshair;
    [SerializeField] private GameObject xHit;
    [SerializeField] private RectTransform hitMark;

    [Header("Camera")]
    [SerializeField]
    private float hipFireYawSpeed = 160;
    [SerializeField]
    private int hipFirePitchSpeed = 120;
    [SerializeField]
    private int hipFireExtraYawSpeed = 220;
    [SerializeField]
    private int hipFireExtraPitchSpeed = 0;
    [SerializeField]
    private float hipFireExtraSpeedRampUpTime = 0.33f;
    [SerializeField]
    private float hipFireExtraSpeedRampUpDelay = 0f;
    [SerializeField]
    private float innerDeadZone = 0.08f;
    [SerializeField]
    private float outerDeadZone = 0.98f;
    [SerializeField]
    private AnimationCurve joystickCurve;
    [SerializeField]
    private int maxUpAngle = 90;
    [SerializeField]
    private int maxDownAngle = -90;

    [Header("ADS")]
    [SerializeField]
    private float ADSyawSpeed = 160;
    [SerializeField]
    private int ADSpitchSpeed = 120;
    [SerializeField]
    private int ADSextraYawSpeed = 220;
    [SerializeField]
    private int ADSextraPitchSpeed = 0;
    [SerializeField]
    private float ADSextraSpeedRampUpTime = 0.33f;
    [SerializeField]
    private float ADSextraSpeedRampUpDelay = 0f;
    [SerializeField]
    private float ADSTriggerDeadZone = 0.25f;
    [SerializeField]
    private float ADSDuration = 0.25f;
    [SerializeField]
    private Vector3 hipFirePosition;
    [SerializeField]
    private Vector3 ADSPosition;
    [SerializeField]
    private int hipFireFOV = 70;
    [SerializeField]
    private int ADSFOV = 35;

    [Header("Sway")]
    [SerializeField]
    private float maxSwayRotation = 5f;
    [SerializeField]
    private float maxSwayADSRotation = 0.5f;
    [SerializeField]
    private float maxSwayMagnitude = 0.25f;

    [Header("Shoot")]
    [SerializeField]
    private float shootTriggerDeadZone = 0.25f;
    [SerializeField]
    private float fireRate = 0.1f;
    [SerializeField]
    private int maxRange = 1000;
    [SerializeField] private float hipFireSpreadRadius = 1f;
    [SerializeField] private float ADSSpreadRadius = 0.5f;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private float recoilDuration = 0.1f;
    [SerializeField] private float recoilForce = 0.05f;
    [SerializeField] private float xHitDuration = 0.5f;

    [Header("Move")]
    [SerializeField] private float moveInnerDeadZone = 0.15f;
    [SerializeField] private float moveOuterDeadZone = 0.8f;
    [SerializeField] private float minMoveSpeed = 1.1111f;
    [SerializeField] private float maxMoveSpeed = 3.8889f;

    [Header("Sprint")]
    [SerializeField] private float sprintMoveSpeed = 6.1111f;
    [SerializeField] private int sprintAngle = 45;
    [SerializeField] private Vector3 sprintPosition;
    [SerializeField] private Vector3 sprintRotation;
    [SerializeField] private Vector3 hipfireRotation;
    [SerializeField] private float sprintRotationDuration = 0.5f;
    [SerializeField] private float sprintRotationSpeed = 0.5f;


    private Rigidbody rBody;
    private PlayerInputActions input = null;
    private float leftTriggerValue = 0f;
    private float rightTriggerValue = 0f;
    private Vector2 rawJoystickInputs = Vector2.zero;
    private Vector2 rawMoveJoystickInputs = Vector2.zero;
    private float camCurrentRotationX = 0f;
    private float camCurrentRotationY = 0f;
    private bool stickAtMaximum = false;
    private float extraSpeedRampUpTimer = 0f;
    private float extraSpeedDelayTimer = 0f;
    private Vector3 gunDefaultRotation;
    private float fireRateTimer = 0f;
    private float recoilTimer = 1f;
    private float xHitTimer = 0f;
    private Vector3 hitPosition = Vector3.zero;
    private bool isSprinting = false;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        input = new PlayerInputActions();
        gunDefaultRotation = hand.localEulerAngles;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Aim.performed += OnAimPerformed;
        input.Player.Aim.canceled += OnAimCanceled;
        input.Player.Move.performed += OnMovePerformed;
        input.Player.Move.canceled += OnMoveCanceled;
        input.Player.ADS.performed += OnADSPerformed;
        input.Player.ADS.canceled += OnADSCanceled;
        input.Player.Shoot.performed += OnShootPerformed;
        input.Player.Shoot.canceled += OnShootCanceled;
        input.Player.Sprint.performed += OnSprint;
        input.Player.Reset.performed += OnReset;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Aim.performed -= OnAimPerformed;
        input.Player.Aim.canceled -= OnAimCanceled;
        input.Player.Move.performed -= OnMovePerformed;
        input.Player.Move.canceled -= OnMoveCanceled;
        input.Player.ADS.performed -= OnADSPerformed;
        input.Player.ADS.canceled -= OnADSCanceled;
        input.Player.Shoot.performed -= OnShootPerformed;
        input.Player.Shoot.canceled -= OnShootCanceled;
        input.Player.Sprint.performed -= OnSprint;
        input.Player.Reset.performed -= OnReset;
    }

    private void OnAimPerformed(InputAction.CallbackContext value)
    {
        rawJoystickInputs = value.ReadValue<Vector2>();
    }

    private void OnAimCanceled(InputAction.CallbackContext value)
    {
        rawJoystickInputs = Vector2.zero;
    }

    private void OnMovePerformed(InputAction.CallbackContext value)
    {
        rawMoveJoystickInputs = value.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext value)
    {
        rawMoveJoystickInputs = Vector2.zero;
    }

    private void OnADSPerformed(InputAction.CallbackContext value)
    {
        leftTriggerValue = value.ReadValue<float>();
    }

    private void OnADSCanceled(InputAction.CallbackContext value)
    {
        leftTriggerValue = 0f;
        extraSpeedDelayTimer = 0f;// Maybe should create a function for that...
        extraSpeedRampUpTimer = 0f;
    }

    private void OnShootPerformed(InputAction.CallbackContext value)
    {
        rightTriggerValue = value.ReadValue<float>();
    }

    private void OnShootCanceled(InputAction.CallbackContext value)
    {
        rightTriggerValue = 0f;
    }

    private void OnSprint(InputAction.CallbackContext value)
    {
        StartSprint();
    }

    private void OnReset(InputAction.CallbackContext value)
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AimDownSight();
        RotateCamera();
        Recoil();
        Shoot();
        Sprint();
        Move();
    }

    private void AimDownSight()
    {
        if (isSprinting)
            return;

        Vector3 velocity = Vector3.zero;
        float FOVvelocity = 0;

        if (leftTriggerValue < ADSTriggerDeadZone)
        {
            crosshair.SetActive(true);
            hand.localPosition = Vector3.SmoothDamp(hand.localPosition, hipFirePosition, ref velocity, ADSDuration);
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, hipFireFOV, ref FOVvelocity, ADSDuration);
        }
        else
        {
            isSprinting = false;
            crosshair.SetActive(false);
            hand.localPosition = Vector3.SmoothDamp(hand.localPosition, ADSPosition, ref velocity, ADSDuration);
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, ADSFOV, ref FOVvelocity, ADSDuration);
        }
    }

    private void RotateCamera()
    {
        //float joystickHorixontalAxis = Input.GetAxis("CamHorizontal");
        //float joystickVerticalAxis = Input.GetAxis("CamVertical");

        //Vector2 joystickInputs = new Vector2(joystickHorixontalAxis, joystickVerticalAxis); 

        Vector2 joystickInputs = rawJoystickInputs;
        joystickInputs = ApplyDeadZone(joystickInputs, innerDeadZone, outerDeadZone);
        joystickInputs = ApplyJoystickCurve(joystickInputs);

        float currentYawSpeed;
        float currentPitchSpeed;
        int currentExtraYawSpeed;
        int currentExtraPitchSpeed;
        float currentExtraSpeedRampUpTime;
        float currentExtraSpeedRampUpDelay;

        if (leftTriggerValue < ADSTriggerDeadZone)
        {
            currentYawSpeed = hipFireYawSpeed;
            currentPitchSpeed = hipFirePitchSpeed;
            currentExtraYawSpeed = hipFireExtraYawSpeed;
            currentExtraPitchSpeed = hipFireExtraPitchSpeed;
            currentExtraSpeedRampUpTime = hipFireExtraSpeedRampUpTime;
            currentExtraSpeedRampUpDelay = hipFireExtraSpeedRampUpDelay;
        }
        else
        {
            currentYawSpeed = ADSyawSpeed ;
            currentPitchSpeed = ADSpitchSpeed;
            currentExtraYawSpeed = ADSextraYawSpeed;
            currentExtraPitchSpeed = ADSextraPitchSpeed;
            currentExtraSpeedRampUpTime = ADSextraSpeedRampUpTime;
            currentExtraSpeedRampUpDelay = ADSextraSpeedRampUpDelay;
        }

        if (stickAtMaximum)
        {
            if (joystickInputs.magnitude < 0.9999f)
            {
                stickAtMaximum = false;
                extraSpeedDelayTimer = 0f;
                extraSpeedRampUpTimer = 0f;
            }
            else
            {
                extraSpeedDelayTimer += Time.deltaTime;

                if (extraSpeedDelayTimer >= currentExtraSpeedRampUpDelay)
                {
                    if (extraSpeedRampUpTimer == 0f)
                        extraSpeedRampUpTimer += extraSpeedDelayTimer - currentExtraSpeedRampUpDelay;
                    else
                        extraSpeedRampUpTimer += Time.deltaTime;
                }
            }
        }
        else if (joystickInputs.magnitude >= 0.9999f)
        {
            stickAtMaximum = true;
        }

        if(stickAtMaximum && extraSpeedDelayTimer >= currentExtraSpeedRampUpDelay)
        {
            float rampUpPercent = extraSpeedRampUpTimer / currentExtraSpeedRampUpTime;
            rampUpPercent = Mathf.Clamp01(rampUpPercent);

            currentYawSpeed += currentExtraYawSpeed * rampUpPercent;
            currentPitchSpeed += currentExtraPitchSpeed * rampUpPercent;
        }

        if (leftTriggerValue < ADSTriggerDeadZone)
        {

        }
        else
        {
            currentYawSpeed *= ((float)ADSFOV / hipFireFOV);
            currentPitchSpeed *= ((float)ADSFOV / hipFireFOV);
        }

        camCurrentRotationY += joystickInputs.x * currentYawSpeed * Time.deltaTime;
        camCurrentRotationX += joystickInputs.y * currentPitchSpeed * Time.deltaTime;
        camCurrentRotationX = Mathf.Clamp(camCurrentRotationX, maxDownAngle, maxUpAngle);

        Camera.main.transform.rotation = Quaternion.Euler(camCurrentRotationX, camCurrentRotationY, 0);

        SwayGun(joystickInputs);
    }

    private void SwayGun(Vector2 joystickInputs)
    {
        float swayrotation;

        if (leftTriggerValue < ADSTriggerDeadZone)
            swayrotation = maxSwayRotation;
        else
            swayrotation = maxSwayADSRotation;

        float newAngleX = gunDefaultRotation.x + Mathf.Clamp(swayrotation * (joystickInputs.y / maxSwayMagnitude), -swayrotation, swayrotation);
        float newAngleY = gunDefaultRotation.y + Mathf.Clamp(swayrotation * (joystickInputs.x / maxSwayMagnitude), -swayrotation, swayrotation);

        hand.localEulerAngles = new Vector3(newAngleX, newAngleY, hand.localEulerAngles.z);
    }

    private void Recoil()
    {
        recoilTimer += Time.deltaTime;
        float t = recoilTimer / recoilDuration;
        float recoilValue = recoilCurve.Evaluate(t);
        recoilValue *= recoilForce * -1f;
        gun.localPosition = new Vector3(gun.localPosition.x, gun.localPosition.y, 0f + recoilValue);
    }

    private void Shoot()
    {
        xHitTimer -= Time.deltaTime;
        if (xHitTimer <= 0f)
            xHit.SetActive(false);

        hitMark.anchoredPosition = Camera.main.WorldToScreenPoint(hitPosition);

        fireRateTimer -= Time.deltaTime;
        if(rightTriggerValue >= shootTriggerDeadZone && fireRateTimer <= 0)
        {
            fireRateTimer = fireRate;
            recoilTimer = 0f;

            float spreadRadius;
            if (leftTriggerValue < ADSTriggerDeadZone)
                spreadRadius = hipFireSpreadRadius;
            else
                spreadRadius = ADSSpreadRadius;

            Vector2 randomSpread = Random.insideUnitCircle * spreadRadius;
            Vector3 shotDirection = Camera.main.transform.forward + (Camera.main.transform.right * randomSpread.x) + (Camera.main.transform.up * randomSpread.y);

            RaycastHit hit;
            if(Physics.Raycast(Camera.main.transform.position, shotDirection, out hit, maxRange))
            {
                hitMark.gameObject.SetActive(true);
                hitPosition = hit.point;
                if(hit.transform.root.GetComponent<TargetController>())
                {
                    xHit.SetActive(true);
                    xHitTimer = xHitDuration;

                    hit.transform.root.GetComponent<TargetController>().Hit();
                }
            }
            else
            {
                xHit.SetActive(false);
                hitMark.gameObject.SetActive(false);
            }
        }
    }

    private void Move()//Add curve for the move stick...
    {
        Vector2 joystickInputs = rawMoveJoystickInputs;
        joystickInputs = ApplyDeadZone(joystickInputs, moveInnerDeadZone, moveOuterDeadZone);

        float joystickMagnitude = joystickInputs.magnitude;
        if (joystickMagnitude < moveInnerDeadZone)
        {
            rBody.velocity = Vector3.zero;
            return;
        }

        float stickAngle = Vector2.Angle(joystickInputs, Vector2.up) * Mathf.Sign(joystickInputs.x);
        Vector3 cameraForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        cameraForward.Normalize();

        float speed = minMoveSpeed + ((maxMoveSpeed - minMoveSpeed) * joystickInputs.magnitude);

        if (isSprinting)
        {
            speed = sprintMoveSpeed;
        }
        else if (leftTriggerValue < ADSTriggerDeadZone)
        {

        }
        else
        {
            speed *= ((float)ADSFOV / hipFireFOV);
        }

        Vector3 velocity = (Quaternion.Euler(0, stickAngle, 0) * cameraForward) * speed;
        rBody.velocity = velocity;
    }

    private void StartSprint()
    {
        if (isSprinting == true)
            return;

        Vector2 joystickInputs = rawMoveJoystickInputs;
        float joystickMagnitude = joystickInputs.magnitude;
        if (joystickMagnitude < moveOuterDeadZone)
            return;

        float stickAngle = Mathf.Abs(Mathf.Atan2(joystickInputs.x, joystickInputs.y) * Mathf.Rad2Deg);
        if (stickAngle > sprintAngle)
            return;

        isSprinting = true;
    }

    private void Sprint()
    {
        Vector3 velocity = Vector3.zero;

        if (isSprinting)
        {
            Vector2 joystickInputs = rawMoveJoystickInputs;
            float joystickMagnitude = joystickInputs.magnitude;
            if (joystickMagnitude < moveOuterDeadZone)
            {
                isSprinting = false;
                return;
            }

            float stickAngle = Mathf.Abs(Mathf.Atan2(joystickInputs.x, joystickInputs.y) * Mathf.Rad2Deg);
            if (stickAngle > sprintAngle)
            {
                isSprinting = false;
                return;
            }

            hand.localPosition = Vector3.SmoothDamp(hand.localPosition, sprintPosition, ref velocity, sprintRotationDuration);
            gun.localRotation = Quaternion.RotateTowards(gun.localRotation, Quaternion.Euler(sprintRotation), sprintRotationSpeed * Time.deltaTime);
        }
        else
        {
            gun.localRotation = Quaternion.RotateTowards(gun.localRotation, Quaternion.Euler(hipfireRotation), sprintRotationSpeed * Time.deltaTime);
        }
    }

    private Vector2 ApplyDeadZone(Vector2 joystickInputs, float innerDeadZone = 0.19f, float outerDeadZone = 1f)
    {
        float inputsMagnitude = joystickInputs.magnitude;

        if (inputsMagnitude < innerDeadZone)
            return Vector2.zero;
        if (inputsMagnitude > outerDeadZone)
            return joystickInputs.normalized;

        joystickInputs = joystickInputs.normalized * ((inputsMagnitude - innerDeadZone) / (outerDeadZone - innerDeadZone));

        return joystickInputs;
    }

    private Vector2 ApplyJoystickCurve(Vector2 joystickInputs)
    {
        float inputsMagnitude = joystickInputs.magnitude;

        float convertedMagnitude = joystickCurve.Evaluate(inputsMagnitude);

        return joystickInputs.normalized * convertedMagnitude;
    }
}

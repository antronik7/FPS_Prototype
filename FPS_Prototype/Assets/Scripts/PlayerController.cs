using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Transform gun;
    [SerializeField]
    private GameObject crosshair;

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
    [SerializeField]
    private float maxSwayRotation = 5f;
    [SerializeField]
    private float maxSwayADSRotation = 0.5f;
    [SerializeField]
    private float maxSwayMagnitude = 0.25f;

    private PlayerInputActions input = null;
    private float leftTriggerValue = 0f;
    private Vector2 rawJoystickInputs = Vector2.zero;
    private float camCurrentRotationX = 0f;
    private float camCurrentRotationY = 0f;
    private bool stickAtMaximum = false;
    private float extraSpeedRampUpTimer = 0f;
    private float extraSpeedDelayTimer = 0f;
    private Vector3 gunDefaultRotation;

    private void Awake()
    {
        input = new PlayerInputActions();
        gunDefaultRotation = gun.localEulerAngles;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Aim.performed += OnAimPerformed;
        input.Player.Aim.canceled += OnAimCanceled;
        input.Player.ADS.performed += OnADSPerformed;
        input.Player.ADS.canceled += OnADSCanceled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Aim.performed -= OnAimPerformed;
        input.Player.Aim.canceled -= OnAimCanceled;
        input.Player.ADS.performed -= OnADSPerformed;
        input.Player.ADS.canceled -= OnADSCanceled;
    }

    private void OnAimPerformed(InputAction.CallbackContext value)
    {
        rawJoystickInputs = value.ReadValue<Vector2>();
    }

    private void OnAimCanceled(InputAction.CallbackContext value)
    {
        rawJoystickInputs = Vector2.zero;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AimDownSight();
        RotateCamera();
    }

    private void AimDownSight()
    {
        Vector3 velocity = Vector3.zero;
        float FOVvelocity = 0;

        if (leftTriggerValue < ADSTriggerDeadZone)
        {
            crosshair.SetActive(true);
            gun.localPosition = Vector3.SmoothDamp(gun.localPosition, hipFirePosition, ref velocity, ADSDuration);
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, hipFireFOV, ref FOVvelocity, ADSDuration);
        }
        else
        {
            crosshair.SetActive(false);
            gun.localPosition = Vector3.SmoothDamp(gun.localPosition, ADSPosition, ref velocity, ADSDuration);
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

        float newAngleX = gunDefaultRotation.x + Mathf.Clamp(-swayrotation * (joystickInputs.y / maxSwayMagnitude), -swayrotation, swayrotation);
        float newAngleY = gunDefaultRotation.y + Mathf.Clamp(swayrotation * (joystickInputs.x / maxSwayMagnitude), -swayrotation, swayrotation);

        gun.localEulerAngles = new Vector3(newAngleX, newAngleY, gun.localEulerAngles.z);
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

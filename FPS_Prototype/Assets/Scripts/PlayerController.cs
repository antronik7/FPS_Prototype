using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private int yawSpeed = 160;
    [SerializeField]
    private int pitchSpeed = 120;
    [SerializeField]
    private float innerDeadZone = 0.08f;
    [SerializeField]
    private float outerDeadZone = 0.98f;
    [SerializeField]
    private AnimationCurve joystickCurve;
    [SerializeField]
    private int extraYawSpeed = 220;
    [SerializeField]
    private int extraPitchSpeed = 0;
    [SerializeField]
    private float extraSpeedRampUpTime = 0.33f;
    [SerializeField]
    private float extraSpeedRampUpDelay = 0f;

    [SerializeField]
    private int maxUpAngle = 90;
    [SerializeField]
    private int maxDownAngle = -90;

    private float camCurrentRotationX = 0f;
    private float camCurrentRotationY = 0f;
    private bool stickAtMaximum = false;
    private float extraSpeedRampUpTimer = 0f;
    private float extraSpeedDelayTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        float joystickHorixontalAxis = Input.GetAxis("CamHorizontal");
        float joystickVerticalAxis = Input.GetAxis("CamVertical");

        Vector2 joystickInputs = new Vector2(joystickHorixontalAxis, joystickVerticalAxis);
        joystickInputs = ApplyDeadZone(joystickInputs, innerDeadZone, outerDeadZone);
        joystickInputs = ApplyJoystickCurve(joystickInputs);

        float currentYawSpeed = yawSpeed;
        float currentPitchSpeed = pitchSpeed;

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

                if (extraSpeedDelayTimer >= extraSpeedRampUpDelay)
                {
                    if (extraSpeedRampUpTimer == 0f)
                        extraSpeedRampUpTimer += extraSpeedDelayTimer - extraSpeedRampUpDelay;
                    else
                        extraSpeedRampUpTimer += Time.deltaTime;
                }
            }
        }
        else if (joystickInputs.magnitude >= 0.9999f)
        {
            stickAtMaximum = true;
        }

        if(stickAtMaximum && extraSpeedDelayTimer >= extraSpeedRampUpDelay)
        {
            float rampUpPercent = extraSpeedRampUpTimer / extraSpeedRampUpTime;
            rampUpPercent = Mathf.Clamp01(rampUpPercent);

            currentYawSpeed += extraYawSpeed * rampUpPercent;
            currentPitchSpeed += extraPitchSpeed * rampUpPercent;
        }


        camCurrentRotationY += joystickInputs.y * currentYawSpeed * Time.deltaTime;
        camCurrentRotationX += joystickInputs.x * currentPitchSpeed * Time.deltaTime;
        camCurrentRotationX = Mathf.Clamp(camCurrentRotationX, maxDownAngle, maxUpAngle);

        Camera.main.transform.rotation = Quaternion.Euler(camCurrentRotationX, camCurrentRotationY, 0);
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

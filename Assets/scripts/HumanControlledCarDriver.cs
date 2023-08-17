using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanControlledCarDriver : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform backRightTransform;
    [SerializeField] Transform backLeftTransform;

    public float acceleration = 500f;
    public float breakingForce = 300f;
    public float maxTurnAngle = 15f;

    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;

    private void FixedUpdate()
    {
        //calculate current acceleration
        currentAcceleration = acceleration * -Input.GetAxis("Vertical");

        //Set breakforce when breaking
        if (Input.GetKey(KeyCode.Space))
            currentBreakForce = breakingForce;
        else
            currentBreakForce = 0f;

        //acceleration!!
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBreakForce;
        frontLeft.brakeTorque = currentBreakForce;
        backRight.brakeTorque = currentBreakForce;
        backLeft.brakeTorque = currentBreakForce;

        //steering whoo
        currentTurnAngle = maxTurnAngle * Input.GetAxis("Horizontal");
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        //update wheels position
        TurnWheels(frontLeft, frontLeftTransform);
        TurnWheels(frontRight, frontRightTransform);
        TurnWheels(backLeft, backLeftTransform);
        TurnWheels(backRight, backRightTransform);


    }

    void TurnWheels(WheelCollider col, Transform trans)
    {
        //get current state
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);

        //change state
        trans.position = position;
        trans.rotation = rotation;

    }


}

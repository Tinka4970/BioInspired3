using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AIControlledCarDriver : Agent
{
    //oooooooooooooooooooooooooooooooooooooooooooooooooo OVERRIDES BEGIN ooooooooooooooooooooooooooooooooooooooooooooooooo
    public override void Initialize()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //GATHER ALL OBSERVATIONS FED INTO THE NN
        //distance to parking
        sensor.AddObservation(new Vector2((deltax * Mathf.Cos(Mathf.PI / 180 * carRotation.y) - deltaz * Mathf.Sin(Mathf.PI / 180 * carRotation.y)), (deltax * Mathf.Sin(Mathf.PI / 180 * carRotation.y) + deltaz * Mathf.Cos(Mathf.PI / 180 * carRotation.y))));

        //velocity positive for forward, negative for backwards
        velocity = carRigidBody.velocity;
        sensor.AddObservation(new Vector2((velocity.x * Mathf.Cos(Mathf.PI / 180 * carRotation.y) - velocity.z * Mathf.Sin(Mathf.PI / 180 * carRotation.y)), (velocity.x * Mathf.Sin(Mathf.PI / 180 * carRotation.y) + velocity.z * Mathf.Cos(Mathf.PI / 180 * carRotation.y))));
    
        //difference in rotation between car and parking space
        sensor.AddObservation(1 - (Mathf.Abs(gameObject.transform.rotation.eulerAngles.y % 180 - 90) / 90));

        sensor.AddObservation(gameObject.transform.rotation.eulerAngles.y - 180 - trailer.transform.rotation.eulerAngles.y);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        //pull actions from descisions;
        leftRightAction = actions.ContinuousActions[0];
        backForthAction = actions.ContinuousActions[1];
        brakeAction = actions.DiscreteActions[0];

        //actually move the car
        SteerCar(leftRightAction,  backForthAction,  brakeAction);;

        //check if the car is parked
        CheckIfParked();
        
        //calculate currentdistancetoparking
        CalculateReward();
  

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionOut = actionsOut.DiscreteActions;
        ActionSegment<float> continuousActionOut = actionsOut.ContinuousActions;

        //discrete actions -> braking
        if (Input.GetKey(KeyCode.Space)){
            discreteActionOut[0] = 1;
        } else
        {
            discreteActionOut[0] = 0;
        }

        //continuous actions -> steering
        continuousActionOut[0] = Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }
    public override void OnEpisodeBegin()
    {
        //Make sure the car starts from a standstill position
        currentAcceleration = 0;
        currentBreakForce = 0;
        currentTurnAngle = 0;
        frontRight.motorTorque = 0;
        frontLeft.motorTorque = 0;
        timePunishment = (float)-0.01;
        currentOrientation = 0;
        carRotation = new Vector3(0,180,0);
        carRigidBody = gameObject.GetComponent<Rigidbody>();
        carRigidBody.velocity = new Vector3(0, 0, 0);

        //randomize location of car and target
        
        RandomLocationGenerator();
        targetPosition = parkingSpace.transform.position;
        carPosition = gameObject.transform.position;


        //random obstacle placement
        RandomObstaclePlacement(obstacle1);
        while (Mathf.Abs(zValueObstacle - zValueTarget) <= 1)
        {
            RandomObstaclePlacement(obstacle1);
        }
        RandomObstaclePlacement(obstacle2);
        while (Mathf.Abs(zValueObstacle - zValueTarget) <= 1)
        {
            RandomObstaclePlacement(obstacle2);
        }
        RandomObstaclePlacement(obstacle3);
        while (Mathf.Abs(zValueObstacle - zValueTarget) <= 1)
        {
            RandomObstaclePlacement(obstacle3);
        }
        RandomObstaclePlacement(obstacle4);
        while (Mathf.Abs(zValueObstacle - zValueTarget) <= 1)
        {
            RandomObstaclePlacement(obstacle4);
        }

        previousDistanceToParking = Vector3.Distance(carPosition, targetPosition);
        previousRotation = 0;
        currentDistanceToParking = Vector3.Distance(carPosition, targetPosition);
        beginDistance = Vector3.Distance(carPosition, targetPosition);
    }
    

    //oooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo INTRUDUCING ALL VARIABLES oooooooooooooooooooooooooooooooooooooooo
    public GameObject parkingSpace;
    Rigidbody carRigidBody;
    public GameObject obstacle1;
    public GameObject obstacle2;
    public GameObject obstacle3;
    public GameObject obstacle4;
    public GameObject trailer;

    public TrailerScript trailerscript;

    //add wheel colliders and transforms so that we can make em move
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform backRightTransform;
    [SerializeField] Transform backLeftTransform;

    //public variables
    public float acceleration = 500f;
    public float breakingForce = 300f;
    public float maxTurnAngle = 15f;
    public int amountOfObstacles = 4;

    //private variables
    private float currentAcceleration;
    private float currentBreakForce;
    private float currentTurnAngle;
    private float currentDistanceToParking;
    private float previousDistanceToParking;
    private float beginDistance;
    private float leftRightAction;
    private float backForthAction;
    private int brakeAction;
    private float xValueTarget;
    private float yValueTarget;
    private int zValueTarget;
    private float xValueCar;
    private float yValueCar;
    private float zValueCar;
    private float xValueObstacle;
    private float yValueObstacle;
    private int zValueObstacle;
    private Vector3 carPosition;
    private Vector3 carRotation;
    private Vector3 targetPosition;
    private Vector3 velocity;
    private float deltax;
    private float deltaz;
    private float currentOrientation;
    private float previousRotation;
    private float timePunishment;

    //ooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo  FUNCTIONS oooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo

    private void SteerCar(float horizontalInput, float verticalInput, int braking) //function that is called ones descisions are made
    {
        //calculate current acceleration
        currentAcceleration = 2 * acceleration * verticalInput;
        previousDistanceToParking = Vector3.Distance(targetPosition, carPosition);
        previousRotation = 1 - (Mathf.Abs(carRotation.y % 180 - 90) / 90);


        //Set breakforce when breaking
        if (braking == 1)
            currentBreakForce = 2 * breakingForce;
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
        currentTurnAngle = maxTurnAngle * horizontalInput;
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        //update wheels position
        TurnWheels(frontLeft, frontLeftTransform);
        TurnWheels(frontRight, frontRightTransform);
        TurnWheels(backLeft, backLeftTransform);
        TurnWheels(backRight, backRightTransform);

        carPosition = gameObject.transform.position;
        carRotation = gameObject.transform.rotation.eulerAngles;
    }

    void TurnWheels(WheelCollider col, Transform trans) //function to make the wheels look like they're actually turning
    {
        //get current state
        col.GetWorldPose(out Vector3 position, out Quaternion rotation);

        //change state
        trans.SetPositionAndRotation(position, rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.transform.CompareTag("Obstacle"))
        {
            AddReward(-5);
        }

        if (collision.gameObject.transform.CompareTag("Wall"))
        {
            AddReward(-5);
        }
    }

    private void RandomLocationGenerator()
    {
        //random location of the parking place
        zValueTarget = Random.Range(-20 ,10);
        yValueTarget = (float)0.2;
        if (Random.value < 0.5f)
            xValueTarget = (float)13.5;
        else
            xValueTarget = (float)-13.5;

        parkingSpace.transform.position = new Vector3(xValueTarget, yValueTarget, zValueTarget);

        //determining location of the car
        xValueCar = 0;
        yValueCar = (float)0.2;
        zValueCar = (float)29.5;

        gameObject.transform.SetPositionAndRotation(new Vector3(xValueCar, yValueCar, zValueCar), Quaternion.Euler(new Vector3(0, 180, 0)));
        trailer.transform.SetPositionAndRotation(new Vector3(0, (float)0.9, (float)33.75), new Quaternion(0, 0, 0, 0));
    }

    private void RandomObstaclePlacement(GameObject obstacle)
    {
        //random location of obstacle but have it be different from the parking place
        zValueObstacle = Random.Range(-20, 12);
        yValueObstacle = (float)0.67;
        if (Random.value < 0.5f)
            xValueObstacle = (float)13.5;
        else
            xValueObstacle = (float)-13.5;

        obstacle.transform.SetPositionAndRotation(new Vector3(xValueObstacle, yValueObstacle, zValueObstacle), Quaternion.Euler(new Vector3(0, 90, 0)));

    }
    public void CalculateReward()
    {
        if (trailerscript.trailerColliding)
        {
            print("trailer has collided");
            AddReward(-5);
            trailerscript.trailerColliding = false;
        }

        //time penalty, max at 1000 steps = -10
        AddReward(timePunishment);

        //distance reward for getting closer, minus for running away. Max at end of run = 10
        currentDistanceToParking = Vector3.Distance(targetPosition, carPosition);
        AddReward((previousDistanceToParking - currentDistanceToParking) / beginDistance * 10);

        //reward for the correct orientation
        deltax = targetPosition.x - carPosition.x;
        deltaz = targetPosition.z - carPosition.z;
        currentOrientation = 1 - (Mathf.Abs(carRotation.y % 180 - 90) / 90);

        AddReward((currentOrientation - previousRotation) * 10);


    }

    public bool CheckIfParked()
    {
        if ((Mathf.Abs(carPosition.x - targetPosition.x) <= 0.2) && (Mathf.Abs(carPosition.z - targetPosition.z) <= 0.3) && (currentOrientation > 0.85) && (Vector3.Distance(velocity,Vector3.zero) < 0.1))
        {
            AddReward(10 - (Mathf.Abs((gameObject.transform.rotation.eulerAngles.y - 180) - (trailer.transform.rotation.eulerAngles.y)) / 18));


            AddReward(10);
            AddReward(10 * (1 - (Mathf.Abs((carRotation.y - trailer.transform.rotation.y) % 180 - 90) / 90)));
            timePunishment = 0;
            EndEpisode();
            return true;
        } else
        {
            return false;
        }
    }
}

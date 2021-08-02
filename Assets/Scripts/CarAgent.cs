﻿using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class CarAgent : Agent
{
    [Header("Car Agent Settings")]
    public float maxIdleTime = 10;
    public bool monitorInfo = false;

    private CarController car;
    private Engine carEngine;
    private Rigidbody agentRigidbody;
    private TrainingArea trainingArea;
    private int checkpointsPassed = 0;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private float idleMeter;
    private float maxReward = 0;

    private bool respawned = false;

    [HideInInspector] Vector3 localVelocity;
    [HideInInspector] public float speed;

    //performance variables
    [Header("Timers and Performance")]
    public int lapsCompleted;
    public float lapTime;
    public float prevLapTime;
    public float bestLap;
    public int mostLaps;
    //public float totalTime;
    private float spawnTime;

    private bool dejaVu;

    public override void Initialize()
    {
        base.Initialize();

        gameObject.layer = 8;
        car = GetComponent<CarController>();
        carEngine = GetComponent<Engine>();
        agentRigidbody = GetComponent<Rigidbody>();
        trainingArea = transform.parent.GetComponentInParent<TrainingArea>();

        startingPosition = transform.localPosition;
        startingRotation = transform.rotation;

        agentRigidbody.centerOfMass = GameObject.Find("CenterOfMass").transform.localPosition;
        idleMeter = maxIdleTime;
        car.SetGear(1);
        
    }

    public void Start()
    {
        //Debug.Log("Agent Initialized.");

    }

    public void FixedUpdate()
    {
        //brake and freeze position if freshly respawned
        if (respawned)
        {
            car.HandBrake();
            agentRigidbody.isKinematic = true;
            respawned = false;
        }
        else
        {
            agentRigidbody.isKinematic = false;
        }

        //determine slip (temp)
        float driftAngle = Vector3.Angle(agentRigidbody.velocity, transform.forward);
        if (driftAngle > 5f)
        {
            dejaVu = true;
        }
        else
        {
            dejaVu = false;
        }
        /*
        if (monitorInfo)
        {
            if (maxReward < GetCumulativeReward())
            {
                maxReward = GetCumulativeReward();
            }
            trainingArea.UpdateMonitor("Reward: " + GetCumulativeReward().ToString("0.00") +
                        "\nMax Reward: " + maxReward.ToString("0.00") +
                        "\nSpeed: " + speed.ToString("0.00") +
                        "\nTorque: " + car.carEngine.Torque.ToString("0") +
                        "\nRPM: " + car.carEngine.Rpm.ToString("0") +
                        "\nTireRPM: " + car.frontDriverW.rpm.ToString("0") +
                        //"\nDrift Value: " + driftValue + //
                        "\nDrift Angle: " + driftAngle.ToString("0.0") + //
                        "\nGas: " + car.GasPedalPosition.ToString("P") +
                        "\nBrake: " + car.BrakePedalPosition.ToString("P") +
                        "\nSteering Angle: " + car.SteeringAngle.ToString("0.00") +
                        "\nGear: " + car.GetGear() +
                        "\nLaps: " + (int)(checkpointsPassed / trainingArea.totalCheckpoints) +
                        "\nCheckpoints Passed: " + checkpointsPassed +
                        "\nStep Count: " + StepCount);
        }
        */
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //car vision
        //handled by the RayPerceptionSensor component

        //car velocity
        localVelocity = transform.InverseTransformDirection(agentRigidbody.velocity);
        sensor.AddObservation(localVelocity.magnitude);

        //car rotation
        //sensor.AddObservation(transform.localRotation);

        //engine state
        //sensor.AddObservation(car.GetGear());
        //sensor.AddObservation(carEngine.Rpm);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //set steering amount
        car.WheelPosition = actionBuffers.ContinuousActions[1];

        //set gear
        //car.SetGear(actionBuffers.DiscreteActions[0]);
        //todo: mask the action of setting the same gear again

        //determine acceleration and brake
        if (actionBuffers.ContinuousActions[0] < 0)
        {
            car.GasPedalPosition = 0f;
            car.BrakePedalPosition = -actionBuffers.ContinuousActions[0];
        }
        else
        {
            car.BrakePedalPosition = 0f;
            car.GasPedalPosition = actionBuffers.ContinuousActions[0];
        }

        //reward forward speed to encourage movement
        //speed = Mathf.Sqrt(Mathf.Pow(localVelocity.x, 2) + Mathf.Pow(localVelocity.z, 2));
        speed = localVelocity.magnitude;
        AddReward(speed * trainingArea.speedReward);

        //Give up and reset if agent stays idle for too long
        if (idleMeter <= 0)
        {
            AddReward(trainingArea.idlePenalty);
            EndEpisode();
        }
        else
        {
            //reduce idle meter every step
            idleMeter -= 0.01f;
        }
    }

    public override void OnEpisodeBegin()
    {
        agentRigidbody.velocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
        transform.localPosition = startingPosition;
        transform.rotation = startingRotation;
        idleMeter = maxIdleTime;
        respawned = true; //flag as "just respawned" to help reset speed and position

        spawnTime = Time.time;
        prevLapTime = 0f;
        checkpointsPassed = 0;
        lapsCompleted = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            //Give up and reset
            SetReward(trainingArea.collisionPenalty);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        //check if a checkpoint is passed
        if (col.gameObject.CompareTag("checkpoint"))
        {
            //check if agent passed the correct checkpoint (ie. didnt go backwards)
            int checkpointNumber = col.gameObject.GetComponent<Checkpoint>().checkpointNumber;

            //if (checkpointsPassed % trainingArea.totalCheckpoints == checkpointNumber - 1)
            if (true)
            {
                checkpointsPassed++;
                /************disabled for infinite laps********************
                //check it's the last checkpoint(ie the finish line)
                if (checkpointsPassed == trainingArea.totalCheckpoints)
                {
                    AddReward(trainingArea.successReward);
                    Debug.Log("Success!");
                    Done();
                }
                else
                {
                    AddReward(trainingArea.checkpointReward);
                    idleMeter = maxIdleTime;
                }
                **********************************************************/
                AddReward(trainingArea.checkpointReward);
                idleMeter = maxIdleTime;
            }
            else
            {
                //Give up and reset if agent went backwards
                //SetReward(trainingArea.wrongCheckpointPenalty);
                EndEpisode();
            }
        }

        //passing the finish line
        if (col.gameObject.CompareTag("finish"))
        {
            lapTime = Time.time - prevLapTime;
            prevLapTime = lapTime;
            if(bestLap > lapTime)
            {
                bestLap = lapTime;
                trainingArea.UpdateStats(bestLap);

            }
            lapsCompleted = lapsCompleted + 1;
        }
    }

    //statistics tracker
    public void StatTracker()
    {
        //probably useless
    }

    //select "Heuristic Only" in Behaviour Parameters to control agent manually
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        if (Input.GetKey("1"))
        {
            discreteActionsOut[0] = 1;
            Debug.Log("Setting gear: " + discreteActionsOut[0]);
        }
        else if (Input.GetKey("2"))
        {
            discreteActionsOut[0] = 2;
            Debug.Log("Setting gear: " + discreteActionsOut[0]);
        }
        else if (Input.GetKey("3"))
        {
            discreteActionsOut[0] = 3;
            Debug.Log("Setting gear: " + discreteActionsOut[0]);
        }
        else if (Input.GetKey("4"))
        {
            discreteActionsOut[0] = 4;
            Debug.Log("Setting gear: " + discreteActionsOut[0]);
        }
        else if (Input.GetKey("0"))
        {
            discreteActionsOut[0] = 0;
            Debug.Log("Setting gear: " + discreteActionsOut[0]);
        }
    }
}

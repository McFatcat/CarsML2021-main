﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{

    private GameObject[] objectsToFollow;
    private Transform objectToFollow;
    private CarAgent carAgent;
    public Vector3 offset;
    public float followSpeed = 10;
    public float lookSpeed = 10;

    private bool isHeuristicEnabled;
    private int currentObjectToFollow;


    public void Start()
    {
        isHeuristicEnabled = GameObject.Find("TrainingArea").GetComponent<TrainingArea>().heuristic;
        if (!isHeuristicEnabled)
        {
            objectsToFollow = GameObject.FindGameObjectsWithTag("Player");
            objectToFollow = objectsToFollow[0].transform;
            objectToFollow.GetComponent<CarAgent>().monitorInfo = true;
            currentObjectToFollow = 0;
            
        }
        else
        {
            objectToFollow = GameObject.Find("CarAgentHeuristic(Clone)").transform;
        }
        
      
    }

    public void LookAtTarget()
    {
        Vector3 _lookDirection = objectToFollow.position - transform.position;
        Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

    }

    public void MoveToTarget()
    {
        Vector3 _targetPos = objectToFollow.position +
                             objectToFollow.forward * offset.z +
                             objectToFollow.right * offset.x +
                             objectToFollow.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
    }

    // changes camera views (currently between top down and follow camera (random agent)
    public void ToggleView()
    {
        objectToFollow.GetComponent<CarAgent>().monitorInfo = false;
        int nextObject = (currentObjectToFollow + 1) % objectsToFollow.Length;
        Debug.Log(currentObjectToFollow + " + 1 %" + objectsToFollow.Length + " = " + nextObject);
        objectToFollow = objectsToFollow[nextObject].transform;
        objectToFollow.GetComponent<CarAgent>().monitorInfo = true;
        currentObjectToFollow = nextObject;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyUp(KeyCode.F4))
        {
            ToggleView();
           
        }

        LookAtTarget();
        MoveToTarget();
    }

}

﻿using UnityEngine;
using UnityEngine.UI;
using System;  
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using TMPro;
using System.Text.RegularExpressions;

public class TrainingArea : MonoBehaviour
{
    [Header("Training Area Settings")]
    public CarAgent agentPrefab;
    public bool spawnAgents = false;
    public int agentAmount;
    public int totalCheckpoints;
    public Transform finishLine;

    [Header("Reward/Penalty Settings")]
    public float successReward = 50f;
    public float checkpointReward = 10f;
    public float speedReward = 0.01f;
    public float collisionPenalty = -10f;
    public float idlePenalty = -10f;
    public float wrongCheckpointPenalty = -10f;

    //public TextMeshProUGUI monitorHUD;
    public TextMeshProUGUI areaMonitor;
    private Vector3 startPosition;
    private Vector3 finishLinePos;
    private CarAgent[] agentList;
    private CarAcademy academy;

    //performance
    public float recordLap = Mathf.Infinity;

    public List<GameObject> roadBuildList = new List<GameObject>();

    string mapName;
    string modelName;
    [SerializeField] private BlockData bd;


    void Awake(){

        //Screen.SetResolution(1920, 1080, true);
        //Screen.fullScreen = true;

        mapName = "selectedMap";
        LoadMap();

        academy = GameObject.Find("Academy").GetComponent<CarAcademy>();

        startPosition = GameObject.Find("Track").transform.GetChild(0).GetChild(0).position; // get the first road block of the track
       // float offsetY = startPosition.y + 10f;
       // startPosition = new Vector3(startPosition.x, offsetY, startPosition.z);
        EnableCheckPoints();

        //create finish line
        finishLinePos = new Vector3(startPosition.x, startPosition.y + 1.5f, startPosition.z-3f);
        Instantiate(finishLine, finishLinePos, Quaternion.Euler(0, 0, 0), transform.Find("Track"));

        Debug.Log("Training Start");
        

        int agentsToSpawn = agentAmount;
        if (spawnAgents && academy.spawnAgents)
        {
            if (academy.agentsPerArea != 0)
            {
                Debug.Log("Agents (academy): " + academy.agentsPerArea);
                agentsToSpawn = academy.agentsPerArea;
            }
            else
            {
                Debug.Log("Agents (area): " + agentsToSpawn);
            }
            SpawnAgents(agentsToSpawn, startPosition, Quaternion.Euler(0, 0, 0));
        }

    }

    private void FixedUpdate()
    {
        
    }




    //spawn agents in an area around the specified position
    public void SpawnAgents(int agents, Vector3 _position, Quaternion _rotation)
    {
        for (int i = 0; i < agents; i++)
        {
            //Instantiate(agentPrefab, new Vector3(_position.x + Random.Range(-1.5f, 1.5f), 0.1f, _position.z), _rotation, transform.Find("Agents"));
            Instantiate(agentPrefab, _position, _rotation, transform.Find("Agents"));
        }
    }

    public void UpdateMonitor(string text)
    {
        areaMonitor.text = text;
        //monitorHUD.text = text;
    }

    public void UpdateStats(float laptime)
    {
        if (laptime < recordLap)
        {
            recordLap = laptime;
        }
        UpdateMonitor("Best Lap: " + recordLap);
    }

    //useless for now
    public void ResetArea()
    {
        if (agentList == null)
        {
            agentList = GameObject.FindObjectsOfType<CarAgent>();
        }

        foreach (CarAgent agent in agentList)
        {
            agent.OnEpisodeBegin();
        }
        ResetCheckpoints();
    }

    private void ResetCheckpoints()
    {
        //checkpoints are manual for now
    }


    private void LoadMap(){
        string fileName = mapName;
        using(StreamReader sr = new StreamReader(Application.persistentDataPath + "/" + fileName + ".json")){
            string line;
            while((line = sr.ReadLine()) != null){ 
                bd = new BlockData();
                bd = JsonUtility.FromJson<BlockData>(line);
                foreach(GameObject item in roadBuildList){  
                    Debug.Log("script name" + bd.name + " || listname:" + item.name);
                    if(bd.name == item.name){
                        var go = Instantiate(item,bd.blockPosition,bd.blockRotation) as GameObject;
                        go.transform.SetParent(GameObject.Find("Track").transform);
                        go.tag = "wall";
                        go.transform.localPosition = bd.blockPosition;
                        go.transform.localRotation = bd.blockRotation;
                    }
                }
            }
        }
        Debug.Log("Loaded Successfully");
    }

    public void EnableCheckPoints(){
        string checkDiff;
        using(StreamReader sr = new StreamReader(Path.Combine(Application.persistentDataPath,"checkpoints.txt"))){
            checkDiff = sr.ReadLine();
        }

    if(!isNumber(checkDiff)){
        Debug.LogError("Can only place numbers in textfield");
            checkDiff = "3";
    }
    int checkPointDiff = Int32.Parse(checkDiff);
    //int checkPointDiff = 3;
    int counter = 0;
    int cpCounter = 0;
    GameObject parentObj = GameObject.Find("Track");
    Debug.Log(parentObj);
    foreach(Transform child in parentObj.transform){
        Transform checkPointChild = child.transform.GetChild(1);
        if(counter % checkPointDiff == 0){
            checkPointChild.GetComponent<Checkpoint>().isEnabled = true;
            checkPointChild.GetComponent<Checkpoint>().checkpointNumber = cpCounter;
            cpCounter++;
        }
        counter++;
    }
}

    private bool isNumber(string inStr){
        Regex regex = new Regex(@"^\d+$");
        return regex.IsMatch(inStr);
    }

}
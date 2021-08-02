﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;  
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

public class TM_OnClickObject : MonoBehaviour
{
    //Static fields
    string config_fileName;
    string emp;
    string config_path;
    string env_path;
    string env;
    string options_path;
    string mapPath;
    //---------------------------------------------------------------------------------

    //All the TextFields of HyperParameters
    //---------------------------------------------------------------------------------
    InputField tf_epochNum;
    InputField tf_epsilon;
    InputField tf_lamda;
    InputField tf_LR;
    InputField tf_LRS;
    InputField tf_batchSize;
    InputField tf_bufferSize;
    InputField tf_beta;
    InputField tf_hiddenUnits;
    InputField tf_numOfLayers;

    
    string txt_epochNum;
    string txt_epsilon;
    string txt_lamda;
    string txt_LR;
    string txt_LRS;
    string txt_batchSize;
    string txt_bufferSize;
    string txt_beta;
    string txt_hiddenUnits;
    string txt_numOfLayers;
    //-----------------------------HyperParameters End Here------------------------------

    //All the TextFields of Options
    //-----------------------------------------------------------------------------------
    InputField tf_runID;
    InputField tf_timeScale;
    InputField tf_numOfAgents;
    InputField tf_cpReward;
    InputField tf_speedReward;
    InputField tf_collisionPenalty;
    InputField tf_idlePenalty;
    InputField tf_wrongCheckPenalty;

    string txt_runID;
    string txt_timeScale;
    string txt_numOfAgents;
    string txt_cpReward;
    string txt_speedReward;
    string txt_collisionPenalty;
    string txt_idlePenalty;
    string txt_wrongCheckPenalty;
    //-----------------------------Options End Here----------------------------------
   

    Dropdown d_map;
    Text m_MapText;
    void Awake(){

        config_fileName = "trainer_config.yaml";
        config_path = Path.Combine(Application.dataPath, "../") +  "/config/" + config_fileName;
        env_path = Path.Combine(Application.dataPath, "../") + "/trainingScene";
        env = "trainingScene";
        options_path = Application.dataPath + "/options.json";

        d_map = GameObject.Find("MapDropDown").GetComponent<Dropdown>();
        mapPath = Application.persistentDataPath;
        List<string> m_mapDropOptions = new List<string>();
        foreach(var f in Directory.GetFiles(mapPath,"*.json")){
            var filename = new FileInfo(f).Name;
            m_mapDropOptions.Add(filename);
        }

        d_map.AddOptions(m_mapDropOptions);

        if(!File.Exists(options_path)){
            using(FileStream fs = File.Create(options_path));
        }

        
       // m_MapText = GameObject.Find("MapLabel").GetComponent<Text>();

        //All the TextFields of HyperParameters Initializations
        //---------------------------------------------------------------------------------
        tf_epochNum = GameObject.Find("EpochNumberInputField").GetComponent<InputField>();
        tf_epsilon = GameObject.Find("EpsilonInputField").GetComponent<InputField>();
        tf_lamda = GameObject.Find("LamdaInputField").GetComponent<InputField>();
        tf_LR = GameObject.Find("LearningRateInputField").GetComponent<InputField>();
        tf_LRS = GameObject.Find("LearningRateScheduleInputField").GetComponent<InputField>();
        tf_batchSize = GameObject.Find("BatchSizeInputField").GetComponent<InputField>();
        tf_bufferSize = GameObject.Find("BufferSizeInputField").GetComponent<InputField>();
        tf_beta = GameObject.Find("BetaInputField").GetComponent<InputField>();
        tf_hiddenUnits = GameObject.Find("HiddenUnitInputField").GetComponent<InputField>();
        tf_numOfLayers = GameObject.Find("NumOfLayersInputField").GetComponent<InputField>();

        
        txt_epochNum = tf_epochNum.text;
        txt_epsilon = tf_epsilon.text;
        txt_lamda = tf_lamda.text;
        txt_LR = tf_LR.text;
        txt_LRS = tf_LRS.text;
        txt_batchSize = tf_batchSize.text;
        txt_bufferSize = tf_bufferSize.text;
        txt_beta = tf_beta.text;
        txt_hiddenUnits = tf_hiddenUnits.text;
        txt_numOfLayers = tf_numOfLayers.text;
        //-----------------------------HyperParameters End Here------------------------------
        
        //All the TextFields of Options Initializations
        //-----------------------------------------------------------------------------------
        tf_runID            = GameObject.Find("RunIDInputField").GetComponent<InputField>();
        tf_timeScale        = GameObject.Find("TimeScaleInputField").GetComponent<InputField>();
        tf_numOfAgents      = GameObject.Find("NumberOfAgentsInputField").GetComponent<InputField>();
        tf_cpReward         = GameObject.Find("CheckpointRewardInputField").GetComponent<InputField>();
        tf_speedReward      = GameObject.Find("SpeedRewardInputField").GetComponent<InputField>();
        tf_collisionPenalty = GameObject.Find("CollisionPenaltyInputField").GetComponent<InputField>();
        tf_idlePenalty      = GameObject.Find("IdlePenaltyInputField").GetComponent<InputField>();
        tf_wrongCheckPenalty = GameObject.Find("WrongCheckpointPenaltyInputField").GetComponent<InputField>();

        txt_runID        = tf_runID.text;
        txt_timeScale    = tf_timeScale.text;
        txt_numOfAgents  = tf_numOfAgents.text;
        txt_cpReward     = tf_cpReward.text;
        txt_speedReward  = tf_speedReward.text;
        txt_collisionPenalty = tf_collisionPenalty.text;
        txt_idlePenalty      = tf_idlePenalty.text;
        txt_wrongCheckPenalty = tf_wrongCheckPenalty.text;
        //-----------------------------Options End Here----------------------------------
        
        //Fill the Hyperparameters and Options input fields with the current values
        OnSceneAwake();
    }

    public void OnClickRunButton(){

        //get the new values
        getTextFromFields();

        //save hyper parameters to config_path
        OverwriteHyperParameters(txt_epochNum,txt_epsilon,txt_lamda,txt_LR,txt_LRS,txt_batchSize,txt_bufferSize,txt_beta,txt_hiddenUnits,txt_numOfLayers);

        //save options to a json (except runid and timescale)
        OverwriteOptions(txt_numOfAgents, txt_cpReward, txt_speedReward, txt_collisionPenalty, txt_idlePenalty, txt_wrongCheckPenalty, txt_timeScale);

        //CHECK IF RUNID FILENAME ALREADY EXISTS, if it exists add --resume to the command below
        //run command "/C mlagents-learn" + config_path + " --train --time-scale=" + txt_timeScale + " --run-id=" + txt_runID + " --env=" + env + " " + CheckRunID(txt_runID);
        //string cmdString = "/C mlagents-learn" + config_path + " --train --time-scale=" + txt_timeScale + " --run-id=" + txt_runID + " --env=" + env + " " + CheckRunID(txt_runID);
        //System.Diagnostics.Process.Start("CMD.exe", cmdString); //Start cmd process

        SelectedMapFile();

        string strCmdText;
        string strCmdText1;
        string confPath;
        if (!Application.isEditor) 
        strCmdText1 = Path.Combine(Application.dataPath, "../../") + "/trainingScene/CarsML2021";
        else
        strCmdText1 = Path.Combine(Application.dataPath, "../") + "/trainingScene/CarsML2021";
        //if (!Application.isEditor)
        //confPath = Path.Combine(Application.dataPath, "../../") +  "/config/" + config_fileName;
        //else
        confPath = Path.Combine(Application.dataPath, "../") +  "/config/" + config_fileName;

        strCmdText = "/K mlagents-learn " + confPath + " --train --time-scale="+txt_timeScale+" --run-id="+txt_runID+" --env=" + strCmdText1 ;
        //System.Diagnostics.Process.Start("CMD.exe",strCmdText1); //Start cmd process
        Debug.Log(strCmdText);
        System.Diagnostics.Process.Start("CMD.exe",strCmdText); //Start cmd process
    }

    public void SelectedMapFile(){
        int mapVal = d_map.value;
        string mapFile = d_map.options[mapVal].text;

        File.Copy(Path.Combine(mapPath,mapFile),Path.Combine(mapPath,"selectedMap.json"),true);
    }


    //save hyper parameters to config_path
    public void OverwriteHyperParameters(string epochNum, string epsilon, string lamda, string LR, string LRS, string batchSize, string bufferSize, string beta, string hiddenUnits, string numOfLayers){
        using(StreamWriter sw = new StreamWriter(config_path)){
            //write all hyperparamaters in YAML format
    
            //construct objects to write in yaml
            var yamlObj = new YamlObj{
                behaviors = new Behaviors{
                CarBrain = new CarBrain{
                    trainer_type = "ppo",

                    hyperparameters = new Hyperparameters{
                    num_epoch = txt_epochNum,
                    epsilon = txt_epsilon,
                    lambd = txt_lamda,
                    learning_rate = txt_LR,
                    learning_rate_schedule = txt_LRS,
                    batch_size = txt_batchSize,
                    buffer_size = txt_bufferSize,
                    beta = txt_beta
                    },

                    network_settings = new NetworkSettings{
                    normalize = "true",
                    hidden_units = txt_hiddenUnits,
                    num_layers = txt_numOfLayers
                    },

                    reward_signals = new RewardSignals(){
                        extrinsic = new Extrinsic{
                            gamma = "0.99",
                            strength = "1.0"
                        },
                    },

                    time_horizon = "128",
                    max_steps = "1.0e8",
                    summary_freq = "10000"

                }
            }
            };

            //string behav = JsonUtility.ToJson(behaviors);
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(yamlObj);
            System.Console.WriteLine(yaml);

            sw.WriteLine(yaml);
            //Debug.Log(behaviors.CarBrain.trainer_type);

        }
    }

    //save options to options_path
    public void OverwriteOptions(string numOfAgents, string cpReward, string speedReward, string collisionPenalty, string idlePenalty, string wrongCheckPenalty, string timescale){
        using(StreamWriter sw = new StreamWriter(options_path)){
            //write all options in JSON format
            Options options = new Options();
            options.numOfAgents = numOfAgents;
            options.cpReward = cpReward;
            options.speedReward = speedReward;
            options.collisionPenalty = collisionPenalty;
            options.idlePenalty = idlePenalty;
            options.wrongCheckPenalty = wrongCheckPenalty;
            options.timescale = timescale;
            string opt = JsonUtility.ToJson(options);
            sw.WriteLine(opt);
        }
        using(StreamWriter sw = new StreamWriter(Path.Combine(Application.persistentDataPath,"checkpoints.txt"))){
            InputField checks = GameObject.Find("MapSelectInputField").GetComponent<InputField>();
            string txt_checks = checks.text;
            sw.WriteLine(txt_checks);
        }
        
    }

    public string CheckRunID(string runID){
        string path = Path.Combine(Application.dataPath, "../") + "/models/";
            if(Directory.Exists(path + runID)){
                Debug.Log("Directory Exists:" + path + runID);
                return "--resume";
            }
        return "";
    }

    string GetLatestModel(){
        string path = Path.Combine(Application.dataPath, "../") + "/models/";
        var dirName = new DirectoryInfo(path).GetDirectories()
                       .OrderByDescending(d=>d.LastWriteTimeUtc).First();
        return dirName.Name;

    }

    public void OnClickBack(){
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    //possibly just change it with Awake()
    public void OnSceneAwake(){
        if(File.Exists(config_path)){
            using(StreamReader sr = new StreamReader(config_path)){
                //fill all hyperparameters with the existing ones
                string line = sr.ReadToEnd();
                var r = new StringReader(line);
                //while((line = sr.ReadLine()) != null){ 
                    var yam = new YamlObj();
                    var deserializer = new DeserializerBuilder().Build();
                    var yamlObject = deserializer.Deserialize(r);
                    var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();
                    var json = serializer.Serialize(yamlObject);
                    yam = JsonUtility.FromJson<YamlObj>(json);
                    
                    txt_epochNum = yam.behaviors.CarBrain.hyperparameters.num_epoch;
                    txt_epsilon = yam.behaviors.CarBrain.hyperparameters.epsilon;
                    txt_lamda = yam.behaviors.CarBrain.hyperparameters.lambd;
                    txt_LR = yam.behaviors.CarBrain.hyperparameters.learning_rate;
                    txt_LRS = yam.behaviors.CarBrain.hyperparameters.learning_rate_schedule;
                    txt_batchSize = yam.behaviors.CarBrain.hyperparameters.batch_size;
                    txt_bufferSize = yam.behaviors.CarBrain.hyperparameters.buffer_size;
                    txt_beta = yam.behaviors.CarBrain.hyperparameters.beta;
                    txt_hiddenUnits = yam.behaviors.CarBrain.network_settings.hidden_units;
                    txt_numOfLayers = yam.behaviors.CarBrain.network_settings.num_layers;



                    tf_epochNum.GetComponent<InputField>().text = txt_epochNum;
                    tf_epsilon.GetComponent<InputField>().text = txt_epsilon;
                    tf_lamda.GetComponent<InputField>().text = txt_lamda;
                    tf_LR.GetComponent<InputField>().text = txt_LR;
                    tf_LRS.GetComponent<InputField>().text = txt_LRS;
                    tf_batchSize.GetComponent<InputField>().text = txt_batchSize;
                    tf_batchSize.GetComponent<InputField>().text = txt_batchSize;
                    tf_bufferSize.GetComponent<InputField>().text = txt_bufferSize;
                    tf_beta.GetComponent<InputField>().text = txt_beta;
                    tf_hiddenUnits.GetComponent<InputField>().text = txt_hiddenUnits;
                    tf_numOfLayers.GetComponent<InputField>().text = txt_numOfLayers;



                //}
            }
        }

        if(File.Exists(options_path)){
            using(StreamReader sr = new StreamReader(options_path)){
                //fill all options with the existing ones
                string line;
                while((line = sr.ReadLine()) != null){  //is while necessary? probably not, it's probably making a lot of junk objects like this, but will i dare change it? nah.
                    Options options = new Options();
                    options = JsonUtility.FromJson<Options>(line);
                    txt_numOfAgents = options.numOfAgents;
                    txt_cpReward = options.cpReward;
                    txt_speedReward = options.speedReward;
                    txt_collisionPenalty = options.collisionPenalty;
                    txt_idlePenalty = options.idlePenalty;
                    txt_wrongCheckPenalty = options.wrongCheckPenalty;
                    txt_timeScale = options.timescale;

                    tf_numOfAgents.GetComponent<InputField>().text = txt_numOfAgents;
                    tf_cpReward.GetComponent<InputField>().text = txt_cpReward;
                    tf_speedReward.GetComponent<InputField>().text = txt_speedReward;
                    tf_collisionPenalty.GetComponent<InputField>().text = txt_collisionPenalty;
                    tf_idlePenalty.GetComponent<InputField>().text = txt_idlePenalty;
                    tf_wrongCheckPenalty.GetComponent<InputField>().text = txt_wrongCheckPenalty;
                    tf_runID.GetComponent<InputField>().text = GetLatestModel();
                    tf_timeScale.GetComponent<InputField>().text = txt_timeScale;
                }
            }
        }
    }


    //a function to update the input fields quickly 
    void getTextFromFields(){
        txt_runID        = tf_runID.text;
        txt_timeScale    = tf_timeScale.text;
        txt_numOfAgents  = tf_numOfAgents.text;
        txt_cpReward     = tf_cpReward.text;
        txt_speedReward  = tf_speedReward.text;
        txt_collisionPenalty = tf_collisionPenalty.text;
        txt_idlePenalty      = tf_idlePenalty.text;
        txt_wrongCheckPenalty = tf_wrongCheckPenalty.text;
        txt_timeScale = tf_timeScale.text;

        txt_epochNum = tf_epochNum.text;
        txt_epsilon = tf_epsilon.text;
        txt_lamda = tf_lamda.text;
        txt_LR = tf_LR.text;
        txt_LRS = tf_LRS.text;
        txt_batchSize = tf_batchSize.text;
        txt_bufferSize = tf_bufferSize.text;
        txt_beta = tf_beta.text;
        txt_hiddenUnits = tf_hiddenUnits.text;
        txt_numOfLayers = tf_numOfLayers.text;
    }
}

//all the classes to make the yaml object with the correct format Behaviors->CarBrain->HyperParams|NetSettings|RewardSignals->Extrinsic
[System.Serializable]
public class Hyperparameters{
    public string   num_epoch;
    public string   epsilon;
    public string   lambd;
    public string   learning_rate;
    public string   learning_rate_schedule;
    public string   batch_size;
    public string   buffer_size;
    public string   beta;
}

[System.Serializable]
public class NetworkSettings{
    public string normalize;
    public string hidden_units ;
    public string num_layers ;
}
[System.Serializable]
public class Extrinsic{
    public string gamma ;
    public string strength ;
}
[System.Serializable]
public class RewardSignals{
    public Extrinsic extrinsic ;
}
[System.Serializable]
public class CarBrain{
    public string   trainer_type    ;
    public Hyperparameters hyperparameters ;
    public NetworkSettings network_settings ;
    public RewardSignals reward_signals ;
    public string time_horizon ;
    public string max_steps ;
    public string summary_freq ;
}
[System.Serializable]
public class YamlObj{
    public Behaviors behaviors ;
}
[System.Serializable]
public class Behaviors{
    public CarBrain CarBrain ;
}

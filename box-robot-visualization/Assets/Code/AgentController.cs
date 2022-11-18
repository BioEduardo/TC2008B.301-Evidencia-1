// TC2008B. Sistemas Multiagentes y Gráficas Computacionales
// C# client to interact with Python. Based on the code provided by Sergio Ruiz.
// Octavio Navarro. October 2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AgentData
{
    public string id;
    public float x, y, z;

    public AgentData(string id, float x, float y, float z)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[Serializable]
public class RobotData : AgentData
{
    public bool has_box;

    public RobotData(string id, float x, float y, float z, bool has_box) : base(id, x, y, z)
    {
        this.has_box = has_box;
    }
}

[Serializable]
public class CajaData : AgentData
{
    public int status;

    public CajaData(string id, float x, float y, float z, int status) : base(id, x, y, z)
    {
        this.status = status;
    }
}

[Serializable]
public class EstanteData : AgentData
{
    public int boxes;

    public EstanteData(string id, float x, float y, float z, int boxes) : base(id, x, y, z)
    {
        this.boxes = boxes;
    }
}

[Serializable]

public class RobotsData
{
    public List<RobotData> positions;

    public RobotsData() => this.positions = new List<RobotData>();
}

[Serializable]

public class CajasData
{
    public List<CajaData> positions;

    public CajasData() => this.positions = new List<CajaData>();
}

[Serializable]
public class EstantesData
{
    public List<EstanteData> positions;

    public EstantesData() => this.positions = new List<EstanteData>();
}

[Serializable]
public class AgentsData
{
    public List<AgentData> positions;

    public AgentsData() => this.positions = new List<AgentData>();
}

public class AgentController : MonoBehaviour
{
    // private string url = "https://agents.us-south.cf.appdomain.cloud/";
    string serverUrl = "http://localhost:8585";
    string getRobotsEndpoint = "/getRobots";
    string getEstantesEndpoint = "/getEstantes";
    string getCajasEndpoint = "/getCajas";
    string getPuertasEndpoint = "/getPuertas";
    string getParedesEndpoint = "/getParedes";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    AgentsData puertasData, paredesData;
    RobotsData robotsData;
    CajasData cajasData;
    EstantesData estanteData;
    public Dictionary<string, GameObject> agents, robotsCaja;
    public Dictionary<string, Vector3> prevPositions, currPositions;

    bool updated = false, started = false;

    public GameObject robotPrefab, robotboxPrefab, paredesPrefab, cajaPrefab, estantePrefab, puertaPrefab, floor;
    public int NAgents, width, height, maxtime;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        robotsData = new RobotsData();
        cajasData = new CajasData();
        puertasData = new AgentsData();
        paredesData = new AgentsData();
        estanteData = new EstantesData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();

        agents = new Dictionary<string, GameObject>();
        robotsCaja = new Dictionary<string, GameObject>();

        floor.transform.localScale = new Vector3((float)width/2, 1, (float)height/2);
        // floor.transform.localPosition = new Vector3((float)width/2 - .6f, 1, (float)height/2 +.4f);
        floor.transform.localPosition = new Vector3((float)width/2, 0, (float)height/2);
        
        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
    }

    private void Update() 
    {
        if(timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);

            foreach(var agent in currPositions)
            {
                Vector3 currentPosition = agent.Value;
                Vector3 previousPosition = prevPositions[agent.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;
                
                if(robotsCaja[agent.Key].activeInHierarchy) {
                    robotsCaja[agent.Key].transform.localPosition = interpolated;
                    if(direction != Vector3.zero) robotsCaja[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
                }
                else {
                    if(agents[agent.Key].activeInHierarchy) {
                        agents[agent.Key].transform.localPosition = interpolated;
                        if(direction != Vector3.zero) agents[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
                    } 
                }
                
            }

            // float t = (timer / timeToUpdate);
            // dt = t * t * ( 3f - 2f*t);
        }
    }
 
    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetRobotsData());
            //StartCoroutine(GetCajasData());
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("NAgents", NAgents.ToString());
        form.AddField("width", width.ToString());
        form.AddField("height", height.ToString());
        form.AddField("maxtime", maxtime.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Configuration upload complete!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetRobotsData());
            StartCoroutine(GetParedesData());
            StartCoroutine(GetCajasData());
            StartCoroutine(GetEstantesData());
            StartCoroutine(GetPuertasData());
        }
    }

    IEnumerator GetRobotsData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getRobotsEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            robotsData = JsonUtility.FromJson<RobotsData>(www.downloadHandler.text);
            Debug.Log(www.downloadHandler.text);

            foreach(RobotData robot in robotsData.positions)
            {
                Vector3 newRobotPosition = new Vector3(robot.x, robot.y + .025f, robot.z);
                

                    if(!started)
                    {
                        prevPositions[robot.id] = newRobotPosition;
                            agents[robot.id] = Instantiate(robotPrefab, newRobotPosition, Quaternion.identity);
                            robotsCaja[robot.id] = Instantiate(robotboxPrefab, newRobotPosition, Quaternion.identity);
                            robotsCaja[robot.id].SetActive(false);
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(robot.id, out currentPosition))
                            prevPositions[robot.id] = currentPosition;
                        currPositions[robot.id] = newRobotPosition;
                        if(robot.has_box)
                        {   
                            agents[robot.id].SetActive(false);
                            robotsCaja[robot.id].SetActive(true);
                        }
                        else {
                            agents[robot.id].SetActive(true);
                            robotsCaja[robot.id].SetActive(false);
                        }
                    }
            }

            updated = true;
            if(!started) started = true;
        }
    }

    // IEnumerator GetCajasData() 
    // {
    //     UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajasEndpoint);
    //     yield return www.SendWebRequest();
 
    //     if (www.result != UnityWebRequest.Result.Success)
    //         Debug.Log(www.error);
    //     else 
    //     {
    //         cajasData = JsonUtility.FromJson<CajasData>(www.downloadHandler.text);
    //         Debug.Log(cajasData.positions);

    //         foreach(CajaData caja in cajasData.positions)
    //         {
    //             Vector3 newCajaPosition = new Vector3(caja.x, caja.y, caja.z);

    //                 if(!started)
    //                 {
    //                     prevPositions[caja.id] = newCajaPosition;
    //                     agents[caja.id] = Instantiate(cajaPrefab, newCajaPosition, Quaternion.identity);
    //                 }
    //                 else
    //                 {
    //                     Vector3 currentPosition = new Vector3();
    //                     if(currPositions.TryGetValue(caja.id, out currentPosition))
    //                         prevPositions[caja.id] = currentPosition;
    //                     currPositions[caja.id] = newCajaPosition; 
    //                     if(caja.status == 1) agents[caja.id].SetActive(false);
    //                             else agents[caja.id].SetActive(true);
    //                 }
    //         }

    //         updated = true;
    //         if(!started) started = true;
    //     }
    // }

    IEnumerator GetCajasData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajasEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            cajasData = JsonUtility.FromJson<CajasData>(www.downloadHandler.text);

            Debug.Log(cajasData.positions);

            foreach(CajaData caja in cajasData.positions)
            {
                Instantiate(cajaPrefab, new Vector3(caja.x, caja.y + .125f, caja.z), Quaternion.identity);
                // if(caja.status == 1) agents[caja.id].SetActive(false);
                //         else agents[caja.id].SetActive(true);

            }
        }
    }

    IEnumerator GetEstantesData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getEstantesEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            estanteData = JsonUtility.FromJson<EstantesData>(www.downloadHandler.text);

            Debug.Log(estanteData.positions);

            foreach(EstanteData estante in estanteData.positions)
            {
                Instantiate(estantePrefab, new Vector3(estante.x, estante.y, estante.z), estantePrefab.transform.rotation);
            }
        }
    }

    IEnumerator GetParedesData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getParedesEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            paredesData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);
            Debug.Log(paredesData.positions);
        

            foreach(AgentData obstacle in paredesData.positions)
            {
                Instantiate(paredesPrefab, new Vector3(obstacle.x, obstacle.y, obstacle.z), Quaternion.identity);
            }
        }
    }

    IEnumerator GetPuertasData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getPuertasEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            puertasData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);
            Debug.Log(puertasData.positions);

            foreach(AgentData puerta in puertasData.positions)
            {
                Instantiate(puertaPrefab, new Vector3(puerta.x, puerta.y, puerta.z), puertaPrefab.transform.rotation);
            }
        }
    }
}

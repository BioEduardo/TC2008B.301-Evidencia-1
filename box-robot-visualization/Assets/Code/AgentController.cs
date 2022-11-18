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

public class CajaData : AgentData
{
    public bool status;

    public CajaData(string id, float x, float y, float z, bool status) : base(id, x, y, z)
    {
        this.recogido = recogido;
    }
}

[Serializable]
public class EstanteData
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

    public CajasData() => this.positions = new List<RobotData>();
}

[Serializable]
public class EstantesData
{
    public List<EstanteData> positions;

    public EstantesData() => this.positions = new List<EstanteData>();
}

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
    Dictionary<string, GameObject> robots, paredes, cajas, puertas, estantes;
    Dictionary<string, Vector3> prevPositions, currPositions;

    bool updated = false, started = false, startedBox = false;

    public GameObject robotPrefab, robotboxPrefab, paredesPrefab, cajaPrefab, estantePrefab, puertaPrefab, floor;
    public int NAgents, width, height, maxtime;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        agentsData = new AgentsData();
        obstacleData = new AgentsData();
        puertasData = new AgentsData();
        paredesData = new AgentsData();
        estantesData = new AgentsData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();

        agents = new Dictionary<string, GameObject>();

        floor.transform.localScale = new Vector3((float)width/10, 1, (float)height/10);
        floor.transform.localPosition = new Vector3((float)width/2-0.5f, 0, (float)height/2-0.5f);
        
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

                agents[agent.Key].transform.localPosition = interpolated;
                if(direction != Vector3.zero) agents[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
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
            StartCoroutine(GetCajasData());
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
            agentsData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach(AgentData agent in agentsData.positions)
            {
                Vector3 newAgentPosition = new Vector3(agent.x, agent.y, agent.z);

                    if(!started)
                    {
                        prevPositions[agent.id] = newAgentPosition;
                        agents[agent.id] = Instantiate(agentPrefab, newAgentPosition, Quaternion.identity);
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(agent.id, out currentPosition))
                            prevPositions[agent.id] = currentPosition;
                        currPositions[agent.id] = newAgentPosition;
                    }
            }

            updated = true;
            if(!started) started = true;
        }
    }

    IEnumerator GetCajasData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajasEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            cajasData = JsonUtility.FromJson<CajasData>(www.downloadHandler.text);

            Debug.Log("Caja Positions. " + cajasData.positions);

            foreach(AgentData agent in agentsData.positions)
            {
                Vector3 newCajaPosition = new Vector3(caja.x, caja.y, caja.z);
                if (!startedBox){
                    prevPositions[caja.id] = newCajaPosition;
                    agents[caja.id] = Instantiate(cajaPrefab, new Vector3(caja.x, caja.y + .125f, caja.z), Quaternion.identity);
                }
                else{
                    if (caja.status) agents[caja.id].SetActive(false);
                }
            }
            if (!startedBox) startedBox = true;
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
            obstacleData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            Debug.Log(obstacleData.positions);

            foreach(AgentData obstacle in obstacleData.positions)
            {
                Instantiate(estantePrefab, new Vector3(obstacle.x, obstacle.y, obstacle.z), Quaternion.identity);
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
            obstacleData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            Debug.Log(obstacleData.positions);

            foreach(AgentData obstacle in obstacleData.positions)
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
            obstacleData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            Debug.Log(obstacleData.positions);

            foreach(AgentData obstacle in obstacleData.positions)
            {
                Instantiate(puertaPrefab, new Vector3(obstacle.x, obstacle.y, obstacle.z), Quaternion.identity);
            }
        }
    }
}

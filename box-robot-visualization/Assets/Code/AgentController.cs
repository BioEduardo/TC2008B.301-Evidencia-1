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
    //Clase principal de los agentes, la cual guarda el id y la posicion de los agentes
    //creados en el modelo
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
    //Clase del Agente Robot, que hereda de la clase principal y crea el booleano que indica
    //si el robot tiene una caja en sus manos
    public bool has_box;

    public RobotData(string id, float x, float y, float z, bool has_box) : base(id, x, y, z)
    {
        this.has_box = has_box;
    }
}

[Serializable]
public class CajaData : AgentData
{
    //Clase del Agente caja, que hereda de la clase principal y crea el booleano que indica
    //si la caja ya fue recogida o no
    public bool status;

    public CajaData(string id, float x, float y, float z, bool status) : base(id, x, y, z)
    {
        this.status = status;
    }
}

[Serializable]
public class EstanteData : AgentData
{
    //Clase del Agente estante, que hereda de la clase principal y crea el entero que indica
    //cuantas cajas tiene actualmente el estante
    public int boxes;

    public EstanteData(string id, float x, float y, float z, int boxes) : base(id, x, y, z)
    {
        this.boxes = boxes;
    }
}

[Serializable]

public class RobotsData
{
    //Clase que permite guardar las posiciones de los agentes Robot, asi como tambien actualizarlas
    public List<RobotData> positions;

    public RobotsData() => this.positions = new List<RobotData>();
}

[Serializable]

public class CajasData
{
    //Clase que permite guardar las posiciones de los agentes Caja, asi como tambien actualizarlas
    public List<CajaData> positions;

    public CajasData() => this.positions = new List<CajaData>();
}

[Serializable]
public class EstantesData
{
    //Clase que permite guardar las posiciones de los agentes Estante, asi como tambien actualizarlas
    public List<EstanteData> positions;

    public EstantesData() => this.positions = new List<EstanteData>();
}

[Serializable]
public class AgentsData
{
    //Clase que permite guardar las posiciones de los agentes Pared y Puerta, asi como tambien actualizarlas
    public List<AgentData> positions;

    public AgentsData() => this.positions = new List<AgentData>();
}

public class AgentController : MonoBehaviour
{
    //Clase que permitira asignar los objetos al controlador, asi como inicializar las rutas de acceso
    //a los metodos del modelo
    // private string url = "https://agents.us-south.cf.appdomain.cloud/";
    string serverUrl = "http://localhost:8585";
    //Rutas de acceso
    string getRobotsEndpoint = "/getRobots";
    string getEstantesEndpoint = "/getEstantes";
    string getCajasEndpoint = "/getCajas";
    string getPuertasEndpoint = "/getPuertas";
    string getParedesEndpoint = "/getParedes";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    //Listas con las posiciones de los agentes
    AgentsData puertasData, paredesData;
    RobotsData robotsData;
    CajasData cajasData;
    EstantesData estanteData;
    //Listas de los gameobjects que se instanciaran en unity
    public Dictionary<string, GameObject> agents, robotsCaja;
    //Lista de las posisciones que se iran actualizando
    public Dictionary<string, Vector3> prevPositions, currPositions;

    // Mantiene el numero de cajas dibujadas en un estante dado su id
    public Dictionary<string, int> cajasEnEstante;

    bool updated = false, started = false, startedBox = false;

    public GameObject robotPrefab, robotboxPrefab, paredesPrefab, cajaPrefab, estantePrefab, puertaPrefab, floor;
    public int NAgents, width, height, maxtime;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        //Metodo que se llama al inicio de la simulacion, dando las posiciones de los agentes, creando los diccionarios
        //y escalando el suelo al tamaño del modelo
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
        floor.transform.localPosition = new Vector3((float)width/2, 0, (float)height/2);
        
        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
    }

    private void Update() 
    {
        //MEtodo que se llama cada frame, este metodo actualiza las posiciones de los agentes moviles,
        //asi como tambien actualiza el modelo de los robots y desaparece las cajas recogidas
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
                
                //Vertifica si el robot tiene una caja. Si la tiene, activa el prefab del robot con caja
                if(robotsCaja[agent.Key].activeInHierarchy) {
                    robotsCaja[agent.Key].transform.localPosition = interpolated;
                    if(direction != Vector3.zero) robotsCaja[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
                }
                else { // Funciona para desaparecer las cajas que ya no deben estar activas
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
        //Metodo que llama los Datos de los robots que se deben actualizar cada frame
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetRobotsData());
            StartCoroutine(GetCajasData());
            StartCoroutine(UpdateEstantesData());
        }
    }

    IEnumerator SendConfiguration()
    {
        //Corutina que envia los datos de las configuracion inicial al modelo, posteriormente
        //inicia las corutinas para instanciar todos los agentes
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
        //Corutina que envia y recibe los datos del agente robot, instanciandolos en las 
        //posiciones iniciales y actualizandolas una vez que sea llamada la corutina
        //en el metodo update
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

    IEnumerator GetCajasData() 
    {
        //Corutina que instancia las cajas en su posicion inicial, y que posteriomente 
        //desactivara la instancia de la caja cuando esta sea recogida
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCajasEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            cajasData = JsonUtility.FromJson<CajasData>(www.downloadHandler.text);

            Debug.Log("Caja Positions. " + cajasData.positions);

            foreach(CajaData caja in cajasData.positions)
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
        //Corutina que recibe las posiciones de los estantes y los instancia
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getEstantesEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            estanteData = JsonUtility.FromJson<EstantesData>(www.downloadHandler.text);

            Debug.Log(estanteData.positions);
            cajasEnEstante = new Dictionary<string, int>();

            foreach(EstanteData estante in estanteData.positions)
            {
                // Registra el id del estante en el diccionario de cantidad de cajas
                cajasEnEstante.Add(estante.id, 0);
                Instantiate(estantePrefab, new Vector3(estante.x, estante.y, estante.z), estantePrefab.transform.rotation);
            }
        }
    }

    IEnumerator UpdateEstantesData() 
    {
        //Corutina que actualiza el estado de cada estanteria, sumandole altura a las cajas
        //para simular su sobreposicion en una estanteria mientras esta tenga espacio
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getEstantesEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            estanteData = JsonUtility.FromJson<EstantesData>(www.downloadHandler.text);
            foreach (var estante in estanteData.positions) {
                // Crea cajas mientras no se haya alcanzado el numero real de cajas de la simulacion
                while (cajasEnEstante[estante.id] < estante.boxes) {
                    Instantiate(cajaPrefab,
                                new Vector3(estante.x,
                                            estante.y + 0.125f * cajasEnEstante[estante.id] + .125f, estante.z),
                                Quaternion.identity);
                    cajasEnEstante[estante.id]++;
                }
            }
        }
    }

    IEnumerator GetParedesData() 
    {
        //Corutina que instancia las paredes del modelo
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
                Instantiate(paredesPrefab, new Vector3(obstacle.x, obstacle.y + 0.5f, obstacle.z), Quaternion.identity);
            }
        }
    }

    IEnumerator GetPuertasData() 
    {
        //Corutina que instancia la pueta del modelo
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

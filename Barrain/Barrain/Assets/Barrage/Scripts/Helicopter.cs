using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Helicopter : MonoBehaviour
{

    private Vector3 dest;
    private GameObject freight;
    private Vector3 initPos;
    private float startTime;
    private float timeTransport;

    private float fuel;
    public float MaxFuel;

    public Camera MainCamera;
    public Vector3 PosHelicopterInit;


    private Vector3 initPosPiece;
    public float timeLoading;
    private float startTimeLoading;


    private bool newLoad;
    private bool onRoad;

    public Vector2 pos;
    public Vector2 size;
    public Texture2D fuelBarEmpty;
    public Texture2D fuelBarFull;


    // Use this for initialization
    void Start ()
    {
        Init();
    }

    public void Init()
    {
        transform.position = PosHelicopterInit;
        transform.rotation = Quaternion.Euler(0, 270, 0);
        initPos = transform.position;
        newLoad = true;
        fuel = MaxFuel;

        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().freezeRotation = true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Animator>().speed = 2;
    }

    void FixedUpdate()
    {
        if(MainCamera.GetComponent<GameManager>().GameState != GameManager.GameStates.Terminated)
        {
            if (freight)
            {
                if (onRoad)
                {
                    MoveAt(freight.transform.position, timeTransport);
                }


                if (Vector3.Distance(freight.transform.position, transform.position) < 130 && MainCamera.GetComponent<GameManager>().GameState == GameManager.GameStates.Factory)
                {
                    onRoad = false;
                    Load(freight);
                }
                else if (Vector3.Distance(freight.transform.position, transform.position) < 130 && MainCamera.GetComponent<GameManager>().GameState == GameManager.GameStates.Transport)
                {
                    freight = null;
                    fuel--;
                    onRoad = false;
                    MainCamera.GetComponent<GameManager>().GameState = MainCamera.GetComponent<GameManager>().NextGameState;
                }


            }


            if (fuel < 0)
            {
                MainCamera.GetComponent<GameManager>().LevelFailed();
                MainCamera.GetComponent<GameManager>().txtInfo.text = "Plus de carburant";
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().freezeRotation = false;
                GetComponent<Animator>().speed = 0;
            }
        }
        
        
    }

    public void Load(GameObject bloc)
    {
        if (newLoad)
        {
            startTimeLoading = Time.time;
            initPosPiece = bloc.transform.position;
            newLoad = false;     
        }

        if(Time.time > startTimeLoading + timeLoading)
        {
            Piece p = Piece.Search(bloc);
            
            MainCamera.GetComponent<GameManager>().Load(p); 
            GetComponentInChildren<Light>().enabled = false;
            return;
        }
        else
        {
            GetComponentInChildren<Light>().enabled = true;
            bloc.transform.position = Vector3.Slerp(initPosPiece, transform.position + new Vector3(0f, -20f, 0f), (Time.time - startTimeLoading) / timeLoading);
        }

    }

    public void MoveAt(Vector3 dest, float timeTransport)
    {

        transform.position = Vector3.Slerp(initPos, dest + new Vector3(0f, 120f, 0f), (Time.time - startTime) / timeTransport);
        if (MainCamera.GetComponent<GameManager>().GameState == GameManager.GameStates.Transport)
        {
            MainCamera.transform.position = transform.position - new Vector3(0f, -50f, 200f);
        }
        
        //met l'hélicopter dans la bonne direction
        Quaternion rotation = Quaternion.LookRotation((dest - transform.position)) *
                                Quaternion.EulerAngles(0f, 90f, 0f);


        transform.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 10f);
    }

    public void Move(GameObject freight, float timeTransport)
    {
        initPos = transform.position;
        this.freight = freight;
        startTime = Time.time;
        this.timeTransport = timeTransport;

        newLoad = true;
        onRoad = true;

    }
	

    void OnGUI()
    {
        float posX = Screen.width * pos.x;
        float posY = Screen.height * pos.y;

        GUI.BeginGroup(new Rect(posX, posY, size.x, size.y));
        GUI.Box(new Rect(0, 0, size.x, size.y), fuelBarEmpty);

        // draw the filled-in part:
        GUI.BeginGroup(new Rect(0, (size.y - (size.y * fuel/MaxFuel)), size.x, size.y * fuel/MaxFuel));
        GUI.Box(new Rect(0, -size.y + (size.y * fuel/MaxFuel), size.x, size.y), fuelBarFull);
        GUI.EndGroup();
        GUI.EndGroup();
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {


    public enum GameStates { Start, Factory, Barrage, Pose, Transport, Terminated}

    public GameStates GameState;
    public GameStates NextGameState;
    public Queue<Piece> nextPieces;

    public Piece[] Freights;
    public int FreightSize;

    public Camera MainCamera;
    public Camera CameraFreights;
    public Barrage Barrage;

    public TextAsset xmlFile;

    public Vector3 PosFreight;
    public Vector3 DistBwnFreight;
    public GameObject supportFreight;
    private List<GameObject> supportsFreight;

    private Piece SelectedPiece;
    public GameObject bloc;

    public Button btBarrage;
    public Button btFactory;

    public Button btNext;
    public Button btReload;

    public GameObject Helicopter;

    public GameObject nearFactory;
    public GameObject apartFactory;
    public GameObject factory;
    public GameObject barrage;

    public GameObject terrainFactory;


    //extract data
    private float time;
    private int nMissingBlocs;
    private int nTransport;
    private int nPiece;
    private int nEssay;
    private float averagePieceSize;


    private int maxMissingBlocs;

    //MissingBlocGUI
    public Vector2 pos;
    public Vector2 size;
    public Texture2D fuelBarEmpty;
    public Texture2D fuelBarFull;

    //GUI
    public Button btLoadLevel;
    public Dropdown ddChooseLevel;
    public Canvas cChooseLevel;

    public Text txtSuceed;
    public Text txtFailed;
    public Text txtInfo;

    public string path;
    private Dictionary<string, string> dLevels;
    private List<string> lLevels;
    private string levelName;
    private int currentLevel;

    private PlayerBarrageStatistics stats;

    // Use this for initialization
    void Start () {
        dLevels = ListLevel(path);
        lLevels = new List<string>(dLevels.Keys);
        ddChooseLevel.AddOptions(lLevels);

        stats = new PlayerBarrageStatistics();

        //UI
        btBarrage.GetComponent<Button>().onClick.AddListener(GoToBarrage);
        btFactory.GetComponent<Button>().onClick.AddListener(GoToFactory);
        supportsFreight = new List<GameObject>();

    }

    public void LoadLevel(int opt = 0)
    {
        txtFailed.gameObject.SetActive(false);
        txtSuceed.gameObject.SetActive(false);
        nearFactory.SetActive(false);
        apartFactory.SetActive(false);
        cChooseLevel.gameObject.SetActive(false);
        btBarrage.gameObject.SetActive(true);
        btFactory.gameObject.SetActive(true);

        if (factory != null)
        {
            factory.GetComponent<Factory>().Init();
        }

        if (Freights != null)
        {
            foreach (Piece p in Freights)
            {
                if (p != null)
                {
                    p.ResetPBlocs();
                }

            }
        }

        btBarrage.interactable = true;
        btFactory.interactable = false;

        nextPieces = new Queue<Piece>();

        if(opt == 0)
        {
            ParseXml(dLevels[lLevels[ddChooseLevel.value]]);
            currentLevel = ddChooseLevel.value;
            levelName = lLevels[ddChooseLevel.value];
        }
        else if(opt == 1)
        {
            ParseXml(dLevels[lLevels[currentLevel]]);
            levelName = lLevels[currentLevel];
        }
        else if(opt == 2)
        {
            currentLevel++;
            ParseXml(dLevels[lLevels[currentLevel]]);
            levelName = lLevels[currentLevel];
        }

        ddChooseLevel.value = currentLevel;


        factory.GetComponent<Factory>().Init();

        GameState = GameStates.Factory;
        txtInfo.text = "Cliquer sur une pièce proche de l'usine pour la charger dans l'hélicopter \n" +
                        "Cliquer sur le bouton <- pour déplacer l'hélicoptère au barrage \n";

        Freights = new Piece[FreightSize];

        PlaceSupport();

        Barrage.Generate();
        Helicopter.GetComponent<Helicopter>().Init();
        

        Helicopter.transform.position = factory.transform.position + new Vector3(0, 140, 0);

        //extract data
        time = Time.time;
        nMissingBlocs = 0;
        nTransport = 0;
        nPiece = 0;
        averagePieceSize = 0;
    }

    public void Reload()
    {
        LoadLevel(1);
    }

    public void NextLevel()
    {
        if(currentLevel < lLevels.Count-2)
        {
            LoadLevel(2);
        }    
    }


    void GoToBarrage()
    {


        GameState = GameStates.Transport;
        NextGameState = GameStates.Barrage;
        Helicopter.GetComponent<Helicopter>().Move(barrage, 2.0f);
        btBarrage.interactable = false;
        btFactory.interactable = true;
        nTransport++;
        txtInfo.text = "Cliquer sur une pièce chargée pour la placer sur le barrage \n" +
                        "Utiliser les touches W,A,S,D pour la déplacer et le touches U,I,O,J,K,L pour la tourner \n" +
                        "Appuyer sur la touche Espace pour la fixer et continuer \n" +
                        "Cliquer sur le bouton -> pour déplacer l'hélicoptère à l'usine \n";

    }

    void GoToFactory()
    {
        GameState = GameStates.Transport;
        NextGameState = GameStates.Factory;
        Helicopter.GetComponent<Helicopter>().Move(factory, 2.0f);
        btFactory.interactable = false;
        btBarrage.interactable = true;
        nTransport++;
        txtInfo.text = "Cliquer sur une pièce proche de l'usine pour la charger dans l'hélicopter \n" +
                        "Cliquer sur le bouton <- pour déplacer l'hélicoptère au barrage \n";
    }

    public void AddMissingBloc()
    {
        nMissingBlocs++;
        if(nMissingBlocs > maxMissingBlocs)
        {
            Barrage.Destroy();
            txtInfo.text = "Structure du barrage trop fragile \n";
            LevelFailed();
        }
    }

    public void EndPose()
    {
        Helicopter.GetComponentInChildren<Light>().enabled = false;
        if (GameState == GameStates.Pose)
        {
            GameState = GameStates.Barrage;
            foreach (var p in Freights)
            {
                if(p != null)
                {
                    return;
                }
            }
            if(factory.GetComponent<Factory>().pieces.Count == 0)
            {
                txtInfo.text = "Plus de pièces";
                LevelFailed();
        }
        }
        
        
    }

    public void LevelSucceed()
    {
        time = Time.time - time;
        averagePieceSize = averagePieceSize/nPiece;
        GameState = GameStates.Terminated;

        txtSuceed.gameObject.SetActive(true);
        if (currentLevel < lLevels.Count - 2)
        {
            btNext.gameObject.SetActive(true);
        }
        else
        {
            btNext.gameObject.SetActive(true);
        }
        btReload.gameObject.SetActive(false);
        


        cChooseLevel.gameObject.SetActive(true);

        stats.SetData(levelName, true, time, nMissingBlocs, nTransport, nPiece, averagePieceSize);

        btBarrage.gameObject.SetActive(false);
        btFactory.gameObject.SetActive(false);

        txtInfo.text = "";



    }



    public void LevelFailed()
    {
        
        if (GameState != GameStates.Terminated)
        {
            time = Time.time - time;
            GameState = GameStates.Terminated;

            txtFailed.gameObject.SetActive(true);
            btReload.gameObject.SetActive(true);
            btNext.gameObject.SetActive(false);

            cChooseLevel.gameObject.SetActive(true);

            stats.SetData(levelName, false, time, nMissingBlocs, nTransport, nPiece, averagePieceSize);

            btBarrage.gameObject.SetActive(false);
            btFactory.gameObject.SetActive(false);
}
        
    }

    public void ExitAndSave()
    {
        PlayerBarrageStatistics.SaveBarrageData(stats);
        Application.Quit();
    }

    public Piece GetSelectedPiece()
    {
        return SelectedPiece;
    }

    private void PlaceSupport()
    {
        foreach(GameObject go in supportsFreight)
        {
            Destroy(go);
        }
        supportsFreight = new List<GameObject>();
        for (int i = 0; i < Freights.Length; i++)
        {
            supportsFreight.Add(
                (GameObject)Instantiate(supportFreight, PosFreight-new Vector3(0, 10f, 0) + DistBwnFreight * i, Quaternion.identity));
            supportsFreight[i].layer = 8;
            supportsFreight[i].transform.rotation = Quaternion.Euler(-40f, 0, 0);
        }
    }
    public bool Load(Piece piece)
    {
        

        for (int i = 0; i < Freights.Length; i++)
        {
            if (Freights[i] == null)
            {
                factory.GetComponent<Factory>().pieces.Remove(piece);
                piece.ResetPBlocs();
                foreach (Vector3 blocPos in piece.blocs)
                {
                    Freights[i] = piece;
                    GameObject b = (GameObject)Instantiate(bloc, PosFreight + DistBwnFreight * i+blocPos*5.0f, Quaternion.identity);
                    piece.pBlocs.Add(b);
                    b.AddComponent<Rigidbody>();
                    b.GetComponent<Rigidbody>().useGravity = false;
                    b.transform.GetChild(0).gameObject.layer = 8;
                    b.layer = 8;
                    //b.transform.position = PosFreight + DistBwnFreight * i;
                    if (piece.pBlocs.Count > 1)
                    {
                        piece.pBlocs[piece.pBlocs.Count - 2].AddComponent<FixedJoint>().connectedBody =
                            b.GetComponent<Rigidbody>();
                    }

                }

                piece.pBlocs[0].GetComponent<Rigidbody>().AddTorque(transform.up * 10000);
                
                return true;
            }
        }

        return false;
        
    }

	
	// Update is called once per frame
	void FixedUpdate () {

        if (Input.GetMouseButtonDown(0) && GameState == GameStates.Barrage)
        {
            Ray ray = CameraFreights.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            foreach (Piece piece in Freights)
            {
                if(piece != null)
                {
                    foreach (GameObject o in piece.pBlocs)
                    {
                        if (o.GetComponent<Collider>().Raycast(ray, out hit, 2000f))
                        {
                            GameState = GameStates.Pose;
                            Helicopter.GetComponentInChildren<Light>().enabled = true;
                            btFactory.interactable = false;
                            SelectedPiece = Piece.Search(o);
                            piece.ResetPBlocs();
                            Barrage.PutPiece(SelectedPiece);
                            //Extract Data
                            nPiece++;
                            averagePieceSize += SelectedPiece.GetSize();

                            for(int i = 0; i < Freights.Length; i++)
                            {
                                if(Freights[i] == SelectedPiece)
                                {
                                    Freights[i] = null;
                                }
                            }

                            return;
                        }
                    }
                }
                

            }

            
            
        }
    }

    Dictionary<string, string> ListLevel(string path)
    {

        Dictionary<string, string> levels = new Dictionary<string, string>();

        try
        {
            var files = from file in Directory.GetFiles(path, "*.xml")
                        from line in File.ReadAllLines(file)
                        where line.Contains("<barrage>")
                        select new
                        {
                            File = file,
                        };
            foreach (var f in files)
            {
                levels.Add(Path.GetFileNameWithoutExtension(f.File), f.File);
            }
        }
        catch (UnauthorizedAccessException UAEx)
        {
            Console.WriteLine(UAEx.Message);
        }
        catch (PathTooLongException PathEx)
        {
            Console.WriteLine(PathEx.Message);
        }

        return levels;
    }


    public void ParseXml(string fileName)
    {
        String xmlString = xmlFile.ToString();

        TextReader readert;
        readert = new StreamReader(fileName);
        xmlString = readert.ReadToEnd();


        try
        {

            using (XmlReader reader = new XmlTextReader(new StringReader(xmlString)))
            {
               
                int tX = Barrage.width;

                reader.ReadToFollowing("size");
                reader.MoveToAttribute("y");
                int tY = Convert.ToInt32(reader.Value)+2;

                reader.MoveToAttribute("z");
                int tZ = Convert.ToInt32(reader.Value);

                //Base

                Barrage.barrageMatrix = new int[tX, tY+100, tZ];

                for (int x = 0; x < tX; x++)
                {
                    for (int z = 0; z < tZ; z++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            Barrage.barrageMatrix[x, y, z] = 1;
                        }

                    }
                }

                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < tZ; z++)
                    {
                        for (int y = 0; y < tY; y++)
                        {
                            Barrage.barrageMatrix[x, y, z] = 1;
                        }

                        for (int y = 0; y < tY; y++)
                        {
                            Barrage.barrageMatrix[tX - 1 - x, y, z] = 1;
                        }


                    }
                }

                reader.ReadToFollowing("lvl");
                reader.MoveToAttribute("freightSize");
                FreightSize = Convert.ToInt32(reader.Value);

                reader.MoveToAttribute("maxMissingBlocs");
                maxMissingBlocs = Convert.ToInt32(reader.Value);

                reader.MoveToAttribute("fuelCapacity");
                Helicopter.GetComponent<Helicopter>().MaxFuel = Convert.ToInt32(reader.Value);

                reader.MoveToAttribute("freightSize");
                FreightSize = Convert.ToInt32(reader.Value);

                reader.MoveToAttribute("nearFactory");
                if (reader.Value.Trim() == "True")
                {
                    terrainFactory.SetActive(false);
                    factory = nearFactory;
                    nearFactory.SetActive(true);

                }
                else {
                    terrainFactory.SetActive(true);
                    factory = apartFactory;
                    apartFactory.SetActive(true);
                }

                while (reader.Read())
                {

                    if (reader.Name == "piece")
                    {
                        reader.Read();
                        if (reader.Value.Trim().Length > 0)
                        {
                            nextPieces.Enqueue(new Piece(reader.Value.Trim()));

                        }

                    }
                    if (reader.Name == "bloc")
                    {
                        try
                        {
                            reader.MoveToAttribute("x");
                            int x = Convert.ToInt32(reader.Value);
                            reader.MoveToAttribute("y");
                            int y = Convert.ToInt32(reader.Value);
                            reader.MoveToAttribute("z");
                            int z = Convert.ToInt32(reader.Value);


                            try
                            {
                                Barrage.barrageMatrix[x + 2, y + 2, z] = 1;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                print(e.StackTrace);
                            }
                        }
                        catch (FormatException e)
                        {

                        }
                    }

                }



            }
        }
        catch (Exception e)
        {
            print("Invalid Level !");
        }
    }

    void OnGUI()
    {
        
        if(maxMissingBlocs > 0)
        {
            float posX = Screen.width * pos.x;
            float posY = Screen.height * pos.y;

            GUI.BeginGroup(new Rect(posX, posY, size.x, size.y));
            GUI.Box(new Rect(0, 0, size.x, size.y), fuelBarEmpty);

            // draw the filled-in part:
            GUI.BeginGroup(new Rect(0, (size.y - (size.y * (1 - nMissingBlocs / maxMissingBlocs))), size.x, size.y * (1 - nMissingBlocs / maxMissingBlocs)));
            GUI.Box(new Rect(0, -size.y + (size.y * (1 - (nMissingBlocs / maxMissingBlocs))), size.x, size.y), fuelBarFull);
            GUI.EndGroup();
            GUI.EndGroup();
        }
        
    }


}

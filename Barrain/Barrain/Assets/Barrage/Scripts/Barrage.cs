using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class Barrage : MonoBehaviour {

    public int[,,] barrageMatrix;
    public TextAsset xmlFile;
    public GameObject canvasBloc;
    public GameObject target;
    public GameObject patchBloc;
    public GameObject errorBloc;

    public GameObject helicopter;

    public GameObject Lake;

    public GameManager GameManager;

    public Material rock;

    private List<GameObject> goBlocs;

    public int width;

    int mov;

    private bool space;
    //private Piece piece;

    private Vector3 targetPos;

    Piece piece;

    

    public float blocSize;

    void Start() {
        
        
    }


    public void Generate()
    {
        if(goBlocs != null)
        {
            foreach (GameObject go in goBlocs)
            {
                Destroy(go);
            }
            
        }

        goBlocs = new List<GameObject>();


        InvokeRepeating("UpdateWaterLevel", 0.5f, 0.5f);


        for (int x = 0; x < barrageMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < barrageMatrix.GetLength(1); y++) 
            {
                for (int z = 0; z < barrageMatrix.GetLength(2); z++)
                {
                    if(barrageMatrix[x, y, z] == 1)
                    {
                        goBlocs.Add((GameObject)Instantiate(canvasBloc, new Vector3(x * blocSize, y * blocSize, blocSize * z), Quaternion.identity));
                    }
                }
            }
        }

        targetPos = new Vector3(0, 0, 0);
        
        
    }

    public void PutPiece(Piece piece)
    {
        this.piece = piece;
        CreateTarget(this.piece);
        PlaceTarget(targetPos, this.piece);
        piece.helicopter = helicopter;
    }

    public bool IsFinished()
    {

        for (int x = 0; x < barrageMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < barrageMatrix.GetLength(1)-100; y++)
            {
                for (int z = 0; z < barrageMatrix.GetLength(2); z++)
                {
                    if (barrageMatrix[x, y, z] == 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    float GetUpper(float x, float z)
    {
        /*
        if (barrageMatrix[(int)x, barrageMatrix.GetLength(1) - 1, (int)z] != 0)
        {
            return -1;
        }*/

        for (int iy = barrageMatrix.GetLength(1)-1; iy > 0; iy--) {
            try
            {
                if (barrageMatrix[(int)x, (int)iy, (int)z] != 0)
                {
                    if (barrageMatrix.GetLength(1) - 1 == iy)
                    {
                        //return -1;
                    }
                    return (iy + 1);
                }
            }catch(IndexOutOfRangeException ioore)
            {
                return 0;
            }
            
        }

        return 12; 
    }

    public void Destroy()
    {
        foreach(GameObject bloc in goBlocs)
        {
            if (!bloc.GetComponent<Rigidbody>())
            {
                bloc.AddComponent<Rigidbody>();
            }     
            if (UnityEngine.Random.Range(0f, 1f) > 0.9f)
            {
                bloc.GetComponent<Rigidbody>().AddForce(transform.up * 1200);
            }

            for (int x = 0; x < barrageMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < barrageMatrix.GetLength(1); y++)
                {
                    for (int z = 0; z < barrageMatrix.GetLength(2); z++)
                    {
                        barrageMatrix[x, y, z] = 0;
                        
                    }
                }
            }

        }
    }

    void Update()
    {
        if(GameManager.GameState == GameManager.GameStates.Pose && piece != null)
        {
            if (Input.GetAxis("Horizontal") > 0 && targetPos.x < barrageMatrix.GetLength(0) - 1 && mov != 1)
            {
                targetPos.x++;
                mov = 1;
            }
            if (Input.GetAxis("Horizontal") < 0 && targetPos.x > 0 && mov != 2)
            {
                targetPos.x--;
                mov = 2;
            }

            if (Input.GetAxis("Vertical") > 0 && targetPos.z < barrageMatrix.GetLength(2) - 1 && mov != 1)
            {
                targetPos.z++;
                mov = 1;
            }
            if (Input.GetAxis("Vertical") < 0 && targetPos.z > 0 && mov != 2)
            {
                targetPos.z--;
                mov = 2;
            }
            if ((Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.L)) && mov != 1)
            {
                piece.Rotate(new Vector3(0, 0, 90));
                mov = 1;
            }
            if ((Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.J)) && mov != 2)
            {
                piece.Rotate(new Vector3(0, 0, -90));
                mov = 2;
            }
            if ((Input.GetKey(KeyCode.Keypad7) || Input.GetKey(KeyCode.U)) && mov != 1)
            {
                piece.Rotate(new Vector3(0, 90, 0));
                mov = 1;
            }
            if ((Input.GetKey(KeyCode.Keypad9) || Input.GetKey(KeyCode.O)) && mov != 2)
            {
                piece.Rotate(new Vector3(0, -90, 0));
                mov = 2;
            }
            if ((Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.I)) && mov != 1)
            {
                piece.Rotate(new Vector3(90, 0, 0));
                mov = 1;
            }
            if ((Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.K)) && mov != 2)
            {
                piece.Rotate(new Vector3(-90, 0, 0));
                mov = 2;
            }

            // Pas une bonne solution, ne supporte pas les frappes rapides
            if (mov != 0 && (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) && !Input.anyKey)
            {
                piece.Translate();
                mov = 0;
            }

            targetPos.y = 0;
            foreach (Vector3 bloc in piece.blocs)
            {
                targetPos.y = Math.Max(targetPos.y, GetUpper(targetPos.x + bloc.x, targetPos.z + bloc.z) - piece.GetLowerPosition(bloc.x, bloc.z));
            }

            /*
            //Invalide, impossible pour le moment
            if (targetPos.y < 0)
            {
                targetPos.y = 8;
                PlaceTarget(targetPos, piece);
                target.GetComponent<Renderer>().material.color = Color.red;
            }
            else*/
            {
                PlaceTarget(targetPos, piece);
                //target.GetComponent<Renderer>().material.color = Color.blue;
                if (Input.GetKey(KeyCode.Space))
                {
                    if (!space)
                    {
                        space = true;
                        PlacePatch(targetPos, piece);
                    }

                }
                else
                {
                    space = false;
                }
            }

        }        
    }

    void CreateTarget(Piece piece)
    {
        foreach(Vector3 blocPos in piece.blocs)
        {
            piece.pBlocs.Add((GameObject)Instantiate(target, blocPos * blocSize, Quaternion.identity));
        }
    }

    //TODO fix bug selection other piece
    void PlaceTarget(Vector3 targetPos, Piece piece)
    {
        for(int i = 0; i < piece.blocs.Count; i++)
        {
            piece.pBlocs[i].transform.position = (targetPos + piece.blocs[i]) * blocSize;
        }    
    }

    void PlacePatch(Vector3 targetPos, Piece piece)
    {
        for (int i = 0; i < piece.blocs.Count; i++)
        {
            goBlocs.Add((GameObject)Instantiate(patchBloc, (targetPos + piece.blocs[i]) * blocSize, Quaternion.identity));
            try
            {
                barrageMatrix[(int)(targetPos.x + piece.blocs[i].x), (int)(targetPos.y + piece.blocs[i].y), (int)(targetPos.z + piece.blocs[i].z)] = 2;
            }
            catch(IndexOutOfRangeException e)
            {
                //No Matter
            }
            
        }
        DetectError();
        piece.ResetPBlocs();
        this.piece = null;
        GameManager.btFactory.interactable = true;
        if (IsFinished())
        {
            GameManager.LevelSucceed();
        }
        GameManager.EndPose();
    }

    void PlaceErrorBloc(int posX, int posY, int posZ)
    {
        barrageMatrix[posX, posY, posZ] = 3;
        goBlocs.Add((GameObject)Instantiate(errorBloc, new Vector3(posX, posY, posZ) * blocSize, Quaternion.identity));
        GameManager.AddMissingBloc();
        
        StartCoroutine(TransparencyEffect(1f));
        

    }

    IEnumerator TransparencyEffect(float seconds)
    {
        foreach (GameObject bloc in goBlocs)
        {
            bloc.GetComponentInChildren<MeshRenderer>().material.SetFloat("_Metallic", 0.3f);
        }
        yield return new WaitForSeconds(seconds);
        foreach (GameObject bloc in goBlocs)
        {
            bloc.GetComponentInChildren<MeshRenderer>().material.SetFloat("_Metallic", 1f);
        }
    }

    void DetectError()
    {
        try
        {
            for (int z = 0; z < barrageMatrix.GetLength(2); z++)
            {
                for (int x = 0; x < barrageMatrix.GetLength(0); x++)
                {
                    bool higestBlocFounded = false;
                    for (int y = barrageMatrix.GetLength(1) - 1; y > 0; y--)
                    {
                        if (!higestBlocFounded && barrageMatrix[x, y, z] != 0)
                        {
                            higestBlocFounded = true;
                        }

                        if (higestBlocFounded && barrageMatrix[x, y, z] == 0)
                        {
                            foreach (GameObject o in goBlocs)
                            {
                                //o.GetComponentInChildren<MeshRenderer>().GetComponent<Material>().color.a = 0.5f;
                                //o.GetComponentInChildren<GameObject>()
                            }
                            PlaceErrorBloc(x, y, z);
                        }
                    }
                }
            }
        }
        catch (NullReferenceException e)
        {

        }

    }

    void UpdateWaterLevel()
    {
        try
        {
            if (getLevelFirstHole() * blocSize - 1.5 * blocSize > Lake.transform.position.y)
            {
                Lake.transform.position += new Vector3(0f, 2f, 0f);
            }
            if (getLevelFirstHole() * blocSize - 1.5 * blocSize < Lake.transform.position.y)
            {
                Lake.transform.position += new Vector3(0f, -2f, 0f);
            }
        }
        catch (NullReferenceException e)
        {

        }


    }

    int getLevelFirstHole()
    {
        for (int y = 0; y < barrageMatrix.GetLength(1); y++)
        {
            for (int x = 0; x < barrageMatrix.GetLength(0); x++)
            {
                for (int z = 0; z < barrageMatrix.GetLength(2); z++)
                {
                
                    if (barrageMatrix[x, y, z] == 0)
                    {
                        //Debug.Log("Hole : " + x + " " + y + " " + z);
                        return y;    
                    }
                   
                }
            }
        }
        return barrageMatrix.GetLength(1)-1;
    }
}




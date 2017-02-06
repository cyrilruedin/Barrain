using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;

public class Factory : MonoBehaviour
{

    public GameObject bloc;
    public float blocSize;
    public Vector3 position;

    public GameManager GameManager;

    public GameObject helicopter;
    public List<Piece> pieces;


    public void Init()
    {
        if(pieces == null)
        {
            pieces = new List<Piece>();
        }
        
        foreach (Piece p in pieces)
        {
            p.ResetPBlocs();
        }
        pieces.Clear();
    }

    public void addPiece(Piece p)
    {
        pieces.Add(p);
        position = new Vector3(Random.Range(transform.position.x - 30f, transform.position.x + 30f), 30, Random.Range(transform.position.z - 3f, transform.position.z - 30f));
        CreateModel(p, position, Quaternion.identity);
    }


    void CreateModel(Piece piece, Vector3 position, Quaternion rotation)
    {

        foreach (Vector3 blocPos in piece.blocs)
        {
            piece.pBlocs.Add((GameObject)Instantiate(bloc, blocPos*blocSize+position, Quaternion.identity));
            piece.pBlocs[piece.pBlocs.Count - 1].AddComponent<Rigidbody>();
            if (piece.pBlocs.Count > 1)
            {
                piece.pBlocs[piece.pBlocs.Count - 2].AddComponent<FixedJoint>().connectedBody =
                    piece.pBlocs[piece.pBlocs.Count - 1].GetComponent<Rigidbody>();
            }
            
        }
    }

    // Update is called once per frame
    void Update () {

        if(GameManager.GameState == GameManager.GameStates.Factory)
        {
            if (pieces.Count < 4)
            {
                try
                {
                    addPiece(GameManager.nextPieces.Dequeue());
                }
                catch (System.InvalidOperationException e)
                {
                    //TODO Notify Gamemager
                }

            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                foreach (Piece piece in pieces)
                {
                    foreach (GameObject o in piece.pBlocs)
                    {
                        if (o.GetComponent<Collider>().Raycast(ray, out hit, 2000f))
                        {
                            helicopter.GetComponent<Helicopter>().Move(o, 5f);
                        }
                    }
                }
            }
        }
        
	}
}

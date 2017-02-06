using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Piece
{

    public static List<Piece> pieces = new List<Piece>() { };
    public List<Vector3> blocs;
    public List<GameObject> pBlocs;


    public GameObject helicopter;


    public Piece()
    {
        blocs = new List<Vector3>();
        pBlocs = new List<GameObject>();
        pieces.Add(this);
    }

    public Vector3 this[int index]
    {
        get { return blocs[index]; }

    }

    public Piece(Piece piece) : this()
    {
        blocs.AddRange(piece.blocs);
    }

    public Piece(string definedPiece) : this()
    {

        switch (definedPiece)
        {
            case "L":
                blocs.Add(new Vector3(0, 2, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(0, 0, 0));
                blocs.Add(new Vector3(1, 0, 0));
                break;
            case "I":
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(0, 0, 0));
                break;
            case "U":
                blocs.Add(new Vector3(0, 2, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(0, 0, 0));
                blocs.Add(new Vector3(1, 0, 0));
                blocs.Add(new Vector3(2, 2, 0));
                blocs.Add(new Vector3(2, 1, 0));
                blocs.Add(new Vector3(2, 0, 0));
                break;
            case "T":
                blocs.Add(new Vector3(0, 2, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(0, 0, 0));
                blocs.Add(new Vector3(1, 1, 0));
                blocs.Add(new Vector3(2, 1, 0));
                break;
            case "Cross":
                blocs.Add(new Vector3(1, 2, 0));
                blocs.Add(new Vector3(1, 1, 0));
                blocs.Add(new Vector3(1, 0, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(2, 1, 0));
                break;
            case "Cube2":
                blocs.Add(new Vector3(0, 0, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(1, 0, 0));
                blocs.Add(new Vector3(1, 1, 0));
                blocs.Add(new Vector3(0, 0, 1));
                blocs.Add(new Vector3(0, 1, 1));
                blocs.Add(new Vector3(1, 0, 1));
                blocs.Add(new Vector3(1, 1, 1));
                break;
            case "Cube1":
                blocs.Add(new Vector3(0, 0, 0));
                break;
            case "Rect2x2x1":
                blocs.Add(new Vector3(0, 0, 0));
                blocs.Add(new Vector3(0, 1, 0));
                blocs.Add(new Vector3(1, 0, 0));
                blocs.Add(new Vector3(1, 1, 0));
                break;

            default:
                return;


        }

        

    }

    public int GetSize()
    {
        return blocs.Count;
    }

    public void Rotate(Vector3 rot)
    {
        MatrixOperation(Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(rot.x, rot.y, rot.z), new Vector3(1, 1, 1)));
    }

    public bool Translate()
    {
        helicopter.GetComponent<Helicopter>().Move(pBlocs[0], 5f);
        return true;

    }

    public float GetLowerPosition(float x, float z)
    {
        float lower = 1000;
        foreach (Vector3 bloc in blocs)
        {

            if (bloc.x == x && bloc.z == z)
            {
                lower = Math.Min((int)lower, (int)bloc.y);
            }
        }

        return lower;
    }

    private void MatrixOperation(Matrix4x4 m)
    {
        for (int i = 0; i < blocs.Count; i++)
        {
            blocs[i] = m.MultiplyPoint3x4(blocs[i]);
            blocs[i] = new Vector3((float)Math.Round(blocs[i].x), (float)Math.Round(blocs[i].y), (float)Math.Round(blocs[i].z));
        }
    }

    public void InstantiatePiece(Vector3 position, Quaternion rotation, double size, GameObject model)
    {
        foreach(Vector3 bloc in blocs)
        {
            pBlocs.Add((GameObject)GameObject.Instantiate(model, bloc + rotation * position, rotation));
            pBlocs[pBlocs.Count - 1].AddComponent<Rigidbody>();
            if (pBlocs.Count > 1)
            {
                pBlocs[pBlocs.Count - 2].AddComponent<FixedJoint>().connectedBody =
                    pBlocs[pBlocs.Count - 1].GetComponent<Rigidbody>();
            }
        }

    }

    public void ResetPBlocs()
    {
        foreach(GameObject bloc in pBlocs)
        {
            GameObject.Destroy(bloc);
        }
        pBlocs.Clear();
    }

    static public Piece Search(GameObject cube)
    {
        foreach (Piece piece in pieces)
        {
            
            foreach (GameObject bloc in piece.pBlocs)
            {
                if(ReferenceEquals(cube, bloc))
                {
                    return piece;
                }
            }
        }

        return null;
    }

}
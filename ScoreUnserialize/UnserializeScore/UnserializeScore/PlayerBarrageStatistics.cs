using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class PlayerBarrageStatistics
{
    public int sequenceID;
    private string[] levelName = new string[100];
    private bool[] levelSuccess = new bool[100];
    private float[] time = new float[100];
    private int[] nMissingBlocs = new int[100];
    private int[] nTransport = new int[100];
    private int[] nPiece = new int[100];
    private float[] averagePieceSize = new float[100];


    public PlayerBarrageStatistics()
    {
        sequenceID = 0;
        Array.Clear(levelName, 0, levelName.Length);
        Array.Clear(levelSuccess, 0, levelSuccess.Length);
        Array.Clear(time, 0, time.Length);
        Array.Clear(nMissingBlocs, 0, nMissingBlocs.Length);
        Array.Clear(nTransport, 0, nTransport.Length);
        Array.Clear(nPiece, 0, nPiece.Length);
        Array.Clear(averagePieceSize, 0, averagePieceSize.Length);
    }

    public void SetData(string levelName, bool levelSuccess, float time, int nMissingBlocs, int nTransport, int nPiece, float averagePieceSize)
    {
        this.levelName[sequenceID] = levelName;
        this.levelSuccess[sequenceID] = levelSuccess;
        this.time[sequenceID] = time;
        this.nMissingBlocs[sequenceID] = nMissingBlocs;
        this.nTransport[sequenceID] = nTransport;
        this.nPiece[sequenceID] = nPiece;
        this.averagePieceSize[sequenceID] = averagePieceSize;

        this.sequenceID++;
    }

    public static void SaveBarrageData(PlayerBarrageStatistics p)
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/save_barrage.binary");
        formatter.Serialize(saveFile, p);
        saveFile.Close();
    }

    public static PlayerBarrageStatistics LoadBarrageData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Binder = new PreMergeToMergedDeserializationBinder();
        FileStream saveFile = File.Open("save_barrage.binary",
            FileMode.Open);

        PlayerBarrageStatistics p =
            (PlayerBarrageStatistics)formatter.Deserialize(saveFile);
        saveFile.Close();
        return p;
    }

    public int GetNSequenceID()
    {
        return sequenceID;
    }

    public string GetLevelName(int id)
    {
        return this.levelName[id];
    }

    public bool GetLevelSuccess(int id)
    {
        return this.levelSuccess[id];
    }

    public float GetTime(int id)
    {
        return time[id];
    }
   
    public int GetNMissingBlocs(int id)
    {
        return nMissingBlocs[id];
    }

    public int GetNTransport(int id)
    {
        return nTransport[id];
    }

    public int GetNPiece(int id)
    {
        return nPiece[id];
    }

    public float GetAveragePieceSize(int id)
    {
        return averagePieceSize[id];
    }

    
}

sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;

        // For each assemblyName/typeName that you want to deserialize to
        // a different type, set typeToDeserialize to the desired type.
        String exeAssembly = Assembly.GetExecutingAssembly().FullName;


        // The following line of code returns the type.
        typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
            typeName, exeAssembly));

        return typeToDeserialize;
    }
}

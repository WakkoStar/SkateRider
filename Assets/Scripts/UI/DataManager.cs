using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;


[Serializable]
public class Wrapper<T>
{
    public List<T> items;
}

public static class DataManager
{
    static string filePath = Application.persistentDataPath + "/dataToSave.dat";

    public static void SaveData<T>(Wrapper<T> itemWrapper)
    {
        FileStream fs = new FileStream(filePath, FileMode.Create);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        try
        {
            binaryFormatter.Serialize(fs, itemWrapper);
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
    }

    public static List<T> LoadData<T>()
    {
        if (!File.Exists(filePath)) return default(List<T>);

        Wrapper<T> itemWrapper = null;


        Stream fs = new FileStream(filePath, FileMode.Open);
        fs.Position = 0;
        try
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            itemWrapper = (Wrapper<T>)binaryFormatter.Deserialize(fs);

            return itemWrapper.items;
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();

        }

    }
}

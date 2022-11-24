using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public enum CustomableType
{
    All = -1,
    Wheels = 0,
    Trucks = 1,
    Board = 2,
    Grip = 3
}

[Serializable]
public class Customable
{
    public CustomableType type;
    public Texture2D textureToApply;
    public Texture2D thumbnail;
    public int price;
    public string nameId;

    public Customable(CustomableType type, Texture2D textureToApply, Texture2D thumbnail, int price, string nameId)
    {
        this.type = type;
        this.textureToApply = textureToApply;
        this.thumbnail = thumbnail;
        this.price = price;
        this.nameId = nameId;
    }

    static public List<SerializableCustomable> SerializeCustomables(List<Customable> customables)
    {
        var serializableCustomables = customables.Select(customable => SerializeCustomable(customable)).ToList();
        return serializableCustomables;
    }

    static public SerializableCustomable SerializeCustomable(Customable product)
    {
        return new SerializableCustomable(
               product.type,
               product.textureToApply.GetRawTextureData(),
               product.thumbnail.GetRawTextureData(),
               product.price,
               product.nameId
        );
    }
}

[Serializable]
public class SerializableCustomable
{
    public CustomableType type;
    public byte[] textureToApply;
    public byte[] thumbnail;
    public int price;
    public string nameId;

    public SerializableCustomable(CustomableType type, byte[] textureToApply, byte[] thumbnail, int price, string nameId)
    {
        this.type = type;
        this.textureToApply = textureToApply;
        this.thumbnail = thumbnail;
        this.price = price;
        this.nameId = nameId;
    }
}


using UnityEngine;
using System;

public enum ProductType
{
    Wheels = 0,
    Trucks = 1,
    Board = 2,
    Grip = 3
}

[Serializable]
public class Product
{
    public ProductType type;
    public Texture2D textureToApply;
    public Texture2D thumbnail;
    public int price;
    public string nameId;

    public Product(ProductType type, Texture2D textureToApply, Texture2D thumbnail, int price, string nameId)
    {
        this.type = type;
        this.textureToApply = textureToApply;
        this.thumbnail = thumbnail;
        this.price = price;
        this.nameId = nameId;
    }
}

[Serializable]
public class SerializableProduct
{
    public ProductType type;
    public byte[] textureToApply;
    public byte[] thumbnail;
    public int price;
    public string nameId;

    public SerializableProduct(ProductType type, byte[] textureToApply, byte[] thumbnail, int price, string nameId)
    {
        this.type = type;
        this.textureToApply = textureToApply;
        this.thumbnail = thumbnail;
        this.price = price;
        this.nameId = nameId;
    }
}


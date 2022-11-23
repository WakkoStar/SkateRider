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
    public string textureFileName;
    public string thumbnailFileName;
    public int price;
    public string nameId;

    public Product(ProductType type, string textureFileName, string thumbnailFileName, int price, string nameId)
    {
        this.type = type;
        this.textureFileName = textureFileName;
        this.thumbnailFileName = thumbnailFileName;
        this.price = price;
        this.nameId = nameId;
    }
}

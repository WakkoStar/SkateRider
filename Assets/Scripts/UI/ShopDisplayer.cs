using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject ProductElement;
    public Product[] products;
    private List<Product> _filteredProducts;

    public UnityEvent OnBuyAccept;
    public UnityEvent OnBuyDeny;

    private SerializableProduct _wantedProduct;

    private List<SerializableProduct> _inventory;
    // Start is called before the first frame update
    void Start()
    {
        _inventory = DataManager.LoadData<SerializableProduct>("inventory");

        DisplayAll();
    }

    public void FilterBy(int type)
    {
        DisplayProducts(SerializeProducts(products.ToList().FindAll(p => p.type == (ProductType)type)));
    }

    public void DisplayAll()
    {
        DisplayProducts(SerializeProducts(products.ToList()));
    }

    public void DisplayProducts(List<SerializableProduct> filteredProducts)
    {
        DestroyChilds();

        foreach (var product in filteredProducts)
        {
            var productDisplayer = Instantiate(ProductElement, transform).GetComponent<ProductDisplayer>();
            productDisplayer.SetProduct(product);
            productDisplayer.gameObject.GetComponent<Button>().onClick.AddListener(() => Buy(product));

            //IF PLAYER PREFS CONTAINS 
            if (_inventory.Find(item => product.nameId == item.nameId) != null)
            {
                productDisplayer.SetPurchased();
            }
        }
    }

    void DestroyChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void Buy(SerializableProduct product)
    {
        var collectableCount = PlayerPrefs.GetInt("collectibleCount");
        if (collectableCount >= product.price)
        {
            OnBuyAccept.Invoke();
            _wantedProduct = product;
            //AFFICHER CONFIRMATION
        }
        else
        {
            OnBuyDeny.Invoke();
            //AFFICHER MESSAGE D'ERREUR
        }
    }

    public void UpdateShop()
    {
        _inventory = DataManager.LoadData<SerializableProduct>("inventory");
        DisplayAll();
    }

    public void ConfirmBuy()
    {
        PlayerPrefs.SetInt("collectibleCount", PlayerPrefs.GetInt("collectibleCount") - _wantedProduct.price);

        _wantedProduct.price = -1;
        DataManager.AddToData<SerializableProduct>(_wantedProduct, "inventory");

        _inventory = DataManager.LoadData<SerializableProduct>("inventory");

        DisplayAll();
    }

    List<SerializableProduct> SerializeProducts(List<Product> products)
    {
        var serializableProducts = products.Select(item =>
           {
               return new SerializableProduct(
               item.type,
               item.textureToApply.GetRawTextureData(),
               item.thumbnail.GetRawTextureData(),
               item.price,
               item.nameId
               );

           }).ToList();

        return serializableProducts;
    }

    public SerializableProduct SerializeProduct(Product product)
    {
        return new SerializableProduct(
               product.type,
               product.textureToApply.GetRawTextureData(),
               product.thumbnail.GetRawTextureData(),
               product.price,
               product.nameId
        );
    }
}

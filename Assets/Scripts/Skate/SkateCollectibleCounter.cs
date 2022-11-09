

public class SkateCollectibleCounter
{
    public GUIController guiController;
    private int _collectibleCount = 0;

    //COLLECTIBLE 
    public void AddCollectible()
    {
        IncrementCollectible();
        guiController.DisplayCollectibleScore(GetCollectibleCount());
    }

    private void IncrementCollectible()
    {
        _collectibleCount += 1;
    }
    private int GetCollectibleCount()
    {
        return _collectibleCount;
    }

}

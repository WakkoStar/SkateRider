

public class SkateCollectibleCounter
{
    public SkateStateManager skateState;
    private int _collectibleCount = 0;

    //COLLECTIBLE 
    public void AddCollectible()
    {
        IncrementCollectible();
        skateState.GetSkateMainScreen().DisplayCollectibleScore(GetCollectibleCount());
    }

    public void ResetCollectible()
    {
        _collectibleCount = 0;
    }

    public void IncrementCollectible(int value = 1)
    {
        _collectibleCount += value;
    }

    public int GetCollectibleCount()
    {
        return _collectibleCount;
    }


    public void SetStartGame()
    {
        ResetCollectible();
        skateState.GetSkateMainScreen().DisplayCollectibleScore(GetCollectibleCount());
    }

}

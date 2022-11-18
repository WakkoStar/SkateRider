

public class SkateCollectibleCounter
{
    public SkateStateManager skateState;
    private int _collectibleCount = 0;

    //COLLECTIBLE 
    public void AddCollectible()
    {
        IncrementCollectible();
        skateState.GetSkateMainScreen().DisplayCollectibleScore(GetCollectibleCount());//A VIRER
    }

    public void ResetCollectible()
    {
        _collectibleCount = 0;
    }

    private void IncrementCollectible()
    {
        _collectibleCount += 1;
    }
    private int GetCollectibleCount()
    {
        return _collectibleCount;
    }

    public void SetRestartGame()
    {
        ResetCollectible();
    }

}

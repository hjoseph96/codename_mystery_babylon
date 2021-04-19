public class AnimatedTileConfigurationInfluenceZone : InfluenceZone
{
    public TileConfiguration Config;

    private void Awake() => SetInvisible();

    public void Apply()
    {
        var rect = GetWorldRectInt();
        foreach (var pos in rect.allPositionsWithin)
        {
            //WorldGrid.Instance[pos - WorldGrid.Instance.Origin].SetTileConfig(Config);
        }
    }
}

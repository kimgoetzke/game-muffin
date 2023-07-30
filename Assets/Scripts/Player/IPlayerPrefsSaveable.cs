namespace CaptainHindsight.Player
{
  public interface IPlayerPrefsSaveable
  {
    void TryLoadPlayerPrefs();

    void TrySavePlayerPrefs(string spawnPointName);
  }
}
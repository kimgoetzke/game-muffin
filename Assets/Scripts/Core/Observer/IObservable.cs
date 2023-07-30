namespace CaptainHindsight.Core.Observer
{
  public interface IObservable
  {
    public void RegisterObserver(Observer observer);
  }
}
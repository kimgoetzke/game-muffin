using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Core.Observer
{
  public abstract class Observer : MonoBehaviour
  {
    [TitleGroup("Configuration")] [SerializeField]
    private bool isObservingThis = true;

    [HideIf("isObservingThis", true)] [SerializeField]
    private GameObject observedObject;

    protected virtual void Awake()
    {
      if (isObservingThis) observedObject = gameObject;
    }

    protected virtual void Start()
    {
      observedObject.GetComponent<IObservable>().RegisterObserver(this);
    }

    // This method can be used in cases where the observers Start() method is called too late and
    // the IObservable might have already sent information. Note that the registration process
    // cannot be moved to Awake() due to the Script Execution Order. It is not guaranteed that the
    // IObservable exists before the Observer.
    protected void RegisterObserverManually()
    {
      observedObject.GetComponent<IObservable>().RegisterObserver(this);
    }

    public virtual void ProcessInformation()
    {
    }
  }

  public abstract class OriginObserver : Observer
  {
    public abstract void ProcessInformation(Transform origin);
  }

  public abstract class BoolObserver : Observer
  {
    public abstract void ProcessInformation(bool isTrue);
  }
}
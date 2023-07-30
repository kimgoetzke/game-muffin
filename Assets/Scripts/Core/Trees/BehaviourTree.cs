using UnityEngine;

namespace CaptainHindsight.Core.Trees
{
  public abstract class BehaviourTree : MonoBehaviour
  {
    private Node _root = null;

    protected void Start()
    {
      _root = SetupTree();
    }

    private void Update()
    {
      _root?.Evaluate();
    }

    protected abstract Node SetupTree();
  }
}
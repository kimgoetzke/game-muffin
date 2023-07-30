using System.Collections.Generic;

namespace CaptainHindsight.Core.Trees
{
  public enum NodeState
  {
    Running,
    Success,
    Failure
  }

  public class Node
  {
    protected NodeState State;

    public Node Parent;
    protected List<Node> Children = new();

    private Dictionary<string, object> _dataContext = new();

    public Node()
    {
      Parent = null;
    }

    public Node(List<Node> children)
    {
      foreach (var child in children)
        _Attach(child);
    }

    private void _Attach(Node node)
    {
      node.Parent = this;
      Children.Add(node);
    }

    public virtual NodeState Evaluate()
    {
      return NodeState.Failure;
    }

    public void SetData(string key, object value)
    {
      _dataContext[key] = value;
    }

    public object GetData(string key)
    {
      object value = null;
      if (_dataContext.TryGetValue(key, out value))
        return value;

      var node = Parent;
      while (node != null)
      {
        value = node.GetData(key);
        if (value != null)
          return value;
        node = node.Parent;
      }

      return null;
    }

    public bool ClearData(string key)
    {
      if (_dataContext.ContainsKey(key))
      {
        _dataContext.Remove(key);
        return true;
      }

      var node = Parent;
      while (node != null)
      {
        var cleared = node.ClearData(key);
        if (cleared)
          return true;
        node = node.Parent;
      }

      return false;
    }
  }
}
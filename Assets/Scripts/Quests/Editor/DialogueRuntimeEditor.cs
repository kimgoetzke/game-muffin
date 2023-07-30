#if UNITY_EDITOR
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEditor;
using UnityEngine;

namespace CaptainHindsight.Quests.Editor
{
  [CustomEditor(typeof(DialogueManager))]
  [InitializeOnLoad]
  public class DialogueRuntimeEditor : UnityEditor.Editor
  {
    // The purpose of this class is to auto-display an Ink Player window in the editor
    // when in Play mode that shows the Ink dialogue. This is useful for trouble shooting 
    // quests. You can enabled/disable this by commenting in/out the event subscription
    // in the constructer below.

    private static bool _storyExpanded;

    static DialogueRuntimeEditor()
    {
      //DialogueManager.OnCreateStory += OnCreateStory;
    }

    private static void OnCreateStory(Story story)
    {
      var window = InkPlayerWindow.GetWindow(true);
      // Change to true if you want the InkPlayer to show automatically
      // when entering Play mode
      if (window != null) InkPlayerWindow.Attach(story);
    }

    public override void OnInspectorGUI()
    {
      Repaint();
      base.OnInspectorGUI();
      var realTarget = target as DialogueManager;
      var story = realTarget.CurrentStory;
      InkPlayerWindow.DrawStoryPropertyField(story, new GUIContent("Story"));
    }
  }
}
#endif
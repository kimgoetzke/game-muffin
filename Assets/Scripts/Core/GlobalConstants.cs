using System;

namespace CaptainHindsight.Core
{
  public static class GlobalConstants
  {
    public const string ACTIVE_SCENE = "SceneActive";
    public const string PREVIOUS_SCENE = "ScenePrevious";
    public const string PREVIOUS_SCENE_EXIT_SPAWN = "ScenePreviousExit";
    public const string UNLOCKED_SKILLS = "PlayerUnlockedSkills";
    public const string CURRENT_LEVEL = "PlayerLevel";
    public const string CURRENT_EXPERIENCE = "PlayerExperience";
    public const string CURRENT_HEALTH = "PlayerHealth";
    public const string CURRENT_MAX_HEALTH = "PlayerMaxHealth";
    public const string SKILL_POINTS = "PlayerSkillPoints";
    public const string AUDIO_SFX_VOLUME = "AudioSfxVolume";
    public const string AUDIO_MUSIC_VOLUME = "AudioMusicVolume";
    public const string ALL_QUESTS = "QuestsAll";
    public const string DIALOGUE_VARIABLES = "QuestDialogueVariables";

    public const string AUDIO_SFX_MIXER = "SFX";
    public const string AUDIO_MUSIC_MIXER = "Music";

    // Lists all constants used in PlayerPrefsManager which are associated with string values.
    // Allows for easy iteration over all constants in the context of logging current values.
    public static void ForEachStringConstant(Action<object> value)
    {
      value(ACTIVE_SCENE);
      value(PREVIOUS_SCENE);
      value(PREVIOUS_SCENE_EXIT_SPAWN);
      value(ALL_QUESTS);
      value(UNLOCKED_SKILLS);
      value(DIALOGUE_VARIABLES);
    }

    // Lists all constants used in PlayerPrefsManager which are associated with int values.
    // Allows for easy iteration over all constants in the context of logging current values.
    public static void ForEachIntConstant(Action<object> value)
    {
      value(CURRENT_LEVEL);
      value(CURRENT_EXPERIENCE);
      value(CURRENT_HEALTH);
      value(CURRENT_MAX_HEALTH);
      value(SKILL_POINTS);
    }

    // Lists all constants used in PlayerPrefsManager which are associated with float values.
    // Allows for easy iteration over all constants in the context of logging current values.
    public static void ForEachFloatConstant(Action<object> value)
    {
      value(AUDIO_SFX_VOLUME);
      value(AUDIO_MUSIC_VOLUME);
    }
  }
}
using System.Globalization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class MHealthBar : MonoBehaviour
  {
    [SerializeField] private Slider slider;
    [SerializeField] private Image image;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Label labelSetting;

    [SerializeField]
    [ShowIf("@this.labelSetting == Label.Health || this.labelSetting == Label.Both")]
    private TextMeshProUGUI healthMesh;

    [SerializeField] [ShowIf("@this.labelSetting == Label.Name || this.labelSetting == Label.Both")]
    private TextMeshProUGUI nameMesh;

    [SerializeField] private TextMeshProUGUI indicatorMesh;

    private enum Label
    {
      None,
      Name,
      Health,
      Both
    }

    public void ConfigureBar(int maxHealth, string objName = "")
    {
      // Set colour and current/maxValue
      image.color = gradient.Evaluate(1);
      slider.maxValue = maxHealth;
      slider.value = maxHealth;

      // Set or disable healthMesh (numeric values shown on the bar)
      if (labelSetting is Label.Health or Label.Both) SetHealthMesh(maxHealth);
      else if (healthMesh != null) healthMesh.enabled = false;

      // Set or disable nameMesh (string shown above the bar)
      if (labelSetting is Label.Name or Label.Both) SetNameMesh(objName);
      else if (nameMesh != null) nameMesh.enabled = false;

      // Disable indicatorMesh (used to indicate interaction options)
      indicatorMesh.enabled = false;
    }

    public void SetMaxHealth(int maxHealth)
    {
      slider.maxValue = maxHealth;

      // Set or disable healthMesh (numeric values shown on the bar)
      if (labelSetting is Label.Health or Label.Both) SetHealthMesh((int)slider.value);
      else if (healthMesh != null) healthMesh.enabled = false;
    }

    public void SetHealth(int health)
    {
      slider.value = health;
      var normalisedValue = slider.value / slider.maxValue;
      image.color = gradient.Evaluate(normalisedValue);
      if (slider.value > slider.maxValue) slider.value = slider.maxValue;
      if (labelSetting is Label.Health or Label.Both) SetHealthMesh(health);
    }

    private void SetNameMesh(string objName)
    {
      nameMesh.text = objName;
    }

    private void SetHealthMesh(int health)
    {
      healthMesh.text = health + " / " + slider.maxValue.ToString(CultureInfo.InvariantCulture);
    }
  }
}
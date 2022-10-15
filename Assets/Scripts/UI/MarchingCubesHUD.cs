using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarchingCubesHUD : MonoBehaviour
{
  [SerializeField] Image healthBar = null;
  [SerializeField] TextMeshProUGUI healthText = null;
  [SerializeField] SubmarineControl submarine = null;

  private float health;
  private float maxHealth;

  void Update()
  {
    if (submarine != null)
    {
      health = submarine.GetHealth();
      maxHealth = submarine.GetMaxHealth();
    }
    if (healthBar != null)
    {
      healthBar.fillAmount = health / maxHealth;
    }
    if (healthText != null)
    {
      healthText.text = health.ToString("0");// + " / " + maxHealth.ToString("0");
    }
  }
}

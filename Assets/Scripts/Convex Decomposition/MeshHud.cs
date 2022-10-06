using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeshHud : MonoBehaviour
{
  [SerializeField] ConvexMeshBuilder convexMeshBuilder = null;
  [SerializeField] TextMeshProUGUI volumeText = null;
  [SerializeField] TextMeshProUGUI hullVolumeText = null;
  [SerializeField] TextMeshProUGUI concavityText = null;

  void Update()
  {
    volumeText.text = "Volume: " + convexMeshBuilder.GetVolume().ToString("0.00");
    hullVolumeText.text = "Hull Volume: " + convexMeshBuilder.GetHullVolume().ToString("0.00");
    concavityText.text = "Concavity: " + convexMeshBuilder.GetConcavity().ToString("0.00");
  }
}

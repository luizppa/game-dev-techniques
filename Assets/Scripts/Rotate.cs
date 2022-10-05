using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
  [SerializeField] float speed = 1f;
  [SerializeField] Vector3 axis = Vector3.up;
  private bool play = true;

  void Update()
  {
    if (play)
    {
      transform.Rotate(axis, speed * Time.deltaTime * 50);
    }
  }

  public void SetSpeed(float speed)
  {
    this.speed = speed;
  }

  public void Play()
  {
    play = true;
  }

  public void Stop()
  {
    play = false;
  }
}

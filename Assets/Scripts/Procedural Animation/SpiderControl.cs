using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderControl : MonoBehaviour
{
  private Rigidbody rb = null;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  void Update()
  {
    Move();
  }

  void Move()
  {
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");
    Vector3 direction = new Vector3(horizontal, 0, vertical);
    rb.MovePosition(transform.position + direction * 10f * Time.deltaTime);
  }
}

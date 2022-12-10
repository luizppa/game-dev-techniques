using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum TerraformMode
{
    Add = 1,
    Remove = -1
}

[RequireComponent(typeof(LineRenderer))]
public class TerraformController : MonoBehaviour
{

  [Header("Terraform")]
	[SerializeField] TerraformMode mode = TerraformMode.Remove;
	[SerializeField] float terraformRange = 10f;
  [SerializeField] float terraformRadius = 5f;
  [SerializeField] float terraformStrength = 0.5f;
  [SerializeField] float terraformInterval = 0.1f;
	[SerializeField] LayerMask terraformLayer;

	[Header("Laser")]
	[SerializeField] Color addColor = Color.green;
	[SerializeField] Color removeColor = Color.red;
	[SerializeField] float laserSpeed = 10f;

	[Header("UI")]
	[SerializeField] Image modeImage = null;
	[SerializeField] Sprite addSprite = null;
	[SerializeField] Sprite removeSprite = null;

  private bool canTerraform = true;
	private SurfaceManager surfaceManager = null;
	private LineRenderer lineRenderer = null;
	private RaycastHit hit;
	private bool isHit = false;
	
	void Start()
	{
		surfaceManager = SurfaceManager.Instance;
		lineRenderer = GetComponent<LineRenderer>();
	}

	void Update()
	{
		if(!Application.isPlaying || Time.timeScale == 0){
			return;
		}

		CalculateHit();

		if(Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton3)){
			ToggleMode();
		}
		if(CheckInput()){
			Terraform();
		}
		else{
			ClearTerraformEffect();
		}
		UpdateUi();
	}

	void CalculateHit(){
		isHit = Physics.Raycast(transform.position, Camera.main.transform.forward, out hit, terraformRange, terraformLayer);
	}

	void ToggleMode(){
		if(mode == TerraformMode.Add){
			mode = TerraformMode.Remove;
		}else{
			mode = TerraformMode.Add;
		}
	}

	void UpdateUi()
	{
		if(mode == TerraformMode.Add){
			modeImage.sprite = addSprite;
		}else{
			modeImage.sprite = removeSprite;
		}
	}

	bool CheckInput()
	{
		return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.JoystickButton5);
	}

	void Terraform()
	{
		if (isHit)
		{
			DrawTerraformEffect(hit.point);
			
			if(canTerraform){
				List<GPUChunk> chunks = surfaceManager.GetChunksInRadius(hit.point, terraformRadius);
				foreach (GPUChunk chunk in chunks)
				{
					chunk.Terraform(hit.point, terraformRadius, terraformStrength, mode);
				}
				canTerraform = false;
				StartCoroutine(TerraformCooldown());
			}
		}
		else{
			ClearTerraformEffect();
		}
  }

  void DrawTerraformEffect(Vector3 position)
  {
		Color color = mode == TerraformMode.Add ? addColor : removeColor;
		float speed = mode == TerraformMode.Add ? -laserSpeed : laserSpeed;

		lineRenderer.material.SetVector("_LaserOrigin", transform.position);
		lineRenderer.material.SetColor("_Color", color);
		lineRenderer.material.SetFloat("_LaserSpeed", speed * Time.timeScale);
		lineRenderer.positionCount = 2;
    lineRenderer.SetPositions(new Vector3[] { transform.position, position });
  }

	void ClearTerraformEffect(){
		lineRenderer.positionCount = 0;
		lineRenderer.SetPositions(new Vector3[] {});
	}

  IEnumerator TerraformCooldown()
  {
    yield return new WaitForSeconds(terraformInterval);
    canTerraform = true;
  }

	public Vector3 GetHitPosition(){
		if(isHit){
			return hit.point;
		}
		else{
			return Vector3.zero;
		}
	}

	public float GetHitDistance(){
		if(isHit){
			return hit.distance;
		}
		else{
			return 0f;
		}
	}

	public bool IsHit(){
		return isHit;
	}

	public float GetRange(){
		return terraformRange;
	}
}

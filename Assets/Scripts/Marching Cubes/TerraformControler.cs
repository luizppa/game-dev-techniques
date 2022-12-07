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
public class TerraformControler : MonoBehaviour
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

	[Header("UI")]
	[SerializeField] Image modeImage = null;
	[SerializeField] Sprite addSprite = null;
	[SerializeField] Sprite removeSprite = null;

  private bool canTerraform = true;
	private SurfaceManager surfaceManager = null;
	private LineRenderer lineRenderer = null;
	
	void Start()
	{
		surfaceManager = SurfaceManager.Instance;
		lineRenderer = GetComponent<LineRenderer>();
	}

	void Update()
	{
		if(!Application.isPlaying){
			return;
		}

		if(Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button3)){
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
		return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Joystick1Button5);
	}

	void Terraform()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit, terraformRange, terraformLayer))
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

		lineRenderer.material.SetVector("_LaserOrigin", transform.position);
		lineRenderer.material.SetColor("_Color", color);
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
}

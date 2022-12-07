using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum TerraformMode
{
    Add = 1,
    Remove = -1
}

public class TerraformControler : MonoBehaviour
{

  [Header("Terraform")]
	[SerializeField] TerraformMode mode = TerraformMode.Remove;
  [SerializeField] float terraformRadius = 5f;
  [SerializeField] float terraformStrength = 0.5f;
  [SerializeField] float terraformInterval = 0.1f;
	[SerializeField] LayerMask terraformLayer;

	[Header("UI")]
	[SerializeField] Image modeImage = null;
	[SerializeField] Sprite addSprite = null;
	[SerializeField] Sprite removeSprite = null;

  private bool canTerraform = true;
	private SurfaceManager surfaceManager = null;
	
	void Start()
	{
		surfaceManager = SurfaceManager.Instance;
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F)){
			ToggleMode();
		}
		if(Application.isPlaying && canTerraform && CheckInput()){
			Terraform();
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
		return Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Joystick1Button8);
	}

	void Terraform()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Camera.main.transform.forward, out hit, 10f, terraformLayer))
		{
			// List<GPUChunk> chunks = GetTerraformAffectedChunks(hit.point);
			List<GPUChunk> chunks = surfaceManager.GetChunksInRadius(hit.point, terraformRadius);
			foreach (GPUChunk chunk in chunks)
			{
				chunk.Terraform(hit.point, terraformRadius, terraformStrength, mode);
			}
			canTerraform = false;
			StartCoroutine(TerraformCooldown());
		}
  }

  void DrawTerraformEffect(Vector3 position)
  {
    // lineRenderer.SetPositions(new Vector3[] { transform.position, position });
  }

  List<GPUChunk> GetTerraformAffectedChunks(Vector3 position)
  {
    List<GPUChunk> chunks = new List<GPUChunk>();
    foreach (Collider collider in Physics.OverlapSphere(position, terraformRadius, LayerMask.GetMask("Terrain")))
    {
      GPUChunk chunk = collider.gameObject.GetComponent<GPUChunk>();
      if (chunk != null)
      {
        chunks.Add(chunk);
      }
    }
    return chunks;
  }

  IEnumerator TerraformCooldown()
  {
    yield return new WaitForSeconds(terraformInterval);
    canTerraform = true;
  }
}

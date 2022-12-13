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
	[SerializeField] GameObject addParticlesPrefab = null;
	[SerializeField] GameObject removeParticlesPrefab = null;

	[Header("UI")]
	[SerializeField] Image modeImage = null;
	[SerializeField] Sprite addSprite = null;
	[SerializeField] Sprite removeSprite = null;

  private bool canTerraform = true;
	private SurfaceManager surfaceManager = null;
	private LineRenderer lineRenderer = null;
	private RaycastHit hit;
	private GameObject particles = null;
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

		if(particles != null && isHit){
			ClearParticleEffect();
			UpdateParticleEffect(hit.point);
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
		
		UpdateParticleEffect(position);
  }

	void UpdateParticleEffect(Vector3 position){
		Vector3 particlesPosition = Vector3.MoveTowards(position, transform.position, 0.5f);

		if(particles == null){
			GameObject particlesPrefab = mode == TerraformMode.Add ? addParticlesPrefab : removeParticlesPrefab;
			particles = Instantiate(particlesPrefab, particlesPosition, Quaternion.identity);
		}
		else{
			particles.transform.position = particlesPosition;
		}
	}

	void ClearTerraformEffect(){
		lineRenderer.positionCount = 0;
		lineRenderer.SetPositions(new Vector3[] {});
		ClearParticleEffect();
	}

	void ClearParticleEffect(){
		if(particles == null){
			return;
		}
		ParticleSystem particleComponent = particles.GetComponent<ParticleSystem>();
		particleComponent.Stop();
		particles = null;
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

using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
	[SerializeField] TerraformController terraformController = null;
	[SerializeField] Image leftCrosshairArrow = null;
	[SerializeField] Image rightCrosshairArrow = null;
	[SerializeField] Image noTargetIcon = null;
	[SerializeField] float crosshairCloseScale = 1f;
	[SerializeField] float crosshairFarScale = 0.5f;

	// Start is called before the first frame update
	void Start()
	{
			
	}

	// Update is called once per frame
	void Update()
	{
		UpdatePosition();
	}

	void UpdatePosition(){
		if(terraformController.IsHit()){
			Vector3 rayHit = Camera.main.WorldToScreenPoint(terraformController.GetHitPosition());
			float scaleLerp = terraformController.GetHitDistance()/terraformController.GetRange();
			float scale = Mathf.Lerp(crosshairCloseScale, crosshairFarScale, scaleLerp);

			transform.position = new Vector3(Screen.width/2f, rayHit.y, 0f);
			leftCrosshairArrow.enabled = true;
			rightCrosshairArrow.enabled = true;
			noTargetIcon.enabled = false;
			transform.localScale = new Vector3(scale, scale, 1);
		}
		else{
			transform.position = new Vector3(Screen.width/2f, Screen.height/2f, 0);
			leftCrosshairArrow.enabled = false;
			rightCrosshairArrow.enabled = false;
			noTargetIcon.enabled = true;
			transform.localScale = new Vector3(1, 1, 1);
		}
	}
}

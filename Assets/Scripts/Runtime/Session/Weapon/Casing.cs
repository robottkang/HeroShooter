using UnityEngine;
using Cysharp.Threading.Tasks;

public class Casing : MonoBehaviour
{
	[Header("Force X")]
	public float minXForce;
	public float maxXForce;
	[Header("Force Y")]
	public float minYForce;
	public float maxYForce;
	[Header("Force Z")]
	public float minZForce;
	public float maxZForce;
	[Header("Rotation Force")]
	public float minRotation;
	public float maxRotation;
	[Header("Despawn Time")]
	public float despawnTime;

	[Header("Audio")]
	public AudioClip[] casingSounds;
	public AudioSource audioSource;

	[Header("Spin Settings")]
	public float speed = 2500f;

	private void OnEnable()
	{
		GetComponent<Rigidbody>().AddRelativeTorque(
			Random.Range(minRotation, maxRotation),
			Random.Range(minRotation, maxRotation),
			Random.Range(minRotation, maxRotation)
			* Time.deltaTime);

		GetComponent<Rigidbody>().AddRelativeForce(
			Random.Range (minXForce, maxXForce),
			Random.Range (minYForce, maxYForce),
			Random.Range (minZForce, maxZForce));
	}

	private void Start()
	{
		RemoveCasingAsync().Forget();
		transform.rotation = Random.rotation;
		PlaySoundAsync().Forget();
	}

	private void FixedUpdate()
	{
		transform.Rotate(Vector3.right, speed * Time.deltaTime);
		transform.Rotate(Vector3.down, speed * Time.deltaTime);
	}

	private async UniTaskVoid PlaySoundAsync()
	{
		await UniTask.WaitForSeconds(Random.Range(0.25f, 0.85f));
		audioSource.clip = casingSounds[Random.Range(0, casingSounds.Length)];
		audioSource.Play();
	}

	private async UniTaskVoid RemoveCasingAsync()
	{
		await UniTask.WaitForSeconds(despawnTime);
		Destroy(gameObject);
	}
}
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Casing : MonoBehaviour
{
	[Header("Force X")]
	public float minimumXForce;
	public float maximumXForce;
	[Header("Force Y")]
	public float minimumYForce;
	public float maximumYForce;
	[Header("Force Z")]
	public float minimumZForce;
	public float maximumZForce;
	[Header("Rotation Force")]
	public float minimumRotation;
	public float maximumRotation;
	[Header("Despawn Time")]
	public float despawnTime;

	[Header("Audio")]
	public AudioClip[] casingSounds;
	public AudioSource audioSource;

	[Header("Spin Settings")]
	public float speed = 2500.0f;

	private void OnEnable()
	{
		GetComponent<Rigidbody>().AddRelativeTorque(
			Random.Range(minimumRotation, maximumRotation),
			Random.Range(minimumRotation, maximumRotation),
			Random.Range(minimumRotation, maximumRotation)
			* Time.deltaTime);

		GetComponent<Rigidbody>().AddRelativeForce(
			Random.Range (minimumXForce, maximumXForce),
			Random.Range (minimumYForce, maximumYForce),
			Random.Range (minimumZForce, maximumZForce));
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
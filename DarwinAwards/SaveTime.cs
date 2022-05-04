using UnityEngine;

namespace DarwinAwards;

public class SaveTime : MonoBehaviour
{
	public float timeCreated;

	public void Awake()
	{
		timeCreated = Time.fixedTime;
	}
}

using UnityEngine;

namespace DarwinAwards;

public class SaveTime : MonoBehaviour
{
	public float timeCreated;

	public SaveTime()
	{
		timeCreated = Time.fixedTime;
	}
}

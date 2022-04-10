using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DarwinAwards;

public static class Display
{
	private static GameObject? deaths;
	private static GameObject linePrefab = null!;

	private static readonly Dictionary<string, Sprite> iconMap = new();

	[HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
	public class AddDeathList
	{
		private static void Postfix(Hud __instance)
		{
			deaths = new GameObject("deaths", typeof(RectTransform));
			deaths.AddComponent<DragNDrop>();
			deaths.transform.SetParent(__instance.m_rootObject.transform);
			deaths.transform.localPosition = DarwinAwards.deathLogAnchor.Value;

			Vector2 middleLeft = new(0, 0.5f);

			linePrefab = new GameObject("death line", typeof(RectTransform));
			linePrefab.SetActive(false);
			linePrefab.transform.localPosition = Vector3.zero;
			RectTransform lineTransform = (RectTransform)linePrefab.transform;
			lineTransform.pivot = middleLeft;
			lineTransform.anchorMin = middleLeft;
			lineTransform.anchorMax = middleLeft;
			lineTransform.sizeDelta = new Vector2(300, 50);
			Text linePrefabText = linePrefab.AddComponent<Text>();
			linePrefabText.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(x => x.name == "AveriaSerifLibre-Bold");
			linePrefabText.fontSize = 14;
			linePrefabText.alignment = TextAnchor.MiddleLeft;
			linePrefabText.lineSpacing = 0.7f;
			Outline outline = linePrefab.AddComponent<Outline>();
			outline.effectColor = Color.black;
			outline.effectDistance = new Vector2(1, -1);

			GameObject deathIcon = new("Death Icon");
			deathIcon.transform.SetParent(linePrefab.transform);
			deathIcon.AddComponent<Image>();
			RectTransform deathRect = deathIcon.GetComponent<RectTransform>();
			deathRect.sizeDelta = new Vector2(16, 16);
			deathRect.anchoredPosition = new Vector2(-20, 0);
			deathRect.pivot = middleLeft;
			deathRect.anchorMin = middleLeft;
			deathRect.anchorMax = middleLeft;
		}
	}

	public static void FillIconMap()
	{
		iconMap.Add("general", Helper.loadSprite("death-skull.png"));
		iconMap.Add("death by fire", Helper.loadSprite("fire.png"));
		iconMap.Add("death by poison", Helper.loadSprite("poison-bottle.png"));
		iconMap.Add("death by frost", Helper.loadSprite("death-skull.png"));
		iconMap.Add("death by lightning", Helper.loadSprite("lightning.png"));
		iconMap.Add("death by elemental", Helper.loadSprite("minerals.png"));
		iconMap.Add("death by tree", Helper.loadSprite("treedeath.png"));
		iconMap.Add("death by pierce", Helper.loadSprite("pierced-body.png"));
		iconMap.Add("death by slash", Helper.loadSprite("slash.png"));
		iconMap.Add("death by blunt", Helper.loadSprite("blunt.png"));
		iconMap.Add("death by physical", Helper.loadSprite("physical.png"));
		iconMap.Add("death by gravity", Helper.loadSprite("fall.png"));
		iconMap.Add("death by drowning", Helper.loadSprite("drowning.png"));
		iconMap.Add("death by player", Helper.loadSprite("skull-crack.png"));
		iconMap.Add("death by creature", Helper.loadSprite("monsterdeath.png"));
		iconMap.Add("death by boss", Helper.loadSprite("bossdeath.png"));
	}

	public static void AddText(DarwinAwards.DeathText text)
	{
		GameObject textLine = Object.Instantiate(linePrefab, deaths!.transform);
		textLine.SetActive(true);
		((RectTransform)textLine.transform).anchoredPosition = new Vector2(0, 25 * (deaths.transform.childCount - 1));
		textLine.GetComponent<Text>().text = text.text;
		textLine.AddComponent<SaveTime>();
		if (DarwinAwards.timerForDeaths.Value > 0)
		{
			textLine.AddComponent<TimedDestruction>().Trigger(DarwinAwards.timerForDeaths.Value);
		}

		textLine.transform.Find("Death Icon").GetComponent<Image>().sprite = iconMap[text.category];

		if (deaths.transform.childCount > DarwinAwards.numberOfDeaths.Value)
		{
			for (int i = 1; i < deaths.transform.childCount; ++i)
			{
				((RectTransform)deaths.transform.GetChild(i).transform).anchoredPosition = new Vector2(0, 25 * (i - 1));
			}

			Object.Destroy(deaths.transform.GetChild(0).gameObject);
		}
	}

	public static void CheckDeathTimes(object sender, EventArgs e)
	{
		if (deaths is not null)
		{
			for (int i = 0; i < deaths.transform.childCount; ++i)
			{
				Transform deathLine = deaths.transform.GetChild(i);
				if (DarwinAwards.timerForDeaths.Value > 0 && deathLine.GetComponent<SaveTime>().timeCreated + DarwinAwards.timerForDeaths.Value < Time.fixedTime)
				{
					Object.Destroy(deathLine.gameObject);
				}
				else
				{
					TimedDestruction destruction = deaths.transform.GetChild(i).GetComponent<TimedDestruction>();
					destruction?.CancelInvoke(nameof(TimedDestruction.DestroyNow));
					if (DarwinAwards.timerForDeaths.Value > 0)
					{
						(destruction ? destruction : deathLine.gameObject.AddComponent<TimedDestruction>())?.Trigger(DarwinAwards.timerForDeaths.Value - (Time.fixedTime - deathLine.GetComponent<SaveTime>().timeCreated));
					}
				}
			}
		}
	}

	public static void AnchorDeathLog(object sender, EventArgs e)
	{
		if (deaths is not null)
		{
			deaths.transform.localPosition = DarwinAwards.deathLogAnchor.Value;
			deaths.transform.GetComponent<DragNDrop>().SetPosition(deaths.transform.position);
		}
	}
}

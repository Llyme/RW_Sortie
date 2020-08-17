using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_Sortie
{
	public class Settings : ModSettings
	{
		public const string DESCRIPTION =
			"* Items that are NOT CHECKED will not be included when " +
			"picking a random incident.\n" +
			"* Incidents will not be picked if it isn't allowed in the scenario.";
		public const string DESCRIPTION_PROMPT_FIRST =
			"Show a dialog prompt before starting the next incident/event.";
		public const string DESCRIPTION_DISPLAY_IF_QUEUED =
			"Displays the next incident/event if it is queued, " +
			"meaning that it is going to happen eventually. " +
			"Only works if 'Prompt before Starting an Incident/Event' is enabled.";

		public const string PROMPT_FIRST =
			"Prompt before Starting an Incident/Event";
		public const string DISPLAY_IF_QUEUED =
			"Display Next Incident/Event if Queued";

		public static bool PromptFirst = true;
		public static bool DisplayIfQueued = false;
		public static HashSet<string> BlackList = null;

		public static HashSet<IncidentDef> defs = null;
		public static Vector2 scrollVector = new Vector2();
		public static Rect scrollRect = new Rect();

		public static void DoWindowContents(Rect inRect)
		{
			if (defs == null)
			{
				defs = new HashSet<IncidentDef>();

				foreach (IncidentDef def in DefDatabase<IncidentDef>.AllDefs)
					defs.Add(def);
			}

			if (BlackList == null)
				BlackList = new HashSet<string>
				{
					"StrangerInBlackJoin"
				};

			Listing_Standard gui = new Listing_Standard();

			gui.Begin(inRect);
			{
				gui.Label(DESCRIPTION);

				float height = gui.CurHeight + 10f;
				gui.ColumnWidth /= 2f;

				gui.BeginScrollView(
					new Rect(
						0,
						height,
						gui.ColumnWidth + 20f,
						inRect.height - height - 20f
					),
					ref scrollVector,
					ref scrollRect
				);
				{
					foreach (IncidentDef def in defs)
						try
						{
							string name = def.defName;

							bool prev = !BlackList.Contains(name);
							bool flag = prev;

							gui.CheckboxLabeled(
								$"[{name}] {def.label}",
								ref flag,
								def.description ?? " "
							);

							if (flag != prev)
								if (flag)
									BlackList.Remove(name);
								else
									BlackList.Add(name);
						}
						catch { }
				}
				gui.EndScrollView(ref scrollRect);

				float width = gui.ColumnWidth;

				gui.ColumnWidth += 20f;
				gui.NewColumn();
				gui.Gap(height);
				gui.ColumnWidth = 120f;

				if (gui.ButtonText("Enable All"))
					BlackList.Clear();

				if (gui.ButtonText("Disable All"))
					foreach (IncidentDef def in defs)
						BlackList.Add(def.defName);

				gui.Gap(10f);
				gui.ColumnWidth = width - 40f;

				gui.CheckboxLabeled(PROMPT_FIRST, ref PromptFirst, DESCRIPTION_PROMPT_FIRST);
				gui.CheckboxLabeled(DISPLAY_IF_QUEUED, ref DisplayIfQueued, DESCRIPTION_DISPLAY_IF_QUEUED);
			}
			gui.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref PromptFirst, "PromptFirst", PromptFirst, true);
			Scribe_Values.Look(ref DisplayIfQueued, "DisplayIfQueued", DisplayIfQueued, true);
			Scribe_Collections.Look(ref BlackList, "BlackList", LookMode.Value);
		}
	}
}

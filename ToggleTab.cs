using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RW_Sortie
{
	public class ToggleTab : MainButtonWorker_ToggleTab
	{
		public const string DIALOG_MESSAGE =
			"Are you sure you want to start the next incident/event?";
		public const string DIALOG_YES =
			"Yes";
		public const string DIALOG_NO =
			"No";

		public override void DoButton(Rect rect)
		{
			if (Widgets.ButtonTextSubtle(
				rect,
				def.LabelCap,
				mouseoverSound: SoundDefOf.Mouseover_Category
			))
			{
				FiringIncident fi = NextIncident(out List<QueuedIncident> incidents, out QueuedIncident incident);

				if (fi == null)
					return;

				if (Settings.PromptFirst)
				{
					string message = DIALOG_MESSAGE;

					if (Settings.DisplayIfQueued && incidents == null)
					{
						string label = $"[{fi.def.defName}] {fi.def.LabelCap}";
						message = $"Next Event: {label}\n\n{message}";
					}

					Find.WindowStack.Add(new Dialog_MessageBox(
						message,
						DIALOG_YES,
						() => StartIncident(fi, incidents, incident),
						DIALOG_NO
					));
				}
				else
					StartIncident(fi, incidents, incident);

				return;
			}

			TooltipHandler.TipRegion(rect, def.description);
		}

		public static FiringIncident NextIncident(out List<QueuedIncident> incidents, out QueuedIncident incident)
		{
			Storyteller teller = Find.Storyteller;
			IncidentQueue queue = teller.incidentQueue;

			incidents =
				Traverse.Create(queue)
					.Field("queuedIncidents")
					.GetValue<List<QueuedIncident>>();

			if (incidents.Count > 0)
			{
				incident = incidents[0];
				return incident.FiringIncident;
			}

			queue = null;
			incident = null;

			HashSet<IncidentDef> disabled = new HashSet<IncidentDef>();

			foreach (ScenPart part in Find.Scenario.AllParts)
				if (part is ScenPart_DisableIncident part_incident)
					disabled.Add(part_incident.Incident);

			Map map = Find.AnyPlayerHomeMap;
			int pawn_count =
				PawnsFinder
					.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
					.Count();
			StoryWatcher watcher = Find.StoryWatcher;

			List<IncidentDef> defs = DefDatabase<IncidentDef>.AllDefs.Where(v =>
				(
					Settings.BlackList == null ||
					!Settings.BlackList.Contains(v.defName)
				) &&
				v.TargetAllowed(map) &&
				teller.difficulty.threatScale >= v.minThreatPoints &&
				!disabled.Contains(v) &&
				(
					v.minPopulation == 0 ||
					pawn_count >= v.minPopulation
				) &&
				(
					v.allowedBiomes == null ||
					!v.allowedBiomes.Contains(map.Biome)
				) &&
				(
					v.minGreatestPopulation == 0 ||
					watcher.statsRecord.greatestPopulation >= v.minGreatestPopulation
				) &&
				(
					teller.difficulty.allowBigThreats ||
					v.category != IncidentCategoryDefOf.ThreatBig
				)
			).ToList();

			IncidentDef def = defs.RandomElement();
			IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, map);

			return new FiringIncident(def, null, parms);
		}

		public static void StartIncident(FiringIncident fi,
										 List<QueuedIncident> incidents,
										 QueuedIncident incident)
		{
			fi.def.Worker.TryExecute(fi.parms);
			fi.parms.target.StoryState.Notify_IncidentFired(fi);

			if (incidents != null && incident != null)
				incidents.Remove(incident);
		}

		/*public static void StartIncident_bak()
		{
			Storyteller teller = Find.Storyteller;
			IncidentQueue queue = teller.incidentQueue;
			List<QueuedIncident> incidents =
				Traverse.Create(queue)
					.Field("queuedIncidents")
					.GetValue<List<QueuedIncident>>();

			for (int i = 0; i < incidents.Count; i++)
				try
				{
					QueuedIncident incident = incidents[i];
					FiringIncident fi = incident.FiringIncident;

					if (fi.def.Worker.TryExecute(fi.parms))
					{
						fi.parms.target.StoryState.Notify_IncidentFired(fi);
						incidents.Remove(incident);
						return;
					}
				}
				catch { }

			HashSet<IncidentDef> disabled = new HashSet<IncidentDef>();

			foreach (ScenPart part in Find.Scenario.AllParts)
				if (part is ScenPart_DisableIncident part_incident)
					disabled.Add(part_incident.Incident);

			Map map = Find.AnyPlayerHomeMap;
			int pawn_count =
				PawnsFinder
					.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
					.Count();
			StoryWatcher watcher = Find.StoryWatcher;

			List<IncidentDef> defs = DefDatabase<IncidentDef>.AllDefs.Where(v =>
				(
					Settings.BlackList == null ||
					!Settings.BlackList.Contains(v.defName)
				) &&
				v.TargetAllowed(map) &&
				teller.difficulty.difficulty >= v.minDifficulty &&
				!disabled.Contains(v) &&
				(
					v.minPopulation == 0 ||
					pawn_count >= v.minPopulation
				) &&
				(
					v.allowedBiomes == null ||
					!v.allowedBiomes.Contains(map.Biome)
				) &&
				(
					v.minGreatestPopulation == 0 ||
					watcher.statsRecord.greatestPopulation >= v.minGreatestPopulation
				) &&
				(
					teller.difficulty.allowBigThreats ||
					(
						v.category != IncidentCategoryDefOf.ThreatBig &&
						v.category != IncidentCategoryDefOf.RaidBeacon
					)
				)
			).ToList();

			defs.Shuffle();

			foreach (IncidentDef def in defs)
			{
				try
				{
					IncidentParms parms =
						StorytellerUtility.DefaultParmsNow(def.category, map);

					if (!def.Worker.CanFireNow(parms, false))
						continue;

					var fi = new FiringIncident(def, null, parms);

					if (fi.def.Worker.TryExecute(parms))
					{
						fi.parms.target.StoryState.Notify_IncidentFired(fi);
						return;
					}
				}
				catch { }
			}
		}*/
	}
}

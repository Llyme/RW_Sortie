using UnityEngine;
using Verse;

namespace RW_Sortie
{
	public class SettingsController : Mod
	{
		public SettingsController(ModContentPack content) : base(content)
		{
			GetSettings<Settings>();
		}

		public override string SettingsCategory()
		{
			return "Sortie";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoWindowContents(inRect);
		}
	}
}

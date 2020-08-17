using UnityEngine;
using Verse;

namespace RW_Sortie
{
	public class MainTabWindow : RimWorld.MainTabWindow
	{
		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(300f, 400f);
			}
		}

		public override void DoWindowContents(Rect canvas)
		{
			GUI.BeginGroup(canvas);
			{
				Listing_Standard listing_Standard = new Listing_Standard();

				listing_Standard.Begin(canvas);
				{
					Text.Font = GameFont.Medium;
					listing_Standard.Label(Translator.Translate("Sortie"), -1f, null);

					Text.Font = GameFont.Small;
				}
				listing_Standard.End();
			}
			GUI.EndGroup();
		}

		public override void PreOpen()
		{
			base.PreOpen();
			forcePause = true;
		}
	}
}

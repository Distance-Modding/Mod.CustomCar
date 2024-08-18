using HarmonyLib;

namespace Distance.CustomCar.Patches
{
	[HarmonyPatch(typeof(Profile), "SetColorsForAllCars", new System.Type[] { typeof(CarColors) })]
	internal static class Profile__SetColorsForAllCars
	{
		[HarmonyPrefix]
		internal static bool Prefix(Profile __instance, CarColors cc)
		{
			CarColors[] carColors = new CarColors[G.Sys.ProfileManager_.carInfos_.Length];
			for (int i = 0; i < carColors.Length; i++)
			{
				carColors[i] = cc;
			}

			__instance.carColorsList_ = carColors;
			__instance.dataModified_ = true;

			return false;
		}
	}
}

using HarmonyLib;

namespace Distance.CustomCar.Harmony
{
	[HarmonyPatch(typeof(Profile), "Save")]
	internal static class Profile__Save
	{
		[HarmonyPostfix]
		internal static void Postfix()
		{
			Mod.Instance.CarColors.SaveAll();
		}
	}
}

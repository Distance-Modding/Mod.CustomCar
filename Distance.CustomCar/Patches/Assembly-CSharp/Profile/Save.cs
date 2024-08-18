using HarmonyLib;

namespace Distance.CustomCar.Patches
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

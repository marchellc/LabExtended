using GameCore;
using HarmonyLib;
using LabExtended.API;
using LabExtended.API.Containers;
namespace LabExtended.Patches.Functions.Players;

public static class DummyPlayerPatch {
    [HarmonyPatch(typeof(DummyUtils), nameof(DummyUtils.SpawnDummy))]
    public static void SpawnPostfix(ref ReferenceHub __result)
    {
        _ = new ExPlayer(__result, SwitchContainer.GetNewNpcToggles(true));
    }
}
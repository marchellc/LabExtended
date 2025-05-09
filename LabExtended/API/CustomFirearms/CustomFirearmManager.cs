using LabExtended.API.CustomItems;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Firearms;
using PlayerStatsSystem;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Manages Custom Firearm items.
/// </summary>
public static class CustomFirearmManager
{
    [LoaderInitialize(1)]
    private static void OnInit()
    {

    }
}
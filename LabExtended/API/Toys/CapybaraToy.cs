using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.API.Toys;

/// <summary>
/// A wrapper for <see cref="AdminToys.CapybaraToy"/>.
/// </summary>
public class CapybaraToy : AdminToy, IWrapper<AdminToys.CapybaraToy>
{
    /// <summary>
    /// Spawns a new capybara toy.
    /// </summary>
    public CapybaraToy() : base(PrefabList.Capybara.CreateInstance().GetComponent<AdminToyBase>())
    {
        Base = base.Base as AdminToys.CapybaraToy;

        if (Base is null)
            throw new Exception("Could not spawn CapybaraToy");
    }

    internal CapybaraToy(AdminToys.CapybaraToy baseValue) : base(baseValue)
        => Base = baseValue;

    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new AdminToys.CapybaraToy Base { get; }

    /// <summary>
    /// Whether collisions are enabled.
    /// </summary>
    public bool CollisionsEnabled
    {
        get => Base._collisionsEnabled;
        set => Base.CollisionsEnabled = value;
    }
}
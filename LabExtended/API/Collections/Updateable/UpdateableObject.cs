namespace LabExtended.API.Collections.Updateable;

/// <summary>
/// Represents an object which can be updated.
/// </summary>
public class UpdateableObject
{
    /// <summary>
    /// Gets called once per interval set by the parent list.
    /// </summary>
    /// <param name="delta">The time (in seconds) elapsed between the previous and this update call.</param>
    public virtual void OnUpdate(float delta) { }
}
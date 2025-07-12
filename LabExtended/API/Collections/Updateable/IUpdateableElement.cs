namespace LabExtended.API.Collections.Updateable;

/// <summary>
/// Represents an object which can be updated.
/// </summary>
public interface IUpdateableElement
{
    /// <summary>
    /// Gets called once per interval set by the parent list.
    /// </summary>
    /// <param name="listReference">The reference to the parent list.</param>
    /// <param name="delta">The time (in seconds) elapsed between the previous and this update call.</param>
    void OnUpdate(object listReference, float delta);
}
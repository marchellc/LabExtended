namespace LabExtended.Core.Events
{
    /// <summary>
    /// Represents an event that can be cancelled by a <see langword="bool"/>.
    /// </summary>
    public class BoolCancellableEvent : CancellableEvent<bool> { }
}
namespace LabExtended.API.Hints.Interfaces;

public interface IHintRateModifier
{
    float ModifyRate(float targetRate);
}
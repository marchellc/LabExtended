namespace LabExtended.API.Modules
{
    public class GenericModule<T> : Module
        where T : Module
    {
        public T CastParent
        {
            get
            {
                if (Parent is null)
                    return null;

                return (T)Parent;
            }
        }

        public override bool ValidateAdd(Module module)
            => base.ValidateAdd(module) && module is T;
    }
}
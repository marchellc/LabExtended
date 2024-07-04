using LabExtended.API;

namespace LabExtended.Modules
{
    public class PlayerModule : Module
    {
        public ExPlayer Player
        {
            get
            {
                if (Parent is null)
                    return null;

                return (ExPlayer)Parent;
            }
        }
    }
}
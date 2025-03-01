using LabExtended.Extensions;
using LabExtended.Utilities;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.API;

using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

using System.Reflection;

namespace LabExtended.Commands
{
    public abstract class CustomCommand<T> : CustomCommand
        where T : class
    {
        private List<ArgumentCollectionMember> _members;

        public CustomCommand() : base() { }

        public abstract T Instantiate();

        public override ArgumentDefinition[] BuildArgs()
            => GenerateArgs();

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var buffer = ArrayPool<object>.Shared.Rent(1);

            try
            {
                using (var collectionPooled = WrapperPool<T>.Shared.Rent(null, Instantiate))
                {
                    var collection = collectionPooled.Value;
                    
                    for (int i = 0; i < _members.Count; i++)
                    {
                        var member = _members[i];

                        buffer[0] = args.Get(member.Definition.Name);

                        member.SetValue(collection, buffer);
                    }

                    if (collection is IArgumentCollection argumentCollection)
                        argumentCollection.Initialize(sender, ctx, args);

                    OnCommand(sender, ctx, collection);

                    if (collection is IDisposable disposable)
                        disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                ctx.RespondFail(ex);
            }

            ArrayPool<object>.Shared.Return(buffer);
        }


        public virtual void OnCommand(ExPlayer sender, ICommandContext ctx, T collection) { }

        private ArgumentDefinition[] GenerateArgs()
        {
            _members = new();

            var properties = typeof(T).GetAllProperties();
            var list = ListPool<ArgumentDefinition>.Shared.Rent();

            foreach (var property in properties)
            {
                var setMethod = property.GetSetMethod(true);

                if (setMethod is null || setMethod.IsStatic)
                    continue;

                var setter = FastReflection.ForMethod(setMethod);
                var member = new ArgumentCollectionMember();
                var attribute = property.GetCustomAttribute<CollectionParameterAttribute>();
                var parser = CommandParser.FromType(property.PropertyType);

                member.SetValue = setter;
                member.Definition = new ArgumentDefinition(
                    property.PropertyType,

                    attribute is null || string.IsNullOrWhiteSpace(attribute.Name) ? property.Name.SpaceByUpperCase() : attribute.Name,
                    attribute is null || string.IsNullOrWhiteSpace(attribute.Description) ? parser.Name : attribute.Description,

                    parser, 
                    null,
                    attribute?.IsOptional ?? false);

                _members.Add(member);

                list.Add(member.Definition);
            }

            return ListPool<ArgumentDefinition>.Shared.ToArrayReturn(list);
        }
    }
}
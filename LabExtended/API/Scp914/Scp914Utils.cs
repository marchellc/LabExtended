using InventorySystem;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

using Scp914;
using Scp914.Processors;

using Utils.NonAllocLINQ;

namespace LabExtended.API.Scp914;

/// <summary>
/// Utilities targeting SCP-914.
/// </summary>
public static class Scp914Utils
{
    /// <summary>
    /// Fills the target dictionary with a list of vanilla recipes.
    /// </summary>
    /// <param name="target">The target dictionary.</param>
    public static void FillDefaultRecipes(Dictionary<Scp914KnobSetting, List<Scp914Entry>> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var roughRecipes = target.GetOrAdd(Scp914KnobSetting.Rough, () => new());
        var coarseRecipes = target.GetOrAdd(Scp914KnobSetting.Coarse, () => new());
        var oneToOneRecipes = target.GetOrAdd(Scp914KnobSetting.OneToOne, () => new());
        var fineRecipes = target.GetOrAdd(Scp914KnobSetting.Fine, () => new());
        var veryFineRecipes = target.GetOrAdd(Scp914KnobSetting.VeryFine, () => new());

        var typeChance = DictionaryPool<ItemType, float>.Shared.Rent();

        void ProcessFirearm(List<Scp914Entry> entries, ItemType input, FirearmItemProcessor.FirearmOutput[] array)
        {
            if (!entries.TryGetFirst(x => x.InputType == input, out var entry))
                entries.Add(entry = new(input));

            for (var i = 0; i < array.Length; i++)
            {
                var output = array[i];
                var chance = 100f / output.TargetItems.Length;
                var recipe = default(Scp914Recipe);

                if (entry.Recipes.Count < 1)
                {
                    recipe = new();
                    entry.Recipes.Add(recipe);
                }
                else
                {
                    recipe = entry.Recipes[0];
                }

                recipe.Chance = output.Chance * 100f;
                recipe.Items.Clear();
                
                typeChance.Clear();
                
                for (var x = 0; x < output.TargetItems.Length; x++)
                {
                    if (!typeChance.ContainsKey(output.TargetItems[x]))
                        typeChance.Add(output.TargetItems[x], chance);
                    else
                        typeChance[output.TargetItems[x]] += chance;
                }

                for (var x = 0; x < output.TargetItems.Length; x++)
                {
                    var item = output.TargetItems[x];
                    var itemChance = typeChance[item];
                    
                    recipe.WithItem(new(itemChance, item));
                }
            }
        }
        
        void ProcessStandard(List<Scp914Entry> entries, ItemType input, ItemType[] array)
        {
            if (!entries.TryGetFirst(x => x.InputType == input, out var entry))
                entries.Add(entry = new(input));
            
            typeChance.Clear();

            var chance = 100f / array.Length;
            var recipe = default(Scp914Recipe);

            if (entry.Recipes.Count < 1)
            {
                recipe = new();
                entry.Recipes.Add(recipe);
            }
            else
            {
                recipe = entry.Recipes[0];
            }

            recipe.Chance = 100f;
            recipe.Items.Clear();

            for (var i = 0; i < array.Length; i++)
            {
                if (!typeChance.ContainsKey(array[i]))
                    typeChance.Add(array[i], chance);
                else
                    typeChance[array[i]] += chance;
            }

            for (var i = 0; i < array.Length; i++)
            {
                var item = array[i];
                var itemChance = typeChance[item];
                
                if (recipe.Items.Any(x => x.Item == item))
                    continue;
                
                recipe.Items.Add(new(itemChance, item));
            }
        }
        
        foreach (var item in InventoryItemLoader.AvailableItems)
        {
            if (item.Value?.gameObject == null || !item.Value.gameObject.TryGetComponent<Scp914ItemProcessor>(out var processor))
                continue;

            if (processor is AmmoItemProcessor ammoProcessor)
            {
                if (!roughRecipes.TryGetFirst(x => x.InputType == item.Key, out var roughRecipe))
                    roughRecipes.Add(roughRecipe = new(item.Key));
                
                if (!coarseRecipes.TryGetFirst(x => x.InputType == item.Key, out var coarseRecipe))
                    coarseRecipes.Add(coarseRecipe = new(item.Key));
                
                if (!oneToOneRecipes.TryGetFirst(x => x.InputType == item.Key, out var oneToOneRecipe))
                    oneToOneRecipes.Add(oneToOneRecipe = new(item.Key));
                
                if (!fineRecipes.TryGetFirst(x => x.InputType == item.Key, out var fineRecipe))
                    fineRecipes.Add(fineRecipe = new(item.Key));
                
                if (!veryFineRecipes.TryGetFirst(x => x.InputType == item.Key, out var veryFineRecipe))
                    veryFineRecipes.Add(veryFineRecipe = new(item.Key));
                
                roughRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ammoProcessor._previousAmmo)));

                coarseRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ammoProcessor._previousAmmo)));

                oneToOneRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ammoProcessor._oneToOne)));

                fineRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ammoProcessor._nextAmmo)));
                
                veryFineRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ammoProcessor._nextAmmo)));
            }
            else if (processor is DisruptorItemProcessor)
            {
                if (!roughRecipes.TryGetFirst(x => x.InputType == item.Key, out var roughRecipe))
                    roughRecipes.Add(roughRecipe = new(item.Key));
                
                if (!coarseRecipes.TryGetFirst(x => x.InputType == item.Key, out var coarseRecipe))
                    coarseRecipes.Add(coarseRecipe = new(item.Key));
                
                if (!oneToOneRecipes.TryGetFirst(x => x.InputType == item.Key, out var oneToOneRecipe))
                    oneToOneRecipes.Add(oneToOneRecipe = new(item.Key));
                
                if (!fineRecipes.TryGetFirst(x => x.InputType == item.Key, out var fineRecipe))
                    fineRecipes.Add(fineRecipe = new(item.Key));
                
                if (!veryFineRecipes.TryGetFirst(x => x.InputType == item.Key, out var veryFineRecipe))
                    veryFineRecipes.Add(veryFineRecipe = new(item.Key));

                roughRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.Flashlight)));

                coarseRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.GunE11SR)));
                
                oneToOneRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.Jailbird)));

                fineRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.ParticleDisruptor)));

                veryFineRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.ParticleDisruptor)));
            }
            else if (processor is Scp1344ItemProcessor)
            {
                if (!oneToOneRecipes.TryGetFirst(x => x.InputType == item.Key, out var oneToOneRecipe))
                    oneToOneRecipes.Add(oneToOneRecipe = new(item.Key));
                
                if (!veryFineRecipes.TryGetFirst(x => x.InputType == item.Key, out var veryFineRecipe))
                    veryFineRecipes.Add(veryFineRecipe = new(item.Key));
                
                oneToOneRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.GrenadeFlash))
                    .WithItem(new(100f, ItemType.Adrenaline)));
                
                veryFineRecipe.Recipes.Add(new Scp914Recipe()
                    .WithChance(100f)
                    .WithItem(new(100f, ItemType.Adrenaline))
                    .WithItem(new(100f, ItemType.Adrenaline))
                    .WithItem(new(100f, ItemType.SCP2176)));
            } 
            
            if (processor is FirearmItemProcessor firearmProcessor)
            {
                if (firearmProcessor._roughOutputs?.Length > 0)
                    ProcessFirearm(roughRecipes, item.Key, firearmProcessor._roughOutputs);
                
                if (firearmProcessor._coarseOutputs?.Length > 0)
                    ProcessFirearm(coarseRecipes, item.Key, firearmProcessor._coarseOutputs);
                
                if (firearmProcessor._oneToOneOutputs?.Length > 0)
                    ProcessFirearm(oneToOneRecipes, item.Key, firearmProcessor._oneToOneOutputs);
                
                if (firearmProcessor._fineOutputs?.Length > 0)
                    ProcessFirearm(fineRecipes, item.Key, firearmProcessor._fineOutputs);
                
                if (firearmProcessor._veryFineOutputs?.Length > 0)
                    ProcessFirearm(veryFineRecipes, item.Key, firearmProcessor._veryFineOutputs);
            }
            
            if (processor is StandardItemProcessor standardProcessor)
            {
                if (standardProcessor._roughOutputs?.Length > 0)
                    ProcessStandard(roughRecipes, item.Key, standardProcessor._roughOutputs);
                
                if (standardProcessor._coarseOutputs?.Length > 0)
                    ProcessStandard(coarseRecipes, item.Key, standardProcessor._coarseOutputs);
                
                if (standardProcessor._oneToOneOutputs?.Length > 0)
                    ProcessStandard(oneToOneRecipes, item.Key, standardProcessor._oneToOneOutputs);
                
                if (standardProcessor._fineOutputs?.Length > 0)
                    ProcessStandard(fineRecipes, item.Key, standardProcessor._fineOutputs);
                
                if (standardProcessor._veryFineOutputs?.Length > 0)
                    ProcessStandard(veryFineRecipes, item.Key, standardProcessor._veryFineOutputs);
            }
        }
        
        DictionaryPool<ItemType, float>.Shared.Return(typeChance);
    }

    /// <summary>
    /// Prints the provided recipes into the server console.
    /// <param name="recipes">The recipe list to print.</param>
    /// </summary>
    public static void PrintRecipes(Dictionary<Scp914KnobSetting, List<Scp914Entry>> recipes)
    {
        if (recipes is null)
            throw new ArgumentNullException(nameof(recipes));
        
        ApiLog.Debug("SCP-914 API", StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine();
            
            foreach (var setting in recipes)
            {
                x.AppendLine($"&1{setting.Key}&r (&6{setting.Value.Count}&r):");

                foreach (var entry in setting.Value)
                {
                    x.AppendLine($">- &3{entry.InputType}&r (&6{entry.Recipes.Count}&r):");

                    foreach (var recipe in entry.Recipes)
                    {
                        x.AppendLine($"  <- &3Chance&r: &6{recipe.Chance}&r");

                        foreach (var output in recipe.Items)
                        {
                            x.AppendLine($"   * &3{output.Chance}%&r: &6{output.Item}&r");
                        }
                    }
                }
            }
        }));
    }
}
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

using LabExtended.Events;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Holds cached firearm modules.
/// </summary>
public class FirearmModuleCache : IDisposable
{
    /// <summary>
    /// Gets a list of all created caches.
    /// </summary>
    public static Dictionary<Firearm, FirearmModuleCache> List { get; } = new();
    
    internal FirearmModuleCache() { }
    
    /// <summary>
    /// Gets the target firearm.
    /// </summary>
    public Firearm Firearm { get; private set; }

    /// <summary>
    /// Gets a list of all modules.
    /// </summary>
    public List<ModuleBase> Modules { get; private set; } = ListPool<ModuleBase>.Shared.Rent();

    /// <summary>
    /// Gets a list of modules grouped by type.
    /// </summary>
    public Dictionary<Type, ModuleBase> TypeToModule { get; private set; } =
        DictionaryPool<Type, ModuleBase>.Shared.Rent();

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.A7BurnEffectModule"/> module instance.
    /// </summary>
    public A7BurnEffectModule A7BurnEffect;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AnimationToggleableReloaderModule"/> module instance.
    /// </summary>
    public AnimationToggleableReloaderModule AnimationToggleableReloader;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AnimationTriggerReloaderModule"/> module instance.
    /// </summary>
    public AnimationTriggerReloaderModule AnimationTriggerReloader;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AnimatorReloaderModuleBase"/> module instance.
    /// </summary>
    public AnimatorReloaderModuleBase AnimatorReloader;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AnimatorStateSetterModule"/> module instance.
    /// </summary>
    public AnimatorStateSetterModule AnimatorStateSetter;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AnimatorSpectatorSyncModule"/> module instance.
    /// </summary>
    public AnimatorSpectatorSyncModule AnimatorSpectatorSync;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AttachmentDependentHitreg"/> module instance.
    /// </summary>
    public AttachmentDependentHitreg AttachmentDependentHitreg;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AudioModule"/> module instance.
    /// </summary>
    public AudioModule Audio;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.AutomaticActionModule"/> module instance.
    /// </summary>
    public AutomaticActionModule AutomaticAction;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.BuckshotHitreg"/> module instance.
    /// </summary>
    public BuckshotHitreg BuckshotHitreg;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.CylinderAmmoModule"/> module instance.
    /// </summary>
    public CylinderAmmoModule CylinderAmmo;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DisruptorActionModule"/> module instance.
    /// </summary>
    public DisruptorActionModule DisruptorAction;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DisruptorAdsModule"/> module instance.
    /// </summary>
    public DisruptorAdsModule DisruptorAds;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DisruptorAudioModule"/> module instance.
    /// </summary>
    public DisruptorAudioModule DisruptorAudio;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DisruptorHitregModule"/> module instance.
    /// </summary>
    public DisruptorHitregModule DisruptorHitreg;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DisruptorModeSelector"/> module instance.
    /// </summary>
    public DisruptorModeSelector DisruptorModeSelector;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.DoubleActionModule"/> module instance.
    /// </summary>
    public DoubleActionModule DoubleAction;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.EventBasedEquipperModule"/> module instance.
    /// </summary>
    public EventBasedEquipperModule EventBasedEquipper;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.EventManagerModule"/> module instance.
    /// </summary>
    public EventManagerModule EventManager;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.GripControllerModule"/> module instance.
    /// </summary>
    public GripControllerModule GripController;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.HitscanHitregModuleBase"/> module instance.
    /// </summary>
    public HitscanHitregModuleBase HitscanHitreg;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.ImpactEffectsModule"/> module instance.
    /// </summary>
    public ImpactEffectsModule ImpactEffects;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.LinearAdsModule"/> module instance.
    /// </summary>
    public LinearAdsModule LinearAds;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.MagazineModule"/> module instance.
    /// </summary>
    public MagazineModule Magazine;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.MovementInaccuracyModule"/> module instance.
    /// </summary>
    public MovementInaccuracyModule MovementInaccuracy;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.MultiBarrelHitscan"/> module instance.
    /// </summary>
    public MultiBarrelHitscan MultiBarrelHitscan;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.PumpActionModule"/> module instance.
    /// </summary>
    public PumpActionModule PumpActionModule;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.RecoilPatternModule"/> module instance.
    /// </summary>
    public RecoilPatternModule RecoilPattern;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.RevolverClipReloaderModule"/> module instance.
    /// </summary>
    public RevolverClipReloaderModule RevolverClipReloader;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.A7BurnEffectModule"/> module instance.
    /// </summary>
    public RevolverRouletteModule RevolverRoulette;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.SimpleInspectorModule"/> module instance.
    /// </summary>
    public SimpleInspectorModule SimpleInspector;
    
    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.SimpleTriggerModule"/> module instance.
    /// </summary>
    public SimpleTriggerModule SimpleTrigger;
    
    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.SingleBulletHitscan"/> module instance.
    /// </summary>
    public SingleBulletHitscan SingleBulletHitscan;

    /// <summary>
    /// Gets the <see cref="InventorySystem.Items.Firearms.Modules.SubsequentShotsInaccuracyModule"/> module instance.
    /// </summary>
    public SubsequentShotsInaccuracyModule SubsequentShotsInaccuracy;

    /// <summary>
    /// Gets the first module that implements the <see cref="IActionModule"/> interface.
    /// </summary>
    public IActionModule ActionModule;

    /// <summary>
    /// Gets the first module that implements the <see cref="IAdsModule"/> interface.
    /// </summary>
    public IAdsModule AdsModule;

    /// <summary>
    /// Gets the first module that implements the <see cref="IAmmoContainerModule"/> interface.
    /// </summary>
    public IAmmoContainerModule AmmoContainerModule;

    /// <summary>
    /// Gets the first module that implements the <see cref="IHitregModule"/> interface.
    /// </summary>
    public IHitregModule HitregModule;

    /// <summary>
    /// Gets the first module that implements the <see cref="IPrimaryAmmoContainerModule"/> interface.
    /// </summary>
    public IPrimaryAmmoContainerModule PrimaryAmmoContainer;
    
    /// <summary>
    /// Checks if any module meets the provided predicate.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <returns>true if a matching module exists</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool HasModule(Predicate<ModuleBase> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        for (var i = 0; i < Modules.Count; i++)
        {
            if (predicate(Modules[i]))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if any module meets the provided predicate.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="module">The found module instance.</param>
    /// <returns>true if a matching module exists</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool HasModule(Predicate<ModuleBase> predicate, out ModuleBase module)
    {        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < Modules.Count; i++)
        {
            var target = Modules[i];
            
            if (predicate(target))
            {
                module = target;
                return true;
            }
        }

        module = null;
        return false;
    }
    
    /// <summary>
    /// Checks if any module meets the provided predicate.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <typeparam name="T">The module type to cast to.</typeparam>
    /// <returns>true if a matching module exists</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool HasModule<T>(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < Modules.Count; i++)
        {
            if (Modules[i] is not T target)
                continue;
            
            if (predicate(target))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if any module meets the provided predicate.
    /// </summary>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="module">The found module instance.</param>
    /// <typeparam name="T">The module type to cast to.</typeparam>
    /// <returns>true if a matching module exists</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool HasModule<T>(Predicate<T> predicate, out T module)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < Modules.Count; i++)
        {
            if (Modules[i] is not T target)
                continue;
            
            if (predicate(target))
            {
                module = target;
                return true;
            }
        }

        module = default;
        return false;
    }
    
    /// <summary>
    /// Updates the module cache.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Update(Firearm firearm)
    {
        if (firearm == null)
            throw new ArgumentNullException(nameof(firearm));

        if (Firearm != null && firearm != Firearm)
            List.Remove(Firearm);
        
        List[firearm] = this;

        Modules.Clear();
        
        ResetFields();

        for (var i = 0; i < firearm.Modules.Length; i++)
        {
            var module = firearm.Modules[i];
            var type = module.GetType();

            if (TypeToModule.ContainsKey(type))
            {
                ApiLog.Warn("Firearm Module Cache", $"Firearm &3{firearm.ItemTypeId}&r has a duplicate module: &6{type.Name}&r");
                continue;
            }
            
            Modules.Add(module);
            TypeToModule.Add(type, module);
            
            SetFields(module);
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Modules != null)
            ListPool<ModuleBase>.Shared.Return(Modules);
        
        if (TypeToModule != null)
            DictionaryPool<Type, ModuleBase>.Shared.Return(TypeToModule);

        Modules = null;
        Firearm = null;
        TypeToModule = null;

        ResetFields();
        
        if (Firearm != null)
            List.Remove(Firearm);
    }

    private void ResetFields()
    {
        A7BurnEffect = null;
        AnimationToggleableReloader = null;
        AnimationTriggerReloader = null;
        AnimatorReloader = null;
        AnimatorSpectatorSync = null;
        AnimatorStateSetter = null;
        AttachmentDependentHitreg = null;
        Audio = null;
        AutomaticAction = null;
        BuckshotHitreg = null;
        CylinderAmmo = null;
        DisruptorAction = null;
        DisruptorAds = null;
        DisruptorAudio = null;
        DisruptorHitreg = null;
        DisruptorModeSelector = null;
        DoubleAction = null;
        EventBasedEquipper = null;
        EventManager = null;
        GripController = null;
        HitscanHitreg = null;
        ImpactEffects = null;
        LinearAds = null;
        Magazine = null;
        MovementInaccuracy = null;
        MultiBarrelHitscan = null;
        PumpActionModule = null;
        RecoilPattern = null;
        RevolverClipReloader = null;
        RevolverRoulette = null;
        SimpleInspector = null;
        SimpleTrigger = null;
        SingleBulletHitscan = null;
        SubsequentShotsInaccuracy = null;
        SubsequentShotsInaccuracy = null;
        ActionModule = null;
        AdsModule = null;
        HitregModule = null;
        PrimaryAmmoContainer = null;
    }

    private void SetFields(ModuleBase module)
    {
        if (module is A7BurnEffectModule a7BurnEffectModule)
            A7BurnEffect = a7BurnEffectModule;
        
        if (module is AnimationToggleableReloaderModule animationToggleableReloaderModule)
            AnimationToggleableReloader = animationToggleableReloaderModule;
        
        if (module is AnimationTriggerReloaderModule animationTriggerReloaderModule)
            AnimationTriggerReloader = animationTriggerReloaderModule;
        
        if (module is AnimatorReloaderModuleBase animatorReloaderModule)
            AnimatorReloader = animatorReloaderModule;
        
        if (module is AnimatorSpectatorSyncModule animatorSpectatorSyncModule)
            AnimatorSpectatorSync = animatorSpectatorSyncModule;
        
        if (module is AnimatorStateSetterModule animatorStateSetterModule)
            AnimatorStateSetter = animatorStateSetterModule;
        
        if (module is AttachmentDependentHitreg attachmentDependentHitregModule)
            AttachmentDependentHitreg = attachmentDependentHitregModule;
        
        if (module is AudioModule audioModule)
            Audio = audioModule;
        
        if (module is AutomaticActionModule automaticActionModule)
            AutomaticAction = automaticActionModule;
        
        if (module is BuckshotHitreg buckshotHitregModule)
            BuckshotHitreg = buckshotHitregModule;
        
        if (module is CylinderAmmoModule cylinderAmmoModule)
            CylinderAmmo = cylinderAmmoModule;
        
        if (module is DisruptorActionModule disruptorActionModule)
            DisruptorAction = disruptorActionModule;
        
        if (module is DisruptorAdsModule disruptorAdsModule)
            DisruptorAds = disruptorAdsModule;
        
        if (module is DisruptorAudioModule disruptorAudioModule)
            DisruptorAudio = disruptorAudioModule;
        
        if (module is DisruptorHitregModule disruptorHitregModule)
            DisruptorHitreg = disruptorHitregModule;
        
        if (module is DisruptorModeSelector disruptorModeSelector)
            DisruptorModeSelector = disruptorModeSelector;
        
        if (module is DoubleActionModule doubleActionModule)
            DoubleAction = doubleActionModule;
        
        if (module is EventBasedEquipperModule eventBasedEquipperModule)
            EventBasedEquipper = eventBasedEquipperModule;
        
        if (module is EventManagerModule eventManagerModule)
            EventManager = eventManagerModule;
        
        if (module is GripControllerModule gripControllerModule)
            GripController = gripControllerModule;
        
        if (module is HitscanHitregModuleBase hitscanHitregModule)
            HitscanHitreg = hitscanHitregModule;
        
        if (module is ImpactEffectsModule impactEffectsModule)
            ImpactEffects = impactEffectsModule;
        
        if (module is LinearAdsModule linearAdsModule)
            LinearAds = linearAdsModule;
        
        if (module is MagazineModule magazineModule)
            Magazine = magazineModule;
        
        if (module is MovementInaccuracyModule movementInaccuracyModule)
            MovementInaccuracy = movementInaccuracyModule;
        
        if (module is MultiBarrelHitscan multiBarrelHitscan)
            MultiBarrelHitscan = multiBarrelHitscan;
        
        if (module is PumpActionModule pumpActionModule)
            PumpActionModule = pumpActionModule;
        
        if (module is RecoilPatternModule recoilPatternModule)
            RecoilPattern = recoilPatternModule;
        
        if (module is RevolverClipReloaderModule revolverClipReloaderModule)
            RevolverClipReloader = revolverClipReloaderModule;
        
        if (module is RevolverRouletteModule revolverRouletteModule)
            RevolverRoulette = revolverRouletteModule;
        
        if (module is SimpleInspectorModule simpleInspectorModule)
            SimpleInspector = simpleInspectorModule;

        if (module is SimpleTriggerModule simpleTriggerModule)
            SimpleTrigger = simpleTriggerModule;
        
        if (module is SingleBulletHitscan singleBulletHitscan)
            SingleBulletHitscan = singleBulletHitscan;

        if (module is SubsequentShotsInaccuracyModule subsequentShotsInaccuracyModule)
            SubsequentShotsInaccuracy = subsequentShotsInaccuracyModule;
        
        if (module is IActionModule actionModule)
            ActionModule = actionModule;
        
        if (module is IAdsModule adsModule)
            AdsModule = adsModule;
        
        if (module is IAmmoContainerModule ammoContainerModule && AmmoContainerModule is null)
            AmmoContainerModule = ammoContainerModule;
        
        if (module is IHitregModule hitregModule)
            HitregModule = hitregModule;
        
        if (module is IPrimaryAmmoContainerModule primaryAmmoContainerModule)
            PrimaryAmmoContainer = primaryAmmoContainerModule;
    }

    private static void OnDestroyed(ItemBase item)
    {
        if (item == null || item is not Firearm firearm)
            return;
        
        if (List.TryGetValue(firearm, out var cache))
            cache.Dispose();
    }

    private static void OnCreated(ItemBase item)
    {
        if (item == null || item is not Firearm firearm)
            return;

        _ = firearm.GetModules();
    }

    private static void OnRestart()
    {
        var list = ListPool<FirearmModuleCache>.Shared.Rent(List.Values);
        
        for (var i = 0; i < list.Count; i++)
            list[i].Dispose();
        
        List.Clear();
        
        ListPool<FirearmModuleCache>.Shared.Return(list);
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        InternalEvents.OnRoundRestart += OnRestart;
        
        ItemBase.OnItemAdded += OnCreated;
        ItemBase.OnItemRemoved += OnDestroyed;
    }
}
﻿namespace LabExtended.API.CustomItems.Firearms
{
    [Flags]
    public enum CustomFirearmFlags : byte
    {
        None = 0,
        AmmoAsInventoryItems = 2,
        UnlimitedAmmo = 4,
        ReuseStatus = 8
    }
}
﻿using System;
using System.Runtime.InteropServices;
using AltV.Net.Data;
using AltV.Net.Elements.Args;
using AltV.Net.Shared.Elements.Entities;
using AltV.Net.Shared.Utils;

namespace AltV.Net.Elements.Entities;

public class VirtualEntity : WorldObject, IVirtualEntity
{
    public IntPtr VirtualEntityNativePointer { get; }
    public override IntPtr NativePointer => VirtualEntityNativePointer;

    private static IntPtr GetEntityPointer(ICore core, IntPtr virtualEntityNativePointer)
    {
        unsafe
        {
            return core.Library.Shared.VirtualEntity_GetBaseObject(virtualEntityNativePointer);
        }
    }

    public static uint GetId(IntPtr pedPointer)
    {
        unsafe
        {
            return Alt.Core.Library.Shared.VirtualEntity_GetID(pedPointer);
        }
    }

    public VirtualEntity(ICore core, IVirtualEntityGroup group, Position position, uint streamingDistance) : this(
        core, core.CreateVirtualEntityEntity(out var id, group, position, streamingDistance), id)
    {
    }

    public VirtualEntity(ICore core, IntPtr nativePointer, uint id) : base(core, GetEntityPointer(core, nativePointer), BaseObjectType.VirtualEntity, id)
    {
        this.VirtualEntityNativePointer = nativePointer;
    }

    public ISharedVirtualEntityGroup Group
    {
        get
        {
            unsafe
            {
                CheckIfEntityExists();
                var groupPointer = Core.Library.Shared.VirtualEntity_GetGroup(VirtualEntityNativePointer);
                if (groupPointer == IntPtr.Zero) return null;
                return (ISharedVirtualEntityGroup)Core.BaseBaseObjectPool.Get(groupPointer, BaseObjectType.VirtualEntity);
            }
        }
    }

    public bool HasStreamSyncedMetaData(string key)
    {
        unsafe
        {
            CheckIfEntityExists();
            var stringPtr = MemoryUtils.StringToHGlobalUtf8(key);
            var result = Core.Library.Shared.VirtualEntity_HasStreamSyncedMetaData(VirtualEntityNativePointer, stringPtr);
            Marshal.FreeHGlobal(stringPtr);
            return result == 1;
        }
    }

    public bool GetStreamSyncedMetaData<T>(string key, out T result)
    {
        CheckIfEntityExists();
        GetStreamSyncedMetaData(key, out MValueConst mValue);
        var obj = mValue.ToObject();
        mValue.Dispose();
        if (!(obj is T cast))
        {
            result = default;
            return false;
        }

        result = cast;
        return true;
    }

    public void GetStreamSyncedMetaData(string key, out MValueConst value)
    {
        CheckIfEntityExists();
        unsafe
        {
            var stringPtr = MemoryUtils.StringToHGlobalUtf8(key);
            value = new MValueConst(Core, Core.Library.Shared.VirtualEntity_GetStreamSyncedMetaData(VirtualEntityNativePointer, stringPtr));
            Marshal.FreeHGlobal(stringPtr);
        }
    }

    public uint StreamingDistance
    {
        get
        {
            unsafe
            {
                CheckIfEntityExists();
                return Core.Library.Server.VirtualEntity_GetStreamingDistance(VirtualEntityNativePointer);
            }
        }
    }

    public void SetStreamSyncedMetaData(string key, object value)
    {
        CheckIfEntityExists();
        Alt.Core.CreateMValue(out var mValue, value);
        SetStreamSyncedMetaData(key, in mValue);
        mValue.Dispose();
    }

    public void SetStreamSyncedMetaData(string key, in MValueConst value)
    {
        unsafe
        {
            var stringPtr = MemoryUtils.StringToHGlobalUtf8(key);
            Core.Library.Server.VirtualEntity_SetStreamSyncedMetaData(VirtualEntityNativePointer, stringPtr, value.nativePointer);
            Marshal.FreeHGlobal(stringPtr);
        }
    }

    public void DeleteStreamSyncedMetaData(string key)
    {
        CheckIfEntityExists();
        unsafe
        {
            var stringPtr = MemoryUtils.StringToHGlobalUtf8(key);
            Core.Library.Server.Entity_DeleteStreamSyncedMetaData(VirtualEntityNativePointer, stringPtr);
            Marshal.FreeHGlobal(stringPtr);
        }
    }
}
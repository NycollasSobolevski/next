using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Next.Services.Audio;

[SupportedOSPlatform("windows")]
public partial class WindowsAudioService
{
    private const string SystemSoundsId = "system-sounds";
    private const string SystemSoundsName = "System Sounds";
    private const int AudioSessionStateExpired = 2;

    public IReadOnlyList<AppAudioSession> GetAppSessions()
    {
        if (!OperatingSystem.IsWindows()) return [];

        var entries = EnumerateSessions();
        try
        {
            return entries
                .GroupBy(entry => entry.Id)
                .Select(group => new AppAudioSession(
                    group.Key,
                    group.First().Name,
                    (int)Math.Round(group.Max(entry => GetVolume(entry.Volume)) * 100f),
                    group.All(entry => GetMute(entry.Volume))))
                .OrderBy(session => session.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error listing app sessions: {ex.Message}");
            return [];
        }
        finally
        {
            ReleaseSessions(entries);
        }
    }

    public void SetAppVolume(string id, int volume)
    {
        if (!OperatingSystem.IsWindows()) return;

        float scalar = Math.Clamp(volume, 0, 100) / 100f;
        var entries = EnumerateSessions();
        try
        {
            Guid emptyGuid = Guid.Empty;
            foreach (var entry in entries.Where(entry => entry.Id == id))
            {
                Marshal.ThrowExceptionForHR(entry.Volume.SetMasterVolume(scalar, ref emptyGuid));
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error setting volume of {id}: {ex.Message}");
        }
        finally
        {
            ReleaseSessions(entries);
        }
    }

    public void SetAppMuted(string id, bool isMuted)
    {
        if (!OperatingSystem.IsWindows()) return;

        var entries = EnumerateSessions();
        try
        {
            Guid emptyGuid = Guid.Empty;
            foreach (var entry in entries.Where(entry => entry.Id == id))
            {
                Marshal.ThrowExceptionForHR(entry.Volume.SetMute(isMuted, ref emptyGuid));
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error setting mute of {id}: {ex.Message}");
        }
        finally
        {
            ReleaseSessions(entries);
        }
    }

    private static float GetVolume(ISimpleAudioVolume volume)
    {
        Marshal.ThrowExceptionForHR(volume.GetMasterVolume(out float level));
        return level;
    }

    private static bool GetMute(ISimpleAudioVolume volume)
    {
        Marshal.ThrowExceptionForHR(volume.GetMute(out bool isMuted));
        return isMuted;
    }

    private static List<AudioSessionEntry> EnumerateSessions()
    {
        var result = new List<AudioSessionEntry>();

        var device = GetDefaultRenderDevice();
        if (device == null) return result;

        Guid iid = typeof(IAudioSessionManager2).GUID;
        // CLSCTX_ALL = 23
        int hr = device.Activate(ref iid, 23, IntPtr.Zero, out var managerObj);
        Marshal.ReleaseComObject(device);

        if (hr != 0 || managerObj == null) return result;

        var manager = (IAudioSessionManager2)managerObj;
        try
        {
            if (manager.GetSessionEnumerator(out var sessionEnum) != 0) return result;

            try
            {
                sessionEnum.GetCount(out int count);
                for (int i = 0; i < count; i++)
                {
                    if (sessionEnum.GetSession(i, out var control) != 0) continue;

                    control.GetState(out int state);
                    control.GetProcessId(out uint pid);
                    bool isSystemSounds = control.IsSystemSoundsSession() == 0;

                    if (state == AudioSessionStateExpired)
                    {
                        Marshal.ReleaseComObject(control);
                    }
                    else if (isSystemSounds)
                    {
                        result.Add(new AudioSessionEntry(SystemSoundsId, SystemSoundsName, (ISimpleAudioVolume)control));
                    }
                    else if (pid != 0)
                    {
                        var (id, name) = ResolveProcess(pid);
                        result.Add(new AudioSessionEntry(id, name, (ISimpleAudioVolume)control));
                    }
                    else
                    {
                        Marshal.ReleaseComObject(control);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(sessionEnum);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(manager);
        }

        return result;
    }

    private static (string Id, string Name) ResolveProcess(uint pid)
    {
        try
        {
            using var process = Process.GetProcessById((int)pid);
            string? description = process.MainModule?.FileVersionInfo.FileDescription;
            string name = string.IsNullOrWhiteSpace(description) ? process.ProcessName : description;
            return (process.ProcessName.ToLowerInvariant(), name);
        }
        catch (Exception)
        {
            return ($"pid-{pid}", $"Process {pid}");
        }
    }

    private static void ReleaseSessions(IEnumerable<AudioSessionEntry> entries)
    {
        foreach (var entry in entries)
        {
            Marshal.ReleaseComObject(entry.Volume);
        }
    }

    private readonly record struct AudioSessionEntry(string Id, string Name, ISimpleAudioVolume Volume);

    #region Session COM Interfaces

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionManager2
    {
        [PreserveSig] int GetAudioSessionControl(IntPtr audioSessionGuid, uint streamFlags, out IntPtr sessionControl);
        [PreserveSig] int GetSimpleAudioVolume(IntPtr audioSessionGuid, uint streamFlags, out IntPtr audioVolume);
        [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator sessionEnum);
        [PreserveSig] int RegisterSessionNotification(IntPtr sessionNotification);
        [PreserveSig] int UnregisterSessionNotification(IntPtr sessionNotification);
        [PreserveSig] int RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionId, IntPtr duckNotification);
        [PreserveSig] int UnregisterDuckNotification(IntPtr duckNotification);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int sessionCount);
        [PreserveSig] int GetSession(int sessionCount, out IAudioSessionControl2 session);
    }

    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionControl2
    {
        [PreserveSig] int GetState(out int state);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string displayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, ref Guid eventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string iconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string value, ref Guid eventContext);
        [PreserveSig] int GetGroupingParam(out Guid groupingParam);
        [PreserveSig] int SetGroupingParam(ref Guid groupingParam, ref Guid eventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr newNotifications);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr newNotifications);
        [PreserveSig] int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string sessionIdentifier);
        [PreserveSig] int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string sessionInstanceIdentifier);
        [PreserveSig] int GetProcessId(out uint processId);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference([MarshalAs(UnmanagedType.Bool)] bool optOut);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ISimpleAudioVolume
    {
        [PreserveSig] int SetMasterVolume(float level, ref Guid eventContext);
        [PreserveSig] int GetMasterVolume(out float level);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool isMuted, ref Guid eventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool isMuted);
    }

    #endregion
}

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Next.Services.Audio;

[SupportedOSPlatform("windows")]
public partial class WindowsAudioService : IAudioService
{
    private readonly Logger? _logger;
    private readonly object _throttleLock = new();
    private int _targetVolume = -1;
    private bool _isProcessing = false;

    public WindowsAudioService(Logger? logger = null)
    {
        _logger = logger;
        _logger?.WriteLine("[WindowsAudioService] Initialized Windows Core Audio (WASAPI).");
    }

    public int GetMasterVolume()
    {
        if (!OperatingSystem.IsWindows()) return -1;

        try
        {
            var endpoint = GetMasterEndpointVolume();
            if (endpoint == null) return -1;

            endpoint.GetMasterVolumeLevelScalar(out float scalar);
            Marshal.ReleaseComObject(endpoint);

            return Math.Clamp((int)Math.Round(scalar * 100f), 0, 100);
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error reading master volume: {ex.Message}");
            return -1;
        }
    }

    public void SetMasterVolume(int volume)
    {
        if (!OperatingSystem.IsWindows()) return;

        int clampedVolume = Math.Clamp(volume, 0, 100);

        lock (_throttleLock)
        {
            _targetVolume = clampedVolume;
            if (_isProcessing) return;
            _isProcessing = true;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                int currentTarget;
                lock (_throttleLock)
                {
                    currentTarget = _targetVolume;
                }

                ApplyVolumeDirect(currentTarget);

                await Task.Delay(25);

                lock (_throttleLock)
                {
                    if (currentTarget == _targetVolume)
                    {
                        _isProcessing = false;
                        break;
                    }
                }
            }
        });
    }

    private void ApplyVolumeDirect(int volume)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            var endpoint = GetMasterEndpointVolume();
            if (endpoint == null) return;

            Guid emptyGuid = Guid.Empty;
            float scalar = volume / 100f;
            endpoint.SetMasterVolumeLevelScalar(scalar, ref emptyGuid);
            Marshal.ReleaseComObject(endpoint);

            _logger?.WriteLine($"[WindowsAudioService] Set volume to {volume}%");
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error setting volume to {volume}%: {ex.Message}");
        }
    }

    public bool IsMuted()
    {
        if (!OperatingSystem.IsWindows()) return false;

        try
        {
            var endpoint = GetMasterEndpointVolume();
            if (endpoint == null) return false;

            endpoint.GetMute(out bool isMuted);
            Marshal.ReleaseComObject(endpoint);
            return isMuted;
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error checking mute status: {ex.Message}");
            return false;
        }
    }

    public void SetMuted(bool isMuted)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            var endpoint = GetMasterEndpointVolume();
            if (endpoint == null) return;

            Guid emptyGuid = Guid.Empty;
            endpoint.SetMute(isMuted, ref emptyGuid);
            Marshal.ReleaseComObject(endpoint);

            _logger?.WriteLine($"[WindowsAudioService] Set mute to {isMuted}");
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[WindowsAudioService] Error setting mute: {ex.Message}");
        }
    }

    private static IMMDevice? GetDefaultRenderDevice()
    {
        var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        // 0 = eRender, 1 = eMultimedia
        int hr = enumerator.GetDefaultAudioEndpoint(0, 1, out var device);
        Marshal.ReleaseComObject(enumerator);

        return hr != 0 || device == null ? null : device;
    }

    private static IAudioEndpointVolume? GetMasterEndpointVolume()
    {
        var device = GetDefaultRenderDevice();
        if (device == null) return null;

        Guid iid = typeof(IAudioEndpointVolume).GUID;
        // CLSCTX_ALL = 23
        int hr = device.Activate(ref iid, 23, IntPtr.Zero, out var endpointObj);
        Marshal.ReleaseComObject(device);

        if (hr != 0 || endpointObj == null) return null;

        return (IAudioEndpointVolume)endpointObj;
    }

    #region COM Interfaces

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumerator
    {
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IntPtr ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr pClient);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
        [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig] int GetState(out int pdwState);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int GetChannelCount(out uint pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig] int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        [PreserveSig] int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
        [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig] int VolumeStepUp(ref Guid pguidEventContext);
        [PreserveSig] int VolumeStepDown(ref Guid pguidEventContext);
        [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig] int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    #endregion
}

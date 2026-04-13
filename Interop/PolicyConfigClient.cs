using System.Runtime.InteropServices;

using NAudio.CoreAudioApi;

namespace AudioDeviceManager.Interop;

internal sealed class PolicyConfigClient
{
    public void SetDefaultDevice(string deviceId)
    {
        var policyConfigType = Type.GetTypeFromCLSID(typeof(PolicyConfigVistaClient).GUID)
                               ?? throw new InvalidOperationException("Couldn't create the Windows PolicyConfig COM object.");
        var policyConfig = (IPolicyConfigVista)Activator.CreateInstance(policyConfigType)!;

        try
        {
            Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Console));
            Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Multimedia));
            Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Communications));
        }
        finally
        {
            Marshal.ReleaseComObject(policyConfig);
        }
    }
}

[ComImport]
[Guid("568B9108-44BF-40B4-9006-86AFE5B5A620")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfigVista
{
    int GetMixFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        out IntPtr mixFormat);

    int GetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        [MarshalAs(UnmanagedType.Bool)] bool defaultFormat,
        out IntPtr deviceFormat);

    int SetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr endpointFormat,
        IntPtr mixFormat);

    int GetProcessingPeriod(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        [MarshalAs(UnmanagedType.Bool)] bool defaultPeriod,
        out long defaultDevicePeriod,
        out long minimumDevicePeriod);

    int SetProcessingPeriod(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        ref long devicePeriod);

    int GetShareMode(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        out IntPtr mode);

    int SetShareMode(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr mode);

    int GetPropertyValue(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr propertyKey,
        out IntPtr propertyValue);

    int SetPropertyValue(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        IntPtr propertyKey,
        IntPtr propertyValue);

    [PreserveSig]
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, Role role);

    int SetEndpointVisibility(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        [MarshalAs(UnmanagedType.Bool)] bool visible);
}

[ComImport]
[Guid("294935CE-F637-4E7C-A41B-AB255460B862")]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class PolicyConfigVistaClient
{
}

using System.Runtime.InteropServices;

using NAudio.CoreAudioApi;

namespace AudioDeviceManager.Interop;

internal sealed class PolicyConfigClient
{
    public void SetDefaultDevice(string deviceId)
    {
        if (TrySetDefaultWithPolicyConfig(deviceId, out var policyConfigError))
        {
            return;
        }

        if (TrySetDefaultWithPolicyConfigVista(deviceId, out var policyConfigVistaError))
        {
            return;
        }

        throw new InvalidOperationException(
            "Windows rejected both PolicyConfig variants while changing the default audio device.",
            new AggregateException(
                policyConfigError ?? new InvalidOperationException("PolicyConfig failed without an exception."),
                policyConfigVistaError ?? new InvalidOperationException("PolicyConfigVista failed without an exception.")));
    }

    private static bool TrySetDefaultWithPolicyConfig(string deviceId, out Exception? error)
    {
        IPolicyConfig? policyConfig = null;

        try
        {
            var policyConfigType = Type.GetTypeFromCLSID(typeof(PolicyConfigClientComObject).GUID)
                                   ?? throw new InvalidOperationException("Couldn't create the PolicyConfig COM object.");
            policyConfig = (IPolicyConfig)Activator.CreateInstance(policyConfigType)!;

            SetDefaultEndpoints(deviceId, policyConfig.SetDefaultEndpoint);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception;
            return false;
        }
        finally
        {
            ReleaseComObject(policyConfig);
        }
    }

    private static bool TrySetDefaultWithPolicyConfigVista(string deviceId, out Exception? error)
    {
        IPolicyConfigVista? policyConfig = null;

        try
        {
            var policyConfigType = Type.GetTypeFromCLSID(typeof(PolicyConfigVistaClient).GUID)
                                   ?? throw new InvalidOperationException("Couldn't create the PolicyConfigVista COM object.");
            policyConfig = (IPolicyConfigVista)Activator.CreateInstance(policyConfigType)!;

            SetDefaultEndpoints(deviceId, policyConfig.SetDefaultEndpoint);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception;
            return false;
        }
        finally
        {
            ReleaseComObject(policyConfig);
        }
    }

    private static void SetDefaultEndpoints(string deviceId, Func<string, Role, int> setDefaultEndpoint)
    {
        Marshal.ThrowExceptionForHR(setDefaultEndpoint(deviceId, Role.Console));
        Marshal.ThrowExceptionForHR(setDefaultEndpoint(deviceId, Role.Multimedia));
        Marshal.ThrowExceptionForHR(setDefaultEndpoint(deviceId, Role.Communications));
    }

    private static void ReleaseComObject(object? instance)
    {
        if (instance is not null && Marshal.IsComObject(instance))
        {
            Marshal.ReleaseComObject(instance);
        }
    }
}

[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    int GetMixFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        out IntPtr mixFormat);

    int GetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        [MarshalAs(UnmanagedType.Bool)] bool defaultFormat,
        out IntPtr deviceFormat);

    int ResetDeviceFormat(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

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
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, Role role);

    int SetEndpointVisibility(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        [MarshalAs(UnmanagedType.Bool)] bool visible);
}

[ComImport]
[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class PolicyConfigClientComObject
{
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
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, Role role);

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

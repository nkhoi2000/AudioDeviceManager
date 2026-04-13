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
        return TrySetDefaultDevice(
            deviceId,
            () => CreateComInstance<IPolicyConfig>(typeof(PolicyConfigClientComObject).GUID),
            out error);
    }

    private static bool TrySetDefaultWithPolicyConfigVista(string deviceId, out Exception? error)
    {
        return TrySetDefaultDevice(
            deviceId,
            () => CreateComInstance<IPolicyConfigVista>(typeof(PolicyConfigVistaClient).GUID),
            out error);
    }

    private static bool TrySetDefaultDevice<TPolicyConfig>(
        string deviceId,
        Func<TPolicyConfig> factory,
        out Exception? error)
        where TPolicyConfig : class, IPolicyConfigApi
    {
        TPolicyConfig? policyConfig = null;

        try
        {
            policyConfig = factory();
            SetDefaultEndpoints(policyConfig, deviceId);
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
            if (policyConfig is not null && Marshal.IsComObject(policyConfig))
            {
                Marshal.ReleaseComObject(policyConfig);
            }
        }
    }

    private static TPolicyConfig CreateComInstance<TPolicyConfig>(Guid classId)
        where TPolicyConfig : class
    {
        var policyConfigType = Type.GetTypeFromCLSID(classId)
                               ?? throw new InvalidOperationException("Couldn't create the Windows PolicyConfig COM object.");

        return (TPolicyConfig)Activator.CreateInstance(policyConfigType)!;
    }

    private static void SetDefaultEndpoints(IPolicyConfigApi policyConfig, string deviceId)
    {
        Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Console));
        Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Multimedia));
        Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, Role.Communications));
    }
}

internal interface IPolicyConfigApi
{
    int SetDefaultEndpoint(string deviceId, Role role);
}

[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig : IPolicyConfigApi
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
    new int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, Role role);

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
internal interface IPolicyConfigVista : IPolicyConfigApi
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
    new int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, Role role);

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

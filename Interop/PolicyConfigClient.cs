using System.Runtime.InteropServices;

using NAudio.CoreAudioApi;

namespace AudioDeviceManager.Interop;

internal sealed class PolicyConfigClient
{
    public void SetDefaultDevice(string deviceId)
    {
        var policyConfigType = Type.GetTypeFromCLSID(typeof(PolicyConfig).GUID)
                               ?? throw new InvalidOperationException("Couldn't create the Windows PolicyConfig COM object.");
        var policyConfig = (IPolicyConfig)Activator.CreateInstance(policyConfigType)!;

        try
        {
            policyConfig.SetDefaultEndpoint(deviceId, Role.Console);
            policyConfig.SetDefaultEndpoint(deviceId, Role.Multimedia);
            policyConfig.SetDefaultEndpoint(deviceId, Role.Communications);
        }
        finally
        {
            Marshal.ReleaseComObject(policyConfig);
        }
    }
}

[ComImport]
[Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    int GetMixFormat();

    int GetDeviceFormat();

    int SetDeviceFormat();

    int GetProcessingPeriod();

    int SetProcessingPeriod();

    int GetShareMode();

    int SetShareMode();

    int GetPropertyValue();

    int SetPropertyValue();

    [PreserveSig]
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, Role role);

    int SetEndpointVisibility();
}

[ComImport]
[Guid("294935CE-F637-4E7C-A41B-AB255460B862")]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class PolicyConfig
{
}

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Calls Windows Credential Manager through a fixed bounded generic-credential boundary.</summary>
public sealed class WindowsCredentialManager : IWindowsCredentialManager
{
    private const uint CredentialTypeGeneric = 1;
    private const uint CredentialPersistLocalMachine = 2;
    private const int ErrorNotFound = 1168;

    /// <inheritdoc />
    public bool IsAvailable => OperatingSystem.IsWindows();

    /// <inheritdoc />
    public ValueTask<WindowsCredentialManagerResult> WriteAsync(
        string targetName,
        string userName,
        ReadOnlyMemory<byte> secretBytes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateIdentity(targetName, userName);
        if (secretBytes.IsEmpty
            || secretBytes.Length > OpenAiCredentialPolicy.MaximumApiKeyLength)
        {
            throw new ArgumentException("Secret bytes must use the bounded credential contract.", nameof(secretBytes));
        }

        if (!IsAvailable)
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Unavailable());
        }

        nint targetPointer = 0;
        nint userPointer = 0;
        nint secretPointer = 0;
        byte[] secretCopy = secretBytes.ToArray();
        try
        {
            targetPointer = Marshal.StringToCoTaskMemUni(targetName);
            userPointer = Marshal.StringToCoTaskMemUni(userName);
            secretPointer = Marshal.AllocHGlobal(secretCopy.Length);
            Marshal.Copy(secretCopy, 0, secretPointer, secretCopy.Length);
            NativeCredential credential = new()
            {
                Type = CredentialTypeGeneric,
                TargetName = targetPointer,
                CredentialBlobSize = checked((uint)secretCopy.Length),
                CredentialBlob = secretPointer,
                Persist = CredentialPersistLocalMachine,
                UserName = userPointer,
            };
            return ValueTask.FromResult(NativeMethods.CredWrite(ref credential, flags: 0)
                ? WindowsCredentialManagerResult.Succeeded(ReadOnlyMemory<byte>.Empty)
                : WindowsCredentialManagerResult.Failed());
        }
        catch (Exception exception) when (IsNativeBoundaryFailure(exception))
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Failed());
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretCopy);
            ClearAndFreeSecret(secretPointer, secretBytes.Length);
            Marshal.FreeCoTaskMem(userPointer);
            Marshal.FreeCoTaskMem(targetPointer);
        }
    }

    /// <inheritdoc />
    public ValueTask<WindowsCredentialManagerResult> ReadAsync(
        string targetName,
        string expectedUserName,
        int maximumSecretBytes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateIdentity(targetName, expectedUserName);
        if (maximumSecretBytes != OpenAiCredentialPolicy.MaximumApiKeyLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumSecretBytes),
                "Read must use the exact credential bound.");
        }

        if (!IsAvailable)
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Unavailable());
        }

        nint targetPointer = 0;
        nint credentialPointer = 0;
        try
        {
            targetPointer = Marshal.StringToCoTaskMemUni(targetName);
            if (!NativeMethods.CredRead(
                    targetPointer,
                    CredentialTypeGeneric,
                    flags: 0,
                    out credentialPointer))
            {
                return ValueTask.FromResult(Marshal.GetLastPInvokeError() == ErrorNotFound
                    ? WindowsCredentialManagerResult.Missing()
                    : WindowsCredentialManagerResult.Failed());
            }

            NativeCredential credential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
            if (credential.Type != CredentialTypeGeneric
                || credential.CredentialBlob == 0
                || credential.CredentialBlobSize is 0
                || credential.CredentialBlobSize > maximumSecretBytes
                || !string.Equals(
                    Marshal.PtrToStringUni(credential.TargetName),
                    targetName,
                    StringComparison.Ordinal)
                || !string.Equals(
                    Marshal.PtrToStringUni(credential.UserName),
                    expectedUserName,
                    StringComparison.Ordinal))
            {
                return ValueTask.FromResult(WindowsCredentialManagerResult.Failed());
            }

            byte[] secret = new byte[credential.CredentialBlobSize];
            try
            {
                Marshal.Copy(credential.CredentialBlob, secret, 0, secret.Length);
                return ValueTask.FromResult(WindowsCredentialManagerResult.Succeeded(secret));
            }
            finally
            {
                CryptographicOperations.ZeroMemory(secret);
            }
        }
        catch (Exception exception) when (IsNativeBoundaryFailure(exception))
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Failed());
        }
        finally
        {
            ClearAndFreeCredential(credentialPointer);
            Marshal.FreeCoTaskMem(targetPointer);
        }
    }

    /// <inheritdoc />
    public ValueTask<WindowsCredentialManagerResult> DeleteAsync(
        string targetName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateTarget(targetName);
        if (!IsAvailable)
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Unavailable());
        }

        nint targetPointer = 0;
        try
        {
            targetPointer = Marshal.StringToCoTaskMemUni(targetName);
            if (NativeMethods.CredDelete(targetPointer, CredentialTypeGeneric, flags: 0))
            {
                return ValueTask.FromResult(
                    WindowsCredentialManagerResult.Succeeded(ReadOnlyMemory<byte>.Empty));
            }

            return ValueTask.FromResult(Marshal.GetLastPInvokeError() == ErrorNotFound
                ? WindowsCredentialManagerResult.Missing()
                : WindowsCredentialManagerResult.Failed());
        }
        catch (Exception exception) when (IsNativeBoundaryFailure(exception))
        {
            return ValueTask.FromResult(WindowsCredentialManagerResult.Failed());
        }
        finally
        {
            Marshal.FreeCoTaskMem(targetPointer);
        }
    }

    private static void ValidateIdentity(string targetName, string userName)
    {
        ValidateTarget(targetName);
        if (!string.Equals(userName, WindowsOpenAiCredentialStore.UserName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Credential user name must use the fixed identity.", nameof(userName));
        }
    }

    private static void ValidateTarget(string targetName)
    {
        ArgumentNullException.ThrowIfNull(targetName);
        if (!string.Equals(targetName, WindowsOpenAiCredentialStore.TargetName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Credential target must use the fixed identity.", nameof(targetName));
        }
    }

    private static void ClearAndFreeSecret(nint pointer, int byteCount)
    {
        if (pointer == 0)
        {
            return;
        }

        byte[] zeros = new byte[byteCount];
        Marshal.Copy(zeros, 0, pointer, byteCount);
        Marshal.FreeHGlobal(pointer);
    }

    private static void ClearAndFreeCredential(nint credentialPointer)
    {
        if (credentialPointer == 0)
        {
            return;
        }

        NativeCredential credential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
        if (credential.CredentialBlob != 0
            && credential.CredentialBlobSize is > 0 and <= OpenAiCredentialPolicy.MaximumApiKeyLength)
        {
            byte[] zeros = new byte[credential.CredentialBlobSize];
            Marshal.Copy(zeros, 0, credential.CredentialBlob, zeros.Length);
        }

        NativeMethods.CredFree(credentialPointer);
    }

    private static bool IsNativeBoundaryFailure(Exception exception) =>
        exception is DllNotFoundException
            or EntryPointNotFoundException
            or BadImageFormatException
            or Win32Exception
            or ExternalException;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public nint TargetName;
        public nint Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public nint CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public nint Attributes;
        public nint TargetAlias;
        public nint UserName;
    }

    private static partial class NativeMethods
    {
#pragma warning disable SYSLIB1054
        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CredWrite(ref NativeCredential credential, uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CredRead(
            nint targetName,
            uint type,
            uint flags,
            out nint credential);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CredDelete(nint targetName, uint type, uint flags);

        [DllImport("Advapi32.dll")]
        internal static extern void CredFree(nint buffer);
#pragma warning restore SYSLIB1054
    }
}

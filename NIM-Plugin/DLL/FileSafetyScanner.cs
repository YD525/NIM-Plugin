using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection.Emit;

namespace NIM_Plugin.DLL
{
    /// <summary>
    /// Pure BCL IL scanner.
    /// Reads every method body of a managed DLL and reports calls that match
    /// the file I/O, path query and path enumeration blacklist.
    /// </summary>
    public static class FileSafetyScanner
    {
        // Blacklist: format "Namespace.Type::Method(Param1,Param2)".
        // These strings mirror the exact signatures produced by reflection,
        // so they must NOT be renamed.
        private static readonly HashSet<string> Blacklist = new HashSet<string>(StringComparer.Ordinal)
        {
            // File write / delete / move / copy
            "System.IO.File::WriteAllText(System.String,System.String)",
            "System.IO.File::WriteAllText(System.String,System.String,System.Text.Encoding)",
            "System.IO.File::WriteAllBytes(System.String,System.Byte[])",
            "System.IO.File::WriteAllLines(System.String,System.String[])",
            "System.IO.File::AppendAllText(System.String,System.String)",
            "System.IO.File::AppendAllText(System.String,System.String,System.Text.Encoding)",
            "System.IO.File::Delete(System.String)",
            "System.IO.File::Move(System.String,System.String)",
            "System.IO.File::Copy(System.String,System.String)",
            "System.IO.File::Copy(System.String,System.String,System.Boolean)",
            "System.IO.File::Replace(System.String,System.String,System.String)",
            "System.IO.File::Create(System.String)",
            "System.IO.File::CreateText(System.String)",
            "System.IO.File::OpenWrite(System.String)",
            "System.IO.File::SetAttributes(System.String,System.IO.FileAttributes)",
            "System.IO.File::SetCreationTime(System.String,System.DateTime)",
            "System.IO.File::SetLastWriteTime(System.String,System.DateTime)",

            // Directory write / delete
            "System.IO.Directory::Delete(System.String)",
            "System.IO.Directory::Delete(System.String,System.Boolean)",
            "System.IO.Directory::Move(System.String,System.String)",
            "System.IO.Directory::CreateDirectory(System.String)",

            // File stream / writer constructors
            "System.IO.FileStream::.ctor(System.String,System.IO.FileMode)",
            "System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.IO.FileAccess)",
            "System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.IO.FileAccess,System.IO.FileShare)",
            "System.IO.StreamWriter::.ctor(System.String)",
            "System.IO.StreamWriter::.ctor(System.String,System.Boolean)",
            "System.IO.StreamWriter::.ctor(System.String,System.Boolean,System.Text.Encoding)",
            "System.IO.FileInfo::Delete()",
            "System.IO.FileInfo::MoveTo(System.String)",
            "System.IO.FileInfo::CopyTo(System.String)",
            "System.IO.FileInfo::CopyTo(System.String,System.Boolean)",
            "System.IO.FileInfo::Replace(System.String,System.String)",

            // Path query / combine / resolve
            "System.IO.Path::GetFullPath(System.String)",
            "System.IO.Path::GetFullPath(System.String,System.String)",
            "System.IO.Path::GetDirectoryName(System.String)",
            "System.IO.Path::GetFileName(System.String)",
            "System.IO.Path::GetFileNameWithoutExtension(System.String)",
            "System.IO.Path::GetExtension(System.String)",
            "System.IO.Path::GetPathRoot(System.String)",
            "System.IO.Path::GetTempPath()",
            "System.IO.Path::GetTempFileName()",
            "System.IO.Path::Combine(System.String,System.String)",
            "System.IO.Path::Combine(System.String,System.String,System.String)",
            "System.IO.Path::Combine(System.String[])",
            "System.IO.Path::ChangeExtension(System.String,System.String)",
            "System.IO.Path::HasExtension(System.String)",
            "System.IO.Path::IsPathRooted(System.String)",
            "System.IO.Path::GetRandomFileName()",

            // Path / file / directory existence query
            "System.IO.File::Exists(System.String)",
            "System.IO.Directory::Exists(System.String)",
            "System.IO.File::GetAttributes(System.String)",
            "System.IO.File::GetCreationTime(System.String)",
            "System.IO.File::GetLastWriteTime(System.String)",
            "System.IO.File::GetLastAccessTime(System.String)",
            "System.IO.File::GetCreationTimeUtc(System.String)",
            "System.IO.File::GetLastWriteTimeUtc(System.String)",
            "System.IO.File::GetLastAccessTimeUtc(System.String)",

            // Path enumeration: static Directory
            "System.IO.Directory::GetFiles(System.String)",
            "System.IO.Directory::GetFiles(System.String,System.String)",
            "System.IO.Directory::GetFiles(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::GetDirectories(System.String)",
            "System.IO.Directory::GetDirectories(System.String,System.String)",
            "System.IO.Directory::GetDirectories(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::GetFileSystemEntries(System.String)",
            "System.IO.Directory::GetFileSystemEntries(System.String,System.String)",
            "System.IO.Directory::GetFileSystemEntries(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::EnumerateFiles(System.String)",
            "System.IO.Directory::EnumerateFiles(System.String,System.String)",
            "System.IO.Directory::EnumerateFiles(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::EnumerateDirectories(System.String)",
            "System.IO.Directory::EnumerateDirectories(System.String,System.String)",
            "System.IO.Directory::EnumerateDirectories(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::EnumerateFileSystemEntries(System.String)",
            "System.IO.Directory::EnumerateFileSystemEntries(System.String,System.String)",
            "System.IO.Directory::EnumerateFileSystemEntries(System.String,System.String,System.IO.SearchOption)",
            "System.IO.Directory::GetCurrentDirectory()",
            "System.IO.Directory::GetLogicalDrives()",

            // Path enumeration: DirectoryInfo instance
            "System.IO.DirectoryInfo::GetFiles()",
            "System.IO.DirectoryInfo::GetFiles(System.String)",
            "System.IO.DirectoryInfo::GetFiles(System.String,System.IO.SearchOption)",
            "System.IO.DirectoryInfo::GetDirectories()",
            "System.IO.DirectoryInfo::GetDirectories(System.String)",
            "System.IO.DirectoryInfo::GetDirectories(System.String,System.IO.SearchOption)",
            "System.IO.DirectoryInfo::GetFileSystemInfos()",
            "System.IO.DirectoryInfo::GetFileSystemInfos(System.String)",
            "System.IO.DirectoryInfo::GetFileSystemInfos(System.String,System.IO.SearchOption)",
            "System.IO.DirectoryInfo::EnumerateFiles()",
            "System.IO.DirectoryInfo::EnumerateFiles(System.String)",
            "System.IO.DirectoryInfo::EnumerateFiles(System.String,System.IO.SearchOption)",
            "System.IO.DirectoryInfo::EnumerateDirectories()",
            "System.IO.DirectoryInfo::EnumerateDirectories(System.String)",
            "System.IO.DirectoryInfo::EnumerateDirectories(System.String,System.IO.SearchOption)",
            "System.IO.DirectoryInfo::EnumerateFileSystemInfos()",
            "System.IO.DirectoryInfo::EnumerateFileSystemInfos(System.String)",
            "System.IO.DirectoryInfo::EnumerateFileSystemInfos(System.String,System.IO.SearchOption)",

            // Drive / volume enumeration
            "System.IO.DriveInfo::GetDrives()",
            "System.IO.DriveInfo::.ctor(System.String)",

            // Environment paths
            "System.Environment::GetFolderPath(System.Environment.SpecialFolder)",
            "System.Environment::GetEnvironmentVariable(System.String)",
            "System.Environment::GetEnvironmentVariables()",
            "System.Environment::ExpandEnvironmentVariables(System.String)",
            "System.Environment::GetLogicalDrives()",

            // P/Invoke entry points (native file APIs)
            "CreateFile", "CreateFileW", "CreateFileA",
            "WriteFile", "DeleteFile", "DeleteFileW", "DeleteFileA",
            "MoveFile", "MoveFileW", "MoveFileA",
            "MoveFileEx", "MoveFileExW", "MoveFileExA",
            "CopyFile", "CopyFileW", "CopyFileA",
            "SetFileAttributes", "SetFileAttributesW", "SetFileAttributesA",
            "FindFirstFile", "FindFirstFileW", "FindFirstFileA",
            "FindNextFile", "FindNextFileW", "FindNextFileA",
            "GetFileAttributes", "GetFileAttributesW", "GetFileAttributesA",

             // Process enumeration / query
            "System.Diagnostics.Process::GetProcesses()",
            "System.Diagnostics.Process::GetProcesses(System.String)",
            "System.Diagnostics.Process::GetProcessesByName(System.String)",
            "System.Diagnostics.Process::GetProcessesByName(System.String,System.String)",
            "System.Diagnostics.Process::GetProcessById(System.Int32)",
            "System.Diagnostics.Process::GetProcessById(System.Int32,System.String)",
            "System.Diagnostics.Process::GetCurrentProcess()",

            // Process start / run program
            "System.Diagnostics.Process::Start()",
            "System.Diagnostics.Process::Start(System.String)",
            "System.Diagnostics.Process::Start(System.String,System.String)",
            "System.Diagnostics.Process::Start(System.Diagnostics.ProcessStartInfo)",

            // P/Invoke entry points (native process APIs)
            "CreateProcess", "CreateProcessW", "CreateProcessA",
            "ShellExecute", "ShellExecuteW", "ShellExecuteA", "ShellExecuteEx",
            "WinExec",

            // Registry access (Microsoft.Win32)
            "Microsoft.Win32.Registry::get_CurrentUser()",
            "Microsoft.Win32.Registry::get_LocalMachine()",
            "Microsoft.Win32.Registry::get_ClassesRoot()",
            "Microsoft.Win32.Registry::get_Users()",
            "Microsoft.Win32.Registry::get_CurrentConfig()",
            "Microsoft.Win32.Registry::GetValue(System.String,System.String)",
            "Microsoft.Win32.Registry::GetValue(System.String,System.String,System.Object)",
            "Microsoft.Win32.Registry::SetValue(System.String,System.String,System.Object)",
            "Microsoft.Win32.Registry::SetValue(System.String,System.String,System.Object,Microsoft.Win32.RegistryValueKind)",
            "Microsoft.Win32.Registry::DeleteValue(System.String,System.String)",
            "Microsoft.Win32.Registry::DeleteValue(System.String,System.String,System.Boolean)",

            // Registry key read / query (instance)
            "Microsoft.Win32.RegistryKey::OpenSubKey(System.String)",
            "Microsoft.Win32.RegistryKey::OpenSubKey(System.String,System.Boolean)",
            "Microsoft.Win32.RegistryKey::OpenSubKey(System.String,Microsoft.Win32.RegistryKeyPermissionCheck)",
            "Microsoft.Win32.RegistryKey::OpenSubKey(System.String,Microsoft.Win32.RegistryKeyPermissionCheck,System.Security.AccessControl.RegistryRights)",
            "Microsoft.Win32.RegistryKey::GetValue(System.String)",
            "Microsoft.Win32.RegistryKey::GetValue(System.String,System.Object)",
            "Microsoft.Win32.RegistryKey::GetValue(System.String,System.Object,Microsoft.Win32.RegistryValueOptions)",
            "Microsoft.Win32.RegistryKey::GetValueNames()",
            "Microsoft.Win32.RegistryKey::GetSubKeyNames()",
            "Microsoft.Win32.RegistryKey::GetValueKind(System.String)",

            // Registry key write / delete (instance)
            "Microsoft.Win32.RegistryKey::CreateSubKey(System.String)",
            "Microsoft.Win32.RegistryKey::CreateSubKey(System.String,Microsoft.Win32.RegistryKeyPermissionCheck)",
            "Microsoft.Win32.RegistryKey::CreateSubKey(System.String,Microsoft.Win32.RegistryKeyPermissionCheck,Microsoft.Win32.RegistryOptions)",
            "Microsoft.Win32.RegistryKey::CreateSubKey(System.String,System.Boolean)",
            "Microsoft.Win32.RegistryKey::CreateSubKey(System.String,System.Boolean,System.Boolean)",
            "Microsoft.Win32.RegistryKey::SetValue(System.String,System.Object)",
            "Microsoft.Win32.RegistryKey::SetValue(System.String,System.Object,Microsoft.Win32.RegistryValueKind)",
            "Microsoft.Win32.RegistryKey::DeleteSubKey(System.String)",
            "Microsoft.Win32.RegistryKey::DeleteSubKey(System.String,System.Boolean)",
            "Microsoft.Win32.RegistryKey::DeleteSubKeyTree(System.String)",
            "Microsoft.Win32.RegistryKey::DeleteSubKeyTree(System.String,System.Boolean)",
            "Microsoft.Win32.RegistryKey::DeleteValue(System.String)",
            "Microsoft.Win32.RegistryKey::DeleteValue(System.String,System.Boolean)",

            // P/Invoke entry points (native registry APIs)
            "RegOpenKey", "RegOpenKeyEx", "RegOpenKeyExW", "RegOpenKeyExA",
            "RegCreateKey", "RegCreateKeyEx", "RegCreateKeyExW", "RegCreateKeyExA",
            "RegSetValue", "RegSetValueEx", "RegSetValueExW", "RegSetValueExA",
            "RegQueryValue", "RegQueryValueEx", "RegQueryValueExW", "RegQueryValueExA",
            "RegDeleteKey", "RegDeleteKeyW", "RegDeleteKeyA",
            "RegDeleteValue", "RegDeleteValueW", "RegDeleteValueA",
            "RegEnumKey", "RegEnumKeyEx", "RegEnumKeyExW", "RegEnumKeyExA",
            "RegEnumValue", "RegEnumValueW", "RegEnumValueA",
        };

        // Cached opcode table built from System.Reflection.Emit.OpCodes at first use.
        private static Dictionary<short, OpCode> OpCodeMapCache;

        private static Dictionary<short, OpCode> OpCodeMap
        {
            get
            {
                if (OpCodeMapCache == null)
                {
                    OpCodeMapCache = BuildOpCodeMap();
                }
                return OpCodeMapCache;
            }
        }

        /// <summary>
        /// Scans the given DLL and returns every blacklist hit.
        /// </summary>
        /// <param name="DllPath">Absolute path to the managed DLL.</param>
        /// <returns>Result object; IsSafe is true when no violation was found.</returns>
        public static DllScanResult Scan(string DllPath)
        {
            var Result = new DllScanResult { IsSafe = true };

            Assembly Assembly;
            try
            {
                // ReflectionOnlyLoadFrom does NOT execute the plugin's static ctor.
                // .NET Framework only. On .NET Core replace with Assembly.LoadFrom.
                Assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(DllPath);
            }
            catch (Exception Ex)
            {
                Result.Violations.Add("Load failed: " + Ex.Message);
                Result.IsSafe = false;
                return Result;
            }

            Type[] Types;
            try
            {
                Types = Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException Ex)
            {
                Types = Ex.Types;
            }

            foreach (var Type in Types)
            {
                if (Type == null)
                {
                    continue;
                }

                MethodBase[] Methods;
                try
                {
                    Methods = Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Static | BindingFlags.Instance |
                                              BindingFlags.DeclaredOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var Method in Methods)
                {
                    ScanMethod(Method, Result);
                }

                // Constructors are not returned by GetMethods, scan them separately.
                ConstructorInfo[] Constructors;
                try
                {
                    Constructors = Type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.Static | BindingFlags.Instance);
                }
                catch
                {
                    Constructors = null;
                }

                if (Constructors != null)
                {
                    foreach (var Constructor in Constructors)
                    {
                        ScanMethod(Constructor, Result);
                    }
                }
            }

            Result.IsSafe = Result.Violations.Count == 0;
            return Result;
        }

        /// <summary>
        /// Scans one method: checks P/Invoke entry point first, then walks IL.
        /// </summary>
        /// <param name="Method">Method whose body is inspected.</param>
        /// <param name="Result">Result object receiving violations.</param>
        private static void ScanMethod(MethodBase Method, DllScanResult Result)
        {
            // P/Invoke: match against DllImport entry point before walking IL.
            DllImportAttribute DllImport = null;
            try
            {
                var Attributes = Method.GetCustomAttributes(typeof(DllImportAttribute), false);
                if (Attributes.Length > 0)
                {
                    DllImport = (DllImportAttribute)Attributes[0];
                }
            }
            catch
            {
                // Reflection-only context may fail; ignore.
            }

            if (DllImport != null)
            {
                string EntryPoint = string.IsNullOrEmpty(DllImport.EntryPoint)
                    ? Method.Name
                    : DllImport.EntryPoint;

                if (Blacklist.Contains(EntryPoint))
                {
                    Result.Violations.Add(string.Format(
                        "P/Invoke: {0}::{1} -> {2}!{3}",
                        Method.DeclaringType != null ? Method.DeclaringType.FullName : "<global>",
                        Method.Name, DllImport.Value, EntryPoint));
                }
                return;
            }

            MethodBody Body;
            try
            {
                Body = Method.GetMethodBody();
            }
            catch
            {
                return;
            }

            if (Body == null)
            {
                return;
            }

            byte[] IlBytes;
            try
            {
                IlBytes = Body.GetILAsByteArray();
            }
            catch
            {
                return;
            }

            if (IlBytes == null || IlBytes.Length == 0)
            {
                return;
            }

            Module Module = Method.Module;
            int Index = 0;

            while (Index < IlBytes.Length)
            {
                int Offset = Index;
                byte FirstByte = IlBytes[Index++];
                short Code = FirstByte == 0xFE
                    ? (short)(0xFE00 | IlBytes[Index++])
                    : FirstByte;

                if (!OpCodeMap.TryGetValue(Code, out var Instruction))
                {
                    return;
                }

                int OperandPosition = Index;

                switch (Instruction.OperandType)
                {
                    case OperandType.InlineNone:
                        break;
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                    case OperandType.ShortInlineBrTarget:
                        Index += 1;
                        break;
                    case OperandType.InlineVar:
                        Index += 2;
                        break;
                    case OperandType.InlineI:
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineString:
                    case OperandType.InlineSig:
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineType:
                    case OperandType.InlineTok:
                    case OperandType.ShortInlineR:
                        Index += 4;
                        break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        Index += 8;
                        break;
                    case OperandType.InlineSwitch:
                        if (OperandPosition + 4 > IlBytes.Length)
                        {
                            return;
                        }
                        int SwitchCount = BitConverter.ToInt32(IlBytes, OperandPosition);
                        Index += 4 + SwitchCount * 4;
                        break;
                    default:
                        return;
                }

                short OpCodeValue = Instruction.Value;
                bool IsMethodCall =
                    OpCodeValue == OpCodes.Call.Value ||
                    OpCodeValue == OpCodes.Callvirt.Value ||
                    OpCodeValue == OpCodes.Newobj.Value ||
                    OpCodeValue == OpCodes.Ldftn.Value ||
                    OpCodeValue == OpCodes.Ldvirtftn.Value;

                if (!IsMethodCall)
                {
                    continue;
                }

                if (OperandPosition + 4 > IlBytes.Length)
                {
                    return;
                }

                int Token = BitConverter.ToInt32(IlBytes, OperandPosition);

                MethodBase Target;
                try
                {
                    Target = Module.ResolveMethod(Token);
                }
                catch
                {
                    continue;
                }

                if (Target == null)
                {
                    continue;
                }

                string Signature = FormatSignature(Target);

                if (Blacklist.Contains(Signature))
                {
                    Result.Violations.Add(string.Format(
                        "Method: {0}::{1} -> {2} (IL_{3:X4})",
                        Method.DeclaringType != null ? Method.DeclaringType.FullName : "<global>",
                        Method.Name, Signature, Offset));
                }
            }
        }

        /// <summary>
        /// Formats a MethodBase into "Namespace.Type::Method(Param1,Param2)".
        /// Must match the blacklist string format exactly.
        /// </summary>
        /// <param name="Method">Target method.</param>
        /// <returns>Normalized signature string.</returns>
        private static string FormatSignature(MethodBase Method)
        {
            string TypeName = Method.DeclaringType != null
                ? Method.DeclaringType.FullName
                : "<global>";

            string MethodName = Method is ConstructorInfo ? ".ctor" : Method.Name;

            ParameterInfo[] Parameters = Method.GetParameters();
            if (Parameters.Length == 0)
            {
                return TypeName + "::" + MethodName + "()";
            }

            var Builder = new StringBuilder();
            Builder.Append(TypeName);
            Builder.Append("::");
            Builder.Append(MethodName);
            Builder.Append('(');

            for (int I = 0; I < Parameters.Length; I++)
            {
                if (I > 0)
                {
                    Builder.Append(',');
                }

                Type ParameterType = Parameters[I].ParameterType;
                Builder.Append(ParameterType.FullName ?? ParameterType.Name);
            }

            Builder.Append(')');
            return Builder.ToString();
        }

        /// <summary>
        /// Builds the opcode lookup table from System.Reflection.Emit.OpCodes.
        /// </summary>
        /// <returns>Dictionary keyed by the short opcode value.</returns>
        private static Dictionary<short, OpCode> BuildOpCodeMap()
        {
            var Map = new Dictionary<short, OpCode>();

            FieldInfo[] Fields = typeof(System.Reflection.Emit.OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static);

            foreach (var Field in Fields)
            {
                if (Field.FieldType != typeof(System.Reflection.Emit.OpCode))
                {
                    continue;
                }

                var OpCodeEntry = (System.Reflection.Emit.OpCode)Field.GetValue(null);
                Map[OpCodeEntry.Value] = OpCodeEntry;
            }

            return Map;
        }
    }

    /// <summary>
    /// Result of a static scan performed by FileSafetyScanner.
    /// </summary>
    public sealed class DllScanResult
    {
        /// <summary>
        /// True when no violation was found.
        /// </summary>
        public bool IsSafe { get; set; }

        /// <summary>
        /// Human-readable list of every violation found in the assembly.
        /// </summary>
        public List<string> Violations { get; } = new List<string>();

        /// <summary>
        /// Returns a short summary suitable for logging.
        /// </summary>
        public override string ToString()
        {
            return IsSafe
                ? "SAFE"
                : "BLOCKED: " + string.Join(" | ", Violations);
        }
    }
}
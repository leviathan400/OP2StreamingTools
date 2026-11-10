using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// MemoryReader
// A utility library for reading memory from external processes
// Provides functionality to read integers and strings from process memory
// using both module-relative offsets and absolute memory addresses

namespace OP2Memory
{
    /// <summary>
    /// Provides static methods for reading memory from external processes.
    /// Uses Windows API functions to access process memory safely.
    /// </summary>
    public static class MemoryReader
    {
        #region Windows API Imports

        /// <summary>
        /// Opens an existing local process object for memory access.
        /// </summary>
        /// <param name="dwDesiredAccess">The access to the process object</param>
        /// <param name="bInheritHandle">Whether the returned handle can be inherited</param>
        /// <param name="processId">The identifier of the local process to be opened</param>
        /// <returns>Handle to the opened process, or IntPtr.Zero if failed</returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int processId);

        /// <summary>
        /// Reads data from an area of memory in a specified process.
        /// </summary>
        /// <param name="hProcess">Handle to the process with memory that is being read</param>
        /// <param name="lpBaseAddress">Pointer to the base address in the specified process from which to read</param>
        /// <param name="lpBuffer">Pointer to a buffer that receives the contents from the address space of the specified process</param>
        /// <param name="size">Number of bytes to be read from the specified process</param>
        /// <param name="bytesRead">Number of bytes transferred into the specified buffer</param>
        /// <returns>True if the function succeeds, false otherwise</returns>
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int size, out int bytesRead);

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="hObject">A valid handle to an open object</param>
        /// <returns>True if the function succeeds, false otherwise</returns>
        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Constants

        /// <summary>
        /// Required to read memory in a process using ReadProcessMemory.
        /// </summary>
        const int PROCESS_VM_READ = 0x0010;

        /// <summary>
        /// Required to retrieve certain information about a process.
        /// </summary>
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a 32-bit integer from a process memory location specified by module base address + offset.
        /// </summary>
        /// <param name="processName">Name of the target process (without .exe extension)</param>
        /// <param name="offset">Offset from the main module's base address</param>
        /// <returns>The 32-bit integer value read from memory</returns>
        /// <exception cref="Exception">Thrown when process is not found or memory read fails</exception>
        public static int ReadIntFromModuleOffset(string processName, int offset)
        {
            // Find the target process by name
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception($"Process '{processName}' not found");

            Process process = processes[0];

            // Open the process with required permissions for memory reading
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");

            try
            {
                // Calculate target address: main module base + offset
                IntPtr baseAddress = process.MainModule.BaseAddress;
                IntPtr targetAddress = IntPtr.Add(baseAddress, offset);

                // Read 4 bytes (32-bit integer) from the target address
                byte[] buffer = new byte[4];
                bool success = ReadProcessMemory(hProcess, targetAddress, buffer, buffer.Length, out _);

                if (!success)
                    throw new Exception("Failed to read process memory");

                // Convert the byte array to a 32-bit integer
                return BitConverter.ToInt32(buffer, 0);
            }
            finally
            {
                // Always close the process handle to prevent resource leaks
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// Reads an ASCII string from a process memory location specified by module base address + offset.
        /// </summary>
        /// <param name="processName">Name of the target process (without .exe extension)</param>
        /// <param name="offset">Offset from the module's base address</param>
        /// <param name="length">Maximum number of bytes to read for the string</param>
        /// <param name="moduleName">Name of the specific module to use as base (optional, defaults to main module)</param>
        /// <returns>The ASCII string read from memory, with null terminators removed</returns>
        /// <exception cref="Exception">Thrown when process/module is not found or memory read fails</exception>
        public static string ReadStringFromModuleOffset(string processName, int offset, int length, string moduleName = null)
        {
            // Find the target process by name
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception($"Process '{processName}' not found");

            Process process = processes[0];

            // Open the process with required permissions for memory reading
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");

            try
            {
                IntPtr baseAddress;

                if (string.IsNullOrEmpty(moduleName))
                {
                    // Use main executable's base address (existing behavior)
                    baseAddress = process.MainModule.BaseAddress;
                }
                else
                {
                    // Find the specific module within the process
                    ProcessModule targetModule = null;
                    foreach (ProcessModule module in process.Modules)
                    {
                        if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        {
                            targetModule = module;
                            break;
                        }
                    }

                    if (targetModule == null)
                        throw new Exception($"Module '{moduleName}' not found in process '{processName}'");

                    baseAddress = targetModule.BaseAddress;
                }

                // Calculate target address: module base + offset
                IntPtr targetAddress = IntPtr.Add(baseAddress, offset);

                // Read the specified number of bytes for the string
                byte[] buffer = new byte[length];
                bool success = ReadProcessMemory(hProcess, targetAddress, buffer, buffer.Length, out _);
                if (!success)
                    throw new Exception("Failed to read process memory");

                // Convert bytes to ASCII string and remove null terminators
                return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
            }
            finally
            {
                // Always close the process handle to prevent resource leaks
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// Reads a 32-bit integer from a process memory location specified by absolute address.
        /// </summary>
        /// <param name="processName">Name of the target process (without .exe extension)</param>
        /// <param name="address">Absolute memory address to read from</param>
        /// <returns>The 32-bit integer value read from memory</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when process is not found</exception>
        public static int ReadIntFromAbsoluteAddress(string processName, IntPtr address)
        {
            // Find the target process by name
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception($"Process '{processName}' not found");

            Process process = processes[0];

            // Open the process with required permissions for memory reading
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");

            try
            {
                // Read 4 bytes (32-bit integer) from the absolute address
                byte[] buffer = new byte[4];
                bool success = ReadProcessMemory(hProcess, address, buffer, buffer.Length, out _);

                if (!success)
                    throw new Exception("Failed to read process memory");

                // Convert the byte array to a 32-bit integer
                return BitConverter.ToInt32(buffer, 0);
            }
            finally
            {
                // Always close the process handle to prevent resource leaks
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// Reads an ASCII string from a process memory location specified by absolute address.
        /// </summary>
        /// <param name="processName">Name of the target process (without .exe extension)</param>
        /// <param name="address">Absolute memory address to read from</param>
        /// <param name="length">Maximum number of bytes to read for the string</param>
        /// <returns>The ASCII string read from memory, with null terminators removed</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when process is not found</exception>
        public static string ReadStringFromAbsoluteAddress(string processName, IntPtr address, int length)
        {
            // Find the target process by name
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                throw new Exception($"Process '{processName}' not found");

            Process process = processes[0];

            // Open the process with required permissions for memory reading
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process");

            try
            {
                // Read the specified number of bytes for the string
                byte[] buffer = new byte[length];
                bool success = ReadProcessMemory(hProcess, address, buffer, buffer.Length, out _);

                if (!success)
                    throw new Exception("Failed to read process memory");

                // Convert bytes to ASCII string and remove null terminators
                return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
            }
            finally
            {
                // Always close the process handle to prevent resource leaks
                CloseHandle(hProcess);
            }
        }

        #endregion
    }
}
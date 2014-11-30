using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ModoStream
{
    class Program
    {
        static Assembly lib = Assembly.Load(ModoStream.Properties.Resources.LeaguesharpStreamingModelib); 
        static void Main(string[] args)
        {
            SetUpOffsets();
            Enable();

            LeagueSharp.Game.OnWndProc += OnWndProc;
            AppDomain.CurrentDomain.DomainUnload += delegate
            {
                Disable();
            };
        }

        static Int32 GetModuleAddress(String ModuleName)
        {
            Process P = Process.GetCurrentProcess();
            for (int i = 0; i < P.Modules.Count; i++)
                if (P.Modules[i].ModuleName == ModuleName)
                    return (Int32)(P.Modules[i].BaseAddress);
            return 0;
        }

        static byte[] ReadMemory(Int32 address, Int32 length)
        {
            MethodInfo _ReadMemory = lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[2];
            return (byte[])_ReadMemory.Invoke(null, new object[] { address, length });  
        }

        static void WriteMemory(Int32 address, byte value)
        {
            MethodInfo _WriteMemory = lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[4];
            _WriteMemory.Invoke(null, new object[] { address, value });
        }

        static void WriteMemory(Int32 address, byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
                WriteMemory(address + i, array[i]);
        }

        static string version = LeagueSharp.Game.Version.Substring(0, 4);
        static Int32 LeaguesharpCore = GetModuleAddress("Leaguesharp.Core.dll");
        static Dictionary<string, Int32[]> offsets;

        enum functionOffset : int
        {
            drawEvent = 0,
            printChat = 1,
            watermark1 = 2
        }

        enum asm : byte
        {
            ret = 0xC3,
            push_ebp = 0x55,
            nop = 0x90
        }

        static void SetUpOffsets()
        {
            offsets = new Dictionary<string, Int32[]>();
            offsets.Add("4.19", new Int32[] { 0x5F40, 0x9B60, 0x9B40 });
            offsets.Add("4.20", new Int32[] { 0x5F60, 0x9B80, 0x9B60 });
        }

        static void Enable()
        {
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.drawEvent], (byte)asm.ret);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], (byte)asm.ret);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.watermark1], new byte[] { (byte)asm.nop, (byte)asm.nop, (byte)asm.nop, 
                                                                                                         (byte)asm.nop, (byte)asm.nop, (byte)asm.nop });
           // Marshal.WriteByte(new IntPtr(LeaguesharpCore + offsets[version][(int)functionOffset.printChat]), 0xC3);
        }

        static void Disable()
        {
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.drawEvent], (byte)asm.push_ebp);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], (byte)asm.push_ebp);
        }

        static bool IsEnabled() { return ReadMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], 1)[0] == (byte)asm.ret; } //(Marshal.ReadByte(new IntPtr(LeaguesharpCore + offsets[version][(int)functionOffset.printChat])) == (byte)asm.ret); }

        static uint hotkey = 0x24;  //home key
        static void OnWndProc(LeagueSharp.WndEventArgs args)
        {
            if (args.Msg == 0x100) //WM_KEYDOWN
            {
                if (args.WParam == hotkey)
                {
                    if (IsEnabled())
                        Disable();
                    else
                        Enable();
                }
            }
        }
    }
}

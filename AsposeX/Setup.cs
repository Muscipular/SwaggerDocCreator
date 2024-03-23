using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace AsposeX;

public static class Setup
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static Harmony _harmony;
    private static readonly object Locker = new();

    public static void Do()
    {
        lock (Locker)
        {
            if (_harmony != null)
            {
                return;
            }

            string key =
                "PExpY2Vuc2U+DQogIDxEYXRhPg0KICAgIDxMaWNlbnNlZFRvPkFzcG9zZSBTY290bGFuZCB" +
                "UZWFtPC9MaWNlbnNlZFRvPg0KICAgIDxFbWFpbFRvPmJpbGx5Lmx1bmRpZUBhc3Bvc2UuY2" +
                "9tPC9FbWFpbFRvPg0KICAgIDxMaWNlbnNlVHlwZT5EZXZlbG9wZXIgT0VNPC9MaWNlbnNlV" +
                "HlwZT4NCiAgICA8TGljZW5zZU5vdGU+TGltaXRlZCB0byAxIGRldmVsb3BlciwgdW5saW1p" +
                "dGVkIHBoeXNpY2FsIGxvY2F0aW9uczwvTGljZW5zZU5vdGU+DQogICAgPE9yZGVySUQ+MTQ" +
                "wNDA4MDUyMzI0PC9PcmRlcklEPg0KICAgIDxVc2VySUQ+OTQyMzY8L1VzZXJJRD4NCiAgIC" +
                "A8T0VNPlRoaXMgaXMgYSByZWRpc3RyaWJ1dGFibGUgbGljZW5zZTwvT0VNPg0KICAgIDxQc" +
                "m9kdWN0cz4NCiAgICAgIDxQcm9kdWN0PkFzcG9zZS5Ub3RhbCBmb3IgLk5FVDwvUHJvZHVj" +
                "dD4NCiAgICA8L1Byb2R1Y3RzPg0KICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl" +
                "0aW9uVHlwZT4NCiAgICA8U2VyaWFsTnVtYmVyPjlhNTk1NDdjLTQxZjAtNDI4Yi1iYTcyLT" +
                "djNDM2OGYxNTFkNzwvU2VyaWFsTnVtYmVyPg0KICAgIDxTdWJzY3JpcHRpb25FeHBpcnk+M" +
                "jAxNTEyMzE8L1N1YnNjcmlwdGlvbkV4cGlyeT4NCiAgICA8TGljZW5zZVZlcnNpb24+My4w" +
                "PC9MaWNlbnNlVmVyc2lvbj4NCiAgICA8TGljZW5zZUluc3RydWN0aW9ucz5odHRwOi8vd3d" +
                "3LmFzcG9zZS5jb20vY29ycG9yYXRlL3B1cmNoYXNlL2xpY2Vuc2UtaW5zdHJ1Y3Rpb25zLm" +
                "FzcHg8L0xpY2Vuc2VJbnN0cnVjdGlvbnM+DQogIDwvRGF0YT4NCiAgPFNpZ25hdHVyZT5GT" +
                "zNQSHNibGdEdDhGNTlzTVQxbDFhbXlpOXFrMlY2RThkUWtJUDdMZFRKU3hEaWJORUZ1MXpP" +
                "aW5RYnFGZkt2L3J1dHR2Y3hvUk9rYzF0VWUwRHRPNmNQMVpmNkowVmVtZ1NZOGkvTFpFQ1R" +
                "Hc3pScUpWUVJaME1vVm5CaHVQQUprNWVsaTdmaFZjRjhoV2QzRTRYUTNMemZtSkN1YWoyTk" +
                "V0ZVJpNUhyZmc9PC9TaWduYXR1cmU+DQo8L0xpY2Vuc2U+";
            Stream lStream = new MemoryStream(Convert.FromBase64String(key));
            _harmony = new Harmony("AsposeX");

            PathWord();
            PathCells();
            PathPdf();
            PathHtml();
            PathBarCode();
            SetLicense(new Aspose.Cells.License().SetLicense, lStream);
            SetLicense(new Aspose.Words.License().SetLicense, lStream);
            SetLicense(new Aspose.Pdf.License().SetLicense, lStream);
            SetLicense(new Aspose.Html.License().SetLicense, lStream);
            SetLicense(new Aspose.BarCode.License().SetLicense, lStream);

            // harmony.UnpatchAll();
        }
    }

    private static void PathHtml()
    {
        TransCode<Aspose.Html.License, HtmlHook>.Do("\u0003\u2008", "\u0006\u2000", "\u0003",
            "\u0003\u2002", "\u0008\u2001", "\u0006\u2000", "\u0008\u2004");
        // PatchMethodCall();
    }
    private static void PathBarCode()
    {
        TransCode<Aspose.BarCode.License, BarCodeHook>.Do("\u0005\u0018", "\u0008\u0002", "\u0006\u0002",
            "\u0008", "\u0005\u0010", "\u0006", "\u0002\u0005");
        // PatchMethodCall();
    }

    public interface IHook
    {
        void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret);
    }

    public class HtmlHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            if ((uint)totalLength.GetValue(instance) == 0x301A && (uint)curPos.GetValue(instance) == 0x2F3D)
            {
                var value = ret.GetValue(instance);
                value.GetType().GetField("\u0002", Flags).SetValue(value, 0);
            }
        }
    }
    public class BarCodeHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            if ((uint)totalLength.GetValue(instance) == 0x3C56 && (uint)curPos.GetValue(instance) == 0x38A2)
            {
                var value = ret.GetValue(instance);
                value.GetType().GetField("\u0002", Flags).SetValue(value, 0);
            }
        }
    }

    public static class TransCode<T, THook> where THook : IHook, new()
    {
        private static MethodInfo _runMethod;
        private static FieldInfo _totalLength, _gotoPos, _curPos, _ret;
        private static FastInvokeHandler _oCall;

        public static void Do(string className, string warpMethod, string runMethod, string total, string sGoto,
            string sCur, string sRet)
        {
            var coreType = GetTypeIn(typeof(T).Assembly, className);
            //\u0003(global::\u000F\u2001
            var method = coreType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(e =>
                    e.Name == warpMethod && e.GetParameters().Length == 1 &&
                    e.GetParameters()[0].ParameterType == typeof(bool));
            var method2 = coreType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(e =>
                    e.Name == runMethod && e.GetParameters().Length == 0 && e.ReturnType == typeof(void));
            var d = Transpiler;
            _oCall = HarmonyLib.MethodInvoker.GetHandler(method2);
            _totalLength = AccessTools.Field(coreType, total);
            _gotoPos = AccessTools.Field(coreType, sGoto);
            _curPos = AccessTools.Field(coreType, sCur);
            _ret = AccessTools.Field(coreType, sRet);
            _runMethod = method2;
            _harmony.Patch(method, transpiler: new HarmonyMethod(d.Method));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Console.WriteLine("do trans");
            var codeInstructions = instructions.ToList();
            bool found = false;
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].opcode == OpCodes.Call && codeInstructions[i].operand == _runMethod)
                {
                    var fn = Patcher;
                    codeInstructions[i] = new CodeInstruction(OpCodes.Call, fn.Method);
                    found = true;
                }
            }

            if (!found)
            {
                var sb = new StringBuilder().Append("not found method ").Append(SName(_runMethod.FullDescription()))
                    .AppendLine();
                foreach (var instruction in instructions)
                {
                    sb.Append(SName(instruction.ToString()));
                    sb.AppendLine();
                }

                throw new Exception(sb.ToString());
            }

            return codeInstructions;
        }


        public static void Patcher(object instance)
        {
            _oCall(instance);
            new THook().Hook(instance, _totalLength, _gotoPos, _curPos, _ret);
            //*/
        }
    }

    public class DebugHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            // if (gotoPos.GetValue(instance) != null)
            // {
            //     var fieldInfo2 = instance.GetType().GetField("\u0023\u003Dz9KXz7AY\u003D", Flags);
            //     var val = fieldInfo2.GetValue(instance) as object[];
            //     if (val.Length > 0)
            //     {
            //         for (var index = 0; index < val.Length; index++)
            //         {
            //             var v = val[index];
            //             // Console.WriteLine($" >>> {index} {SObj(v)}");
            //         }
            //     }
            // }

            var o = ret.GetValue(instance);
            var res = SObj(o);
            if ((uint)totalLength.GetValue(instance) != 0x015C)
            {

                Console.WriteLine(
                    $"{(gotoPos.GetValue(instance) != null ? " >>> " : " <<< ")} RW: 0x{curPos.GetValue(instance):X4} => 0x{gotoPos.GetValue(instance) ?? 0:X4} 0x{totalLength.GetValue(instance):X4} RES: {res}");
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }

    private static void PathPdf()
    {
        TransCode<Aspose.Pdf.License, PdfHook>.Do(
            "\u0023\u003Dq\u0024vkZ_ki0\u0024v2pZcPEyTBryDH8wHPiquOozQRIeDwG5b8\u003D",
            "#=zr2qJCoJt4nmVRvSXjROF50VYOEgb",
            "#=zuKv3Nn7G3a3UEacA3UnqTCMS_LzZ8gE3geLrJki7NwFI",
            "\u0023\u003DzeHjAAao\u003D", "\u0023\u003DzZwx_6Fc\u003D", "\u0023\u003DzBGpLSTs\u003D",
            "\u0023\u003DzjmFaA1o\u003D");
    }

    public class PdfHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            var o = ret.GetValue(instance);
            if ((uint?)totalLength.GetValue(instance) == 0x2EB0)
            {
                if ((uint?)curPos.GetValue(instance) == 0x2D10)
                {
                    o.GetType().GetField("\u0023\u003dzdDLebNo\u003d", Flags).SetValue(o, new DateTime(2010, 01, 01));
                }
            }
        }
    }

    private static Type GetTypeIn(Assembly a, string name)
    {
        Type[] types;
        try
        {
            types = a.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types;
        }

        return types.FirstOrDefault(e => e.FullName == name);
    }

    private static void PathWord()
    {
        TransCode<Aspose.Words.License, WordHook>.Do(
            "U",
            "Y",
            "b",
            "t", "H", "B",
            "m3");
        //b
    }

    public class WordHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            if ((uint)totalLength.GetValue(instance) == 0x14B)
            {
                if ((uint)curPos.GetValue(instance) == 0x72)
                {
                    gotoPos.SetValue(instance, new uint?());
                }
            }
        }
    }

    private static void PatchMethodCall()
    {
        var method = MethodBase.GetCurrentMethod();
        Func<object, object[], object> a = method.Invoke;
        var k = MethodCall;
        _harmony.Patch(a.Method, postfix: new HarmonyMethod(k.Method));
    }

    private static void PathCells()
    {
        TransCode<Aspose.Cells.License, CellHook>.Do(
            "d",
            "\u000e\u000f",
            "\u000F",
            "\u0008", "\u0008\u000F", "\u0005",
            "\u000E\u0005");
        // PatchMethodCall();
    }

    public class CellHook : IHook
    {
        public void Hook(object instance, FieldInfo totalLength, FieldInfo gotoPos, FieldInfo curPos, FieldInfo ret)
        {
            // new DebugHook().Hook(instance, totalLength, gotoPos, curPos, ret);
            var o = ret.GetValue(instance);
            var totalLenValue = (uint?)totalLength.GetValue(instance);
            if (totalLenValue == 0x04EF)
            {
                if ((uint?)curPos.GetValue(instance) == 0x03D2)
                {
                    o.GetType().GetField("\u0002", Flags).SetValue(o, "20110101".ToCharArray());
                }
            }

            if (totalLenValue == 0xE5)
            {
                if ((uint?)curPos.GetValue(instance) == 0xD9)
                {
                    o.GetType().GetField("\u0002", Flags).SetValue(o, 0);
                }
            }
        }
    }

    // ReSharper disable InconsistentNaming
    private static void MethodCall(MethodBase __instance, object[] __args, object __result)
    {
        Console.WriteLine($"Method Call : {SName(__instance.FullDescription())}");
        Console.WriteLine("this: " + __args[0]);
        var arg = __args[1] as object[];
        for (var index = 0; index < arg.Length; index++)
        {
            var o = arg[index];
            Console.WriteLine("arg" + index + ": " + o);
        }

        Console.WriteLine("result: " + SName(__result?.GetType().FullName) + " " + __result);
        // PrintStack();

        Console.WriteLine();
    }

    private static void PrintStack()
    {
        foreach (var frame in new StackTrace().GetFrames())
        {
            var method = frame.GetMethod();
            if (method != null)
            {
                Console.Write(SName(method.DeclaringType?.FullName) + "." + SName(method.Name));
                Console.Write("(");
                Console.Write(string.Join(", ",
                    method.GetParameters().Select(e => $"{SName(e.ParameterType.FullName)} {SName(e.Name)}")));
                Console.WriteLine(") => " + SName((method as MethodInfo)?.ReturnType.FullDescription() ?? ""));
            }
        }
    }

    private static string XToString(object n)
    {
        if (n is string[] ss)
        {
            return string.Join(" ", ss);
        }

        if (n is char[] cc)
        {
            return "Char[] " + string.Join("", cc);
        }

        if (n is byte[] dd)
        {
            StringBuilder s = new StringBuilder();
            foreach (var b in dd)
            {
                s.AppendFormat("{0:X}", b);
            }

            return s.ToString();
        }

        return n?.ToString();
    }

    private static object SObj(object o)
    {
        if (o != null)
        {
            var type = o.GetType();
            var sName = SName(type.FullName);
            const int length = 80;
            foreach (var f in type.GetFields(Flags))
            {
                var a = XToString(f.GetValue(o));
                if (a != null && a.Length > length)
                {
                    a = a.Substring(0, length);
                }

                sName += $", {SName(f.Name)} : {a?.ToString()?.Replace("\n", "").Replace("\r", "")}";
            }

            foreach (var f in type.BaseType.GetFields(Flags))
            {
                var a = XToString(f.GetValue(o));
                if (a != null && a.Length > length)
                {
                    a = a.Substring(0, length);
                }

                sName += $", B{SName(f.Name)} : {a?.ToString()?.Replace("\n", "").Replace("\r", "")}";
            }

            o = sName;
        }
        else
        {
            o = "null";
        }

        return o;
    }

    private static string SName(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return "";
        }

        return string.Join("",
            s.Select(e =>
                Char.IsLetterOrDigit(e) || "._#=+<>:() ,{}[]`~@$%^&*-|\\/?;'\"".IndexOf(e) >= 0
                    ? e.ToString()
                    : ("\\u" + ((long)e).ToString("x4"))));
    }

    private static void SetLicense(Action<Stream> license, Stream stream)
    {
        stream.Position = 0;
        // SetHtml(harmony);
        license(stream);
        stream.Position = 0;
    }
}
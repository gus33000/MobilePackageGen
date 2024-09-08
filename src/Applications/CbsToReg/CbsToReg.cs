using System.Text;
using System.Text.RegularExpressions;

namespace CbsToReg
{
    public partial class CbsToReg
    {
        private readonly List<RegistryCollection> _registries;

        //ctor
        public CbsToReg()
        {
            _registries = [];
        }

        public void Add(RegistryCollection reg)
        {
            _registries.Add(reg);
        }

        private static string KeyNameReplace(string s, string software, string system)
        {
            return s.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE", "HKEY_LOCAL_MACHINE\\" + software, StringComparison.InvariantCultureIgnoreCase).Replace("HKEY_LOCAL_MACHINE\\SYSTEM", "HKEY_LOCAL_MACHINE\\" + system, StringComparison.InvariantCultureIgnoreCase);
        }

        public string Build(string softwareName, string systemName)
        {
            StringBuilder str = new();
            str.Append("Windows Registry Editor Version 5.00\r\n\r\n");

            foreach (RegistryCollection registry in _registries)
            {
                str.Append("[" + KeyNameReplace(registry.KeyName, softwareName, systemName) + "]" + "\r\n");

                foreach (RegistryValue registryValue in registry.RegistryValues)
                {
                    string? keyName = registryValue.Name == "" ? "@" : "\"" + registryValue.Name + "\"";

                    str.Append(keyName.Replace("\\", @"\\") + "=" + ConvertValueToString(registryValue.Value, registryValue.ValueType) + "\r\n");
                }
                str.Append("\r\n");
            }
            return str.ToString();
        }

        private string ConvertValueToString(string value, string valueType)
        {
            value ??= "";

            if (valueType == "REG_DWORD")
            {
                return "dword:" + value.Replace("0x", "");
            }

            if (valueType == "REG_QWORD")
            {
                return "hex(b):" + string.Join(",", SplitInParts(value, 2));
            }
            else if (valueType == "REG_SZ")
            {
                return "\"" + value.Replace(@"\", @"\\") + "\"";
            }
            else if (valueType == "REG_EXPAND_SZ")
            {
                return "hex(2):" + string.Join(",", SplitInParts(ToHex(value.Replace("\"", "") + "\0"), 2));
            }
            else if (valueType == "REG_BINARY")
            {
                return "hex:" + string.Join(",", SplitInParts(value, 2));
            }
            else if (valueType == "REG_NONE")
            {
                return "hex(0):";
            }
            else if (valueType == "REG_MULTI_SZ")
            {
                string? finalString = "";

                foreach (Match match in rg_splitComma.Matches(value).Cast<Match>())
                {
                    string? tmp = match.Value.TrimStart(',');

                    if (tmp.StartsWith("\""))
                    {
                        tmp = tmp[1..];
                    }

                    if (tmp.EndsWith("\""))
                    {
                        tmp = tmp.Remove(tmp.Length - 1);
                    }

                    finalString += tmp + "\0";
                }
                finalString += "\0";

                return "hex(7):" + string.Join(",", SplitInParts(ToHex(finalString), 2)); //fix for the quots
            }
            else if (OnlyHexInString(valueType))
            {
                return $"hex({valueType}):" + string.Join(",", SplitInParts(value, 2)); //??
            }

            return "$([INVALID_DATA])!!"; //??
        }

        private readonly Regex rg_checkHex = CheckRegex();
        private readonly Regex rg_splitComma = SplitCommaRegex();

        public bool OnlyHexInString(string test)
        {
            return rg_checkHex.IsMatch(test);
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            for (int i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }

        public static string ToHex(string s)
        {
            StringBuilder sb = new();
            foreach (char c in s)
            {
                List<string>? splittedParts = SplitInParts(string.Format("{0:X4}", (int)c), 2).ToList();
                splittedParts.Reverse();
                sb.AppendFormat(string.Join("", splittedParts));
            }
            return sb.ToString();
        }

        [GeneratedRegex("\\A\\b[0-9a-fA-F]+\\b\\Z", RegexOptions.Compiled)]
        private static partial Regex CheckRegex();

        [GeneratedRegex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled)]
        private static partial Regex SplitCommaRegex();
    }
}
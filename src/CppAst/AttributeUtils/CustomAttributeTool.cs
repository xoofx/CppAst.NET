using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CppAst
{
    public class MetaAttribute
    {
        public string FeatureName;
        public Dictionary<string, object> ArgumentMap = new Dictionary<string, object>();

        public override string ToString()
        {
			var builder = new StringBuilder();
            builder.Append($"{FeatureName} {{");
            foreach ( var kvp in ArgumentMap )
            {
                builder.Append( $"{kvp.Key}: {kvp.Value}, ");
            }
            builder.Append("}");
			return builder.ToString();
		}

        public bool QueryKeyIsTrue(string key)
        {
            return ArgumentMap.ContainsKey(key) && (((ArgumentMap[key] is bool) && (bool)ArgumentMap[key]) || ((ArgumentMap[key] is string && (string)ArgumentMap[key] == "true")));
        }
        
        public bool QueryKeysAreTrue(List<string> keys)
        {
            if (keys == null || !keys.Any())
            {
                return false;
            }
            
            foreach (string key in keys)
            {
                if (!QueryKeyIsTrue(key))
                {
                    return false;
                }
            }

            return true;
        }
	}

    public class MetaAttributeMap
    {
        public List<MetaAttribute> MetaList { get; private set; } = new List<MetaAttribute>();

        public bool IsNull
        {
            get
            {
                return MetaList.Count == 0;
            }
        }
        
        public object QueryArgument(string argName)
        {
            if (MetaList.Count == 0) return null;

            foreach (var argMap in MetaList)
            {
                if (argMap.ArgumentMap.ContainsKey(argName))
                {
                    return argMap.ArgumentMap[argName];
                }
            }
            
            return null;
        }

        public bool QueryArgumentAsBool(string argName, bool defaultVal)
        {
            var obj = QueryArgument(argName);
            if (obj != null)
            {
                try
                {
                    return Convert.ToBoolean(obj);
                }
                catch(Exception)
                {
                }
            }

            return defaultVal;
        }

        public int QueryArgumentAsInteger(string argName, int defaultVal)
        {
            var obj = QueryArgument(argName);
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch (Exception)
                {
                }
            }

            return defaultVal;
        }

        public string QueryArgumentAsString(string argName, string defaultVal)
        {
            var obj = QueryArgument(argName);
            if (obj != null)
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception)
                {
                }
            }

            return defaultVal;
        }
    }

    public static class CustomAttributeTool
    {
        public const string kMetaLeaderWord = "rmeta";
        public const string kMetaClassLeaderWord = "class";
        public const string kMetaFunctionLeaderWord = "function";
        public const string kMetaFieldLeaderWord = "field";
        public const string kMetaEnumLeaderWord = "enum";
        const string kMetaNotSetWord = "not_set_internal";
        const string kMetaSeparate = "____";
        const string kMetaArgumentSeparate = "|";
        const string kMetaStartWord = kMetaLeaderWord + kMetaSeparate;

        public static bool IsRstudioAttribute(string meta)
        {
            return meta.StartsWith(kMetaStartWord);
        }

        private static List<string> DivideForMetaAttribute(string meta)
        {
            var attrArray = meta.Split(kMetaSeparate);
            var retList = new List<string>();

            for(int i = 1; i < attrArray.Length; i++)
            {
                retList.Add(attrArray[i]);
            }

            return retList;
        }
        
        public static MetaAttribute ParseMetaStringFor(string meta, string needLeaderWord, out string errorMessage)
        {
            string feature = "", arguments = "";
            errorMessage = "";
        
            if (!IsRstudioAttribute(meta))
            {
                return null;
            }
        
            List<string> tmpList = DivideForMetaAttribute(meta);
            if(tmpList.Count < 2 || tmpList[0] != needLeaderWord)
            {
                return null;
            }
        
            var arrVal = tmpList[1].Split(kMetaArgumentSeparate);
            feature =  arrVal[0];
            if(arrVal.Length >= 2)
            {
                arguments = arrVal[1];
            }
        
            MetaAttribute attribute = new MetaAttribute();
            attribute.FeatureName = feature;
            bool parseSuc = NamedParameterParser.ParseNamedParameters(arguments, attribute.ArgumentMap, out errorMessage);
            if(parseSuc)
            {
                return attribute;
            }
            else
            {
                return null;
            }
        }

        public static MetaAttribute ParseMetaStringFor(string meta, out string errorMessage)
        {
            errorMessage = "";
            MetaAttribute attribute = new MetaAttribute();
            bool parseSuc = NamedParameterParser.ParseNamedParameters(meta, attribute.ArgumentMap, out errorMessage);
            if(parseSuc)
            {
                return attribute;
            }
            
            return null;
        }

    }
}

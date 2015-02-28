using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uni.Extensions
{
    public static class UniExtensions
    {
        public static T To<T>(this object obj, T defaultValue = default(T))
        {
            try
            {
                Type convertionType = typeof(T);
                Type objType = obj.GetType();
                if (obj == null || obj == DBNull.Value)
                    return defaultValue;
                if (objType == convertionType || convertionType.IsAssignableFrom(objType))
                    return (T)obj;
                if (convertionType.IsPrimitive)
                    obj = Convert.ChangeType(obj, convertionType);
                else if (convertionType == typeof(Guid))
                    obj = Guid.Parse(obj.ToString());
                else if (convertionType == typeof(ExpandoObject))
                    obj = obj.ToExpando();
                else if (convertionType.IsEnum)
                    obj = Enum.Parse(convertionType, obj.ToString());
                else if (convertionType == typeof(TimeSpan))
                    obj = Convert.ToDateTime(obj).TimeOfDay;
                else
                {
                    var objDict = obj.ToDictionary();
                    obj = Activator.CreateInstance<T>();
                    typeof(T).GetProperties().ToList().ForEach(pi =>
                    {
                        if (objDict.ContainsKey(pi.Name))
                            UniExtensions.PropertySetter(obj, pi.Name)(obj, objDict[pi.Name]);
                    }
                    );
                }
                return (T)Convert.ChangeType(obj, convertionType);
            }
            catch (Exception ex)
            {
            }
            return defaultValue;
        }
        public static dynamic ToExpando(this object obj)
        {
            if (obj.GetType() == typeof(ExpandoObject)) return obj;
            var result = new ExpandoObject();
            var dict = result as IDictionary<string, object>;
            int i = 0;
            if (obj.GetType() == typeof(NameValueCollection) || obj.GetType().IsSubclassOf(typeof(NameValueCollection)))
            {
                var nv = (NameValueCollection)obj;
                nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(f => dict.Add(f));
            }
            else if (typeof(IEnumerable<dynamic>).IsAssignableFrom(obj.GetType()))
                obj.ToEnumerable<dynamic>().ToList().ForEach(f => dict.Add("Item" + (i++).ToString(), f));
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(obj.GetType()))
                obj.ToDictionary().ToList().ForEach(f => dict.Add(f.Key, f.Value));
            else
                obj.GetType().GetProperties().ToList().ForEach(f => dict.Add(f.Name, f.GetValue(obj, null)));
            return result;
        }
        public static dynamic RecordToExpando(this IDataReader rdr)
        {
            dynamic retValue = new ExpandoObject();
            var dict = retValue as IDictionary<string, object>;
            for (int i = 0; i < rdr.FieldCount; i++)
                dict.Add(rdr.GetName(i), DBNull.Value.Equals(rdr[i]) ? null : rdr[i]);
            return retValue;
        }
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (typeof(Dictionary<string, object>).IsAssignableFrom(obj.GetType()))
                return (IDictionary<string, object>)obj;
            else
                return (IDictionary<string, object>)obj.ToExpando();
        }
        public static IEnumerable<T> ToEnumerable<T>(this object obj)
        {
            return obj as IEnumerable<T>;
        }
        public static T[] ToArray<T>(this object obj)
        {
            return ToEnumerable<T>(obj).ToArray();
        }
        public static bool IsNull<T>(this T obj)
        {
            Type objType = obj.GetType();
            if (objType == typeof(string))
                return string.IsNullOrEmpty(obj.ToString());
            else if (objType == typeof(Guid))
                return obj.To<Guid>() == Guid.Empty;
            else if (objType == typeof(DateTime))
            {
                var dateObj = obj.To<DateTime>();
                return dateObj == DateTime.MinValue || dateObj == DateTime.MaxValue;
            }
            return obj == null;
        }
        public static bool IsNotNull<T>(this T obj)
        {
            return !IsNull(obj);
        }
        public static string ToParameterString(this object obj, string parameterPrefix, string PKField = "", string parameterSuffix = "")
        {
            var retValue = string.Empty;
            dynamic dynamicObj = obj;
            var objType = obj.GetType();
            var sb = new StringBuilder();
            var isClass = !Enum.GetNames(typeof(TypeCode)).Contains(objType.Name);
            if (objType.IsArray)
                for (int x = 0; x < dynamicObj.Length; x++)
                    sb.AppendFormat("{0}{1}{2}{3}", parameterPrefix, x, parameterSuffix, x < dynamicObj.Length - 1 ? "," : "");
            else if (string.IsNullOrEmpty(objType.Namespace) || isClass)
            {
                var dict = obj.ToDictionary();
                if (!string.IsNullOrEmpty(PKField))
                    dict = obj.ToDictionary().Where(f => f.Key != PKField).ToDictionary(f => f.Key, f => f.Value);
                for (int x = 0; x < dict.Count; x++)
                    if (dict.ElementAt(x).Value != null && dict.ElementAt(x).Value.GetType().IsArray)
                        for (int y = 0; y < ((object[])dict.ElementAt(x).Value).Length; y++)
                            sb.AppendFormat("{0}{1}{2}{3}", parameterPrefix, dict.ElementAt(x).Key + y.ToString(), parameterSuffix, x < dict.Count - 1 ? "," : "");
                    else
                        sb.AppendFormat("{0}{1}{2}{3}", parameterPrefix, dict.ElementAt(x).Key, parameterSuffix, x < dict.Count - 1 ? "," : "");
            }
            else
                sb.AppendFormat("{0}{1}", parameterPrefix, "0");
            retValue = sb.ToString();
            return retValue;
        }
        public static string ToColumns(this object obj, string PKField = "")
        {
            return string.Format("{0}", string.Join(",", obj.ToDictionary().Keys.Where(f => f != PKField)));
        }
        public static string ToColumnParameterString(this object obj, string parameterPrefix, string PKField = "", string parameterSuffix = "", string seperator = ",")
        {
            var columnParameterList = new List<string>();
            if (obj.GetType().IsArray)
                for (int i = 0; i < obj.ToArray<dynamic>().Length; i++)
                    columnParameterList.Add(string.Format("{0}={1}{2}", PKField, parameterPrefix, i));
            else if (obj.ToDictionary().Count > 0)
                foreach (var item in obj.ToDictionary().Keys.Where(f => f != PKField))
                    columnParameterList.Add(string.Format("{0}={1}{0}{2}", item, parameterPrefix, parameterSuffix));
            else
                columnParameterList.Add(string.Format("{0}={1}0", PKField, parameterPrefix));
            return string.Format("{0}", string.Join(seperator, columnParameterList));
        }
        public static Dictionary<string, object> ToParameters(this object obj, string parameterPrefix, string PKField = "", string parameterSuffix = "")
        {
            var retValue = new Dictionary<string, object>();
            if (obj == null) return retValue;
            dynamic dynamicObj = obj;
            var objType = obj.GetType();
            var isClass = !Enum.GetNames(typeof(TypeCode)).Contains(objType.Name);
            if (objType.IsArray)
                for (int x = 0; x < dynamicObj.Length; x++)
                    retValue.Add(string.Format("{0}{1}{2}", parameterPrefix, x, parameterSuffix), (dynamicObj[x].GetType() == typeof(ExpandoObject)) ? ((object)dynamicObj[x]).ToDictionary()[PKField] : dynamicObj[x]);
            else if (objType == typeof(Dictionary<string, object>) || string.IsNullOrEmpty(objType.Namespace) || isClass)
            {
                if (string.IsNullOrEmpty(objType.Namespace) || (isClass && !objType.IsGenericType))
                    dynamicObj = obj.ToExpando();
                foreach (var item in dynamicObj)
                    if (item.Value != null && item.Value.GetType().IsArray)
                    {
                        dynamic argValueArray = item.Value;
                        var sb = new StringBuilder();
                        for (int y = 0; y < argValueArray.Length; y++)
                            retValue.Add(string.Format("{0}{1}{2}", parameterPrefix, item.Key + y.ToString(), parameterSuffix), argValueArray[y]);
                    }
                    else
                        retValue.Add(string.Format("{0}{1}{2}", parameterPrefix, item.Key, parameterSuffix), item.Value == null ? DBNull.Value : item.Value);
            }
            else
                retValue.Add(string.Format("{0}{1}", parameterPrefix, "0"), obj);
            return retValue;
        }
        public static IEnumerable<dynamic> ToCallBack(this DbCommand com)
        {
            foreach (var outputParameter in com.Parameters.Cast<DbParameter>().Where(f => f.Direction == ParameterDirection.Output))
            {
                dynamic ret = new ExpandoObject();
                ((IDictionary<string, object>)ret).Add(outputParameter.ParameterName, outputParameter.Value);
                yield return ret;
            }
        }
        public static Action<object, object> PropertySetter(object obj, string propertyName)
        {
            var type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            MethodInfo SetterMethodInfo = pi.GetSetMethod();
            ParameterExpression param = Expression.Parameter(typeof(object), "param");
            Expression convertedParam = Expression.Convert(param, type);
            ParameterExpression paramNewValue = Expression.Parameter(typeof(object), "newValue");
            Expression convertedParamNewValue = Expression.Convert(paramNewValue, pi.PropertyType);
            MethodCallExpression MethodCallSetterOfProperty = Expression.Call(convertedParam, SetterMethodInfo, convertedParamNewValue);
            Expression SetPropertyValueExp = Expression.Lambda(MethodCallSetterOfProperty, param, paramNewValue);
            Expression<Action<object, object>> SetPropertyValueLambda = (Expression<Action<object, object>>)SetPropertyValueExp;
            return SetPropertyValueLambda.Compile();
        }
        public static T ToAttribute<T>(this object obj)
        {
            object retValue = null;
            if (obj is Type)
                retValue = obj.To<Type>().GetCustomAttributes(true).Where(k => k.GetType().Name == typeof(T).Name).SingleOrDefault();
            else if (obj is MethodInfo)
                retValue = obj.To<MethodInfo>().GetCustomAttributes(true).Where(k => k.GetType().Name == typeof(T).Name).SingleOrDefault();
            return retValue == null ? Activator.CreateInstance<T>() : (T)retValue;
        }
        public static Nullable<DbType> GetDbType(this string type)
        {
            switch (type.ToLower())
            {
                //case "string": return DbType.AnsiString;
                //case "string": return DbType.AnsiStringFixedLength;
                //case "decimal": return DbType.Currency;
                //case "DateTime": return DbType.Date;
                //case "string": return DbType.StringFixedLength;
                //case "decimal": return DbType.VarNumeric;
                case "varchar": return DbType.String;
                case "byte[]": return DbType.Binary;
                case "bool": return DbType.Boolean;
                case "bit": return DbType.Boolean;
                case "byte": return DbType.Byte;
                case "DateTime": return DbType.DateTime;
                case "decimal": return DbType.Decimal;
                case "number": return DbType.Decimal;
                case "double": return DbType.Double;
                case "Guid": return DbType.Guid;
                case "short": return DbType.Int16;
                case "int": return DbType.Int32;
                case "long": return DbType.Int64;
                case "object": return DbType.Object;
                case "sbyte": return DbType.SByte;
                case "float": return DbType.Single;
                case "string": return DbType.String;
                case "TimeSpan": return DbType.Time;
                case "ushort": return DbType.UInt16;
                case "uint": return DbType.UInt32;
                case "ulong": return DbType.UInt64;
                default: return null;
            }
        }
        public static DataTable ToDataTable(this object obj)
        {
            var objType = obj.GetType();
            if (objType == typeof(DataTable)) return (DataTable)obj;

            var dt = new DataTable();
            IEnumerable<dynamic> objAsEnumerable = null;
            IDictionary<string, object> objDict = null;
            if (objType.Namespace != null && objType != typeof(ExpandoObject) && typeof(Dictionary<string, object>).IsAssignableFrom(objType) == false)
            {
                objAsEnumerable = obj.ToEnumerable<dynamic>();
                objDict = (objAsEnumerable.ElementAt(0) as object).ToDictionary();
            }
            else
                objDict = obj.ToDictionary();

            dt.Columns.AddRange(objDict.Select(f => new DataColumn(f.Key)).ToArray());
            var newRow = dt.NewRow();

            if (objAsEnumerable == null)
            {
                objDict.ToList().ForEach(f => newRow[f.Key] = f.Value);
                dt.Rows.Add(newRow);
            }
            else
                objAsEnumerable.ToList().ForEach(f =>
                {
                    newRow = dt.NewRow();
                    (f as object).ToDictionary().ToList().ForEach(ff =>
                    {
                        if (!dt.Columns.Contains(ff.Key))
                            dt.Columns.Add(new DataColumn(ff.Key));
                        newRow[ff.Key] = ff.Value;
                    }
                    );
                    dt.Rows.Add(newRow);
                });
            dt.AcceptChanges();

            return dt;
        }
        #region CSV
        public static string WriteCsv<T>(this IEnumerable<T> list, Encoding encoding, string seperator = ",", bool writeHeadersToCSV = false)
        {
            string content = string.Empty;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default))
                {
                    WriteToStream<T>(list, writer, seperator, writeHeadersToCSV);
                    writer.Flush();
                    memoryStream.Position = 0;
                    using (StreamReader reader = new StreamReader(memoryStream, encoding ?? Encoding.Default))
                    {
                        content = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        memoryStream.Close();
                    }
                }
            }
            return content;
        }
        private static void WriteToStream<T>(IEnumerable<T> list, TextWriter writer, string seperator = ",", bool writeHeadersToCSV = false)
        {
            List<string> fields = typeof(T).GetProperties().Select(f => f.Name).ToList();
            if (fields.Count == 0)
                fields = Activator.CreateInstance<T>().ToDictionary().Keys.ToList();
            if (writeHeadersToCSV && fields.Count > 0)
                WriteRecord(fields, writer, seperator);
            for (int i = 0; i < list.Count(); i++)
            {
                T item = list.ElementAt(i);
                var itemDict = item.ToDictionary();
                if (i == 0 && writeHeadersToCSV && fields.Count == 0)
                {
                    fields = itemDict.Keys.ToList();
                    WriteRecord(fields, writer, seperator);
                }
                fields.Clear();
                fields.AddRange(itemDict.Values.Select(o => o == null ? "" : o.ToString()));
                WriteRecord(fields, writer, seperator);
            }
        }
        public static string WriteCsv(this DataTable dataTable, Encoding encoding, string seperator = ",", bool writeHeadersToCSV = false)
        {
            string content = string.Empty;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default))
                {
                    WriteToStream(dataTable, writer, seperator, writeHeadersToCSV);
                    writer.Flush();
                    memoryStream.Position = 0;
                    using (StreamReader reader = new StreamReader(memoryStream, encoding ?? Encoding.Default))
                    {
                        content = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        memoryStream.Close();
                    }
                }
            }
            return content;
        }
        private static void WriteToStream(DataTable dataTable, TextWriter writer, string seperator = ",", bool writeHeadersToCSV = false)
        {
            List<string> fields = (from DataColumn column in dataTable.Columns select column.ColumnName).ToList();
            if (writeHeadersToCSV)
                WriteRecord(fields, writer, seperator);
            foreach (DataRow row in dataTable.Rows)
            {
                fields.Clear();
                fields.AddRange(row.ItemArray.Select(o => o.ToString()));
                WriteRecord(fields, writer, seperator);
            }
        }
        private static void WriteRecord(IList<string> fields, TextWriter writer, string seperator = ",")
        {
            for (int i = 0; i < fields.Count; i++)
            {
                bool quotesRequired = fields[i].Contains(",");
                bool escapeQuotes = fields[i].Contains("\"");
                string fieldValue = (escapeQuotes ? fields[i].Replace("\"", "\"\"") : fields[i]);
                var ReplaceCarriageReturnsAndLineFeedsFromFieldValues = true;
                var CarriageReturnAndLineFeedReplacement = ",";
                var Seperator = seperator;
                if (ReplaceCarriageReturnsAndLineFeedsFromFieldValues && (fieldValue.Contains("\r") || fieldValue.Contains("\n")))
                {
                    quotesRequired = true;
                    fieldValue = fieldValue.Replace("\r\n", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\r", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\n", CarriageReturnAndLineFeedReplacement);
                }
                writer.Write(string.Format("{0}{1}{0}{2}", (quotesRequired || escapeQuotes ? "\"" : string.Empty), fieldValue, (i < (fields.Count - 1) ? Seperator : string.Empty)));
            }
            writer.WriteLine();
        } 
        #endregion
        #region Async Invoke
        public static Task<TResult> AsyncInvoke<TServiceContract, TResult>(this TServiceContract obj, Func<TServiceContract, TResult> operation)
        {
            Task<TResult> retValue;

            retValue = Task.Factory.StartNew(f =>
            {
                Func<TServiceContract, TResult> taskOperation = (Func<TServiceContract, TResult>)f;

                TResult result = taskOperation(obj);

                return result;
            }, operation);

            return retValue;
        }
        public static Task AsyncInvoke<TServiceContract>(this TServiceContract obj, Action<TServiceContract> operation)
        {
            Task retValue;

            retValue = Task.Factory.StartNew(f =>
            {
                Action<TServiceContract> taskOperation = (Action<TServiceContract>)f;

                taskOperation(obj);
            }, operation);

            return retValue;
        } 
        #endregion
    }
}
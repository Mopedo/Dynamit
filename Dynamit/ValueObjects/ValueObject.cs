﻿using System;
using System.Dynamic;
using Dynamit.ValueObjects.Bool;
using Dynamit.ValueObjects.Byte;
using Dynamit.ValueObjects.DateTime;
using Dynamit.ValueObjects.Decimal;
using Dynamit.ValueObjects.Double;
using Dynamit.ValueObjects.Int;
using Dynamit.ValueObjects.Long;
using Dynamit.ValueObjects.Sbyte;
using Dynamit.ValueObjects.Short;
using Dynamit.ValueObjects.Single;
using Dynamit.ValueObjects.String;
using Dynamit.ValueObjects.Uint;
using Dynamit.ValueObjects.Ulong;
using Dynamit.ValueObjects.Ushort;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit.ValueObjects
{
    [Database]
    public abstract class ValueObject
    {
        internal static (ulong? objectNo, int? hash, TypeCode typeCode) Make(object value)
        {
            var hash = value?.GetHashCode();
            switch (value)
            {
                case string @string: return (new String1(@string).GetObjectNo(), hash, TypeCode.String);
                case bool @bool: return (new Bool1 {content = @bool}.GetObjectNo(), hash, TypeCode.Boolean);
                case byte @byte: return (new Byte1 {content = @byte}.GetObjectNo(), hash, TypeCode.Object);
                case System.DateTime datetime: return (new DateTime1 {content = datetime}.GetObjectNo(), hash, TypeCode.DateTime);
                case decimal @decimal: return (new Decimal1 {content = decimal.Round(@decimal, 6)}.GetObjectNo(), hash, TypeCode.Object);
                case double @double: return (new Double1 {content = @double}.GetObjectNo(), hash, TypeCode.Object);
                case int @int: return (new Int1 {content = @int}.GetObjectNo(), hash, TypeCode.Object);
                case long @long: return (new Long1 {content = @long}.GetObjectNo(), hash, TypeCode.Object);
                case sbyte @sbyte: return (new Sbyte1 {content = @sbyte}.GetObjectNo(), hash, TypeCode.Object);
                case short @short: return (new Short1 {content = @short}.GetObjectNo(), hash, TypeCode.Object);
                case float @float: return (new Single1 {content = @float}.GetObjectNo(), hash, TypeCode.Object);
                case uint @uint: return (new Uint1 {content = @uint}.GetObjectNo(), hash, TypeCode.Object);
                case ulong @ulong: return (new Ulong1 {content = @ulong}.GetObjectNo(), hash, TypeCode.Object);
                case ushort @ushort: return (new Ushort1 {content = @ushort}.GetObjectNo(), hash, TypeCode.Object);
                case IDynamicMetaObjectProvider dyn: return Make(GetStaticType(dyn));
                case null: return (null, null, 0);
                default: throw new InvalidValueTypeException(value.GetType());
            }
        }

        internal abstract long ByteCount { get; }

        internal static object GetStaticType(dynamic value)
        {
            object o;
            try
            {
                System.DateTime d = value;
                o = d;
            }
            catch
            {
                try
                {
                    string s = value;
                    o = s;
                }
                catch
                {
                    try
                    {
                        bool b = value;
                        o = b;
                    }
                    catch
                    {
                        try
                        {
                            int i = value;
                            o = i;
                        }
                        catch
                        {
                            try
                            {
                                long l = value;
                                o = l;
                            }
                            catch
                            {
                                try
                                {
                                    double d = value;
                                    o = d;
                                }
                                catch
                                {
                                    Type type = value.GetType();
                                    if (type.FullName == "Newtonsoft.Json.Linq.JObject")
                                        throw new InvalidValueTypeException(type,
                                            "Dynamic tables cannot contain inner objects");
                                    throw new InvalidValueTypeException(type);
                                }
                            }
                        }
                    }
                }
            }
            return o;
        }
    }
}
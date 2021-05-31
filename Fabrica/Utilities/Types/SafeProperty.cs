﻿/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.CSharp.RuntimeBinder;

namespace Fabrica.Utilities.Types
{


    public class SafeProperty : DynamicObject, IEquatable<SafeProperty>, IConvertible
    {

        private readonly object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeProperty"/> class.
        /// </summary>
        /// <param name="value">The value to store in the instance</param>
        public SafeProperty(object value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        /// <remarks><see langword="null"/> is considered as not being a value.</remarks>
        public bool HasValue => (_value != null);

        /// <summary>
        /// Gets the inner value
        /// </summary>
        public object Value => _value;

        /// <summary>
        /// Returns a default value if Value is null
        /// </summary>
        /// <typeparam name="T">When no default value is supplied, required to supply the default type</typeparam>
        /// <param name="defaultValue">Optional parameter for default value, if not given it returns default of type T</param>
        /// <returns>If value is not null, value is returned, else default value is returned</returns>
        public T Default<T>(T defaultValue = default(T))
        {
            if (HasValue)
            {
                try
                {
                    return (T)_value;
                }
                catch
                {
                    var typeName = _value.GetType().Name;
                    var message = $"Cannot convert value of type '{typeName}' to type '{typeof (T).Name}'";

                    throw new InvalidCastException(message);
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Attempts to convert the value to type of T, failing to do so will return the defaultValue.
        /// </summary>
        /// <typeparam name="T">When no default value is supplied, required to supply the default type</typeparam>
        /// <param name="defaultValue">Optional parameter for default value, if not given it returns default of type T</param>
        /// <returns>If value is not null, value is returned, else default value is returned</returns>
        public T TryParse<T>(T defaultValue = default (T))
        {
            if (HasValue)
            {
                try
                {
                    if (_value.GetType().IsAssignableFrom(typeof(T)))
                    {
                        return (T)_value;
                    }

                    var type = typeof(T);

                    var stringValue = _value as string;
                    if (type == typeof(DateTime))
                    {
                        if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                        {
                            return (T)((object)result);
                        }
                    }
                    else if (stringValue != null)
                    {
                        var converter = TypeDescriptor.GetConverter(type);

                        if (converter.IsValid(stringValue))
                        {
                            return (T)converter.ConvertFromInvariantString(stringValue);
                        }
                    }
                    else if (type == typeof(string))
                    {
                        return (T)Convert.ChangeType(_value, TypeCode.String, CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public static bool operator ==([CanBeNull] SafeProperty dynamicValue, object compareValue)
        {

            if( dynamicValue == null )
                return false;
            
            if (dynamicValue._value == null && compareValue == null)
            {
                return true;
            }

            return dynamicValue._value != null && dynamicValue._value.Equals(compareValue);
        }

        public static bool operator !=([CanBeNull] SafeProperty dynamicValue, object compareValue)
        {
            return !(dynamicValue == compareValue);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="compareValue"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="compareValue">An <see cref="SafeProperty"/> to compare with this instance.</param>
        public bool Equals([CanBeNull] SafeProperty compareValue)
        {
            if (ReferenceEquals(null, compareValue))
            {
                return false;
            }

            return ReferenceEquals(this, compareValue) || Equals(compareValue._value, _value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
        /// </summary>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="SafeProperty"/>; otherwise, <c>false</c>.</returns>
        /// <param name="compareValue">The <see cref="object"/> to compare with the current <see cref="SafeProperty"/>.</param>
        public override bool Equals([CanBeNull] object compareValue)
        {
            if (ReferenceEquals(null, compareValue))
            {
                return false;
            }

            if (ReferenceEquals(this, compareValue))
            {
                return true;
            }

            return compareValue.GetType() == typeof(SafeProperty) && Equals((SafeProperty)compareValue);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Provides implementation for binary operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as addition and multiplication.
        /// </summary>
        /// <returns><c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns <c>false</c>, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
        /// <param name="binder">Provides information about the binary operation. The binder.Operation property returns an <see cref="T:System.Linq.Expressions.ExpressionType"/> object. For example, for the sum = first + second statement, where first and second are derived from the DynamicObject class, binder.Operation returns ExpressionType.Add.</param><param name="arg">The right operand for the binary operation. For example, for the sum = first + second statement, where first and second are derived from the DynamicObject class, <paramref name="arg"/> is equal to second.</param><param name="result">The result of the binary operation.</param>
        public override bool TryBinaryOperation([NotNull] BinaryOperationBinder binder, object arg, out object result)
        {
            result = null;

            if (binder.Operation != ExpressionType.Equal)
                return false;

            var convert = Binder.Convert(CSharpBinderFlags.None, arg.GetType(), typeof(SafeProperty));

            if (!TryConvert((ConvertBinder)convert, out object resultOfCast))
                return false;

            result = resultOfCast?.Equals(arg) ?? Equals(arg, resultOfCast);

            return true;
        }

        /// <summary>
        /// Provides implementation for type conversion operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that convert an object from one type to another.
        /// </summary>
        /// <returns><c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns <c>false</c>, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
        /// <param name="binder">Provides information about the conversion operation. The binder.Type property provides the type to which the object must be converted. For example, for the statement (String)sampleObject in C# (CType(sampleObject, Type) in Visual Basic), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Type returns the <see cref="T:System.String"/> type. The binder.Explicit property provides information about the kind of conversion that occurs. It returns true for explicit conversion and false for implicit conversion.</param><param name="result">The result of the type conversion operation.</param>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;

            if (_value == null)
            {
                return true;
            }

            var binderType = binder.Type;
            if (binderType == typeof(String))
            {
                result = Convert.ToString(_value);
                return true;
            }

            if (binderType == typeof(Guid) || binderType == typeof(Guid?))
            {
                if (Guid.TryParse(Convert.ToString(_value), out var guid))
                {
                    result = guid;
                    return true;
                }
            }
            else if (binderType == typeof(TimeSpan) || binderType == typeof(TimeSpan?))
            {
                if (TimeSpan.TryParse(Convert.ToString(_value), out var timespan))
                {
                    result = timespan;
                    return true;
                }
            }
            else if (binderType.IsEnum)
            {
                // handles enum to enum assignments
                if (_value.GetType().IsEnum)
                {
                    if (binderType == _value.GetType())
                    {
                        result = _value;
                        return true;
                    }

                    return false;
                }

                // handles number to enum assignments
                if (Enum.GetUnderlyingType(binderType) == _value.GetType())
                {
                    result = Enum.ToObject(binderType, _value);
                    return true;
                }

                return false;
            }
            else
            {
                if (binderType.IsGenericType && binderType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    binderType = binderType.GetGenericArguments()[0];
                }

                var typeCode = Type.GetTypeCode(binderType);

                if (typeCode == TypeCode.Object)
                {

                    if( !binderType.IsInstanceOfType( _value ) ) 
                        return false;

                    result = _value;
                    return true;

                }

                result = Convert.ChangeType(_value, typeCode);

                return true;
            }
            return base.TryConvert(binder, out result);
        }

        public override string ToString()
        {
            return _value == null ? base.ToString() : Convert.ToString(_value);
        }

        public static implicit operator bool([NotNull] SafeProperty dynamicValue)
        {
            if (!dynamicValue.HasValue)
            {
                return false;
            }

            if (dynamicValue._value.GetType().IsValueType)
            {
                return (Convert.ToBoolean(dynamicValue._value));
            }

            if (bool.TryParse(dynamicValue.ToString( CultureInfo.InvariantCulture ), out var result))
            {
                return result;
            }

            return true;
        }

        [CanBeNull]
        public static implicit operator string(SafeProperty dynamicValue)
        {
            return dynamicValue.HasValue
                       ? Convert.ToString(dynamicValue._value)
                       : null;
        }

        public static implicit operator int([NotNull] SafeProperty dynamicValue)
        {

            if( (dynamicValue.Equals( null )) || (dynamicValue._value == null) )
                return 0;
            
            if( dynamicValue._value.GetType().IsValueType )
                return Convert.ToInt32(dynamicValue._value);

            return int.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator Guid([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return default(Guid);
            
            if (dynamicValue._value is Guid)
                return (Guid)dynamicValue._value;

            return Guid.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator DateTime([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return default(DateTime);
            
            
            if (dynamicValue._value is DateTime)
                return (DateTime)dynamicValue._value;

            return DateTime.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator TimeSpan([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return default(TimeSpan);
          
            
            if (dynamicValue._value is TimeSpan)
                return (TimeSpan)dynamicValue._value;

            return TimeSpan.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator long([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return 0;
            
            if (dynamicValue._value.GetType().IsValueType)
                return Convert.ToInt64(dynamicValue._value);

            return long.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator float([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return 0;
            
            if (dynamicValue._value.GetType().IsValueType)
                return Convert.ToSingle(dynamicValue._value);

            return float.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator decimal([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return 0;
            
            
            if (dynamicValue._value.GetType().IsValueType)
                return Convert.ToDecimal(dynamicValue._value);

            return decimal.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }

        public static implicit operator double([NotNull] SafeProperty dynamicValue)
        {

            if ((dynamicValue.Equals(null)) || (dynamicValue._value == null))
                return 0;
            
            if (dynamicValue._value.GetType().IsValueType)
                return Convert.ToDouble(dynamicValue._value);

            return double.Parse(dynamicValue.ToString( CultureInfo.InvariantCulture ));

        }


        #region Implementation of IConvertible

        /// <summary>
        /// Returns the <see cref="T:System.TypeCode"/> for this instance.
        /// </summary>
        /// <returns>
        /// The enumerated constant that is the <see cref="T:System.TypeCode"/> of the class or value type that implements this interface.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public TypeCode GetTypeCode()
        {
            if (_value == null) return TypeCode.Empty;
            return Type.GetTypeCode(_value.GetType());
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A Boolean value equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
       /// A Unicode character equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 8-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 16-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 32-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit signed integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An 64-bit unsigned integer equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A single-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A double-precision floating-point number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal"/> number using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Decimal"/> number equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.DateTime"/> using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DateTime"/> instance equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String"/> using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> instance equivalent to the value of this instance.
        /// </returns>
        /// <param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(_value, provider);
        }

        /// <summary>
        /// Converts the value of this instance to an <see cref="T:System.Object"/> of the specified <see cref="T:System.Type"/> that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> instance of type <paramref name="conversionType"/> whose value is equivalent to the value of this instance.
        /// </returns>
        /// <param name="conversionType">The <see cref="T:System.Type"/> to which the value of this instance is converted. </param><param name="provider">An <see cref="T:System.IFormatProvider"/> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(_value, conversionType, provider);
        }

        #endregion
    }







}

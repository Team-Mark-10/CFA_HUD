using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CFA_HUD
{

    /// <summary>
    /// The implementing class will be able to serialise its data into a string JSON format by hand.
    /// </summary>
    public interface IJSONSerializable
    {
        public string ToJSONFormat();
    }

    /// <summary>
    /// The implementing class encapsulates a data value which can be displayed on the UI and JSONSerialized
    /// </summary>
    public interface IArbitraryData : IJSONSerializable
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns></returns>
        public string GetName();

        /// <summary>
        /// Is this a default field or a one custom set by the user.
        /// </summary>
        /// <returns></returns>
        public bool IsUserSet();

        /// <summary>
        /// Attempts to set the value of the arbitrary data. Applies validation.
        /// </summary>
        /// <param name="value">The string to try parse</param>
        /// <returns>Whether the string was successfully parsed or not</returns>
        public bool TrySetValue(string value);

        /// <summary>
        /// Returns that data is a UI friendly format.
        /// </summary>
        /// <returns></returns>
        public string ToDisplayFormat();
    }


    /// <summary>
    /// An class that represents a arbitrary data value attached to a patient.
    /// </summary>
    /// <typeparam name="T">What type the of the Value this instance encapsulates.</typeparam>
    public abstract class ArbitraryData<T> : IArbitraryData
    {

        /// <summary>
        /// The name of the arbitrary data field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the arbitrary data field
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// If this is a default field or a one custom set by the user.
        /// </summary>
        /// <returns></returns>
        protected bool isUserSet = false;

        /// <summary>
        /// Constructs a new ArbitraryData Object. 
        /// 
        /// <example>
        /// For example:
        /// <code>
        /// var unitData = new ArbitraryData<string>("Unit", "ADFA", ArbitraryDataSerialisers.StringSerialiser);
        /// </code>
        /// </example>
        /// 
        /// </summary>
        /// <param name="name">The name of the arbitrary data field</param>
        /// <param name="value">The intial value of the arbitrary data field</param>
        /// <param name="_serialiser"> The function that will convert this ArbitraryData object into a JSON string. See <see cref="ArbitraryDataSerializers"/> to get the default serialisers.</param>
        public ArbitraryData(string name, T value, bool isUserSet = false)
        {
            Name = name;
            Value = value;
            this.isUserSet = isUserSet;
        }

        /// <summary>
        /// Returns the ArbitraryData object in JSON format using the serialiser.
        /// </summary>
        /// <returns>A JSON String</returns>
        public abstract string ToJSONFormat();

        /// <summary>
        /// Returns the data formatted as a string for display on UI
        /// </summary>
        /// <returns>Formatted string</returns>
        public abstract string ToDisplayFormat();

        /// <summary>
        /// Tries to set the underlying value from a string
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <returns>If setting the value was successful or not</returns>
        public abstract bool TrySetValue(string value);

        public string GetName()
        {
            return Name;
        }

        public bool IsUserSet()
        {
            return isUserSet;
        }
    }


    public class ArbitraryIntValue : ArbitraryData<int>
    {
        public ArbitraryIntValue(string name, int value) : base(name, value) { }

        public override string ToDisplayFormat()
        {
            return $"{Value}";
        }

        public override string ToJSONFormat()
        {
            return $"{{ \"name\": \"{Name}\", \"value\": {Value} }}";
        }

        public override bool TrySetValue(string value)
        {
            var success = int.TryParse(value, out int newValue);
            Value = newValue;

            isUserSet = true;

            return success;
        }
    }

    public class ArbitraryFloatValue : ArbitraryData<float>
    {
        public ArbitraryFloatValue(string name, float value) : base(name, value) { }

        public override string ToDisplayFormat()
        {
            return $"{Value:F2}";
        }

        public override string ToJSONFormat()
        {
            return $"{{ \"name\": \"{Name}\", \"value\": {Value} }}";
        }

        public override bool TrySetValue(string value)
        {
            var success = float.TryParse(value, out float newValue);
            Value = newValue;

            isUserSet = true;


            return success;
        }
    }

    public class ArbitraryBoolValue : ArbitraryData<bool>
    {
        public const string TRUE = "TRUE";
        public const string FALSE = "FALSE";


        public ArbitraryBoolValue(string name, bool value) : base(name, value) { }

        public override string ToDisplayFormat()
        {
            return $"{(Value ? "TRUE" : "FALSE")}";
        }

        public override string ToJSONFormat()
        {
            return $"{{ \"name\": \"{Name}\", \"value\": {(Value ? "true" : "false")} }}";
        }

        public override bool TrySetValue(string value)
        {
            switch (value)
            {
                case TRUE:
                    Value = true;
                    return true;
                case FALSE:
                    Value = false;
                    return true;
                default:
                    return false;
            }
        }
    }

    public class ArbitraryStringValue : ArbitraryData<string>
    {
        public ArbitraryStringValue(string name, string value) : base(name, value) { }


        public override string ToDisplayFormat()
        {
            return $"{Value}";
        }

        public override string ToJSONFormat()
        {
            return $"{{ \"name\": \"{Name}\", \"value\": \"{Value}\" }}";
        }

        public override bool TrySetValue(string value)
        {
            Value = value;

            isUserSet = true;

            return true;
        }
    }

    public class ArbitraryDateTimeValue : ArbitraryData<DateTime>
    {
        public ArbitraryDateTimeValue(string name, DateTime value) : base(name, value) { }

        public override string ToDisplayFormat()
        {
            return Value.ToShortDateString();
        }

        public override string ToJSONFormat()
        {
            return $"{{ \"name\": \"{Name}\", \"value\": \"{XmlConvert.ToString(Value, XmlDateTimeSerializationMode.Utc)}\" }}";
        }

        public override bool TrySetValue(string value)
        {
            var success = DateTime.TryParse(value, out DateTime newValue);
            Value = newValue;

            isUserSet = true;

            return success;
        }
    }
}

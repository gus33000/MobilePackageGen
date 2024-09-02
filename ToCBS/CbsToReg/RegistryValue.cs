namespace CbsToReg
{
    public struct RegistryValue
    {
        public RegistryValue(string name, string value, string valueType, string mutable, string operationHint)
        {
            Name = name;
            Value = value;
            ValueType = valueType;
            Mutable = mutable;
            OperationHint = operationHint;
        }
        public string Name
        {
            get; set;
        }
        public string Value
        {
            get; set;
        }
        public string ValueType
        {
            get; set;
        }
        public string Mutable
        {
            get; set;
        }
        public string OperationHint
        {
            get; set;
        }
    }
}
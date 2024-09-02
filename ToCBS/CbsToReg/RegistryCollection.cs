namespace CbsToReg
{
    public struct RegistryCollection
    {
        public RegistryCollection(string keyName, List<RegistryValue> registryValues)
        {
            KeyName = keyName;
            RegistryValues = registryValues;
        }
        public string KeyName
        {
            get; set;
        }
        public List<RegistryValue> RegistryValues
        {
            get; set;
        }
    }
}
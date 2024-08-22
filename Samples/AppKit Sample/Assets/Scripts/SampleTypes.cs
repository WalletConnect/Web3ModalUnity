using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace WalletConnect.Web3Modal.Sample
{
    [Struct("Mail")]
    public class Mail
    {
        [Parameter("tuple", "from", 1, "Person")]
        public Person From { get; set; }

        [Parameter("tuple[]", "to", 2, "Person[]")]
        public List<Person> To { get; set; }

        [Parameter("string", "contents", 3)] public string Contents { get; set; }
    }

    [Struct("Person")]
    public class Person
    {
        [Parameter("string", "name", 1)] public string Name { get; set; }

        [Parameter("address[]", "wallets", 2)] public List<string> Wallets { get; set; }
    }

    [Struct("Group")]
    public class Group
    {
        [Parameter("string", "name", 1)] public string Name { get; set; }

        [Parameter("tuple[]", "members", 2, "Person[]")]
        public List<Person> Members { get; set; }
    }
}
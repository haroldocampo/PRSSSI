using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.SpecialObjects {
    public class KeyAndValue {
        public object Key { get; set; }
        public string Value { get; set; }

        public KeyAndValue(object key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

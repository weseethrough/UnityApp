using System;
using Sqo.Attributes;

namespace RaceYourself.Models
{
    public class Parameter
    {
        [UniqueConstraint]
        public string key;
        public string value;

        public Parameter (){}
        public Parameter (string key, string value) {
            this.key = key;
            this.value = value;
        }
    }
}


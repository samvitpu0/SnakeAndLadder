using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    public class SkipFieldAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionInfo : DescriptionAttribute
    {
        public new string Description { get; set; }
        public string Value { get; set; }

        public DescriptionInfo(string description, string value)
        {
            Description = description;
            Value = value;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using dnYara.Interop;

namespace dnYara
{
    /// <summary>
    /// Data structure representing a single rule.
    /// </summary>
    public sealed class Rule
    {
        private Nullable<YR_RULE> _struct;

        /// <summary>
        /// Rule identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Pointer to a sequence of null terminated strings with tag names. An additional null character
        /// marks the end of the sequence. Example: tag1\0tag2\0tag3\0\0.
        /// To iterate over the tags you can use yr_rule_tags_foreach().
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Key-Value pairs associated with the rule. The value portion can be one of
        /// <list type="bullet">
        /// <term>string</term>
        /// <term>long (int64)</term>
        /// <term>boolean</term>
        /// <term>null</term>
        /// </list>
        /// </summary>
        public IDictionary<string, object>  Metas { get; private set; }

        public long TimeCost => _struct.HasValue ? _struct.Value.time_cost : 0;

        public Rule()
        {
            Identifier = string.Empty;
            Tags = new List<string>();
            Metas = new Dictionary<string, object>();
        }

        private static (string name, object value) ExtractMetaValue (YR_META meta) {
            var name = Marshal.PtrToStringAnsi(meta.identifier);
            object v = null;
            switch((META_TYPE)meta.type) {
                case META_TYPE.META_TYPE_NULL:
                    break;
                case META_TYPE.META_TYPE_INTEGER:
                    v = meta.integer;
                    break;
                case META_TYPE.META_TYPE_BOOLEAN:
                    v = meta.integer == 0 ? false : true;
                    break;
                case META_TYPE.META_TYPE_STRING:
                    v = Marshal.PtrToStringAnsi(meta.strings);
                    break;
            }
            return (name, v);
        }

        public Rule(YR_RULE rule)
        {
            _struct = rule;
            IntPtr ptr = _struct.Value.identifier;
            Identifier = Marshal.PtrToStringAnsi(ptr);
            Tags = new List<string>();
            ObjRefHelper.ForEachStringInObjRef(_struct.Value.tags, Tags.Add);
            Metas = ObjRefHelper.GetMetas(_struct.Value.metas).Select(ExtractMetaValue).ToDictionary();

        }
    }
}
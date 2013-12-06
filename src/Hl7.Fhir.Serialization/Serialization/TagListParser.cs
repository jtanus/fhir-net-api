﻿/*
  Copyright (c) 2011-2013, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Hl7.Fhir.Model;
using System.Xml.Linq;
using System.IO;
using Hl7.Fhir.Support;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hl7.Fhir.Serialization
{
    internal static class TagListParser
    {
        internal static IList<Tag> ParseTags(XmlReader xr)
        {
            xr.MoveToContent();

            var taglist = (XElement)XElement.ReadFrom(xr);

            if (taglist.Name == BundleXmlParser.XFHIRNS + TagListSerializer.TAGLIST_TYPE)
            {
                if (taglist.Elements().All(xe => xe.Name == BundleXmlParser.XFHIRNS + BundleXmlParser.XATOM_CATEGORY))
                    return ParseTags(taglist.Elements());
                else
                    throw Error.Format("TagList contains unexpected child elements");
            }
            else
                throw Error.Format("Unexpected element name {0} found at start of TagList", taglist.Name);
        }

        internal static IList<Tag> ParseTags(JsonReader xr)
        {
            var tagObj = JObject.Load(xr);

            var tagType = tagObj[SerializationConfig.RESOURCETYPE_MEMBER_NAME];
            if(tagType == null || tagType.Value<string>() != TagListSerializer.TAGLIST_TYPE)
                throw Error.Format("TagList should start with a resourceType member TagList");

            var categoryArray = tagObj[BundleXmlParser.XATOM_CATEGORY] as JArray;
            if (categoryArray != null)
                return ParseTags(categoryArray);
            else
                return new List<Tag>();
        }

        internal static IList<Tag> ParseTags(IEnumerable<XElement> tags)
        {
            var result = new List<Tag>();

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    var scheme = SerializationUtil.StringValueOrNull(tag.Attribute(BundleXmlParser.XATOM_CAT_SCHEME));
                    var term = SerializationUtil.StringValueOrNull(tag.Attribute(BundleXmlParser.XATOM_CAT_TERM));
                    var label = SerializationUtil.StringValueOrNull(tag.Attribute(BundleXmlParser.XATOM_CAT_LABEL));

                    result.Add(new Tag(term,scheme,label));
                }
            }

            return result;
        }


        internal static IList<Tag> ParseTags(JToken token)
        {
            var result = new List<Tag>();
            var tags = token as JArray;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    var scheme = tag.Value<string>(BundleXmlParser.XATOM_CAT_SCHEME);
                    var term = tag.Value<string>(BundleXmlParser.XATOM_CAT_TERM);
                    var label = tag.Value<string>(BundleXmlParser.XATOM_CAT_LABEL);
                    
                    result.Add(new Tag(term,scheme,label));
                }
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using MdExtract;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestTagPos()
        {
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "title";
            int i = YMLMeister.GetTagStartPos(content, tag);
            Assert.AreEqual(i, 5);
        }

        [TestMethod]
        public void TestMultilineValue()
        {
            YMLMeister YMLMeister= new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "helpviewer_keywords";
            Assert.IsTrue(YMLMeister.IsMultilineValue(content, tag));
        }

        [TestMethod]
        public void TestSinglelineValue()
        {
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.assetid";
            Assert.IsFalse(YMLMeister.IsMultilineValue(content, tag));
        }

        [TestMethod]
        public void TestPrefix()
        {
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.custom";
            string expected = "---\r\ntitle: \"Abstract Classes (C++) | Microsoft Docs\"\r\n";

            string result = YMLMeister.GetPrefix(content, tag);

            int x = String.Compare(expected, result);
            int xx = expected.Length;
            int z = result.Length;
            //Assert.AreEqual(x, 0);
            Assert.AreEqual(result.Length, expected.Length);
            System.Diagnostics.Debug.WriteLine(result + tag);
        }

        [TestMethod]
        public void TestTagEndPosSingleVal()
        {
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            int startPos = YMLMeister.GetTagStartPos(content, tag);
            int endPos = YMLMeister.GetTagValueEndPos(content, startPos);
            string s = content.Substring(0, endPos);
            Assert.AreEqual(endPos, 112);
        }

        [TestMethod]
        public void TestOutputforGetTagAndValue()
        {
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            string val = YMLMeister.GetTagAndValue(content, tag);
            Debug.Write(val);

            tag = "ms.author";
            val = YMLMeister.GetTagAndValue(content, tag);
            Debug.Write(val);
        }


        [TestMethod]
        public void TestGetValue()
        {
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            string temp = YMLMeister.GetValue(content, tag);
            Debug.WriteLine("-------TestGetValue----------");
            Debug.Write(temp);
            Debug.WriteLine("-------");
        }
        [TestMethod]
        public void TestTagEndPosMultiLine()
        {
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            int startPos = YMLMeister.GetTagStartPos(content, tag);
            //int lineEnd = content.IndexOf("\n", startPos);
            int endPos = YMLMeister.GetTagValueEndPos(content, startPos);
            string s = content.Substring(startPos, endPos - startPos);
            Assert.AreEqual(s.Length, 197);
        }

        [TestMethod]
        public void TestSuffix()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Suffix for ms.author-----");
            string result = YMLMeister.GetSuffix(content, tag);
            Debug.Write(result);
            Debug.WriteLine("-------------");
        }

        /// <summary>
        /// Verifies that GetPrefix GetTagAndValue and GetSuffix all
        /// work as expected without adding or removing chars from the original string.
        /// </summary>
        [TestMethod]
        public void TestPrefixTagSuffix()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Prefix tag suffix for ms.author-----");
            string prefix = YMLMeister.GetPrefix(content, tag);
            string tagAndval = YMLMeister.GetTagAndValue(content, tag);
            string suffix = YMLMeister.GetSuffix(content, tag);
            StringBuilder sb = new StringBuilder();
            sb.Append(prefix).Append(tagAndval).Append(suffix);
            Debug.Assert(sb.ToString().Length == content.Length);
            Debug.Write(prefix + tagAndval + suffix);
            Debug.WriteLine("-------------");
        }

#if false
        [TestMethod]
        public void TestDeleteTagAndValue()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Delete ms.author-----");
            var result = YMLMeister.DeleteTagAndValue(content, tag);
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
            Assert.AreEqual(result.Length, 206);
        }

        [TestMethod]
        public void TestReplaceSingleLine()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Delete ms.author-----");
            var result = YMLMeister.ReplaceSingleValue(content, tag, "Donald Jr");
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
        //    Assert.AreEqual(result.Length, 207);
        }
        [TestMethod]
        public void TestCreateTag()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.ImNew";
            string newVal = "\"new Value\"";
            Debug.WriteLine("-----Create ms.ImNew-----");
            var result = YMLMeister.CreateTag(content, tag, newVal);
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
            //    Assert.AreEqual(result.Length, 207);
        }

#endif
        [TestMethod]
        public void TestGetYmlBlock()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string yml = YMLMeister.GetYmlBlock(content);
            Debug.WriteLine("-----Get YML Block-----");
            int i = yml.Length;
            Debug.Write(yml);
            Debug.WriteLine("<---output ends here");
            //    Assert.AreEqual(result.Length, 207);
        }

        [TestMethod]
        public void TestGetAllTags()
        {
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string yml = YMLMeister.GetYmlBlock(content);
            Debug.WriteLine("-----Get All tags-----");
            var tags = YMLMeister.GetAllTags(yml);
            Debug.WriteLine("<---output ends here");
            foreach (string s in tags)
            {
                Debug.WriteLine(s);
            }
            //    Assert.AreEqual(result.Length, 207);
        }

        [TestMethod]
        public void TestGetAllTagsWithCPPBracketKeywords()
        {
            string f = @"../../catlmodule-class.md";
            string content = File.ReadAllText(f);
            string yml = YMLMeister.GetYmlBlock(content);
            Debug.WriteLine("-----Get All tags-----");
            var tags = YMLMeister.GetAllTags(yml);
            Debug.WriteLine("<---output ends here");
            foreach (string s in tags)
            {
                Debug.WriteLine(s);
            }
            //    Assert.AreEqual(result.Length, 207);
        }

        [TestMethod]
        public void TestMaketagFromString()
        {
            string f = @"../../catlmodule-class.md";
            string content = File.ReadAllText(f);
            string yml = YMLMeister.GetYmlBlock(content);
            Debug.WriteLine("-----Get All tags-----");
            var matches = YMLMeister.GetAllTags(yml);
            Debug.WriteLine("<---output ends here");
            var tags = new List<Tag>();
            foreach (string m in matches)
            {
                var t = new Tag(m);
                tags.Add(t);
                Debug.WriteLine("tag name is: " + t.TagName);
                foreach (var i in t.TagValues)
                {
                    Debug.WriteLine("   " + i);
                }
            }
            Debug.WriteLine("make tags from those same tuples:");
            foreach (var v in tags)
            {
                Debug.WriteLine(v.ToString());
            }
        }

        [TestMethod]
        public void TestParseYml2()
        {
            string f = @"../../abstract-classes3.md";
            var content = File.ReadAllText(f);
            var result = YMLMeister.ParseYML2(content, null);
            foreach (var v in result)
            {
                Debug.WriteLine(v.TagName);
                foreach (var v2 in v.TagValues)
                {
                    Debug.WriteLine("   " + v2);
                }
            }
        }

        [TestMethod]
        public void TestMdExtract()
        {
            string tagAndVal = @"f1_keywords: ['CAtlBaseModule [ATL]', 'ATLCORE/ATL::CAtlBaseModule', 'ATLCORE/ATL::CAtlBaseModule::CAtlBaseModule', 'ATLCORE/ATL::CAtlBaseModule::AddResourceInstance', 'ATLCORE/ATL::CAtlBaseModule::GetHInstanceAt', 'ATLCORE/ATL::CAtlBaseModule::GetModuleInstance', 'ATLCORE/ATL::CAtlBaseModule::GetResourceInstance', 'ATLCORE/ATL::CAtlBaseModule::RemoveResourceInstance', 'ATLCORE/ATL::CAtlBaseModule::SetResourceInstance', 'ATLCORE/ATL::CAtlBaseModule::m_bInitFailed']";
            MdExtract.Tag tag = new MdExtract.Tag(tagAndVal);
            Assert.AreEqual(tag.TagFormatString, "bracket");
        }

        [TestMethod]
        public void TestUnique()
        {
            string source = "C: \\Users\\mblome\\vcppdocs\\docs\\standard-library\\allocator-base-class.md\tIGNORE\tf1_keywords\t[\"allocators/stdext::allocator_base\", \"allocators/stdext::allocators::allocator_base\", \"allocators/stdext::allocator_base::const_pointer\", \"allocators/stdext::allocator_base::const_reference\", \"allocators/stdext::allocator_base::difference_type\", \"allocators/stdext::allocator_base::pointer\", \"allocators/stdext::allocator_base::reference\", \"allocators/stdext::allocator_base::size_type\", \"allocators/stdext::allocator_base::value_type\", \"allocators/stdext::allocator_base::_Charalloc\", \"allocators/stdext::allocator_base::_Chardealloc\", \"allocators/stdext::allocator_base::address\", \"allocators/stdext::allocator_base::allocate\", \"allocators/stdext::allocator_base::reference\",\"allocators/stdext::allocator_base::construct\", \"allocators/stdext::allocator_base::deallocate\", \"allocators/stdext::allocator_base::destroy\", \"allocators/stdext::allocator_base::reference\",\"allocators/stdext::allocator_base::max_size\"]\tbracket";
            var elements = source.Split('\t');
            var tagData = new Tag(elements[0], elements[3], elements[4], true);
            Assert.IsTrue(tagData.TagValues.Count < elements[2].Length);
        }
    }
}

;

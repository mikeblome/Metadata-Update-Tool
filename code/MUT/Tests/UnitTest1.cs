using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using MUT;

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
            YMLMeister y = new YMLMeister();
            string tag = "title";
            int i = y.GetTagStartPos(content, tag);
            Assert.AreEqual(i, 5);
        }

        [TestMethod]
        public void TestMultilineValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "dev_langs";
            Assert.IsTrue(y.IsMultilineValue(content, tag));
        }

        [TestMethod]
        public void TestSinglelineValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.assetid";
            Assert.IsFalse(y.IsMultilineValue(content, tag));
        }

        [TestMethod]
        public void TestPrefix()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.custom";
            string expected = "---\r\ntitle: \"Abstract Classes (C++) | Microsoft Docs\"\r\n";

            string result = y.GetPrefix(content, tag);

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
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            int startPos = y.GetTagStartPos(content, tag);
            int endPos = y.GetTagValueEndPos(content, startPos);
            string s = content.Substring(0, endPos);
            Assert.AreEqual(endPos, 112);
        }

        [TestMethod]
        public void TestOutputforGetTagAndValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            string val = y.GetTagAndValue(content, tag);
            Debug.Write(val);

            tag = "ms.author";
            val = y.GetTagAndValue(content, tag);
            Debug.Write(val);
        }


        [TestMethod]
        public void TestGetValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            string temp = y.GetValue(content, tag);
            Debug.WriteLine("-------TestGetValue----------");
            Debug.Write(temp);
            Debug.WriteLine("-------");
        }
        [TestMethod]
        public void TestTagEndPosMultiLine()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            int startPos = y.GetTagStartPos(content, tag);
            int endPos = y.GetTagValueEndPos(content, startPos);
            string s = content.Substring(startPos, endPos - startPos);
            Assert.AreEqual(endPos, 326);
        }

        [TestMethod]
        public void TestSuffix()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Suffix for ms.author-----");
            string result = y.GetSuffix(content, tag);
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
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Prefix tag suffix for ms.author-----");
            string prefix = y.GetPrefix(content, tag);
            string tagAndval = y.GetTagAndValue(content, tag);
            string suffix = y.GetSuffix(content, tag);
            StringBuilder sb = new StringBuilder();
            sb.Append(prefix).Append(tagAndval).Append(suffix);
            Debug.Assert(sb.ToString().Length == content.Length);
            Debug.Write(prefix + tagAndval + suffix);
            Debug.WriteLine("-------------");
        }

        [TestMethod]
        public void TestDeleteTagAndValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Delete ms.author-----");
            var result = y.DeleteTagAndValue(content, tag);
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
            Assert.AreEqual(result.Length, 206);
        }

        [TestMethod]
        public void TestReplaceSingleLine()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Delete ms.author-----");
            var result = y.ReplaceSingleValue(content, tag, "Donald Jr");
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
        //    Assert.AreEqual(result.Length, 207);
        }
        [TestMethod]
        public void TestCreateTag()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.ImNew";
            string newVal = "\"new Value\"";
            Debug.WriteLine("-----Create ms.ImNew-----");
            var result = y.CreateTag(content, tag, newVal);
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
            //    Assert.AreEqual(result.Length, 207);
        }


        [TestMethod]
        public void TestGetYmlBlock()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string yml = y.GetYmlBlock(content);
            Debug.WriteLine("-----Get YML Block-----");
            int i = yml.Length;
            Debug.Write(yml);
            Debug.WriteLine("<---output ends here");
            //    Assert.AreEqual(result.Length, 207);
        }

        [TestMethod]
        public void TestGetAllTags()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string yml = y.GetYmlBlock(content);
            Debug.WriteLine("-----Get All tags-----");
            var tags = y.GetAllTags(yml);
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
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string yml = y.GetYmlBlock(content);
            Debug.WriteLine("-----Get All tags-----");
            var matches = y.GetAllTags(yml);
            Debug.WriteLine("<---output ends here");
            var tags = new List<Tag>();
            foreach (string m in matches)
            {
                var t = new Tag(m);
                tags.Add(t);
                Debug.WriteLine("tag name is: " + t.Name);
                foreach (var i in t.Values)
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
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            var content = File.ReadAllText(f);
            var result = y.ParseYML2(content);
            foreach (var v in result)
            {
                Debug.WriteLine(v.Name);
                foreach (var v2 in v.Values)
                {
                    Debug.WriteLine("   " + v2);
                }
            }
        }
    }
}

;

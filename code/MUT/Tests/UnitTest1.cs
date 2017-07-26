using System;
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
            int i = y.GetTagStartPos(ref content, ref tag);
            Assert.AreEqual(i, 5);
        }

        [TestMethod]
        public void TestMultilineValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "dev_langs";
            Assert.IsTrue(y.IsMultilineValue(ref content, ref tag));
        }

        [TestMethod]
        public void TestSinglelineValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.assetid";
            Assert.IsFalse(y.IsMultilineValue(ref content, ref tag));
        }

        [TestMethod]
        public void TestPrefix()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes-cpp.md";
            string content = File.ReadAllText(f);
            string tag = "ms.custom";
            string expected = "---\r\ntitle: \"Abstract Classes (C++) | Microsoft Docs\"\r\n";

            string result = y.GetPrefix(ref content, ref tag);

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
            int startPos = y.GetTagStartPos(ref content, ref tag);
            int endPos = y.GetTagValueEndPos(ref content, startPos);
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
            string val = y.GetTagAndValue(ref content, ref tag);
            Debug.Write(val);

            tag = "ms.author";
            val = y.GetTagAndValue(ref content, ref tag);
            Debug.Write(val);
        }


        [TestMethod]
        public void TestGetValue()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes2.md";
            string content = File.ReadAllText(f);
            string tag = "translation.priority.ht";
            string temp = y.GetValue(ref content, ref tag);
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
            int startPos = y.GetTagStartPos(ref content, ref tag);
            int endPos = y.GetTagValueEndPos(ref content, startPos);
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
            string result = y.GetSuffix(ref content, ref tag);
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
            string prefix = y.GetPrefix(ref content, ref tag);
            string tagAndval = y.GetTagAndValue(ref content, ref tag);
            string suffix = y.GetSuffix(ref content, ref tag);
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
            var result = y.DeleteTagAndValue(ref content, ref tag);
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
            Assert.AreEqual(result.Length, 207);
        }
        public void TestReplaceSingleLine()
        {
            YMLMeister y = new YMLMeister();
            string f = @"../../abstract-classes3.md";
            string content = File.ReadAllText(f);
            string tag = "ms.author";
            Debug.WriteLine("-----Delete ms.author-----");
            var result = y.ReplaceSingleValue(ref content, ref tag, "Donald Jr");
            int i = result.Length;
            Debug.Write(result);
            Debug.WriteLine("<---output ends here");
        //    Assert.AreEqual(result.Length, 207);
        }

    }
}


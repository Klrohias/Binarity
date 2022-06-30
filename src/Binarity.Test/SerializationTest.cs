using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Binarity;
using Binarity.Atributes;


namespace Binarity.Test
{
    [TestClass]
    public class SerializationTest
    {
        public class MyClassB
        {
            [BinarityField("testA")] public string Name { get; set; }
            [BinarityField("testB")] public string Info { get; set; }
            [BinarityField("testC")] public int Num { get; set; }
            [BinarityField("fileA")] public Stream fileStream { get; set; }
            [BinarityField("arrayA")]public List<MyClassC> listOfC { get; set; }
            [BinarityField("arrayB")] public MyClassC[] arrayOfC { get; set; }
        }
        public class MyClassC
        {
            [BinarityField("text")] public string Text { get; set; }
        }
        public class MyClassA
        {
            [BinarityField("testA")] public MyClassB field1;
        }

        [TestMethod]
        public void SerializeObject()
        {
            var objectMyClassA = new MyClassA();
            var file = File.OpenRead("C:/Windows/notepad.exe");
            objectMyClassA.field1
                = new MyClassB
                {
                    Name = "hello, world",
                    Info = "The quick brown fox jumps over the lazy dog",
                    Num = 1145141,
                    fileStream = file
                };
            var result = BinaritySerialization.Serialize(objectMyClassA);
            file.Close();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeserializeObject()
        {
            byte[] result;
            {
                var objectMyClassA = new MyClassA();
                var file = File.OpenRead("C:/Windows/notepad.exe");
                objectMyClassA.field1
                    = new MyClassB
                    {
                        Name = "hello, world",
                        Info = "The quick brown fox jumps over the lazy dog",
                        Num = 1145141,
                        fileStream = file,
                        listOfC = new List<MyClassC>()
                        {
                            new MyClassC {Text = "test1"},
                            new MyClassC {Text = "test2"},
                        },
                        arrayOfC = new MyClassC[]
                        {
                            new MyClassC {Text = "test3"},
                            new MyClassC {Text = "test4"},
                        }
                    };
                result = BinaritySerialization.Serialize(objectMyClassA);
                file.Close();
                Assert.IsNotNull(result);
            }

            {
                var objectMyClassA = BinaritySerialization.Deserialize<MyClassA>(result);
                Assert.IsNotNull(objectMyClassA);
                Assert.IsNotNull(objectMyClassA.field1);
                Assert.AreEqual(objectMyClassA.field1.Name, "hello, world");
                var file = File.OpenWrite("D:/notepad.exe");
                objectMyClassA.field1.fileStream.CopyTo(file);
                file.Close();
            }
        }

        [TestMethod]
        public void TestInteger()
        {
            {
                var number = 114514;
                var byteResult = BinaritySerialization.Serialize(number);
                var numberResult = BinaritySerialization.Deserialize<int>(byteResult);
                Assert.AreEqual(numberResult, number);
            }
            {
                var number = ulong.MaxValue;
                var byteResult = BinaritySerialization.Serialize(number);
                var numberResult = BinaritySerialization.Deserialize<ulong>(byteResult);
                Assert.AreEqual(numberResult, number);
            }
        }

        [TestMethod]
        public void TestChinese()
        {
            // ... Because I had learned Chinese, so I will use Chinese to do this test.
            // If this test is successful, it means that this library support other languages...

            {
                var testString = "Hello, World! 你好，世界。\n我希望COVID-19疫情能尽快结束，谢谢 :)";
                var chineseResult = BinaritySerialization.Serialize(testString);
                var deserializeResult = BinaritySerialization.Deserialize<string>(chineseResult);

                Assert.AreEqual(testString, deserializeResult);
            }
        }
    }
}
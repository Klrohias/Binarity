using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Binarity;
using Binarity.Atributes;


namespace Binarity.Test
{
    [TestClass]
    public class SerializerTest
    {
        public class MyClassB
        {
            [BinartiryField("testA")] public string Name { get; set; }
            [BinartiryField("testB")] public string Info { get; set; }
            [BinartiryField("testC")] public int Num { get; set; }
            [BinartiryField("fileA")] public Stream fileStream { get; set; }


        }
        public class MyClassA
        {
            [BinartiryField("testA")] public MyClassB field1;
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
    }
}
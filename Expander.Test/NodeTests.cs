using System;
using NUnit.Framework;
using Expander;
using System.Collections.Generic;


namespace Expander.Test
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void ShouldParseSimpleTerm()
        {
            int pointer = 0;
            Node n = Node.ParseExpression("LEAF123", ref pointer, 1);
            Assert.AreEqual(6, pointer);
            Assert.AreEqual(0, n.children.Count);
            Assert.AreEqual("LEAF123", n.name);
        }

        // TODO: decide if this actually matters or not. Would have to maintain a string denoting the path and then
        // check it against each new node to see if there was an expandable node matching up the tree.
        //[Test]
        //public void ShouldRejectInfiniteRecursion()
        //{
        //    int pointer = 0;
        //    Node.lookup["CodeA"] = @"Func1(CodeA)";
        //    Node n = Node.ParseExpression("Func1(CodeA)", ref pointer, 1);
        //    Assert.AreEqual(0, n.level);
        //}

        [Test]
        public void OriginalTestExpression()
        {
            Node.lookup["Code1"] = @"GetDataY('HV1',1,'gubbins')";
            Node.lookup["Code2"] = @"Func4(Code3,1)";
            Node.lookup["Code3"] = @"GetDataX('aric','DAILY',BaseFunc())";

            int pointer = 0;
            Node n = Node.ParseExpression(
                @"Func0(Func1(Func2(Func3(Code1,Code2),1)),Func5(Code2,GetDataX('thing')))", 
                ref pointer, 1);

            Assert.AreEqual(9, Node.stats[Node.MAX_DEPTH]);
            Assert.AreEqual(29, Node.stats[Node.NODE_COUNT]);
        }
    }
}
